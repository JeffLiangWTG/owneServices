using CargoWise.Types;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	abstract class VATDeferStrategyAbstractTest : EU.Business.Declaration.Testing.VATDeferStrategyTest
	{
		public override void TestVATDeferNumberShouldBeDefaultedWhenOrganisationChanged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA2", declaration.CountryCode);
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.L;

			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Deferment number should be recalculated when importer has changed.", "TVA2", declaration.ZG_VATDeferNumber);
		}

		public override void TestVATDeferNumberShouldBeDefaultedWhenVATDeferTypeChanged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA0", declaration.CountryCode);
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.L;

			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Deferment type should be inferred from declarant organisation setup.", VATProcedureList.Codes.L, declaration.ZG_VATDeferType);
			AssertEquals("Defer type changed, the defer number should have been defaulted to declarant VAT number", "TVA0", declaration.ZG_VATDeferNumber);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			AssertEquals("Deferment number should change because defer type has changed.", ZString.Empty, declaration.ZG_VATDeferNumber);
		}

		public override void TestPaymentMethodSourceDependencies()
		{
			TestDefermentAccountNumberShouldBeDefaultedWhenOrganisationChanged();
		}

		public override void TestVATDeferTypeSourceDependencies()
		{
			TestVATDeferNumberShouldBeDefaultedWhenOrganisationChanged();
		}

		protected override bool ExpectDeclarantAsSourceOrgForPaymentMethodA => false;

		public new void TestVatDutyDeferrer()
		{
			Assert("This is the logic in GB, we don't like it in FR.", true);
		}

		public abstract new void TestDefermentAccountNumberShouldBeDefaultedWhenPaymentMethodChanged();

		public abstract new void TestDefermentAccountNumberShouldBeDefaultedWhenOrganisationChanged();

		protected override (EU.Business.Declaration.JobDeclaration, OrgHeader) CreateDeclarationAndImporter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "DANXXX", declaration.CountryCode);
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.ALT, "ALTXXX", declaration.CountryCode);
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVAXXX", declaration.CountryCode);
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "535D2B5E");

			Factory.Save();

			return (declaration, importer);
		}

		protected OrgHeader CreateDeclarant(string declarationCountryCode)
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "DANZZZ", declarationCountryCode);
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.ALT, "ALTZZZ", declarationCountryCode);
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI002", ZString.Empty, ZString.Empty, "59BC3AC4");

			return declarant;
		}
	}
}
