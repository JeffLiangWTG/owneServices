using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GbMawbExport)]
	public class MawbExportAddInfo : AutoMawbExportAddInfo, IConsolMessagingProvider
	{
		protected MawbExportAddInfo(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MawbExportAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public void Initialize()
		{
			LoadConsol();
			CalculateMUCR();
		}

		public void CalculateMUCR(bool updateOnlyWhenEmpty = true)
		{
			if ((updateOnlyWhenEmpty && ME_MasterUCR.IsEmpty) || !updateOnlyWhenEmpty)
			{
				ZString masterUCR = new MasterUCRCalculator().Calculate(this);
				if (!string.IsNullOrEmpty(masterUCR))
				{
					ME_MasterUCR = masterUCR.Left(ME_MasterUCRInfo.MaxLength);
				}
			}
		}

		public void DefaultBadgeCodeFromLoadPort()
		{
			BadgeCodeSetting badgeCodeSetting = (Consol.LoadPort?.Code ?? ZString.Empty) == ZString.Empty ? null : BadgeCodeGetter.InstanceCachedFor(this).GetFromPortCode(Consol.LoadPort.Code, BadgeDirectionList.Codes.EXP);
			if (!ME_ProfileInfo.ReadOnly)
			{
				if (badgeCodeSetting != null)
				{
					ME_Profile = badgeCodeSetting.BadgeCode;
				}
				else
				{
					ME_Profile = string.Empty;
				}
			}
		}

		void LoadConsol()
		{
			if (Consol.IsExport() &&
				!Consol.IsDomestic() &&
				!Consol.JK_RL_NKLoadPort.IsEmpty &&
				Consol.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
			{
				BringInDefaultUserValuesFromConsol();
			}
		}

		public ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					var baseCusAddInfo = (CusAddInfo)Parent;
					consol = Factory.Load<ForwardingConsol>(baseCusAddInfo.B7_ParentID);
				}
				return consol;
			}
		}

		void BringInDefaultUserValuesFromConsol()
		{
			if (!IsInDatabase)
			{
				using (SuspendSettingHasChanges())
				{
					ME_ExportLocation = GB.Business.Declaration.PortConverter.UnlocoToChief(Consol.JK_RL_NKLoadPort, Consol.Factory, Consol.JK_TransportMode).Left(ME_ExportLocationInfo.MaxLength);
					ME_TransportMode = Consol.JK_TransportMode.Left(ME_TransportModeInfo.MaxLength);
					ME_TransportID = Consol.JK_JX_JV_VoyageFlight.Left(ME_TransportIDInfo.MaxLength);
					if (Consol.DepartureCTOAddress != null)
					{
						ME_ExportShed = Consol.DepartureCTOAddress.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedKingdomCodeTypes.CTOShed, Core.Constants.CountryCodes.UnitedKingdom).Right(3);
					}
					DefaultBadgeCodeFromLoadPort();
				}
			}
			CalculateCTStatusIfNeeded();
		}

		public static string GetBestOrOnlyPima(CodeDescriptionPairList profiles)
		{
			var result = string.Empty;
			if (profiles.Count == 1)
			{
				result = profiles[0].Code;
			}
			else
			{
				var primaryBadge = (from CodeDescriptionPair p in profiles where p.Description.EndsWith("***") select p).FirstOrDefault();
				if (primaryBadge != null)
				{
					result = primaryBadge.Code;
				}
			}
			return result;
		}

		internal void CalculateCTStatusIfNeeded()
		{
			using (SuspendSettingHasChanges())
			{
				if (!ME_UseAntiSmugglingTrptid)
				{
					ME_CommunityTransitStatus = (from ForwardingShipment s in Consol.Shipments select s.JS_CommunityTransitStatus).OrderBy(n => n, new CommunityTransitStatusComparer()).FirstOrDefault().Left(ME_CommunityTransitStatusInfo.MaxLength);
				}
				ME_CommunityTransitStatusInfo.RefreshBinding();
			}
		}

		ChiefExportDes242MessagesOnConsolCollection messages;
		public ChiefExportDes242MessagesOnConsolCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new ChiefExportDes242MessagesOnConsolCollection(Consol);
					messages.Load();
				}
				return messages;
			}
		}

		[ReadOnlyMember(nameof(ME_UseAntiSmugglingTrptid))]
		public override ZString ME_TransportID
		{
			get { return base.ME_TransportID; }
			set
			{
				base.ME_TransportID = value;
				if (ME_TransportMode == TransportTypeList.Codes.Air)
				{
					var carrierCode = value.ToUpper().Left(2);
					if (!carrierCode.IsEmpty)
					{
						var airline = RefAirline.LoadFromAirline2LetterCode(Factory, carrierCode);
						if (airline != null)
						{
							RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Desc, airline.RM_AirlineCountry));
							if (country != null)
							{
								ME_TransportCountry = country.Code.Left(ME_TransportCountryInfo.MaxLength);
							}
						}
					}
				}
				else if (ME_TransportMode == TransportTypeList.Codes.Sea)
				{
					// TODO - get vessel country from somewhere (Lloyds? Vessel name?)
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.ProfilesListForExports))]
		public override ZString ME_Profile
		{
			get { return base.ME_Profile; }
			set
			{
				var oldValue = ME_Profile;
				base.ME_Profile = value;
				var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var credential = (from CredentialsSetting cred in credentials where cred.BadgeCode == ME_Profile select cred).FirstOrDefault();
				if (credential != null)
				{
					if (credential.IsCcskShed)
					{
						if (credential.Company.IsEmpty)
						{
							ME_ExportShed = credential.PIMA.Right(3);
						}
						else
						{
							ME_ExportShed = credential.Company.Left(3);
						}
						ME_ExportLocation = credential.PIMA.SubstringSafe(0, 3);
					}
				}
				else
				{
					if (ME_Profile.Length > 3)
					{
						ME_ExportShed = ME_Profile.Right(3);
					}
					else
					{
						ME_ExportShed = ZString.Empty;
					}
				}

				if (ME_Profile != oldValue)
				{
					CalculateMUCR(false);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.ShedsList))]
		public override ZString ME_ExportShed
		{
			get { return base.ME_ExportShed; }
			set { base.ME_ExportShed = value; }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.LocationOfGoodsList))]
		public override ZString ME_ExportLocation
		{
			get { return base.ME_ExportLocation; }
			set { base.ME_ExportLocation = value; }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.CustomsStatisticalReferenceList))]
		public override ZString ME_CustomsStatisticalReference
		{
			get { return base.ME_CustomsStatisticalReference; }
			set { base.ME_CustomsStatisticalReference = value; }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.CommunityTransitList))]
		[ReadOnlyMember(nameof(NOT_ME_UseAntiSmugglingTrptid))]
		public override ZString ME_CommunityTransitStatus
		{
			get { return base.ME_CommunityTransitStatus; }
			set { base.ME_CommunityTransitStatus = value; }
		}

		public override ZBool ME_UseAntiSmugglingTrptid
		{
			get { return base.ME_UseAntiSmugglingTrptid; }
			set
			{
				base.ME_UseAntiSmugglingTrptid = value;
				CalculateCTStatusIfNeeded();
			}
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.MasterOptList))]
		public override ZString ME_MasterOpt
		{
			get { return base.ME_MasterOpt; }
			set { base.ME_MasterOpt = value; }
		}

		bool NOT_ME_UseAntiSmugglingTrptid
		{
			get { return !ME_UseAntiSmugglingTrptid; }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.RouteOfEntryList))]
		[MaxLength(3)]
		public ZString ME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding
		{
			get { return ME_ChiefMasterRouteOfEntry.IsEmpty ? string.Empty : StatusChecker.GetStatusCodeFromRouteOfEntryStatic(ME_ChiefMasterRouteOfEntry); }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.CountriesList))]
		[ReadOnlyMember(nameof(ME_UseAntiSmugglingTrptid))]
		public override ZString ME_TransportCountry
		{
			get { return base.ME_TransportCountry; }
			set { base.ME_TransportCountry = value; }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.TransportTypeList))]
		[ReadOnlyMember(nameof(ME_UseAntiSmugglingTrptid))]
		public override ZString ME_TransportMode
		{
			get { return base.ME_TransportMode; }
			set { base.ME_TransportMode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.StyleOfEntryList))]
		[MaxLength(2)]
		public override ZString ME_ChiefMasterStyleOfEntry
		{
			get { return base.ME_ChiefMasterStyleOfEntry; }
			set { base.ME_ChiefMasterStyleOfEntry = value; }
		}

		[List(nameof(Lookups) + "." + nameof(MawbExportAddInfoLookups.CustomsReturnCodeList))]
		public override ZString ME_ChiefCustomsReturnCode
		{
			get { return base.ME_ChiefCustomsReturnCode; }
			set { base.ME_ChiefCustomsReturnCode = value; }
		}

		public override bool IsDeleted
		{
			get { return false; }
		}

		public ZString CustomsProfile => ME_Profile;

		public ZString MessageType => EUJobMessageTypeList.Codes.Export;

		public ZString MasterBill => Consol?.JK_MasterBillNum ?? ZString.Empty;

		public ZString HouseSplitReference => ZString.Empty;

		public ZString HouseBill => ZString.Empty;

		public bool IsInventoryControlledAirImport => false;

		public ZString LocationOfGoods => ME_ChiefGoodsLocation;

		public ZString SubLocation => ZString.Empty;

		public ZString SubLocationOfGoods => LocationOfGoods.Right(3) + SubLocation;

		public ForwardingConsol RelevantConsol => Consol;

		public GlbBranch Branch => GlbBranch.CurrentBranch;

		public Business.Declaration.InvoiceLineCompleteCollection GetInvoiceLines() => null;

		public ZBool IsSea => Consol?.IsSea ?? false;

		public OrgAddress Declarant =>
			(Consol?.JK_OA_SendingForwarderAddress.IsValid ?? false) && !(bool)GBCustomsDataRegistry.Instance.GB_UseBranchEoriForMucrOnConsols.GetValueWithFallbackDefault(Guid.Empty, Branch.PK.ToGuid(), Guid.Empty)
			? Consol.SendingForwarderAddress
			: Branch?.OrgProxy?.MainAddress;

		public ZString JobReference => Consol?.JK_UniqueConsignRef ?? ZString.Empty;

		public ZBool FindGen51Statement => ZBool.False;

		ForwardingConsol consol;

		// No support for Courier from consol yet - for the future
		ZString IConsolMessagingProvider.CourierSiteId => "";
		ZString IConsolMessagingProvider.CourierCarrierCode => "";
		ZString IConsolMessagingProvider.CourierConsignmentReference => "";
	}
}
