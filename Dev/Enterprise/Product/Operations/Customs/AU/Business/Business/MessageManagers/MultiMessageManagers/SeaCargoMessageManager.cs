using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class SeaCargoMessageManager : MultiMessageManager
	{
		protected SeaCargoMessageManager(CusSCAOceanBill oceanBill, ZString ownerCompanyABN)
			: base()
		{
			OceanBill = oceanBill;
			OwnerCompanyABN = ownerCompanyABN;
		}

		public readonly ZString OwnerCompanyABN;
		public CusSCAOceanBill OceanBill { get; set; }

		public override ZString MessagingApplicationName
		{
			get
			{
				return "Sea Cargo";
			}
		}

		#region Implementation

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override bool CanSendOriginal(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend)
		{
			return base.CanSendOriginal(sender, managersToSend) && OceanBillDetailsValid(sender);
		}

		protected override bool CanWithdraw(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managers)
		{
			return base.CanWithdraw(sender, managers) && OceanBillDetailsValid(sender);
		}

		protected override bool CanAmend(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managers)
		{
			return base.CanAmend(sender, managers) && OceanBillDetailsValid(sender);
		}

		internal bool OceanBillDetailsValid(Customs.Business.ISendsMessagesToCustoms sender)
		{
			var valid = false;
			if (OceanBill != null)
			{
				valid = !OceanBill.Notifications.HasMessageErrors();
				if (!valid)
				{
					sender.NotifyUserOfAnInvalidOperation(OceanBill.Notifications.GetMessageErrors().ToUniqueMessageListString());
				}
			}
			return valid;
		}

		#endregion
	}
}
