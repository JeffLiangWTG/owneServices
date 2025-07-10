using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public abstract class SystemUsage : NonPersistentBusinessObject, IObsoleteValidation, IDocumentSupportable
	{
		protected SystemUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(factory)
		{
			User = user;
			PeriodStart = periodStart;
			PriceHeaderCode = BillingConstants.PriceHeaderType.ODM;
		}

		#region Main Properties

		public readonly IUsingParty User;

		public ZGuid OrganisationPK { get { return !User.OrganisationPK.IsEmpty ? User.OrganisationPK : MiscOrganisationPK; } }
		public ZDateTime PeriodStart { get; private set; }
		public ZString ServerCode { get { return User.ServerCode; } }
		public ZString CompanyCode { get { return User.CompanyCode; } }
		public ZString EnterpriseCode { get { return User.EnterpriseCode; } }
		public ZString SubCode { get; set; }
		public ZBool IsBillable => User.UsageOwnerLicence?.Database.IsBillable ?? true;
		protected ZString PriceHeaderCode { get; set; }

		public string LicenceNineCode
		{
			get
			{
				return (!EnterpriseCode.IsEmpty ? (string)EnterpriseCode : "xxx")
					+ "-"
					+ (!CompanyCode.IsEmpty ? (string)CompanyCode : "xxx")
					+ "-"
					+ (!ServerCode.IsEmpty ? (string)ServerCode : "xxx");
			}
		}

		public ZString LicenceMode
		{
			get { return licenceMode; }
			set
			{
				SetNonPersistentPropertyValue(LicenceModeInfo, ref licenceMode, value);
			}
		}
		ZString licenceMode;

		public ZPropertyInfo LicenceModeInfo
		{
			get { return this.GetZPropertyInfo(nameof(LicenceMode)); }
		}

		public virtual ZString PeriodStartAsText
		{
			get { return PeriodStart.ToString("MMM yyyy", CultureInfo.InvariantCulture); }
		}

		#endregion

		#region Chargeable Usage PKs

		public List<ZGuid> ChargeableUsagePKs
		{
			get { return chargeableUsagePKs ?? (chargeableUsagePKs = new List<ZGuid>()); }
		}
		List<ZGuid> chargeableUsagePKs;

		#endregion

		#region Organisation Related Objects

		public EDIOrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.Load<EDIOrgHeader>(OrganisationPK)); }
		}
		EDIOrgHeader organisation;

		static internal ZGuid CalcInvoicedOrganisationPK(ClientInvoiceDelivery invoiceDelivery, ZGuid defaultPk)
		{
			return invoiceDelivery != null ? invoiceDelivery.CalcInvoicedOrganisationPK(defaultPk) : defaultPk;
		}

		public ZGuid InvoicedOrganisationPK
		{
			get { return CalcInvoicedOrganisationPK(InvoiceDelivery, OrganisationPK); }
		}

		public EDIOrgHeader InvoicedOrganisation
		{
			get { return invoicedOrganisation ?? (invoicedOrganisation = Factory.Load<EDIOrgHeader>(InvoicedOrganisationPK)); }
		}
		EDIOrgHeader invoicedOrganisation;

		public LicenceCompany LicCompany
		{
			get { return Organisation != null ? Organisation.LicCompany : null; }
		}

		public LicenceCompany InvoicedLicCompany
		{
			get { return InvoicedOrganisation != null ? InvoicedOrganisation.LicCompany : null; }
		}

		public ClientInvoiceDelivery InvoiceDelivery
		{
			get { return invoiceDelivery ?? (invoiceDelivery = GetInvoiceDeliveryCore()); }
		}
		ClientInvoiceDelivery invoiceDelivery;

		protected virtual ClientInvoiceDelivery GetInvoiceDeliveryCore()
		{
			return LicCompany != null ? LicCompany.InvoiceDeliveries.FindByServerAndSystem(ServerCode, SystemCode) : null;
		}

		public ClientLicenceBilling Billing => LicCompany?.SelfBilling;

		public ZBool IsBilled => InvoiceDelivery?.L9_IsBilled ?? ZBool.False;

		public virtual void SetPreviewOnly(ClientLicencePriceHeader previewPriceHeader)
		{
			if (PriceHeaderCode == previewPriceHeader.L6_SystemCode)
			{
				if (priceHeader != null && priceHeader.PK != previewPriceHeader.PK)
				{
					throw new InvalidOperationException("Attempt to set preview price header after normal price header already calculated for " + SystemDescription
						+ " " + (Organisation?.OH_Code ?? ""));
				}
				priceHeader = previewPriceHeader;
			}
		}

		public virtual ClientLicencePriceHeader PriceHeader
		{
			get
			{
				if (priceHeader == null && PeriodStart.IsValid && LicCompany != null)
				{
					ZDateTime midMonth = new DateTime(PeriodStart.Year, PeriodStart.Month, 15);
					var invDelivery = InvoiceDelivery;
					if (invDelivery == null || !invDelivery.L9_UseParentPrices)
					{
						priceHeader = LicCompany.PriceHeaderForDate(midMonth, PriceHeaderCode);
					}
					if (priceHeader == null && InvoicedOrganisationPK != OrganisationPK && InvoicedLicCompany != null)
					{
						priceHeader = InvoicedLicCompany.PriceHeaderForDate(midMonth, PriceHeaderCode);
					}
				}

				return priceHeader;
			}
		}
		ClientLicencePriceHeader priceHeader;

		public virtual void ResetPriceHeader() => priceHeader = null;

		public bool IsInvoiced
		{
			get { return OrganisationPK == InvoicedOrganisationPK; }
		}

		#endregion

		#region MiscOrganisation

		public static ZGuid MiscOrganisationPK
		{
			get
			{
				if (miscOrganisationPK.IsEmpty)
				{
					miscOrganisationPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
				}

				return miscOrganisationPK;
			}
		}

		[ThreadStatic]
		static ZGuid miscOrganisationPK;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new SystemUsageDocumentSupporter(this); }
		}

		#endregion

		public abstract ZString SystemCode { get; }

		public ZString SystemDescription
		{
			get { return BillingConstants.BillingSystemList.GetDescriptionFromCode(SystemCode); }
		}

		public virtual void CalculateAmount()
		{
		}

		public ZDecimal Amount
		{
			get { return Utilities.Round(AmountCore, BillingConstants.RoundingDecimals); }
		}

		protected virtual ZDecimal AmountCore
		{
			get { return UnitCount * UnitPrice; }
		}

		/// <summary>
		/// Part of Amount that is exempt from processing fees.
		/// Will usually be zero, except for 3rd party charges like 1-stop.
		/// Range is 0..Amount
		/// </summary>
		public ZDecimal AmountExemptProcessingFee
		{
			get { return Utilities.Round(AmountExemptProcessingFeeCore, BillingConstants.RoundingDecimals); }
		}

		protected virtual ZDecimal AmountExemptProcessingFeeCore
		{
			get { return 0m; }
		}

		public virtual ZInt UnitCount
		{
			get { return 0; }
		}

		public virtual ZDecimal UnitPrice
		{
			get { return 0m; }
		}

		public virtual ZString CurrencyCode
		{
			get { return PriceHeader != null ? PriceHeader.L6_RX_NKCurrency : ZString.Empty; }
		}

		public virtual bool HasLicenceUnits
		{
			get { return false; }
		}

		/// <summary>
		/// Called from SystemBill.GetGeneralSummarySections.
		/// The caller may modify the summary to distinguish usage from different servers or different months,
		/// e.g., by appending the date to the description.
		/// </summary>
		public abstract SummarySection[] GetGeneralSummarySections();

		public virtual ZDecimal LicenceUnitsAmount
		{
			get;
			protected set;
		}

		public virtual bool ShowOnBillingSummary
		{
			get { return Amount != 0m || LicenceUnitsAmount != 0m; }
		}

		public ZString ClientCompanyDescription => (!string.IsNullOrEmpty(User.CompanyCode) && !string.IsNullOrEmpty(User.CompanyName)) ?
			FormattableString.Invariant($"{User.CompanyName} ({User.EnterpriseCode}-{User.CompanyCode}-{User.ServerCode})") : "";

		public bool IsMinimumFeeOwner { get; internal set; }

		/// <summary>
		/// Usage of a single item on the price list.
		/// </summary>
		public class SubUsage
		{
			public string PriceItemCode { get; set; }
			public ClientLicencePriceItem PriceItem { get; set; }

			public int RawUsageCount { get; set; }

			/// <summary>
			/// The number of module users for TransactionalModule fee type
			/// </summary>
			public int IncludedUserCount { get; set; }

			// Raw count adjusted by fee type to give the number of units to be charged.
			// E.g., if the fee type is Per Licence and then the unit count is 1 for any non-zero usage count,
			// or if the usage is a number of MB and the fee type is per 10GB then units = usage / 10000.
			public int UnitCount { get; set; }

			// UnitCount times price, rounded to the appropriate precision.
			public decimal Amount { get; set; }

			public decimal LicenceUnits { get; set; }

			public decimal LicenceUnitAmount { get { return UnitCount * LicenceUnits; } }
		}
	}
}

