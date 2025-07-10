using System.Windows.Forms;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI
{
	public class PayableOrderSecurityOverrideProvider : InteractiveSecurityOverrideProvider
	{
		public PayableOrderSecurityOverrideProvider(AccPayableOrderHeader payableOrderHeader)
		{
			PayableOrderHeader = payableOrderHeader;
		}

		#region Overrides

		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			SecurityCore security = null;
			while (true)
			{
				security = base.RequestLoginCredentials(checkPoint);

				if (LastLoginFormResult != DialogResult.OK)
				{
					break;
				}

				if (security != null)
				{
					if (PayableOrderHeader.APH_Calc_CreatedUser == security.UserPK
							&& PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.CreatorApproval))
					{
						Globals.Message.Show(Res.GetString("b58b529a-3f6f-44a5-a097-04bf5b6066e9", "Order approval by initiator is disallowed in the registry. The approver has to be a different user than the initiator."));
					}
					else
					{
						break;
					}
				}
			}

			return security;
		}

		protected override string GetSecurityOverrideMessage(SecurityCheckpoint checkPoint)
		{
			if (UserInitiatorAndNotAllowedToApprove)
			{
				if (checkPoint == Env.Security.PayableOrderApprovalFirstLevelApproval ||
					checkPoint == Env.Security.PayableOrderApprovalSecondLevelApproval ||
					checkPoint == Env.Security.PayableOrderApprovalThirdLevelApproval)
				{
					return Res.GetString("77174e8f-dfe0-4dc3-ad7f-e50d1cf6c368", @"Order approval by initiator is disallowed in the registry.
This transaction must be approved by a user with {0} authority.
If a user with this level of authority enters their username and password below you may continue.
Otherwise, hit cancel to continue without approving this order.", checkPoint.DisplayText);
				}
				else
				{
					return Res.GetString("d3B0a2a2-52c2-40b4-8596-066fa864c9ba", @"Order approval by initiator is disallowed in the registry.
This transaction does not require any approval authorization level.
However, this transaction must be approved by another user.
Otherwise, hit cancel to continue without approving this order.");
				}
			}
			else
			{
				return Res.GetString("9ac657db-0f58-4bdc-ab7f-a79aec90d023", @"This transaction must be approved by a user with {0} authority.
If a user with this level of authority enters their username and password below you may continue.
Otherwise, hit cancel to continue without approving this order.", checkPoint.DisplayText);
			}
		}

		protected override bool UserInitiatorAndNotAllowedToApprove
		{
			get
			{
				return PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.CreatorApproval)
								&& Env.CurrentUser.PK == PayableOrderHeader.APH_Calc_CreatedUser;
			}
		}

		#endregion

		readonly AccPayableOrderHeader PayableOrderHeader;
	}
}
