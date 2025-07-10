using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public abstract class CommonNctsMessagingMenuItem : ZMenuItem
	{
		public NctsHeader NctsHeader
		{
			get { return nctsHeader; }
			set
			{
				if (nctsHeader != value)
				{
					if (nctsHeader != null)
					{
						nctsHeader.BH_HeaderTypeInfo.ValueChanged -= BH_HeaderTypeInfo_ValueChanged;
					}
					nctsHeader = value;
					if (nctsHeader != null)
					{
						nctsHeader.BH_HeaderTypeInfo.ValueChanged += BH_HeaderTypeInfo_ValueChanged;
					}
					SetupMessageSenderProvider();
				}
			}
		}
		NctsHeader nctsHeader;

		protected virtual void SetupMessageSenderProvider()
		{
		}

		protected void BH_HeaderTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupMessageSenderProvider();
		}
	}
}
