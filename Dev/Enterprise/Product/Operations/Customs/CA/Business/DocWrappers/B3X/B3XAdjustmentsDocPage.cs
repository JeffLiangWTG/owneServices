using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class B3XAdjustmentsDocPage : AdjustmentsDocPage
	{
		public B3XAdjustmentsDocPage() : base()
		{
		}

		protected override AdjustmentsDocPage CreateNewPage()
		{
			return new B3XAdjustmentsDocPage();
		}

		protected override AdjustmentsDocLine CreateNewLine(bool isEmpty = false)
		{
			return new B3XAdjustmentsDocLine(isEmpty);
		}

		public List<AdjustmentsDocLine> DocLines
		{
			get
			{
				var result = new List<AdjustmentsDocLine>();
				if (AsAccountForDocLine1 != null)
				{
					result.Add(AsAccountForDocLine1);
				}
				if (AsAccountForDocLine2 != null)
				{
					result.Add(AsAccountForDocLine2);
				}
				if (AsClaimForDocLine1 != null)
				{
					result.Add(AsClaimForDocLine1);
				}
				if (AsClaimForDocLine2 != null)
				{
					result.Add(AsClaimForDocLine2);
				}
				return result;
			}
		}

		protected override void SetMoreForPage(AdjustmentsDocPage page, JobComInvoiceHeader subHeader)
		{
			if (page is B3XAdjustmentsDocPage b3xPage && subHeader != null)
			{
				if (subHeader is IEDIInvoiceOGD invoice)
				{
					var vendor = invoice.Vendor;
					if (vendor != null)
					{
						var address = vendor as OrgAddress;
						if (address != null)
						{
							b3xPage.VendorFormatted = AdjustmentDocHelper.AddressForVendorFormatted(address);
						}
						else
						{
							var docAddress = vendor as JobDocAddress;
							b3xPage.VendorFormatted = docAddress != null ? AdjustmentDocHelper.AddressForVendorFormatted(docAddress) : (ZString)($@"{vendor.E2_CompanyNameTruncated}
{vendor.E2_State} {vendor.E2_Postcode}").TrimEnd();
						}
					}
				}

				var declaration = subHeader.JobDeclaration;
				if (declaration != null)
				{
					b3xPage.TypeCode = declaration.JE_MessageSubType;

					if (declaration.IsImporterDirectPayment)
					{
						b3xPage.PaymentCode = "I";
					}
					else if (declaration.IsGSTDirectPayment)
					{
						b3xPage.PaymentCode = "G";
					}

					b3xPage.TransportMode = TransportTypeList.GetTransportModeNumber(declaration.JE_TransportMode);
					b3xPage.PortOfUnlading = declaration.CA_UnladingOffice.TrimStart('0');
					b3xPage.FreightCharges = declaration.CalculatedFreightAmount;
					b3xPage.ReleaseDate = declaration.JE_EntryAuthorisationDate;
					if (declaration.IsLVX)
					{
						b3xPage.OtherReferences = declaration.Invoices[0].CA_OtherReference;
						b3xPage.CarrierDate = declaration.LVXInvoiceHeader.JZ_InvoiceDate;
						b3xPage.CarrierCode = declaration.LVXInvoiceHeader.CA_LVSCarrier;
					}
					b3xPage.B3Comments = declaration.B3Comments;
					b3xPage.AccountSecurityCode = declaration.TransactionNumber.AccountSecurityCode;
					b3xPage.TransactionNumberID = declaration.TransactionNumber.UniqueIdentifier;
				}

				b3xPage.TotalValueForDuty = ZDecimal.Zero;
				b3xPage.USPortOfExit = subHeader.CA_USPortOfExit;
				b3xPage.InvoiceNumber = subHeader.JZ_InvoiceNumber;
				b3xPage.ExchangeRate = subHeader.EffectiveExchangeRateForInvoiceCurr;
			}
		}

		public ZString VendorFormatted { get; set; }
		public ZString TypeCode { get; set; }
		public ZString PaymentCode { get; set; }
		public ZString TransportMode { get; set; }
		public ZString PortOfUnlading { get; set; }
		public ZDecimal TotalValueForDuty { get; set; }
		public ZString USPortOfExit { get; set; }
		public ZDecimal FreightCharges { get; set; }
		public ZString InvoiceNumber { get; set; }
		public ZDecimal ExchangeRate { get; set; }
		public ZDateTime ReleaseDate { get; set; }
		public ZString OtherReferences { get; set; }
		public ZDateTime CarrierDate { get; set; }
		public ZString CarrierCode { get; set; }
		public ZString B3Comments { get; set; }
		public ZString AccountSecurityCode { get; set; }
		public ZString TransactionNumberID { get; set; }
	}
}
