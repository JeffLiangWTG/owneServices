using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class PackageProviderHelperTest : TestCaseWithFactory
	{
		public void TestKindOfPackages()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, providerHelper.KindOfPackages);
				emcsPackage.B5_UnitType = Core.Constants.PkgUnit.Bag;
				AssertEquals(Core.Constants.PkgUnit.Bag, providerHelper.KindOfPackages);
			});
		}

		public void TestNumberOfPackages()
		{
			emcsInvoiceLine.ZG_IsMainPack = ZBool.True;
			emcsPackage.B5_UnitCount = 12;

			CombineAssertions(() =>
			{
				emcsPackage.B5_UnitType = "VQ";
				AssertEquals("Null, as not countable", null, providerHelper.NumberOfPackages);

				emcsPackage.B5_UnitType = "BO";
				AssertEquals("Value, as countable", 12L, providerHelper.NumberOfPackages);
			});
		}

		public void TestNumberOfPackages_InnerPack()
		{
			emcsInvoiceLine.ZG_IsMainPack = ZBool.False;
			emcsPackage.B5_UnitCount = 12;

			emcsPackage.B5_UnitType = "BO";
			AssertEquals(0L, providerHelper.NumberOfPackages);
		}

		public void TestSealNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, providerHelper.SealNumber);
				emcsPackage.B5_SealNumber = "11DE11111111111115";
				AssertEquals("11DE11111111111115", providerHelper.SealNumber);
			});
		}

		public void TestShippingMarks()
		{
			emcsPackage.B5_MarksAndNumbers = "MARKS AND NUMBERS";
			emcsPackage.B5_UnitType = "BO";

			AssertEquals("Countable, no parent package", "MARKS AND NUMBERS", providerHelper.ShippingMarks);
		}

		public void TestShippingMarks_NotCountable()
		{
			emcsPackage.B5_MarksAndNumbers = "MARKS AND NUMBERS";
			emcsPackage.B5_UnitType = "VQ";

			AssertEquals("Not countable", ZString.Empty, providerHelper.ShippingMarks);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var boCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "BO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			boCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration = Factory.New<EMCSJobDeclaration>();
			emcsPackage = declaration.EMCSPackages.AddNew();
			emcsInvoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			providerHelper = new PackageProviderHelper(emcsInvoiceLine, emcsPackage);
		}
		EMCSJobDeclaration declaration;
		EMCSPackage emcsPackage;
		EMCSJobComInvoiceLine emcsInvoiceLine;
		PackageProviderHelper providerHelper;
	}
}
