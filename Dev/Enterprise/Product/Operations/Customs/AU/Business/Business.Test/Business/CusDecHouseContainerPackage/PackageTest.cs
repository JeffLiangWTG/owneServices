using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(Package))]
	public class PackageTest : Customs.Business.Testing.BasePackageTest
	{
		public void TestDeleteKillsPackingGroupWhenNoSiblings()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			PackingGroup group = declaration.PackingGroups.AddNew();
			Package pack1 = group.Packages.AddNew();
			pack1.Delete();
			Assert("Group", group.IsDeleted);
			Assert("Pack", pack1.IsDeleted);
		}

		public void TestDeleteDoesNotKillPackingGroupWhenHasSiblings()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			PackingGroup group = declaration.PackingGroups.AddNew();
			Package pack1 = group.Packages.AddNew();
			Package pack2 = group.Packages.AddNew();
			pack1.Delete();
			Assert("Group", !group.IsDeleted);
			Assert("Pack1", pack1.IsDeleted);
			Assert("Pack2", !pack2.IsDeleted);
		}

		public void TestDeleteDoesNotKillsPackingGroupWhenNoSiblingsButSyncronising()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			PackingGroup group = declaration.PackingGroups.AddNew();
			Package pack1 = group.Packages.AddNew();
			pack1.IsSynchronising = true;
			pack1.Delete();
			Assert("Group", !group.IsDeleted);
			Assert("Pack", pack1.IsDeleted);
		}

		public void TestDoNotDeleteEvenIfPackQtyIsEmpty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			PackingGroup multiPack = (PackingGroup)bill.PackingGroups.AddNew();
			Package package = multiPack.Packages.AddNew();

			Factory.Save();
			AssertEquals(false, package.IsDeleted);
		}

		public void TestCW_CargoStatus()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			PackingGroup group = declaration.PackingGroups.AddNew();
			group.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			Package pack1 = group.Packages.AddNew();
			AssertEquals("CW_CargoStatus", CMRConsolidatedCargoStatuses.ShortDescriptions.Acsseized, pack1.CW_CargoStatus);
		}

		public void TestCW_PackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var group = declaration.PackingGroups.AddNew();
			var pack1 = group.Packages.AddNew();
			AssertEquals("CW_PackType should default to PKG if empty", Core.Constants.PkgUnit.Package, pack1.CW_PackType);

			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Box;
			AssertEquals("CW_PackType should default to Declaration pack type if empty", Core.Constants.PkgUnit.Box, pack1.CW_PackType);

			declaration.JE_MasterBill = "081-003994877";
			pack1.CW_HouseBill = "HB00238";
			pack1.CW_PackQty = 20;
			pack1.CW_PackType = Core.Constants.PkgUnit.Bottle;
			AssertEquals("CW_PackType should default to Pack UQ if entered", Core.Constants.PkgUnit.Bottle, pack1.CW_PackType);

			var pack2 = group.Packages.AddNew();
			AssertEquals("CW_PackType should default to Declaration pack type if empty", Core.Constants.PkgUnit.Box, pack2.CW_PackType);

			var pack3 = group.Packages.AddNew();
			pack3.CW_HouseBill = "HB00238";
			pack3.CW_PackQty = 50;
			pack3.CW_PackType = Core.Constants.PkgUnit.Keg;
			AssertEquals("CW_PackType should default to Pack UQ if entered", Core.Constants.PkgUnit.Keg, pack3.CW_PackType);
		}
	}
}
