using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public sealed class TransactionImportJobChargeMappingProviderTest : TestCaseWithFactory
	{
		#region Get Related Apportion Charge

		public void TestGetRelatedApportionCharge_InvalidParameters()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var apportionedCharge = CreateApportionCharge(consol);

			Factory.Save();

			var header = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.USD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.Creditor1);
			var line = header.Lines[0];
			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);

			var relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Cannot get related apportioned charge because no matching criteria provided.", null, relatedConsolCostApportionJobCharge);

			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, apportionedCharge.PK);
			Factory.ServiceContainer.RemoveService<TransactionImportAdditionalInfoProvider>();

			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Cannot get related apportioned charge because of null additionalInfoProvider", null, relatedConsolCostApportionJobCharge);

			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, apportionedCharge.PK);
			Factory.ServiceContainer.AddService<TransactionImportAdditionalInfoProvider>(additionalInfoProvider);

			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals(null, relatedConsolCostApportionJobCharge);
		}

		public void TestGetRelatedApportionCharge_PrimaryKey()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var apportionedCharge = CreateApportionCharge(consol);

			Factory.Save();

			var header = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.USD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.Creditor1);
			var line = header.Lines[0];

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var matchingCriteriaCollection = new MatchingCriteria[] { JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey", apportionedCharge.PK.ToString()) };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, matchingCriteriaCollection);

			var relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertNull("Cannot get related apportioned charge when the critical matching criterion (PK) is used but the matching is not performed.", relatedConsolCostApportionJobCharge);

			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, ZGuid.NewZGuid());
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertNull("Cannot get related apportioned charge when the mapped charge doesn't exist.", relatedConsolCostApportionJobCharge);

			var jobChargeWithoutConsol = Factory.NewWithValidTestData<Charge>();
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, jobChargeWithoutConsol.PK);
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertNull("Cannot get related apportioned charge when the mapped charge is not an apportioned charge.", relatedConsolCostApportionJobCharge);

			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, apportionedCharge.PK);
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol2.PK, line);
			AssertNull("Cannot get related apportion charge when the mapped apportioned charge belongs to another consol.", relatedConsolCostApportionJobCharge);

			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Successfully get the apportioned charge.", apportionedCharge, relatedConsolCostApportionJobCharge);
		}

		public void TestGetRelatedApportionCharge_DisplaySequence()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var apportionedCharge = CreateApportionCharge(consol1);
			apportionedCharge.JR_DisplaySequence = new ZShort(1);

			Factory.Save();

			var header = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.USD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.Creditor1);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge.JR_AC;
			line.AL_JH = apportionedCharge.JR_JH;

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var matchingCriteriaCollection = new MatchingCriteria[] { JobChargeMappingTestHelper.CreateMatchingCriteria("DisplaySequence", "1") };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, matchingCriteriaCollection);

			var relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol1.PK, line);
			AssertNull("Cannot get related apportioned charge when non-critical matching criteria are used but the matching is not performed.", relatedConsolCostApportionJobCharge);

			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(10));
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol1.PK, line);
			AssertNull("Cannot get related apportioned charge because consol1 doesn't contain apportioned charge with mapped display sequence.", relatedConsolCostApportionJobCharge);

			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(1));
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol2.PK, line);
			AssertNull("Cannot get related apportioned charge because the apportioned charge with display sequence1 belongs to consol1 not consol2.", relatedConsolCostApportionJobCharge);

			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol1.PK, line);
			AssertEquals("Successfully get the apportioned charge.", apportionedCharge, relatedConsolCostApportionJobCharge);
		}

		public void TestGetRelatedApportionCharge_WhenChargeWithSameDisplaySequenceInMemory()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);

			var apportionedCharge = consolCost.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			var header = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.USD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.Creditor1);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge.JR_AC;
			line.AL_JH = job.PK;

			AssertEquals("Precondition", new ZShort(0), apportionedCharge.JR_DisplaySequence);

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var matchingCriteriaCollection = new MatchingCriteria[] { JobChargeMappingTestHelper.CreateMatchingCriteria("DisplaySequence", "0") };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, matchingCriteriaCollection);

			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(0));
			var relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Should not get charge since it has not been saved to database.", null, relatedConsolCostApportionJobCharge);
		}

		public void TestGetRelatedApportionCharge_MultipleMatchedCharges()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost3 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);

			Factory.Save();

			var apportionedCharge1 = consolCost1.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			var apportionedCharge2 = consolCost2.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			var apportionedCharge3 = consolCost3.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			apportionedCharge1.JR_DisplaySequence = new ZShort(1);
			apportionedCharge2.JR_DisplaySequence = new ZShort(2);
			apportionedCharge3.JR_DisplaySequence = new ZShort(2);

			Factory.Save();

			var header = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.USD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.Creditor1);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge1.JR_AC;
			line.AL_JH = job.PK;

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var matchingCriteriaCollection = new MatchingCriteria[] { JobChargeMappingTestHelper.CreateMatchingCriteria("DisplaySequence", "1") };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, matchingCriteriaCollection);

			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(1));
			var relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Should get apportionedCharge1 as the related apportioned charge", apportionedCharge1, relatedConsolCostApportionJobCharge);

			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(2));
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertNull("Cannot get related apportioned charge because two apportioned charges in consol have the same display sequence 1", relatedConsolCostApportionJobCharge);
		}

		public void TestGetRelatedApportionCharge_Mixed()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);

			Factory.Save();

			var apportionedCharge1 = consolCost1.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			var apportionedCharge2 = consolCost2.ApportionmentCharges.Where(x => x.JR_JH == job.PK).Single();
			apportionedCharge1.JR_DisplaySequence = new ZShort(1);
			apportionedCharge2.JR_DisplaySequence = new ZShort(2);

			Factory.Save();

			var header = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.USD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.Creditor1);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge1.JR_AC;
			line.AL_JH = job.PK;

			var primaryKeyCriteria = JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey");
			var displaySequenceCriteria = JobChargeMappingTestHelper.CreateMatchingCriteria("DisplaySequence");

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, new[] { primaryKeyCriteria, displaySequenceCriteria });
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, apportionedCharge1.PK);
			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(2));
			var relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Should get apportionedCharge1 since Primary Key takes precedence.", apportionedCharge1, relatedConsolCostApportionJobCharge);

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, new[] { displaySequenceCriteria });
			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(2));
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Should get apportionedCharge2 by using the display sequence when PK matching criterion doesn't exist.", apportionedCharge2, relatedConsolCostApportionJobCharge);

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, new[] { primaryKeyCriteria, displaySequenceCriteria });
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, ZGuid.Empty);
			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(2));
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Cannot get related apportioned charge because when critical matching (PK) fails, do not use any other matching criteria.", null, relatedConsolCostApportionJobCharge);

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, apportionedCharge1.PK);
			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, new ZShort(2));
			relatedConsolCostApportionJobCharge = TransactionImportJobChargeMappingProvider.GetRelatedApportionCharge(Factory, consol.PK, line);
			AssertEquals("Cannot get related apportioned charge because when no matching criteria used.", null, relatedConsolCostApportionJobCharge);
		}

		#endregion

		public void TestIsRelatedApportionChargeMatchedConsolCost()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var jobChargeInConsol1 = CreateApportionCharge(consol1, "S001");
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			var jobChargeInConsol2 = CreateApportionCharge(consol2, "S002");
			var targetConsolCost = Factory.NewWithValidTestData<JobConsolCost>();

			AssertEquals("Pre-condition", ZGuid.Empty, targetConsolCost.RelatedConsolCostPK);
			var isMatched = TransactionImportJobChargeMappingProvider.IsRelatedApportionChargeMatchedConsolCost(targetConsolCost, null);
			AssertEquals("Should match - If a charge has no related apportion charge, it can match a consol cost without related consol cost", true, isMatched);

			var jobChargeWithoutConsol = Factory.NewWithValidTestData<ApportionSplitCharge>();
			isMatched = TransactionImportJobChargeMappingProvider.IsRelatedApportionChargeMatchedConsolCost(targetConsolCost, jobChargeWithoutConsol);
			AssertEquals("Should match - If a charge's related apportion charge does not have a parent consol cost, it can match a consol cost without related consol cost", true, isMatched);

			targetConsolCost.RelatedConsolCostPK = jobChargeInConsol1.ParentConsolCost.PK;
			isMatched = TransactionImportJobChargeMappingProvider.IsRelatedApportionChargeMatchedConsolCost(targetConsolCost, jobChargeInConsol1);
			AssertEquals("Should match - If a charge's related apportion charge has a parent consol cost A, it can match a consol cost related to the consol cost A", true, isMatched);

			isMatched = TransactionImportJobChargeMappingProvider.IsRelatedApportionChargeMatchedConsolCost(targetConsolCost, null);
			AssertEquals("Should not match - If a charge has no related apportion charge, it cannot match a consol cost which has a related consol cost", false, isMatched);

			isMatched = TransactionImportJobChargeMappingProvider.IsRelatedApportionChargeMatchedConsolCost(targetConsolCost, jobChargeInConsol2);
			AssertEquals("Should not match - If a charge's related apportion charge has a parent consol cost B, it cannot match a consol cost related to consol cost A", false, isMatched);

			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			isMatched = TransactionImportJobChargeMappingProvider.IsRelatedApportionChargeMatchedConsolCost(targetConsolCost, jobChargeInConsol2);
			AssertEquals("Should match - registry is off", true, isMatched);
		}

		public void TestSetRelatedApportionCharge()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);

			var consolCostToSearch1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			var consolCostToSearch2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			var chargeToSearch1 = consolCostToSearch1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var chargeToSearch2 = consolCostToSearch1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);
			var chargeToSearch3 = consolCostToSearch2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);

			var consolCostToUpdate = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			var chargeToUpdate1 = consolCostToUpdate.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var chargeToUpdate2 = consolCostToUpdate.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			var linePK = ZGuid.NewZGuid();

			AssertNull("Pre-condition", chargeToUpdate1.RelatedApportionChargeFromDB);
			AssertNull("Pre-condition", chargeToUpdate2.RelatedApportionChargeFromDB);

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.SetRelatedApportionCharge(Factory, linePK, consolCostToUpdate, null);
			AssertEquals("Should not update RelatedApportionChargeFromDB, because chargesToSearch is null", null, chargeToUpdate1.RelatedApportionChargeFromDB);
			AssertEquals("Should not update RelatedApportionChargeFromDB, because chargesToSearch is null", null, chargeToUpdate2.RelatedApportionChargeFromDB);
			AssertCollectedInfo(null);

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.SetRelatedApportionCharge(Factory, linePK, consolCostToUpdate, new ApportionSplitCharge[] { null });
			AssertEquals("Should not update RelatedApportionChargeFromDB, because the element in chargesToSearch is null", null, chargeToUpdate1.RelatedApportionChargeFromDB);
			AssertEquals("Should not update RelatedApportionChargeFromDB, because the element in chargesToSearch is null", null, chargeToUpdate2.RelatedApportionChargeFromDB);
			AssertCollectedInfo(null);

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.SetRelatedApportionCharge(Factory, linePK, consolCostToUpdate, new[] { chargeToSearch1 });
			AssertEquals("Should RelatedApportionChargeFromDB, because chargeToUpdate1 and chargeToSearch1 are both linked to job 1", chargeToSearch1, chargeToUpdate1.RelatedApportionChargeFromDB);
			AssertEquals("Should not update RelatedApportionChargeFromDB, because chargeToUpdate2 is linked to job2 while no chargeToSearch linked to job2", null, chargeToUpdate2.RelatedApportionChargeFromDB);
			AssertCollectedInfo(JobChargeMappingInfoType.KeyMatched);

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.SetRelatedApportionCharge(Factory, linePK, consolCostToUpdate, new[] { chargeToSearch3, chargeToSearch2 });
			AssertEquals("Should not update RelatedApportionChargeFromDB, since that field of chargeToUpdate1 already has value", chargeToSearch1, chargeToUpdate1.RelatedApportionChargeFromDB);
			AssertEquals("Should update RelatedApportionChargeFromDB, because chargeToUpdate2 and chargeToSearch2 are both linked to job 2", chargeToSearch2, chargeToUpdate2.RelatedApportionChargeFromDB);
			AssertCollectedInfo(JobChargeMappingInfoType.KeyMatched);

			void AssertCollectedInfo(JobChargeMappingInfoType? infoType)
			{
				if (infoType.HasValue)
				{
					AssertEquals(infoType, additionalInfoProvider.TransactionLineMappingInfoCollection.Single(x => x.LinePK == linePK).InfoType);
				}
				else
				{
					AssertEquals(false, additionalInfoProvider.TransactionLineMappingInfoCollection.Any(x => x.LinePK == linePK));
				}
			}
		}

		public void TestSetRelatedConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);

			var relatedConsolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			var relatedCharge1 = relatedConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var relatedCharge2 = relatedConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			var targetConsolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			var targetCharge1 = targetConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var targetCharge2 = targetConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			AssertEquals("Pre-condition", ZGuid.Empty, targetConsolCost.RelatedConsolCostPK);

			TransactionImportJobChargeMappingProvider.SetRelatedConsolCost(targetConsolCost, null);
			AssertEquals("RelatedApportionCharge is null", ZGuid.Empty, targetConsolCost.RelatedConsolCostPK);

			TransactionImportJobChargeMappingProvider.SetRelatedConsolCost(targetConsolCost, relatedCharge1);
			AssertEquals("Has valid RelatedApportionCharge", relatedConsolCost.PK, targetConsolCost.RelatedConsolCostPK);
		}

		public void TestCreateAdditionalInfoProviderIfNecessary()
		{
			AssertNoAdditionalInfoProviderCreated("No TransactionImporterAdditionInfoProvider created due to registry is off.",
				LedgerTypes.AccountsPayable,
				isCancelled: false,
				isRegistryEnabled: false,
				TransactionType.INV);
			AssertNoAdditionalInfoProviderCreated("No TransactionImporterAdditionInfoProvider created due to Ledger Type is is AP.",
				LedgerTypes.AccountsReceivable,
				isCancelled: false,
				isRegistryEnabled: true,
				TransactionType.INV);
			AssertNoAdditionalInfoProviderCreated("No TransactionImporterAdditionInfoProvider created due to transaction is cancelled.",
				LedgerTypes.AccountsPayable,
				isCancelled: true,
				isRegistryEnabled: true,
				TransactionType.INV);
			AssertNoAdditionalInfoProviderCreated("No TransactionImporterAdditionInfoProvider created due to Transaction Type is not INV or CRD.",
				LedgerTypes.AccountsPayable,
				isCancelled: true,
				isRegistryEnabled: true,
				TransactionType.ADJ);
			AssertNoAdditionalInfoProviderCreated("No TransactionImporterAdditionInfoProvider created due to Transaction Type is null.",
				LedgerTypes.AccountsPayable,
				isCancelled: false,
				isRegistryEnabled: true,
				transactionType: null);
			AssertNoAdditionalInfoProviderCreated("No TransactionImporterAdditionInfoProvider created due to Ledger Type is null.",
				null,
				isCancelled: false,
				isRegistryEnabled: true,
				TransactionType.INV);
			AssertNoAdditionalInfoProviderCreated("No Exception should throw when Ledger, IsCancelled, TransactionType are both null.",
				null,
				isCancelled: null,
				isRegistryEnabled: true,
				transactionType: null);

			AssertAdditionalInfoProviderCreated(TransactionType.INV, null);
			AssertAdditionalInfoProviderCreated(TransactionType.CRD, null);
			AssertAdditionalInfoProviderCreated(TransactionType.INV, false);
			AssertAdditionalInfoProviderCreated(TransactionType.CRD, false);
		}

		void AssertNoAdditionalInfoProviderCreated(string notCreateReason, ZString? ledger, ZBool? isCancelled, bool isRegistryEnabled, TransactionType? transactionType)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);
			var newFactory = new BusinessObjectFactory();
			var targetInvoice = newFactory.New<APInvoice>();
			targetInvoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = JobChargeMappingTestHelper.CreateUniversalTransaction("S0000010", null);
			universalTransaction.Ledger = ledger;
			universalTransaction.TransactionType = transactionType;
			universalTransaction.IsCancelled = isCancelled;
			var universalTransactionXml = universalTransaction.Serialize();
			AssertNotNull(universalTransactionXml);
			AssertEquals(0, targetInvoice.Lines.Count);

			TransactionImportJobChargeMappingProvider.Initialize(newFactory, universalTransaction);
			var additionalInfoProvider = newFactory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>();
			AssertNull(notCreateReason, additionalInfoProvider);
		}

		void AssertAdditionalInfoProviderCreated(TransactionType transactionType, ZBool? isCancelled)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var newFactory = new BusinessObjectFactory();
			var targetInvoice = newFactory.New<APInvoice>();
			targetInvoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = JobChargeMappingTestHelper.CreateUniversalTransaction("S0000010", null);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.TransactionType = transactionType;
			universalTransaction.IsCancelled = isCancelled;
			var universalTransactionXml = universalTransaction.Serialize();
			AssertNotNull(universalTransactionXml);
			AssertEquals(0, targetInvoice.Lines.Count);

			TransactionImportJobChargeMappingProvider.Initialize(newFactory, universalTransaction);
			var additionalInfoProvider = newFactory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>();
			AssertNotNull(additionalInfoProvider);
		}

		#region Map Transaction Line

		public void TestMapTransactionLine_InvalidParameter()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);

			Factory.Save();

			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = charge.JR_AC;
			line.AL_JH = job.PK;
			line.OriginalJobCharge = null;

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(charge.PK.ToString(), jobNumber: "S0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals("Map transaction line with null additionalInfoProvider", null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			Factory.ServiceContainer.AddService<TransactionImportAdditionalInfoProvider>(additionalInfoProvider);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, null, jobNumber: "S0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals("Posting Journal has null Primary Key and null Display Sequence.", null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			universalLine = JobChargeMappingTestHelper.CreateUniversalLine("Invalid PK", "Invalid Display Sequence", jobNumber: "S0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals("Posting Journal has invalid Primary Key and invalid Display Sequence.", null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(charge.PK.ToString(), "1");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals("Posting Journal has no Job number nor consol number.", null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
		}

		public void TestMapTransactionLine_Charge()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);

			Factory.Save();

			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = charge1.JR_AC;
			line.AL_JH = job.PK;

			AssertEquals("Pre-condition: charge 1 should have Display Sequence 1.", new ZShort(1), charge1.JR_DisplaySequence);
			AssertEquals("Pre-condition: charge 2 should have Display Sequence 2.", new ZShort(2), charge2.JR_DisplaySequence);

			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(charge1.PK.ToString(), "2", jobNumber: "S0001");
			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals("Should match shipment charge 1 when posting journal has Job number but has no consol number. PK take precedence when its valid.", charge1.PK, line.OriginalJobCharge.PK);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			line.OriginalJobCharge = null;
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, "2", jobNumber: "S0001");
			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals("Should match shipment charge 2; when PK matching criterion doesn't exist, use the display sequence for matching.", charge2.PK, line.OriginalJobCharge.PK);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			line.OriginalJobCharge = null;
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine("Invalid PK", "2", jobNumber: "S0001");
			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals("Should not match any charge; when PK matching fails, do not use any other matching criteria.", null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
		}

		public void TestMapTransactionLine_Charge_CashAdvanceValidation()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_OSCostAmt = 100M;
			charge.JR_OSCostExRate = 1M;
			charge.JR_IsAPCashAdvance = true;

			var cashRequestHeader = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Creditor1, LedgerTypes.AccountsPayable, 100M, 100M, "EUR", CashAdvanceStatusCodes.RequestHeader.Paid);
			var cashRequestLine = TestObjectCreator.CreateCashAdvanceRequestLine(cashRequestHeader.PK, 100M, 100M, CashAdvanceStatusCodes.RequestLine.Paid);
			charge.JR_CAL_APLine = cashRequestLine.PK;

			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(charge.PK.ToString(), jobNumber: "S0001");
			var header = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.USD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.Creditor1);
			var line = header.Lines[0];
			line.AL_AC = charge.JR_AC;
			line.AL_JH = job.PK;
			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);

			var errorMessage = @"The related job charge has a paid Advance Payment in a different currency to the currency of the invoice being posted. Unable to apply Advance Payment to the invoice. Please correct the currency of the Invoice, or cancel and re-create the Advance Payment to post this charge.
