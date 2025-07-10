using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.OperationalActions
{
	public class DeclarationUpdatePreviousDocumentsApplicator : EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator
	{
		public DeclarationUpdatePreviousDocumentsApplicator(BusinessObjectFactory factory, ZString dataGroupingCode) : base(factory, dataGroupingCode)
		{
		}

		public override CodeDescriptionPairList DocumentCodeList
		{
			get
			{
				return Factory.GetCachedValue("ESOperationalActions.PreviousDocumentCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(base.DocumentCodeList);
					result.AddRangeOverwriteIfExists(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, DataGroupingCode, UniversalReferenceConstants.RefCusCodeListTypes.DC40A, ZDateTime.Today));
					result.SortByDescription();
					return result;
				});
			}
		}
	}
}
