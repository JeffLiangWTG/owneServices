using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration => (JobDeclaration)Parent;

		public ZZRefCusCodeListCombinedCollection ClearanceList => MXRefCusCodeListTypes.GetCustomsFacilities(Factory);

		public ZZRefCusCodeListCombinedCollection EntryAreaList => MXRefCusCodeListTypes.GetCustomsFacilities(Factory);

		protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			var baseMessageTypes = Factory.GetCachedValue<JobMessageTypeList>().GetAllCodesZString().ToList();
			baseMessageTypes.Remove(JobMessageTypeList.Codes.Import);
			baseMessageTypes.Remove(JobMessageTypeList.Codes.Export);

			return baseMessageTypes;
		}

		public override ICollection GoodsOrigin => Factory.GetCachedValue<GoodsRegionList>();

		public override ICollection GoodsDestination => Factory.GetCachedValue<GoodsRegionList>();

		public CodeDescriptionPairList CustomRegimeList => Declaration.IsImport ? CustomsRegimeList.GetCustomRegimeListForImport(Factory) : CustomsRegimeList.GetCustomRegimeListForExport(Factory);

		public override CodeDescriptionPairList MessageSubTypeList => Declaration.IsImport ? MXDeclarationTypeList.GetMessageSubTypeListForImport(Factory) : MXDeclarationTypeList.GetMessageSubTypeListForExport(Factory);

#if DEBUG
		public override CodeDescriptionPairList GetEffectiveMessageSubTypeList(ZString messageType)
		{
			var subTypeList = new CodeDescriptionPairList();
			subTypeList.AddRange(MessageSubTypeList.Cast<ICodeDescription>().Take(2).ToList());
			return subTypeList;
		}
#endif
	}
}
