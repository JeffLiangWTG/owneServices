using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage()]
	public class OdplUsage : SystemUsage
	{
		/// <summary>
		/// On Demand usage for an organization from a single database and month.
		/// </summary>
		/// <param name="owner">Owner of the usage. As a LicenceHeader it indicates both the LicenceCompany and LicenceDatabase involved. Not null. For a user created company, this is the owner of the database.</param>
		/// <param name="periodStart">month</param>
		/// <param name="clientCompany">The ClientCompany reporting the usage. May be null if this just contains purchased seats.</param>
		public OdplUsage(BusinessObjectFactory factory, LicenceHeader owner, ZDateTime periodStart, ClientCompany clientCompany)
			: base(factory, new UsingParty(owner, clientCompany), periodStart)
		{
			ClientCo = clientCompany;
			LicHeader = owner;
		}

		/// <summary>
		/// Constructor for commitment discount usage
		/// </summary>
		public OdplUsage(BusinessObjectFactory factory, UsingParty user, ZDateTime periodStart)
			: base(factory, user, periodStart)
		{
		}

		public readonly ClientCompany ClientCo;
		public readonly LicenceHeader LicHeader;
		public LicenceDatabase Database
		{
			get { return ClientCo != null ? ClientCo.Database : (LicHeader != null ? LicHeader.Database : null); }
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.ODM; }
		}

		#region PriceHeader

		public override void SetPreviewOnly(ClientLicencePriceHeader previewPriceHeader)
		{
			if (!priceHeaderIsLoaded)
			{
				priceHeaderIsLoaded = true;
				priceHeader = previewPriceHeader;
			}
		}

		static internal ClientLicencePriceHeader PriceHeaderForDate(ZDateTime monthAndYear, EDIOrgHeader org, ClientInvoiceDelivery invoiceDelivery)
		{
			ClientLicencePriceHeader result = null;
			if (org != null && org.LicCompany != null)
			{
				ZDateTime midMonth = new DateTime(monthAndYear.Year, monthAndYear.Month, 15);
				if (invoiceDelivery == null || !invoiceDelivery.L9_UseParentPrices)
				{
					result = org.LicCompany.OnDemandPriceHeaderForDate(midMonth);
				}
				if (result == null || result.IsQuickTransactionalPricelist)
				{
					var invoicedOrgPk = SystemUsage.CalcInvoicedOrganisationPK(invoiceDelivery, ZGuid.Empty);
					if (!invoicedOrgPk.IsEmpty)
					{
						EDIOrgHeader invoicedOrg = org.Factory.Load<EDIOrgHeader>(invoicedOrgPk);
						if (invoicedOrg != null && invoicedOrg.LicCompany != null)
						{
							ClientLicencePriceHeader parent = invoicedOrg.LicCompany.OnDemandPriceHeaderForDate(midMonth);
							if (parent != null)
							{
								result = parent;
							}
						}
					}
				}
			}

			return result;
		}

		public override ClientLicencePriceHeader PriceHeader
		{
			get
			{
				if (!priceHeaderIsLoaded)
				{
					priceHeaderIsLoaded = true;
					if (PeriodStart.IsValid && LicCompany != null)
					{
						priceHeader = PriceHeaderForDate(PeriodStart, Organisation, InvoiceDelivery);
					}

					if (priceHeader != null && priceHeader.IsQuickTransactionalPricelist)
					{
						priceHeader = null;
					}
				}
				return priceHeader;
			}
		}
		ClientLicencePriceHeader priceHeader;
		bool priceHeaderIsLoaded;

		#endregion

		#region Module Usages

		public OdplModuleUsageCollection ModuleUsages
		{
			get { return moduleUsages ?? (moduleUsages = new OdplModuleUsageCollection(Factory)); }
		}
		OdplModuleUsageCollection moduleUsages;

		public OdplModuleUsage AddModuleUsage(ZString moduleCode, int staffCount, ClientLicencePriceItem priceItem)
		{
			if (priceItem != null && !priceItem.IsOnDemandFeeType)
			{
				return null;
			}

			if (ModuleUsages.Cast<OdplModuleUsage>().Any(x => x.ModuleCode == moduleCode))
			{
				throw new InvalidOperationException("AddModuleUsage called more than once for module code " + moduleCode);
			}

			var odplModuleUsage = ModuleUsages.AddNew();
			odplModuleUsage.ModuleCode = moduleCode;
			odplModuleUsage.StaffCount = staffCount;
			if (priceItem != null)
			{
				odplModuleUsage.PopulateFromPriceItem(priceItem);
			}

			return odplModuleUsage;
		}

		public bool IsHybrid { get; private set; }

		#endregion

		#region Calculate Amount

		public override void CalculateAmount()
		{
			CalculateIsHybrid();

			if (PriceHeader == null)
			{
				return;
			}

			ModuleUsages.Sort("Order");
		}

		public bool HasWebUsage
		{
			get
			{
				if (!hasWebUsage.HasValue)
				{
					OdplModuleUsage webTracker = ModuleUsages.Cast<OdplModuleUsage>().FirstOrDefault(x => x.ModuleCode == "WEB");
					hasWebUsage = webTracker != null && webTracker.StaffCount > 0;
				}
				return hasWebUsage.Value;
			}
		}
		bool? hasWebUsage;

		void CalculateIsHybrid()
		{
			IsHybrid = LicHeader != null && HasPurchasedSeats(LicHeader.PurchasedModules.FindByCode(BillingConstants.CoreModuleCode));
		}

		bool HasPurchasedSeats(LicenceModules module)
		{
			return HasPurchasedSeats(module, PeriodStart);
		}

		static public bool HasPurchasedSeats(LicenceModules module, ZDateTime periodStart)
		{
			return module != null
				&& module.LM_UserCount > 0
				&& (module.LM_ExpiryDate.IsEmpty || module.LM_ExpiryDate > periodStart)
				&& (
					module.LM_LicenceType == LicenceTypes.Codes.PUR ||
					module.LM_LicenceType == LicenceTypes.Codes.OTM ||
					module.LM_LicenceType == LicenceTypes.Codes.ODM ||
					module.LM_LicenceType == LicenceTypes.Codes.OPN ||
					module.LM_LicenceType == LicenceTypes.Codes.SRU ||
					module.LM_LicenceType == LicenceTypes.Codes.REN
				);
		}

		#endregion

		#region Amounts

		protected override ZDecimal AmountCore
		{
			get { return ModuleUsages.Cast<OdplModuleUsage>().Sum(x => x.Amount); }
		}

		public OdplModuleUsage CoreUsage
		{
			get
			{
				return ModuleUsages.Cast<OdplModuleUsage>().FirstOrDefault(x => x.ModuleCode == BillingConstants.CoreModuleCode);
			}
		}

		public bool HasOnDemandUsage
		{
			get
			{
				return ModuleUsages.Cast<OdplModuleUsage>().Any(x => x.UnitCount > 0 && (x.UnitPrice > 0m || PriceHeader == null));
			}
		}

		public override ZDecimal LicenceUnitsAmount
		{
			get { return ModuleUsages.Cast<OdplModuleUsage>().Sum(x => x.LicenceUnitsAmount); }
		}

		public decimal MixedAmountAsMoney
		{
			get { return ModuleUsages.Cast<OdplModuleUsage>().Sum(x => x.MixedAmountAsMoney); }
		}

		public decimal MixedAmountAsLicenceUnits
		{
			get { return ModuleUsages.Cast<OdplModuleUsage>().Sum(x => x.MixedAmountAsLicenceUnits); }
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			List<SummarySection> result = new List<SummarySection>();

			Dictionary<ClientLicencePriceItem, ClientLicencePriceItem> childToParentMap = BuildChildToParentMap();

			IEnumerable<OdplModuleUsage> modules = ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ShowOnSummary);
			if (modules.Any())
			{
				SummarySection section = CreateGeneralSummary(modules, "On Demand Production License Usage", Amount, childToParentMap);
				result.Add(section);
			}

			return result.ToArray();
		}

		SummarySection CreateGeneralSummary(IEnumerable<OdplModuleUsage> summaryUsages,
			ZString headerDescription,
			ZDecimal totalAmount,
			Dictionary<ClientLicencePriceItem, ClientLicencePriceItem> childToParentMap)
		{
			ZDecimal licUsage = 0m;
			bool isMixed = summaryUsages.Any(s => s.PurchasedStaffCount != 0);

			Dictionary<ClientLicencePriceItem, ClientLicencePriceItem> parentsDone = new Dictionary<ClientLicencePriceItem, ClientLicencePriceItem>();

			SummarySection result = new SummarySection(Factory);
			foreach (OdplModuleUsage moduleUsage in summaryUsages)
			{
				// Add group header label if needed
				ClientLicencePriceItem priceItem;
				ClientLicencePriceItem parent;
				if (PriceHeader != null
					&& null != (priceItem = PriceHeader.LocalOrStandardItems.FindByCode(moduleUsage.ModuleCode))
					&& childToParentMap.TryGetValue(priceItem, out parent)
					&& parent.L7_FeeType.IsEmpty
					&& !parentsDone.ContainsKey(parent))
				{
					parentsDone.Add(parent, parent);
					result.Lines.AddNew().MainDescription = parent.L7_DescriptionLocalized;
				}

				SummaryLine summaryLine = result.Lines.AddNew();
				summaryLine.MainDescription = moduleUsage.ModuleName;
				summaryLine.AdditionalDescription = moduleUsage.FeeTypeDescription;
				summaryLine.UnitCount = moduleUsage.UnitCount.ToString();
				summaryLine.LicenceUnits = moduleUsage.LicenceUnits.ToString();
				summaryLine.LicenceUnitsAmount = moduleUsage.LicenceUnitsAmount.ToString();
				summaryLine.UnitPrice = moduleUsage.UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				summaryLine.Amount = moduleUsage.Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				licUsage += moduleUsage.LicenceUnitsAmount;

				if (moduleUsage.PurchasedStaffCount > 0)
				{
					summaryLine.PurchasedCount = moduleUsage.PurchasedStaffCount.ToString();
					summaryLine.TotalUnitCount = moduleUsage.MixedUnitCount.ToString();
				}
			}

			SummaryLine summaryHeader = result.Header;
			summaryHeader.MainDescription = headerDescription;
			summaryHeader.AdditionalDescription = "Fee Basis";
			summaryHeader.LicenceUnits = "Licence Units";
			summaryHeader.LicenceUnitsAmount = "Total Licence Units";

			if (HasLicenceUnits)
			{
				summaryHeader.UnitPrice = ZString.Format("Price\r\n({0})", priceHeader.Currency.RX_Code);
				summaryHeader.Amount = ZString.Format("Total Price\r\n({0})", priceHeader.Currency.RX_Code);
			}
			else
			{
				summaryHeader.UnitPrice = "Price";
				summaryHeader.Amount = "Total";
			}

			summaryHeader.PurchasedCount = isMixed ? "Purchased Users" : "";
			summaryHeader.UnitCount = isMixed ? "On Demand Users" : "Users";
			summaryHeader.TotalUnitCount = isMixed ? "Total Users" : "";
			summaryHeader.TotalAmount = totalAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryHeader.TotalLicenceUnits = licUsage.ToString();

			return result;
		}

		public override bool HasLicenceUnits
		{
			get { return ModuleUsages.Cast<OdplModuleUsage>().Any(x => x.ShowOnSummary && x.LicenceUnits > 0); }
		}

		public Dictionary<ClientLicencePriceItem, ClientLicencePriceItem> BuildChildToParentMap()
		{
			Dictionary<ClientLicencePriceItem, ClientLicencePriceItem> childToParent = new Dictionary<ClientLicencePriceItem, ClientLicencePriceItem>();

			if (PriceHeader != null)
			{
				List<ClientLicencePriceItem> parentStack = new List<ClientLicencePriceItem>();

				foreach (var priceItem in PriceHeader.LocalOrStandardItems.Cast<ClientLicencePriceItem>().OrderBy(s => s.L7_Order))
				{
					int indent = priceItem.DescriptionIndentLevel;

					while (parentStack.Count > 0 && parentStack[parentStack.Count - 1].DescriptionIndentLevel >= indent)
					{
						parentStack.RemoveAt(parentStack.Count - 1);
					}

					if (parentStack.Count > 0)
					{
						childToParent.Add(priceItem, parentStack[parentStack.Count - 1]);
					}
					parentStack.Add(priceItem);
				}
			}

			return childToParent;
		}

		#endregion

		public ZInt PurchasedLicenceUnits { get; set; }
		public ZDecimal LicenceUnitRate { get; set; }

		public ZString CountryCode
		{
			get
			{
				return ClientCo != null && !ClientCo.LCC_RN_NKCountryCode.IsEmpty
					? ClientCo.LCC_RN_NKCountryCode
					: LicCompany.LC_CompanyCountry;
			}
		}
	}
}

