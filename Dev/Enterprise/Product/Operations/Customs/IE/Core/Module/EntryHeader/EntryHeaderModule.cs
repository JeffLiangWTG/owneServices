using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.Module
{
	public class EntryHeaderModule : EU.Module.EntryHeaderModule
	{
		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
