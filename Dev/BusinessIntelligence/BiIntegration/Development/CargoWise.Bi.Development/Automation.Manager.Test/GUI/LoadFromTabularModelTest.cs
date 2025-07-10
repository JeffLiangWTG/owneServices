using CargoWise.Bi.Development.Automation.Manager;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.GUI.Testing
{
	public class LoadFromTabularModelTest : TestCase
	{
		[RequiresSTA]
		public void TestFormLoad()
		{
			using (var form = new LoadFromTabularModel())
			{
				form.Show();
				AssertEquals("Load From Tabular Model", form.Title);
				form.Close();
			}
		}
	}
}
