using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(LADTCusTempStorageDec))]
	public class LADTCusTempStorageDecTest : CusTempStorageDecTest
	{
		[TestDate(2020, 06, 11)]
		public void TestSetDefaultValues()
		{
			var storage = GetCusTempStorageDecForTesting();
			AssertEquals(FRConstants.TemporaryStorage.AppCodeLAD, storage.STH_DeclarationType);
		}

		public override void TestValidation()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			AssertType<LADTCusTempStorageDecValidation>(storageDec.Validation);
		}

		protected override CusTempStorageDec GetCusTempStorageDecForTesting()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeLAD;
			header.CreateRelatedCusTempStorageDec();
			var result = LADTCusTempStorageDec.Load(header);
			result.CusTempStorageLines.RemoveAndDeleteAll();
			return result;
		}

		protected override ZString GetAppCode() => FRConstants.TemporaryStorage.AppCodeLAD;
	}
}
