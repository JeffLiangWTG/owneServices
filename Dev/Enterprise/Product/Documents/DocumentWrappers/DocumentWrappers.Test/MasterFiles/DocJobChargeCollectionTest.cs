using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	public abstract class DocJobChargeCollectionTest<CollectionType, ElementType> : NonPersistentBusinessObjectCollectionTestCase<CollectionType> where CollectionType : DocJobChargeCollection where ElementType : DocJobCharge
	{
		public void TestTotalCharges()
		{
			CreateJobHeaderAndCharges();

			var coll = GetNewCollection(JobHeaderDocWrapper, "TEST0");
			AssertEquals(0M, coll.TotalCharges);
			Assert(coll.Count == 0);

			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, MRGChargeCode.PK);
			var lineCharge3 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, REVChargeCode.PK);
			var lineCharge4 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK);
			var lineCharge5 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, DSBChargeCode.PK);

			coll = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5
			);

			AssertEquals(5, coll.Count);
			AssertEquals(100.000M, coll.TotalCharges);
			Assert(coll.Count > 0);
		}

		public void TestTotalLocalSellAmount()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			CreateJobHeaderAndCharges();

			var coll = GetNewCollection(JobHeaderDocWrapper, "TEST0");
			AssertEquals(0M, coll.TotalCharges);
			Assert(coll.Count == 0);

			var lineCharge1 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK, 1);
			var lineCharge2 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, MRGChargeCode.PK, 0);
			var lineCharge3 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, REVChargeCode.PK, 20);
			var lineCharge4 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK, 20);
			var lineCharge5 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, DSBChargeCode.PK, 50);

			coll = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5
			);

			AssertEquals(5, coll.Count);
			AssertEquals(100.000M, coll.TotalLocalSellAmount);
			Assert(coll.Count > 0);
		}

		public void TestTotalLocalSellAmountIncTax()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			CreateJobHeaderAndCharges();

			var coll = GetNewCollection(JobHeaderDocWrapper, "TEST0");
			AssertEquals(0M, coll.TotalCharges);
			Assert(coll.Count == 0);

			var lineCharge1 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK, 1);
			var lineCharge2 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, MRGChargeCode.PK, 0);
			var lineCharge3 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, REVChargeCode.PK, 2);
			var lineCharge4 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK, 2);
			var lineCharge5 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, DSBChargeCode.PK, 5);

			coll = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5
			);

			AssertEquals(5, coll.Count);
			AssertEquals(110.00M, coll.TotalLocalSellAmountIncTax);
			Assert(coll.Count > 0);
		}

		public void TestTotalTaxAmount()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			CreateJobHeaderAndCharges();

			var coll = GetNewCollection(JobHeaderDocWrapper, "TEST0");
			AssertEquals(0M, coll.TotalCharges);
			Assert(coll.Count == 0);

			var lineCharge1 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK, 1);
			var lineCharge2 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, MRGChargeCode.PK, 0);
			var lineCharge3 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, REVChargeCode.PK, 2);
			var lineCharge4 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK, 2);
			var lineCharge5 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, DSBChargeCode.PK, 5);

			coll = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5
			);

			AssertEquals(5, coll.Count);
			AssertEquals(10.00M, coll.TotalTaxAmount);
			Assert(coll.Count > 0);
		}

		public void TestChargeSheetOSAmountDisplay()
		{
			var jobHeaderBisObj = Factory.NewJobForTesting<JobHeader>();
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeaderBisObj.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeaderBisObj.JH_JobNum = "Job1";
			var jobHeaderDocWrapper = DocJobHeader.New(jobHeaderBisObj, Factory);

			var coll = DocJobChargeCollection.GetCollection(jobHeaderDocWrapper, "TEST0");
			AssertEquals("", coll.ChargeSheetOSAmountDisplay);

			var orgFactory = new BusinessObjectFactory();
			var client = orgFactory.LoadTop1<OrgHeader>(new ZQuery());
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();
			jobHeaderBisObj.LocalChargesPK = client.PK;

			var code = CreateChargeCode("TESTCH");
			var lineCharge = CreateLineCharge(jobHeaderBisObj, client.PK, 100M, code.PK);
			lineCharge.JR_OSSellAmt = 100M;

			Factory.Save();

			coll = GetNewCollection(jobHeaderDocWrapper, "TEST1", lineCharge);
			AssertEquals("", coll.ChargeSheetOSAmountDisplay);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();
			AssertEquals("OS AMOUNT", coll.ChargeSheetOSAmountDisplay);
		}

		public void TestDisbursementTypeJobCharges()
		{
			CreateJobHeaderAndCharges();

			var collection = DocJobChargeCollection.GetCollection(JobHeaderDocWrapper, "TEST0");
			AssertEquals("Collection is empty", 0, collection.DisbursementTypeJobCharges.Count);

			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, MRGChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				lineCharge1,
				lineCharge2
			);

			AssertEquals("Collection has one element", 1, collection.DisbursementTypeJobCharges.Count);

			var lineCharge3 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK);
			var lineCharge4 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, REVChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST2",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4
			);

			AssertEquals("Collection has two elements", 2, collection.DisbursementTypeJobCharges.Count);

			var lineCharge5 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, DSBChargeCode.PK);
			collection = GetNewCollection(JobHeaderDocWrapper, "TEST3",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5
			);

			AssertEquals("Collection has two elements", 3, collection.DisbursementTypeJobCharges.Count);
		}

		public void TestNonZeroDisbursementJobCharges()
		{
			CreateJobHeaderAndCharges();

			var collection = DocJobChargeCollection.GetCollection(JobHeaderDocWrapper, "TEST0");
			AssertEquals("Collection is empty", 0, collection.NonZeroDisbursementJobCharges.Count);

			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, MRGChargeCode.PK);
			var lineCharge3 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, REVChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				lineCharge1,
				lineCharge2,
				lineCharge3
			);
			AssertEquals("Collection has 1 elements", 1, collection.NonZeroDisbursementJobCharges.Count);

			var lineCharge4 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK);
			var lineCharge5 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST2",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5
			);
			AssertEquals("Collection has 3 elements", 3, collection.NonZeroDisbursementJobCharges.Count);

			var lineCharge6 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, DSBChargeCode.PK);
			var lineCharge7 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, MRGChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST3",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5,
				lineCharge6,
				lineCharge7
			);

			AssertEquals("Collection has 3 elements", 3, collection.NonZeroDisbursementJobCharges.Count);
		}

		public void TestNonZeroJobCharges()
		{
			CreateJobHeaderAndCharges();

			var collection = DocJobChargeCollection.GetCollection(JobHeaderDocWrapper, "TEST0");
			AssertEquals("Collection is empty", 0, collection.NonZeroJobCharges.Count);

			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, MRGChargeCode.PK);
			var lineCharge3 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, REVChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				lineCharge1,
				lineCharge2,
				lineCharge3
			);
			AssertEquals("Collection has 2 elements", 2, collection.NonZeroJobCharges.Count);

			var lineCharge4 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK);
			var lineCharge5 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, DSBChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST2",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5
			);
			AssertEquals("Collection has 4 elements", 4, collection.NonZeroJobCharges.Count);

			var lineCharge6 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, MRGChargeCode.PK);
			collection = GetNewCollection(JobHeaderDocWrapper, "TEST3",
				lineCharge1,
				lineCharge2,
				lineCharge3,
				lineCharge4,
				lineCharge5,
				lineCharge6
			);
			AssertEquals("Collection has 4 elements", 4, collection.NonZeroJobCharges.Count);
		}

		public void TestNonZeroChargesOrEstimatedRevenue()
		{
			CreateJobHeaderAndCharges();

			var collection = DocJobChargeCollection.GetCollection(JobHeaderDocWrapper, "TEST0");

			var line1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);
			var line2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, REVChargeCode.PK);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST1",
				line1,
				line2
			);

			AssertEquals("Collection has 1 element", 1, collection.NonZeroChargesOrEstimatedRevenue.Count);

			var line3 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 0.000M, REVChargeCode.PK);
			line3.JR_EstimatedRevenue = 100m;
			collection = GetNewCollection(JobHeaderDocWrapper, "TEST2",
				line1,
				line2,
				line3
			);

			AssertEquals("Collection has 2 element", 2, collection.NonZeroChargesOrEstimatedRevenue.Count);
		}

		public void TestDirectAddReportsDeveloperError()
		{
			CreateJobHeaderAndCharges();
			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);

			var collection = DocJobChargeCollection.GetCollection(JobHeaderDocWrapper, "TEST");

			ErrorReporter.Clear();
			collection.Add(GetNewElement(lineCharge1));
			AssertEquals("Should not add elements to the DocJobChargeCollection outside of PopulateCollection delegate.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAddViaPopulateCollectionDoesNotReportDeveloperError()
		{
			CreateJobHeaderAndCharges();
			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);

			var collection = GetNewCollection(JobHeaderDocWrapper, "TEST");

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST1", lineCharge1);
			AssertEquals("No errors reported", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestGetCollection_PopulateCollection()
		{
			CreateJobHeaderAndCharges();
			JobHeaderBisObj.FillWithValidTestData();
			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, MRGChargeCode.PK);
			Factory.Save();

			var collection = GetNewCollection(JobHeaderDocWrapper, "EMPTY", (coll) => { });
			AssertNotNull(collection);
			AssertEquals(0, collection.Count);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST", (coll) =>
			{
				coll.Add(GetNewElement(lineCharge1));
				coll.Add(GetNewElement(lineCharge2));
			});
			AssertNotNull(collection);
			AssertEquals(2, collection.Count);

			var newWrapper = DocJobHeader.New(JobHeaderBisObj, Factory);
			AssertNotSame("Collections are cached by key including wrapper PK", collection, DocJobChargeCollection.GetCollection(newWrapper, "TEST", (coll) =>
			{
				coll.Add(GetNewElement(lineCharge1));
				coll.Add(GetNewElement(lineCharge2));
			}));

			AssertNotSame("Collections are cached by key including 'property' name", collection, DocJobChargeCollection.GetCollection(JobHeaderDocWrapper, "TEST_", (coll) =>
			{
				coll.Add(GetNewElement(lineCharge1));
				coll.Add(GetNewElement(lineCharge2));
			}));

			var newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", (coll) =>
			{
			});
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertSame("Getting value cached on Factory level", collection, newCollection);

			var lineCharge3 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, REVChargeCode.PK, 2);
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", (coll) =>
			{
				coll.Add(GetNewElement(lineCharge1));
				coll.Add(GetNewElement(lineCharge2));
			});
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertNotSame("Cached value was reset by creating JobCharge", collection, newCollection);

			collection = newCollection;
			lineCharge1.JR_Desc = "Some change";
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", (coll) =>
			{
				coll.Add(GetNewElement(lineCharge1));
				coll.Add(GetNewElement(lineCharge2));
			});
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertNotSame("Cached value was reset by JobCharge change", collection, newCollection);

			collection = newCollection;
			lineCharge3.Delete();
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", (coll) =>
			{
				coll.Add(GetNewElement(lineCharge1));
				coll.Add(GetNewElement(lineCharge2));
			});
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertNotSame("Cached value was reset by deleting a JobCharge", collection, newCollection);

			collection = newCollection;
			var newFactory = new BusinessObjectFactory();
			var chargeInNewFactory = newFactory.Load<JobCharge>(lineCharge2.PK);
			chargeInNewFactory.Delete();
			newFactory.Save();
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", (coll) =>
			{
				coll.Add(GetNewElement(lineCharge1));
			});
			AssertNotNull(newCollection);
			AssertEquals(1, newCollection.Count);
			AssertNotSame("Cached value was reset by deleting a JobCharge via Data Refresh", collection, newCollection);
		}

		public void TestGetCollection_JobCharges()
		{
			CreateJobHeaderAndCharges();
			JobHeaderBisObj.FillWithValidTestData();
			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, DSBChargeCode.PK);
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, MRGChargeCode.PK);
			Factory.Save();

			var collection = GetNewCollection(JobHeaderDocWrapper, "EMPTY");
			AssertNotNull(collection);
			AssertEquals(0, collection.Count);

			collection = GetNewCollection(JobHeaderDocWrapper, "TEST", lineCharge1, lineCharge2);
			AssertNotNull(collection);
			AssertEquals(2, collection.Count);

			var newWrapper = DocJobHeader.New(JobHeaderBisObj, Factory);
			AssertNotSame("Collections are cached by key including wrapper PK", collection, GetNewCollection(newWrapper, "TEST", lineCharge1, lineCharge2));

			AssertNotSame("Collections are cached by key including 'property' name", collection, GetNewCollection(JobHeaderDocWrapper, "TEST_", lineCharge1, lineCharge2));

			var newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST");
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertSame("Getting value cached on Factory level", collection, newCollection);

			var lineCharge3 = CreateLineChargeWithTax(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 20.000M, REVChargeCode.PK, 2);
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", lineCharge1, lineCharge2);
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertNotSame("Cached value was reset by creating JobCharge", collection, newCollection);

			collection = newCollection;
			lineCharge1.JR_Desc = "Some change";
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", lineCharge1, lineCharge2);
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertNotSame("Cached value was reset by JobCharge change", collection, newCollection);

			collection = newCollection;
			lineCharge3.Delete();
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", lineCharge1, lineCharge2);
			AssertNotNull(newCollection);
			AssertEquals(2, newCollection.Count);
			AssertNotSame("Cached value was reset by deleting a JobCharge", collection, newCollection);

			collection = newCollection;
			var newFactory = new BusinessObjectFactory();
			var chargeInNewFactory = newFactory.Load<JobCharge>(lineCharge2.PK);
			chargeInNewFactory.Delete();
			newFactory.Save();
			newCollection = GetNewCollection(JobHeaderDocWrapper, "TEST", lineCharge1);
			AssertNotNull(newCollection);
			AssertEquals(1, newCollection.Count);
			AssertNotSame("Cached value was reset by deleting a JobCharge via Data Refresh", collection, newCollection);
		}

		#region Overrides

		protected override CollectionType GetCollectionToTest()
		{
			CreateJobHeaderAndCharges();
			return GetNewCollection(JobHeaderDocWrapper, "TEST");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobCharge = Factory.New<JobCharge>();
			return GetNewElement(jobCharge);
		}

		public override void TestAdd()
		{
			base.TestAdd();

			AssertEquals("Should not add elements to the DocJobChargeCollection outside of PopulateCollection delegate.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public override void TestDelete()
		{
			Assert("Functionality Not Required by DocJobChargeCollections", true);
		}

		public override void TestTypedget_Item()
		{
			base.TestTypedget_Item();

			AssertEquals("Should not add elements to the DocJobChargeCollection outside of PopulateCollection delegate.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Implementation

		CollectionType GetNewCollection(DocumentWrapper parent, string collectionName, Action<CollectionType> populateCollection)
			=> DocJobChargeCollection.CollectionGetter<CollectionType, ElementType>.GetCollection(parent, collectionName, populateCollection);

		CollectionType GetNewCollection(DocumentWrapper parent, string collectionName, params JobCharge[] jobChargesToAdd)
			=> DocJobChargeCollection.CollectionGetter<CollectionType, ElementType>.GetCollection(parent, collectionName, jobChargesToAdd);

		ElementType GetNewElement(JobCharge jobCharge) => DocJobChargeCollection.CollectionGetter<CollectionType, ElementType>.GetNewElement(jobCharge, Factory);

		protected JobHeader JobHeaderBisObj;
		protected DocJobHeader JobHeaderDocWrapper;
		protected AccChargeCode DSBChargeCode;
		protected AccChargeCode MRGChargeCode;
		protected AccChargeCode REVChargeCode;

		protected AccChargeCode CreateChargeCode(string chargeCode)
		{
			var code = Factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "Test Charge Code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			code.FillWithValidTestData();
			return code;
		}

		protected void CreateJobHeaderAndCharges()
		{
			JobHeaderBisObj = Factory.NewJobForTesting<JobHeader>();
			JobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;
			JobHeaderBisObj.JH_GE = GlbDepartment.CurrentDepartment.PK;

			JobHeaderDocWrapper = DocJobHeader.New(JobHeaderBisObj, Factory);

			var orgHeaderBisObj = Factory.LoadTop1<OrgHeader>(new ZQuery());
			JobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;

			DSBChargeCode = CreateChargeCode("DSBChg");
			DSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;

			MRGChargeCode = CreateChargeCode("MRGChg");
			MRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;

			REVChargeCode = CreateChargeCode("REVChg");
			REVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
		}

		protected JobCharge CreateLineChargeWithTax(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK, ZDecimal gSTAmt)
		{
			JobCharge lineCharge = CreateLineCharge(jobHeaderBisObj, localChargesPK, amount, chargeCodePK);
			if (amount != 0)
			{
				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly((int)(gSTAmt / amount * 100));
				lineCharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			}
			lineCharge.JR_LocalSellAmt = amount;
			return lineCharge;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		protected JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_LocalSellAmt = amount;
			lineCharge.JR_OH_SellAccount = localChargesPK;

			return lineCharge;
		}

		#endregion
	}

	[TestedType(typeof(DocJobChargeCollection))]
	public class DocJobChargeCollectionTest : DocJobChargeCollectionTest<DocJobChargeCollection, DocJobCharge>
	{
	}
}
