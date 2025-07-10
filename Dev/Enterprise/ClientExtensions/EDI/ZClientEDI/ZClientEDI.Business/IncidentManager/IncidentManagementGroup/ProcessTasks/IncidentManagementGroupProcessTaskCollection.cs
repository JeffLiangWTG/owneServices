using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class IncidentManagementGroupProcessTaskCollection : ProcessTaskCollection
	{
		public IncidentManagementGroupProcessTaskCollection(IncidentManagementGroup parent) : base(parent)
		{
		}

		public new IncidentManagementGroupProcessTask this[int index]
		{
			get { return (IncidentManagementGroupProcessTask)Elements[index]; }
		}

		public virtual new IncidentManagementGroupProcessTask AddNew()
		{
			return (IncidentManagementGroupProcessTask)base.AddNew();
		}

		public new IncidentManagementGroup Parent
		{
			get { return (IncidentManagementGroup)base.Parent; }
		}
	}
}
