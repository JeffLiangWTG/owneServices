using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderLookups))]
	public class CusEntryHeaderLookupsTest : EU.Business.Declaration.Testing.CusEntryHeaderLookupsTest
	{
		public void TestTriggeringPointForValidationList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertContainsExactElementsInAnyOrder(new string[] { TriggerPointsCodeList.Codes.PAB, TriggerPointsCodeList.Codes.VAQ, TriggerPointsCodeList.Codes.NUL }, entryHeader.Lookups.TriggeringPointForValidationList.GetAllCodes());

			Factory.ClearCachedValue<CodeDescriptionPairList>("Enterprise.Customs.FR.Business.Declaration.CusEntryHeaderLookups.GetTriggeringPointForValidationListCore");
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder(new string[] { TriggerPointsCodeList.Codes.REC, TriggerPointsCodeList.Codes.NUL }, entryHeader.Lookups.TriggeringPointForValidationList.GetAllCodes());
		}
	}
}
