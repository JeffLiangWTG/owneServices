using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class NewWorkItemConvertedFromJiraIssue : WorkItemConvertedFromJiraIssue
	{
		public NewWorkItemConvertedFromJiraIssue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override INumberFountainProxy JobNumberFountain
		{
			get { return Modules.ClientNumberFountainRegistration.GetInstance().NewWorkItemNo; }
		}
	}
}
