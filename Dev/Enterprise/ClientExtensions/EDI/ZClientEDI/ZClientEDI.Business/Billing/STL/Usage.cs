using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public sealed class UsageOwner : IEquatable<UsageOwner>
	{
		public UsageOwner(LicenceHeader licHeader)
			: this(licHeader.Database, licHeader.Company, licHeader)
		{
		}

		public UsageOwner(IBilledDatabase db, LicenceCompany company, LicenceHeader licHeader)
		{
			Database = db;
			Company = company;
			LicHeader = licHeader;
		}

		public readonly LicenceHeader LicHeader;
		public readonly LicenceCompany Company;
		public readonly IBilledDatabase Database;

		public string PerCompanyBillingPriceCurrency { get { return LicHeader?.LA_RX_NKPriceCurrency ?? string.Empty; } }

		public bool Equals(UsageOwner other)
		{
			return Company.PK == other.Company.PK
				&& Database?.PK == other.Database?.PK;
		}

		public override bool Equals(object obj)
		{
			return Equals((UsageOwner)obj);
		}

		public override int GetHashCode()
		{
			return Company.PK.GetHashCode()
				^ (Database?.PK.GetHashCode() ?? 0);
		}
	}

	public sealed class UsageOwnerDelivery
	{
		/// <summary>
		/// Usage owner and delivery
		/// </summary>
		/// <param name="owner">Owner. Not null</param>
		/// <param name="delivery">Delivery details. Null if delivery instructions are not configured</param>
		/// <param name="invoicedCompany">Company licence of the invoiced org. Can be null if the org has no licence or there is no org configured.</param>
		public UsageOwnerDelivery(UsageOwner owner, ClientInvoiceDelivery delivery, LicenceCompany invoicedCompany)
		{
			this.owner = owner;
			this.delivery = delivery;
			this.invoicedCompany = invoicedCompany;

			if (invoicedCompany != null)
			{
				invoicedOrganisationPk = invoicedCompany.LC_OH;
			}
			else
			{
				invoicedOrganisationPk = delivery == null || !delivery.L9_IsBilled || delivery.L9_OH_InvoiceTo.IsEmpty ? owner.Company.LC_OH : delivery.L9_OH_InvoiceTo;
			}

			invoiceGroup = ClientInvoiceDelivery.CreateGroup(delivery, invoicedOrganisationPk, owner.Company, owner.Database);
		}

		readonly UsageOwner owner;
		readonly ClientInvoiceDelivery delivery;
		readonly ZGuid invoicedOrganisationPk;
		readonly LicenceCompany invoicedCompany;
		readonly InvoiceGroup invoiceGroup;

		public UsageOwner Owner { get { return owner; } }
		public InvoiceGroup InvoiceGroup { get { return invoiceGroup; } }
		public LicenceCompany OwnerCompany { get { return owner.Company; } }
		public LicenceCompany InvoicedCompany { get { return invoicedCompany; } }
		public IBilledDatabase Database { get { return owner.Database; } }
		public ClientInvoiceDelivery Delivery { get { return delivery; } }
		public ZGuid InvoicedOrganisationPk { get { return invoicedOrganisationPk; } }
	}

	/// <summary>
	/// Usage of a module or service. Basic input to the billing.
	/// Wraps a ClientChargeableUsage and/or a ClientPremiumService to provide a common interface.
	/// Multiple instances may map to a single price item, or to no price item at all.
	/// </summary>
	public class Usage
	{
		readonly ClientChargeableUsage chargeable;
		readonly ClientPremiumService service;
		readonly IBilledDatabase database;

		internal Usage(ClientChargeableUsage chargeableUsage)
		{
			this.chargeable = chargeableUsage;
			this.database = chargeable.Database;
			PeriodStart = chargeable.U1_PeriodStart;
		}

		/// <summary>
		/// Constructor for usage coming from ChargeableUsage table.
		/// </summary>
		/// <param name="chargeableUsage">Cannot be null</param>
		public Usage(ClientChargeableUsage chargeableUsage, IBilledDatabase db, BillingDbUsageCodes billingDbUsageCodes, ClientCompany clientCompany)
		{
			this.chargeable = chargeableUsage;
			Company = clientCompany;
			database = db;
			PeriodStart = chargeable.U1_PeriodStart;

			if (billingDbUsageCodes != null)
			{
				if (billingDbUsageCodes.KeyRefIndex1 != 0)
				{
					AdditionalDescription = chargeableUsage.U1_Reference1;
				}
			}
		}

		public UsageOwnerDelivery OwnerDelivery { get; set; }

		/// <summary>
		/// Constructor for usage coming from dbo.ClientPremiumService table.
		/// </summary>
		/// <param name="chargeableUsage">null unless the usage has already been billed</param>
		/// <param name="service">cannot be null</param>
		public Usage(ClientChargeableUsage chargeableUsage, ClientPremiumService service, ZDateTime periodStart, ClientCompany clientCompany)
		{
			this.chargeable = chargeableUsage;
			this.service = service;
			PeriodStart = periodStart;
			AdditionalDescription = service.CPS_ClientRef;

			if (service.CPS_LCC.IsValid && clientCompany == null)
			{
				clientCompany = service.Factory.Load<ClientCompany>(service.CPS_LCC);
			}
			Company = clientCompany;
		}

		public ZGuid PK { get { return (service != null ? service.PK : chargeable.PK); } }
		public ZString Code { get { return (service != null ? (ZString)BillingConstants.BillingSystem.Service : chargeable.U1_Code); } }
		public ZString SubCode { get { return (service != null ? service.CPS_Type : chargeable.U1_SubCode); } }
		public ZDecimal UnitCount { get { return (service != null ? (ZDecimal)service.CPS_Units : chargeable.U1_UnitCount); } }
		public LicenceDatabase Database { get { return (service != null ? service.Database : chargeable.Database); } }
		public string CompanyCode { get { return (service != null ? string.Empty : chargeable.CompanyCode); } }
		public ZGuid ClientCompanyPk { get { return (service != null ? service.CPS_LCC : chargeable.U1_LCC); } }
		public ZGuid InvoicePk { get { return (chargeable != null ? chargeable.U1_AH_Invoice : ZGuid.Empty); } }
		public ClientChargeableUsage ChargeableUsage { get { return chargeable; } }
		public ClientCompany Company { get; }

		public string ServicePriceHeaderCode => CalculateServicePriceHeaderCode(service);

		static string CalculateServicePriceHeaderCode(ClientPremiumService service)
		{
			if (service != null)
			{
				if (!service.CPS_PriceHeaderCode.IsEmpty)
				{
					return service.CPS_PriceHeaderCode;
				}
				else
				{
					return BillingConstants.PriceHeaderType.STL;
				}
			}
			else
			{
				return null;
			}
		}

		public ZString BilledUsageCode
		{
			get
			{
				if (service != null)
				{
					return service.IsSystemLicenceFee ? service.CPS_Type.ToString() : BillingConstants.BillingSystem.Service;
				}
				else
				{
					return chargeable.U1_Code;
				}
			}
		}

		public ZString AdditionalDescription { get; set; }

		/// <summary>
		/// The country code if this usage is to be discounted because of the usage country.
		/// Empty if this usage does not get a country discount.
		/// </summary>
		public ZString CountryCodeForDiscount { get; set; }

		public readonly ZDateTime PeriodStart;

		public bool IsSpecialTaxUsage
		{
			get { return service != null && service.CPS_PriceHeaderCode == BillingConstants.PriceHeaderType.LDaaS; }
		}

		public Guid MainDatabasePk
		{
			get
			{
				IBilledDatabase db = service != null ? service.Database : database;
				return (db.LD_LD_ParentDatabase.IsEmpty ? db.PK : db.LD_LD_ParentDatabase).ToGuid();
			}
		}

		public void CreateOrUpdateChargeableUsageForInvoice(InvoicingBase invoice, ZDateTime periodStart, int unitCountAdjustedForFeeType, decimal price, ZGuid licenceCompanyPk)
		{
			if (service != null)
			{
				PremiumServiceBill.CreateOrUpdateChargeableUsage(invoice,
					service,
					chargeable,
					periodStart,
					unitCountAdjustedForFeeType,
					MainDatabasePk,
					price,
					licenceCompanyPk);
			}
			else if (chargeable != null)
			{
				var usageToModify = chargeable.GetInAnotherFactory(invoice.Factory);
				if (usageToModify.U1_Code == BillingConstants.BillingSystem.BorderWise)
				{
					// Borderwise usage has U1_LD set after it is loaded into memory for calculation purposes.
					// No need to make it permanent.
					usageToModify.U1_LD = ZGuid.Empty;
				}
				usageToModify.U1_AH_Invoice = invoice.PK;
				usageToModify.U1_InvoicedUnitCount = usageToModify.U1_UnitCount;
			}
		}
	}

	/// <summary>
	/// Combines ClientChargeableUsage and ClientPremiumService records into a unified set
	/// </summary>
	public class UsageSet
	{
		public UsageSet(IList<ClientChargeableUsage> chargeableUsages, ClientPremiumService[] services, IReadOnlyList<ClientPremiumService> servicesNotInDatabase, ZDateTime periodStart
			, Dictionary<Guid, IBilledDatabase> databasePkMap
			, Dictionary<Guid, ClientCompany> clientCompanyPkMap)
		{
			var servicePkToService = services != null ? services.ToDictionary(x => x.PK.ToGuid()) : null;
			var databasePkAndCodeToService = servicesNotInDatabase != null ? servicesNotInDatabase.ToDictionary(x => new PkCodePair(x.CPS_LD.ToGuid(), x.CPS_Type)) : null;

			usageList = new List<Usage>(chargeableUsages.Count + services.Length);

			var billingDbCodeMap = BuildBillingDbCodeMap();

			ClientPremiumService service = null;
			foreach (var chargeableUsage in chargeableUsages)
			{
				if (chargeableUsage.U1_Code == BillingConstants.BillingSystem.Service)
				{
					if (servicePkToService != null &&
						!chargeableUsage.U1_Parent.IsEmpty &&
						servicePkToService.TryGetValue(chargeableUsage.U1_Parent.ToGuid(), out service))
					{
						servicePkToService.Remove(chargeableUsage.U1_Parent.ToGuid());
						var clientCompany = TryGetClientCompany(clientCompanyPkMap, service.CPS_LCC);
						usageList.Add(new Usage(chargeableUsage, service, periodStart, clientCompany));
					}
					else if (databasePkAndCodeToService != null &&
						!chargeableUsage.U1_LD.IsEmpty)
					{
						var pkCodePair = new PkCodePair(chargeableUsage.U1_Parent.ToGuid(), chargeableUsage.U1_SubCode);
						if (databasePkAndCodeToService.TryGetValue(pkCodePair, out service))
						{
							databasePkAndCodeToService.Remove(pkCodePair);
							var clientCompany = TryGetClientCompany(clientCompanyPkMap, service.CPS_LCC);
							usageList.Add(new Usage(chargeableUsage, service, periodStart, clientCompany));
						}
					}
				}
				else
				{
					IBilledDatabase db;
					if (databasePkMap != null)
					{
						databasePkMap.TryGetValue(chargeableUsage.U1_LD.ToGuid(), out db);
					}
					else
					{
						db = chargeableUsage.Database;
					}

					BillingDbUsageCodes billingDbUsageCodes;
					if (!billingDbCodeMap.TryGetValue(new Tuple<string, string>(chargeableUsage.U1_Code, chargeableUsage.U1_SubCode), out billingDbUsageCodes))
					{
						billingDbUsageCodes = null;
					}

					var clientCompany = TryGetClientCompany(clientCompanyPkMap, chargeableUsage.U1_LCC);
					var usage = new Usage(chargeableUsage, db, billingDbUsageCodes, clientCompany);
					if (chargeableUsage.U1_Code == BillingConstants.Category.GoldenTax && chargeableUsage.U1_SubCode == "GTS" && !chargeableUsage.U1_Reference1.IsEmpty)
					{
						usage.AdditionalDescription = Res.GetString("EDI|GoldenTax|RegCode", "Reg. Code: {0}", chargeableUsage.U1_Reference1);
					}
					usageList.Add(usage);
				}
			}

			if (servicePkToService != null)
			{
				foreach (var serviceWithoutUsage in servicePkToService.Values)
				{
					var clientCompany = TryGetClientCompany(clientCompanyPkMap, serviceWithoutUsage.CPS_LCC);
					usageList.Add(new Usage(null, serviceWithoutUsage, periodStart, clientCompany));
				}
			}

			if (databasePkAndCodeToService != null)
			{
				foreach (var serviceWithoutUsage in databasePkAndCodeToService.Values)
				{
					var clientCompany = TryGetClientCompany(clientCompanyPkMap, serviceWithoutUsage.CPS_LCC);
					usageList.Add(new Usage(null, serviceWithoutUsage, periodStart, clientCompany));
				}
			}
		}

		static ClientCompany TryGetClientCompany(Dictionary<Guid, ClientCompany> clientCompanyPkMap, ZGuid clientCompanyPk)
		{
			return clientCompanyPkMap != null && clientCompanyPk.IsValid &&
				clientCompanyPkMap.TryGetValue(clientCompanyPk.ToGuid(), out var result)
					? result
					: null;
		}

		static Dictionary<Tuple<string, string>, BillingDbUsageCodes> BuildBillingDbCodeMap()
		{
			var billingDbCodesList = EDIDataRegistry.Instance.BillingDbUsageCodesList.Value;
			var billingDbCodeMap = new Dictionary<Tuple<string, string>, BillingDbUsageCodes>();
			foreach (BillingDbUsageCodes item in billingDbCodesList)
			{
				billingDbCodeMap.Add(new Tuple<string, string>(item.Category, item.PriceItemCode), item);
			}

			return billingDbCodeMap;
		}

		class PkCodePair : Tuple<Guid, string>
		{
			public PkCodePair(Guid pk, string code) : base(pk, code) { }
		}

		public IEnumerable<Usage> Usages { get { return usageList; } }

		readonly List<Usage> usageList;
	}
}

