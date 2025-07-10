using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	[TestedType(typeof(LADTTemporyStorageUserControlForPlugin))]
	sealed class CusTempStorageFormLADTTest : CusTempStorageFormAbstractTest
	{
		protected override Type TemporyStorageDecUserControlType => typeof(ISTAndLADTCusTempStorageDecUserControl);

		protected override CINTemporyStorageUserControlForPlugin TemporyStorageUserControlForPlugin => new LADTTemporyStorageUserControlForPlugin();

		protected override ZString TemporaryStorageHeaderApplicationCode => Business.FRConstants.TemporaryStorage.AppCodeLAD;
	}
}
