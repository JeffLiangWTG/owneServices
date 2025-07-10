using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class TradeChainPartnerSendingObjectValidation : ZValidation
	{
		public TradeChainPartnerSendingObjectValidation(TradeChainPartnerSendingObject parent) : base(parent)
		{
			this.parent = parent;
			this.parentListInternals = parent;
		}

		public override Type AutoValidationType
		{
			get
			{
				return typeof(TradeChainPartnerSendingObjectValidation);
			}
		}

		public override void ValidateAll()
		{
			IDisposable suspender = parentListInternals.SuspendListChanged();
			try
			{
			}
			finally
			{
				suspender.Dispose();
			}
		}

		public void ValidateCA_Type()
		{
			TCPAddInfoValidation.ValidateCA_Type();
		}

		public void ValidateCA_CSAIDType()
		{
			TCPAddInfoValidation.ValidateCA_CSAIDType();
		}

		TradeChainPartnerAddInfoValidation TCPAddInfoValidation => parent.TradeChainPartner.AddInfoValidation;

		public TradeChainPartnerSendingObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly TradeChainPartnerSendingObject parent;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal parentListInternals;
	}
}
