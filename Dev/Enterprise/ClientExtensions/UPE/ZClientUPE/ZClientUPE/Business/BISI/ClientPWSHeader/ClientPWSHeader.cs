using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class ClientPWSHeader : AutoClientPWSHeader
	{
		public ClientPWSHeader(BusinessObjectFactory factory, DataRow row)
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

		public ClientPWSChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ClientPWSChargeCollection(this);
					fCharges.Load();
					fCharges.IsManagedForDataRefresh = true;
				}
				return fCharges;
			}
		}
		ClientPWSChargeCollection fCharges;

		#endregion
	}
}
