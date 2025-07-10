using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class SupportIncidentAttacherForTest : SupportIncidentAttacher
	{
		public SupportIncidentAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, bool isWorkItem) : base(destinationCollection, findBoxList, moduleID, isWorkItem)
		{
		}

		public bool TestAttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}

		public void AttachItemsCore_Exposed(IBusinessObjectCollection destinationCollection, IEnumerable<BusinessObject> list)
		{
			base.AttachItemsCore(destinationCollection, list);
		}
	}
}
