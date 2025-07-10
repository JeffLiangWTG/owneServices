using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Module
{
	public class EntryHeaderModule : Customs.Module.EntryHeaderModule
	{
		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, FilterBusinessObject);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new EntryHeaderController();

		public override ZBool HasActions => true;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
