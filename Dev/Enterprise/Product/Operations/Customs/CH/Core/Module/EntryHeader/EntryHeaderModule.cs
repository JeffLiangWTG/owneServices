using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CH.Module;

public class EntryHeaderModule : Customs.Module.EntryHeaderModule
{
	public override ZBool HasActions => true;

	public override bool AllowNew => false;

	public override bool AllowDelete => false;

	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new EntryHeaderController();

	protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, (EntryHeaderFilterBusinessObject)FilterBusinessObject);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();
}
