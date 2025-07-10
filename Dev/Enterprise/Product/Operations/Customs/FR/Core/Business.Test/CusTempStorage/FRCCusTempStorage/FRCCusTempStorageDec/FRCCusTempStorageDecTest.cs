using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(FRCCusTempStorageDec))]
	public class FRCCusTempStorageDecTest : CusTempStorageDecTest
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(FRConstants.TemporaryStorage.AppCodeFRC, GetCusTempStorageDecForTesting().STH_DeclarationType);
		}
		protected override CusTempStorageDec GetCusTempStorageDecForTesting()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeFRC;
			header.CreateRelatedCusTempStorageDec();
			return FRCCusTempStorageDec.Load(header);
		}

		protected override ZString GetAppCode() => FRConstants.TemporaryStorage.AppCodeFRC;
	}
}
