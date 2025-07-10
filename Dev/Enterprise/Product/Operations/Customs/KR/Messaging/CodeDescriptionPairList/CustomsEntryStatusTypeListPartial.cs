using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	partial class CustomsEntryStatusTypeList : IStatusList
	{
		public static bool IsDeclinedByCustoms(string code)
		{
			return code == Codes.DMS
				|| code == Codes.CCL
				|| code == Codes.RJC;
		}
		string IStatusList.GetDocumentPrintingWarningMessage()
		{
			return Res.GetString("EB9F23EF-8A70-4168-A41D-21EFD3EEE9D6", "This document has been declined by Customs.");
		}

		bool IStatusList.ShouldUsersBeWarnedPriorToPrintingDocument(string code)
		{
			return IsDeclinedByCustoms(code);
		}
		public static CodeDescriptionPairList GetStatusListForRefundDeclaration(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CustomsEntryStatusTypeList_GetStatusListForRefundDeclaration", () =>
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)Constants.Lists.KrOnlyTest);
				result.AddPair(Codes.NDC, Descriptions.NDC);
				result.AddPair(Codes.DMS, Descriptions.DMS);
				result.AddPair(Codes.ANT, Descriptions.ANT);
				result.AddPair(Codes.PNR, Descriptions.PNR);
				result.AddPair(Codes.PFL, Descriptions.PFL);
				return result;
			});
		}
	}
}
