using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.UPE.Business.CommercialInvoice
{
	public static class CommercialInvoiceDocManager
	{
		public static ZBool HasCommercialInvoice(DocManagerInfo docManagerInfo)
		{
			return CommercialInvoiceDoc(docManagerInfo) != null;
		}

		public static Image CommercialInvoiceImage(DocManagerInfo docManagerInfo)
		{
			Image result = null;
			IeDoc eDoc = CommercialInvoiceDoc(docManagerInfo);
			if (eDoc != null)
			{
				try
				{
					result = Image.FromStream(new MemoryStream(eDoc.ImageData));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("PossiblyInvalidCommercialInvoiceDocumentImage", ex.Message, ex);
				}
			}
			return result;
		}

		public static IeDoc CommercialInvoiceDoc(DocManagerInfo docManagerInfo)
		{
			return docManagerInfo.Documents != null ? docManagerInfo.Documents.GetMostRecentEDoc(commercialInvoiceDocType) : null;
		}
		public const string commercialInvoiceDocType = "CIV";
	}
}
