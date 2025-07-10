using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ChargeCollection))]
	public class ChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestChargeCollectionErrorReportedWhenDeletedChargeRemainsInBusinessObjectCollection()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ChargeCollectionRemoveMethodInfo);

			var charge = Factory.New<Charge>();

			var collection1 = new MyDummyBizObjCollectionWithInternalOverrides(Factory);
			collection1.Add(charge);
			AssertEquals(1, collection1.Count);

			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.CreateShipment("S0001"), false);
			var collection2 = new ChargeCollection(job);
			collection2.Add(charge);
			AssertEquals(1, collection2.Count);

			AssertEquals(2, ((IBusinessObjectInternals)charge).ParentCollections.Length);

			var expectedKey = "Business_Object_Collections_With_Deleted_Charge_3";
			var expectedMessage = string.Format(
@"Deleted Charge:
	PK = {0}
	Type = Charge
	Types around row = Charge
	Factory Instance = {1}
	IsDeleted = True
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = Factory Level : (DeletingJobCharge)

Original property values for deleted bizo are not accessible

ChargeCollectionRemoveMethodInfo:
Hash Code = {3}, Contains?NotInCollection
Before Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.Testing.ChargeCollectionTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject
	Factory Instance = {1}
	Hash Code = {2}
	Contains bizo = True
	Has Changes = True
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.ChargeCollection
	Element Type = Enterprise.Accounting.Business.JobInvoicing.Charge
	Factory Instance = {1}
	Hash Code = {3}
	Contains bizo = False
	Has Changes = False
	Number of elements = 0
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True
After Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.Testing.ChargeCollectionTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject
	Factory Instance = {1}
	Hash Code = {2}
	Contains bizo = True
	Has Changes = True
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True",
				charge.PK, Factory._Instance, collection1.GetHashCode(), collection2.GetHashCode());

			ErrorReporter.Clear();
			charge.Delete();

			Assert("Collection1 contains deleted charge", collection1.Contains(charge));
			Assert("Collection2 does not contain deleted charge", !collection2.Contains(charge));

			AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("Error must be reported", expectedKey, ErrorReporter.LastKeyReported);
			AssertMultilineASCIIEquals(expectedMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		internal class MyDummyBizObjCollectionWithInternalOverrides : DummyBusinessObjectCollection, IBusinessObjectCollectionInternals
		{
			public MyDummyBizObjCollectionWithInternalOverrides(BusinessObjectFactory factory)
				: base(factory) { }

			public override void Remove(BusinessObject elementToRemove)
			{
			}
		}

		public void TestReloadChargesRemovesDeletedOnes()
		{
			var factory1 = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(factory1);
			var chargeCode = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);

			Job job = factory1.NewJobWithValidTestDataForTesting<Job>();
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCode.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var jobFromF2 = factory2.Load<Job>(job.PK);
			jobFromF2.Charges[0].Delete();
			factory2.Save();

			job.Charges.Reload();

			AssertEquals("Reload has picked up that there is deleted charge", 1, job.Charges.Count);
		}

		[ExpectNoExceptions]
		public void TestReloadChargesWhenThereAreNoCharges()
		{
			Factory.NewJobForTesting<Job>().Charges.Reload();
		}

		public void TestReloadChargesWhenThereAreManyCharges()
		{
			// warmup for DB hits
			new DynamicBusinessObjectCollection(new BusinessObjectFactory())
				.Load(
					$"SELECT NULL FROM {JobChargeSchema.Constants.SqlSchemaName}.{JobChargeSchema.Constants.TableName} WHERE {JobChargeSchema.PK.Name} = @PK"
					, new ZSqlParameter[]
					{
							ZSqlParameter.New("@PK", ZGuid.NewZGuid(), JobChargeSchema.PK),
					});

			var factory1 = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(factory1);
			var chargeCode = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);
			Job job = factory1.NewJobWithValidTestDataForTesting<Job>();

			using (job.Charges.GetCheckDuplicateDisplaySequenceSuspender())
			{
				for (int count = 0; count < 10; count++)
				{
					var charge = job.Charges.AddNew();
					charge.JR_AC = chargeCode.PK;
					charge.JR_GB = GlbBranch.CurrentBranch.PK;
				}
			}

			factory1.Save();

			using (job.Charges.GetCheckDuplicateDisplaySequenceSuspender())
			{
				for (int count = 0; count < 8; count++)
				{
					var charge = job.Charges.AddNew();
					charge.JR_AC = chargeCode.PK;
					charge.JR_GB = GlbBranch.CurrentBranch.PK;
				}
			}

			int hitCountBefore = CargoWise.Data.Db.Connection.ExecutedCommandCountForAllConnections;
			job.Charges.Reload();
			int hitCountAfter = CargoWise.Data.Db.Connection.ExecutedCommandCountForAllConnections;

			int dbHits = hitCountAfter - hitCountBefore - 10;

			AssertEquals("Should have used 4 db hits to reload in batches (14 in total, less 10 for reloading charges that were originally in the database)", 4, dbHits);
		}

		public void TestHasUnpostedARWhenAPPosted()
		{
			ZGuid chargeCodePK1 = ZGuid.NewZGuid();
			ZGuid chargeCodePK2 = ZGuid.NewZGuid();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = chargeCodePK1;
			charge1.JR_E6 = ZGuid.NewZGuid();
			charge1.JR_APInvoiceNum = "1";

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK1;
			charge2.JR_APInvoiceNum = "1";

			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = chargeCodePK2;
			charge3.JR_APInvoiceNum = "2";

			APInvoiceLine apLine = Factory.NewWithValidTestData<APInvoiceLine>();
			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			charge1.JR_AL_APLine = apLine.PK;
			AssertEquals("PreCondition", true, charge1.IsCostPosted);

			Assert("Cost posted for apportioned charges", !job.Charges.HasUnpostedARWhenAPPosted(new ZGuid[] { chargeCodePK1 }));

			charge2.JR_AL_APLine = apLine.PK;
			Assert(job.Charges.HasUnpostedARWhenAPPosted(new ZGuid[] { chargeCodePK1 }));
			Assert(!job.Charges.HasUnpostedARWhenAPPosted(new ZGuid[] { chargeCodePK2 }));
		}

		public void TestHasPostedAROrAP()
		{
			ZGuid chargeCodePK1 = ZGuid.NewZGuid();
			ZGuid chargeCodePK2 = ZGuid.NewZGuid();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = chargeCodePK1;
			charge1.JR_E6 = ZGuid.NewZGuid();
			charge1.JR_APInvoiceNum = "1";

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK1;
			charge2.JR_APInvoiceNum = "1";

			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = chargeCodePK2;
			charge3.JR_APInvoiceNum = "2";

			APInvoiceLine apLine = Factory.NewWithValidTestData<APInvoiceLine>();
			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			charge1.JR_AL_APLine = apLine.PK;
			AssertEquals("PreCondition", true, charge1.IsCostPosted);
			Assert("Cost posted for apportioned charges", !job.Charges.HasPostedAROrAP(new ZGuid[] { chargeCodePK1 }));

			ARInvoiceLine arLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			arLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge3.JR_AL_ARLine = arLine.PK;

			Assert(!job.Charges.HasPostedAROrAP(new ZGuid[] { chargeCodePK1 }));
			Assert(job.Charges.HasPostedAROrAP(new ZGuid[] { chargeCodePK2 }));
		}

		public void TestGetMatchingUnapportionedChargesFromThisJobOnly()
		{
			ZGuid chargeCodePK1 = ZGuid.NewZGuid();
			ZGuid chargeCodePK2 = ZGuid.NewZGuid();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = chargeCodePK1;
			charge1.JR_E6 = ZGuid.NewZGuid();
			charge1.JR_APInvoiceNum = "1";

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK1;
			charge2.JR_APInvoiceNum = "1";

			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = chargeCodePK2;
			charge3.JR_APInvoiceNum = "2";

			Charge charge4 = job.Charges.AddNew();
			charge4.JR_AC = chargeCodePK1;
			charge4.JR_E6 = ZGuid.NewZGuid();
			charge4.JR_APInvoiceNum = "1";
			charge4.JR_JH = ZGuid.NewZGuid(); // Simulates a 'blue' charge loaded from a child job and included in this job's Charge collection.

			Charge charge5 = job.Charges.AddNew();
			charge5.JR_AC = chargeCodePK1;
			charge5.JR_APInvoiceNum = "1";
			charge5.JR_JH = ZGuid.NewZGuid(); // Simulates a 'blue' charge loaded from a child job and included in this job's Charge collection.

			Charge charge6 = job.Charges.AddNew();
			charge6.JR_AC = chargeCodePK2;
			charge6.JR_APInvoiceNum = "2";
			charge6.JR_JH = ZGuid.NewZGuid(); // Simulates a 'blue' charge loaded from a child job and included in this job's Charge collection.

			Charge charge7 = job.Charges.AddNew();
			charge7.JR_AC = chargeCodePK2;
			charge7.JR_APInvoiceNum = "ab";

			Charge charge8 = job.Charges.AddNew();
			charge8.JR_AC = chargeCodePK2;
			charge8.JR_APInvoiceNum = "cd";

			List<Charge> charges = job.Charges.GetMatchingUnapportionedChargesFromThisJobOnly(chargeCodePK1, "1", "");
			AssertEquals("GetMatchingCharges", 1, charges.Count);
			AssertEquals("GetMatchingCharges", true, charges.Contains(charge2));

			charges = job.Charges.GetMatchingUnapportionedChargesFromThisJobOnly(chargeCodePK2, "1", "");
			AssertEquals("GetMatchingCharges", 0, charges.Count);

			charges = job.Charges.GetMatchingUnapportionedChargesFromThisJobOnly(chargeCodePK2, "2", "");
			AssertEquals("GetMatchingCharges", 1, charges.Count);
			AssertEquals("GetMatchingCharges", true, charges.Contains(charge3));

			charges = job.Charges.GetMatchingUnapportionedChargesFromThisJobOnly(chargeCodePK2, "2", "A");
			AssertEquals("GetMatchingCharges", 2, charges.Count);
			AssertEquals("GetMatchingCharges", true, charges.Contains(charge3));
			AssertEquals("GetMatchingCharges", true, charges.Contains(charge7));

			charges = job.Charges.GetMatchingUnapportionedChargesFromThisJobOnly(chargeCodePK2, "c", "AC");
			AssertEquals("GetMatchingCharges", 1, charges.Count);
			AssertEquals("GetMatchingCharges", true, charges.Contains(charge8));
		}

		public void TestOnAddHooksUpDisplayInfoChanged()
		{
			Job savedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge savedCharge = savedJob.Charges.AddNew();
			bool eventFired = false;
			savedJob.Charges.DisplaySequenceChanged += (s, e) => { eventFired = true; };
			savedCharge.JR_DisplaySequence = 2;
			AssertEquals(true, eventFired);
		}

		public void TestFetchFromLocalCacheIfNoAdditionalCharges()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Job savedJob = newFactory.NewJobWithValidTestDataForTesting<Job>();
			Charge savedCharge = savedJob.Charges.AddNew();
			savedCharge.FillWithValidTestData();
			newFactory.Save();

			Job newJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			AssertEquals(true, newJob.Charges.FetchOnlyFromLocalCache_ForTestOnly);
			newJob.Charges.IncludeChargesFromJobs(savedJob.PK);
			newJob.Charges.Load();
			AssertEquals(false, newJob.Charges.FetchOnlyFromLocalCache_ForTestOnly);
			AssertEquals("1 charge has come through - even though this job is not yet saved", 1, newJob.Charges.Count);
		}

		public void TestAllowSortIsTrue()
		{
			ChargeCollection coll = new ChargeCollection(Factory.NewJobForTesting<Job>());
			AssertEquals(true, coll.AllowSort_ForTestOnly);
		}

		public void TestIsManagedForDataRefresh()
		{
			ChargeCollection coll = new ChargeCollection(Factory.NewJobForTesting<Job>());
			AssertEquals(true, coll.IsManagedForDataRefresh);
		}

		public void TestAllowNew()
		{
			ChargeCollection collection = new ChargeCollection(Factory.NewJobForTesting<Job>());
			collection.LoadedFromAutopopulate = true;
			AssertEquals(false, collection.AllowNew);
		}

		/*Please create JobHeader via switching user context*/
		[MasterFiles.Business.Testing.SuspendToTestReportJobIsChangedByDifferentCompany]
		public void TestIncludeChargesFromJobs()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			GlbCompany otherCompany = newFactory.NewWithValidTestData<GlbCompany>();
			ForwardingShipment ship = newFactory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment ship2 = newFactory.NewWithValidTestData<ForwardingShipment>();

			newFactory.Save();

			var objectCreator = new TestObjectCreator(newFactory);
			var chargeCodeInCurrentCompany = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);
			var chargeCodeInOtherCompany = objectCreator.CreateChargeCode("CC1", "CC1 Other Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, otherCompany);

			Job job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "463426";
			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			charge1.JR_LocalSellAmt = 200m;
			Charge charge2 = job1.Charges.AddNew();
			charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			charge2.JR_LocalSellAmt = 200m;

			Job job2 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = ship.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job2.JH_JobNum = "5522566";
			Charge job2Charge0 = job2.Charges.AddNew();
			Charge job2Charge1 = job2.Charges.AddNew();
			Charge job2Charge2 = job2.Charges.AddNew();
			job2Charge0.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge0.JR_LocalSellAmt = 200m;
			job2Charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge1.JR_LocalSellAmt = 200m;
			job2Charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge2.JR_LocalSellAmt = 200m;

			Job job3 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = ship2.PK;
			job3.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job3.JH_JobNum = "919385";
			Charge charge3 = job3.Charges.AddNew();
			charge3.JR_AC = chargeCodeInCurrentCompany.PK;
			charge3.JR_LocalSellAmt = 200m;

			Job jobInOtherCompany = newFactory.NewJobWithValidTestDataForTesting<Job>();
			jobInOtherCompany.JH_ParentID = ship2.PK;
			jobInOtherCompany.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobInOtherCompany.JH_JobNum = "919385A";
			jobInOtherCompany.JH_GC = otherCompany.PK;
			Charge chargeInOtherCompany = jobInOtherCompany.Charges.AddNew();
			chargeInOtherCompany.JR_AC = chargeCodeInOtherCompany.PK;
			chargeInOtherCompany.JR_LocalSellAmt = 200m;

			newFactory.Save();

			ChargeCollection charges = new ChargeCollection(job1);
			charges.Load();
			AssertEquals(2, charges.Count);

			Assert(charges.IncludeChargesFromJobs(job2.PK));
			Assert("Shouldn't add the same jobs twice", !charges.IncludeChargesFromJobs(job2.PK));
			charges.Load();
			AssertEquals(5, charges.Count);

			Assert(charges.IncludeChargesFromJobs(job2.PK, job3.PK));
			Assert("Shouldn't add the same jobs twice", !charges.IncludeChargesFromJobs(job2.PK, job3.PK));
			charges.Load();
			AssertEquals(6, charges.Count);

			// Posted this Job
			AccTransactionHeader header = newFactory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_JH = job1.PK;

			AccTransactionLines line = newFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			// AccTransactionLines is not valid with empty AL_LineType.
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_AG = objectCreator.GLHeader1.PK;
			job2Charge1.ReverseWIP(ZDateTime.Now);
			job2Charge1.JR_AL_ARLine = line.PK;

			// Posted to it's own job
			AccTransactionHeader otherHeader = newFactory.NewWithValidTestData<AccTransactionHeader>();
			otherHeader.AH_JH = job2.PK;

			AccTransactionLines line2 = newFactory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_AH = otherHeader.PK;
			// AccTransactionLines is not valid with empty AL_LineType.
			line2.AL_LineType = TransactionLineTypes.WIP;
			line2.AL_AG = objectCreator.GLHeader1.PK;
			job2Charge2.ReverseWIP(ZDateTime.Now);
			job2Charge2.JR_AL_ARLine = line2.PK;

			newFactory.Save();

			Assert("Shouldn't add the same jobs twice", !charges.IncludeChargesFromJobs(job2.PK, job3.PK));
			charges.Load();
			AssertEquals(6, charges.Count);
			AssertCollectionContains(job2Charge1, charges);
			AssertCollectionContains(job2Charge2, charges);
		}

		/*Please create JobHeader via switching user context*/
		[MasterFiles.Business.Testing.SuspendToTestReportJobIsChangedByDifferentCompany]
		public void TestSequenceSettingsForIncludeChargesFromJobs()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			GlbCompany otherCompany = newFactory.NewWithValidTestData<GlbCompany>();
			ForwardingShipment ship = newFactory.NewWithValidTestData<ForwardingShipment>();
			ship.JS_UniqueConsignRef = "S00001042";
			ForwardingShipment ship2 = newFactory.NewWithValidTestData<ForwardingShipment>();
			ship2.JS_UniqueConsignRef = "S00002981";

			newFactory.Save();

			var objectCreator = new TestObjectCreator(newFactory);
			var chargeCodeInCurrentCompany = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);
			var chargeCodeInOtherCompany = objectCreator.CreateChargeCode("CC1", "CC1 Other Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, otherCompany);

			Job job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "S00001011";
			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			charge1.JR_LocalSellAmt = 200m;
			Charge charge2 = job1.Charges.AddNew();
			charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			charge2.JR_LocalSellAmt = 200m;

			Job job2 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = ship.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job2.JH_JobNum = "S00001042";
			Charge job2Charge0 = job2.Charges.AddNew();
			Charge job2Charge1 = job2.Charges.AddNew();
			Charge job2Charge2 = job2.Charges.AddNew();
			job2Charge0.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge0.JR_LocalSellAmt = 200m;
			job2Charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge1.JR_LocalSellAmt = 200m;
			job2Charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge2.JR_LocalSellAmt = 200m;

			Job job3 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = ship2.PK;
			job3.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job3.JH_JobNum = "S00002981";
			Charge charge3 = job3.Charges.AddNew();
			charge3.JR_AC = chargeCodeInCurrentCompany.PK;
			charge3.JR_LocalSellAmt = 200m;

			Job jobInOtherCompany = newFactory.NewJobWithValidTestDataForTesting<Job>();
			jobInOtherCompany.JH_ParentID = ship2.PK;
			jobInOtherCompany.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobInOtherCompany.JH_JobNum = "919385A";
			jobInOtherCompany.JH_GC = otherCompany.PK;
			Charge chargeInOtherCompany = jobInOtherCompany.Charges.AddNew();
			chargeInOtherCompany.JR_AC = chargeCodeInOtherCompany.PK;
			chargeInOtherCompany.JR_LocalSellAmt = 200m;

			newFactory.Save();

			ChargeCollection charges = new ChargeCollection(job1);
			charges.Load();
			AssertEquals(2, charges.Count);

			charges.IncludeChargesFromJobs(job2.PK);
			charges.Load();
			Assert(!charges.HasChanges);
			AssertEquals(5, charges.Count);

			charges.IncludeChargesFromJobs(job2.PK, job3.PK);
			charges.Load();
			Assert(!charges.HasChanges);
			AssertEquals(6, charges.Count);

			AssertEquals(charges.GetBiggestSequenceNumber_ForTestOnly(), (ZShort)6); // Charges should be resequenced when charges from other jobs are included

			charge3.ReverseWIP(ZDateTime.Now);
			charge3.JR_AL_ARLine = newFactory.NewWithValidTestData<ARInvoice>().Lines.AddNew().PK;
			charge3.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue; // Now Posted
			charge3.ARLine.AL_GB = GlbBranch.CurrentBranch.PK;
			charge3.ARLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			charge3.ARLine.AL_OSAmount = charge3.ARLine.AL_LineAmount = charge3.JR_OSSellAmt;
			charge3.ARLine.AL_AG = objectCreator.GLHeader1.PK;

			newFactory.Save();

			charges.IncludeChargesFromJobs(job2.PK, job3.PK);
			AssertEquals((ZShort)1, charge1.JR_DisplaySequence);
			AssertEquals((ZShort)2, charge2.JR_DisplaySequence);
			AssertEquals((ZShort)3, job2Charge0.JR_DisplaySequence);
			AssertEquals((ZShort)4, job2Charge1.JR_DisplaySequence);
			AssertEquals((ZShort)5, job2Charge2.JR_DisplaySequence);
			AssertEquals((ZShort)6, charge3.JR_DisplaySequence);
		}

		public void TestSequenceSettingsForIncludeChargesFromJobs_CaseWhenSequenceNumberNearShortMaxValue()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(newFactory);

			Job job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "S00001011";
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_LocalSellAmt = 200m;
			Charge job1Charge2 = job1.Charges.AddNew();
			job1Charge2.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge2.JR_LocalSellAmt = 200m;
			job1Charge2.JR_AL_ARLine = newFactory.NewWithValidTestData<ARInvoice>().Lines.AddNew().PK;
			job1Charge2.ARLine.AL_AG = objectCreator.GLHeader1.PK;
			job1Charge2.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			job1Charge2.ARLine.AL_GB = GlbBranch.CurrentBranch.PK;
			job1Charge2.ARLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			job1Charge2.ARLine.AL_OSAmount = job1Charge2.ARLine.AL_LineAmount = job1Charge2.JR_OSSellAmt;

			Job job2 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_JobNum = "S00001042";
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_LocalSellAmt = 200m;
			job2Charge1.JR_AL_ARLine = newFactory.NewWithValidTestData<ARInvoice>().Lines.AddNew().PK;
			job2Charge1.ARLine.AL_AG = objectCreator.GLHeader1.PK;
			job2Charge1.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			job2Charge1.ARLine.AL_GB = GlbBranch.CurrentBranch.PK;
			job2Charge1.ARLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			job2Charge1.ARLine.AL_OSAmount = job2Charge1.ARLine.AL_LineAmount = job2Charge1.JR_OSSellAmt;
			Charge job2Charge2 = job2.Charges.AddNew();
			job2Charge2.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge2.JR_LocalSellAmt = 200m;
			Charge job2Charge3 = job2.Charges.AddNew();
			job2Charge3.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge3.JR_LocalSellAmt = 200m;
			Charge job2Charge4 = job2.Charges.AddNew();
			job2Charge4.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge4.JR_LocalSellAmt = 200m;
			job2Charge4.JR_AL_ARLine = newFactory.NewWithValidTestData<ARInvoice>().Lines.AddNew().PK;
			job2Charge4.ARLine.AL_AG = objectCreator.GLHeader1.PK;
			job2Charge4.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			job2Charge4.ARLine.AL_GB = GlbBranch.CurrentBranch.PK;
			job2Charge4.ARLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			job2Charge4.ARLine.AL_OSAmount = job2Charge4.ARLine.AL_LineAmount = job2Charge4.JR_OSSellAmt;

			Job job3 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_JobNum = "S00002981";
			Charge job3Charge1 = job3.Charges.AddNew();
			job3Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job3Charge1.JR_LocalSellAmt = 200m;
			job3Charge1.JR_AL_ARLine = newFactory.NewWithValidTestData<ARInvoice>().Lines.AddNew().PK;
			job3Charge1.ARLine.AL_AG = objectCreator.GLHeader1.PK;
			job3Charge1.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			job3Charge1.ARLine.AL_GB = GlbBranch.CurrentBranch.PK;
			job3Charge1.ARLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			job3Charge1.ARLine.AL_OSAmount = job3Charge1.ARLine.AL_LineAmount = job3Charge1.JR_OSSellAmt;
			job3Charge1.JR_DisplaySequence = short.MaxValue - 2;
			Charge job3Charge2 = job3.Charges.AddNew();
			job3Charge2.JR_AC = Env.Registry.FreightChargeCode;
			job3Charge2.JR_LocalSellAmt = 200m;
			Charge job3Charge3 = job3.Charges.AddNew();
			job3Charge3.JR_AC = Env.Registry.FreightChargeCode;
			job3Charge3.JR_LocalSellAmt = 200m;

			newFactory.Save();

			Assert("PostCondition: Job1Charge2 Posted", job1Charge2.IsRevenuePosted);
			Assert("PostCondition: Job2Charge1 Posted", job2Charge1.IsRevenuePosted);
			Assert("PostCondition: Job2Charge4 Posted", job2Charge4.IsRevenuePosted);
			Assert("PostCondition: Job3Charge1 Posted", job3Charge1.IsRevenuePosted);
			AssertEquals("PostCondition: Job1Charge2 has Sequence Number 2", (ZShort)2, job1Charge2.JR_DisplaySequence);
			AssertEquals("PostCondition: Job2Charge1 has Sequence Number 1", (ZShort)1, job2Charge1.JR_DisplaySequence);
			AssertEquals("PostCondition: Job2Charge4 has Sequence Number 4", (ZShort)4, job2Charge4.JR_DisplaySequence);
			AssertEquals("PostCondition: Job3Charge1 has Sequence Number 32765", (ZShort)32765, job3Charge1.JR_DisplaySequence);

			ChargeCollection charges = new ChargeCollection(job1);
			charges.IncludeChargesFromJobs(job2.PK, job3.PK);
			charges.Load();
			AssertEquals("Sequence number should be next after posted maximum", (ZShort)32766, job1Charge1.JR_DisplaySequence);
			AssertEquals("Sequence number must be unchanged as Revenue Posted", (ZShort)2, job1Charge2.JR_DisplaySequence);
			AssertEquals("Sequence number must be unchanged as Revenue Posted", (ZShort)1, job2Charge1.JR_DisplaySequence);
			AssertEquals("Sequence number should be next after last set", (ZShort)32767, job2Charge2.JR_DisplaySequence);
			AssertEquals("Sequence number should be starting from 1", (ZShort)1, job2Charge3.JR_DisplaySequence);
			AssertEquals("Sequence number must be unchanged as Revenue Posted", (ZShort)4, job2Charge4.JR_DisplaySequence);
			AssertEquals("Sequence number must be unchanged as Revenue Posted", (ZShort)32765, job3Charge1.JR_DisplaySequence);
			AssertEquals("Sequence number should be next after last set", (ZShort)2, job3Charge2.JR_DisplaySequence);
			AssertEquals("Sequence number should be next after last set", (ZShort)3, job3Charge3.JR_DisplaySequence);
		}

		/*Please create JobHeader via switching user context*/
		[MasterFiles.Business.Testing.SuspendToTestReportJobIsChangedByDifferentCompany]
		public void TestIncludeChargesFromParentJobs()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			GlbCompany otherCompany = newFactory.NewWithValidTestData<GlbCompany>();
			ForwardingShipment ship = newFactory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment ship2 = newFactory.NewWithValidTestData<ForwardingShipment>();

			newFactory.Save();

			var objectCreator = new TestObjectCreator(newFactory);
			var chargeCodeInCurrentCompany = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);
			var chargeCodeInOtherCompany = objectCreator.CreateChargeCode("CC1", "CC1 Other Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, otherCompany);

			Job job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "463426";
			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			charge1.JR_LocalSellAmt = 200m;
			Charge charge2 = job1.Charges.AddNew();
			charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			charge2.JR_LocalSellAmt = 200m;

			Job job2 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = ship.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job2.JH_JobNum = "5522566";
			Charge job2Charge0 = job2.Charges.AddNew();
			Charge job2Charge1 = job2.Charges.AddNew();
			Charge job2Charge2 = job2.Charges.AddNew();
			job2Charge0.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge0.JR_LocalSellAmt = 200m;
			job2Charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge1.JR_LocalSellAmt = 200m;
			job2Charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge2.JR_LocalSellAmt = 200m;

			Job job3 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = ship2.PK;
			job3.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job3.JH_JobNum = "919385";
			Charge charge3 = job3.Charges.AddNew();
			charge3.JR_AC = chargeCodeInCurrentCompany.PK;
			charge3.JR_LocalSellAmt = 200m;

			Job jobInOtherCompany = newFactory.NewJobWithValidTestDataForTesting<Job>();
			jobInOtherCompany.JH_ParentID = ship2.PK;
			jobInOtherCompany.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobInOtherCompany.JH_JobNum = "919385A";
			jobInOtherCompany.JH_GC = otherCompany.PK;
			Charge chargeInOtherCompany = jobInOtherCompany.Charges.AddNew();
			chargeInOtherCompany.JR_AC = chargeCodeInOtherCompany.PK;
			chargeInOtherCompany.JR_LocalSellAmt = 200m;

			newFactory.Save();

			ChargeCollection charges = new ChargeCollection(job1);
			charges.Load();
			AssertEquals("No Parent Job, No Additional Jobs", 2, charges.Count);

			job2.JH_JH_ParentJob = job1.PK;
			charges.Load();
			AssertEquals("With Parent Job, No Additional Jobs", 5, charges.Count);

			charges.IncludeChargesFromJobs(job3.PK);
			charges.Load();
			AssertEquals("With Parent Job and Additional Jobs", 6, charges.Count);

			// Posted this Job
			AccTransactionHeader header = newFactory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_JH = job1.PK;

			AccTransactionLines line = newFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			// AccTransactionLines is not valid with empty AL_LineType.
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_AG = objectCreator.GLHeader1.PK;
			job2Charge1.ReverseWIP(ZDateTime.Now);
			job2Charge1.JR_AL_ARLine = line.PK;

			// Posted to it's own job
			AccTransactionHeader otherHeader = newFactory.NewWithValidTestData<AccTransactionHeader>();
			otherHeader.AH_JH = job2.PK;

			AccTransactionLines line2 = newFactory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_AH = otherHeader.PK;
			// AccTransactionLines is not valid with empty AL_LineType.
			line2.AL_LineType = TransactionLineTypes.WIP;
			line2.AL_AG = objectCreator.GLHeader1.PK;
			job2Charge2.ReverseWIP(ZDateTime.Now);
			job2Charge2.JR_AL_ARLine = line2.PK;

			newFactory.Save();

			charges.Load();
			AssertEquals(6, charges.Count);
			AssertCollectionContains(job2Charge1, charges);
			AssertCollectionContains(job2Charge2, charges);
		}

		public void TestSequenceSort()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var shipment = testObjectCreator.CreateShipment("100");
			var job = testObjectCreator.CreateJob(shipment);

			var collection = job.Charges;
			var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "IsInDatabaseCharge", testObjectCreator.AUD, 100m, testObjectCreator.ABIGAS, testObjectCreator.AUD, 100m, testObjectCreator.AALSHI);
			var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "IsInDatabaseCharge2", testObjectCreator.AUD, 100m, testObjectCreator.ABIGAS, testObjectCreator.AUD, 100m, testObjectCreator.AALSHI);

			factory.Save();

			var charge3 = testObjectCreator.CreateCharge(job, testObjectCreator.CC3, "IsNoTInDatabaseCharge", testObjectCreator.AUD, 100m, testObjectCreator.ABIGAS, testObjectCreator.AUD, 100m, testObjectCreator.AALSHI);

			collection.Add(charge1);
			collection.Add(charge2);
			collection.Add(charge3);
			charge1.JR_DisplaySequence = 7;
			charge2.JR_DisplaySequence = 7;
			charge3.JR_DisplaySequence = 7;

			var intreturn = collection.PostedCostSequenceSort_ForTestOnly(charge1, charge3);
			AssertEquals("Charge 1 is in the database and thus comes before charge 3", -1, intreturn);

			intreturn = collection.PostedCostSequenceSort_ForTestOnly(charge3, charge1);
			AssertEquals("Charge 3 is not in the database and thus comes after charge 1", 1, intreturn);

			intreturn = collection.PostedCostSequenceSort_ForTestOnly(charge1, charge2);
			AssertEquals("Results are equal because both are saved in the Db and have the same sequence number", 0, intreturn);
		}

		public void TestContainsChargeCode()
		{
			SetUpCharges();
			AccChargeCodeCollection collection = new AccChargeCodeCollection(Factory, new ZQuery());
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 4;
			collection.Load(filter);
			Charge1.JR_AC = collection[0].PK;
			Charge3.JR_AC = collection[1].PK;
			Charge4.JR_AC = collection[2].PK;

			AssertNotNull(TestJob.Charges.ContainsChargeCode(collection[0]));
			AssertNotNull(TestJob.Charges.ContainsChargeCode(collection[1]));
			AssertNotNull(TestJob.Charges.ContainsChargeCode(collection[2]));
			AssertNull(TestJob.Charges.ContainsChargeCode(collection[3]));
		}

		public void TestValidationSuspendedWhenDefaultingNewChildAndParentIsSuspended()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			GlbDepartment fEADept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			GlbDeptCharges defaultCharge = fEADept.DeptCharges.AddNew();
			defaultCharge.GD_AC = creator.CC1.PK;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;
			shipment.JS_RL_NKDestination = "USLAX";
			TestJob = Factory.NewJobForTesting<Job>();
			TestJob.PlugInData = shipment;
			AssertEquals("Should be 1 item present", 1, TestJob.Charges.Count);
			Assert("Should not be errors on its description field", !TestJob.Charges[0].JR_DescInfo.HasErrors());
		}

		public void TestValidationStillRunsWhenParentIsNotSuspendedWhenDefaultingNewChild()
		{
			ChargeCollection charges = new ChargeCollection(TestJob);
			TestJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestJob.JH_GB = GlbBranch.CurrentBranch.PK;
			Charge charge = charges.AddNew();
			Assert("Should validate Description", !charge.JR_DescInfo.HasErrors());
		}

		public void TestSetDefaultsForNewChild()
		{
			ZGuid testBranch = ZGuid.NewZGuid();
			ZGuid testDepartment = ZGuid.NewZGuid();

			TestJob.JH_GB = testBranch;
			TestJob.JH_GE = testDepartment;

			Charge newCharge = TestJob.Charges.AddNew();
			AssertEquals(testBranch, newCharge.JR_GB);
			AssertEquals(testDepartment, newCharge.JR_GE);
			AssertEquals(TestJob.PK, newCharge.JR_JH);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, newCharge.JR_RX_NKCostCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, newCharge.JR_RX_NKSellCurrency);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard; //Collect
			TestJob.PlugInData = shipment;

			GlbDepartment department = Factory.New<GlbDepartment>();
			department.GE_Code = "FEA";
			TestJob.JH_GE = department.PK;
		}

		public void TestContainsUnPostedAR()
		{
			Charge charge1 = TestJob.Charges.AddNew();
			Charge charge2 = TestJob.Charges.AddNew();

			AssertEquals(false, TestJob.Charges.ContainsUnPostedAR);

			charge1.JR_OSSellAmt = 10;
			AccTransactionLines line1 = Factory.New<AccTransactionLines>();
			line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge1.JR_AL_ARLine = ZGuid.Empty;

			AssertEquals(true, TestJob.Charges.ContainsUnPostedAR);

			charge1.JR_AL_ARLine = line1.PK;
			AssertEquals(false, TestJob.Charges.ContainsUnPostedAR);

			charge2.JR_LocalSellAmt = 20;
			AssertEquals(true, TestJob.Charges.ContainsUnPostedAR);
		}

		public void TestContainsUnPostedAP()
		{
			Charge charge1 = TestJob.Charges.AddNew();
			Charge charge2 = TestJob.Charges.AddNew();

			AssertEquals(false, TestJob.Charges.ContainsUnPostedAR);

			charge1.JR_OSCostAmt = 10;
			AccTransactionLines line1 = Factory.New<AccTransactionLines>();
			line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			charge1.JR_AL_APLine = ZGuid.Empty;

			AssertEquals(true, TestJob.Charges.ContainsUnPostedAP);

			charge1.JR_AL_APLine = line1.PK;
			AssertEquals(false, TestJob.Charges.ContainsUnPostedAP);

			charge2.JR_LocalCostAmt = 20;
			AssertEquals(true, TestJob.Charges.ContainsUnPostedAP);
		}

		public void TestCanBePostedIsNegativePayment()
		{
			string invNumber = "12345";
			ZGuid creditor = ZGuid.NewZGuid();

			Charge testCharge1 = TestJob.Charges.AddNew();
			testCharge1.JR_OSCostAmt = 30;
			testCharge1.JR_OH_CostAccount = creditor;
			testCharge1.JR_APInvoiceNum = invNumber;
			testCharge1.JR_PaymentType = "CHQ";

			Charge testCharge2 = TestJob.Charges.AddNew();
			testCharge2.JR_OSCostAmt = 50;
			testCharge2.JR_OH_CostAccount = Guid.NewGuid();
			testCharge2.JR_APInvoiceNum = "209374";
			testCharge2.JR_PaymentType = "CHQ";

			Charge testCharge3 = TestJob.Charges.AddNew();
			testCharge3.JR_OSCostAmt = -100;
			testCharge3.JR_OH_CostAccount = creditor;
			testCharge3.JR_APInvoiceNum = invNumber;
			testCharge3.JR_PaymentType = "CHQ";

			Assert("Error must be set", TestJob.Charges.IsNegativePayment);

			testCharge2.JR_OSCostAmt = 70;
			testCharge2.JR_OH_CostAccount = creditor;
			testCharge2.JR_APInvoiceNum = invNumber;
			testCharge2.JR_PaymentType = "CHQ";
			Assert("No error required", !TestJob.Charges.IsNegativePayment);
		}

		public void TestDisplaySequenceWhenAddNewCharge() => TestChargeDisplaySequenceCore(
			assertSkipBehavior: (message, job) =>
			{
				for (var i = 0; i < 10; i++)
				{
					AssertEquals((short)0, job.Charges.AddNew().JR_DisplaySequence);
				}
			},
			assertNonSkipBehavior: (message, job) =>
			{
				for (var sequence = 1; sequence <= 10; sequence++)
				{
					AssertEquals(sequence, job.Charges.AddNew().JR_DisplaySequence);
				}
			}
		);

		public void TestDisplaySequenceWhenAddExistingCharge() => TestChargeDisplaySequenceCore(
			assertSkipBehavior: (message, job) =>
			{
				job.JH_JobNum = "001234555";
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				var chargeNotInDatabase = job.Charges.AddNew();
				chargeNotInDatabase.JR_DisplaySequence = 1;
				AssertEquals((ZShort)1, chargeNotInDatabase.JR_DisplaySequence);

				var childJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				childJob.JH_JobNum = "001234556";
				childJob.JH_JobNum = new ZGuid(Guid.NewGuid()).ToStringKey();
				childJob.JH_GB = GlbBranch.CurrentBranch.PK;
				childJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				childJob.JH_JH_ParentJob = job.PK;
				var chargeFromChildJob = Factory.Load<Charge>(ExistedCharge.PK);
				chargeFromChildJob.FillWithValidTestData();
				chargeFromChildJob.JR_JH = childJob.PK;
				chargeFromChildJob.JR_DisplaySequence = 1;
				job.Charges.Add(chargeFromChildJob);

				AssertEquals("DisplaySequence should not re-sort ", (ZShort)1, chargeFromChildJob.JR_DisplaySequence);
				AssertEquals("DisplaySequence should not re-sort ", (ZShort)1, chargeNotInDatabase.JR_DisplaySequence);
			},
			assertNonSkipBehavior: (message, job) =>
			{
				job.JH_JobNum = "001234555";
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				var chargeNotInDatabase = job.Charges.AddNew();
				AssertEquals((ZShort)1, chargeNotInDatabase.JR_DisplaySequence);

				var childJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				childJob.JH_JobNum = "001234556";
				childJob.JH_JobNum = new ZGuid(Guid.NewGuid()).ToStringKey();
				childJob.JH_GB = GlbBranch.CurrentBranch.PK;
				childJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				childJob.JH_JH_ParentJob = job.PK;
				var chargeFromChildJob = Factory.Load<Charge>(ExistedCharge.PK);
				chargeFromChildJob.FillWithValidTestData();
				chargeFromChildJob.JR_JH = childJob.PK;
				chargeFromChildJob.JR_DisplaySequence = 1;
				job.Charges.Add(chargeFromChildJob);

				AssertEquals("DisplaySequence should re-sort ", (ZShort)1, chargeFromChildJob.JR_DisplaySequence);
				AssertEquals("DisplaySequence should re-sort ", (ZShort)2, chargeNotInDatabase.JR_DisplaySequence);
			}
		);

		void AssertChargeDisplaySequenceCore(Job job, Action<Job> assertTest, bool registryValue)
		{
			job.Charges.RemoveAll();
			using (AccountingConfigurationRegistry.Instance.DisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			{
				assertTest(job);
			}
		}

		Job CreateWarehouseStorageJob()
		{
			var whsPlugIn = new DummyJobInvoicingPlugIn(Factory);
			((DummyJobInvoicingPlugInJobInvoicingSupporter)whsPlugIn.InvoicingSupporter)
				.SetConsumerType(JobInvoicingConsumerTypes.WarehouseStorage);

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.PlugInData = whsPlugIn;
			Assert(job.IsWarehousePeriodicBilling);
			return job;
		}

		void TestChargeDisplaySequenceCore(Action<string, Job> assertSkipBehavior, Action<string, Job> assertNonSkipBehavior)
		{
			var newFactory = new BusinessObjectFactory();
			ExistedCharge = newFactory.NewWithValidTestData<Charge>();
			newFactory.Save();
			CombineAssertions(() =>
			{
				AssertChargeDisplaySequenceCore(
					CreateWarehouseStorageJob(),
					job => assertSkipBehavior("IsForceToSkip", job),
					true);

				AssertChargeDisplaySequenceCore(
					CreateWarehouseStorageJob(),
					job => assertNonSkipBehavior("NotSkip_Warehouse_RegistryOff", job),
					false);

				AssertChargeDisplaySequenceCore(
					Factory.NewJobWithValidTestDataForTesting<Job>(),
					job => assertNonSkipBehavior("NotSkip_NonWarehouse_RegistryOn", job),
					true);

				AssertChargeDisplaySequenceCore(
					Factory.NewJobWithValidTestDataForTesting<Job>(),
					job => assertNonSkipBehavior("NotSkip_NonWarehouse_RegistryOff", job),
					false);
			});
		}

		public void TestSequenceNumberSet()
		{
			AssertEquals(0, TestJob.Charges.Count);

			Charge testCharge1 = TestJob.Charges.AddNew();
			AssertEquals((short)1, testCharge1.JR_DisplaySequence);

			Charge testCharge2 = TestJob.Charges.AddNew();
			AssertEquals((short)2, testCharge2.JR_DisplaySequence);

			TestJob.Charges.Remove(testCharge1);
			Charge testCharge3 = TestJob.Charges.AddNew();
			AssertEquals((short)3, testCharge3.JR_DisplaySequence);

			testCharge3.JR_DisplaySequence = short.MaxValue;
			Charge testCharge4 = TestJob.Charges.AddNew();
			AssertEquals((short)1, testCharge4.JR_DisplaySequence);

			Charge testCharge5 = TestJob.Charges.AddNew();
			AssertEquals((short)3, testCharge5.JR_DisplaySequence);

			testCharge5.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			testCharge5.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			testCharge5.ARLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testCharge5.ARLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			Assert("PostCondition: TestCharge5 has revenue posted.", testCharge5.IsRevenuePosted);
			Charge testCharge6 = TestJob.Charges.AddNew();
			AssertEquals((short)3, testCharge6.JR_DisplaySequence);

			TestJob.Charges.RemoveAll();
			Charge testCharge7 = TestJob.Charges.AddNew();
			AssertEquals((short)1, testCharge7.JR_DisplaySequence);
		}

		public void TestSequenceNumberSetWhenChargeIsNotLoaded()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("100");
			var job = creator.CreateJob(shipment);
			Factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var job1 = newFactory1.Load<Job>(job.PK);
			Charge charge1;
			using (job1.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job1.Charges.IsLoaded);
				charge1 = creator.CreateCharge(job1, creator.CC1, 100m, 100m);
			}
			AssertEquals("first charge added to the shipment", (short)1, charge1.JR_DisplaySequence);
			charge1.Factory.Save();

			var job2 = newFactory1.Load<Job>(job.PK);
			Charge charge2;
			using (job2.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				charge2 = creator.CreateCharge(job2, creator.CC2, 100m, 100m);
			}
			AssertEquals((short)2, charge2.JR_DisplaySequence);

			charge2.Factory.Save();

			var job3 = newFactory1.Load<Job>(job.PK);
			Charge charge3;
			using (job3.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job3.Charges.IsLoaded);
				charge3 = creator.CreateCharge(job3, creator.CC3, 100m, 100m);
			}
			AssertEquals((short)3, charge3.JR_DisplaySequence);

			charge3.JR_DisplaySequence = short.MaxValue;
			charge3.Factory.Save();

			var newFactory2 = new BusinessObjectFactory();
			var job4 = newFactory2.Load<Job>(job.PK);
			Charge charge4;
			using (job4.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job4.Charges.IsLoaded);
				charge4 = creator.CreateCharge(job4, creator.CC4, 100m, 100m);
			}
			AssertEquals((short)3, charge4.JR_DisplaySequence);

			charge1.JR_DisplaySequence = 7;
			charge1.Factory.Save();
			charge4.Factory.Save();

			var newFactory3 = new BusinessObjectFactory();
			var job5 = newFactory3.Load<Job>(job.PK);
			Charge charge5;
			using (job5.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job5.Charges.IsLoaded);
				charge5 = creator.CreateCharge(job5, creator.CC5, 100m, 100m);
			}
			AssertEquals((short)1, charge5.JR_DisplaySequence);
		}

		public void TestGetBiggestSequenceNumberWhenCollectionNotLoadedAndCollectionNotEmpty()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("100");
			var job = creator.CreateJob(shipment);
			Factory.Save();

			var charge1 = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			Factory.Save();
			AssertEquals((short)1, charge1.JR_DisplaySequence);

			var newFactory = new BusinessObjectFactory();
			var job2 = newFactory.Load<Job>(job.PK);
			using (job2.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				var charge2 = creator.CreateCharge(job2, creator.CC2, 100m, 100m);
				AssertEquals((short)2, charge2.JR_DisplaySequence);

				var charge3 = creator.CreateCharge(job2, creator.CC3, 100m, 100m);
				AssertEquals("add another charge without saving, it should use the current collection higher sequence number + 1", (short)3, charge3.JR_DisplaySequence);
			}
		}

		public void TestGetUniqueSequenceNumberForUnpostedChargesWithPostedCharge()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("100");
			var job = creator.CreateJob(shipment);
			Factory.Save();

			var postedRevenueCharge1 = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			var charge1LinkedLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var invoice1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice1.AH_JH = job.PK;
			invoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionType = TransactionTypes.Invoice;
			charge1LinkedLine.AL_AH = invoice1.PK;
			charge1LinkedLine.AL_JH = job.PK;
			charge1LinkedLine.AL_LineType = TransactionLineTypes.Revenue;
			charge1LinkedLine.AL_LineAmount = 100m;
			charge1LinkedLine.AL_AG = creator.GLHeader1.PK;
			postedRevenueCharge1.JR_AL_ARLine = charge1LinkedLine.PK;

			var postedRevenueCharge2 = creator.CreateCharge(job, creator.CC2, 100m, 100m);
			var charge2LinkedLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var invoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice2.AH_TransactionType = TransactionTypes.Invoice;
			charge2LinkedLine.AL_AH = invoice2.PK;
			charge2LinkedLine.AL_JH = job.PK;
			charge2LinkedLine.AL_LineType = TransactionLineTypes.Revenue;
			charge2LinkedLine.AL_LineAmount = 100m;
			charge2LinkedLine.AL_AG = creator.GLHeader1.PK;
			postedRevenueCharge2.JR_AL_ARLine = charge2LinkedLine.PK;
			Factory.Save();

			postedRevenueCharge2.JR_DisplaySequence = short.MaxValue;
			Factory.Save();

			AssertEquals((short)1, postedRevenueCharge1.JR_DisplaySequence);
			AssertEquals(short.MaxValue, postedRevenueCharge2.JR_DisplaySequence);
			AssertNotEquals(ZGuid.Empty, postedRevenueCharge1.JR_AL_ARLine);
			AssertNotEquals(ZGuid.Empty, postedRevenueCharge2.JR_AL_ARLine);
			Assert(postedRevenueCharge1.JR_IsRevenuePosted);
			Assert(postedRevenueCharge2.JR_IsRevenuePosted);

			var newFactory = new BusinessObjectFactory();
			var job2 = newFactory.Load<Job>(job.PK);
			using (job2.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				Assert("Charges Count == 0", job2.Charges.Count == 0);
				var charge3 = creator.CreateCharge(job2, creator.CC5, 100m, 100m);
				AssertEquals("postedCharges: 1 MaxValue; collectionCharges: empty; next sequence number should be: 1", (short)1, charge3.JR_DisplaySequence);

				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				Assert("Charges Count == 1", job2.Charges.Count == 1);
				var charge4 = creator.CreateCharge(job2, creator.CC6, 100m, 100m);
				AssertEquals("unpostedCharges: 1 MaxValue; collectionCharges: 1; next sequence number should be: 2", (short)2, charge4.JR_DisplaySequence);
			}
		}

		public void TestGetUniqueSequenceNumberForUnpostedChargesWithUnpostedChargeWithLinkedLine()
		{
			AssertGetUniqueSequenceNumberForUnpostedCharges(true);
		}

		public void TestGetUniqueSequenceNumberForUnpostedChargesWithUnpostedChargeWithoutLinkedLine()
		{
			AssertGetUniqueSequenceNumberForUnpostedCharges(false);
		}

		public void AssertGetUniqueSequenceNumberForUnpostedCharges(bool isChargeWithLinkedLine = true)
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isChargeWithLinkedLine);

			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("100");
			var job = creator.CreateJob(shipment);
			Factory.Save();

			var charge1 = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			var charge2 = creator.CreateCharge(job, creator.CC2, 100m, 100m);
			var charge3 = creator.CreateCharge(job, creator.CC3, 100m, 100m);
			var charge4 = creator.CreateCharge(job, creator.CC4, 100m, 100m);
			Factory.Save();

			charge4.JR_DisplaySequence = short.MaxValue;
			charge3.JR_DisplaySequence = (short)5;
			charge2.JR_DisplaySequence = (short)3;

			Factory.Save();

			AssertEquals((short)1, charge1.JR_DisplaySequence);
			AssertEquals((short)3, charge2.JR_DisplaySequence);
			AssertEquals((short)5, charge3.JR_DisplaySequence);
			AssertEquals(short.MaxValue, charge4.JR_DisplaySequence);
			AssertEquals(isChargeWithLinkedLine, ZGuid.Empty != charge1.JR_AL_ARLine);
			AssertEquals(isChargeWithLinkedLine, ZGuid.Empty != charge2.JR_AL_ARLine);
			AssertEquals(isChargeWithLinkedLine, ZGuid.Empty != charge3.JR_AL_ARLine);
			AssertEquals(isChargeWithLinkedLine, ZGuid.Empty != charge4.JR_AL_ARLine);
			Assert(!charge1.JR_IsRevenuePosted);
			Assert(!charge2.JR_IsRevenuePosted);
			Assert(!charge3.JR_IsRevenuePosted);
			Assert(!charge4.JR_IsRevenuePosted);

			var newFactory = new BusinessObjectFactory();
			var job2 = newFactory.Load<Job>(job.PK);
			using (job2.ChargesLoadSuspender.GetSuspender())
			{
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				Assert("Charges Count == 0", job2.Charges.Count == 0);
				var charge5 = creator.CreateCharge(job2, creator.CC5, 100m, 100m);
				AssertEquals("unpostedCharges: 1 3 5 MaxValue; next sequence number should be: 2", (short)2, charge5.JR_DisplaySequence);

				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				Assert("Charges Count == 1", job2.Charges.Count == 1);
				var charge6 = creator.CreateCharge(job2, creator.CC6, 100m, 100m);
				AssertEquals("unpostedCharges: 1 2 3 5 MaxValue; next sequence number should be: 4", (short)4, charge6.JR_DisplaySequence);

				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				Assert("Charges Count == 2", job2.Charges.Count == 2);
				var charge7 = creator.CreateCharge(job2, creator.CC7, 100m, 100m);
				AssertEquals("unpostedCharges: 1 2 3 4 5 6 MaxValue; next sequence number should be: 6", (short)6, charge7.JR_DisplaySequence);
			}
		}

		public void TestUnpostedSequenceNumbersFromDbIsCached()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S002");
			var job = creator.CreateJob(shipment);
			Factory.Save();

			var postedRevenueCharge1 = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			var arLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_JH = postedRevenueCharge1.JR_JH;
			arLine.AL_AH = invoice.PK;
			arLine.AL_JH = postedRevenueCharge1.JR_JH;
			arLine.AL_LineType = TransactionLineTypes.Revenue;
			arLine.AL_LineAmount = 100m;
			arLine.AL_AG = creator.GLHeader1.PK;
			postedRevenueCharge1.JR_AL_ARLine = arLine.PK;
			Factory.Save();

			postedRevenueCharge1.JR_DisplaySequence = short.MaxValue;
			Factory.Save();

			var loadCacheQuery = @"
