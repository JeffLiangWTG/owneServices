using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDSupportingDocumentForLineWrapper : DeclarationDVDSupportingDocumentWrapper, IDeclarationDVDSupportingDocumentForLine
	{
		public DeclarationDVDSupportingDocumentForLineWrapper(SupportingDocument doc) : base(doc)
		{
		}

		public ZString UnitOfMeasure => document.CSI_UnitOfQuantity;

		public ZDecimal Quantity => document.CSI_Quantity;

		public ZString Currency => document.CSI_RX_NKCurrency;

		public ZDecimal Amount => document.CSI_Value;
	}
}
