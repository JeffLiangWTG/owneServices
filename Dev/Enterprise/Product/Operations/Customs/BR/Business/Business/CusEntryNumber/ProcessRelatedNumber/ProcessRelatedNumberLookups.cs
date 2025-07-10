using Enterprise.Customs.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ProcessRelatedNumberLookups : CusEntryNumLookups
	{
		public ProcessRelatedNumberLookups(ProcessRelatedNumber parent)
			: base(parent)
		{
		}

		JobDeclaration Declaration => Parent.Parent as JobDeclaration;

		public override CodeDescriptionPairList AdditionalReferenceNumberTypes
		{
			get
			{
				var isImportOnly = Declaration?.IsImportOnly ?? false;

				return Factory.GetCachedValue("BR.ProcessRelatedNumberLookups.ProcessRelatedTypeList_" + isImportOnly, () =>
				{
					var result = new ProcessRelatedTypeList();
					if (isImportOnly)
					{
						result.RemoveCode(ProcessRelatedTypeList.Codes.JUD);
						result.RemoveCode(ProcessRelatedTypeList.Codes.PRE);
						result.RemoveCode(ProcessRelatedTypeList.Codes.EJD);
					}
					return result;
				});
			}
		}
	}
}
