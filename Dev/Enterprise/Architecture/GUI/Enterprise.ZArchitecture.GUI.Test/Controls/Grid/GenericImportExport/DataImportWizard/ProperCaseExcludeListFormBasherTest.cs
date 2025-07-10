using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	[TestedType(typeof(ProperCaseExcludeListForm))]
	sealed class ProperCaseExcludeListFormBasherTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = (ZForm)GetFormToBash())
			{
				AssertEquals("Proper Case Exclude List", form.FormHeading);
			}
		}

		public void TestProperCaseExcludeListOnlyGetsUpdatedOnSave()
		{
			var wizard = new ImportWizard(Helper.CollectionInfo, null, null);
			wizard.ProperCaseExcludeList.AddNew().Word = "IBM";

			using (var form = new ProperCaseExcludeListForm(wizard))
			{
				form.Show();
				((ProperCaseExcludeWordCollection)form.DataSource).AddNew().Word = "ATM";
			}

			AssertContainsExcludeWord(wizard.ProperCaseExcludeList, "IBM", true);
			AssertContainsExcludeWord(wizard.ProperCaseExcludeList, "ATM", false);

			using (var form = new ProperCaseExcludeListForm(wizard))
			{
				form.Show();
				((ProperCaseExcludeWordCollection)form.DataSource).AddNew().Word = "USD";
				form.FireSaveButton();
			}

			AssertContainsExcludeWord(wizard.ProperCaseExcludeList, "IBM", true);
			AssertContainsExcludeWord(wizard.ProperCaseExcludeList, "USD", true);
			AssertContainsExcludeWord(wizard.ProperCaseExcludeList, "ATM", false);
		}

		#region Implementation

		void AssertContainsExcludeWord(ProperCaseExcludeWordCollection collection, string wordToCheck, bool shouldContain)
		{
			AssertEquals(shouldContain, collection.ToArray<ProperCaseExcludeWord>().Any(word => word.Word == wordToCheck));
		}

		protected override Form GetFormToBashCore()
		{
			var wizard = new ImportWizard(Helper.CollectionInfo, null, null);
			return new ProperCaseExcludeListForm(wizard);
		}

		ImportWizardTestHelper helper;
		ImportWizardTestHelper Helper
		{
			get { return helper ?? (helper = new ImportWizardTestHelper(Factory)); }
		}

		#endregion
	}
}
