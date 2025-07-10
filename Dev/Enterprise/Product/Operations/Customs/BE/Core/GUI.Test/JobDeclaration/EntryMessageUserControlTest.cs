using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

class EntryMessageUserControlTest : TestCaseWithFactory
{
	public void TestMessageUserControlType()
	{
		using (var entryMessageUserControl = new EntryMessageUserControlForTest())
		{
			using (var messageUserControl = entryMessageUserControl.GetMessageUserControl())
			{
				AssertType(typeof(MessageUserControl), messageUserControl);
			}
		}
	}

	class EntryMessageUserControlForTest : EntryMessageUserControl
	{
		public new BaseCustomsEntryUserControl GetMessageUserControl() => base.GetMessageUserControl();
	}
}
