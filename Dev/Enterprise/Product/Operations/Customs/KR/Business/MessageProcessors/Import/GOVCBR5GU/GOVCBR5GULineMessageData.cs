using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5GULineMessageData : NonPersistentBusinessObject
	{
		public GOVCBR5GULineMessageData(BusinessObjectFactory factory) : base(factory)
		{
			this.list = factory.GetCachedValue<ViolationCodeList>();
		}

		readonly ViolationCodeList list;
		public ZInt EntryLineNo { get; set; }
		public ZString ViolationCode { get; set; }
		public ZString ViolationDescription => list.GetDescriptionFromCode(ViolationCode);
		public ZString ViolationName { get; set; }
		public ZString CorrectionMethod { get; set; }

		public ZString TariffDescription { get; set; }
		public ZString Tariff { get; set; }
		public ZString CountryOfOrigin { get; set; }
		public ZDecimal CustomsQuantity { get; set; }
		public ZString CustomsUnitQty { get; set; }
		public ZDecimal NetWeightInKG { get; set; }
		public ZDecimal CustomsValueUSD { get; set; }
	}

	public class GOVCBR5GULineMessageDataCollection : NonPersistentBusinessObjectCollection<GOVCBR5GULineMessageData>
	{
		public GOVCBR5GULineMessageDataCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GOVCBR5GULineMessageData(Factory);
		protected override bool AllowNewCore => false;
	}
}
