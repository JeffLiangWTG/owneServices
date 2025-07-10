using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(Job))]
	public class AutoratingJobTest : JobHeaderTest
	{
		#region Implementation

		#region Organisations

		OrgHeader fTestOrganisation;
		protected OrgHeader TestOrganisation
		{
			get
			{
				if (fTestOrganisation == null)
				{
					fTestOrganisation = TestObjectCreator.CreateOrgHeader("Org", true, true, true, true, true, true);
				}

				return fTestOrganisation;
			}
		}

		protected OrgHeader fCreditor1;
		protected OrgHeader Creditor1
		{
			get
			{
				if (fCreditor1 == null)
				{
					fCreditor1 = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false);
				}

				return fCreditor1;
			}
		}

		#endregion

		#region Job

		Job fTestJob;
		protected virtual Job TestJob
		{
			get
			{
				if (fTestJob == null)
				{
					fTestJob = TestObjectCreator.CreateJob(Creditor1, 0, null, 0);
				}

				return fTestJob;
			}
		}

		#endregion

		#region Currencies

		protected RefCurrency AUD
		{
			get { return this.TestObjectCreator.LocalCurrency; }
		}

		protected RefCurrency USD
		{
			get { return this.TestObjectCreator.USD; }
		}

		#endregion

		protected ZGuid TestGuid;
		protected TestObjectCreator TestObjectCreator;

		#endregion

		#region Autorating Tests

		#region JR_Desc

		#region JR_Desc EmptyRateLineLocalDescription

		public void TestJR_Desc_EmptyRateLineLocalDescription_None() => AssertJR_Desc_EmptyRateLineLocalDescription(InvoiceDescriptionOptionsList.Codes.None);

		public void TestJR_Desc_EmptyRateLineLocalDescription_All() => AssertJR_Desc_EmptyRateLineLocalDescription(InvoiceDescriptionOptionsList.Codes.All);

		void AssertJR_Desc_EmptyRateLineLocalDescription(string invoiceLineDisplayOption)
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Description Local 地方";

			Creditor1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Creditor1.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup localClientGroup = Creditor1.CompanyData.InvoiceRollupOrGroups.AddNew();
			localClientGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			localClientGroup.PG_InvoiceLineDisplayOption = invoiceLineDisplayOption;

			Factory.Save();

			var clientRateEntry = Helper.NewClientRate(Creditor1).AddRateEntryWithFlatRateLine("AIR", "LCL", "AU", "", "CC1", 100m);
			var clientRateLine = clientRateEntry.RateLines[0];
			clientRateLine.OverrideChargeDescription = true;
			clientRateLine.TL_RateDesc += " UPDATED1";
			clientRateLine.TL_RateDescLocal = "";

			var criteria = new TestRatingCriteria();
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(clientRateLine, 40, 30, 60, autoRatingParameters.Criteria);
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
				var charge = TestJob.Charges.AddNew();
				charge.JR_AC = clientRateLine.TL_AC;

				TestJob.AddAutorateRevenue(rateInfo, charge);

				CombineAssertions("GIVEN empty RateLine.LocalDescription THEN it should default to RateLine.Description", () =>
				{
					AssertContains("InvoiceLineDescription", "CC1 Description UPDATED1", rateInfo.InvoiceLineDescription);
					AssertContains("JobCharge Description", "CC1 Description UPDATED1", TestJob.Charges[0].JR_Desc);
				});
			}
		}

		#endregion

		public void TestJR_Desc_ChargeCodeLocalDescriptionIsEmpty()
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "";

			Creditor1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Factory.Save();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var rateEntry = Helper.NewClientRate(consignee).AddRateEntryWithFlatRateLine("AIR", "LCL", "AU", "", "CC1", 100m);
			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_RateDesc += " UPDATED1";
			rateLine.TL_RateDescLocal = "RateLine Local Description UPDATED2";

			var criteria = new TestRatingCriteria() { LocalClient = consignee };
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(rateLine, 0m, 0m, 0m, autoRatingParameters.Criteria);
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
				var charge = TestJob.Charges.AddNew();
				charge.JR_AC = rateLine.TL_AC;

				TestJob.AddAutorateRevenue(rateInfo, charge);
				AssertEquals
				(
					"GIVEN empty chargeCode.LocalDescription and local Debtor THEN should show RateLine.LocalDescription",
					"RateLine Local Description UPDATED2",
					TestJob.Charges[0].JR_Desc
				);
			}
		}

		public void TestJR_Desc_EmptyDebtor()
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Description Local 地方";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var rateEntry = Helper.NewClientRate(consignee).AddRateEntryWithFlatRateLine("AIR", "LCL", "AU", "", "CC1", 100m);
			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_RateDesc += " UPDATED1";
			rateLine.TL_RateDescLocal += " UPDATED2";

			var criteria = new TestRatingCriteria() { LocalClient = consignee };
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(rateLine, 0m, 0m, 0m, autoRatingParameters.Criteria);
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TestJob.JH_OA_LocalChargesAddr = ZGuid.Empty;

				var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
				var charge = TestJob.Charges.AddNew();
				charge.JR_AC = rateLine.TL_AC;

				TestJob.AddAutorateRevenue(rateInfo, charge);
				AssertEquals
				(
					"GIVEN empty Debtor THEN should show RateLine.Description",
					"CC1 Description UPDATED1",
					TestJob.Charges[0].JR_Desc
				);
			}
		}

		public void TestJR_Desc_CompanyTariff_NonLocalDebtor()
		{
			AssertJR_Desc
			(
				isLocalDebtor: false,
				getRatingHeader: (OrgHeader orgHeader) => Helper.NewCompanyTariff()
			);
		}

		public void TestJR_Desc_CompanyTariff_LocalDebtor()
		{
			AssertJR_Desc
			(
				isLocalDebtor: true,
				getRatingHeader: (OrgHeader orgHeader) => Helper.NewCompanyTariff()
			);
		}

		public void TestJR_Desc_ClientRate_NonLocalDebtor()
		{
			AssertJR_Desc
			(
				isLocalDebtor: false,
				getRatingHeader: (OrgHeader orgHeader) => Helper.NewClientRate(orgHeader)
			);
		}

		public void TestJR_Desc_ClientRate_LocalDebtor()
		{
			AssertJR_Desc
			(
				isLocalDebtor: true,
				getRatingHeader: (OrgHeader orgHeader) => Helper.NewClientRate(orgHeader)
			);
		}

		void AssertJR_Desc(bool isLocalDebtor, Func<OrgHeader, RatingHeader> getRatingHeader)
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Description Local 地方";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			if (isLocalDebtor)
			{
				Creditor1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				Assert("Local Debtor", Creditor1.IsLocalCountry);
			}
			else
			{
				AssertEquals("NonLocal Debtor", false, Creditor1.IsLocalCountry);
			}

			Factory.Save();

			var rateEntry = getRatingHeader(consignee).AddRateEntryWithFlatRateLine("AIR", "LCL", "AU", "", "CC1", 100m);
			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_RateDesc += " UPDATED1";
			rateLine.TL_RateDescLocal += " UPDATED2";

			var criteria = new TestRatingCriteria() { LocalClient = consignee };
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(rateLine, 0m, 0m, 0m, autoRatingParameters.Criteria);
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
				var charge = TestJob.Charges.AddNew();
				charge.JR_AC = rateLine.TL_AC;

				TestJob.AddAutorateRevenue(rateInfo, charge);
				AssertEquals
				(
					"LocalDebtor should show RateLine.LocalDescription, otherwise RateLine.Description",
					isLocalDebtor
						? "CC1 Description Local 地方 UPDATED2"
						: "CC1 Description UPDATED1",
					TestJob.Charges[0].JR_Desc
				);
			}
		}

		TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion

		public void TestJR_DescContainsAdditionalJobReferenceFromAutorating()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			TestJob.LocalChargesPK = consignee.PK;

			TestObjectCreator.Job1.PlugInData = shipment;
			Factory.Save();

			var entry = Factory.New<Costing>().EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.AddNew();
			entry.JobServiceForSpotEntry = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Service");
			var rateLine = entry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());
			var calculationResult = CalculationResult.CreateForTest(rateLine, 10m, 0m, 0m, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
			rateInfo.InvoiceLineDescription = "Service";

			TestJob.Charges.RemoveAndDeleteAll();
			var charge = TestJob.Charges.AddNew();
			charge.JR_AC = rateLine.TL_AC;

			TestJob.AddAutorateRevenue(rateInfo, charge);

			AssertEquals(rateInfo.InvoiceLineDescription, TestJob.Charges[0].JR_Desc);

			TestJob.Charges.RemoveAndDeleteAll();
			entry.TI_ContractNumber = "REF20375";
			rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
			rateInfo.InvoiceLineDescription = "Service";
			charge = TestJob.Charges.AddNew();
			charge.JR_AC = rateLine.TL_AC;

			TestJob.AddAutorateRevenue(rateInfo, charge);

			AssertEquals("Service {REF20375}", TestJob.Charges[0].JR_Desc);

			TestJob.Charges.RemoveAndDeleteAll();
			entry.TI_ContractNumber = "NUU0823745";
			rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
			rateInfo.InvoiceLineDescription = "Service";
			charge = TestJob.Charges.AddNew();
			charge.JR_AC = rateLine.TL_AC;
			TestJob.AddAutorateCost(rateInfo, charge);

			AssertEquals("For cost add autorated for but not use invoice line description", "International Freight {NUU0823745}", TestJob.Charges[0].JR_Desc);
		}

		public void TestAddAutorateCost_ExplicitZeroAmount()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			TestJob.LocalChargesPK = debtor.PK;

			TestObjectCreator.Job1.PlugInData = shipment;

			var info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			info.AddFlatPaymentBasis(100m, shipment.RatingAdapter.OperationalJobCode, AUD.Code);
			TestJob.Charges.RemoveAndDeleteAll();
			var charge = TestJob.Charges.AddNew();
			charge.JR_AC = info.ChargeCode.PK;
			var updatedCharge = TestJob.AddAutorateCost(info, charge);
			AssertEquals(100m, updatedCharge.JR_LocalCostAmt);

			var newInfo = new AutoRateInfo(Factory);
			newInfo.ChargeCode = info.ChargeCode;
			newInfo.AddFlatPaymentBasis(0m, shipment.RatingAdapter.OperationalJobCode, AUD.Code);
			newInfo.HasExplicitZeroAmount = true;
			var newUpdatedCharge = TestJob.AddAutorateCost(newInfo, charge);
			AssertEquals(0m, newUpdatedCharge.JR_LocalCostAmt);
		}

		public void TestAddAutorateRevenue_ExplicitZeroAmount()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			TestJob.LocalChargesPK = debtor.PK;

			TestObjectCreator.Job1.PlugInData = shipment;

			var info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			info.AddFlatPaymentBasis(100m, shipment.RatingAdapter.OperationalJobCode, AUD.Code);
			TestJob.Charges.RemoveAndDeleteAll();
			var charge = TestJob.Charges.AddNew();
			charge.JR_AC = info.ChargeCode.PK;
			var updatedCharge = TestJob.AddAutorateRevenue(info, charge);
			AssertEquals(100m, updatedCharge.JR_LocalSellAmt);

			var newInfo = new AutoRateInfo(Factory);
			newInfo.ChargeCode = info.ChargeCode;
			newInfo.AddFlatPaymentBasis(0m, shipment.RatingAdapter.OperationalJobCode, AUD.Code);
			newInfo.HasExplicitZeroAmount = true;
			var newUpdatedCharge = TestJob.AddAutorateRevenue(newInfo, charge);
			AssertEquals(0m, newUpdatedCharge.JR_LocalSellAmt);
		}

		public void TestAutoRateCost_Return()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			TestJob.LocalChargesPK = debtor.PK;

			TestObjectCreator.Job1.PlugInData = shipment;

			var info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			TestJob.Charges.RemoveAndDeleteAll();
			Charge charge = TestJob.Charges.AddNew();
			charge.JR_AC = info.ChargeCode.PK;
			Charge updatedCharge = TestJob.AddAutorateCost(info, charge);
			AssertEquals(charge, updatedCharge);

			charge.JR_ChargeType = Core.Constants.ChargeType.Revenue;
			Factory.Save();

			updatedCharge = TestJob.AddAutorateCost(info, charge);
			AssertNull("UpdatedCharge", updatedCharge);

			charge.JR_ChargeType = "";
			var transactionLine = Factory.New<AccTransactionLines>();
			charge.JR_AL_APLine = transactionLine.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Cost;

			updatedCharge = TestJob.AddAutorateCost(info, charge);
			AssertNull("UpdatedCharge", updatedCharge);

			info.ProviderPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			updatedCharge = TestJob.AddAutorateCost(info, charge);
			AssertNotNull("UpdatedCharge", updatedCharge);
			AssertNotEquals(charge, updatedCharge);
		}

		public void TestAutoRate_CostCalcDescNeverAppears()
		{
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code).InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			AutoRateInfo info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.New<AccChargeCode>();
			info.CalculationDescription = "my cost is secret don't show anyone!";

			TestJob.Charges.RemoveAndDeleteAll();
			Charge charge = TestJob.Charges.AddNew();
			charge.JR_AC = info.ChargeCode.PK;
			charge.JR_OH_SellAccount = ZGuid.Empty;

			TestJob.SetAmountsOnCharge(charge, info, CostSell.Cost);
			AssertEquals("Cost Calc description should NOT be included", string.Empty, TestJob.Charges[0].JR_Desc);
		}

		public void TestAutoRate_CostCalcDescNotAddedOnAutoRating()
		{
			AutoRateInfo info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			info.CalculationDescription = "Additional text which shouldn't be shown for costs";

			OrgHeader testOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();

			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			Factory.Save();

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = testOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;

			testJob.Charges.RemoveAndDeleteAll();
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = info.ChargeCode.PK;
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_CostRated = true;

			testJob.AddAutorateCost(info, charge);
			AssertEquals("Cost Calc description should NOT be included", string.Empty, testJob.Charges[0].JR_Desc);
		}

		public void TestDisbursementChargeIncludesCalculatedDescriptionForRevenue()
		{
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();

			var debtorGroup = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			debtorGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			debtorGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			TestJob.LocalChargesPK = debtor.PK;
			TestObjectCreator.Job1.PlugInData = shipment;

			Factory.Save();

			var rates = new AutoRateInfoCollection(Factory);
			var dsbRateInfo = rates.AddNew(DSBCharge, AUD.RX_Code, 20m);
			var interactor = new LoggerDecorator();
			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };

			dsbRateInfo.InvoiceLineDescription = DSBCharge.AC_Desc;
			dsbRateInfo.CalculationDescription = "1 20GP Container(s) @ AUD 35.00/Container";
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("Expect DSB charge to have addition description", "RAKHSH CHARGE - 1 20GP Container(s) @ AUD 35.00/Container", TestJob.Charges[0].JR_Desc);

			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Cost, operationalJobCodes);

			AssertEquals("DSB cost should not include CalculationSingleLineDescription", DSBCharge.AC_Desc, TestJob.Charges[0].JR_Desc);
		}

		public void TestDescMaxLength_AutoRateRevenue()
		{
			AutoRateInfo info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.New<AccChargeCode>();
			info.InvoiceLineDescription = ZString.Replicate('A', JobChargeSchema.JR_Desc.MaxLength);
			info.AddFlatPaymentBasis(100, "S00001234", "AUD");
			TestJob.Charges.RemoveAndDeleteAll();
			Charge charge = TestJob.Charges.AddNew();
			TestJob.AddAutorateRevenue(info, charge);
			AssertEquals("full description", info.InvoiceLineDescription, TestJob.Charges[0].JR_Desc);

			info.InvoiceLineDescription = ZString.Replicate('A', JobChargeSchema.JR_Desc.MaxLength + 1);
			TestJob.Charges.RemoveAndDeleteAll();
			charge = TestJob.Charges.AddNew();
			TestJob.AddAutorateRevenue(info, charge);
			AssertEquals("TRIMMED description", info.InvoiceLineDescription.Length - 1, TestJob.Charges[0].JR_Desc.Length);

			info.InvoiceLineDescription = "ZUBIN";
			TestJob.Charges.RemoveAndDeleteAll();
			charge = TestJob.Charges.AddNew();
			TestJob.AddAutorateRevenue(info, charge);
			AssertEquals("FULL description", "ZUBIN", TestJob.Charges[0].JR_Desc);
		}

		public void TestAddAutoRateRevenue_DebtorOverride()
		{
			AutoRateInfo info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.New<AccChargeCode>();
			info.DebtorOverridePK = TestOrganisation.PK;
			info.AddFlatPaymentBasis(100, "S00001234", "AUD");

			AutoRateInfo info2 = new AutoRateInfo(Factory);
			info2.ChargeCode = Factory.New<AccChargeCode>();

			Charge charge = TestJob.Charges.AddNew();
			TestJob.AddAutorateRevenue(info, charge);
			AssertEquals("Correct Debtor", TestOrganisation.PK, TestJob.Charges[0].SellAccount.PK);

			Charge charge2 = TestJob.Charges.AddNew();
			TestJob.AddAutorateRevenue(info2, charge2);
			AssertNull("Blank Debtor", TestJob.Charges[1].SellAccount);
		}

		public void TestAddAutoRateRevenue_WithPostedDisbursementCostAndNoDebtor()
		{
			AutoRateInfo info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.New<AccChargeCode>();
			info.DebtorOverridePK = TestOrganisation.PK;
			info.AddFlatPaymentBasis(100, "S00001234", "AUD");

			Charge charge = TestJob.Charges.AddNew();
			charge.JR_ChargeType = Core.Constants.ChargeType.Disbursement;
			charge.JR_AC = info.ChargeCode.PK;
			charge.JR_OH_SellAccount = ZGuid.Empty;
			APInvoiceLine costPosted = Factory.New<APInvoiceLine>();
			charge.JR_AL_APLine = costPosted.PK;
			Assert("Is Disbursement Charge", charge.IsDisbursementCharge);
			Assert("Cost is Posted on Charge", charge.IsCostPosted);
			AssertEquals("Pre-condition: Only one Charge", 1, TestJob.Charges.Count);

			Charge resultCharge = TestJob.AddAutorateRevenue(info, charge);
			AssertEquals("No new Charge added", 1, TestJob.Charges.Count);
			AssertEquals("Nothing updated", 0m, TestJob.Charges[0].JR_OSSellAmt);
		}

		public void TestAddAutoRateRevenue_Return()
		{
			AutoRateInfo info = new AutoRateInfo(Factory);
			info.ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			info.AddFlatPaymentBasis(100, "S00001234", "AUD");
			TestJob.Charges.RemoveAndDeleteAll();

			Charge charge = TestJob.Charges.AddNew();
			charge.JR_AC = info.ChargeCode.PK;
			Charge updatedCharge = TestJob.AddAutorateRevenue(info, charge);
			AssertEquals(charge, updatedCharge);

			Factory.Save();

			charge.JR_SellRatingOverride = true;
			charge.HasChanges = false;
			updatedCharge = TestJob.AddAutorateRevenue(info, charge);
			AssertNull("UpdatedCharge", updatedCharge);

			charge.JR_SellRatingOverride = false;
			charge.HasChanges = false;

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			charge.ReverseWIP(ZDateTime.Now);
			charge.JR_AL_ARLine = transactionLine.PK;
			charge.HasChanges = false;
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			updatedCharge = TestJob.AddAutorateRevenue(info, charge);
			AssertNull("UpdatedCharge", updatedCharge);

			TestJob.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			updatedCharge = TestJob.AddAutorateRevenue(info, charge);
			AssertNotNull("UpdatedCharge", updatedCharge);
			AssertNotEquals(charge, updatedCharge);

			ErrorReporter.Clear(); // To clear developer error for resetting HasChanges on BizO in Db with real changes
		}

		public void TestAddAutoRateRevenue_WhenOnlyCostWasAutorated()
		{
			AssertAddAutoRateRevenue_WhenOnlyCostWasAutorated(Factory.New<AccChargeCode>());
		}

		public void TestAutoRateRevenueWhenDSBCharge()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CUSDSB";

			var info = new AutoRateInfo(Factory);
			info.ChargeCode = chargeCode;
			info.AddFlatPaymentBasis(100, "S00001234", "AUD");
			info.OverriddenGSTAmount = 6.43M;
			info.Currency = "NZD";
			info.UseOverriddenGSTAmount = true;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var taxRate = TestObjectCreator.CreateTaxRate("rate", "test", 1);

			TestJob.Charges.RemoveAndDeleteAll();
			var currency = TestObjectCreator.GetCurrency("NZD");
			var header = TestObjectCreator.CreateOrgHeader("test header", true, true);
			var charge = TestObjectCreator.CreateCharge(TestJob, chargeCode, "test charge", currency, 100, header, currency, 100, header);
			charge.JR_AC = info.ChargeCode.PK;
			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_ChargeType = Core.Constants.ChargeType.Disbursement;
			charge.JR_CostRatingOverride = false;

			var updatedCharge = TestJob.AddAutorateRevenue(info, charge);

			AssertEquals(updatedCharge.JR_OSCostGSTAmt_Calc, 1m);
			AssertEquals(updatedCharge.JR_OSSellGSTAmt_Calc, 1m);
		}

		public void TestLocalSellAmountWhenAutorateRevenueWithDSBCharge()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CUSDSB";

			var info = new AutoRateInfo(Factory);
			info.ChargeCode = chargeCode;
			info.AddFlatPaymentBasis(100, "S00001234", "JPY");
			info.OverriddenGSTAmount = 6.43M;
			info.Currency = "JPY";
			info.UseOverriddenGSTAmount = true;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var taxRate = TestObjectCreator.CreateTaxRate("rate", "test", 1);

			TestJob.Charges.RemoveAndDeleteAll();
			var currency = TestObjectCreator.GetCurrency("JPY");
			var header = TestObjectCreator.CreateOrgHeader("test header", true, true);
			var charge = TestObjectCreator.CreateCharge(TestJob, chargeCode, "test charge", currency, 100, header, currency, 100, header);
			charge.JR_AC = info.ChargeCode.PK;
			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_ChargeType = Core.Constants.ChargeType.Disbursement;
			charge.JR_CostRatingOverride = false;
			charge.JR_OSSellAmt = 505.53m;
			charge.JR_OSCostAmt = 505.53m;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(136.25m);
			charge.JR_LocalCostAmt = 68879m;
			charge.JR_LocalSellAmt = 68879m;

			var updatedCharge = TestJob.AddAutorateRevenue(info, charge);

			AssertEquals(updatedCharge.JR_LocalSellAmt, 68879m);
			AssertEquals(updatedCharge.JR_LocalCostAmt, 68879m);
		}

		public void TestAddAutoRateRevenue_WhenOnlyCostWasAutorated_AndCostMarginIsSet()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = "MRG";
			chargeCode.AC_Code = "ABC";
			chargeCode.AC_MarginPercentage = 50m;

			AssertAddAutoRateRevenue_WhenOnlyCostWasAutorated(chargeCode);
		}

		void AssertAddAutoRateRevenue_WhenOnlyCostWasAutorated(AccChargeCode chargeCode)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			TestJob.PlugInData = shipment;

			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;
			var rates = new AutoRateInfoCollection(Factory);
			var revenueInfo = rates.AddNew(chargeCode, AUD.RX_Code, 100, operationalJobCode);
			var interactor = new LoggerDecorator();

			TestJob.Charges.RemoveAndDeleteAll();

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { operationalJobCode };

			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			var charge = TestJob.Charges[0];
			AssertEquals("Precondition: sell amount was set", 100m, charge.JR_OSSellAmt);
			AssertEquals("Precondition: cost amount was set", 100m * 0.01m * chargeCode.AC_MarginPercentage, charge.JR_OSCostAmt);

			rates = new AutoRateInfoCollection(Factory);
			var costInfo = rates.AddNew(chargeCode, AUD.RX_Code, 80, operationalJobCode);

			strategy.AddAutoRates(interactor, rates, CostSell.Cost, operationalJobCodes);
			AssertEquals("Sell amount was not reset", 100m, charge.JR_OSSellAmt);
			AssertEquals("Cost amount was set", 80m, charge.JR_OSCostAmt);
		}

		public void TestIAutoRatingAccountingInfo()
		{
			#region Setup
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TEST1";
			consignee.OH_IsDebtor = false;
			consignee.OH_IsConsignor = false;
			consignee.OH_IsConsignee = true;
			consignee.OH_IsBroker = false;
			consignee.OH_IsForwarder = false;

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TEST2";
			consignor.OH_IsDebtor = false;
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = false;
			consignor.OH_IsBroker = false;
			consignor.OH_IsForwarder = false;

			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			OrgHeader broker = Factory.New<OrgHeader>();
			broker.OH_Code = "TEST4";
			broker.OH_IsDebtor = false;
			broker.OH_IsConsignor = false;
			broker.OH_IsConsignee = false;
			broker.OH_IsBroker = true;
			broker.OH_IsForwarder = false;

			OrgHeader forwarder = CreateForwarder();

			Factory.Save();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			TestJob.LocalChargesPK = debtor.PK;

			AssertEquals(debtor, TestJob.LocalCharges);

			TestJob.PlugInData = shipment;

			GlbDepartment importDepartment = Factory.New<GlbDepartment>();
			#endregion

			importDepartment.GE_Code = "FIA";

			TestJob.JH_GE = importDepartment.PK;
			TestJob.LocalChargesPK = debtor.PK;

			AssertEquals(debtor, TestJob.LocalCharges);

			importDepartment.GE_Code = "FEA";
			AssertEquals(debtor, TestJob.LocalCharges);

			TestJob.LocalChargesPK = broker.PK;
			AssertEquals(broker, TestJob.LocalCharges);

			importDepartment.GE_Code = "FIA";
			TestJob.LocalChargesPK = broker.PK;
			AssertEquals(broker, TestJob.LocalCharges);

			importDepartment.GE_Code = "FEA";
			TestJob.LocalChargesPK = forwarder.PK;
			AssertEquals(forwarder, TestJob.LocalCharges);

			importDepartment.GE_Code = "FIA";
			TestJob.LocalChargesPK = forwarder.PK;
			AssertEquals(forwarder, TestJob.LocalCharges);

			importDepartment.GE_Code = "BRN";
			TestJob.LocalChargesPK = forwarder.PK;
			AssertEquals(forwarder, TestJob.LocalCharges);

			TestJob.LocalChargesPK = broker.PK;
			AssertEquals(broker, TestJob.LocalCharges);
		}

		public void TestAutoRateDoesNotProduceDuplicateChargeDescriptions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			OrgHeader testOrg1 = orgFactory.New<OrgHeader>();
			testOrg1.OH_Code = "Test1Z";
			testOrg1.CompanyData.OB_APCategory = "ABC"; //just to get the default invoice roll up group created
			orgFactory.Save();

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode.AC_Code = "ZUB11";
			chargeCode.AC_Desc = "RAKHSH CHARGE";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;

			TestJob.LocalChargesPK = testOrg1.PK;
			TestJob.PlugInData = shipment;

			var rates = new AutoRateInfoCollection(Factory);
			AutoRateInfo rate1 = rates.AddNew(chargeCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100);
			rate1.AdditionalInvoiceLineDescription = "Additional Invoice Line description";

			rates.AddIfNotExist(rate1);

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			strategy.AddAutoRates(new LoggerDecorator(), rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("New charge added", 1, TestJob.Charges.Count);
			AssertEquals("New charge with rated amount", 100m, TestJob.Charges[0].JR_LocalSellAmt);
			AssertEquals("New charge with debtor", testOrg1.PK, TestJob.Charges[0].JR_OH_SellAccount);
			AssertEquals("New charge's should not contain duplicated 'Additional Invoice Line description'", "RAKHSH CHARGE\r\nAdditional Invoice Line description", TestJob.Charges[0].JR_Desc);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignDisbursementCharge()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			debtor.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			Factory.Save();

			TestJob.LocalChargesPK = debtor.PK;

			TestObjectCreator.Job1.PlugInData = shipment;

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };

			var dsbCharge = Factory.New<AccChargeCode>();
			dsbCharge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			dsbCharge.AC_Code = "ZUB11";
			dsbCharge.AC_Desc = "RAKHSH CHARGE";

			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(dsbCharge, USD.RX_Code, 100);

			strategy.AddAutoRates(new LoggerDecorator(), rates, CostSell.Cost, operationalJobCodes);
			AssertEquals(1, TestJob.Charges.Count);

			AssertEquals("OS Cost Currency set to foreign currency", USD.RX_Code, TestJob.Charges[0].JR_RX_NKCostCurrency);
			AssertEquals("OS Sell Currency set to foreign currency", USD.RX_Code, TestJob.Charges[0].JR_RX_NKCostCurrency);
			AssertEquals("OS Cost Amount set to foreign amount autorated", 100m, TestJob.Charges[0].JR_OSCostAmt);
			AssertEquals("OS Sell Amount set to cost amount (disbursement charge code)", 100m, TestJob.Charges[0].JR_OSSellAmt);

			Assert("Local Cost Amount is not the autorated foreign amount", TestJob.Charges[0].JR_LocalCostAmt != 100m);
			Assert("Local Sell Amount is not the autorated foreign amount", TestJob.Charges[0].JR_LocalSellAmt != 100m);
		}

		public void TestAutoRateDisbursement_NoSellAmount()
		{
			#region Setup

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = "";

			Factory.Save();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			TestJob.LocalChargesPK = debtor.PK;

			TestObjectCreator.Job1.PlugInData = shipment;

			#endregion

			AccChargeCode dSBCharge = Factory.New<AccChargeCode>();
			dSBCharge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			dSBCharge.AC_Code = "ZUB11";
			dSBCharge.AC_Desc = "RAKHSH CHARGE";

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };

			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(dSBCharge, USD.RX_Code, 100);

			strategy.AddAutoRates(new LoggerDecorator(), rates, CostSell.Cost, operationalJobCodes);
			AssertEquals(1, TestJob.Charges.Count);

			AssertEquals("OS Cost Currency set to foreign currency", USD.RX_Code, TestJob.Charges[0].JR_RX_NKCostCurrency);
			AssertEquals("OS Sell Currency set to foreign currency", USD.RX_Code, TestJob.Charges[0].JR_RX_NKCostCurrency);
			AssertEquals("OS Cost Amount set to foreign amount autorated", 100m, TestJob.Charges[0].JR_OSCostAmt);
			AssertEquals("OS Sell Amount set to cost amount (disbursement charge code)", 100m, TestJob.Charges[0].JR_OSSellAmt);
		}

		public void TestAutoRateForeignDisbursementChargeWithOverriddenGSTSpecifiedNotGSTRegistered()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(false, ZGuid.Empty, 145m, 0m, 145m, 0m);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignDisbursementChargeWithOverriddenGSTSpecifiedGSTRegistered()
		{
			AccTaxRate newTaxRate = TestObjectCreator.CreateTaxRate("ZZ~GST", "RateForTest", 125, 10);
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, newTaxRate.PK, 100m, 12.50m, 100m, 12.50m);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignDisbursementChargeWithOverriddenGSTSpecifiedGSTRegisteredWithRateByDateForCost()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 100m, 10m, 100m, 10m, ZDate.Today, true);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignDisbursementChargeWithOverriddenGSTSpecifiedGSTRegisteredWithRateByDateForSell()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 100m, 10m, 100m, 10m, ZDate.Today, false);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignDisbursementChargeWithOverriddenGSTSpecifiedGSTRegisteredWithNoRateByDateForCost()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 145m, 0m, 145m, 14.5m, TestObjectCreator.GST1WithDates_DateWithNoRate, true);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignDisbursementChargeWithOverriddenGSTSpecifiedGSTRegisteredWithNoRateByDateForSell()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 100m, 10m, 100m, 0m, TestObjectCreator.GST1WithDates_DateWithNoRate, false);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignChargeWithOverriddenGSTSpecifiedGSTRegisteredWithRateByDateForCost()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 100m, 10m, 0m, 0m, ZDate.Today, true, false);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignChargeWithOverriddenGSTSpecifiedGSTRegisteredWithRateByDateForSell()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 0m, 0m, 100m, 10m, ZDate.Today, false, false);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignChargeWithOverriddenGSTSpecifiedGSTRegisteredWithNoRateByDateForCost()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 145m, 0m, 0m, 0m, TestObjectCreator.GST1WithDates_DateWithNoRate, true, false);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateForeignChargeWithOverriddenGSTSpecifiedGSTRegisteredWithNoRateByDateForSell()
		{
			CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(true, TestObjectCreator.GST1WithDates.PK, 0m, 0m, 145m, 0m, TestObjectCreator.GST1WithDates_DateWithNoRate, false, false);
		}

		void CoreTestForAutoRateForeignChargeWithOverriddenGSTSpecified(bool isCompanyGSTRegisterd, ZGuid gSTRatePK,
			decimal expectedCostAmount, decimal expectedCostGSTAmount, decimal expectedSellAmount, decimal expectedSellGSTAmount, ZDate? taxDate = null, bool isForCost = false, bool isDisbursement = true)
		{
			bool originalValue = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = isCompanyGSTRegisterd;

			try
			{
				#region Setup

				OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();

				Factory.Save();
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				TestJob.LocalChargesPK = debtor.PK;

				TestObjectCreator.Job1.PlugInData = shipment;

				#endregion

				AccChargeCode dSBCharge = Factory.New<AccChargeCode>();
				if (isDisbursement)
				{
					dSBCharge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				}
				dSBCharge.AC_Code = "ZUB11";
				dSBCharge.AC_Desc = "RAKHSH CHARGE";
				dSBCharge.AC_AT_GSTRate = gSTRatePK;

				var rates = new AutoRateInfoCollection(Factory);
				var rate1 = rates.AddNew(dSBCharge, USD.RX_Code, 100);
				rate1.ProviderPK = TestObjectCreator.CreateOrgHeader("ZUB", true, true).PK;
				rate1.UseOverriddenGSTAmount = true;
				rate1.OverriddenGSTAmount = 45;

				TestJob.AgentCollectPK = TestObjectCreator.ABIGAS.PK;
				TestJob.LocalChargesPK = TestObjectCreator.AALSHI.PK;
				TestJob.LocalCharges.CompanyData.SetARTaxApplicable(true);

				if (taxDate.HasValue)
				{
					var charge = TestJob.Charges.AddNew();
					charge.JR_AC = dSBCharge.PK;
					charge.AddPaymentBases(new[] { TestObjectCreator.CreatePaymentBasis(20, shipment.RatingAdapter.OperationalJobCode) }, isForCost);
					if (isForCost)
					{
						charge.JR_CostTaxDate = taxDate.Value;
					}
					else
					{
						charge.JR_SellTaxDate = taxDate.Value;
					}
				}

				var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
				var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };

				strategy.AddAutoRates(new LoggerDecorator(), rates, isForCost ? CostSell.Cost : CostSell.Revenue, operationalJobCodes);

				AssertEquals(1, TestJob.Charges.Count);

				var testCharge = TestJob.Charges[0];
				testCharge.JR_OSSellExRate = 0.5m;
				AssertEquals("OS Cost Currency", isDisbursement || isForCost ? USD.RX_Code : AUD.RX_Code, testCharge.JR_RX_NKCostCurrency);
				AssertEquals("OS Cost Amount", expectedCostAmount, testCharge.JR_OSCostAmt);
				AssertEquals("OS Cost GST Amount", expectedCostGSTAmount, testCharge.JR_OSCostGSTAmt_Calc);

				AssertEquals("OS Sell Currency", USD.RX_Code, testCharge.JR_RX_NKSellCurrency);
				AssertEquals("OS Sell Amount", expectedSellAmount, testCharge.JR_OSSellAmt);
				AssertEquals("OS Sell GST Amount", expectedSellGSTAmount, testCharge.JR_OSSellGSTAmt_Calc);

				Assert("Local Cost Amount is not the autorated foreign amount", testCharge.JR_LocalCostAmt != 100m);
				Assert("Local Sell Amount is not the autorated foreign amount", testCharge.JR_LocalSellAmt != 100m);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			}
		}

		#region TestAddAutoRates

		public void TestAddAutoRates()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			TestJob.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			TestObjectCreator.Job1.PlugInData = shipment;
			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;

			TestJob.JH_GE = TestObjectCreator.FIADepartment.PK;
			AccChargeCode dSB = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			dSB.AC_DepartmentFilterList = "ALL";
			dSB.AC_GC = GlbCompany.CurrentCompany.PK;
			AccChargeCode fRT = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			fRT.AC_DepartmentFilterList = "ALL";
			fRT.AC_GC = GlbCompany.CurrentCompany.PK;

			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(dSB, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100, operationalJobCode);
			var interactor = new LoggerDecorator();

			var rate2 = rates.AddNew(fRT, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 200, operationalJobCode);
			rate2.Attributes.Add(JobChargeAttribTypeList.Codes.Commodity, "GEN");
			rate2.Attributes.Add(JobChargeAttribTypeList.Codes.LocationType, "AAA");
			rate2.Attributes.Add(JobChargeAttribTypeList.Codes.LocationDesc, "LOC1");

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { operationalJobCode };

			AssertEquals(0, TestJob.Charges.Count);
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals(2, TestJob.Charges.Count);
			AssertEquals("Should be valid", true, TestJob.Charges[0].JR_AC.IsValid);
			AssertEquals("Should be valid", true, TestJob.Charges[1].JR_AC.IsValid);

			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals("Shouldn't add more rates than it already has", 2, TestJob.Charges.Count);

			var charge1 = TestJob.Charges[0];
			var charge2 = TestJob.Charges[1];

			AssertEquals("Should be valid", true, charge1.JR_AC.IsValid);
			AssertEquals("Should be valid", true, charge2.JR_AC.IsValid);

			Assert("Cost Amount is marked as Rated for Rate 1", charge1.JR_CostRated);
			Assert("Sell Amount is marked as Rated for Rate 2", charge2.JR_SellRated);
			Assert("Sell Amount is marked as Rated for Rate 1", charge1.JR_SellRated);
			Assert("Cost Amount is not marked as Rated for Rate 2", !charge2.JR_CostRated);

			AssertEquals(0, charge1.JobChargeAttributes.Count);
			AssertEquals(3, charge2.JobChargeAttributes.Count);
			AssertEquals(JobChargeAttribTypeList.Codes.Commodity, charge2.JobChargeAttributes[0].EC_Name);
			AssertEquals("GEN", charge2.JobChargeAttributes[0].EC_Value);
			AssertEquals(JobChargeAttribTypeList.Codes.LocationType, charge2.JobChargeAttributes[1].EC_Name);
			AssertEquals("AAA", charge2.JobChargeAttributes[1].EC_Value);
			AssertEquals(JobChargeAttribTypeList.Codes.LocationDesc, charge2.JobChargeAttributes[2].EC_Name);
			AssertEquals("LOC1", charge2.JobChargeAttributes[2].EC_Value);
		}

		#endregion

		#region TestAddAutoRates_DisplayInternationalDescriptionForCorrectCountries

		public void TestAddAutoRates_DisplayInternationalDescriptionForCorrectCountries()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			TestJob.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.MainAddress.OA_Address1 = "Some Street";
			debtor.OH_IsDebtor = true;

			Factory.Save();

			TestObjectCreator.Job1.PlugInData = shipment;

			AssertEquals("Precondition", false, AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value);

			AssertAutoRatesDisplayInternationalDescription(shipment, "AUMEL", debtor, false);
			AssertAutoRatesDisplayInternationalDescription(shipment, "CNSHA", debtor, false);

			try
			{
				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				AssertAutoRatesDisplayInternationalDescription(shipment, "CNBJO", debtor, true);
				AssertAutoRatesDisplayInternationalDescription(shipment, "TWCLI", debtor, true);
				AssertAutoRatesDisplayInternationalDescription(shipment, "NZAKL", debtor, true);
				AssertAutoRatesDisplayInternationalDescription(shipment, "AUSYD", debtor, true);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		void AssertAutoRatesDisplayInternationalDescription(ForwardingShipment shipment, string unloco, OrgHeader debtor, bool hasLocalLangDescriptionOnInvoice)
		{
			debtor.OH_RL_NKClosestPort = unloco;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = unloco.Substring(0, 3);
			chargeCode.AC_Desc = "FRT CHRG";
			chargeCode.AC_LocalLanguageDescription = "Freight Charge";

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };

			var rates = new AutoRateInfoCollection(Factory);
			var rate = rates.AddNew(chargeCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100);
			var interactor = new LoggerDecorator();

			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals(1, TestJob.Charges.Count);

			rate.InvoiceLineDescription = "Rate line specific charge description";
			rate.InvoiceLineLocalDescription = "Rate line specific charge description";

			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals(1, TestJob.Charges.Count);

			AssertEquals("Overriden rate line desc is prefered to charge local language and normal descriptions", "Rate line specific charge description", TestJob.Charges[0].JR_Desc);

			try
			{
				TestJob.LocalChargesPK = debtor.PK;
				GlbCompany.CurrentCompany.SetCountry(unloco.Substring(0, 2));
				GlbCompany.CurrentCompany.Factory.Save();

				rate.InvoiceLineDescription = ZString.Empty;
				rate.InvoiceLineLocalDescription = ZString.Empty;

				TestJob.Charges.RemoveAndDeleteAll();
				strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
				AssertEquals(1, TestJob.Charges.Count);

				if (hasLocalLangDescriptionOnInvoice)
				{
					AssertEquals("Invoice Line Description shows local language charge code description", "Freight Charge", TestJob.Charges[0].JR_Desc);
				}
				else
				{
					AssertEquals("Defaults to charge code description", "FRT CHRG", TestJob.Charges[0].JR_Desc);
				}

				rate.InvoiceLineDescription = "Another overriden rate line description";
				rate.InvoiceLineLocalDescription = "Another overriden rate line description";

				TestJob.Charges.RemoveAndDeleteAll();
				strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
				AssertEquals(1, TestJob.Charges.Count);

				AssertEquals("Always prefers the overriden description when there is one", "Another overriden rate line description", TestJob.Charges[0].JR_Desc);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			}
		}

		public void TestAddAutoRates_InternationalDescriptionHasCalculationDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			TestJob.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.MainAddress.OA_Address1 = "Some Street";
			debtor.OH_IsDebtor = true;

			Factory.Save();

			TestObjectCreator.Job1.PlugInData = shipment;

			debtor.OH_RL_NKClosestPort = "UAODS";

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_Desc = "FRT CHRG";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_LocalLanguageDescription = "Freight Charge";

			var rateEntry = Helper.NewClientRate(debtor).AddRateEntryWithFlatRateLine("AIR", "LCL", "AU", "", "FRT", 100m);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria { LocalClient = debtor };
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calcLog = new Rating.Integration.CalculationLog() { BaseRate = 100m };
			var calcOutput = new CalculatorOutput(autoRatingParameters, calcLog);
			var calculationResult = new CalculationResult(rateLine, calcOutput);

			var rates = new AutoRateInfoCollection(Factory);
			var nonLocalRate = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
			nonLocalRate.CalculationDescription = "some additional calculation info";
			rates.Add(nonLocalRate);

			TestJob.LocalChargesPK = debtor.PK;

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			var interactor = new LoggerDecorator();

			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals(1, TestJob.Charges.Count);
			AssertEquals("Invoice Line Description shows general charge code description", "FRT CHRG", TestJob.Charges[0].JR_Desc);

			var item = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			item.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightExRate;
			var value = new InvoiceRollupOrGroupCollection();
			value.Add(item);

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				try
				{
					TestJob.LocalChargesPK = debtor.PK;
					GlbCompany.CurrentCompany.SetCountry("UA");
					GlbCompany.CurrentCompany.Factory.Save();

					TestJob.Charges.RemoveAndDeleteAll();
					rates.RemoveAndDeleteAll();
					var localRate = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
					localRate.CalculationDescription = "some additional calculation info";
					rates.Add(localRate);
					strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
					AssertEquals(1, TestJob.Charges.Count);
					AssertEquals("Invoice Line Description shows local language charge code description", "Freight Charge", TestJob.Charges[0].JR_Desc);

					using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value))
					{
						TestJob.Charges.RemoveAndDeleteAll();
						strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
						AssertEquals(1, TestJob.Charges.Count);
						AssertEquals("Invoice Line Description shows local language charge code description", "Freight Charge - some additional calculation info", TestJob.Charges[0].JR_Desc);

						localRate.InvoiceLineDescription = "Another overriden rate line description";
						localRate.InvoiceLineLocalDescription = "Another overriden rate line local description";

						TestJob.Charges.RemoveAndDeleteAll();
						strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
						AssertEquals(1, TestJob.Charges.Count);

						AssertEquals("Overriden description takes over local language, additional info is still added", "Another overriden rate line local description - some additional calculation info", TestJob.Charges[0].JR_Desc);
					}
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				}
			}
		}

		public void TestAddAutoRates_EmptyCalculationDescriptionNotSet()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			TestJob.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.MainAddress.OA_Address1 = "Some Street";
			debtor.OH_IsDebtor = true;

			Factory.Save();

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			TestObjectCreator.Job1.PlugInData = shipment;

			debtor.OH_RL_NKClosestPort = "UAODS";

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_Desc = "FRT CHRG";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			var rates = new AutoRateInfoCollection(Factory);
			var rate = rates.AddNew(chargeCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100);
			rate.InvoiceLineDescription = "FRT CHRG";
			rate.CalculationDescription = "FRT: ";

			var item = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			item.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightExRate;
			var value = new InvoiceRollupOrGroupCollection();
			value.Add(item);

			using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value))
			{
				TestJob.Charges.RemoveAndDeleteAll();
				strategy.AddAutoRates(new LoggerDecorator(), rates, CostSell.Cost, operationalJobCodes);
				AssertEquals(1, TestJob.Charges.Count);
				AssertEquals("Invoice Line Description shows no dash", "FRT CHRG", TestJob.Charges[0].JR_Desc);
			}
		}

		#endregion

		#region TestAddAutoRates_ForCosting

		public void TestAddAutoRates_ForCosting()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			TestObjectCreator.Job1.PlugInData = shipment;

			OrgHeader provider = Factory.New<OrgHeader>();
			provider.OH_Code = "PROV1";
			provider.CompanyData.OB_APCategory = "ABC"; //just to get the default invoice roll up group created
			provider.OH_IsCreditor = true;
			Factory.Save();

			var interactor = new LoggerDecorator();
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "ABC";

			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "ZZZ";

			var costInfos = new AutoRateInfoCollection(Factory);
			var costInfo1 = costInfos.AddNew(chargeCode1, "USD", 95, operationalJobCodes[0]);
			costInfo1.ProviderPK = provider.PK;
			costInfo1.Description = "Cost was calculated by my brain";

			var costInfo2 = costInfos.AddNew(chargeCode2, "AUD", 183, operationalJobCodes[0]);

			var revenueInfos = new AutoRateInfoCollection(Factory);
			var revenueInfo1 = revenueInfos.AddNew(chargeCode1, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100);
			revenueInfo1.Description = "Who cares about revenue?";

			var revenueInfo2 = revenueInfos.AddNew(chargeCode2, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 200);
			revenueInfo2.Description = "Nothing to discuss so leave me alone";

			AssertEquals(0, TestJob.Charges.Count);

			strategy.AddAutoRates(interactor, costInfos, CostSell.Cost, operationalJobCodes);
			strategy.AddAutoRates(interactor, revenueInfos, CostSell.Revenue, operationalJobCodes);

			AssertEquals(2, TestJob.Charges.Count);
			AssertEquals("Rate 1 - Correct Revenue", 100m, TestJob.Charges[0].JR_LocalSellAmt);
			AssertEquals("Rate 1 - Correct Revenue Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestJob.Charges[0].JR_RX_NKSellCurrency);
			AssertEquals("Rate 1 - Correct Cost", 95m, TestJob.Charges[0].JR_OSCostAmt);
			AssertEquals("Rate 1 - Correct Cost Currency", "USD", TestJob.Charges[0].JR_RX_NKCostCurrency);
			AssertEquals("Rate 1 - Correct Creditor", provider.PK, TestJob.Charges[0].JR_OH_CostAccount);
			AssertEquals("Rate 1 - Correct Cost Desc", "Cost was calculated by my brain", TestJob.Charges[0].CostCalculationDescription.ToAscii());
			AssertEquals("Rate 1 - Correct Revenue Desc", "Who cares about revenue?", TestJob.Charges[0].RevenueCalculationDescription.ToAscii());
			Assert("Cost Amount is marked as Rated", TestJob.Charges[0].JR_CostRated);

			AssertEquals("Rate 2 - Correct Revenue", 200m, TestJob.Charges[1].JR_LocalSellAmt);
			AssertEquals("Rate 2 - Correct Revenue Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestJob.Charges[1].JR_RX_NKSellCurrency);
			AssertEquals("Rate 2 - Correct Cost", 183m, TestJob.Charges[1].JR_LocalCostAmt);
			AssertEquals("Rate 2 - Correct Cost Currency (local as none was specified)", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestJob.Charges[1].JR_RX_NKCostCurrency);
			AssertEquals("Rate 2 - Correct Creditor", ZGuid.Empty, TestJob.Charges[1].JR_OH_CostAccount);
			AssertEquals("Rate 2 - Correct Revenue Desc", "Nothing to discuss so leave me alone", TestJob.Charges[1].RevenueCalculationDescription.ToAscii());
			Assert("Revenue Amount is marked as Rated", TestJob.Charges[1].JR_SellRated);
		}

		#endregion

		#region TestAddAutoRates_WhereCostPosted

		#endregion

		#region TestAddAutoRates_ForCostingForRevenueChargeCode

		public void TestAddAutoRates_ForCostingForRevenueChargeCode()
		{
			#region Setup

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			Factory.Save();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			TestJob.LocalChargesPK = debtor.PK;

			TestObjectCreator.Job1.PlugInData = shipment;

			#endregion

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "ABC";
			chargeCode1.AC_ChargeType = "REV";

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(chargeCode1, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100);

			strategy.AddAutoRates(new LoggerDecorator(), rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals("Rate 1 - Correct Revenue", 100m, TestJob.Charges[0].JR_LocalSellAmt);
			AssertEquals("Rate 1 - Correct Revenue Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestJob.Charges[0].JR_RX_NKSellCurrency);
			AssertEquals("Rate 1 - No cost - this is a revenue charge code", 0m, TestJob.Charges[0].JR_OSCostAmt);
			AssertEquals("Rate 1 - No cost - this is a revenue charge code", 0m, TestJob.Charges[0].JR_LocalCostAmt);
			AssertEquals("Rate 1 - No Cost Currency - this is a revenue charge code", ZString.Empty, TestJob.Charges[0].JR_RX_NKCostCurrency);
		}

		#endregion

		#region TestAddAutoRates_UsesDetailedDescriptionOption

		AutoRateInfoCollection ReloadTestRates(ZString operationalJobCode)
		{
			#region Charge Codes

			var oRG = Factory.New<AccChargeCode>();
			oRG.AC_ChargeGroup = "ORG";
			oRG.AC_Code = "ORG";
			oRG.AC_DepartmentFilterList = "ALL";

			var dST = Factory.New<AccChargeCode>();
			dST.AC_ChargeGroup = "DST";
			dST.AC_Code = "DST";
			dST.AC_DepartmentFilterList = "ALL";

			var fRT = Factory.New<AccChargeCode>();
			fRT.AC_ChargeGroup = "FRT";
			fRT.AC_Code = "FRT";
			fRT.AC_DepartmentFilterList = "ALL";
			fRT.AC_LocalLanguageDescription = "";

			Env.Registry.FreightChargeCode = fRT.PK.ToGuid();

			#endregion

			var testRates = new AutoRateInfoCollection(Factory);

			var rate1 = testRates.AddNew(oRG, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 1, operationalJobCode);
			rate1.InvoiceLineDescription = "rate1";
			rate1.CalculationDescription = "400Kg @ 0.75 / Kg";

			var rate2 = testRates.AddNew(fRT, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 2, operationalJobCode);
			rate2.InvoiceLineDescription = "rate2";
			rate2.CalculationDescription = "200Kg @ 0.81 / Kg";

			var rate3 = testRates.AddNew(dST, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 3, operationalJobCode);
			rate3.InvoiceLineDescription = "rate3";
			rate3.CalculationDescription = "I Am a Goblet and If you don't like it you can go eat it. A secondary charge too";

			return testRates;
		}

		public void TestAddAutoRates_UsesDetailedDescriptionOption()
		{
			var newFactory = new BusinessObjectFactory();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			TestJob.PlugInData = shipment;
			TestJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;
			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			var interactor = new LoggerDecorator();

			#region Local Client / Overseas Agent

			var testLocalClient = newFactory.New<OrgHeader>();
			testLocalClient.OH_Code = "Local1";

			var testOverseasAgent = newFactory.New<OrgHeader>();
			testOverseasAgent.OH_Code = "OverSea1";

			#endregion

			#region No Description if No Org Specified

			testLocalClient.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup localClientGroup = testLocalClient.CompanyData.InvoiceRollupOrGroups.AddNew();
			localClientGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			localClientGroup.PG_InvoiceLineDisplayOption = "";

			testOverseasAgent.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup overseasGroup = testOverseasAgent.CompanyData.InvoiceRollupOrGroups.AddNew();
			overseasGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			overseasGroup.PG_InvoiceLineDisplayOption = "";

			newFactory.Save();

			TestJob.LocalChargesPK = ZGuid.Empty;
			TestJob.AgentCollectPK = ZGuid.Empty;

			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("3 charges", 3, TestJob.Charges.Count);

			AssertEquals("No Desc present for FOB Origin charge", "rate1", TestJob.Charges[0].JR_Desc);
			AssertEquals("No Desc present for Freight charge", "rate2", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			#endregion

			#region Local Client

			#region Local Client - Freight and FOB

			TestJob.LocalChargesPK = testLocalClient.PK;
			TestJob.AgentCollectPK = testOverseasAgent.PK;

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightFOB;
			newFactory.Save();
			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("Correct Debtor", testLocalClient.PK, TestJob.Charges[0].JR_OH_SellAccount);
			AssertEquals("Correct Debtor", testLocalClient.PK, TestJob.Charges[1].JR_OH_SellAccount);
			AssertEquals("Correct Debtor", testLocalClient.PK, TestJob.Charges[2].JR_OH_SellAccount);

			AssertEquals("Desc present for FOB Origin charge", "rate1 - 400Kg @ 0.75 / Kg", TestJob.Charges[0].JR_Desc);

			AssertEquals("Precondition: Rate 2 is a freight charge", "FRT", TestJob.Charges[1].ChargeCode.AC_ChargeGroup);
			AssertEquals("Precondition: Rate 2 is THE freight charge code", Env.Registry.FreightChargeCode, TestJob.Charges[1].ChargeCode.PK.ToGuid());
			Assert("Precondition: Rate 2 charge code has no local lang description", TestJob.Charges[1].ChargeCode.AC_LocalLanguageDescription.IsEmpty);

			AssertEquals("Desc present for Freight charge", "rate2 - 200Kg @ 0.81 / Kg", TestJob.Charges[1].JR_Desc);

			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			#endregion

			#region Local Client - Freight Only

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.Freight;
			newFactory.Save();
			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("No Desc present for FOB Origin charge", "rate1", TestJob.Charges[0].JR_Desc);
			AssertEquals("Desc present for Freight charge", "rate2 - 200Kg @ 0.81 / Kg", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			#endregion

			#region Local Client - All

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			newFactory.Save();
			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("Desc present for FOB Origin charge", "rate1 - 400Kg @ 0.75 / Kg", TestJob.Charges[0].JR_Desc);
			AssertEquals("Desc present for Freight charge", "rate2 - 200Kg @ 0.81 / Kg", TestJob.Charges[1].JR_Desc);
			AssertEquals("Desc present for Destination charge", "rate3 - I Am a Goblet and If you don't like it you can go eat it. A secondary charge too", TestJob.Charges[2].JR_Desc);

			#endregion

			#region Local Client - None

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			newFactory.Save();
			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("No Desc present for FOB Origin charge", "rate1", TestJob.Charges[0].JR_Desc);
			AssertEquals("No Desc present for Freight charge", "rate2", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			#endregion

			#endregion

			#region Overseas Agent - Export FOB Shipment

			#region Blank

			shipment.JS_INCO = "FOB";
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.RL_Code;
			shipment.JS_RL_NKDestination = "INBOM";

			localClientGroup.PG_InvoiceLineDisplayOption = "";
			overseasGroup.PG_InvoiceLineDisplayOption = "";
			newFactory.Save();

			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("3 charges", 3, TestJob.Charges.Count);

			AssertEquals("Correct Debtor", testLocalClient.PK, TestJob.Charges[0].JR_OH_SellAccount);   // origin - local client for export, FOB
			AssertEquals("Correct Debtor", testOverseasAgent.PK, TestJob.Charges[1].JR_OH_SellAccount); // freight - overseas agent for export, FOB
			AssertEquals("Correct Debtor", testOverseasAgent.PK, TestJob.Charges[2].JR_OH_SellAccount); // destination - overseas agent for export, FOB

			AssertEquals("No Desc present for FOB Origin charge", "rate1", TestJob.Charges[0].JR_Desc);
			AssertEquals("No Desc present for Freight charge", "rate2", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			#endregion

			#region Local CLient - Freight and FOB, Overseas Agent - Freight and FOB

			TestJob.LocalChargesPK = testLocalClient.PK;
			TestJob.AgentCollectPK = testOverseasAgent.PK;

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightFOB;
			newFactory.Save();

			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("Desc present for FOB Origin charge", "rate1 - 400Kg @ 0.75 / Kg", TestJob.Charges[0].JR_Desc);
			AssertEquals("No Desc present for Freight charge", "rate2", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightFOB;
			overseasGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightFOB;
			newFactory.Save();

			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("Desc present for FOB Origin charge", "rate1 - 400Kg @ 0.75 / Kg", TestJob.Charges[0].JR_Desc);
			AssertEquals("Desc present for Freight charge", "rate2 - 200Kg @ 0.81 / Kg", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			#endregion

			#region Local Client - Freight, Overseas Agent - Freight and FOB

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.Freight;
			newFactory.Save();
			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("No Desc present for FOB Origin charge", "rate1", TestJob.Charges[0].JR_Desc);
			AssertEquals("Desc present for Freight charge", "rate2 - 200Kg @ 0.81 / Kg", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc present for Destination charge", "rate3", TestJob.Charges[2].JR_Desc);

			#endregion

			#region Local Client - All, Overseas Agent - All

			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			overseasGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			newFactory.Save();
			TestJob.Charges.RemoveAndDeleteAll();
			strategy.AddAutoRates(interactor, ReloadTestRates(operationalJobCode), CostSell.Revenue, operationalJobCodes);

			AssertEquals("Desc present for FOB Origin charge", "rate1 - 400Kg @ 0.75 / Kg", TestJob.Charges[0].JR_Desc);
			AssertEquals("Desc present for Freight charge", "rate2 - 200Kg @ 0.81 / Kg", TestJob.Charges[1].JR_Desc);
			AssertEquals("No Desc as this charge is not paid by any of the parties", "rate3 - I Am a Goblet and If you don't like it you can go eat it. A secondary charge too", TestJob.Charges[2].JR_Desc);

			#endregion

			#endregion
		}

		#endregion

		#region TestAddAutoRates_LocalDesc_ForCost

		public void TestAddAutoRates_LocalDesc_ForCost()
		{
			AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			TestJob.PlugInData = shipment;

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Local1";
			localClient.OH_RL_NKClosestPort = "AUSYD";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "PROV1";
			orgHeader.CompanyData.OB_APCategory = "ABC";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			TestJob.LocalChargesPK = localClient.PK;

			Factory.Save();

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "ABC";
			chargeCode.AC_Desc = "ABC Charge Desc";
			chargeCode.AC_LocalLanguageDescription = "ABC Local Charge Desc";

			var costInfos = new AutoRateInfoCollection(Factory);
			var info = costInfos.AddNew(chargeCode, "AUD", 95);
			info.ProviderPK = orgHeader.PK;
			info.InvoiceLineDescription = "ABC Charge Desc";

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			var interactor = new LoggerDecorator();

			strategy.AddAutoRates(interactor, costInfos, CostSell.Cost, operationalJobCodes);
			AssertEquals(95m, TestJob.Charges[0].JR_LocalCostAmt);
			AssertEquals("ABC Local Charge Desc", TestJob.Charges[0].JR_Desc);
		}

		#endregion

		public void TestAddAutoRates_WhenNoChangeOnChargeAfterAutoRatingRevenue_ThenCalculationLogShouldBeSaved()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestJob.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.MainAddress.OA_Address1 = "Some Street";
			debtor.OH_IsDebtor = true;

			TestObjectCreator.Job1.PlugInData = shipment;

			debtor.OH_RL_NKClosestPort = "UAODS";

			var rateEntry = Helper.NewClientRate(debtor).AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 100m);
			var rateLine = rateEntry.RateLines[0];

			Factory.Save();

			var criteria = new TestRatingCriteria { LocalClient = debtor };
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calcLog = new Rating.Integration.CalculationLog() { BaseRate = 100m };
			var calcOutput = new CalculatorOutput(autoRatingParameters, calcLog);
			var calculationResult = new CalculationResult(rateLine, calcOutput);

			var rates = new AutoRateInfoCollection(Factory);
			var nonLocalRate = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
			rates.Add(nonLocalRate);

			TestJob.LocalChargesPK = debtor.PK;

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			var interactor = new LoggerDecorator();

			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals(1, TestJob.Charges.Count);

			var logsWrapper = CalculationLogsLoader.Load(TestJob.Charges[0]);
			AssertNotNull(logsWrapper);
			AssertEquals("Calculation logs should be saved after auto rating", 100m, logsWrapper.Logs[0].BaseRate);

			Factory.Save();

			calcLog.BaseRate = 200m;
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);
			AssertEquals(1, TestJob.Charges.Count);

			logsWrapper = CalculationLogsLoader.Load(TestJob.Charges[0]);
			AssertNotNull(logsWrapper);
			AssertEquals("Calculation logs should be saved after auto rating even no change on the charge.", 200m, logsWrapper.Logs[0].BaseRate);
		}

		#region TestAddAutoRates_AfterPostedCharges

		AutoRateInfo GetAutoRate(AutoRateInfoCollection rates, AccChargeCode chargeCode, ZString lineDesc)
		{
			AutoRateInfo rate = rates.AddNew(chargeCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 0m);
			rate.InvoiceLineDescription = lineDesc;
			return rate;
		}

		AccChargeCode GetChargeCode(Guid chargeCode)
		{
			AccChargeCode charge = Factory.Load<AccChargeCode>(chargeCode);
			charge.AC_DepartmentFilterList = "ALL";
			charge.AC_GC = GlbCompany.CurrentCompany.PK;
			charge.AC_ChargeType = Core.Constants.ChargeType.Margin;
			return charge;
		}

		#endregion

		#region TestAddAutoRates_CanCreateDuplicateCharge

		public void TestAddAutoRates_CanCreateDuplicateCharge()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			TestJob.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			TestObjectCreator.Job1.PlugInData = shipment;
			TestJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;
			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { operationalJobCode };
			var interactor = new LoggerDecorator();

			var dSB = GetChargeCode(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			var fRT = GetChargeCode(Env.Registry.FreightChargeCode);
			var mRG100 = new TestObjectCreator(new BusinessObjectFactory()).MRG100;

			var rateInfos = new AutoRateInfoCollection(Factory);
			var rateDSB = GetAutoRate(rateInfos, dSB, "rate1");
			var rateFRT = GetAutoRate(rateInfos, fRT, "rate2");

			AssertEquals(0, TestJob.Charges.Count);

			rateDSB.AddFlatPaymentBasis(80, operationalJobCode);
			rateFRT.AddFlatPaymentBasis(185, operationalJobCode);
			strategy.AddAutoRates(interactor, rateInfos, CostSell.Cost, operationalJobCodes);

			rateDSB.Bases.Clear();
			rateFRT.Bases.Clear();

			rateDSB.AddFlatPaymentBasis(100, operationalJobCode);
			rateFRT.AddFlatPaymentBasis(200, operationalJobCode);
			strategy.AddAutoRates(interactor, rateInfos, CostSell.Revenue, operationalJobCodes);

			AssertEquals(2, TestJob.Charges.Count);

			rateInfos = new AutoRateInfoCollection(Factory);
			var rateMRG100 = GetAutoRate(rateInfos, mRG100, "rate3");
			rateMRG100.AddFlatPaymentBasis(385, operationalJobCode);

			strategy.AddAutoRates(interactor, rateInfos, CostSell.Cost, operationalJobCodes);

			rateMRG100.Bases.Clear();
			rateMRG100.AddFlatPaymentBasis(400, operationalJobCode);
			strategy.AddAutoRates(interactor, rateInfos, CostSell.Revenue, operationalJobCodes);

			AssertEquals("3 charges present - 1 new charges added", 3, TestJob.Charges.Count);

			foreach (Charge chrg in TestJob.Charges)
			{
				if (chrg.ChargeCode != null && chrg.ChargeCode.AC_Code == fRT.AC_Code)
				{
					AssertEquals("Correct FRT Sell Amount", 200m, chrg.JR_LocalSellAmt);
					AssertEquals("Correct FRT Cost Amount", 185m, chrg.JR_LocalCostAmt);
				}

				if (chrg.ChargeCode != null && chrg.ChargeCode.AC_Code == dSB.AC_Code)
				{
					AssertEquals("Correct DSB Sell Amount", 100m, chrg.JR_LocalSellAmt);
					AssertEquals("Correct DSB Cost Amount", 80m, chrg.JR_LocalCostAmt);
				}

				if (chrg.ChargeCode != null && chrg.ChargeCode.AC_Code == mRG100.AC_Code)
				{
					AssertEquals("MRG100 Sell Amount", 400m, chrg.JR_LocalSellAmt);
					AssertEquals("MRG100 Cost Amount", 385m, chrg.JR_LocalCostAmt);
				}
			}
		}

		#endregion

		#region TestAddAutoRates_UsesIncoTerms

		public void TestAddAutoRates_UsesIncoTerms()
		{
			#region Setup

			var localPort = "AUBTB";

			var consignee = TestObjectCreator.CreateOrgHeader("AAA", false, false);
			consignee.OH_IsConsignee = true;

			var consignor = TestObjectCreator.CreateOrgHeader("BBB", false, false);
			consignor.OH_IsConsignor = true;

			var debtor = TestObjectCreator.CreateOrgHeader("CCC", false, true);
			var agent = TestObjectCreator.CreateOrgHeader("DDD", false, true);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKDestination = "USMEM";
			shipment.JS_RL_NKOrigin = localPort;
			shipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;
			Factory.Save();

			GlbDepartment dept = Factory.New<GlbDepartment>();

			// Setup 3 charge codes
			AccChargeCode oRG = Factory.New<AccChargeCode>();
			oRG.AC_ChargeGroup = "ORG";
			oRG.AC_Code = "ORG";
			oRG.AC_DepartmentFilterList = "ALL";
			oRG.AC_GC = GlbCompany.CurrentCompany.PK;

			AccChargeCode dST = Factory.New<AccChargeCode>();
			dST.AC_ChargeGroup = "DST";
			dST.AC_Code = "DST";
			dST.AC_DepartmentFilterList = "ALL";
			dST.AC_GC = GlbCompany.CurrentCompany.PK;

			AccChargeCode fRT = Factory.New<AccChargeCode>();
			fRT.AC_ChargeGroup = "FRT";
			fRT.AC_Code = "FRT";
			fRT.AC_DepartmentFilterList = "ALL";
			fRT.AC_GC = GlbCompany.CurrentCompany.PK;

			// Setup Autorating
			var rates = new AutoRateInfoCollection(Factory);

			AutoRateInfo rateOrg = rates.AddNew(oRG, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100);
			AutoRateInfo rateDst = rates.AddNew(dST, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 200);
			AutoRateInfo rateFrt = rates.AddNew(fRT, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 300);

			var interactor = new LoggerDecorator();

			#endregion

			#region Export - ExWorks

			AssertEquals("Shipment should be defined as an Export. Current Company Closest port is " + GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort + ". Current Branch country code is " + GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + ".", false, shipment.IsImport());

			TestJob.PlugInData = shipment;
			TestJob.JH_GE = dept.PK;
			TestJob.LocalChargesPK = debtor.PK;
			TestJob.AgentCollectPK = agent.PK;

			TestJob.Charges.RemoveAll();

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };

			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("No charges excluded", 3, TestJob.Charges.Count);

			AssertEquals("FRT charge assigned to Agent (2nd party)", agent.PK, ChargeSellAccount(TestJob.Charges, rateFrt));
			AssertEquals("ORG charge assigned to Agent (2nd party)", agent.PK, ChargeSellAccount(TestJob.Charges, rateOrg));
			AssertEquals("DST charge assigned to Agent (2nd party)", agent.PK, ChargeSellAccount(TestJob.Charges, rateDst));

			#endregion

			#region Export - FOB

			shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = localPort;
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;

			AssertEquals("Shipment should be defined as an Export. Current Company Closest port is " + GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort + ". Current Branch country code is " + GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + ".", false, shipment.IsImport());

			TestJob.PlugInData = shipment;

			TestJob.Charges.RemoveAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("No charges excluded", 3, TestJob.Charges.Count);

			AssertEquals("FRT charge assigned to Agent (2nd party)", agent.PK, ChargeSellAccount(TestJob.Charges, rateFrt));
			AssertEquals("ORG charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateOrg));
			AssertEquals("DST charge assigned to Agent (2nd party)", agent.PK, ChargeSellAccount(TestJob.Charges, rateDst));

			#endregion

			#region Export - DDP

			shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_RL_NKOrigin = localPort;
			shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;

			AssertEquals("Shipment should be defined as an Export. Current Company Closest port is " + GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort + ". Current Branch country code is " + GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + ".", false, shipment.IsImport());

			TestJob.PlugInData = shipment;

			TestJob.Charges.RemoveAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("No charges excluded", 3, TestJob.Charges.Count);

			AssertEquals("FRT charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateFrt));
			AssertEquals("ORG charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateOrg));
			AssertEquals("DST charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateDst));

			#endregion

			#region Import - ExWorks

			shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKDestination = localPort;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;

			AssertEquals("Shipment should be defined as an Export. Current Company Closest port is " + GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort + ". Current Branch country code is " + GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + ".", true, shipment.IsImport());

			TestJob.PlugInData = shipment;

			TestJob.Charges.RemoveAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("No charges excluded", 3, TestJob.Charges.Count);

			AssertEquals("FRT charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateFrt));
			AssertEquals("ORG charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateOrg));
			AssertEquals("DST charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateDst));

			#endregion

			#region Import - FOB

			shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKDestination = localPort;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;

			AssertEquals("Shipment should be defined as an Export. Current Company Closest port is " + GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort + ". Current Branch country code is " + GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + ".", true, shipment.IsImport());

			TestJob.PlugInData = shipment;

			TestJob.Charges.RemoveAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("FRT and DST assigned to local debtor", 3, TestJob.Charges.Count);

			AssertEquals("FRT charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateFrt));
			AssertEquals("ORG charge assigned to Agent", agent.PK, ChargeSellAccount(TestJob.Charges, rateOrg));
			AssertEquals("DST charge assigned to Local Client", debtor.PK, ChargeSellAccount(TestJob.Charges, rateDst));

			#endregion

			#region Import - DDP

			shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKDestination = localPort;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;

			AssertEquals("Shipment should be defined as Import. Current Company Closest port is " + GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort + ". Current Branch country code is " + GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + ".", true, shipment.IsImport());

			TestJob.PlugInData = shipment;

			TestJob.Charges.RemoveAll();
			strategy.AddAutoRates(interactor, rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("No charges excluded", 3, TestJob.Charges.Count);

			AssertEquals("FRT charge assigned to Agent", agent.PK, ChargeSellAccount(TestJob.Charges, rateFrt));
			AssertEquals("ORG charge assigned to Agent", agent.PK, ChargeSellAccount(TestJob.Charges, rateOrg));
			AssertEquals("DST charge assigned to Agent", agent.PK, ChargeSellAccount(TestJob.Charges, rateDst));

			#endregion
		}

		/// <summary>
		/// Checks a given Charges collection for a specific charge based on a Rate.
		/// Returns the Sell Account of that charge.
		/// </summary>
		/// <param name="charges">Charges on the job.</param>
		/// <param name="rate">The Rate who's Charge Code we want to check for</param>
		/// <returns>Sell Account (ie, the Debtor PK) for that specific charge.</returns>
		ZGuid ChargeSellAccount(ChargeCollection charges, AutoRateInfo rate)
		{
			ZGuid result = ZGuid.Empty;
			Charge[] foundCharges = (Charge[])charges.Find(new ZQuery(JobChargeSchema.JR_AC, SQLComparisonOperator.Equal, rate.ChargeCode.PK));

			if (foundCharges != null && foundCharges.Length > 0)
			{
				result = foundCharges[0].JR_OH_SellAccount;
			}

			return result;
		}

		#endregion

		#region TestAddAutoRates_WrongChargeCodes

		public void TestAddAutoRates_WrongChargeCodes()
		{
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			var testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.LocalChargesPK = debtor.PK;
			testJob.PlugInData = shipment;

			testJob.JH_GE = TestObjectCreator.FIADepartment.PK;
			AccChargeCode dSB = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			dSB.AC_DepartmentFilterList = "rubbish";
			dSB.AC_GC = GlbCompany.CurrentCompany.PK;
			AccChargeCode fRT = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			fRT.AC_DepartmentFilterList = "ALL";
			fRT.AC_GC = TestObjectCreator.NonCurrentBranch.PK;

			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;
			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(dSB, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100, operationalJobCode);
			var rate2 = rates.AddNew(fRT, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 200, operationalJobCode);

			AssertEquals(0, testJob.Charges.Count);

			var strategy = new AutoRateInvoicingStrategy(shipment, testJob);
			strategy.AddAutoRates(new LoggerDecorator(), rates, CostSell.Revenue, new[] { operationalJobCode });

			AssertEquals("Charge count", 2, testJob.Charges.Count);
			AssertNoErrors("Should not be in error", testJob.Charges[0].JR_ACInfo);
			AssertHasErrors("Should be in error", testJob.Charges[1].JR_ACInfo);

			ErrorReporter.Clear();
		}

		#endregion

		#region TestAddAutoRates_OverrideAmount

		public void TestAddAutoRates_OverrideAmount()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			TestJob.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			TestObjectCreator.Job1.PlugInData = shipment;

			TestJob.JH_GE = TestObjectCreator.FIADepartment.PK;
			AccChargeCode dSB = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			dSB.AC_DepartmentFilterList = "ALL";
			dSB.AC_GC = GlbCompany.CurrentCompany.PK;
			AccChargeCode fRT = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			fRT.AC_DepartmentFilterList = "ALL";
			fRT.AC_GC = GlbCompany.CurrentCompany.PK;

			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;
			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(dSB, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 100, operationalJobCode);
			var rate2 = rates.AddNew(fRT, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 200, operationalJobCode);

			Charge fRTCharge = TestJob.Charges.AddNew();
			fRTCharge.JR_AC = fRT.PK;
			fRTCharge.JR_OSSellAmt = 10;
			fRTCharge.JR_SellRatingOverride = false;
			fRTCharge.JR_AL_ARLine = ZGuid.Empty;
			fRTCharge.AddPaymentBases(new[] { TestObjectCreator.CreatePaymentBasis(10, operationalJobCode) }, false);
			AssertEquals(1, TestJob.Charges.Count);
			AssertEquals(JobChargeLookups.ReAutorateCharge, TestJob.Charges[0].JR_Calc_SellRatingBehavior);

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			strategy.AddAutoRates(new LoggerDecorator(), rates, CostSell.Revenue, operationalJobCodes);

			AssertEquals(2, TestJob.Charges.Count);
			AssertEquals("Sell Amt must be overriden if revenue is not posted", 200m, fRTCharge.JR_OSSellAmt);
			AssertEquals("Should be valid", true, TestJob.Charges[0].JR_AC.IsValid);
			AssertEquals("Should be valid", true, TestJob.Charges[1].JR_AC.IsValid);
		}

		#endregion

		#region TestAddAutoRates_WithOverrideRatingFlag

		public void TestAddAutoRates_WithOverrideRatingFlag()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;
			var operationalJobCodes = new[] { operationalJobCode };
			var interactor = new LoggerDecorator();

			TestJob.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			TestObjectCreator.Job1.PlugInData = shipment;
			TestJob.JH_GE = TestObjectCreator.FIADepartment.PK;

			var dSB = GetChargeCode(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			var fRT = GetChargeCode(Env.Registry.FreightChargeCode);

			var fRTCharge = GetCharge(TestJob, fRT, false, 11, true, 22, operationalJobCode);
			var dSBCharge = GetCharge(TestJob, dSB, true, 33, false, 33, operationalJobCode);

			var costRates = new AutoRateInfoCollection(Factory);

			var fRTRateCost = GetRateInfo(costRates, fRT, true, 0m, operationalJobCode);
			var dSBRateCost = GetRateInfo(costRates, dSB, false, 350m, operationalJobCode);

			var revenueRates = new AutoRateInfoCollection(Factory);

			var fRTRateSell = GetRateInfo(revenueRates, fRT, false, 250m, operationalJobCode);
			var dSBRateSell = GetRateInfo(revenueRates, dSB, false, 0m, operationalJobCode);

			strategy.AddAutoRates(interactor, costRates, CostSell.Cost, operationalJobCodes);
			strategy.AddAutoRates(interactor, revenueRates, CostSell.Revenue, operationalJobCodes);

			CombineAssertions(() =>
			{
				AssertEquals("FRT CostRatingOverride is NOT checked", false, fRTCharge.JR_CostRatingOverride);
				AssertEquals("FRT Cost Amount is marked as Rated", true, fRTCharge.JR_CostRated);
				AssertEquals("FRT Cost Amount must be overriden", 0m, fRTCharge.JR_LocalCostAmt);

				AssertEquals("FRT SellRatingOverride is checked", true, fRTCharge.JR_SellRatingOverride);
				AssertEquals("FRT Sell Amount is marked as NOT Rated", false, fRTCharge.JR_SellRated);
				AssertEquals("FRT Sell Amount must NOT be overriden", 22m, fRTCharge.JR_LocalSellAmt);

				AssertEquals("DSB CostRatingOverride is checked", true, dSBCharge.JR_CostRatingOverride);
				AssertEquals("DSB Cost Amount is marked as NOT Rated", false, dSBCharge.JR_CostRated);
				AssertEquals("DSB Cost Amount must NOT be overriden", 33m, dSBCharge.JR_LocalCostAmt);

				AssertEquals("DSB SellRatingOverride is NOT checked", false, dSBCharge.JR_SellRatingOverride);
				AssertEquals("DSB Sell Amount is marked as NOT Rated", false, dSBCharge.JR_SellRated);
				AssertEquals("DSB Sell Amount must be overriden", 33m, dSBCharge.JR_LocalSellAmt);

				AssertEquals("Two more charges are added", 4, TestJob.Charges.Count);
			});

			var dsb2Charge = TestJob.Charges.Where(x => x.ChargeCode == dSB).Last();
			var frt2Charge = TestJob.Charges.Where(x => x.ChargeCode == fRT).Last();

			CombineAssertions(() =>
			{
				AssertEquals("FRT Cost was rated", 250m, frt2Charge.JR_LocalCostAmt);
				AssertEquals("FRT Sell was obtained from margin value", 250m, frt2Charge.JR_LocalSellAmt);
				AssertEquals("DSB Cost was obtained from margin value", 350m, dsb2Charge.JR_LocalCostAmt);
				AssertEquals("DSB Sell was rated", 350m, dsb2Charge.JR_LocalSellAmt);
			});

			fRTCharge.JR_CostRatingOverride = true;
			fRTCharge.JR_LocalCostAmt = 1000m;
			fRTCharge.JR_SellRatingOverride = false;
			fRTCharge.AddPaymentBases(new[] { TestObjectCreator.CreatePaymentBasis(1000, operationalJobCode) }, false);

			dSBCharge.JR_CostRatingOverride = false;
			dSBCharge.JR_SellRatingOverride = true;
			dSBCharge.JR_LocalSellAmt = 2000m;
			dSBCharge.AddPaymentBases(new[] { TestObjectCreator.CreatePaymentBasis(2000, operationalJobCode) }, false);

			strategy.AddAutoRates(interactor, costRates, CostSell.Cost, operationalJobCodes);
			strategy.AddAutoRates(interactor, revenueRates, CostSell.Revenue, operationalJobCodes);

			CombineAssertions(() =>
			{
				AssertEquals("FRT CostRatingOverride is checked", true, fRTCharge.JR_CostRatingOverride);
				AssertEquals("FRT Cost Amount is marked as Rated", true, fRTCharge.JR_CostRated);
				AssertEquals("FRT Cost Amount must NOT be overriden", 1000m, fRTCharge.JR_LocalCostAmt);

				AssertEquals("FRT SellRatingOverride is NOT checked", false, fRTCharge.JR_SellRatingOverride);
				AssertEquals("FRT Sell Amount is marked as Rated", true, fRTCharge.JR_SellRated);
				AssertEquals("FRT Sell Amount must be overriden", 250m, fRTCharge.JR_LocalSellAmt);

				AssertEquals("DSB CostRatingOverride is NOT checked", false, dSBCharge.JR_CostRatingOverride);
				AssertEquals("DSB Cost Amount is marked as Rated", true, dSBCharge.JR_CostRated);
				AssertEquals("DSB Cost Amount must be overriden", 350m, dSBCharge.JR_LocalCostAmt);

				AssertEquals("DSB SellRatingOverride is checked", true, dSBCharge.JR_SellRatingOverride);
				AssertEquals("DSB Sell Amount is marked as NOT Rated", false, dSBCharge.JR_SellRated);
				AssertEquals("DSB Sell Amount must be NOT overriden", 2000m, dSBCharge.JR_LocalSellAmt);

				AssertEquals("Two more charges are added", 4, TestJob.Charges.Count);
			});
		}

		Charge GetCharge(Job job, AccChargeCode chargeCode, ZBool costOverride, ZDecimal costAmount, ZBool sellOverride, ZDecimal sellAmount, ZString operationalJobCode)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_LocalCostAmt = costAmount;
			charge.JR_CostRatingOverride = costOverride;
			charge.JR_LocalSellAmt = sellAmount;
			charge.JR_SellRatingOverride = sellOverride;

			if (!costOverride)
			{
				charge.AddPaymentBases(new[] { TestObjectCreator.CreatePaymentBasis(costAmount, operationalJobCode) }, true);
			}

			if (!sellOverride)
			{
				charge.AddPaymentBases(new[] { TestObjectCreator.CreatePaymentBasis(costAmount, operationalJobCode) }, false);
			}

			return charge;
		}

		AutoRateInfo GetRateInfo(AutoRateInfoCollection rates, AccChargeCode chargeCode, ZBool hasExplicitZeroAmount, ZDecimal amount, ZString operationalJobCode)
		{
			var rateInfo = rates.AddNew(chargeCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, amount, operationalJobCode);
			if (hasExplicitZeroAmount)
			{
				rateInfo.CalculationDescription = "desc";
			}

			return rateInfo;
		}

		#endregion

		#region TestAddAutoRates_WithApportionedCost

		public void TestAddAutoRates_WithApportionedCost()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			Factory.Save();

			var interactor = new LoggerDecorator();
			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;
			var operationalJobCodes = new[] { operationalJobCode };

			TestJob.LocalChargesPK = TestObjectCreator.LocalClient.PK;

			TestObjectCreator.Job1.PlugInData = shipment;
			TestJob.JH_GE = TestObjectCreator.FIADepartment.PK;

			var dSB = GetChargeCode(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			var fRT = GetChargeCode(Env.Registry.FreightChargeCode);

			var fRTConsolCost = Factory.New<JobConsolCost>();

			var fRTCharge = GetCharge(TestJob, fRT, false, 11, true, 22, operationalJobCode);
			fRTCharge.JR_E6 = fRTConsolCost.PK;
			AssertEquals("JR_IsApportioned", true, fRTCharge.JR_IsApportioned);

			var dSBCharge = GetCharge(TestJob, dSB, true, 33, false, 33, operationalJobCode);

			var revenueRates = new AutoRateInfoCollection(Factory);
			var fRTRateSell = GetRateInfo(revenueRates, fRT, false, 250m, operationalJobCode);
			var dSBRateSell = GetRateInfo(revenueRates, dSB, false, 0m, operationalJobCode);

			var costRates = new AutoRateInfoCollection(Factory);
			var fRTRateCost = GetRateInfo(costRates, fRT, true, 0m, operationalJobCode);
			var dSBRateCost = GetRateInfo(costRates, dSB, false, 350m, operationalJobCode);

			strategy.AddAutoRates(interactor, costRates, CostSell.Cost, operationalJobCodes);
			strategy.AddAutoRates(interactor, revenueRates, CostSell.Revenue, operationalJobCodes);

			AssertEquals("CostRatingOverride is NOT checked", false, fRTCharge.JR_CostRatingOverride);
			AssertEquals("Cost Amount is marked as Rated", false, fRTCharge.JR_CostRated);
			AssertEquals("Cost Amount should not be overriden", 11m, fRTCharge.JR_LocalCostAmt);

			AssertEquals("SellRatingOverride is checked", true, fRTCharge.JR_SellRatingOverride);
			AssertEquals("Sell Amount is marked as NOT Rated", false, fRTCharge.JR_SellRated);
			AssertEquals("Sell Amount must NOT be overriden", 22m, fRTCharge.JR_LocalSellAmt);

			AssertEquals("CostRatingOverride is checked", true, dSBCharge.JR_CostRatingOverride);
			AssertEquals("Cost Amount is marked as NOT Rated", false, dSBCharge.JR_CostRated);
			AssertEquals("Cost Amount must NOT be overriden", 33m, dSBCharge.JR_LocalCostAmt);

			AssertEquals("SellRatingOverride is NOT checked", false, dSBCharge.JR_SellRatingOverride);
			AssertEquals("Sell Amount is marked as NOT Rated", false, dSBCharge.JR_SellRated);
			AssertEquals("Sell Amount must be overriden", 33m, dSBCharge.JR_LocalSellAmt);

			AssertEquals("Two more charges are added", 4, TestJob.Charges.Count);
		}

		#endregion

		public void TestAddAutoRates_CalculationLogs()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			TestObjectCreator.Job1.PlugInData = shipment;

			var strategy = new AutoRateInvoicingStrategy(shipment, TestJob);
			var operationalJobCodes = new[] { shipment.RatingAdapter.OperationalJobCode };
			var interactor = new LoggerDecorator();

			var rateInfo1 = new AutoRateInfo(Factory);
			rateInfo1.ChargeCode = Factory.New<AccChargeCode>();
			rateInfo1.Currency = "AUD";
			rateInfo1.AddFlatPaymentBasis(10m, operationalJobCodes[0], "AUD");

			var entry = Factory.New<ClientRate>().EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.AddNew();
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_AC = Env.Registry.FreightChargeCode;
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			var calcLog = new Rating.Integration.CalculationLog();
			calcLog.BaseRate = 100m;

			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());
			var calcOutput = new CalculatorOutput(autoRatingParameters, calcLog);
			var calculationResult = new CalculationResult(rateLine, calcOutput);

			var rateInfo2 = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
			rateInfo2.ChargeCode = Factory.New<AccChargeCode>();
			rateInfo2.Currency = "AUD";
			rateInfo2.AddFlatPaymentBasis(20m, operationalJobCodes[0], "AUD");

			var rateInfoCollection = new AutoRateInfoCollection(Factory);
			rateInfoCollection.Add(rateInfo1);
			rateInfoCollection.Add(rateInfo2);

			AssertEquals("Precondition", 0, TestJob.Charges.Count);

			strategy.AddAutoRates(interactor, rateInfoCollection, CostSell.Revenue, operationalJobCodes);
			AssertEquals(2, TestJob.Charges.Count);

			var logsWrapper = CalculationLogsLoader.Load(TestJob.Charges[0]);
			AssertNull("No calculation logs", logsWrapper);

			logsWrapper = CalculationLogsLoader.Load(TestJob.Charges[1]);
			AssertNotNull("Calculation logs added", logsWrapper);
			AssertEquals(100m, logsWrapper.Logs[0].BaseRate);

			TestJob.Charges[1].JR_OSSellAmt = 300m;
			calcLog.BaseRate = 101m;
			strategy.AddAutoRates(interactor, rateInfoCollection, CostSell.Revenue, operationalJobCodes);
			logsWrapper = CalculationLogsLoader.Load(TestJob.Charges[1]);
			AssertNotEquals("The baserate should not be saved.", 101m, logsWrapper.Logs[0].BaseRate);
		}

		#endregion

		protected OrgHeader CreateForwarder()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_Code = "TEST5";
			result.OH_IsDebtor = false;
			result.OH_IsConsignor = false;
			result.OH_IsConsignee = false;
			result.OH_IsBroker = false;
			result.OH_IsForwarder = true;
			return result;
		}

		AccChargeCode fDSBCharge;
		AccChargeCode DSBCharge
		{
			get
			{
				if (fDSBCharge == null)
				{
					fDSBCharge = Factory.New<AccChargeCode>();
					fDSBCharge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
					fDSBCharge.AC_Code = "ZUB11";
					fDSBCharge.AC_Desc = "RAKHSH CHARGE";
				}
				return fDSBCharge;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestGuid = Factory.NewWithValidTestData<OrgHeader>().PK;
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}
	}
}
