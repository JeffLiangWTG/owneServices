using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public override string Code { get { return WorkflowDescriptors.CollectionOrderCode; } }

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|AccCollectionOrderWorkflowDescriptor|Description", "Collection Order"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AccCollectionOrder); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AccCollectionOrder; }
		}

		#endregion

		#region Capapibilities

		public override bool RequiresClient { get { return false; } }

		public override bool RequiresBranch { get { return true; } }

		public override bool RequiresDepartment { get { return true; } }

		public override bool SupportsEventTracking { get { return true; } }

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance { get { return false; } }

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.CollectionOrder }; }
		}

		#endregion
	}
}
