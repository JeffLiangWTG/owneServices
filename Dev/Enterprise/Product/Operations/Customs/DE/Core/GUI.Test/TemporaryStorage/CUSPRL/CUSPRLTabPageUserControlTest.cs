using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class CUSPRLTabPageUserControlTest : TemporaryStorageTabPageUserControlTest
	{
		protected override TemporaryStorageMutexTabPageUserControl GetUserControlToTest()
		{
			return new CUSPRLTabPageUserControl();
		}

		protected override ZString DeclarationTypeDescription => TemporaryStorageApplicationCodeList.Descriptions.SumA;
		protected override ZString DecControlName => "CUSPRLDecControl";
		protected override MutexID MutexID => MutexIDs.CUSPRLTempStorageDec;

		protected override CusTempStorageDec LoadCusTemStorageDec(CusTempStorageJobHeader header)
		{
			return header.CUSPRLCusTempStorageDec;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CUSPRLCusTempStorageDec.LoadOrCreate(header);
		}
	}
}
