using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public class EntryHeaderPaymentInformation : IDataObject
	{
		public EntryHeaderPaymentInformation()
		{
		}
		public ZDateTime? PaymentDate { get; set; }
		public ZDateTime? EntryReleaseDate { get; set; }
		public CodeDescriptionPair TransactionType { get; set; }
		public CodeDescriptionPair PaymentParty { get; set; }
		public CodeDescriptionPair PaymentStatus { get; set; }
		public ZBool? CustomsResponseReceived { get; set; }
		public ZBool? RemittanceAdviceReceived { get; set; }
		public ZDecimal? PaymentAmount { get; set; }

		[MaxLength(20)]
		public ZString? PaymentReference { get; set; }
		[MaxLength(35)]
		public ZString? IncomingPaymentResponseNumber { get; set; }
	}
}
