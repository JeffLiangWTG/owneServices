using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class MessageUserControlTest : TestCaseWithFactory
{
	public void TestManualDeclarationColumn()
	{
		using (var control = new MessageUserControl())
		{
			control.InitializeGridLayout();
			var manualDeclarationColumnInfo = control.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.ZG_ManualDeclaration);
			AssertNotNull(manualDeclarationColumnInfo);

			Assert(manualDeclarationColumnInfo.IsReadOnly);
		}
	}

	public void TestEntryLineAdditionalDataUserControl()
	{
		using (var messageUserControl = new MessageUserControlForTest())
		{
			messageUserControl.InitializeGridLayout();
			using (var entryLineAdditionalDataUserControl = messageUserControl.GetEntryLineAdditionalDataExposed())
			{
				AssertType(typeof(EntryLineAdditionalDataUserControl), entryLineAdditionalDataUserControl);
			}
		}
	}

	sealed class MessageUserControlForTest : MessageUserControl
	{
		internal EntryLineAdditionalDataUserControl GetEntryLineAdditionalDataExposed() => base.GetEntryLineAdditionalData() as EntryLineAdditionalDataUserControl;
	}
}
