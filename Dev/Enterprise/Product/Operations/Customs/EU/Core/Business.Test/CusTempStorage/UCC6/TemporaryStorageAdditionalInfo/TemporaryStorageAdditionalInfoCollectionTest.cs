using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public abstract class TemporaryStorageAdditionalInfoCollectionAbstractTest<TAddInfo> : Customs.Business.Testing.CusSupportingInfoCollectionTest<TAddInfo>
		where TAddInfo : TemporaryStorageAdditionalInfo
	{
	}

	[TestedType(typeof(TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>))]
	sealed class TemporaryStorageAdditionalInfoCollectionTest : TemporaryStorageAdditionalInfoCollectionAbstractTest<TemporaryStorageAdditionalInfo>
	{
		public void TestSetDefaultsForNewChild_TemporaryStorageBill_Master()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
			AssertEquals("CSI_SubType set to 'INF'", "INF", bill.AdditionalInfos.AddNew().CSI_SubType);
		}

		public void TestSetDefaultsForNewChild_TemporaryStorageBill_House()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			bill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
			AssertEquals("CSI_SubType set to empty string", ZString.Empty, bill.AdditionalInfos.AddNew().CSI_SubType);
		}

		protected override CusSupportingInfoCollection<TemporaryStorageAdditionalInfo> GetCusSupportingInfoCollection()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			return new TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(bill);
		}
	}
}
