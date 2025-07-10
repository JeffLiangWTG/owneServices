using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(Package))]
	sealed class PackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var package = declaration.Packages.AddNew();
			AssertType<PackageValidation>("Import", package.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportPackageValidation>("ExportPackageValidation for Export Jobs.", package.Validation);
		}

		public void TestCW_PackQtyReadOnly()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var package = declaration.Packages.AddNew();
			AssertEquals("QTY writable for empty Package.", false, package.CW_PackQtyInfo.ReadOnly);

			package.CW_PackType = "1A";
			AssertEquals("QTY writable for normal Package.", false, package.CW_PackQtyInfo.ReadOnly);

			package.CW_PackType = "NE";
			AssertEquals("QTY writable for BreakBulk Package.", false, package.CW_PackQtyInfo.ReadOnly);

			package.CW_PackType = "VG";
			AssertEquals("QTY readonly for BULK Package.", true, package.CW_PackQtyInfo.ReadOnly);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			package.CW_PackType = "VG";
			AssertEquals("QTY writable for non-export jobs.", false, package.CW_PackQtyInfo.ReadOnly);
		}

		public void TestClearCW_PackQtyIfNeeded()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var package = declaration.Packages.AddNew();
			AssertEquals("QTY writable for empty Package.", false, package.CW_PackQtyInfo.ReadOnly);

			package.CW_PackQty = 1;
			package.CW_PackType = "1A";
			AssertEquals("QTY writable for normal Package so not cleared", 1, package.CW_PackQty);

			package.CW_PackType = "NE";
			AssertEquals("QTY writable for BreakBulk Package so not cleared", 1, package.CW_PackQty);

			package.CW_PackType = "VG";
			AssertEquals("QTY readonly for BULK Package so it is cleared.", ZInt.Zero, package.CW_PackQty);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			package.CW_PackQty = 1;
			package.CW_PackType = "VG";
			AssertEquals("QTY writable for non-export jobs so not cleared", 1, package.CW_PackQty);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "AE";
			var pack = declaration.Packages[0];
			return pack;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
