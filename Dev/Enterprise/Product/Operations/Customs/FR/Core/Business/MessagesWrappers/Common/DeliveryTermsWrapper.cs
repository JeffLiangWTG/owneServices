using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class DeliveryTermsWrapper : IDeliveryTerms
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public DeliveryTermsWrapper(CusEntryLine entryLine)
		{
			this.cusEntryLine = Argument.NotNull(entryLine, "CusEntryLine cannot be null");
		}

		public ZString IncotermCode => cusEntryLine?.RandomLine?.InvoiceHeader?.JZ_IncoTerm ?? ZString.Empty;

		public ZString DeliveryPlace => cusEntryLine?.RandomLine?.InvoiceHeader?.JZ_IncoTermPlace ?? ZString.Empty;

		public ZString IncotermPlace => GetIncotermPlace();

		ZString GetIncotermPlace()
		{
			return this.cusEntryLine.RandomLine?.InvoiceHeader?.ZG_AgreedPlaceCode ?? ZString.Empty;
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly CusEntryLine cusEntryLine;
	}
}
