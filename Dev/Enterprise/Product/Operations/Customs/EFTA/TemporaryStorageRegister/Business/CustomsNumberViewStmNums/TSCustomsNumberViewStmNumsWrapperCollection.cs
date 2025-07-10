using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class TSCustomsNumberViewStmNumsWrapperCollection : CustomsNumberViewStmNumsWrapperCollection
{
	public TSCustomsNumberViewStmNumsWrapperCollection(CustomsNumberViewStmNumsCollection collection) : base(collection) { }

	public new TSCustomsNumberViewStmNumsWrapper this[int i] => (TSCustomsNumberViewStmNumsWrapper)base[i];

	public new TSCustomsNumberViewStmNumsWrapper AddNew() => (TSCustomsNumberViewStmNumsWrapper)base.AddNew();
}
