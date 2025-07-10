using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(BillLayoutBuilder<AsycudaBill>))]
	sealed class BillLayoutBuilderBaseOnlyTest : BillLayoutBuilderAbstractTest<BillLayoutBuilder<AsycudaBill>, AsycudaBill>
	{
		public void TestBillIssuerTextBoxVisibility()
		{
			var bill = GetBill();
			var layout = ((IPanelLayoutProvider)new DefaultBillLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Not manadatory", false, layout.IsVisible(CommonBillControlBag.Instance.BillIssuerTextBox, bill));
				SetIssuerCodeMandatory(bill.Header);
				AssertEquals("Mandatory", true, layout.IsVisible(CommonBillControlBag.Instance.BillIssuerTextBox, bill));
			});
		}

		public void TestBillIssuerNameVisibility()
		{
			var bill = GetBill();
			var layout = ((IPanelLayoutProvider)new DefaultBillLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Not manadatory", false, layout.IsVisible(CommonBillControlBag.Instance.BillIssuerNameTextBox, bill));
				SetIssuerCodeMandatory(bill.Header);
				AssertEquals("Mandatory", true, layout.IsVisible(CommonBillControlBag.Instance.BillIssuerNameTextBox, bill));
			});
		}

		public void TestBillIssuerCodeFindBoxVisibility()
		{
			AddCarrier();
			var bill = GetBill();
			var layout = ((IPanelLayoutProvider)new DefaultBillLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Not manadatory", false, layout.IsVisible(CommonBillControlBag.Instance.BillIssuerCodeFindBox, bill));
				SetIssuerCodeMandatory(bill.Header);
				AssertEquals("Mandatory", true, layout.IsVisible(CommonBillControlBag.Instance.BillIssuerCodeFindBox, bill));
			});
		}

		protected override int ExpectedMaxColumns => 4;

		protected override BillLayoutBuilder<AsycudaBill> GetColumnLayoutBuilderForTesting() => new BillLayoutBuilder<AsycudaBill>();

		AsycudaBill GetBill()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(currentCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, "MANDATORYFORMESSAGETYPE", "FWB");
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_RN_NKCountry = currentCountryCode;

			return header.Bills.AddNew();
		}

		void SetIssuerCodeMandatory(AsycudaManifestHeader header)
		{
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.FWB);
		}

		void AddCarrier()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "3001";
			carrier.ZZ4_Description = "3001 Desc";
			carrier.ZZ4_CountryOrGrouping = GlbCompany.CurrentCompany.Country.Code;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();
		}
	}
}
