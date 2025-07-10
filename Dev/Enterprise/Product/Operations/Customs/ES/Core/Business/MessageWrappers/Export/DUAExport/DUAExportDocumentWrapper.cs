using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportDocumentWrapper : ExportDocumentCommonWrapper, IDUAExportDocuments
	{
		public DUAExportDocumentWrapper(SupportingDocument doc) : base(doc) { }

		public ZDecimal Quantity => document.CSI_Quantity;

		public ZString QtyUnit => document.CSI_UnitOfQuantity.ConvertCargoWiseToES(document.Factory);
	}
}
