using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationProcessRelatedProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationProcessRelatedProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var procRelated = declaration.ProcessRelatedNumbers.AddNew();
			procRelated.CE_EntryType = ProcessRelatedTypeList.Codes.ADM;
			procRelated.CE_EntryNum = "PROC00001";

			var declarationProcRelated = new DeclarationProcessRelatedProvider(procRelated);

			AssertEquals("ReferenceTypeCode should be", ProcessRelatedTypeList.MapToCustomsCode(ProcessRelatedTypeList.Codes.ADM), declarationProcRelated.ReferenceTypeCode);
			AssertEquals("ReferenceNumber should be", "PROC00001", declarationProcRelated.ReferenceNumber);
		}
	}
}


