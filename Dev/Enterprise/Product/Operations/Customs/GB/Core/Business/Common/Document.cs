using System;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public class Document : ISupportingDocument
	{
		public Document(SupportingDocument document)
		{
			this.document = document ?? throw new ArgumentNullException(nameof(document));
		}

		protected readonly SupportingDocument document;

		#region IDocument Members

		CargoWise.Types.ZString ISupportingDocument.Code => document.CSI_Code;

		CargoWise.Types.ZString ISupportingDocument.Part => document.CSI_SubType;

		CargoWise.Types.ZDecimal ISupportingDocument.Quantity => document.CSI_Quantity;

		CargoWise.Types.ZString ISupportingDocument.Reason => document.CSI_Description;

		CargoWise.Types.ZString ISupportingDocument.Reference => document.CSI_ReferenceNumber;

		CargoWise.Types.ZString ISupportingDocument.Status => document.CSI_Availability + document.CSI_Actions;

		#endregion
	}
}
