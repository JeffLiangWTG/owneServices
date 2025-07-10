using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class FindTriageFilterHelperLookups : ZLookups
	{
		public FindTriageFilterHelperLookups(FindTriageFilterHelper parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Types => new IncidentTriageTypes();

		#region Product

		public CodeDescriptionPairList ProductList
		{
			get
			{
				return Factory.GetCachedValue("FindTriageFilterHelperLookups.ProductList",
					() =>
					{
						return IncidentDetailsLookupsHelper.ProductList;
					});
			}
		}

		#endregion

		#region Product Area

		public CodeDescriptionPairList ProductAreaList
		{
			get
			{
				return Factory.GetCachedValue("FindTriageFilterHelperLookups.ProductAreaList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(EDIDataRegistry.Instance.ProductAreas.Value);
						return result;
					});
			}
		}

		#endregion
	}
}
