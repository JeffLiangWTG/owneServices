using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentReleaseGroupLink : AutoBMComponentReleaseGroupLink,
		IBMComponentReleaseGroupLink,
		IAuditParent
	{
		public BMComponentReleaseGroupLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region FO_AutoAssignTasksAge

		[ZDateTimeDurationValue]
		[ReadOnlyMember(nameof(AutoAssignTaskUnavailableComponent))]
		public override ZDateTime FO_AutoAssignTasksAge
		{
			get { return base.FO_AutoAssignTasksAge; }
			set { base.FO_AutoAssignTasksAge = value.ConvertToDurationBasedDate(FO_AutoAssignTasksAgeInfo); }
		}

		protected ZBool AutoAssignTaskUnavailableComponent
		{
			get { return !Component.IsBuffer; }
		}

		#endregion

		#region FO_FC_Component

		[RelatedBusinessObject("Component")]
		public override ZGuid FO_FC_Component
		{
			get { return base.FO_FC_Component; }
			set { base.FO_FC_Component = value; }
		}

		#endregion

		#region FO_ReleaseGateMode

		[List("Lookups.ReleaseGateModeList")]
		public override ZString FO_ReleaseGateMode { get => base.FO_ReleaseGateMode; set => base.FO_ReleaseGateMode = value; }

		#endregion

		#endregion

		#region IBMComponentReleaseGroupLink

		IBMComponent IBMComponentReleaseGroupLink.Component => Component;

		IGlbGroup IBMComponentReleaseGroupLink.ReleaseGroup => ReleaseGroup;

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion

		#region BusinessObject Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Related Business Objects

		public BMComponent Component
		{
			get { return Factory.Load<BMComponent>(FO_FC_Component); }
		}

		#endregion
	}
}
