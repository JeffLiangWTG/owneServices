using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class PreviousDocumentsFieldsUserControlTest : TestCase
{
	public void TestItemNumberCalcEdit()
	{
		using (var control = new PreviousDocumentsFieldsUserControl())
		{
			var itemNumberCalcEdit = control.ItemNumberCalcEdit;
			var resStringData = itemNumberCalcEdit.CaptionResourceString;
			CombineAssertions("ItemNumberCalcEdit", () =>
			{
				AssertType<ZCalcEdit>("Type", itemNumberCalcEdit);
				AssertEquals("BindTo", "CSI_ItemNumber", itemNumberCalcEdit.BindTo);

				AssertEquals("Caption", "Goods Item Identifier", resStringData.Caption);
				AssertEquals("ShortCaption", "Item No.", resStringData.ShortCaption);
				AssertEquals("FullDescription", "[12 01 007 000] Goods Item Identifier", resStringData.FullDescription);
			});
		}
	}
}
