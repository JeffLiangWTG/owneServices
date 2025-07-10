using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ProcessRelatedNumberCollection))]
	public class CusEntryNumProcessRelatedCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoadCorrectly()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			var num1 = Factory.New<ProcessRelatedNumber>();
			num1.CE_Category = "ABC";
			num1.CE_ParentTable = dec.TableName;
			num1.CE_ParentID = dec.PK;
			num1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var num2 = Factory.New<ProcessRelatedNumber>();
			num2.CE_Category = "DRE";
			num2.CE_ParentTable = dec.TableName;
			num2.CE_ParentID = dec.PK;
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			var num3 = Factory.New<ProcessRelatedNumber>();
			num3.CE_Category = "DRE";
			num3.CE_ParentTable = dec2.TableName;
			num3.CE_ParentID = dec2.PK;
			num3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			dec.ProcessRelatedNumbers.Load();
			AssertEquals(1, dec.ProcessRelatedNumbers.Count);
			AssertEquals(num2, dec.ProcessRelatedNumbers[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new ProcessRelatedNumberCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ProcessRelatedNumber>();
		}
	}
}
