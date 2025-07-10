using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5GVLineMessageData : NonPersistentBusinessObject
	{
		public GOVCBR5GVLineMessageData(BusinessObjectFactory factory) : base(factory)
		{
			this.list = factory.GetCachedValue<ImportAmendmentDataItemIDList>();
		}

		readonly ImportAmendmentDataItemIDList list;
		public ZShort FirstLineNo { get; set; }
		public ZString FirstLineDataItemID { get; set; }
		public ZString FirstLineDataItemIDDescription => list.GetDescriptionFromCode(FirstLineDataItemID);
		public ZShort SecondLineNo { get; set; }
		public ZString SecondLineDataItemID { get; set; }
		public ZString SecondLineDataItemIDDescription => list.GetDescriptionFromCode(SecondLineDataItemID);
	}

	public class GOVCBR5GVLineMessageDataCollection : NonPersistentBusinessObjectCollection<GOVCBR5GVLineMessageData>
	{
		public GOVCBR5GVLineMessageDataCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GOVCBR5GVLineMessageData(Factory);
		protected override bool AllowNewCore => false;
	}
}
