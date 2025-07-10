using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	class B2JobDeclarationValidationTest : TestCaseWithFactory
	{
		[TestDate(2020, 09, 16)]
		public virtual void TestCheckJE_EntryAuthorisationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2016, 09, 15);
			AssertHasWarning(declaration.JE_EntryAuthorisationDateInfo, "The date '15-Sep-2016' is more than 4 years old.");

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2020, 09, 15);
			AssertNoWarning(declaration.JE_EntryAuthorisationDateInfo, "The date '15-Sep-2016' is more than 4 years old.");
		}
	}
}
