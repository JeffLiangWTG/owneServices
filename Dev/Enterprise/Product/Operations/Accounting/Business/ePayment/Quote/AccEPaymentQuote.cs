using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business
{
	[UniversalDataContext(DataContextType.AccEPaymentQuote)]
	public class AccEPaymentQuote : AutoAccEPaymentQuote, IEPaymentDeliveryContextValueProvider, IEPaymentLogParent
	{
		public AccEPaymentQuote(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[DecimalPlaces(nameof(FromRXDecimals))]
		public override ZDecimal QU_FromAmount { get => base.QU_FromAmount; set => base.QU_FromAmount = value; }

		[DecimalPlaces(nameof(ToRXDecimals))]
		public override ZDecimal QU_ToAmount { get => base.QU_ToAmount; set => base.QU_ToAmount = value; }

		[DecimalPlaces(nameof(FeeRXDecimals))]
		public override ZDecimal QU_FeeAmount { get => base.QU_FeeAmount; set => base.QU_FeeAmount = value; }

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal QU_ExchangeRate { get => base.QU_ExchangeRate; set => base.QU_ExchangeRate = value; }

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal QU_ExchangeRateInverted { get => base.QU_ExchangeRateInverted; set => base.QU_ExchangeRateInverted = value; }

		[List("Lookups.StatusCodeList")]
		public override ZString QU_Status { get => base.QU_Status; set => base.QU_Status = value; }

		public ZString StatusDescription => $"{QU_Status} - {Lookups.StatusDescriptionCodeList[QU_Status].Description}";

		[List("Lookups.ProviderCodeList")]
		public override ZString QU_ProviderCode { get => base.QU_ProviderCode; set => base.QU_ProviderCode = value; }

		public GlbStaff CreatingUser => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, QU_SystemCreateUser);

		public bool HasReceivedValidResponseFromProvider => new ZString[] { StatusCodes.Received, StatusCodes.Accepted, StatusCodes.Discarded, StatusCodes.Expired }.Contains(QU_Status);

		[List("Lookups.OrgHeaders")]
		public ZGuid OrganisationPK => PaymentApproval?.AV_OH ?? ZGuid.Empty;
		public OrgHeader Organisation => PaymentApproval?.Header;

		public int FromRXDecimals => FromCurrency != null ? FromCurrency.Decimals : LocalRXDecimals;
		public int ToRXDecimals => ToCurrency != null ? ToCurrency.Decimals : LocalRXDecimals;
		// Fee's decimal will be set to the first non-null value in the order of FeeCurrency -> FromCurrency -> LocalRXDecimals
		public int FeeRXDecimals => (FeeCurrency?.Decimals ?? FromCurrency?.Decimals) ?? LocalRXDecimals;
		public int LocalRXDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;
		public int ExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#region IEPaymentDeliveryContextValueProvider Memebers

		ZString IEPaymentDeliveryContextValueProvider.Purpose => $"FX Quote Request {QU_InternalReference} Sent to {QU_ProviderCode}";
		EntityInfo IEPaymentDeliveryContextValueProvider.EntityInfo => EntityInfo.New(this);

		PaymentApprovalBase IEPaymentLogParent.PaymentApprovalForLogging
		{
			get { return Factory.Load<PaymentApprovalBase>(QU_AV); }
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var approval = Factory.NewWithValidTestData<AccPaymentApproval>();
			approval.AV_Ledger = "AP";
			QU_AV = approval.PK;
			QU_GC = GlbCompany.CurrentCompany.PK;
			QU_FromAmount = 1;
			QU_RX_NKFromCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			QU_RX_NKToCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			QU_InternalReference = "AAA";
			QU_ProviderCode = ProviderCodes.OFX;
		}
#endif
	}
}
