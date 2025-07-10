using System.Windows.Forms;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	[TestedType(typeof(CustomMapListsForm))]
	sealed class CustomMapListsFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new CustomMapListsForm(Helper.Wizard);
		}

		ImportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ImportWizardTestHelper(Factory);
				}

				return helper;
			}
		}

		ImportWizardTestHelper helper;

		#endregion
	}
}
