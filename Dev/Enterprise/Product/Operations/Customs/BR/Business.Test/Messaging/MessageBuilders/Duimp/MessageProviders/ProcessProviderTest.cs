using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class ProcessProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(ProcessProvider.New(null));
			AssertType<ProcessProvider>(ProcessProvider.New(Factory.New<ProcessRelatedNumber>()));
		}

		public void TestProcess()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var relatedNumber = declaration.ProcessRelatedNumbers.AddNew();
			relatedNumber.CE_EntryNum = "897881450";

			var dataProvider = ProcessProvider.New(relatedNumber);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be", "ADMINISTRATIVO", dataProvider.Type);
				AssertEquals("Identification should be", "897881450", dataProvider.Identification);
			});
		}
	}
}

