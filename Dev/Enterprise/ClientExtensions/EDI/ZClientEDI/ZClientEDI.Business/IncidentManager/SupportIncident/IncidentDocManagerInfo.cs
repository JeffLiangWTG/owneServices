using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentDocManagerInfo : DocManagerInfo
	{
		public IncidentDocManagerInfo(EdiIncidentRequest parent)
			: base(parent, IncidentConstants.SupportIncidentDocManagerCode)
		{
			Request = parent;
		}

		readonly EdiIncidentRequest Request;

		protected override BusinessObject[] GetRelatedObjects()
		{
			return Request.RelatedSupportIncident?.RelatedItems.Cast<BusinessObject>().ToArray()
				?? base.GetRelatedObjects();
		}
	}
}

