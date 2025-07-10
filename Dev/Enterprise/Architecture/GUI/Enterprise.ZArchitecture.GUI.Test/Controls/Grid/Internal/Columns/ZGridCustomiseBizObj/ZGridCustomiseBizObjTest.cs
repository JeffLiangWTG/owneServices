using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(ZGridCustomiseBizObj))]
	sealed class ZGridCustomiseBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetCurrentLayoutWhenDeletedDoesntThrowException()
		{
			var zGridBizO = (ZGridCustomiseBizObj)GetNewBusinessObject();

			var preConfiguredLayout = Factory.New<StmModuleFilter>();
			preConfiguredLayout.S9_SaveColumnLayout = true;
			preConfiguredLayout.Delete();
			Factory.Save();

			AssertNoExceptionThrown(() => zGridBizO.CurrentLayout = preConfiguredLayout);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ZGridCustomiseBizObj(new string[] { "0" }, new string[] { "0" }, null, ZGuid.Empty);
		}
	}
}