SELECT JR_DisplaySequence AS UnpostedSeqNum
FROM
	dbo.JobCharge
	LEFT JOIN dbo.AccTransactionLines ON AL_PK = JR_AL_ARLine AND AL_LineType = 'REV'
WHERE
	JR_JH = @ParentJobPK
	AND AL_PK IS NULL";

			var newFactory = new BusinessObjectFactory();
			var job2 = newFactory.Load<Job>(job.PK);
			using (job2.ChargesLoadSuspender.GetSuspender())
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands())
			{
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				var charge2 = creator.CreateCharge(job2, creator.CC2, 100m, 100m);
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				var charge3 = creator.CreateCharge(job2, creator.CC3, 100m, 100m);
				AssertEquals("It must load the cache UnpostedSequenceNumbersFromDb only once", 1, Db.Connection.ExecutedCommands.Count(command => command.Contains(loadCacheQuery)));
			}
		}

		public void TestBiggestSequenceNumberFromDbIsCached()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S002");
			var job = creator.CreateJob(shipment);
			Factory.Save();

			var loadCacheQuery = @"
SELECT CONVERT(int, MAX(JR_DisplaySequence)) AS maxSeqNum
FROM dbo.JobCharge
WHERE JR_JH = @ParentJobPK";

			var newFactory = new BusinessObjectFactory();
			var job2 = newFactory.Load<Job>(job.PK);
			using (job2.ChargesLoadSuspender.GetSuspender())
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands())
			{
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				var charge2 = creator.CreateCharge(job2, creator.CC2, 100m, 100m);
				Assert("Charges are not loaded", !job2.Charges.IsLoaded);
				var charge3 = creator.CreateCharge(job2, creator.CC3, 100m, 100m);
				AssertEquals("It must load the cache BiggestSequenceNumberFromDb only once", 1, Db.Connection.ExecutedCommands.Count(command => command.Contains(loadCacheQuery)));
			}
		}

		public void TestValidateInvoiceDetails_InvoiceDate()
		{
			SetUpCharges();

			TestJob.Charges.Validate();
			Assert(!Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(!Charge3.HasRowErrors);
			Assert(!Charge4.HasRowErrors);

			Charge1.JR_APInvoiceNum = "1";
			Charge2.JR_APInvoiceNum = "2";
			Charge3.JR_APInvoiceNum = "1";
			Charge4.JR_APInvoiceNum = "1";

			Charge1.JR_APInvoiceDate = ZDateTime.Today;
			Charge2.JR_APInvoiceDate = ZDateTime.Today;
			Charge3.JR_APInvoiceDate = ZDateTime.Today;
			Charge4.JR_APInvoiceDate = ZDateTime.Today.AddDays(1);

			TestJob.Charges.Validate();
			Assert(Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(Charge3.HasRowErrors);
			Assert(Charge4.HasRowErrors);
		}

		public void TestValidateInvoiceDetails_PaymentDate()
		{
			SetUpCharges();

			TestJob.Charges.Validate();
			Assert(!Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(!Charge3.HasRowErrors);
			Assert(!Charge4.HasRowErrors);

			Charge1.JR_APInvoiceNum = "1";
			Charge2.JR_APInvoiceNum = "2";
			Charge3.JR_APInvoiceNum = "1";
			Charge4.JR_APInvoiceNum = "1";

			Charge1.JR_PaymentDate = ZDateTime.Today;
			Charge2.JR_PaymentDate = ZDateTime.Today;
			Charge3.JR_PaymentDate = ZDateTime.Today;
			Charge4.JR_PaymentDate = ZDateTime.Today.AddDays(1);

			TestJob.Charges.Validate();
			Assert(Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(Charge3.HasRowErrors);
			Assert(Charge4.HasRowErrors);
		}

		public void TestValidateInvoiceDetails_PaymentType()
		{
			SetUpCharges();

			TestJob.Charges.Validate();
			Assert(!Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(!Charge3.HasRowErrors);
			Assert(!Charge4.HasRowErrors);

			Charge1.JR_APInvoiceNum = "1";
			Charge2.JR_APInvoiceNum = "2";
			Charge3.JR_APInvoiceNum = "1";
			Charge4.JR_APInvoiceNum = "1";

			Charge1.JR_PaymentType = "Z";
			Charge2.JR_PaymentType = "Z";
			Charge3.JR_PaymentType = "Z";
			Charge4.JR_PaymentType = "ZY";

			TestJob.Charges.Validate();
			Assert(Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(Charge3.HasRowErrors);
			Assert(Charge4.HasRowErrors);
		}

		public void TestValidateInvoiceDetails_BankAccount()
		{
			SetUpCharges();

			TestJob.Charges.Validate();
			Assert(!Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(!Charge3.HasRowErrors);
			Assert(!Charge4.HasRowErrors);

			Charge1.JR_APInvoiceNum = "1";
			Charge2.JR_APInvoiceNum = "2";
			Charge3.JR_APInvoiceNum = "1";
			Charge4.JR_APInvoiceNum = "1";

			ZGuid bank = ZGuid.NewZGuid();
			Charge1.JR_AB = bank;
			Charge2.JR_AB = bank;
			Charge3.JR_AB = bank;
			Charge4.JR_AB = ZGuid.NewZGuid();

			TestJob.Charges.Validate();
			Assert(Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(Charge3.HasRowErrors);
			Assert(Charge4.HasRowErrors);
		}

		public void TestValidateInvoiceDetails_ChequeBook()
		{
			SetUpCharges();

			TestJob.Charges.Validate();
			Assert(!Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(!Charge3.HasRowErrors);
			Assert(!Charge4.HasRowErrors);

			Charge1.JR_APInvoiceNum = "1";
			Charge2.JR_APInvoiceNum = "2";
			Charge3.JR_APInvoiceNum = "1";
			Charge4.JR_APInvoiceNum = "1";

			ZGuid book = ZGuid.NewZGuid();
			Charge1.JR_AK = book;
			Charge2.JR_AK = book;
			Charge3.JR_AK = book;
			Charge4.JR_AK = ZGuid.NewZGuid();

			TestJob.Charges.Validate();
			Assert(Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(Charge3.HasRowErrors);
			Assert(Charge4.HasRowErrors);
		}

		public void TestValidateInvoiceDetails_Currency()
		{
			SetUpCharges();

			TestJob.Charges.Validate();
			Assert(!Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(!Charge3.HasRowErrors);
			Assert(!Charge4.HasRowErrors);

			Charge1.JR_APInvoiceNum = "1";
			Charge2.JR_APInvoiceNum = "2";
			Charge3.JR_APInvoiceNum = "1";
			Charge4.JR_APInvoiceNum = "1";

			ZString currency = "AAA";
			Charge1.JR_RX_NKCostCurrency = currency;
			Charge2.JR_RX_NKCostCurrency = currency;
			Charge3.JR_RX_NKCostCurrency = currency;
			Charge4.JR_RX_NKCostCurrency = "BBB";

			TestJob.Charges.Validate();
			Assert(Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(Charge3.HasRowErrors);
			Assert(Charge4.HasRowErrors);
		}

		public void TestValidateInvoiceDetails_ChequeNo()
		{
			SetUpCharges();

			TestJob.Charges.Validate();
			Assert(!Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(!Charge3.HasRowErrors);
			Assert(!Charge4.HasRowErrors);

			Charge1.JR_APInvoiceNum = "1";
			Charge2.JR_APInvoiceNum = "2";
			Charge3.JR_APInvoiceNum = "1";
			Charge4.JR_APInvoiceNum = "1";

			Charge1.JR_ChequeNo = "Z";
			Charge2.JR_ChequeNo = "Z";
			Charge3.JR_ChequeNo = "Z";
			Charge4.JR_ChequeNo = "ZY";

			TestJob.Charges.Validate();
			Assert(Charge1.HasRowErrors);
			Assert(!Charge2.HasRowErrors);
			Assert(Charge3.HasRowErrors);
			Assert(Charge4.HasRowErrors);
		}

		public void TestRunDisplaySequenceValidationOnAddingWithGUIMethods()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Charges.AddNew().JR_DisplaySequence = 1;
			Charge newCharge = (Charge)((IBindingList)job.Charges).AddNew();
			newCharge.JR_DisplaySequence = 1;
			((ICancelAddNew)job.Charges).EndNew(1);

			AssertHasRowWarningContaining("JR_DisplaySequence must be validated when charge added through GUI and it's possible to set duplicated sequence number before charge actually added to a collection.",
				newCharge, "Charges with duplicated sequence values will be displayed in the order that they were entered.");
		}

		public void TestContainsDuplicateDisplaySequence()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Charges.AddNew().JR_DisplaySequence = 1;
			job.Charges.AddNew().JR_DisplaySequence = 2;
			AssertEquals("Should not contain duplicate display sequence", false, job.Charges.ContainsDuplicateDisplaySequence_ForTestOnly);

			Charge charge = job.Charges.AddNew();
			charge.JR_DisplaySequence = 1;
			AssertEquals("Should contain duplicate display sequence", true, job.Charges.ContainsDuplicateDisplaySequence_ForTestOnly);

			charge.JR_DisplaySequence = 3;
			AssertEquals("Should not contain duplicate display sequence", false, job.Charges.ContainsDuplicateDisplaySequence_ForTestOnly);
		}

		public void TestContainsChargesFromChildJob()
		{
			TestJob.JH_GB = GlbBranch.CurrentBranch.PK;
			TestJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = TestJob.Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_JH = TestJob.PK;
			Job childJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			childJob.JH_GB = GlbBranch.CurrentBranch.PK;
			childJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			childJob.JH_JH_ParentJob = TestJob.PK;
			charge = childJob.Charges.AddNew();
			charge.FillWithValidTestData();
			TestJob.Charges.Add(charge);
			AssertEquals("Should contain charge from child job", true, TestJob.Charges.ContainsChargesFromChildJob_ForTestOnly);
		}

		public void TestNotContainsDeletedChargeFromdChildJob()
		{
			TestJob.JH_GB = GlbBranch.CurrentBranch.PK;
			TestJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestJob.JH_Name = "TestJob";

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.FillWithValidTestData();
			charge.JR_JH = TestJob.PK;
			TestJob.Charges.Add(charge);

			Job childJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			childJob.JH_Name = "ChildJob";
			childJob.JH_GB = GlbBranch.CurrentBranch.PK;
			childJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			childJob.JH_JH_ParentJob = TestJob.PK;

			var childCharge = Factory.NewWithValidTestData<Charge>();
			childCharge.FillWithValidTestData();
			childJob.Charges.Add(childCharge);
			TestJob.Charges.Add(childCharge);
			childCharge.Delete();
			Factory.Save();
			AssertEquals("Should not contain charge from child job", false, TestJob.Charges.ContainsChargesFromChildJob_ForTestOnly);
		}

		public void TestChargesDoNotHaveDuplicateSequenceNumberErrorsOnAdded()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(factory);
			Job job = factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_Misc, false);
			query.AddToFilter(GlbDepartmentSchema.GE_IsActive, true);
			GlbDepartment department = factory.LoadTop1<GlbDepartment>(query);
			job.JH_GE = department.PK;
			job.JH_JobNum = "Number1";
			ChargeCollection chargeCollection = job.Charges;
			Charge charge = chargeCollection.AddNew();
			charge.JR_AC = testObjectCreator.CC1.PK;
			charge.JR_DisplaySequence = 1;
			Charge charge2 = chargeCollection.AddNew();
			charge2.JR_AC = testObjectCreator.CC2.PK;
			charge2.JR_DisplaySequence = 2;

			AssertEquals("Charge collection should contain 2 charges", 2, chargeCollection.Count);
			AssertEquals(string.Format("Charge Collection should not have errors: {0}", chargeCollection.GetErrors().ToUniqueMessageListString()), false, chargeCollection.HasErrors());

			factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			Job job2 = factory2.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_GE = department.PK;
			job2.JH_JobNum = "Number2";
			job2.JH_JH_ParentJob = job.PK;
			ChargeCollection chargeCollection2 = job2.Charges;
			AccChargeCode chargeCode3 = testObjectCreator.CC3;
			Charge charge3 = chargeCollection2.AddNew();
			charge3.JR_AC = chargeCode3.PK;
			charge3.JR_Desc = "Charge 1";
			charge3.JR_DisplaySequence = 1;

			AccChargeCode chargeCode4 = testObjectCreator.CC4;
			Charge charge4 = chargeCollection2.AddNew();
			charge4.JR_AC = chargeCode4.PK;
			charge4.JR_Desc = "Charge 2";
			charge4.JR_DisplaySequence = 2;

			factory.Save();
			factory2.Save();

			AssertEquals("Charge collection 2 should contain 2 charges", 2, chargeCollection2.Count);
			AssertEquals(string.Format("Charge Collection 2 should not have errors: {0}", chargeCollection2.GetErrors().ToUniqueMessageListString()), false, chargeCollection2.HasErrors());

			List<ZGuid> jobList = new List<ZGuid>();
			jobList.Add(job2.PK);
			chargeCollection.AdditionalJobsToLoadChargesFor_ForTestOnly = jobList.ToArray();
			chargeCollection.Load();

			AssertEquals("Charge collection should contain 4 charges", 4, chargeCollection.Count);
			AssertEquals(string.Format("Charge Collection should not have errors: {0}", chargeCollection.GetErrors().ToUniqueMessageListString()), false, chargeCollection.HasErrors());

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			Job job3 = factory3.Load<Job>(job2.PK);
			ChargeCollection chargeCollection3 = job3.Charges;
			Charge charge5 = chargeCollection3.AddNew();
			charge5.JR_AC = chargeCode3.PK;
			charge5.JR_Desc = "Charge 5";
			charge5.JR_DisplaySequence = 1;

			factory3.Save();

			AssertEquals("Charge collection should contain 5 charges", 5, chargeCollection.Count);
			AssertEquals("JR_DisplaySequence", (ZShort)1, chargeCollection[0].JR_DisplaySequence);
			AssertEquals("JR_DisplaySequence", (ZShort)2, chargeCollection[1].JR_DisplaySequence);
			AssertEquals("JR_DisplaySequence", (ZShort)3, chargeCollection[2].JR_DisplaySequence);
			AssertEquals("JR_DisplaySequence", (ZShort)4, chargeCollection[3].JR_DisplaySequence);
			AssertEquals("JR_DisplaySequence", (ZShort)5, chargeCollection[4].JR_DisplaySequence);
		}

		public void TestChargesDoNotHaveDuplicateSequenceNumberErrorsOnLoaded()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(factory);
			Job job = factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_Misc, false);
			query.AddToFilter(GlbDepartmentSchema.GE_IsActive, true);
			GlbDepartment department = factory.LoadTop1<GlbDepartment>(query);
			job.JH_GE = department.PK;
			job.JH_JobNum = "Number1";
			ChargeCollection chargeCollection = job.Charges;
			Charge charge = chargeCollection.AddNew();
			charge.JR_AC = testObjectCreator.CC1.PK;
			charge.JR_DisplaySequence = 1;
			Charge charge2 = chargeCollection.AddNew();
			charge2.JR_AC = testObjectCreator.CC2.PK;
			charge2.JR_DisplaySequence = 2;

			AssertEquals("Charge collection should contain 2 charges", 2, chargeCollection.Count);
			AssertEquals(string.Format("Charge Collection should not have warnings: {0}", chargeCollection.GetWarnings().ToUniqueMessageListString()), false, chargeCollection.HasWarnings());
			AssertEquals(string.Format("Charge Collection should not have errors: {0}", chargeCollection.GetErrors().ToUniqueMessageListString()), false, chargeCollection.HasErrors());

			factory.Save();

			AccChargeCode chargeCode3 = testObjectCreator.CC3;
			Charge charge3 = chargeCollection.AddNew();
			charge3.JR_AC = chargeCode3.PK;
			charge3.JR_Desc = "Charge 1";
			charge3.JR_DisplaySequence = 1;
			APInvoice apInvoice = factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine apInvoiceLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			apInvoiceLine.AL_AC = chargeCode3.PK;
			apInvoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
			apInvoiceLine.AL_GE = department.PK;
			apInvoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			apInvoiceLine.AL_JH = job.PK;
			charge3.JR_AL_APLine = apInvoiceLine.PK;

			AssertEquals("charge3.JR_IsCostPosted", true, charge3.JR_IsCostPosted);

			AccChargeCode chargeCode4 = testObjectCreator.CC4;
			Charge charge4 = chargeCollection.AddNew();
			charge4.JR_AC = chargeCode4.PK;
			charge4.JR_Desc = "Charge 2";
			charge4.JR_DisplaySequence = 2;
			APInvoice apInvoice2 = factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine apInvoiceLine2 = (APInvoiceLine)apInvoice2.Lines.AddNew();
			apInvoiceLine2.AL_AC = chargeCode4.PK;
			apInvoiceLine2.AL_GB = GlbBranch.CurrentBranch.PK;
			apInvoiceLine2.AL_GE = department.PK;
			apInvoiceLine2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			apInvoiceLine2.AL_JH = job.PK;
			charge4.JR_AL_APLine = apInvoiceLine2.PK;

			AssertEquals("charge4.JR_IsCostPosted", true, charge4.JR_IsCostPosted);

			factory.Save();

			AssertEquals("Charge collection should contain 4 charges", 4, chargeCollection.Count);
			AssertEquals("Charge collection should have warnings", true, chargeCollection.HasWarnings());
			AssertEquals("Charge collection should have no errors", false, chargeCollection.HasErrors());

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Job job2 = factory2.Load<Job>(job.PK);
			ChargeCollection chargeCollection2 = job2.Charges;
			AssertEquals("Charge Collection should contain 4 charges", 4, chargeCollection2.Count);
			AssertEquals(string.Format("Charge Collection 2 should not have warnings: {0}", chargeCollection2.GetWarnings().ToUniqueMessageListString()), false, chargeCollection2.HasWarnings());
			AssertEquals(string.Format("Charge Collection 2 should not have errors: {0}", chargeCollection2.GetErrors().ToUniqueMessageListString()), false, chargeCollection2.HasErrors());

			AssertEquals("1st charge should have cost posted", true, chargeCollection2[0].JR_IsCostPosted);
			AssertEquals("2nd charge should have cost posted", true, chargeCollection2[1].JR_IsCostPosted);
			AssertEquals("3rd charge should have cost posted", false, chargeCollection2[2].JR_IsCostPosted);
			AssertEquals("4th charge should have cost posted", false, chargeCollection2[3].JR_IsCostPosted);

			AssertEquals("charge3 should be 1st", (ZShort)1, chargeCollection2[0].JR_DisplaySequence);
			AssertEquals("charge4 should be 2nd", (ZShort)2, chargeCollection2[1].JR_DisplaySequence);
			AssertEquals("charge1 should be 3rd", (ZShort)3, chargeCollection2[2].JR_DisplaySequence);
			AssertEquals("charge2 should be 4th", (ZShort)4, chargeCollection2[3].JR_DisplaySequence);
		}

		public void TestNumberOfCallsToContainsDuplicateDisplaySequenceOnLoad()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Job job = factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = "S0001000";

			TestObjectCreator testObjectCreator = new TestObjectCreator(factory);
			AccChargeCode chargeCode = testObjectCreator.CC1;

			Job childJob2 = factory.NewJobWithValidTestDataForTesting<Job>();
			childJob2.JH_JobNum = "S0001002";
			childJob2.JH_JH_ParentJob = job.PK;

			Charge charge = childJob2.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 100.00m;

			ChargeCollectionForTest collection = new ChargeCollectionForTest(job);

			for (int i = 0; i < 10; i++)
			{
				charge = collection.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				charge.JR_LocalSellAmt = 100.00m;
			}
			factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			Job job2 = factory2.Load<Job>(job.PK);
			ChargeCollectionForTest collection2 = new ChargeCollectionForTest(job2);
			AssertEquals("collection2.Count == 0", 0, collection2.Count);
			AssertEquals("ContainsDuplicateDisplaySequenceCallCount == 0", 0, collection2.ContainsDuplicateDisplaySequenceCallCount);

			collection2.Load();
			AssertEquals("collection2.Count == 11", 11, collection2.Count);
			AssertEquals("ContainsDuplicateDisplaySequenceCallCount == 1", 1, collection2.ContainsDuplicateDisplaySequenceCallCount);

			Job childJob = factory.NewJobWithValidTestDataForTesting<Job>();
			childJob.JH_JobNum = "S0001001";
			childJob.JH_JH_ParentJob = job.PK;
			charge = childJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 100.00m;
			Charge charge2 = childJob.Charges.AddNew();
			charge2.JR_AC = chargeCode.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_LocalSellAmt = 100.00m;
			Charge charge3 = childJob.Charges.AddNew();
			charge3.JR_AC = chargeCode.PK;
			charge3.JR_GB = GlbBranch.CurrentBranch.PK;
			charge3.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge3.JR_LocalSellAmt = 100.00m;
			factory.Save();

			charge = factory2.Load<Charge>(charge.PK);
			charge2 = factory2.Load<Charge>(charge2.PK);
			charge3 = factory2.Load<Charge>(charge3.PK);

			ChargeCollectionForTest collection3 = new ChargeCollectionForTest(job2);
			collection3.Add(charge);
			collection3.Add(charge2);
			collection3.Add(charge3);
			AssertEquals("collection3.Count == 3", 3, collection3.Count);
			AssertEquals("ContainsDuplicateDisplaySequenceCallCount == 3", 3, collection3.ContainsDuplicateDisplaySequenceCallCount);
		}

		public void TestChargesDuplicateSequenceNumberErrorsWhenEditingExistingCharges()
		{
			var creator = new TestObjectCreator(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge1 = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			charge1.JR_DisplaySequence = 1;
			var charge2 = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			charge2.JR_DisplaySequence = 2;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			jobInNewFactory.Charges.Load();
			var charge1InNewFactory = newFactory.Load<Charge>(charge1.PK);
			charge1InNewFactory.JR_DisplaySequence = 2;

			AssertHasRowWarning(charge1InNewFactory, "Charges with duplicated sequence values will be displayed in the order that they were entered.");
		}

		public void TestChargesHasChangesChangedInvokedWhenOrgModified()
		{
			var helper = new ResetParentScreeningStatusHelperForChargesCollectionTest();
			using (ObjectFactory.Substitute<IResetParentScreeningStatusHelperForCharges>(helper))
			{
				var creator = new TestObjectCreator(Factory);
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();

				var shipment = Factory.New<ForwardingShipment>();
				job.Parent = shipment;
				var charge1 = creator.CreateCharge(job, creator.CC1, 100m, 100m);
				var originalTotalInvokedTimes = helper.TotalInvokedTimes;

				charge1.JR_APInvoiceNum = "12345";
				var invokedTimes = helper.TotalInvokedTimes - originalTotalInvokedTimes;
				AssertGreaterThanOrEqualTo(invokedTimes,1);

				originalTotalInvokedTimes = helper.TotalInvokedTimes;
				var org = Factory.NewWithValidTestData<OrgHeader>();
				charge1.JR_OH_CostAccount = org.PK;
				invokedTimes = helper.TotalInvokedTimes - originalTotalInvokedTimes;
				AssertGreaterThanOrEqualTo(invokedTimes, 1);

				originalTotalInvokedTimes = helper.TotalInvokedTimes;
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				charge1.JR_OH_SellAccount = org2.PK;
				invokedTimes = helper.TotalInvokedTimes - originalTotalInvokedTimes;
				AssertGreaterThanOrEqualTo(invokedTimes, 1);
			}
		}
		public void TestGetUniqueSequenceNumberForUnpostedChargesWhenNextSeqNumOverflow()
		{
			var newCharge = TestJob.Charges.AddNew();
			newCharge.JR_DisplaySequence = 32767;
			AssertNoExceptionThrown(() => TestJob.Charges.GetUniqueSequenceNumberForUnpostedCharges(-2));
			AssertEquals(short.MaxValue, TestJob.Charges.GetUniqueSequenceNumberForUnpostedCharges(-2));

			var newCharge2 = TestJob.Charges.AddNew();
			newCharge2.JR_DisplaySequence = 32767;
			AssertEquals(short.MaxValue, TestJob.Charges.GetUniqueSequenceNumberForUnpostedCharges(-2));
		}

		#region Implementation

		protected Job TestJob;
		protected Charge Charge1;
		protected Charge Charge2;
		protected Charge Charge3;
		protected Charge Charge4;
		protected Charge ExistedCharge;

		protected override void SetUp()
		{
			base.SetUp();

			TestJob = Factory.NewJobWithValidTestDataForTesting<Job>();
		}

		protected void SetUpCharges()
		{
			Charge1 = TestJob.Charges.AddNew();
			Charge2 = TestJob.Charges.AddNew();
			Charge3 = TestJob.Charges.AddNew();
			Charge4 = TestJob.Charges.AddNew();
		}
		#endregion

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ChargeCollection(Factory.NewJobWithValidTestDataForTesting<Job>());
		}

		class ChargeCollectionForTest : ChargeCollection
		{
			public ChargeCollectionForTest(Job parentJob)
				: base(parentJob)
			{
			}

			protected override bool ContainsDuplicateDisplaySequence
			{
				get
				{
					ContainsDuplicateDisplaySequenceCallCount++;
					return base.ContainsDuplicateDisplaySequence;
				}
			}

			public int ContainsDuplicateDisplaySequenceCallCount
			{
				get
				{
					return containsDuplicateDisplaySequenceCallCount;
				}

				set
				{
					containsDuplicateDisplaySequenceCallCount = value;
				}
			}
			int containsDuplicateDisplaySequenceCallCount;
		}

		class ResetParentScreeningStatusHelperForChargesCollectionTest : IResetParentScreeningStatusHelperForCharges
		{
			public int TotalInvokedTimes { get; private set; }

			public void ResetJobParentScreeningStatus(bool hasChanges, IJobHeaderParent jobParent,
				IEnumerable<JobCharge> charges)
			{
				TotalInvokedTimes++;
			}
		}
	}
}
