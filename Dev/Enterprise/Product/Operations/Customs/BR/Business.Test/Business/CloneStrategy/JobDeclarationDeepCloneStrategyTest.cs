using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class JobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneJobDeclarationCustomsOffices()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.BoardingOfficeCode = "1234567";
			declaration.BoardingOfficeIsCustomsEnclosure = true;

			var clonedDeclaration = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(declaration.BoardingOfficeCode, clonedDeclaration.BoardingOfficeCode);
			AssertEquals(declaration.BoardingOfficeIsCustomsEnclosure, clonedDeclaration.BoardingOfficeIsCustomsEnclosure);
		}

		public void TestCloneJobDeclarationCustomsEnclosures()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.BoardingEnclosureCode = "1234567";

			var clonedDeclaration = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(declaration.BoardingEnclosureCode, clonedDeclaration.BoardingEnclosureCode);
		}

		public void TestCloneJobDeclarationEntranceOffice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.EntranceOfficeCode = "1234567";

			var clonedDeclaration = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(declaration.EntranceOfficeCode, clonedDeclaration.EntranceOfficeCode);
		}

		public void TestNotCloneJE_UCR()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_UCR = "123456";

			var clonedDeclaration = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals("JE_UCR should NOT be cloned", ZString.Empty, clonedDeclaration.JE_UCR);
		}

		public void TestFixedJobMessageType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var clonedDeclaration = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals("FixedJobMessageType should be cloned", BRJobMessageTypeList.Codes.ImportLicense, clonedDeclaration.FixedJobMessageType);
		}
	}
}
