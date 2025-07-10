using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ProcessRelatedNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_EntryType()
		{
			var dec = Factory.New<JobDeclaration>();
			var num = dec.ProcessRelatedNumbers.AddNew();
			num.CE_EntryType = "";
			AssertHasMessageErrorContaining(num.CE_EntryTypeInfo, "entered");
			num.CE_EntryType = "XXX";
			AssertNoMessageErrorContaining(num.CE_EntryTypeInfo, "entered");
			AssertHasMessageErrorContaining(num.CE_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
			num.CE_EntryType = ProcessRelatedTypeList.Codes.ADM;
			AssertNoMessageErrorContaining(num.CE_EntryTypeInfo, "entered");
			AssertNoMessageErrorContaining(num.CE_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
