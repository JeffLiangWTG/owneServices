using System.Windows.Forms;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI
{
	[TestedType(typeof(JXCImporterForm))]
	public class BasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new JXCImporterForm(new JXCDataImporterBizO());
		}
	}
}
