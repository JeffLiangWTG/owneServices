using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[TestsSubclassesOf(typeof(GridColumnProvider))]
	public abstract class GridColumnProviderBaseTest : TestCaseWithFactory
	{
		public virtual void TestColumnKeys()
		{
			foreach (IUniqueKeyColumn column in TestProvider.AllColumns)
			{
				AssertNotNull("Column " + ((DataGridColumn)column).HeaderText + " does not have ColumnKey", column.ColumnKey);
			}
		}

		public void TestCutomizeBaseCalled()
		{
			TestProvider.CustomizeDictionary();
			AssertNotNull(TestProvider.AllColumns);
			AssertNotNull(TestProvider.DefaultColumns);
			AssertNotNull(TestProvider.RequiredColumns);

			Assert("base should be called when overriding CustomizeDictionary()", TestProvider.BaseCustomizeDictionaryCalled);
		}

		public virtual void TestTranslatability()
		{
			CombineAssertions(delegate
			{
				const string hao = "好";
				using (var mockRes = Res.UseMockData())
				{
					mockRes.SetResourceGetter((string key) => new ResourceStringData(key, hao));
					SetupNewProvider();
					foreach (IUniqueKeyColumn column in TestProvider.AllColumns)
					{
						var headerText = ((DataGridColumn)column).HeaderText;
						Assert("Grid column field \"" + headerText + "\" is not translatable", string.IsNullOrEmpty(headerText) || headerText.Contains(hao));
					}
				}
			});
		}

		protected abstract GridColumnProvider GetNewTestProvider();

		protected override void SetUp()
		{
			base.SetUp();
			SetupNewProvider();
		}

		protected virtual void SetupNewProvider()
		{
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
		}

		protected GridColumnProvider TestProvider;
	}
}
