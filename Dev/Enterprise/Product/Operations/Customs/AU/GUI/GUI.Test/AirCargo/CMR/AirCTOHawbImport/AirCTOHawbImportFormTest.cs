using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(AirCTOHawbImportForm))]
	sealed class AirCTOHawbImportFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			return new AirCTOHawbImportForm(hawb);
		}
	}
}
