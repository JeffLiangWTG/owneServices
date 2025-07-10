using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCI_TariffNum_UpdateCI_SecondUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs UQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "NAR", "Hello", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, "EXP");
			Factory.Save();
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var tariffUOMView = cusTariff.UnitsOfMeasure.AddNew();
			tariffUOMView.ZZ8_Type = UOMTypeList.Codes.CU2;
			tariffUOMView.ZZ8_UOM = "NAR";

			CombineAssertions(() =>
			{
				AssertEquals("CI_TariffNum is empty", ZString.Empty, pivot.CI_SecondUnitQty);

				pivot.CI_ChildType = ClassificationType.EXP;
				pivot.CI_TariffNum = "123456789";
				AssertEquals("Has CU2 UOM", "NAR", pivot.CI_SecondUnitQty);

				tariffUOMView.ZZ8_Type = UOMTypeList.Codes.CU1;
				pivot.CI_TariffNum = ZString.Empty;
				pivot.CI_TariffNum = "123456789";
				AssertEquals("No CU2 UOM", ZString.Empty, pivot.CI_SecondUnitQty);
			});
		}

		public void TestCI_SecondQty_Caption()
		{
			var resourceStringDataAttribute = pivot.CI_SecondQtyInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Short", "[41] 2nd Qty", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Medium", "[41] Second Qty", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "[41] Second Quantity", resourceStringDataAttribute.Caption);
			});
		}

		public void TestCI_SecondQty_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusClassPartPivot), nameof(CusClassPartPivot.CI_SecondQty), false, x => x.DecimalPlaces == 5);
		}

		public void TestCI_SecondUnitQty_Caption()
		{
			var resourceStringDataAttribute = pivot.CI_SecondUnitQtyInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Short", "[41] 2nd UQ", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Medium", "[41] Second UQ", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Caption", "[41] Second Unit Quantity", resourceStringDataAttribute.Caption);
			});
		}

		public void TestCI_FourthQty_Caption()
		{
			AssertEquals("Caption of CI_FourthQty", "Fourth Qty", pivot.CI_FourthQtyInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestCI_FourthQty_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusClassPartPivot), nameof(CusClassPartPivot.CI_FourthQty), false, x => x.DecimalPlaces == 5);
		}

		public void TestCI_FifthQty_Caption()
		{
			AssertEquals("Caption of CI_FifthQty", "Fifth Qty", pivot.CI_FifthQtyInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestCI_FifthQty_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusClassPartPivot), nameof(CusClassPartPivot.CI_FifthQty), false, x => x.DecimalPlaces == 5);
		}

		public void TestSupportingDocuments()
		{
			AssertType<SupportingDocumentCollection>(pivot.SupportingDocuments);
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(pivot.AdditionalInfos);
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(pivot.PreviousDocuments);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			CombineAssertions(() =>
			{
				var supportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)pivot;
				var supportedTypes = supportingInfoTypeSupporter.GetCusSupportingInfoTypes();
				AssertEquals("SupportingDocument", typeof(SupportingDocument), supportedTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals("AdditionalInfo", typeof(AdditionalInfo), supportedTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals("PreviousDocument", typeof(PreviousDocument), supportedTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return pivot;
		}

		protected override void SetUp()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = org.PK;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.Both;
		}
		CusClassPartPivot pivot;
	}
}
