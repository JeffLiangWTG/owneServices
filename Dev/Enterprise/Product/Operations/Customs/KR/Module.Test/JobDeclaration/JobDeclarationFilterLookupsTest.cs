using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterLookups))]
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			var messageTypeList = lookups.MessageTypeList;
			AssertEquals(6, messageTypeList.Count);
			AssertEquals(KRJobMessageTypeList.Codes.Export, messageTypeList[0].Code);
			AssertEquals(KRJobMessageTypeList.Codes.LocalExport, messageTypeList[1].Code);
			AssertEquals(KRJobMessageTypeList.Codes.Import, messageTypeList[2].Code);
			AssertEquals(KRJobMessageTypeList.Codes.PersonalItems, messageTypeList[3].Code);
			AssertEquals(KRJobMessageTypeList.Codes.ValuationDeclaration, messageTypeList[4].Code);
			AssertEquals(KRJobMessageTypeList.Codes.Carnet, messageTypeList[5].Code);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
		}
		JobDeclarationFilterLookups lookups;
	}
}
