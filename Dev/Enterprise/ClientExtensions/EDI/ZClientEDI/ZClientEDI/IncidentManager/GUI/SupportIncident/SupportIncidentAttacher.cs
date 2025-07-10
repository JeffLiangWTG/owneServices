using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class SupportIncidentAttacher : ProcessManagement.GUI.WorkTaskAttacher
	{
		readonly bool isAttachingToWorkItem;

		public SupportIncidentAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, bool isAttachingToWorkItem)
			: base(destinationCollection, findBoxList, moduleID)
		{
			this.isAttachingToWorkItem = isAttachingToWorkItem;
		}

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var factory = new BusinessObjectFactory();
			var supportIncident = factory.Load<SupportIncident>(bizO.PK);
			if (!supportIncident.CanCreateWorkItem)
			{
				Globals.Message.Show(Enterprise.Client.EDI.IncidentManager.GUI.ModuleSelectionControl.YouCannotCreateWorkItem);
				return true;
			}
			if (isAttachingToWorkItem && bizO is SupportIncident incident)
			{
				incident.ShouldAddRelatedItemAsParent = true;
			}
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
