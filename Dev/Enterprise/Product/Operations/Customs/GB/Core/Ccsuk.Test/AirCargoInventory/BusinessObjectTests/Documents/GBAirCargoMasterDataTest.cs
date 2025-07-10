using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class GBAirCargoMasterDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.CusMAWB), new GBAirCargoMasterData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<CusMAWBCollection>("CollectionType", new GBAirCargoMasterData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.SupplyChainLogistics, new GBAirCargoMasterData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "CCSUK Master Air Waybill", new GBAirCargoMasterData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new GBAirCargoMasterData().IsAllowedForUnallocatedeDocs);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.Customs.EU.GB.CcsukAirInventory, new GBAirCargoMasterData().ModuleID);
		}

		[ExpectNoExceptions]
		public void TestFactorySaveOutsideCountryContext()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			var docManagerInfo = mawb.DocManagerInfo();
			docManagerInfo.AddFileOrDocument(ZBlob.FromAscii("Daniel"), "RRA.txt", "RRA");
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				docManagerInfo.Save();
			}
		}
	}
}
