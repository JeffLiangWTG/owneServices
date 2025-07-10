using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMaxEntryLinesExceeded()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();

			// 99th
			var lineNo = (short)99;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = lineNo++;

			AssertEquals("entryLine should have line number: 99", (short)99, entryLine.CL_LineNumber);
			var maxCount = "99";

			AssertNoMessageError(entryLine.CL_LineNumberInfo, $"The maximum number of entry lines permitted is {maxCount}");

			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = lineNo++;
			AssertHasMessageError(entryLine.CL_LineNumberInfo, $"The maximum number of entry lines permitted is {maxCount}");

			maxCount = "100";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.MaxEntryLines, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MaxEntryLines, maxCount))
			{
				entryLine.Validation.ValidateCL_LineNumber();
				AssertNoMessageError(entryLine.CL_LineNumberInfo, $"The maximum number of entry lines permitted is 99");
				AssertNoMessageError(entryLine.CL_LineNumberInfo, $"The maximum number of entry lines permitted is {maxCount}");

				entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_LineNumber = lineNo++;
				AssertHasMessageError(entryLine.CL_LineNumberInfo, $"The maximum number of entry lines permitted is {maxCount}");

				entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;
				AssertNoMessageError(entryLine.CL_LineNumberInfo, $"The maximum number of entry lines permitted is {maxCount}");
			}
		}
	}
}