Invoice currency is USD | Advance Payment currency of FRT | International Freight is EUR";

			var ex = AssertExceptionThrown<CannotGenerateCashAdvanceJournalException>(() => TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line));
			AssertEquals(errorMessage, ex.Message);

			AssertEquals("Match process is skipped because of Advance Payment Journal exception.", null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
		}

		public void TestMapTransactionLine_ApportionedCharge()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 200m);

			Factory.Save();

			var apportionedCharge1 = consolCost1.ApportionmentCharges.Where(x => x.JR_JH == job1.PK).Single();
			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(apportionedCharge1.PK.ToString(), "1", jobNumber: "S0001", consolNumber: "C0001");
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge1.JR_AC;
			line.AL_JH = job1.PK;

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals(null, line.OriginalJobCharge);
			AssertEquals("Should match consol apportioned charge when posting journal has both Job number and consol number. PK take precedence", apportionedCharge1.PK, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, "1", jobNumber: "S0001", consolNumber: "C0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals(null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals("Should map Display Sequence 1; when PK matching criterion doesn't exist, use the display sequence for matching.", new ZShort(1), additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine("Invalid PK", "1", jobNumber: "S0001", consolNumber: "C0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals(null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals("Should not map any Display Sequence; when PK matching fails, do not use any other matching criteria.", null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
		}

		public void TestMapTransactionLine_ConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 200m);

			Factory.Save();

			var apportionedCharge1 = consolCost1.ApportionmentCharges.Where(x => x.JR_JH == job1.PK).Single();
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			var line = header.Lines[0];
			line.AL_AC = apportionedCharge1.JR_AC;
			line.AL_JH = job1.PK;

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var universalLine = JobChargeMappingTestHelper.CreateUniversalLine(consolCost1.PK.ToString(), "1", jobNumber: null, consolNumber: "C0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals(null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals("Should match consol cost when posting journal has no Job number but has consol number. PK take precedence.", consolCost1.PK, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine(null, "1", jobNumber: null, consolNumber: "C0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals(null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals("Should not map any Display Sequence because the consol cost does not have that property.", null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));

			additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			universalLine = JobChargeMappingTestHelper.CreateUniversalLine("Invalid PK", "1", jobNumber: null, consolNumber: "C0001");
			TransactionImportJobChargeMappingProvider.MapTransactionLine(Factory, universalLine, line);

			AssertEquals(null, line.OriginalJobCharge);
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(line.PK));
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(line.PK));
			AssertEquals("Should not map any Display Sequence; when PK matching fails, do not use any other matching criteria.", null, additionalInfoProvider.GetApportionedChargeDisplaySequence(line.PK));
		}

		#endregion

		public void TestRecordMappingInfo()
		{
			var linePK1 = ZGuid.NewZGuid();
			var mappingInfoType1 = JobChargeMappingInfoType.InvalidKey;
			var linePK2 = ZGuid.NewZGuid();
			var mappingInfoType2 = JobChargeMappingInfoType.InvalidKey;
			var targetPK = ZGuid.NewZGuid();

			AssertNull("Pre-condition", Factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>());
			AssertNoExceptionThrown("No error when there is no TransactionImportAdditionalInfoProvider.", () => TransactionImportJobChargeMappingProvider.RecordMappingInfo(Factory, linePK1, targetPK, mappingInfoType1));

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			AssertNotNull("Pre-condition", Factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>());
			AssertEquals(0, additionalInfoProvider.TransactionLineMappingInfoCollection.Count);

			TransactionImportJobChargeMappingProvider.RecordMappingInfo(Factory, linePK1, targetPK, mappingInfoType1);

			AssertNotNull("The mapping info should be added.", additionalInfoProvider.TransactionLineMappingInfoCollection.Where(x => x.LinePK == linePK1 && x.InfoType == mappingInfoType1));
			AssertEquals(1, additionalInfoProvider.TransactionLineMappingInfoCollection.Count);

			TransactionImportJobChargeMappingProvider.RecordMappingInfo(Factory, linePK2, targetPK, mappingInfoType2);

			AssertNotNull("The mapping info should be added.", additionalInfoProvider.TransactionLineMappingInfoCollection.Where(x => x.LinePK == linePK1 && x.InfoType == mappingInfoType1));
			AssertNotNull(additionalInfoProvider.TransactionLineMappingInfoCollection.Where(x => x.LinePK == linePK2 && x.InfoType == mappingInfoType2));
			AssertEquals(2, additionalInfoProvider.TransactionLineMappingInfoCollection.Count);
		}

		public void TestValidate()
		{
			var linePK1 = ZGuid.NewZGuid();
			var linePK2 = ZGuid.NewZGuid();
			var linePK3 = ZGuid.NewZGuid();
			var targetPKInvalid = ZGuid.Invalid;
			var targetPK1 = ZGuid.NewZGuid();
			var targetPK2 = ZGuid.NewZGuid();

			AssertNull("Pre-condition", Factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>());
			AssertEquals("Should be valid when there is no TransactionImportAdditionalInfoProvider.", true, TransactionImportJobChargeMappingProvider.Validate(Factory));

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			AssertEquals("TransactionImportAdditionalInfoProvider is empty.", true, TransactionImportJobChargeMappingProvider.Validate(Factory));

			additionalInfoProvider.TransactionLineMappingInfoCollection.Add((linePK1, targetPKInvalid, JobChargeMappingInfoType.InvalidKey));
			AssertEquals("Has invalid matching key.", false, TransactionImportJobChargeMappingProvider.Validate(Factory));

			ClearAll();
			AssertEquals("Pre-condition", true, TransactionImportJobChargeMappingProvider.Validate(Factory));
			additionalInfoProvider.TransactionLineMappingInfoCollection.Add((linePK1, targetPK1, JobChargeMappingInfoType.KeyMatched));
			additionalInfoProvider.TransactionLineMatchingCriteriaCollectionMapper[linePK1] = Array.Empty<MatchingCriteria>();
			AssertEquals("There is only one line and is matched.", true, TransactionImportJobChargeMappingProvider.Validate(Factory));

			additionalInfoProvider.TransactionLineMatchingCriteriaCollectionMapper[linePK2] = Array.Empty<MatchingCriteria>();
			AssertEquals("One line is NOT matched.", false, TransactionImportJobChargeMappingProvider.Validate(Factory));

			additionalInfoProvider.TransactionLineMappingInfoCollection.Add((linePK2, targetPK2, JobChargeMappingInfoType.KeyMatched));
			AssertEquals("All lines are matched.", true, TransactionImportJobChargeMappingProvider.Validate(Factory));

			additionalInfoProvider.TransactionLineMappingInfoCollection.Add((linePK2, targetPK2, JobChargeMappingInfoType.KeyMatched));
			AssertEquals("All lines are matched. Mappings will be ignored when have the same line PK and target PK.", true, TransactionImportJobChargeMappingProvider.Validate(Factory));

			additionalInfoProvider.TransactionLineMappingInfoCollection.Add((linePK3, targetPK1, JobChargeMappingInfoType.KeyMatched));
			AssertEquals("Mappings are duplicated when have different line PK and the same target PK.", false, TransactionImportJobChargeMappingProvider.Validate(Factory));

			void ClearAll()
			{
				additionalInfoProvider.TransactionLineMappingInfoCollection.Clear();
				additionalInfoProvider.TransactionLineMatchingCriteriaCollectionMapper.Clear();
			}
		}

		public void TestSetRelatedConsolCostAndApportionCharge_WhenE6_ParentIDIsEmpty()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>($"INV001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);

			var consolCostOnConsol = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);
			var consolCostOnInvoice = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);

			var chargesOnConsol = consolCostOnConsol.ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnInvoice = consolCostOnInvoice.ApportionmentCharges.OfType<ApportionSplitCharge>();

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var matchingCriteriaCollection = new MatchingCriteria[] { JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey", consolCostOnConsol.PK.ToString()) };
			additionalInfoProvider.MapTransactionLineWithConsolCost(invoice.Lines[0].PK, consolCostOnConsol.PK);
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(invoice.Lines[0].PK, matchingCriteriaCollection);

			using (consolCostOnConsol.ReportSettingParentSuspender.GetSuspender())
			{
				consolCostOnConsol.E6_ParentID = ZGuid.Empty;
			}

			TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCostOnInvoice, invoice.Lines[0]);
			AssertEquals("RelatedConsolCostPK shouldn't be set", ZGuid.Empty, consolCostOnInvoice.RelatedConsolCostPK);
			AssertEquals("RelatedApportionChargeFromDB shouldn't be set", true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));
		}

		public void TestSetRelatedConsolCostAndApportionCharge_WhenE6_ParentIDsAreDifferent()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001002");

			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol1);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol1);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);

			var consolCostOnConsol = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);
			var consolCostOnInvoice = TestObjectCreator.CreateConsolCost(invoice, consol1, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);

			var chargesOnConsol = consolCostOnConsol.ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnInvoice = consolCostOnInvoice.ApportionmentCharges.OfType<ApportionSplitCharge>();

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var matchingCriteriaCollection = new MatchingCriteria[] { JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey", consolCostOnConsol.PK.ToString()) };
			additionalInfoProvider.MapTransactionLineWithConsolCost(invoice.Lines[0].PK, consolCostOnConsol.PK);
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(invoice.Lines[0].PK, matchingCriteriaCollection);

			TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCostOnInvoice, invoice.Lines[0]);
			AssertEquals("RelatedConsolCostPK shouldn't be set", ZGuid.Empty, consolCostOnInvoice.RelatedConsolCostPK);
			AssertEquals("RelatedApportionChargeFromDB shouldn't be set", true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));
		}

		public void TestSetRelatedConsolCostAndApportionCharge_WhenCriteriaCollectionIsInvalid()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);

			var consolCostOnConsol = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);
			var consolCostOnInvoice = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);

			var chargesOnConsol = consolCostOnConsol.ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnInvoice = consolCostOnInvoice.ApportionmentCharges.OfType<ApportionSplitCharge>();

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			additionalInfoProvider.MapTransactionLineWithConsolCost(invoice.Lines[0].PK, consolCostOnConsol.PK);
			TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCostOnInvoice, invoice.Lines[0]);
			AssertEquals("No match because there is no matching criteria.", ZGuid.Empty, consolCostOnInvoice.RelatedConsolCostPK);
			AssertEquals(true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));

			var displaySequenceCriteria = JobChargeMappingTestHelper.CreateMatchingCriteria("DisplaySequence", "1");
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(invoice.Lines[0].PK, new MatchingCriteria[] { displaySequenceCriteria });
			TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCostOnInvoice, invoice.Lines[0]);
			AssertEquals("No match because consol cost has no display sequence match.", ZGuid.Empty, consolCostOnInvoice.RelatedConsolCostPK);
			AssertEquals(true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));

			var primaryKeyCriteria = JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey", consolCostOnConsol.PK.ToString());
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(invoice.Lines[0].PK, new MatchingCriteria[] { primaryKeyCriteria });
			TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCostOnInvoice, invoice.Lines[0]);
			AssertEquals("Matched consol cost via primary key.", consolCostOnConsol.PK, consolCostOnInvoice.RelatedConsolCostPK);
			AssertEquals(chargesOnConsol.Single(x => x.JR_JH == job.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job.PK).RelatedApportionChargeFromDB.PK);
		}

		public void TestSetRelatedConsolCostAndApportionCharge()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			Factory.Save();

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);

			var consolCostOnConsol = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);
			var consolCostOnInvoice = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 31m, TestObjectCreator.Creditor1);

			var chargesOnConsol = consolCostOnConsol.ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnInvoice = consolCostOnInvoice.ApportionmentCharges.OfType<ApportionSplitCharge>();

			var additionalInfoProvider = JobChargeMappingTestHelper.RegisterAdditionalInfoProvider(Factory);
			var matchingCriteriaCollection = new MatchingCriteria[] { JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey", consolCostOnConsol.PK.ToString()) };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(invoice.Lines[0].PK, matchingCriteriaCollection);

			additionalInfoProvider.MapTransactionLineWithConsolCost(invoice.Lines[0].PK, consolCostOnConsol.PK);
			using (AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCostOnInvoice, invoice.Lines[0]);
				AssertEquals("RelatedConsolCostPK shouldn't be set", ZGuid.Empty, consolCostOnInvoice.RelatedConsolCostPK);
				AssertEquals("RelatedApportionChargeFromDB shouldn't be set", true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TransactionImportJobChargeMappingProvider.SetRelatedConsolCostAndApportionCharge(consolCostOnInvoice, invoice.Lines[0]);
				AssertEquals("RelatedConsolCostPK should be set", consolCostOnConsol.PK, consolCostOnInvoice.RelatedConsolCostPK);
				AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
				AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		ApportionSplitCharge CreateApportionCharge(ForwardingConsol consol, string shipmentNum = "S001")
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNum, consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 200m, TestObjectCreator.Creditor1, apportionmentListing: apps);
			Factory.Save();

			return consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job.PK);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
