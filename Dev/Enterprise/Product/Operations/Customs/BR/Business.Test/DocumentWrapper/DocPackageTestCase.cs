using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocPackage))]
	sealed class DocPackageTestCase : DocumentWrapperTestCase
	{
		public void TestPackAndUnit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Units");
			var packCode1 = helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "01", "Test 1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			var packCode2 = helper.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "02", "Test 2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateOrGetLanguage("PT", "Portuguese");
			helper.CreateNewOrGetExistingCusCodeListLanguage(packCode1, "PT", "Teste 1");
			helper.CreateNewOrGetExistingCusCodeListLanguage(packCode2, "PT", "Teste 2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = "MB";
			houseBill.CU_BillNum = "123";
			var packing = houseBill.PackingGroups.AddNew();

			var pack = packing.Packages.AddNew();
			pack.CW_PackType = "01";
			pack.CW_PackQty = 10;

			var packageWrapper = DocPackage.New(pack, Factory);
			AssertEquals("Unit", "01", packageWrapper.Unit);
			AssertEquals("CodeAndDescription", "01 - Teste 1", packageWrapper.Pack.Unit.CodeAndDescription);
			AssertEquals("Value", 10m, packageWrapper.Pack.Value);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocPackage.New(packs, Factory) };
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Brazil; }
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = "MB";
			houseBill.CU_BillNum = "123";
			var packing = houseBill.PackingGroups.AddNew();

			packs = packing.Packages.AddNew();
			packs.CW_PackType = "01";
			packs.CW_PackQty = 10;
			base.SetUp();
		}

		BasePackage packs;

		#endregion
	}
}
