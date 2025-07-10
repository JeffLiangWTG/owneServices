using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(UCC5ImportPreviousDocumentsUserControl))]
	class UCC5ImportPreviousDocumentsUserControlTest : PreviousDocumentsUserControlAbstractTest
	{
		protected override IEnumerable<(string, Type, CharacterCasing, int, string)> PreviousDocumentsGridCheckList
		{
			get
			{
				yield return (PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), CharacterCasing.Upper, 47, "");
				yield return (PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), CharacterCasing.Upper, 219, "");
				yield return (PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), CharacterCasing.Normal, 66, "Line No.");
			}
		}

		public void TestPreviousDocumentsGridLineNoColumnMaxLength()
		{
			using (var control = (PreviousDocumentsUserControl)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType())))
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				var columnStyle = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(PreviousDocument.Schema.CSI_LineNo);
				AssertEquals("Max Length", 5, columnStyle.MaxLengthOverride);
			}
		}
	}
}
