using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(TransactionsExportController))]
	public class TransactionsExportControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TransactionsExport;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new XmlExportGUIWrapper(Factory);
		}
	}
}
