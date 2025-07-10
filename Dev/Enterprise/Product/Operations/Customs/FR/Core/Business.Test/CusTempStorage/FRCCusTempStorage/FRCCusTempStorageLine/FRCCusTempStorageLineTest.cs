using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(FRCCusTempStorageLine))]
	class FRCCusTempStorageLineTest : CusTempStorageLineTest
	{
		protected override Type GetDecType() => typeof(FRCCusTempStorageDec);

		protected override ZString GetAppCode() => FRConstants.TemporaryStorage.AppCodeFRC;
	}
}
