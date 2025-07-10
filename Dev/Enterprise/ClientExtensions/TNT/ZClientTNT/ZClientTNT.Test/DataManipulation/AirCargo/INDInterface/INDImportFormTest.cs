using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(INDImportForm))]
	public class INDImportFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			IQDownImportManager iNDImportManager = new IQDownImportManager(new BusinessObjectFactory(), "blah");
			return new INDImportForm(iNDImportManager);
		}
	}
}
