using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DispatchInstructionNumberCollection))]
	class DispatchInstructionNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoadCorrectly()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var num1 = Factory.New<CusEntryNumber>();
			num1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num1.CE_ParentTable = dec.TableName;
			num1.CE_ParentID = dec.PK;
			num1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var num2 = Factory.New<DispatchInstructionNumber>();
			num2.CE_Category = CusEntryNumber.Categories.DispatchInstructionDocument;
			num2.CE_ParentTable = dec.TableName;
			num2.CE_ParentID = dec.PK;
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var num3 = Factory.New<DispatchInstructionNumber>();
			num3.CE_Category = CusEntryNumber.Categories.DispatchInstructionDocument;
			num3.CE_ParentTable = dec2.TableName;
			num3.CE_ParentID = dec2.PK;
			num3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			dec.DispatchInstructionNumbers.Load();
			AssertEquals(1, dec.DispatchInstructionNumbers.Count);
			AssertEquals(num2, dec.DispatchInstructionNumbers[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new DispatchInstructionNumberCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DispatchInstructionNumber>();
		}
	}
}
