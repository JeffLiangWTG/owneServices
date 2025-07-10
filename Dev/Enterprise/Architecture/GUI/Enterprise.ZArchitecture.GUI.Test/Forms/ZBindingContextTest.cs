using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Test.Forms
{
	internal class ZBindingContextTest : TestCaseWithDummy
	{
		public void TestPositionChangedUpdatesRelatedCurrencyMananger()
		{
			var superDummy = Factory.New<SuperDummyBusinessObject>();
			superDummy.Collection.AddNew();
			superDummy.Collection[0].Collection.AddNew();
			superDummy.Collection[0].Collection[0].Z0_Description = "Description";

			using (var form = new ZChildForm(superDummy))
			{
				var textBox = new ZTextBox();
				textBox.BindTo = "Collection.Collection.Z0_Description";
				textBox.CharacterCasing = CharacterCasing.Normal;
				form.Controls.Add(textBox);

				form.DataSourceType = superDummy.GetType();
				form.SetDataBinding(superDummy, "");

				form.Show();
				UserIdleWorker.Flush();

				AssertEquals("Text Initially", "Description", textBox.Text);
				AssertEquals("ReadOnly Initially", false, textBox.ReadOnly);

				superDummy.Collection.RemoveAll();
				Application.DoEvents();
				AssertEquals("Text after RemoveAll()", "", textBox.Text);
				AssertEquals("ReadOnly after RemoveAll()", true, textBox.ReadOnly);
			}
		}
	}
}
