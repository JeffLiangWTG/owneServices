using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.OperationalActions
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
				return Factory.GetCachedValue("GBOperationalActions.DocumentCodeListCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(base.DocumentCodeList);
					result.AddRangeOverwriteIfExists(Factory.GetCachedValue<PreviousDocumentCodeListCDS>());
					result.SortByDescription();
					return result;
				});
			}
		}
	}
}
