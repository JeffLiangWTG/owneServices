using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestItem : AutoAccCommissionApprovalRequestItem, ISelectableViewCommissionLineProvider
	{
		public AccCommissionApprovalRequestItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region CRI_CL0

		[RelatedBusinessObject("CommissionLine")]
		[List("Lookups.CommissionLines")]
		public override ZGuid CRI_CL0
		{
			get { return base.CRI_CL0; }
			set { base.CRI_CL0 = value; }
		}

		public virtual AccCommissionLine CommissionLine
		{
			get { return Factory.Load<AccCommissionLine>(CRI_CL0); }
		}

		public ViewCommissionLine ViewCommissionLine
		{
			get { return Factory.Load<ViewCommissionLine>(CRI_CL0); }
		}

		#endregion

		#region CRI_CRQ

		[RelatedBusinessObject("CommissionApprovalRequest")]
		[List("Lookups.CommissionApprovalRequests")]
		public override ZGuid CRI_CRQ
		{
			get { return base.CRI_CRQ; }
			set { base.CRI_CRQ = value; }
		}

		public virtual AccCommissionApprovalRequest CommissionApprovalRequest
		{
			get { return Factory.Load<AccCommissionApprovalRequest>(CRI_CRQ); }
		}

		#endregion

		#region CRI_IsSelected

		protected internal bool CRI_IsSelected_ReadOnly
		{
			get
			{
				var approvalRequest = CommissionApprovalRequest;
				if (approvalRequest == null)
				{
					return true;
				}

				return approvalRequest.HasAtLeastOneStaffWhoHasApproved;
			}
		}

		#endregion

		#endregion

		#region ISelectableViewCommissionLineProvider Members

		ZBool ISelectableViewCommissionLineProvider.IsSelected
		{
			get { return CRI_IsSelected; }
			set { CRI_IsSelected = value; }
		}

		ZPropertyInfo ISelectableViewCommissionLineProvider.IsSelectedInfo
		{
			get { return CRI_IsSelectedInfo; }
		}

		#endregion
	}
}
