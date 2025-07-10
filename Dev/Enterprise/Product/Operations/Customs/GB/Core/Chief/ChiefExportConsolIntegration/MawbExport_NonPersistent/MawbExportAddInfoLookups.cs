using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Customs.GB.Chief.GenericMessagingHarness;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class MawbExportAddInfoLookups : AutoMawbExportAddInfoLookups
	{
		public MawbExportAddInfoLookups(AutoMawbExportAddInfo parent) : base(parent)
		{
		}

		public new MawbExportAddInfo Parent
		{
			get { return (MawbExportAddInfo)base.Parent; }
		}

		public CodeDescriptionPairList CommunityTransitList
		{
			get { return new ExportCommunityTransitStatusList(); }
		}

		public CodeDescriptionPairList MasterOptList
		{
			get { return new MasterOpt(); }
		}

		public CodeDescriptionPairList RouteOfEntryList
		{
			get { return new MessageStatusList(); }
		}

		public CodeDescriptionPairList StyleOfEntryList
		{
			get { return new ExportStyleOfEntries(); }
		}

		public CodeDescriptionPairList CustomsReturnCodeList
		{
			get { return new CRC(); }
		}

		public CodeDescriptionPairList CustomsStatisticalReferenceList
		{
			get { return new CustomsStatisticalReferenceTypes(); }
		}

		public CodeDescriptionPairList LocationOfGoodsList
		{
			get
			{
				return Factory.GetCachedValue("MawbExportAddInfoLookups.Ports." + Parent.ME_TransportMode + Parent.ME_ExportShed,
					delegate
					{
						var list = new CodeDescriptionPairList();
						if (Parent.ME_ExportShed.IsEmpty)
						{
							list.AddRange(new PortCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, Parent.ME_TransportMode));
						}
						if (Parent.ME_TransportMode == Customs.Business.TransportTypeList.Codes.Air)
						{
							foreach (var shed in new ShedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, ZString.Empty, Parent.ME_ExportShed)
								.Cast<Shed>().Where(x => !x.AirportName.IsEmpty))
							{
								list.AddPairIfNotExist(shed.PortCode, shed.AirportName);
							}
						}
						list.SortByDescription();
						return list;
					});
			}
		}

		public CodeDescriptionPairList ShedsList
		{
			get
			{
				return Factory.GetCachedValue("MawbExportAddInfoLookups.Sheds." + Parent.ME_TransportMode + Parent.ME_ExportLocation,
					delegate
					{
						var list = new CodeDescriptionPairList();
						if (Parent.ME_TransportMode == Customs.Business.TransportTypeList.Codes.Air)
						{
							foreach (Shed shed in new ShedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, Parent.ME_ExportLocation, ZString.Empty))
							{
								list.AddPair(shed.ShedCode, shed.Name);
							}
						}
						list.SortByDescription();
						return list;
					});
			}
		}

		public CodeDescriptionPairList ProfilesForChief => GetProfilesFromBadgeCodes(false);

		CodeDescriptionPairList GetProfilesFromBadgeCodes(bool forCDS)
		{
			var result = new CodeDescriptionPairList();
			var badgeCodes = BadgeCodeGetter.InstanceCachedFor(Parent).GetBadgeCodes(BadgeDirectionList.Codes.EXP, Parent.Consol.LoadPort?.Code ?? ZString.Empty);

			foreach (var badge in badgeCodes.Where(x => (!forCDS && x.CSPCode != GatewayList.Codes.CDS && x.ApplicationCode != Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services) ||
														(forCDS && x.ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)))
			{
				if (badge.CSPCode == GatewayList.Codes.CCSUKviaNTMsgGW)
				{
					var credential = CredentialsSetting.GetCredentialsForBadge(badge.BadgeCode, GlbBranch.CurrentBranch.GB_GC);

					if (credential != null)
					{
						RegistryPimaAndBadgeHelper.AddCcsukCredentialToList(GlbBranch.CurrentBranch, true, CcsukShowDEPProfile, result, badge, credential, false);
					}
					else
					{
						result.AddPairIfNotExist(badge.BadgeCode, string.Empty);
					}
				}
				else
				{
					result.AddPairIfNotExist(badge.BadgeCode, string.Empty);
				}
			}

			return result;
		}

		public CodeDescriptionPairList ProfilesForCDS => Factory.GetCachedValue("MawbExportAddInfoLookups.ProfilesForCDS", () =>
		{
			var result = GetProfilesFromBadgeCodes(true);

			var gbGlbExternalPasswordCollection = new GlbExternalPasswordCollection_GB(GlbCompany.CurrentCompany);
			gbGlbExternalPasswordCollection.Load();

			gbGlbExternalPasswordCollection.OfType<GlbExternalPassword_GB>()
				.Select(x => System.FormattableString.Invariant($"{x.EORI}.{x.Badge}"))
				.ForEach(profile => result.AddPairIfNotExist(profile, string.Empty));

			return result;
		});

		public CodeDescriptionPairList ProfilesListForExports
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(ProfilesForChief);
				result.AddRange(ProfilesForCDS);

				return result;
			}
		}

		public RefCountryCollection CountriesList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public CodeDescriptionPairList TransportTypeList
		{
			get { return Factory.GetCachedValue<TransportTypeList>(); }
		}

		bool CcsukShowDEPProfile
		{
			get { return GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.Value; }
		}
	}
}
