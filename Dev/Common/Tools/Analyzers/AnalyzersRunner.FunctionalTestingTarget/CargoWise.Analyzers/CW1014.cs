using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1014 : Control
	{
		public CW1014()
		{
			var resources = new ComponentResourceManager(typeof(object));

			//CW1014:Embedded Icon Rule
			this.Icon = (Icon)resources.GetObject("name");
		}
	}
}
