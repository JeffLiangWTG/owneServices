using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.OperationalActions
{
	public class DeclarationUpdatePreviousDocumentsApplicator : EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator
	{
		public DeclarationUpdatePreviousDocumentsApplicator(BusinessObjectFactory factory, ZString dataGroupingCode) : base(factory, dataGroupingCode)
		{
		}

		public override CodeDescriptionPairList ClassCodeList
		{
			get
			{
				return Factory.GetCachedValue("DEOperationalActions.ClassCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(base.ClassCodeList);
					result.AddRangeOverwriteIfExists(Factory.GetCachedValue<PreviousDocSubTypeList>());
					result.SortByDescription();
					return result;
				});
			}
		}
	}
}
