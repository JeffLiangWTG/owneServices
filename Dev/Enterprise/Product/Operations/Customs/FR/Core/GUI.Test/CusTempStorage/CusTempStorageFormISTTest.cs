using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	[TestedType(typeof(CINTemporyStorageUserControlForPlugin))]
	sealed class CusTempStorageFormISTTest : CusTempStorageFormAbstractTest
	{
		protected override Type TemporyStorageDecUserControlType => typeof(ISTAndLADTCusTempStorageDecUserControl);

		protected override CINTemporyStorageUserControlForPlugin TemporyStorageUserControlForPlugin => new ISTTemporyStorageUserControlForPlugin();

		protected override ZString TemporaryStorageHeaderApplicationCode => Business.FRConstants.TemporaryStorage.AppCodeIST;
	}
}
