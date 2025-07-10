using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ShipmentConsolAndMasterBillNumbers
{
	[TestedAsNonPersistentBusinessObject]
	public class ShipmentConsolAndMasterBillNumbers : AutoViewShipmentConsolAndMasterBillNumbers
	{
		public ShipmentConsolAndMasterBillNumbers(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Delete

		public override void Delete()
		{
			throw new NotSupportedException("Deletion of the ShipmentConsolAndMasterBillNumbers is not supported.");
		}

		#endregion

	}
}

