using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusInBondMoveHeaderDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(CusReconDeclaration), data.BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertType<CusReconDeclarationCollection>(data.GetBusinessObjectCollection(Factory));
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.EU.DE.MonthlyClosing, data.ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, data.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Monthly Closing", data.HumanReadableName);
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, data.IsAllowedForUnallocatedeDocs);
		}

		protected override void SetUp()
		{
			base.SetUp();

			data = new CusReconDeclarationData();
		}
		CusReconDeclarationData data;
	}
}
