using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business
{
	public class AccTransactionHeaderWithJobInfo : AutoClientAccTransactionHeaderWithJobInfo
	{
		public AccTransactionHeaderWithJobInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		public override void Delete()
		{
			ErrorReporter.ReportOnce("JASAccTransactionHeaderWithJobInfoDeleteIsNotSupported", "Delete is not supported for AccTransactionHeaderWithJobInfo BizO");
		}

		#region Properties

		#region AH_OSOutstandingAmount

		public override ZDecimal AH_OSOutstandingAmount
		{
			get
			{
				if (AH_IsOSOutstandingAmountApplicable && AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.Value)
				{
					return base.AH_OSOutstandingAmount;
				}

				if (Currency != null)
				{
					// Here we keep using old logic to calculate low-precision amount, because we don't have AH_LocalToal and relative properties in this view.
					return InvoiceUnpaid ? AH_OSTotal : TransactionHeaderOSOutstandingAmountProvider.GetLowPrecisionOSOutstandingAmount(AH_OutstandingAmount, AH_ExchangeRate, Currency.RX_Code);
				}

				return 0M;
			}
		}

		protected bool InvoiceUnpaid
		{
			get { return AH_InvoiceAmount + AH_GSTAmount == AH_OutstandingAmount; }
		}

		#endregion

		#region Currency

		public RefCurrency Currency
		{
			get { return Factory.Load<RefCurrency>(AH_RX); }
		}

		#endregion

		#region Header

		public OrgHeader Header
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), AH_OH); }
		}

		#endregion

		#region JASForwardingConsol

		public JASForwardingConsol Consol
		{
			get { return (JASForwardingConsol)Factory.Load(typeof(JASForwardingConsol), AH_JK); }
		}

		#endregion

		#region Shipment

		public JASForwardingShipment Shipment
		{
			get { return (JASForwardingShipment)Factory.Load(typeof(JASForwardingShipment), AH_JS); }
		}

		#endregion

		#endregion
	}
}
