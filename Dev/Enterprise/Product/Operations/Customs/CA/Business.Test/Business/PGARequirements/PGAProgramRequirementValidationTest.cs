using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGAProgramRequirementValidationTest : TestCaseWithFactory
	{
		public void TestCheckDeclareYes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "2403991010";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.HC, pgaProvider);

			var programRequirement = (PGAProgramRequirement)pgaRequirement.ProgramCodeRequirements.First();
			programRequirement.DeclareYes = ZBool.True;

			AssertEquals(YesNoList.Codes.Yes, pgaProvider.GetIndicatorInfo(PGACodes.Codes.HC).Value);

			programRequirement.Validation.CheckDeclareYes(programRequirement.DeclareYesInfo);

			var expected = "The Tariff does not indicate that this program reporting is required.";
			AssertHasWarningContaining(programRequirement.DeclareYesInfo, expected);

			programRequirement.DeclareYes = ZBool.False;
			programRequirement.Validation.CheckDeclareYes(programRequirement.DeclareYesInfo);
			AssertNoWarningContaining(programRequirement.DeclareYesInfo, expected);
		}

		public void TestCheckDeclareYes_CFIA()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "9705000000");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "2403991010";
			var expected = "The Tariff does not indicate that this program reporting is required.";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.CFIA, pgaProvider);

			var programRequirement = (PGAProgramRequirement)pgaRequirement.ProgramCodeRequirements.First();
			programRequirement.DeclareYes = true;

			AssertHasWarningContaining(programRequirement.DeclareYesInfo, expected);
			AssertHasMessageError(programRequirement.DeclareYesInfo, "No CFIA data has been entered. If CFIA does not apply to this invoice line, please tick 'No' for all CFIA programs.");

			programRequirement.Indicator = ZString.Empty;

			invoiceLine.JI_Tariff = "9705000000";

			Assert(programRequirement.DeclareYes);
			AssertNoWarningContaining(programRequirement.DeclareYesInfo, expected);

			var header = invoiceLine.CFIAPGAHeader;
			header.CA_AIRSEndUse = "xx";
			programRequirement.Validation.CheckDeclareYes(programRequirement.DeclareYesInfo);
			AssertNoMessageError(programRequirement.DeclareYesInfo, "No CFIA data has been entered. If CFIA does not apply to this invoice line, please tick 'No' for all CFIA programs.");
		}

		public void TestCheckCFIAAccountNumberWarningOnCFIA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CACustomsDataRegistry.Instance.DefaultCFIAFeePaymentMethod.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, CFIAPaymentMethods.Codes.Broker);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var proxy = Factory.New<OrgHeader>();
			declaration.Branch.GB_OH_OrgProxy = proxy.PK;
			OrgImpAddInfo.Get(importer).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Broker;
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode = "WW";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.CFIA, pgaProvider);

			var programRequirement = (PGAProgramRequirement)pgaRequirement.ProgramCodeRequirements.First();
			programRequirement.DeclareYes = true;
			programRequirement.Validation.CheckDeclareYes(programRequirement.DeclareYesInfo);
			AssertHasWarning(programRequirement.DeclareYesInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);

			programRequirement.DeclareYes = false;
			pgaRequirement.Validation.ValidatePGARequired();
			AssertNoWarning(programRequirement.DeclareYesInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
		}

		public void TestCheckDeclareNo_CFIA()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "9705000000");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9705000000";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.CFIA, pgaProvider);
			var programRequirement = (PGAProgramRequirement)pgaRequirement.ProgramCodeRequirements.First();

			Assert(programRequirement.DeclareYes);

			programRequirement.DeclareYes = false;

			var expected = "The Classification number may require CFIA: All Programs for CFIA. Tick Yes if CFIA: All Programs for CFIA applies, otherwise leave as No.";
			AssertHasWarningContaining(programRequirement.DeclareNoInfo, expected);
		}

		public void TestTariffFlaggedForProgram()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "ECCC", "6005902100", "WEN");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "6005902100";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.ECCC, pgaProvider);

			var programRequirement = pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().First(x => x.ProgramCode == "WEN");
			programRequirement.Indicator = string.Empty;
			programRequirement.Validation.CheckDeclareNotApplicable(programRequirement.DeclareNotApplicableInfo);
			programRequirement.Validation.CheckDeclareNo(programRequirement.DeclareNoInfo);

			var naMessageError = "The classification is flagged for this PGA and Program. Tick Yes to declare the PGA data or tick No to indicate that the PGA and Program do not apply in this case.";
			var noWarning = "The Classification number may require ECCC: Wildlife Enforcement. Tick Yes if ECCC: Wildlife Enforcement applies, otherwise leave as No.";
			AssertHasMessageError(programRequirement.DeclareNotApplicableInfo, naMessageError);
			AssertNoWarning(programRequirement.DeclareNoInfo, noWarning);

			programRequirement.DeclareNo = ZBool.True;
			programRequirement.DeclareNotApplicableInfo.ClearAllNotifications();
			programRequirement.Validation.CheckDeclareNotApplicable(programRequirement.DeclareNotApplicableInfo);
			programRequirement.Validation.CheckDeclareNo(programRequirement.DeclareNoInfo);
			AssertNoMessageError(programRequirement.DeclareNotApplicableInfo, naMessageError);
			AssertHasWarning(programRequirement.DeclareNoInfo, noWarning);
		}

		public void TestCheckRequiredLPCOs()
		{
			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.HC, pgaProvider);

			var apiRequirement = pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == HCPGADepartmentCodes.Codes.API);
			apiRequirement.DeclareYes = ZBool.True;
			var hcPGAHeader = invoiceLine.HCPGAHeader;
			hcPGAHeader.CA_IntendedUseCodeAPI = HCIntendedUseCode.Codes.HC13;
			hcPGAHeader.CA_CategoryAPI = HCCategories.Codes.HC01;
			hcPGAHeader.LPCOViews.RemoveAndDeleteAll();

			apiRequirement.Validation.CheckDeclareYes(apiRequirement.DeclareYesInfo);
			AssertHasMessageError("5001 is mandatory", apiRequirement.DeclareYesInfo, "LPCO(s): [5001: Establishment Licence (EL)] should be added for program: Active Pharmaceutical Ingredients.");

			hcPGAHeader.LPCOViews.AddNew().CLP_Type = "5001";

			apiRequirement.Validation.CheckDeclareYes(apiRequirement.DeclareYesInfo);
			AssertNoMessageError(apiRequirement.DeclareYesInfo, "LPCO(s): [5001: Establishment Licence (EL)] should be added for program: Active Pharmaceutical Ingredients.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
