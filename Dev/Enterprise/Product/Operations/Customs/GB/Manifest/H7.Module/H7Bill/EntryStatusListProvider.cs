using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.ZArchitecture.Core;
using StatusList = Enterprise.Customs.Common.EU.EntryStatusList;
using ThreeCharFunctionCodes = Enterprise.Customs.GB.CDS.Constants.ThreeCharFunctionCodes;

namespace Enterprise.Customs.GB.H7.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GBH7EntryStatusListProvider", () =>
			{
				var results = new CodeDescriptionPairList();
				results.Add(new CodeDescriptionPair(ThreeCharFunctionCodes.DeclarationAccepted, ResponseFunction.GetDescription(ThreeCharFunctionCodes.DeclarationAccepted)));
				results.Add(new CodeDescriptionPair(ThreeCharFunctionCodes.MessageRegistered, ResponseFunction.GetDescription(ThreeCharFunctionCodes.MessageRegistered)));
				results.Add(new CodeDescriptionPair(ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl, ResponseFunction.GetDescription(ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl)));
				results.Add(new CodeDescriptionPair(ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2, ResponseFunction.GetDescription(ThreeCharFunctionCodes.DeclarationSubjectToPhysicalControl2)));
				results.Add(new CodeDescriptionPair(ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue, ResponseFunction.GetDescription(ThreeCharFunctionCodes.DutiesTaxesCalculatedAndDue)));
				results.Add(new CodeDescriptionPair(StatusList.Codes.Clear, ResponseFunction.GetDescription(ThreeCharFunctionCodes.DeclarationCleared)));
				results.Add(new CodeDescriptionPair(StatusList.Codes.Cancelled, ResponseFunction.GetDescription(ThreeCharFunctionCodes.DeclarationCancelled)));
				return results;
			});
		}
	}
}
