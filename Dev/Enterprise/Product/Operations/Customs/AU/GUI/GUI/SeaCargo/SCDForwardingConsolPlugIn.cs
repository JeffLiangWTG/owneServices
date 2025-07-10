using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SCDForwardingConsolPlugIn : SCDForwardingPlugIn
	{
		public SCDForwardingConsolPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			consol = hostEntity as CommonConsol;
			if (consol != null)
			{
				originalUnpackDepot = consol.JK_OA_UnpackDepotAddress;
				consol.JK_OA_UnpackDepotAddressInfo.ValueChanged += new EventHandler(JK_OA_UnpackDepotAddressInfo_ValueChanged);
			}
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (consol != null)
				{
					consol.JK_OA_UnpackDepotAddressInfo.ValueChanged -= new EventHandler(JK_OA_UnpackDepotAddressInfo_ValueChanged);
				}
			}
		}

		#endregion
	}
}
