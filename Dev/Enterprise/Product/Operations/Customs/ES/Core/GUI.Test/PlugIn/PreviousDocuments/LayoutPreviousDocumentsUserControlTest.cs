using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

public class LayoutPreviousDocumentsUserControlTest : EU.GUI.PlugIn.Testing.PreviousDocumentUserControlAbstractTest<LayoutPreviousDocumentsUserControl, JobDeclaration>
{
	protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
	{
		return new[]
		{
				(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle)),
				(PreviousDocument.Schema.CSI_SubType, typeof(ZCodeFindBoxColumnStyle)),
				(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle)),
				(PreviousDocument.Schema.CSI_RN_NKCountryCode, typeof(ZCodeFindBoxColumnStyle)),
			};
	}
}
