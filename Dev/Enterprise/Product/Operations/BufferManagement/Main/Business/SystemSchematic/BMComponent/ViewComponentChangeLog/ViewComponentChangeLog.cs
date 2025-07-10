using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[DebuggerDisplay("Moved entity:[{ParentDescription}] {FromComponent.FC_Name}->{ToComponent.FC_Name} at {CCL_TransferTimeUtc} Mode:{CCL_TransferType} By:{CCL_GS_NKUser} Penetration %:{CCL_BufferPenetrationPercent} Zone: {CCL_BufferZone}")]
	public class ViewComponentChangeLog : AutoViewComponentChangeLog
	{
		public ViewComponentChangeLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		public override void Delete()
		{
			throw new NotSupportedException("This is a view on StmALog. Records should never be deleted.");
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ComponentChangeLogFetchStrategy(this);
		}

		class ComponentChangeLogFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			internal ComponentChangeLogFetchStrategy(ViewComponentChangeLog parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly ViewComponentChangeLog parent;

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);

				if (columns.Any(c => c.ColumnName == "ParentDescription"))
				{
					Factory.AddFetchHint(ProcessHeaderSchema.Constants.TableName, parent.CCL_ParentId);
				}
			}
		}

		#endregion

		#region Properties

		#region CCL_FC_ComponentFrom

		[List("Lookups.Components")]
		public override ZGuid CCL_FC_ComponentFrom
		{
			get { return base.CCL_FC_ComponentFrom; }
			set { base.CCL_FC_ComponentFrom = value; }
		}

		#endregion

		#region CCL_FC_ComponentTo

		[List("Lookups.Components")]
		public override ZGuid CCL_FC_ComponentTo
		{
			get { return base.CCL_FC_ComponentTo; }
			set { base.CCL_FC_ComponentTo = value; }
		}

		#endregion

		#region CCL_DeferReason

		[ResourceStringData("ViewComponentChangeLog.CCL_DeferralReason", Caption = "Deferral Reason")]
		public override ZString CCL_DeferReason
		{
			get { return base.CCL_DeferReason; }
			set { base.CCL_DeferReason = value; }
		}

		#endregion

		#endregion

		#region New Properties

		[ResourceStringData("ViewComponentChangeLog.ParentDescription", Caption = "Workflow")]
		public ZString ParentDescription
		{
			get { return Factory.Load<ProcessHeader>(CCL_ParentId)?.Description ?? ZString.Empty; }
		}

		[ResourceStringData("ViewComponentChangeLog.TransferTypeDescription", Caption = "Transfer Type Description", ShortCaption = "Transfer Type")]
		public ZString TransferTypeDescription
		{
			get { return Lookups.TransferTypes.GetDescriptionFromCode(CCL_TransferType); }
		}

		[ResourceStringData("ViewComponentChangeLog.CcrStatusDescription", Caption = "CCR Status Description", ShortCaption = "CCR Status")]
		public ZString CcrStatusDescription
		{
			get { return Lookups.CcrStatusCodes.GetDescriptionFromCode(CCL_CcrStatus); }
		}

		[ResourceStringData("ViewComponentChangeLog.WorkflowStatusDescription", Caption = "Workflow Status Description", ShortCaption = "Workflow Status")]
		public ZString WorkflowStatusDescription
		{
			get { return Lookups.WorkflowStatusCodes.GetDescriptionFromCode(CCL_WorkflowStatus); }
		}

		[ResourceStringData("ViewComponentChangeLog.TransferTimeLocal", Caption = "Transfer Time (Local)", ShortCaption = "Transfer Time")]
		public ZDateTime TransferTimeLocal
		{
			get { return CCL_TransferTimeUtc.ToLocalBranchTime(GlbBranch.GetCurrentBranch(Factory)); }
		}

		[ResourceStringData("ViewComponentChangeLog.DeferralReasonDescription", Caption = "Deferral Reason Description", ShortCaption = "Reason Description")]
		public ZString DeferralReasonDescription
		{
			get { return Lookups.DeferralReasonsList.GetDescriptionFromCode(CCL_DeferReason); }
		}

		#endregion

		#region Related Business Objects

		public BMComponent FromComponent => Factory.Load<BMComponent>(CCL_FC_ComponentFrom);
		public BMComponent ToComponent => Factory.Load<BMComponent>(CCL_FC_ComponentTo);

		#endregion

		#region Module Filters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module filter 'name'")]
		public static class ModuleFilterConstants
		{
			public const string FromComponent = "From Component";
			public const string ToComponent = "To Component";
			public const string Workflow = "Workflow";
			public const string TransferTime = "Transfer Time";
			public const string TransferType = "Transfer Type";
			public const string CCRStatus = "CCR Status";
			public const string WorkflowStatus = "Workflow Status";
			public const string User = "User";
			public const string BufferPenetration = "Buffer Penetration";
			public const string BufferZone = "Buffer Zone";
			public const string DeferralReason = "Deferral Reason";
		}

		#endregion
	}
}
