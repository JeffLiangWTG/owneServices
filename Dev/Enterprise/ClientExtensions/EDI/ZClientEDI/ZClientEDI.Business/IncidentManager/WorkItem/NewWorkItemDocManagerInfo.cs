using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class NewWorkItemDocManagerInfo : DocManagerInfo
	{
		public NewWorkItemDocManagerInfo(NewWorkItem parent, ZString cocManagerCode)
			: base(parent, cocManagerCode)
		{ }

		protected override BusinessObject[] GetRelatedObjects()
		{
			NewWorkItem workItem = (NewWorkItem)BusinessEntity;
			return workItem.RelatedItems.Cast<BusinessObject>()
				.Select(x => (BusinessObject)((x as IEDocsPluginHostDecider)?.HostBusinessEntity) ?? x)
				.ToArray();
		}
	}
}

