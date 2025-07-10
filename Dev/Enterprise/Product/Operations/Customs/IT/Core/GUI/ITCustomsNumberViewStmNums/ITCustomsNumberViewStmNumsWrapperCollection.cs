using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.GUI;

public class ITCustomsNumberViewStmNumsWrapperCollection : CustomsNumberViewStmNumsCompanyWrapperCollection
{
	public ITCustomsNumberViewStmNumsWrapperCollection(CustomsNumberViewStmNumsCollection collection)
		: base(collection)
	{ }

	public new ITCustomsNumberViewStmNumsWrapper this[int i] => (ITCustomsNumberViewStmNumsWrapper)base[i];

	public new ITCustomsNumberViewStmNumsWrapper AddNew() => (ITCustomsNumberViewStmNumsWrapper)base.AddNew();
}
