using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRWorkflowDescriptor : WorkflowDescriptor
	{
		public static class Constants
		{
			public const string Code = "JPA";
			public static IMultilingualString Description { get { return ResString.GetMultilingualString("{4B6064C5-2FC5-4DBF-B40B-B1B003EC9ADA}", "JP AFR"); } }
		}

		public override string Code
		{
			get { return Constants.Code; }
		}

		public override IMultilingualString Description
		{
			get { return Constants.Description; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.JP.AFR; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(JPAFRHeader); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}

		public override ZString ClientName
		{
			get { return "Carrier"; }
		}

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}
	}
}
