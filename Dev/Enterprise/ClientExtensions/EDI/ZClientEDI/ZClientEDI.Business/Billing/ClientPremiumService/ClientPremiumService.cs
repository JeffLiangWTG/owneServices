using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(LicenceDatabase), "PremiumServices")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientPremiumService : AutoClientPremiumService
	{
		public ClientPremiumService(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		#region Database

		public LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(CPS_LD); }
		}

		#endregion

		#region Usage Owner 

		[RelatedBusinessObject("UsageOwner")]
		[List(nameof(Lookups) + "." + nameof(ClientPremiumServiceLookups.UsageOwners))]
		public override ZGuid CPS_LCC
		{
			get { return base.CPS_LCC; }
			set { base.CPS_LCC = value; }
		}

		[List(nameof(Lookups) + "." + nameof(ClientPremiumServiceLookups.UsageOwnersOrganisations))]
		public ZGuid UsageOwnerOrgPK
		{
			get
			{
				return UsageOwner?.LCC_OH ?? ZGuid.Empty;
			}

			set
			{
				if (value.IsEmpty)
				{
					CPS_LCC = ZGuid.Empty;
				}
				else
				{
					var owner = Lookups.UsageOwners.FirstOrDefault(x => x.LCC_OH == value);
					CPS_LCC = owner != null ? owner.PK : ZGuid.Empty;
				}

				UsageOwnerOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UsageOwnerOrgPKInfo
		{
			get { return GetZPropertyInfo(nameof(UsageOwnerOrgPK)); }
		}

		public ClientCompany UsageOwner
		{
			get { return Factory.Load<ClientCompany>(CPS_LCC); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(ClientPremiumServiceLookups.PriceHeaderCodes))]
		public override ZString CPS_PriceHeaderCode
		{
			get { return base.CPS_PriceHeaderCode; }
			set { base.CPS_PriceHeaderCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(ClientPremiumServiceLookups.PremiumServiceTypes))]
		public override ZString CPS_Type
		{
			get { return base.CPS_Type; }
			set
			{
				base.CPS_Type = value;
			}
		}

		public ZString TypeDescription
		{
			get { return Lookups.PremiumServiceTypes.GetDescriptionFromCode(CPS_Type); }
		}

		public bool IsSystemLicenceFee { get; set; }

		#region IsDateRangeMatched

		public bool IsDateRangeMatched(ZDateTime billingDate)
		{
			return (CPS_StartDate.IsEmpty || billingDate >= CPS_StartDate)
				&& (CPS_EndDate.IsEmpty || billingDate < CPS_EndDate);
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ClientLicencePriceHeader GetPriceHeader()
		{
			ClientLicencePriceHeader priceHeader = null;
			var startDate = !CPS_StartDate.IsEmpty ? CPS_StartDate : ZDateTime.Today;

			if (!CPS_PriceHeaderCode.IsEmpty)
			{
				var standardPricesCompany = LicenceCompany.StandardPricesCompany;
				if (standardPricesCompany != null)
				{
					priceHeader = standardPricesCompany.PriceHeaders
						.Where(x => x.L6_SystemCode == CPS_PriceHeaderCode)
						.OrderBy(x => x.L6_ValidFrom <= startDate ? 0 : 1)
						.ThenByDescending(x => x.L6_ValidFrom)
						.FirstOrDefault();
				}
			}
			else
			{
				var db = Database;

				if (db == null)
				{
					return null;
				}

				var priceLink = db.PriceHeaderLinks.Cast<EdiPriceHeaderLink>()
					.OrderBy(x => x.PHL_ValidFrom <= startDate ? 0 : 1) // put valid first
					.ThenByDescending(x => x.PHL_ValidFrom)
					.FirstOrDefault();

				if (priceLink != null)
				{
					priceHeader = priceLink.PriceHeader;
				}
				else
				{
					var licHeader = db.UsageOwnerOrFirstLicence;

					if (licHeader != null)
					{
						var licCompany = licHeader.Company;
						var invoiceDelivery = licCompany.InvoiceDeliveries.FindByServerAndSystem(db.LD_ServerCode, CPS_Type);
						if (invoiceDelivery == null || !invoiceDelivery.L9_UseParentPrices)
						{
							priceHeader = licCompany.PriceHeaderForDate(startDate, BillingConstants.PriceHeaderType.ODM);
						}
						if (invoiceDelivery != null && priceHeader == null)
						{
							var payingOrg = invoiceDelivery.InvoiceTo;
							if (payingOrg != null && payingOrg.PK != licCompany.LC_OH)
							{
								var payingCompany = payingOrg.LicCompany;
								if (payingCompany != null)
								{
									priceHeader = payingCompany.PriceHeaderForDate(startDate, BillingConstants.PriceHeaderType.ODM);
								}
							}
						}
						if (priceHeader != null && priceHeader.L6_IsStandard)
						{
							priceHeader = priceHeader.StandardPrices;
						}
					}
				}
			}

			return priceHeader;
		}

		#region Log Changes

		public override void OnSaving()
		{
			base.OnSaving();
			LogChanges();
		}

		void LogChanges()
		{
			bool criticalFieldsHasChanges = IsInDatabase &&
				(CPS_TypeInfo.HasChanges || CPS_StartDateInfo.HasChanges || CPS_EndDateInfo.HasChanges);

			LicenceDatabase db;
			if (criticalFieldsHasChanges && (db = Database) != null)
			{
				ZStringBuilder builder = new ZStringBuilder("Premium");
				AppendFieldInformation(builder, "Typ", (ZString)CPS_TypeInfo.OriginalValue, CPS_Type);
				AppendFieldInformation(builder, "Sta", ((ZDateTime)CPS_StartDateInfo.OriginalValue).ToShortDateString(), CPS_StartDate.ToShortDateString());
				AppendFieldInformation(builder, "End", ((ZDateTime)CPS_EndDateInfo.OriginalValue).ToShortDateString(), CPS_EndDate.ToShortDateString());

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				db.Logs.AddNew(Events.EditedARecord, builder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void AppendFieldInformation(ZStringBuilder builder, ZString fieldName, ZString originalValue, ZString newValue)
		{
			bool fieldHasChanges = newValue != originalValue;
			if (!originalValue.IsEmpty || fieldHasChanges)
			{
				builder.Append(string.Format(CultureInfo.CurrentCulture, " | {0}:{1}", fieldName, originalValue));
				if (fieldHasChanges)
				{
					builder.Append(string.Format(CultureInfo.InvariantCulture, "=>{0}", newValue));
				}
			}
		}

		#endregion
	}
}

