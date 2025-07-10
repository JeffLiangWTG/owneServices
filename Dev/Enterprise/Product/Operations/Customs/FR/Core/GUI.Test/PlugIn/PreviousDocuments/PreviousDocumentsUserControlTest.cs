using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	sealed class PreviousDocumentsUserControlTest : PreviousDocumentUserControlAbstractTest<PreviousDocumentsUserControl, JobDeclaration>
	{
		public void TestControlsPrevDocsGroupBox()
		{
			using (var control = CreateControl())
			{
				var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("PrevDocsGroupBox: Visible", true, prevDocsGroupBox.Visible);
					AssertEquals("PrevDocsTypeDropEdit: Visible", true, prevDocsGroupBox.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit").Visible);
					AssertEquals("PrevDocsReferenceTextBox: Visible", true, prevDocsGroupBox.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").Visible);
					AssertEquals("PrevDocsReferenceCodeFindBox: Visible", true, prevDocsGroupBox.FindSingle<ZCodeFindBox>("PrevDocsReferenceCodeFindBox").Visible);
				});
			}
		}

		protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
		{
			return new[]
			{
				(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle)),
				(PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyle)),
				(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
				(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
				(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle)),
			};
		}
	}
}
