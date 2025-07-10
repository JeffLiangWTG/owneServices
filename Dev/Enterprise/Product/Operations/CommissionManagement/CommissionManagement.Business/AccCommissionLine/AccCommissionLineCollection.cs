using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionLineCollection : ActiveBusinessObjectCollection<AccCommissionLine>
	{
		public AccCommissionLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccCommissionLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccCommissionLineCollection(AccCommissionHeader header)
			: base(header.Factory, header, new ZQuery(), AccCommissionLineSchema.CL0_ParentID)
		{
		}

		public AccCommissionLineCollection(AccCommissionLineGroup lineGroup)
			: base(lineGroup.Factory, lineGroup, new ZQuery(), AccCommissionLineSchema.CL0_ParentID)
		{
		}

		public AccCommissionLineCollection(AccCommissionApprovalRequest approvalRequest)
			: base(approvalRequest, typeof(AccCommissionApprovalRequestItem), new ZQuery(), AccCommissionApprovalRequestItemSchema.CRI_CRQ, AccCommissionApprovalRequestItemSchema.CRI_CL0)
		{
		}

		#region New

		protected override bool AllowNew
		{
			get { return base.AllowNew && Relationship.Master != null; }
		}

		#endregion
	}
}
