using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Design
{
	sealed class ZGridColumnStyleEditorTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCreateCollectionFormCanLoadAllControls()
		{
			var editor = new TestGridColumnStyleEditor();
			editor.CreateCollectionForm();
		}

		#region Test Classes

		class TestGridColumnStyleEditor : ZGridColumnStyleEditor
		{
			public new Control GetControlByName(string controlName)
			{
				return base.GetControlByName(controlName);
			}

			public new Form CreateCollectionForm()
			{
				return base.CreateCollectionForm();
			}
		}

		#endregion
	}
}
