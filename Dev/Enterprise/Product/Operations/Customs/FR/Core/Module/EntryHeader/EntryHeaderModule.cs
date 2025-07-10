using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Module
{
	public class EntryHeaderModule : EU.Module.EntryHeaderModule
	{
		public EntryHeaderModule()
		{
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new EntryHeaderController();

		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, (EntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();

		public override ZBool HasActions => true;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		protected override Customs.Module.EntryHeaderOperationalActionSupporter GetNewOperationalActionSupporter() => new EntryHeaderOperationalActionSupporter();
	}
}
