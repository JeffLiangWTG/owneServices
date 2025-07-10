using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class ClientBISIShipmentHeader : AutoClientBISIShipmentHeader
	{
		public ClientBISIShipmentHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void Delete()
		{
			base.Delete();
			Charges.RemoveAndDeleteAll();
		}

		#endregion

		#region Related Business Objects

		public ClientBISIShipmentChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ClientBISIShipmentChargeCollection(this);
					fCharges.Load();
					fCharges.IsManagedForDataRefresh = true;
				}
				return fCharges;
			}
		}
		ClientBISIShipmentChargeCollection fCharges;

		#endregion
	}
}
