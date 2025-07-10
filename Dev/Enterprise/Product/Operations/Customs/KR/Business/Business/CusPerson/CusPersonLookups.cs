using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusPersonLookups : Customs.Business.CusPersonLookups
	{
		public CusPersonLookups(AutoCusPerson parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ImmigrantJobCodeList => Factory.GetCachedValue<ImmigrantJobCodeList>();
		public CodeDescriptionPairList ImmigrantEntryStatusList => Factory.GetCachedValue<YesNoList>();
		public FamilyRelationCollection RelationshipCodeList => KRCustomsRegistry.Instance.FamilyRelations.Value;
	}
}
