using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(FinalSumAWithAPreliminaryMessageSendingAction))]
	class FinalSumAWithAPreliminaryMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingActionProperties()
		{
			line.TSL_LineNo = 1;
			line.TSL_GoodsDescription = "TestDescription";
			line.TSL_OwnerReferenceType = "ZZZ";
			line.TSL_OwnerReferenceNumber = "12345";
			line.TSL_PackageQty = 10;
			line.TSL_PackageType = "CT";
			line.TSL_CustodianIdentifier = "DE123456";
			line.TSL_CustodianIdentifierBranchNo = "0001";
			var action = (FinalSumAWithAPreliminaryMessageSendingAction)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertType<CUSPRLCusTempStorageLine>("MessageObject", action.MessagingObject);
				AssertEquals("LineNo", 1, action.LineNo);
				AssertEquals("Description", "TestDescription", action.Description);
				AssertEquals("OwnerReferenceType", "ZZZ", action.OwnerReferenceType);
				AssertEquals("OwnerReferenceNumber", "12345", action.OwnerReferenceNumber);
				AssertEquals("PackageCount", 10, action.PackageCount);
				AssertEquals("PackageType", "CT", action.PackageType);
				AssertEquals("CustodianEORI", "DE123456", action.CustodianEORI);
				AssertEquals("CustodianBranch", "0001", action.CustodianBranch);
				AssertEquals("ShouldSend", true, action.ShouldSend);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new FinalSumAWithAPreliminaryMessageSendingAction(line);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<CUSPRLCusTempStorageDec>();
			line = declaration.CusTempStorageLines.AddNew();
		}
		CusTempStorageLine line;
	}
}
