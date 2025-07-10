using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.SWL.Business
{
	public class ShipnetARInvoice : ARInvoice
	{
		public ShipnetARInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsValidShipnetType
		{
			get { return IsAccountsReceivable && IsCorrectTransactionType && ShipmentData != null && ShipmentData.JS_IsShipping; }
		}

		public bool IsAccountsReceivable
		{
			get { return (AH_Ledger == LedgerTypes.AccountsReceivable); }
		}

		public bool IsCorrectTransactionType
		{
			get
			{
				return AH_TransactionType == TransactionTypes.Invoice
					|| AH_TransactionType == TransactionTypes.CreditNote;
			}
		}

		public AgencyShipment ShipmentData
		{
			get
			{
				JobHeader header = Job;
				return header == null ? null : Factory.Load<AgencyShipment>(header.JH_ParentID);
			}
		}

		public JobSailing SailingData
		{
			get
			{
				AgencyShipment shipment = ShipmentData;
				return shipment == null ? null : shipment.Sailing;
			}
		}

		public OrgHeader Carrier
		{
			get
			{
				AgencyShipment shipment = ShipmentData;
				return shipment == null ? null : shipment.Principal;
			}
		}

		public ZString OceanBill
		{
			get
			{
				AgencyShipment shipment = ShipmentData;
				return shipment == null ? ZString.Empty : shipment.JS_HouseBill;
			}
		}

		public RefVessel Vessel
		{
			get
			{
				JobSailing sailing = SailingData;
				return sailing == null ? null : sailing.Voyage.Vessel;
			}
		}

		public ZString Voyage
		{
			get
			{
				JobSailing sailing = SailingData;
				return sailing == null ? ZString.Empty : sailing.JX_JV_VoyageFlight;
			}
		}
	}
}
