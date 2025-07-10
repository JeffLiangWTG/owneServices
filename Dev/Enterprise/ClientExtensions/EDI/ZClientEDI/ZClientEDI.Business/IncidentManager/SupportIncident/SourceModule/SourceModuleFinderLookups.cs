using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SourceModuleFinderLookups : ZLookups
	{
		public SourceModuleFinderLookups(SourceModuleFinder sourceModuleFinder)
			: base(sourceModuleFinder)
		{
			incidentLookups = new SupportIncidentLookups(sourceModuleFinder.Factory);
		}

		new SourceModuleFinder Parent
		{
			get { return (SourceModuleFinder)base.Parent; }
		}

		readonly SupportIncidentLookups incidentLookups;

		public ICodeDescriptionPairList ModuleFilterList
		{
			get { return incidentLookups.GetModuleList(Parent.ModuleType, Parent.ProductCode, Parent.ProductAreaFilter); }
		}

		public ICodeDescriptionPairList ProductAreaList
		{
			get { return SupportIncidentLookups.GetProductAreaList(); }
		}
	}
}

