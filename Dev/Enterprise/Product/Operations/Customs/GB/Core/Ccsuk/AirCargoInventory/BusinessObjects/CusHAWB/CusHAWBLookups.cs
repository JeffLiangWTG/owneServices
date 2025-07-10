using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHAWBLookups : Customs.Business.CusHAWBLookups
	{
		public CusHAWBLookups(Customs.Business.AutoCusHAWB parent)
			: base(parent)
		{ }

		public CodeDescriptionPairList PresenceOnNetworkList
		{
			get { return Factory.GetCachedValue<PresenceOnNetworkList>(); }
		}

		public override ICodeDescriptionPairList UnitOfWeightList
		{
			get
			{
				return Factory.GetCachedValue("GBCcsukCusHAWBLookupsWeightUnitList", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(Core.Constants.Weight.Kilograms, Res.GetString("f6a58e45-40fe-4632-9fc5-9858aa1909df", "Kilograms"));
						return list;
					});
			}
		}

		public CodeDescriptionPairList ShipmentDescriptionCodeList
		{
			get { return Factory.GetCachedValue<ShipmentDescriptionCodes>(); }
		}

		public CodeDescriptionPairList CustomsActionsCodes
		{
			get { return Factory.GetCachedValue<CustomsStatusCodes>(); }
		}

		public CodeDescriptionPairList ConsignmentOrEntryTypesList
		{
			get { return Factory.GetCachedValue<ConsignmentOrEntryTypes>(); }
		}

		/// <summary>
		/// Gets both CHIEF-known PIMAs (full sheds and simple agents), plus fallback sheds
		/// </summary>
		public CodeDescriptionPairList ProfilesList
		{
			get { return GetProfilesList(Parent.Branch, Factory); }
		}

		public static CodeDescriptionPairList GetProfilesList(GlbBranch branch, BusinessObjectFactory factory)
		{
			branch = branch ?? GlbBranch.CurrentBranch;
			return factory.GetCachedValue("GBCusHAWBLookupsProfilesList" + branch.PK, () =>
			{
				var hasShedLicence = LicenceAndPimaHelper.ShedEnabled;
				var showDepProfilesToo_NoBecauseThisIsAnImportApplication = false;
				var pimas = new CodeDescriptionPairList(RegistryPimaAndBadgeHelper.GetProfilesKnownToChiefList(branch, factory, hasShedLicence, showDepProfilesToo_NoBecauseThisIsAnImportApplication, false));
				if (hasShedLicence)
				{
					var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (credentials != null)
					{
						var fallbackPimas = (from CredentialsSetting c in credentials where c.IsCcskShed && !c.FallbackForShed.IsEmpty select c);
						foreach (var p in fallbackPimas)
						{
							pimas.AddPairIfNotExist(p.PIMA, "Fallback for " + p.FallbackForShed);
						}
					}
				}
				return pimas;
			});
		}

		public CodeDescriptionPairList UkInventoryControlledAirportsList
		{
			get { return CommonLookupsHelper.UkInventoryControlledAirportsList; }
		}

		public IataAirportsOutsideUKCollection NonUkAirports
		{
			get { return CommonLookupsHelper.NonUkAirportsCollection; }
		}

		public CodeDescriptionPairList ShedsList
		{
			get { return CommonLookupsHelper.GetShedsList(Parent.AirportOfDestination.Right(3)); }
		}

		CommonLookups commonLookupsHelper;
		CommonLookups CommonLookupsHelper
		{
			get { return commonLookupsHelper ?? (commonLookupsHelper = new CommonLookups(Parent)); }
		}

		public new CusHAWB Parent
		{
			get { return (CusHAWB)base.Parent; }
		}
	}
}
