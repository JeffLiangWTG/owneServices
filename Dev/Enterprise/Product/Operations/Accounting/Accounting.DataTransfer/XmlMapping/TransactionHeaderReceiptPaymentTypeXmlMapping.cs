using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.XmlMapping
{
	[Immutable]
	[ImmutableObject(true)]
	public class TxnHeaderReceiptPaymentTypeXmlMapping : EnterpriseCodeExternalCodeMappings
	{
		protected TxnHeaderReceiptPaymentTypeXmlMapping()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.Cash, nameof(Xsd.TxnHeaderReceiptPaymentType.CSH));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.Cheque, nameof(Xsd.TxnHeaderReceiptPaymentType.CHQ));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.CreditCard, nameof(Xsd.TxnHeaderReceiptPaymentType.CCD));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.DirectCredit, nameof(Xsd.TxnHeaderReceiptPaymentType.DCR));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.DirectDebit, nameof(Xsd.TxnHeaderReceiptPaymentType.DDR));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.EFT, nameof(Xsd.TxnHeaderReceiptPaymentType.EFT));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.ScheduledEFT, nameof(Xsd.TxnHeaderReceiptPaymentType.SFT));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.CollectionRequest, nameof(Xsd.TxnHeaderReceiptPaymentType.CRQ));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.DirectDebitLine, nameof(Xsd.TxnHeaderReceiptPaymentType.DDL));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.NonRolledUpBatch, nameof(Xsd.TxnHeaderReceiptPaymentType.NRB));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee, nameof(Xsd.TxnHeaderReceiptPaymentType.AMF));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.BankDebitTax, nameof(Xsd.TxnHeaderReceiptPaymentType.BDT));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.BankDepositFee, nameof(Xsd.TxnHeaderReceiptPaymentType.BDP));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.InterestPaid, nameof(Xsd.TxnHeaderReceiptPaymentType.INT));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.InterestReceived, nameof(Xsd.TxnHeaderReceiptPaymentType.INR));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.PeriodicPayment, nameof(Xsd.TxnHeaderReceiptPaymentType.PPY));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.StampDuty, nameof(Xsd.TxnHeaderReceiptPaymentType.STD));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.MiscellaneousFees, nameof(Xsd.TxnHeaderReceiptPaymentType.MSF));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.MiscellaneousReceipt, nameof(Xsd.TxnHeaderReceiptPaymentType.MSR));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.eNettDirectDebit, nameof(Xsd.TxnHeaderReceiptPaymentType.END));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.eNettDirectCredit, nameof(Xsd.TxnHeaderReceiptPaymentType.ENC));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.eNettCreditCard, nameof(Xsd.TxnHeaderReceiptPaymentType.ECC));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.eNettDirectDebitForeignCurrency, nameof(Xsd.TxnHeaderReceiptPaymentType.EDF));
			yield return new Mapping(ZArchitecture.Core.ReceiptTypes.EPayment, nameof(Xsd.TxnHeaderReceiptPaymentType.EPA));
		}

		public new Xsd.TxnHeaderReceiptPaymentType GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.TxnHeaderReceiptPaymentType.CSH, errorContext, notifications);
		}

		public static readonly TxnHeaderReceiptPaymentTypeXmlMapping Instance = new TxnHeaderReceiptPaymentTypeXmlMapping();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override string Name
		{
			get { return "Receipt Payment Type"; }
		}
	}
}
