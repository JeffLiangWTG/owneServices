using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(InvoiceHeaderExportPreviousDocumentsUserControl))]
	class InvoiceHeaderExportPreviousDocumentsUserControlTest : PreviousDocumentsUserControlAbstractTest
	{
		protected override IEnumerable<(string ColumnName, Type ExpectedColumnType, CharacterCasing ExpectedCharacterCasing, int ExpectedWidth, string ExpectedCaption)> PreviousDocumentsGridCheckList
		{
			get
			{
				yield return (PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), CharacterCasing.Upper, 47, "");
				yield return (PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), CharacterCasing.Upper, 219, "");
			}
		}

		public void TestPrevDocsGroupBoxCaption()
		{
			using (var control = new InvoiceHeaderExportPreviousDocumentsUserControl())
			{
				var groupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
				AssertEquals("PrevDocsGroupBox should have a correct caption.", "Previous Document", groupBox.CaptionResourceString.Caption);
			}
		}
	}
}
