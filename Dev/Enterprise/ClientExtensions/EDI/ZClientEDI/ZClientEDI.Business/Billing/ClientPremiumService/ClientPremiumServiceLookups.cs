//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientPremiumServiceLookups
//
//    This class should be used for overriding collections in AutoClientPremiumServiceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientPremiumServiceLookups : AutoClientPremiumServiceLookups
	{
		public ClientPremiumServiceLookups(AutoClientPremiumService parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PriceHeaderCodes
		{
			get
			{
				return Factory.GetCachedValue("PriceHeaderCodes", () =>
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();
					foreach (ICodeDescription pair in BillingConstants.PriceHeaderType.GetPriceHeaderTypeList())
					{
						if (BillingConstants.PriceHeaderType.IsGlobal(pair.Code))
						{
							result.AddPair(pair.Code, pair.Description);
						}
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList PremiumServiceTypes
		{
			get
			{
				var parent = (ClientPremiumService)Parent;
				var priceHeader = parent.GetPriceHeader();

				return Factory.GetCachedValue("PremiumServiceTypes_" + (priceHeader != null ? priceHeader.PK.ToString() : ""), () =>
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();
					if (priceHeader != null)
					{
						foreach (var item in priceHeader.Items.Cast<ClientLicencePriceItem>()
							.Where(x => !x.L7_Code.IsEmpty && x.L7_Category == BillingConstants.BillingSystem.Service)
							.OrderBy(x => x.L7_Code))
						{
							result.AddPair(item.L7_Code, item.L7_DescriptionLocalized);
						}

						foreach (var codeDescription in EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.Value.OfType<ICodeDescription>())
						{
							result.AddPair(codeDescription.Code, codeDescription.Description);
						}
					}
					else
					{
						result.AddPair("", "<Please define a price list for this database, valid at the service start date>");
					}

					return result;
				});
			}
		}

		public ClientCompanyCollection UsageOwners
		{
			get
			{
				return ((ClientPremiumService)Parent).Database.ClientCompanies;
			}
		}

		public OrgHeaderCollection UsageOwnersOrganisations
		{
			get
			{
				return Factory.GetCachedValue("PremiumServiceUsageOwnersOrganisations_" + ((ClientPremiumService)Parent).Database.PK.ToString(), () =>
					{
						var orgs = new OrgHeaderCollection(Factory);
						orgs.AddRange(UsageOwners.Select(x => x.Org).Where(x => x != null));
						return orgs;
					});
			}
		}

		public static CodeDescriptionPairList GetPremiumServiceTypesInUse(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PremiumServiceTypesInUse", () =>
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();

				foreach (var serviceByType in factory.Load<ClientPremiumService>(new ZQuery())
					.GroupBy(x => x.CPS_Type)
					.OrderBy(x => x.Key))
				{
					result.AddPair(serviceByType.Key, serviceByType.First().TypeDescription);
				}

				return result;
			});
		}
	}
}

