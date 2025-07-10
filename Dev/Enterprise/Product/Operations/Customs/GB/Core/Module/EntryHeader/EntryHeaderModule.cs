using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class EntryHeaderModule : EU.Module.EntryHeaderModule
	{
		public EntryHeaderModule()
		{
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new EntryHeaderController();

		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, (EntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
