using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportPreviousExpDecLine : IImportPreviousExpDecLine
	{
		public string DeclarationNumber { get; set; }
		public int EntryLineNo { get; set; }
		public int InvoiceLineNo { get; set; }
		public string UQ { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.UsedQtyPlaces)]
		public decimal UsedQty { get; set; }
		public short SequenceNumber { get; set; }

		ZString IImportPreviousExpDecLine.DeclarationNumber => DeclarationNumber;
		ZInt IImportPreviousExpDecLine.EntryLineNo => EntryLineNo;
		ZInt IImportPreviousExpDecLine.InvoiceLineNo => InvoiceLineNo;
		ZString IImportPreviousExpDecLine.UQ => UQ;
		ZDecimal IImportPreviousExpDecLine.UsedQty => UsedQty;
		ZShort IImportPreviousExpDecLine.SequenceNumber => SequenceNumber;
	}
}
