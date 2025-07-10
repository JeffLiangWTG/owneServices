using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Modules;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class PSQuotesWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description / ControllerID

		public override string Code
		{
			get { return EDIJobInvoicingConsumerTypes.PSQuote.Code; }
		}

		public override IMultilingualString Description
		{
			get { return EDIJobInvoicingConsumerTypes.PSQuote.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ClientControllerRegistration.ProfessionalServicesQuote; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(ProfessionalServicesQuote); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				ArrayList list = new ArrayList();
				list.AddRange(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType("Quotation Type", QuotationTypeList));
				list.Add(new ProcessTemplateSubType("Program Area", ProgramAreaList));
				list.Add(new ProcessTemplateSubType("Product", ProductList));
				return (ProcessTemplateSubType[])list.ToArray(typeof(ProcessTemplateSubType));
			}
		}

		#region Products

		CodeDescriptionPairList ProductList
		{
			get
			{
				if (fProductList == null)
				{
					fProductList = new EnterpriseModuleList();
				}

				return fProductList;
			}
		}

		CodeDescriptionPairList fProductList;

		#endregion

		#region Program Area

		CodeDescriptionPairList ProgramAreaList
		{
			get
			{
				if (fProgramAreaList == null)
				{
					fProgramAreaList = IncidentConstants.GetProgramAreaList();
				}

				return fProgramAreaList;
			}
		}

		CodeDescriptionPairList fProgramAreaList;

		#endregion

		#region Quotation Type

		CodeDescriptionPairList QuotationTypeList
		{
			get
			{
				if (fQuotationTypeList == null)
				{
					fQuotationTypeList = new CodeDescriptionPairList(new ProfessionalServicesQuoteLookups(null).WorkItemTypeList);
				}

				return fQuotationTypeList;
			}
		}

		CodeDescriptionPairList fQuotationTypeList;

		#endregion

		#endregion

		#region Requires Client / Ports

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override ZString Port1Name
		{
			get { return "Client Location"; }
		}

		public override bool RequiresPort2
		{
			get { return false; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return true; }
		}

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				ProfessionalServicesQuote quote = (ProfessionalServicesQuote)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(quote.Client, ZString.Empty));
			}
			else
			{
				base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			}
		}
	}
}

