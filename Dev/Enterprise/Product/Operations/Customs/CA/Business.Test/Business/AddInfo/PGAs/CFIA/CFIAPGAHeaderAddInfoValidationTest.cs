using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CFIAPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_AIRSEndUse()
		{
			cfiaPGAHeader.CA_AIRSExtensionCode = "00001";
			cfiaPGAHeader.CA_AIRSEndUse = "";
			AssertNoMessageErrors(cfiaPGAHeader.CA_AIRSEndUseInfo);
			cfiaPGAHeader.CA_AIRSEndUse = "XX";
			cfiaPGAHeader.AddInfoValidation.ValidateCA_AIRSEndUse();
			ValidationTestHelper.AssertInvalidCodeMessageError(cfiaPGAHeader.CA_AIRSEndUseInfo, "XX", "01");
		}

		public void TestCheckCFIAAccountNumberWarning()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			CACustomsDataRegistry.Instance.DefaultCFIAFeePaymentMethod.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, CFIAPaymentMethods.Codes.Broker);
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var proxy = Factory.New<OrgHeader>();
			declaration.Branch.GB_OH_OrgProxy = proxy.PK;
			OrgImpAddInfo.Get(importer).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Broker;
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;
			invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode = "WW";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.CFIA, pgaProvider);

			var programRequirement = (PGAProgramRequirement)pgaRequirement.ProgramCodeRequirements.First();
			programRequirement.DeclareYes = true;

			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasWarning(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
			programRequirement.Validation.CheckDeclareYes(programRequirement.DeclareYesInfo);
			AssertHasWarning(programRequirement.DeclareYesInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);

			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.No;
			programRequirement.DeclareYes = false;
			pgaRequirement.Validation.ValidatePGARequired();
			AssertHasWarning(declaration.JE_OH_ImporterInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
			AssertNoWarning(programRequirement.DeclareYesInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
		}

		public void TestCheckCA_AIRSMiscellaneous()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CFIAM", "CFIA Misc Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CFIAM", "02", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeMessageError(cfiaPGAHeader.CA_AIRSMiscellaneousInfo, "XX", "02");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;

			cfiaPGAHeader = invoiceLine.CFIAPGAHeader;
		}
		CFIAPGAHeader cfiaPGAHeader;

		#endregion
	}
}
