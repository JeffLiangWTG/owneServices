using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Testing
{
	public class NumberGeneratorHelperTest : TestCaseWithFactory
	{
		public void TestIsUniqueEntryLocalReferenceNumber()
		{
			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryNum = "ENT008";
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.DrawbackClaim;
			entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNum.CE_ParentTable = JobDeclaration.Schema.TableName;

			AssertEquals(true, NumberGeneratorHelper.IsUniqueEntryNumber(Factory, JobDeclaration.Schema.TableName, "ENT008", CusEntryNumberTypes.Standard.DrawbackClaim, Core.Constants.CountryCodes.France));

			Factory.Save();
			AssertEquals(false, NumberGeneratorHelper.IsUniqueEntryNumber(Factory, JobDeclaration.Schema.TableName, "ENT008", CusEntryNumberTypes.Standard.DrawbackClaim, Core.Constants.CountryCodes.France));
		}
	}
}
