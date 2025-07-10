using System;
using System.Collections.Generic;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class ImportInvoiceLineLayoutPreviousDocumentsUserControlTest : EU.GUI.PlugIn.Testing.PreviousDocumentUserControlAbstractTest<ImportInvoiceLineLayoutPreviousDocumentsUserControl, JobDeclaration>
	{
		protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
		{
			yield return (PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle));
			yield return (PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle));
			yield return (PreviousDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle));
		}
	}
}
