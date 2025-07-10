using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Common.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(factory.GetCachedValue<EDIReleaseImportEntryStatusList>());
			result.AddPairIfNotExist(ExtraConstantCodes.Codes.NotReleased, ExtraConstantCodes.Descriptions.NotReleased);
			return result;
		}
	}
}
