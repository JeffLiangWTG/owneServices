using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CFIAPGAHeaderAddInfo))]
	sealed class CFIAPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CFIAPGAHeaderAddInfo(Factory.NewWithValidTestData<CFIAPGAHeader>().B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var importer = Factory.New<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			CACustomsDataRegistry.Instance.DefaultCFIAFeePaymentMethod.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, CFIAPaymentMethods.Codes.Broker);
			declaration.JE_OH_Importer = importer.PK;

			var proxy = Factory.New<OrgHeader>();
			declaration.Branch.GB_OH_OrgProxy = proxy.PK;
			OrgImpAddInfo.Get(importer).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Broker;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode = "WW";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.CFIA, pgaProvider);

			var programRequirement = (PGAProgramRequirement)pgaRequirement.ProgramCodeRequirements.First();
			programRequirement.DeclareYes = true;

			var cfia = invoiceLine.CFIAPGAHeader;
			cfia.CA_AllProgramInd = YesNoList.Codes.Yes;
			cfia.RunPreSaveValidation();
			AssertHasWarning(programRequirement.DeclareYesInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			CACustomsDataRegistry.Instance.DefaultCFIAFeePaymentMethod.SetValue(Guid.Empty, declaration2.RegistryBranchPK, Guid.Empty, CFIAPaymentMethods.Codes.Broker);
			declaration2.JE_OH_Importer = importer.PK;

			var proxy2 = Factory.New<OrgHeader>();
			declaration2.Branch.GB_OH_OrgProxy = proxy2.PK;
			OrgImpAddInfo.Get(importer).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Broker;

			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine2.CFIAPGAHeader.CA_AIRSExtensionCode = "WW";

			var pgaProvider2 = new PGARequirementProvider(invoiceLine2);
			var pgaRequirement2 = new PGARequirement(Factory, PGACodes.Codes.CFIA, pgaProvider2);

			var programRequirement2 = (PGAProgramRequirement)pgaRequirement2.ProgramCodeRequirements.First();
			programRequirement2.DeclareYes = true;

			var cfia2 = invoiceLine2.CFIAPGAHeader;
			cfia2.CA_AllProgramInd = YesNoList.Codes.Yes;
			cfia2.RunPreSaveValidation();
			AssertNoWarning(programRequirement2.DeclareYesInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
		}
	}
}
