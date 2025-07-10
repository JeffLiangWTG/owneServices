using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(LADTCusTempStorageLine))]
	class LADTCusTempStorageLineTest : CusTempStorageLineTest
	{
		protected override Type GetValidationType() => typeof(LADTCusTempStorageLineValidation);

		protected override Type GetDecType() => typeof(LADTCusTempStorageDec);

		protected override ZString GetAppCode() => FRConstants.TemporaryStorage.AppCodeLAD;
	}
}
