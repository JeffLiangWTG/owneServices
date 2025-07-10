using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(PreviousDocumentsUserControl))]
	class PreviousDocumentsUserControlBaseOnlyTest : PreviousDocumentsUserControlAbstractTest
	{
		protected override IEnumerable<(string, Type, CharacterCasing, int, string)> PreviousDocumentsGridCheckList
		{
			get
			{
				yield return (PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), CharacterCasing.Upper, 47, "");
				yield return (PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), CharacterCasing.Upper, 219, "");
				yield return (PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), CharacterCasing.Normal, 90, "");
				yield return (PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), CharacterCasing.Normal, 66, "Line No.");
			}
		}
	}
}
