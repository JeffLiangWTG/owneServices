using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public static class AccPlaceOfSupplyTestHelper
	{
		internal static void SetUpPOSConfigurationToReturn_ForTestOnly(IJobInvoicingPlugIn invoicingPlugIn, CostSell costSell, ZString posType, ZString posCode, OrgHeader org = null, GlbBranch branch = null)
		{
			var addressToSetUp = costSell == CostSell.Cost ? branch?.OrgProxy?.MainAddress : org?.MainAddress;
			SetUpOrgAddressToReturnPOS(addressToSetUp, posType, posCode);
		}

		internal static void SetUpPOSConfigurationToReturn_ForTestOnly(IJobCostingPlugIn consol, ZString posType, ZString posCode, GlbBranch branch)
		{
			SetUpOrgAddressToReturnPOS(branch?.OrgProxy?.MainAddress, posType, posCode);
		}

		static void SetUpOrgAddressToReturnPOS(OrgAddress address, ZString posType, ZString posCode)
		{
			NUnit.Framework.Assertion.AssertNotNull("Pre-condition: Org address to set up POS should not be null", address);

			if (posType == PlaceOfSupplyTypes.State.Code)
			{
				address.StateCode = posCode;
			}
			else if (posType == PlaceOfSupplyTypes.TaxZone.Code)
			{
				var zoneQuery = new ZQuery(RefZoneHeaderSchema.FZ_Code, posCode);
				zoneQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, "TAX");
				var taxZone = address.Factory.LoadTop1<RefZoneHeader>(zoneQuery);
				NUnit.Framework.Assertion.AssertNotNull($"Tax Zone with code: '{posCode}'", taxZone);
				NUnit.Framework.Assertion.Assert($"Tax Zone with code: '{posCode}' must have UNLOCOs", taxZone.UNLOCOs.Any());
				address.OA_RL_NKRelatedPortCode = taxZone.UNLOCOs[0].RL_Code;
				address.OA_RN_NKCountryCode = taxZone.UNLOCOs[0].RL_RN_NKCountryCode;
				address.StateCode = taxZone.UNLOCOs[0].CountryStates.RW_Code;
			}
			else if (posType == PlaceOfSupplyTypes.Country.Code)
			{
				if (posCode != address.OA_RN_NKCountryCode)
				{
					address.OA_RN_NKCountryCode = posCode;
				}
				address.StateCode = ZString.Empty;
			}
			else if (posType == PlaceOfSupplyTypes.PredefinedRule.Code
				&& posCode == PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry)
			{
				var otherCountry = address.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, Env.CurrentCompany.Country.Code));
				address.OA_RN_NKCountryCode = otherCountry.RN_Code;
				address.StateCode = otherCountry.States.OfType<RefCountryStates>().FirstOrDefault()?.RW_Code ?? ZString.Empty;
			}
			else
			{
				NUnit.Framework.Assertion.Fail("Currently not implemented POS Type: " + posType);
			}
		}
	}
}
