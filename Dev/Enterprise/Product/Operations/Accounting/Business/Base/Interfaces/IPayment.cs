
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IPayment : IPayablesAndReceivables
	{
	}
}

#region Test Payment
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Unmatching;

	public class TestIPayment : TestIPayablesAndReceivables, IPayment, IUnmatchOnReversing
	{
		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZDateTime IMatching.PaymentRequestedDate { get { return ZDateTime.Empty; } }
		ZString IMatching.VoyageVesselOrFlightDate { get { return ZString.Empty; } }
		ZString IMatching.ShipmentHouseBill { get { return ZString.Empty; } }
		ZString IMatching.ShipmentMasterBill { get { return ZString.Empty; } }
		bool IMatching.ChequeOrReference_ReadOnly { get; set; }
		ZGuid IMatching.DisplayInvoiceAddressOverride { get { return ZGuid.Empty; } }
		ZPropertyInfo IMatching.DisplayInvoiceAddressOverrideInfo { get { return null; } }

		public IUnmatchingData UnmatchingData
		{
			get { return new UnmatchingDataProvider(this); }
		}
	}
}
#endif
#endregion