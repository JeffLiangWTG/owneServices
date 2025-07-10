using System.Drawing;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZStmNotePopupEdit;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZNoteTextBoxWithWrongDefaultSizeTest : TestCase
	{
		class ZNoteTextBoxWithWrongDefaultSizeToInduceResizeDuringBaseConstructor : ZNoteTextBox
		{
			public ZNoteTextBoxWithWrongDefaultSizeToInduceResizeDuringBaseConstructor(ZStmNotePopupEdit parentEditor)
				: base(parentEditor)
			{
			}

			protected override Size DefaultSize => new Size(1234, 1234);
		}

		[ExpectNoExceptions]
		public void TestVirtualMethodsDoNotRelyOnMembersInitialisedByConstructor()
		{
			// Repro of issue 6102 - UpdateTextFromNote called by a base class constructor, was accessing the ParentEditor field which had not been initialised
			using (var notePopupEdit = new ZStmNotePopupEdit())
			{
				new ZNoteTextBoxWithWrongDefaultSizeToInduceResizeDuringBaseConstructor(notePopupEdit).Dispose();
			}
		}
	}
}
