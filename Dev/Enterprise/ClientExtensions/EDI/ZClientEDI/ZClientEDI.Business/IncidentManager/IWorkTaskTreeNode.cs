using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IWorkTaskTreeNode : IWorkTaskRelatedItem, IAuditDetails
	{
		BusinessObjectCollection ChildrenOnlyRelatedItems { get; }

		BusinessObjectCollection ParentsOnlyRelatedItems { get; }

		ZDateTime AgreedDeliveryDate { get; }

		ZString CurrentTaskStatus { get; }

		ZString CurrentTaskDescription { get; }

		ZString CurrentTaskCapabilityCodeDescription { get; }

		ZString CurrentTaskAssigned { get; }

		ZString SelectionCriterion1Code { get; }

		ZString SelectionCriterion2Code { get; }

		ZString SelectionCriterion3Code { get; }

		ZString SelectionCriterion4Code { get; }

		ZString SelectionCriterion5Code { get; }

		void AddFetchHintsForOrgAddressIfRequired();

		void AddFetchHintsForOrgHeaderIfRequired();
	}
}
