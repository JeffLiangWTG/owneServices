using System;
using System.Collections.Generic;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class EntryInstructionPreviousDocumentsUserControlTest : PreviousDocumentUserControlAbstractTest<EntryInstructionPreviousDocumentsUserControl, JobDeclaration>
{
	public void TestControlsPrevDocsGroupBox()
	{
		using (var control = CreateControl())
		{
			var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
			CombineAssertions(() =>
			{
				AssertNotNull("PrevDocsGroupBox", prevDocsGroupBox);
				AssertEquals("PrevDocsGroupBox: Visible", true, prevDocsGroupBox.Visible);
				AssertEquals("PrevDocsTypeDropEdit: not Visible", false, prevDocsGroupBox.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit").Visible);
				var codeFindBox = prevDocsGroupBox.FindSingle<ZCodeFindBox>("CodeCodeFindBox");
				var textBox = prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox");
				AssertEquals("CodeCodeFindBox: Visible", true, codeFindBox.Visible);
				AssertEquals("PrevDocsReferenceTextBox: Visible", true, textBox.Visible);

				AssertEquals("CodeCodeFindBox: TabIndex", 0, codeFindBox.TabIndex);
				AssertEquals("PrevDocsReferenceTextBox: TabIndex", 1, textBox.TabIndex);
			});
		}
	}

	public void TestCaptionPrevDocsGroupBox()
	{
		using (var control = CreateControl())
		{
			AssertEquals("Previous Document", control.FindSingle<ZGroupBox>("PrevDocsGroupBox").CaptionResourceString.Caption);
		}
	}

	protected override IEnumerable<(string, Type)> GetOrderedGridColumns() => new[]
	{
		(PreviousDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
		(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle))
	};
}
