using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWise.EntityFramework;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ScheduleB3MessageSupporter : Integration.Customs.CA.IScheduleB3MessageSupporter
	{
		public ScheduleB3MessageSupporter(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly JobDeclaration declaration;

		#region IScheduleB3MessageSupporter Members

		public bool SupportScheduleB3Message
		{
			get { return declaration.JE_MessageType == JobMessageTypeList.Codes.Import || declaration.JE_MessageType == JobMessageTypeList.Codes.LowValueShipments; }
		}

		public string NotSupportScheduleB3MessageReason
		{
			get
			{
				return SupportScheduleB3Message ? string.Empty : Res.GetString("dda74c85-cd91-4c68-91f5-60788807f0a7", "Trigger action Schedule CAD Message is valid only for IMP or LVS declaration.");
			}
		}

		public IProcessor CreateScheduleB3MessageProcessor()
		{
			return new ScheduleCADMessageProcessor(declaration);
		}

		#endregion

		#region IBaseAutoSendingMessageSupporter

		IProcessor Integration.Customs.IBaseAutoSendingMessageSupporter.CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode)
		{
			return new Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor(declaration, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);
		}

		Guid Integration.Customs.IBaseAutoSendingMessageSupporter.RegistryBranchPK
		{
			get
			{
				var branch = declaration.Branch;
				return branch == null ? GlbBranch.CurrentBranch.PK.ToGuid() : branch.PK.ToGuid();
			}
		}

		#endregion
	}
}
