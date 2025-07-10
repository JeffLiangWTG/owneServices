using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingConditionDetailLookups : ZLookups
	{
		public GuidedDecisionMakingConditionDetailLookups(GuidedDecisionMakingConditionDetail parent) : base(parent)
		{
		}

		protected new GuidedDecisionMakingConditionDetail Parent => (GuidedDecisionMakingConditionDetail)base.Parent;

		public ZZRefCusCodeListCombinedCollection ConditionDetailCodesList => Factory.GetCachedValue("EU.GuidedDecisionMakingConditionDetailLookups.ConditionDetailCodesList_" + Parent.Parents[0].Parent.DataGrouping + (Parent.Parents[0].IsImport ? (NoResString)"Import" : (NoResString)"Export"), () =>
		{
			return new ZZRefCusCodeListCombinedCollection(Factory, Parent.Parents[0].Parent.DataGrouping, Parent.Parents[0].IsImport ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, ZDateTime.Today);
		});
	}
}
