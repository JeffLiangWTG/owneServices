using CargoWise.Integration;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Module
{
	public class DeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public DeclarationFilterLookups(Customs.Module.JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public CodeDescriptionPairList DeferredStatus
		{
			get
			{
				return Factory.GetCachedValue("8E39FC53-F7A6-4D1C-B627-50D8CFBD1F79", () =>
				{
					var deferredStatusList = new CodeDescriptionPairList();
					deferredStatusList.AddRange(Declaration.AddInfoLookups.DeferredSubmissionList);
					deferredStatusList.AddPair(DeferredCodes.All, Res.GetString("724B5659-D076-4D40-8A97-890C15FD0412", "Show all records"));
					return deferredStatusList;
				});
			}
		}

		public static class DeferredCodes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
			public const string All = "All";
		}

		public ICodeDescriptionPairList DeclarantTypes => Declaration.Lookups.DeclarantTypeList;

		public CodeDescriptionPairList DestinationTypes => Declaration.Lookups.MessageSubTypeList;

		public CodeDescriptionPairList GuarantorTypes => Declaration.AddInfoLookups.GuarantorTypeList;

		public CodeDescriptionPairList OriginTypes => Declaration.AddInfoLookups.OriginTypeList;

		public CodeDescriptionPairList TransportArrangements => Declaration.AddInfoLookups.TransportArrangementList;

		public CodeDescriptionPairList TransportModes => Declaration.Lookups.TransportTypeList;

		public override CodeDescriptionPairList EntryStatusList() => Declaration.Lookups.EntryStatusList;

		public override CodeDescriptionPairList MessageStatusList() => Declaration.Lookups.MessageStatusList;

		public OrgHeaderCollection AllOrganisations => new OrgHeaderCollection(Factory);

		EMCSJobDeclaration Declaration => declaration ?? (declaration = Factory.GetNull<EMCSJobDeclaration>());
		EMCSJobDeclaration declaration;
	}
}
