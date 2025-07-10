using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocSupportingDocument : DocBaseWrapper
	{
		DocSupportingDocument(SupportingDocument supportingDocument, BusinessObjectFactory factoryToWrap)
			: base(supportingDocument, factoryToWrap)
		{
		}

		public static DocSupportingDocument New(SupportingDocument supportingDocument, BusinessObjectFactory factoryToWrap) => supportingDocument == null ? null : new DocSupportingDocument(supportingDocument, factoryToWrap);

		//Code
		public ZString Code => SupportingDocument.CSI_Code;

		//Referenz
		public ZString Reference => SupportingDocument.CSI_ReferenceNumber;

		//Datum
		public ZString Date => SupportingDocument.CSI_DateOfIssue.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

		//Menge
		public ZString Quantity => SupportingDocument.CSI_Quantity.ToStringRounded(3);

		public ZString UnitOfQuantity => SupportingDocument.CSI_UnitOfQuantity;

		SupportingDocument SupportingDocument => supportingDocument ??= (SupportingDocument)WrappedObject;
		SupportingDocument supportingDocument;
	}
}
