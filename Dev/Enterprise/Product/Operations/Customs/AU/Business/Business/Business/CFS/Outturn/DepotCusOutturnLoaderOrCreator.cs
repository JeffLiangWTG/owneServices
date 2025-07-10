using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal class DepotCusOutturnLoaderOrCreator
	{
		public DepotCusOutturnLoaderOrCreator(CFSShipment shipment, CusOutturnHeader header)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			if (header == null)
			{
				throw new ArgumentNullException(nameof(header));
			}

			this.shipment = shipment;
			this.header = header;
		}

		public DepotCusOutturn FindMatchingOutturn()
		{
			DepotCusOutturn result = null;
			ZQuery query = new ZQuery();
			query.AddToFilter(CusOutturnSchema.C5_CargoType, CMRImportCargoTypes.Codes.LessThanContainerLoad);
			query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, containerNumber);
			query.AddToFilter(CusOutturnSchema.C5_MasterBill, oceanBillNumber);
			query.AddToFilter(CusOutturnSchema.C5_HouseBill, houseBillNumber);

			BusinessObject[] outturns = header.Outturns.Find(query);
			if (outturns.Length == 1)
			{
				result = (DepotCusOutturn)outturns[0];
				if (!result.C5_ParentID.IsEmpty)
				{
					throw new FoundLineAlreadyLinked();
				}
			}

			return result;
		}

		public DepotCusOutturn CreateOutturn()
		{
			DepotCusOutturn outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			outturn.C5_ContainerNumber = containerNumber;
			outturn.C5_MasterBill = oceanBillNumber;
			outturn.C5_HouseBill = houseBillNumber;
			outturn.C5_OuterPacks = shipment.JS_OuterPacks;
			return outturn;
		}

		#region Proxy helpers

		ZString containerNumber
		{
			get { return shipment.ParentContainerRegistration == null ? ZString.Empty : shipment.ParentContainerRegistration.JC_ContainerNum; }
		}

		ZString oceanBillNumber
		{
			get { return shipment.ParentContainerRegistration == null || shipment.ParentContainerRegistration.Consol == null ? ZString.Empty : shipment.ParentContainerRegistration.Consol.JK_MasterBillNum; }
		}

		ZString houseBillNumber
		{
			get { return shipment.JS_HouseBill; }
		}

		#endregion

		#region Exceptions

		[Serializable]
		public class FoundLineAlreadyLinked : ApplicationException
		{
			public FoundLineAlreadyLinked()
				: base()
			{
			}

#if NETFRAMEWORK
			protected FoundLineAlreadyLinked(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{ }
#endif
		}

		#endregion

		#region Implementation

		readonly CFSShipment shipment;
		readonly CusOutturnHeader header;

		#endregion
	}
}
