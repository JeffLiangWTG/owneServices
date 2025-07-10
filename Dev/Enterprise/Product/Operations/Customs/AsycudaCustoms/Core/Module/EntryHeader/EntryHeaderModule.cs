using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	[CodeAlive("Referenced in ModuleRegistration")]
	public class EntryHeaderModule : Customs.Module.EntryHeaderModule
	{
		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, (EntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
