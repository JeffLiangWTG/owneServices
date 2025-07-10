using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business
{
	public class TSTCustomsNumberViewStmNumsWrapperCollection : CustomsNumberViewStmNumsWrapperCollection
	{
		public TSTCustomsNumberViewStmNumsWrapperCollection(CustomsNumberViewStmNumsCollection collection) : base(collection)
		{
		}

		public new TSTCustomsNumberViewStmNumsWrapper this[int i] => (TSTCustomsNumberViewStmNumsWrapper)base[i];

		public new TSTCustomsNumberViewStmNumsWrapper AddNew() => (TSTCustomsNumberViewStmNumsWrapper)base.AddNew();
	}
}
