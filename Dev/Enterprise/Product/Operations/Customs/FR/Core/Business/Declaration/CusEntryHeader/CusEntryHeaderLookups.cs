using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderLookups(CusEntryHeader parent) : EU.Business.Declaration.CusEntryHeaderLookups(parent)
	{
		public CodeDescriptionPairList TriggeringPointForValidationList => GetTriggeringPointForValidationListCore(Parent.Factory);

		CodeDescriptionPairList GetTriggeringPointForValidationListCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.FR.Business.Declaration.CusEntryHeaderLookups.GetTriggeringPointForValidationListCore", () =>
			{
				var result = new CodeDescriptionPairList();

				var entryHeader = Parent;
				if (Parent != null && entryHeader != null)
				{
					var triggerPointsConfiguration = new TriggerPointsConfiguration();
					result = entryHeader.IsImport ? triggerPointsConfiguration.ImportTriggerPointsCodeList : triggerPointsConfiguration.ExportTriggerPointsCodeList;
				}

				return result;
			});
		}
	}
}
