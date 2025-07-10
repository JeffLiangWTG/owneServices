using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CMRCodeListsController))]
	sealed class CMRCodeListsControllerTest : CMRSearchOnlyControllerTest
	{
		public override void TestDeleteForm() => throw new NotSupportedException();

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.CMRCodeLists;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = CMRCodeLists.New(Factory);
			Factory.Save();
			return result;
		}
	}
}
