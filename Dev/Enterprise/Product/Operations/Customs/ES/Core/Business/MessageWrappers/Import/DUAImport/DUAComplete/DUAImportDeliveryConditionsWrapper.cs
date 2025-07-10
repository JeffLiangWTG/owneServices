using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAImportDeliveryConditionsWrapper : IDUACompleteImportDeliveryConditions
	{
		public DUAImportDeliveryConditionsWrapper(CusEntryHeader cusEntryHeader)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			declaration = entryHeader.Declaration;
			invoiceHeader = entryHeader.RandomHeader;
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly JobComInvoiceHeader invoiceHeader;

		public ZString Code => !invoiceHeader.IncoTerm.IsEmpty ? invoiceHeader.IncoTerm : declaration.JE_ShipmentIncoTerm;

		public ZString Place => !invoiceHeader.JZ_IncoTermPlace.IsEmpty ? invoiceHeader.JZ_IncoTermPlace : declaration.JE_ShipmentIncoTermPlace;

		public ZString ZoneIndicator => declaration.ZG_AgreedPlaceCode;
	}
}
