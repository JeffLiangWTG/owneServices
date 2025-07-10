using System.Linq;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionFinalizerLineItemValidation : AutoCommissionFinalizerLineItemValidation
	{
		public CommissionFinalizerLineItemValidation(AutoCommissionFinalizerLineItem parent)
			: base(parent)
		{
		}

		public bool ShouldStopApprovalRequest
		{
			get
			{
				if (!Parent.HasNotifications())
				{
					return false;
				}

				var finalizerItem = Parent as CommissionFinalizerLineItem;
				if (finalizerItem != null)
				{
					return finalizerItem.ViewCommissionLine.PropertiesWithNotifications.Any(x => x.Name != ViewCommissionLine.Schema.HasFullyPaid) || finalizerItem.ViewCommissionLine.HasRowNotifications || finalizerItem.HasRowNotifications;
				}

				return false;
			}
		}
	}
}
