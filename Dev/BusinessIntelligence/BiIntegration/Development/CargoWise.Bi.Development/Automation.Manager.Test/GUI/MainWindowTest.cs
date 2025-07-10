using System.IO;
using System.Linq;
using CargoWise.Bi.Development.Automation.Manager;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.GUI.Testing
{
	public class MainWindowTest : TestCase
	{
		[RequiresSTA]
		public void TestFormLoad()
		{
			using (var form = new MainWindow())
			{
				form.CWSharedPathSet = true;
				AssertNotNull(form);
				form.Show();
				AssertEquals("CargoWise BI Automation Manager", form.Title);
				form.Close();
			}
		}

		string TestPath => Path.Combine(BuildConstants.LocalEnterprisePath, "BusinessIntelligence", "BiIntegration", "Development", "Cargowise.Bi.Development", "SchemaSync.Testing", "ConfigXmlForTest");

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			if (!Directory.Exists(TestPath))
			{
				Directory.CreateDirectory(TestPath);
			}
			var path = Path.Combine(TestPath, "BiConfigFileForTest.xml");
			using (StreamWriter writer = new StreamWriter(path, false))
			{
				writer.WriteLine("test");
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
			var path = Path.Combine(TestPath, "BiConfigFileForTest.xml");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		[RequiresSTA]
		public void TestTableSearchOptionsInitializesDefaultSearchParameter()
		{
			using (var form = new MainWindow())
			{
				form.CWSharedPathSet = true;
				AssertNotNull(form);
				form.Show();

				var defaultSearchParameter = form.TableSearchParameters.ParameterList.FirstOrDefault();
				var defaultSearchCategory = defaultSearchParameter.Category;
				var defaultSearchValue = defaultSearchParameter.Value;
				AssertEquals("Source Table", defaultSearchCategory);

				var tempParameter = new SearchParameter("Source Table", "", CollectionType.TableParameters, null);
				var expectedDefaultSearchValue = tempParameter.Options.FirstOrDefault() ?? "";

				AssertNotNull("There must be a search parameter on the form.", defaultSearchParameter);
				AssertNotNull("The default search value must exist.", defaultSearchValue);
				AssertNotNull("The expected default search value must exist.", expectedDefaultSearchValue);
				AssertEquals(expectedDefaultSearchValue, defaultSearchValue);
				form.Close();
			}
		}

		[RequiresSTA]
		public void TestColumnSearchOptionsInitializesDefaultSearchParameter()
		{
			using (var form = new MainWindow())
			{
				form.CWSharedPathSet = true;
				AssertNotNull(form);
				form.Show();

				var defaultSearchParameter = form.ColumnSearchParameters.ParameterList.FirstOrDefault();
				var defaultSearchCategory = defaultSearchParameter.Category;
				var defaultSearchValue = defaultSearchParameter.Value;
				AssertEquals("Source Column", defaultSearchCategory);

				var tempParameter = new SearchParameter("Source Column", "", CollectionType.ColumnParameters, null);
				var expectedDefaultSearchValue = tempParameter.Options.FirstOrDefault() ?? "";

				AssertNotNull("There must be a search parameter on the form.", defaultSearchParameter);
				AssertNotNull("The default search value must exist.", defaultSearchValue);
				AssertNotNull("The expected default search value must exist.", expectedDefaultSearchValue);
				AssertEquals(expectedDefaultSearchValue, defaultSearchValue);
				form.Close();
			}
		}
	}
}
