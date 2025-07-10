using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class JobComInvoiceHeader : AutoJobComInvoiceHeader, Integration.Customs.ES.IJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region JZ_RX_NKInvoice_Currency

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get => base.JZ_RX_NKInvoice_Currency;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_RX_NKInvoice_Currency))
				{
					var oldValue = JZ_RX_NKInvoice_Currency;
					base.JZ_RX_NKInvoice_Currency = value;
					if (!IsCopying && JZ_RX_NKInvoice_Currency != oldValue)
					{
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		protected override bool JZ_InvoiceCurrExRate_ReadOnly => false;

		protected override bool AllowDefaultSupplier => false;

		protected override ZDecimal GetCurrencyConverterExchangeRate(CurrencyConverter currencyConverter, RefCurrency invoiceCurrency) =>
			((CurrencyConverterWithFixedExchangeRatesDataProvider)CurrencyConverter).GetBaseExchangeRate(invoiceCurrency);

		public override ZString JZ_IncoTermPlace
		{
			get => base.JZ_IncoTermPlace;
			set
			{
				var oldValue = JZ_IncoTermPlace;
				base.JZ_IncoTermPlace = value;
				if (!IsCopying && oldValue != JZ_IncoTermPlace)
				{
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JZ_OH_Supplier
		{
			get => base.JZ_OH_Supplier;
			set
			{
				base.JZ_OH_Supplier = value;
				JZ_OA_SupplierAddress_ZAddress.OrgPK = value;
			}
		}

		[ResourceStringData("A07A8B38-CA19-4A37-B862-CA57755E5E1B", Caption = "Buyer", FullDescription = "[13 09 016 000] Buyer Name", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZGuid JZ_OH_Buyer
		{
			get => base.JZ_OH_Buyer;
			set
			{
				base.JZ_OH_Buyer = value;
				JZ_OA_BuyerAddress_ZAddress.OrgPK = value;
			}
		}

		[ResourceStringData("4D4AA0CD-BA1A-4F6F-89AA-C0250EAB1983", Caption = "Invoice Amount", MediumCaption = "Inv. Amount", ShortCaption = "Inv. Amount", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZDecimal JZ_InvoiceAmount { get => base.JZ_InvoiceAmount; set => base.JZ_InvoiceAmount = value; }

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetSupplierAddressFromSupplier();
		}

		void SetSupplierAddressFromSupplier()
		{
			if (JZ_OH_Supplier.IsValid)
			{
				JZ_OA_SupplierAddress_ZAddress.OrgPK = JZ_OH_Supplier;
			}
		}

		#region SupportingDocuments

		public override int MaxSupportingDocuments => 99;
		public override ZString SupportingDocumentsValidationMessage => Res.GetString("60413935-0CD4-482D-8E23-AC2541CEEEC8", "Customs will not accept a declaration with more than 99 documents per line");
		public override Func<int> GetSupportingDocumentsMaxCountReduction => () => JobDeclaration?.SupportingDocuments?.Count ?? 0;
		protected override ZBool NeedAtLeastOneInvoiceSupportingDocumentCore => base.NeedAtLeastOneInvoiceSupportingDocumentCore && (InvoiceLines.Count == 0 || InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.EntryHeaderValidationModeIsImportNoneOrPDS));

		public void AddNewSupportingDocument(ZString code, ZString reference)
		{
			var document = SupportingDocuments.AddNew();
			document.CSI_Code = code;
			document.CSI_ReferenceNumber = reference;
			document.CSI_ParentID = PK;
		}

		#endregion

		#region EntryInstructions

		public ZBool HasT2LOrT2CLine() => !CusEntryInstructions.Any() || CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.IsT2L || x.IsT2C);

		public ZBool HasAnyDiffEXSEntry() => !CusEntryInstructions.Any() || CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => !x.IsEXS);

		public ZBool HasAnyDiffT2CAndT2LAndEXSEntry() => !CusEntryInstructions.Any() || CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => !x.IsEXS && !x.IsT2C && !x.IsT2L);

		public ZBool HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry() => !CusEntryInstructions.Any() || CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => !x.IsT2C && !x.IsT2L && !x.IsEXS && !x.IsSubStyleBOrC);

		#endregion

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
			=> base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();

		public new CurrencyConverter CurrencyConverter => (CurrencyConverterWithFixedExchangeRatesDataProvider)base.CurrencyConverter;

		protected override CurrencyConverter GetNewCurrencyConverter() => new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this);
	}
}
