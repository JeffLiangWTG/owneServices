using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IL.Business
{
	public partial class JobComInvoiceHeader : AutoILJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoILJobComInvoiceHeader.Schema
		{
			public const int JZ_InvoiceAmount_DecimalPlaces = 2;
		}

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Israel;

		[ResourceStringData("719CCFF2-AFAD-423F-A9D5-2DF7E46AB9B6", Caption = "[24] Tran. Nature", FullDescription = "The nature of the transaction.")]
		public override ZString JZ_ValuationCode { get => base.JZ_ValuationCode; set => base.JZ_ValuationCode = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.InvoiceTypeList))]
		[ResourceStringData("46E2CE47-726B-455E-B2E1-F15E7893FBB9", Caption = "Invoice Type")]
		public override ZString JZ_InvoiceType { get => base.JZ_InvoiceType; set => base.JZ_InvoiceType = value; }

		[ResourceStringData("8290E205-F4A5-4429-A0BB-40F09DE19166", Caption = "Sequence")]
		public override ZShort JZ_InvoiceDisplaySequence { get => base.JZ_InvoiceDisplaySequence; set => base.JZ_InvoiceDisplaySequence = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.PreferenceDocumentTypeList))]
		[ResourceStringData("98940BEB-FE9A-492E-9E65-60D2DAD11CF1", Caption = "Preference Agreement", ShortCaption = "Pref. Agreement")]
		public override ZString JZ_PreferenceDocumentType { get => base.JZ_PreferenceDocumentType; set => base.JZ_PreferenceDocumentType = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CountryList))]
		[ResourceStringData("0BF1E56A-6148-413B-A451-38445B7C814F", Caption = "Country")]
		public override ZString JZ_IncoTermPlace { get => base.JZ_IncoTermPlace; set => base.JZ_IncoTermPlace = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.PaymentTermsList))]
		[ResourceStringData("7BA1C8BE-ECC1-4264-B2BD-805C19A91091", Caption = "Payment Terms")]
		public override ZString JZ_PaymentTerms { get => base.JZ_PaymentTerms; set => base.JZ_PaymentTerms = value; }

		[DecimalPlaces(Schema.JZ_InvoiceAmount_DecimalPlaces)]
		public override ZDecimal JZ_InvoiceAmount { get => base.JZ_InvoiceAmount; set => base.JZ_InvoiceAmount = value; }
	}
}
