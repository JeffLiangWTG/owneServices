using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoHAWBProviderContainerControl : ZUserControl
	{
		public AirCargoHAWBProviderContainerControl()
		{
		}

		public CusHAWBBase HAWB
		{
			get { return fHAWB; }
			set
			{
				fHAWB = value;
				if (ChildControl != null)
				{
					ChildControl.HAWB = value;
				}
			}
		}
		CusHAWBBase fHAWB;

		protected internal virtual AirCargoHAWBProviderContainerControl ChildControl => null;
	}
}
