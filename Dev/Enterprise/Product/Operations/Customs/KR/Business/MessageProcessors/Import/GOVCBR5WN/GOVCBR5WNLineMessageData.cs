using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5WNLineMessageData : NonPersistentBusinessObject
	{
		public GOVCBR5WNLineMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZShort EntryLineNo { get; set; }
		public ZString HSCodeBefore { get; set; }
		public ZString HSCodeAfter { get; set; }
		public ZString HSDescriptionBefore { get; set; }
		public ZString HSDescriptionAfter { get; set; }
		public ZString DescriptionBefore { get; set; }
		public ZString DescriptionAfter { get; set; }
		public ZDecimal QuantityBefore { get; set; }
		public ZDecimal QuantityAfter { get; set; }
		public ZDecimal AdditionalTariffAmountBefore { get; set; }
		public ZDecimal AdditionalTariffAmountAfter { get; set; }

		public GOVCBR5WNLineDutyTaxDetailMessageDataCollection DutyTaxDetails => dutyTaxDetails ?? (dutyTaxDetails = new GOVCBR5WNLineDutyTaxDetailMessageDataCollection(Factory));
		GOVCBR5WNLineDutyTaxDetailMessageDataCollection dutyTaxDetails;
	}

	public class GOVCBR5WNLineMessageDataCollection : NonPersistentBusinessObjectCollection<GOVCBR5WNLineMessageData>
	{
		public GOVCBR5WNLineMessageDataCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GOVCBR5WNLineMessageData(Factory);
		protected override bool AllowNewCore => false;
	}
}
