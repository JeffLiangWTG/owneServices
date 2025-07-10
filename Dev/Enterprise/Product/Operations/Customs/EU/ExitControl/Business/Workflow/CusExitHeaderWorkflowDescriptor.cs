using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderWorkflowDescriptor : WorkflowDescriptor, IWorkflowParentWithLines
	{
		public override string Code => WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("D4806700-8163-417B-86EA-6D7C3864A152", "Exit Control");
		public override Type WorkflowProviderType => typeof(CusExitHeader);
		public override ControllerID ControllerID => ControllerIDs.Customs.EU.ExitControl;
		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.INVALID };
		public override bool SupportsEventTracking => true;
		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;

		public override bool RequiresBranch => false;

		public override bool RequiresDepartment => false;

		public override bool RequiresClient => false;

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null) => new SchemaColumn[] { CusExitReportSchema.CER_MessageStatus, CusExitReportSchema.CER_Status };

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			switch (fieldColumn.Name)
			{
				case CusExitReportSchema.Constants.CER_MessageStatus:
					return Res.GetString("1017D75F-0B86-49A9-9A1F-CFC55CADCA4A", "Message Status");
				case CusExitReportSchema.Constants.CER_Status:
					return Res.GetString("25EF278C-C95F-452C-931F-0D2889B95C0E", "Status");
				default:
					return base.GetFieldColumnDescription(factory, fieldColumn);
			}
		}

		public IEnumerable<string> SupportedTriggerLineTypes => new string[] { TriggerLineTypes.Codes.CusExitReport };
	}
}
