using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(EUH7BillController))]
	public class EUH7BillControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.EUH7Bill;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var result = header.Bills.AddNew();
			Factory.Save();

			return result;
		}
	}
}
