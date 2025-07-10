using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsHeaderLookups : AutoQuarantineColsHeaderLookups
	{
		public QuarantineColsHeaderLookups(AutoQuarantineColsHeader parent) : base(parent)
		{
		}

		public new QuarantineColsHeader Parent => (QuarantineColsHeader)base.Parent;

		public OrgHeaderCollection AllOrganisations
		{
			get { return allOrganisations ?? (allOrganisations = new OrgHeaderCollection(Parent.Factory)); }
		}
		OrgHeaderCollection allOrganisations;

		public CodeDescriptionPairList COLSHeaderStatusList => Factory.GetCachedValue<COLSHeaderStatusList>();

		public CodeDescriptionPairList COLSLodgementStatusList => Factory.GetCachedValue<COLSLodgementStatusList>();

		public CodeDescriptionPairList DeliveryClassification => Factory.GetCachedValue<COLSDeliveryClassificationList>();

		public CodeDescriptionPairList LateLodgementReasonsList
		{
			get
			{
				return Factory.GetCachedValue("QuarantineColsHeaderLookups|LateLodgementReasonsList", () =>
				{
					var result = new CodeDescriptionPairList();
					var cusCodes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Australia,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AUCOLSLateLodgementReason,
						ZDateTime.Today);
					foreach (var cusCode in cusCodes)
					{
						result.AddPair(cusCode.ZZD_Description);
					}
					result.Sort();
					return result;
				});
			}
		}
	}
}
