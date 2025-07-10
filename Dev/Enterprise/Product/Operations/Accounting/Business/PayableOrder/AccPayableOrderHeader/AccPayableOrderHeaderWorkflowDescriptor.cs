using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public const string WorkflowTypeCode = WorkflowDescriptors.AccPayableOrderHeaderCode;

		public override string Code { get { return WorkflowTypeCode; } }

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Accounting|AccPayableOrderHeaderWorkflowDescriptor|Description", "Payable Order"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AccPayableOrderHeader); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AccPayableOrder; }
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
			get { return new[] { BusinessContext.PayableOrder }; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("8716c0b7-b412-46db-a8dd-352526d60015", "Order Type"), PayableOrderTypeList));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList PayableOrderTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("b9fbc0d4-eaf9-47bc-a12d-5f1f41ac1720", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.PayableOrderType));
				return result;
			}
		}

		#endregion
	}
}

