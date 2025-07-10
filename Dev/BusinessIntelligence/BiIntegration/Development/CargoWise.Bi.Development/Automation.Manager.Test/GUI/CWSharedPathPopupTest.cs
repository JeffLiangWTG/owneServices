using CargoWise.Bi.Development.Automation.Manager;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.GUI.Testing
{
	public class CWSharedPathPopupTest : TestCase
	{
		[RequiresSTA]
		public void TestFormLoad()
		{
			var form = new CWSharedPathPopup();
			AssertNotNull(form);
			form.Show();
			form.Close();
			form.Dispose();
		}
	}
}
