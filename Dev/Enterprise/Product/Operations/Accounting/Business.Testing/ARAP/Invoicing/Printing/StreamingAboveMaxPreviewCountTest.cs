using System;
using System.Linq;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	sealed class StreamingAboveMaxPreviewCountTest : InvoicePrintTaskTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Statement.TestAboveMaxPreviewCount = true;
			AccountingMasterFilesRegistry.Instance.InvoiceUsePrintStreaming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Statement.TestAboveMaxPreviewCount = false;
		}

		protected override DocumentPack GetDocumentPack(InvoicePrintTask task, int index)
		{
			return task.GetTask().GetDocumentPacks().ElementAt(index);
		}
	}
}
