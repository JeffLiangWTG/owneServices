using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(XXXImportForm))]
	public class XXXImportFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			XXXImportManager iNDImportManager = new XXXImportManager(new BusinessObjectFactory(), "blah");
			return new XXXImportForm(iNDImportManager);
		}
	}
}
