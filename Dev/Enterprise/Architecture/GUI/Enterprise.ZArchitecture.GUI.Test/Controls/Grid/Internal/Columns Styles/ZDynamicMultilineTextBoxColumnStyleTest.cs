using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZDynamicMultilineTextBoxColumnStyleTest : TestCaseWithDummy
	{
		public void TestZDynamicMultilineTextBoxColumnStyleInfo()
		{
			var info = new ZDynamicMultilineTextBoxColumnStyleInfo();
			AssertEquals(typeof(ZDynamicMultilineTextBoxColumnStyle), info.ColumnStyleType);
		}

		public void TestEditControl()
		{
			using (var form = new ZForm(Dummy))
			{
				var childBizO = Dummy.Collection.AddNew();
				childBizO.Z0_NVarChar = "meh";

				var grid = new ZGrid();
				var styleInfo = new ZDynamicMultilineTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_NVarChar, 200);
				styleInfo.CharacterCasing = CharacterCasing.Lower;
				grid.ColumnStyles.Add(styleInfo);
				form.Controls.Add(grid);
				grid.SetDataBinding(Dummy.Collection, "");
				form.Show();

				var columnStyle = (ZDynamicMultilineTextBoxColumnStyle)grid.TableStyles[0].GridColumnStyles[0];
				AssertEquals("meh", columnStyle.EditControl.Text);
				Assert(((ZTextBox)columnStyle.EditControl).IsDynamicMultiline);
				AssertEquals(CharacterCasing.Lower, ((ZTextBox)columnStyle.EditControl).CharacterCasing);
				Assert(((INavigatingGridColumn)columnStyle).ShouldColumnHandleKey(Keys.Tab));
			}
		}
	}
}
