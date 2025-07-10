using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(ISTCusTempStorageLine))]
	class ISTCusTempStorageLineTest : CusTempStorageLineTest
	{
		protected override Type GetValidationType() => typeof(ISTCusTempStorageLineValidation);

		protected override Type GetDecType() => typeof(ISTCusTempStorageDec);

		protected override ZString GetAppCode() => FRConstants.TemporaryStorage.AppCodeIST;
	}
}
