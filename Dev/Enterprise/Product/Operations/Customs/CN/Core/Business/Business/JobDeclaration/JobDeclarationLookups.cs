using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class JobDeclarationLookups : AutoCNJobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration => Parent;

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override CodeDescriptionPairList EntryStatusListForDefaultFallBack => Factory.GetCachedValue<Common.CN.EntryStatusList>();

		protected override CodeDescriptionPairList PackingUnitTypesListCore => JE_TotalNoOfPacksPackType_List;

		protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			if (!Parent.IsDeclarationIntegrated)
			{
				yield return JobMessageTypeList.Codes.Drawback;
				yield return JobMessageTypeList.Codes.Refund;
				yield return JobMessageTypeList.Codes.MiscellaneousCustoms;
				yield return JobMessageTypeList.Codes.ExWarehouse;
			}
		}

		public override CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				CodeDescriptionPairList result;
				if (Parent.IsAir || Parent.IsPost)
				{
					result = Factory.GetCachedValue("CNDeclarationCargoIdTypeListAirMail", () =>
					{
						var tempList = new CodeDescriptionPairList();
						tempList.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
						return tempList;
					});
				}
				else
				{
					result = base.CargoIdTypeList;
				}
				return result;
			}
		}

		public override CodeDescriptionPairList MergeByList =>
			Factory.GetCachedValue("CNDeclarationMergeByList", () =>
			{
				var result = new CodeDescriptionPairList(base.MergeByList);
				result.RemoveCode(OrgConstants.MergeInvoiceLines.Classification);
				result.RemoveCode(OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways);
				return result;
			});

		public override CodeDescriptionPairList MessageSubTypeList => DecTypeList.GetDecTypeList(Parent);

		public override CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportTypeList>();

		public new ZZRefCusCodeListCombinedCollection CustomsOfficeList => CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, Parent.DateOfValuation);

		public RefUNLOCOCollection Ports => fPorts ?? (fPorts = new RefUNLOCOCollection(Factory));
		RefUNLOCOCollection fPorts;

		protected override ZQuery DestinationPortFilter()
		{
			return Declaration.WillGenerateBothEntries ? PortQuery(string.Empty, PortLocation.Local) : base.DestinationPortFilter();
		}

		protected override ZQuery OriginPortFilter()
		{
			return Declaration.WillGenerateBothEntries ? PortQuery(string.Empty, PortLocation.Local) : base.OriginPortFilter();
		}

		protected override ZQuery FinalDestinationPortFilter()
		{
			return Declaration.WillGenerateBothEntries ? PortQuery(string.Empty, PortLocation.Local) : base.FinalDestinationPortFilter();
		}

		protected override ZQuery DischargePortFilter()
		{
			return Declaration.WillGenerateBothEntries ? PortQuery(Declaration.JE_TransportMode, PortLocation.Local) : base.DischargePortFilter();
		}

		public CodeDescriptionPairList TransportModeInlandList
		{
			get
			{
				return Factory.GetCachedValue("CNJobDeclarationLookupsTransportModeInlandList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport, ResString.GetMultilingualString("9D2B7CFF-2E77-4F65-9BBB-BD05278FA2F2", "Waterway Transport"));
					result.AddPair(Customs.Business.TransportTypeList.Codes.Road, ResString.GetMultilingualString("C46DBDBE-7C9A-42E5-9A38-D0B751E579E0", "Road Transport"));
					result.AddPair(Customs.Business.TransportTypeList.Codes.Rail, ResString.GetMultilingualString("8101E5FF-7C88-436C-AAFF-D7489BF7CB03", "Rail Transport"));
					return result;
				});
			}
		}

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<JobMessageStatusList>();

		public ICodeDescriptionPairList CNTransportModeCodes
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("Customs.CN.Business.CNTransportModeCodes", () =>
				{
					var list = new CNTransportModeList();
					list.RemoveCode(CNTransportModeList.Codes.Air);
					list.RemoveCode(CNTransportModeList.Codes.Sea);
					list.RemoveCode(CNTransportModeList.Codes.Rail);
					list.RemoveCode(CNTransportModeList.Codes.Road);
					list.RemoveCode(CNTransportModeList.Codes.Mail);
					list.RemoveCode(CNTransportModeList.Codes.FixedTransportInstallations);
					list.RemoveCode(CNTransportModeList.Codes.PassengerCarried);
					return list;
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection CNPortList => CNRefCusCodeListTypes.GetCNPorts(Factory, Parent.DateOfValuation);

		public ZZRefCusCodeListCombinedCollection CNCIQPortList => CNRefCusCodeListTypes.GetCNCIQPorts(Factory, Parent.DateOfValuation);

		public ICodeDescriptionPairList ClearanceModeCodes => Factory.GetCachedValue<ClearanceModeList>();

		public ICodeDescriptionPairList TransitModeCodes => Factory.GetCachedValue<TransitModeList>();
	}
}
