using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitReportLookups : ExitControlBase.Business.CusExitReportLookups
	{
		public CusExitReportLookups(AutoCusExitReport parent)
			: base(parent)
		{
		}

		public new CusExitReport Parent => (CusExitReport)base.Parent;

		public virtual CodeDescriptionPairList DeclarantTypeList => Factory.GetCachedValue("EU.ExitControl.DeclarantTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(RepresentationTypeList.Codes._2Direct, RepresentationTypeList.Descriptions._2Direct);
			result.AddPair(RepresentationTypeList.Codes._3Indirect, RepresentationTypeList.Descriptions._3Indirect);
			return result;
		});
		public virtual CodeDescriptionPairList DiscrepancyTypeList => Factory.GetCachedValue<ExitReportDiscrepancyTypeList>();
		public virtual CodeDescriptionPairList TypeList => Factory.GetCachedValue<ExitReportTypeList>();
		public virtual CodeDescriptionPairList TransportModeList => Factory.GetCachedValue<TransportTypeList>();

		public virtual CodeDescriptionPairList TransportTypeList
		{
			get
			{
				var transportMode = Parent.CER_TransportMode;
				return Factory.GetCachedValue("EU.CusExitReportLookups.TransportTypeList." + transportMode, () => GetApplicableTransportTypes(transportMode));
			}
		}

		CodeDescriptionPairList GetApplicableTransportTypes(ZString transportMode)
		{
			var transportTypesList = new CodeDescriptionPairList();
			switch (transportMode)
			{
				case Enterprise.Customs.Business.TransportTypeList.Codes.Sea:
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._10, CusExitReportTransportTypeList.Descriptions._10);
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._11, CusExitReportTransportTypeList.Descriptions._11);
					break;
				case Customs.Business.TransportTypeList.Codes.Rail:
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._21, CusExitReportTransportTypeList.Descriptions._21);
					break;
				case Customs.Business.TransportTypeList.Codes.Road:
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._30, CusExitReportTransportTypeList.Descriptions._30);
					break;
				case Customs.Business.TransportTypeList.Codes.Air:
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._40, CusExitReportTransportTypeList.Descriptions._40);
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._41, CusExitReportTransportTypeList.Descriptions._41);
					break;
				case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._80, CusExitReportTransportTypeList.Descriptions._80);
					transportTypesList.AddPair(CusExitReportTransportTypeList.Codes._81, CusExitReportTransportTypeList.Descriptions._81);
					break;
				default:
					transportTypesList = new CusExitReportTransportTypeList();
					break;
			}
			return transportTypesList;
		}

		public CodeDescriptionPairList StatusList => StatusListCore;
		protected virtual CodeDescriptionPairList StatusListCore => Factory.GetCachedValue<AESEntryStatusList>();

		public CodeDescriptionPairList MessageStatusList => MessageStatusListCore;
		protected virtual CodeDescriptionPairList MessageStatusListCore => Factory.GetCachedValue<LogicalStatusList>();

		public virtual CustomsOfficeCodeCollection OfficeOfExitList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles
			(Factory, Parent.CountryCode, new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland });

		public override RefCountryCollection TransportNationalities
		{
			get
			{
				var transportNationalitiesCache = new RefCountryCollection(Factory);
				transportNationalitiesCache.ApplySort(RefCountrySchema.RN_Desc.Name, ListSortDirection.Ascending);
				return transportNationalitiesCache;
			}
		}
	}
}
