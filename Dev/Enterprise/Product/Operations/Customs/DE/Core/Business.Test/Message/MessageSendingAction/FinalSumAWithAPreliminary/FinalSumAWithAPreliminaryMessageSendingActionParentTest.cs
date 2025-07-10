using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(FinalSumAWithAPreliminaryMessageSendingActionParent))]
	class FinalSumAWithAPreliminaryMessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingOjectCollection()
		{
			declaration.CusTempStorageLines[0].TSL_CustomsStatus = CustomsStatusList.Codes.TST;
			var parent = (FinalSumAWithAPreliminaryMessageSendingActionParent)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				var sendingObjectsCollection = parent.SendingObjectsCollection;
				AssertType<FinalSumAWithAPreliminaryMessageSendingActionCollection>("SendingObjectsCollection", sendingObjectsCollection);
				AssertEquals("Sendable lines", 1, sendingObjectsCollection.Count);
			});
		}

		public void TestMessageSendingObjectProperties()
		{
			var parent = (FinalSumAWithAPreliminaryMessageSendingActionParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;
			CombineAssertions(() =>
			{
				AssertEquals("Columns", 8, properties.Count());

				var lineNoProperty = properties.ElementAt(0);
				AssertEquals("Line No. Caption", "Line No.", lineNoProperty.ResourceString.Caption);
				AssertEquals("Line No. Width", 63, lineNoProperty.ColumnWidth);

				var descriptionOfGoodsProperty = properties.ElementAt(1);
				AssertEquals("Description Of Goods Caption", "Description Of Goods", descriptionOfGoodsProperty.ResourceString.Caption);
				AssertEquals("Description Of Goods Width", 150, descriptionOfGoodsProperty.ColumnWidth);

				var ownerReferenceTypeProperty = properties.ElementAt(2);
				AssertEquals("Owner Reference Type Caption", "Owner Reference Type", ownerReferenceTypeProperty.ResourceString.Caption);
				AssertEquals("Owner Reference Type Width", 135, ownerReferenceTypeProperty.ColumnWidth);

				var ownerReferenceNumberProperty = properties.ElementAt(3);
				AssertEquals("Owner Reference Number Caption", "Owner Reference Number", ownerReferenceNumberProperty.ResourceString.Caption);
				AssertEquals("Owner Reference Number Width", 270, ownerReferenceNumberProperty.ColumnWidth);

				var packageCountProperty = properties.ElementAt(4);
				AssertEquals("Package Count Caption", "Package Count", packageCountProperty.ResourceString.Caption);
				AssertEquals("Package Count Width", 97, packageCountProperty.ColumnWidth);

				var packageTypeProperty = properties.ElementAt(5);
				AssertEquals("Package Type Caption", "Package Type", packageTypeProperty.ResourceString.Caption);
				AssertEquals("Package Type Width", 89, packageTypeProperty.ColumnWidth);

				var custodianEORI = properties.ElementAt(6);
				AssertEquals("Custodian EORI Caption", "Custodian EORI", custodianEORI.ResourceString.Caption);
				AssertEquals("Custodian EORI Width", 97, custodianEORI.ColumnWidth);

				var custodianBranchEORI = properties.ElementAt(7);
				AssertEquals("Custodian Branch Caption", "Custodian Branch", custodianBranchEORI.ResourceString.Caption);
				AssertEquals("Custodian Branch Width", 106, custodianBranchEORI.ColumnWidth);
			});
		}

		public void TestMessageErrorCollector()
		{
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var line1 = declaration.CusTempStorageLines[0];
			line1.TSL_GoodsDescription = "TEST";
			var line2 = declaration.CusTempStorageLines[1];
			line2.TSL_GrossWeight = 1;
			CombineAssertions(() =>
			{
				var parent = (FinalSumAWithAPreliminaryMessageSendingActionParent)GetNewBusinessObject();
				var messageErrors = parent.BizObjValidationMessageErrors;
				var headerCustomsOfficeMessageError = header.SJH_CustomsOfficeInfo.HumanReadableName + ":";
				var grossWeightMessageError = line1.TSL_GrossWeightInfo.HumanReadableName + ":";
				var goodsDescriptionMessageError = line2.TSL_GoodsDescriptionInfo.HumanReadableName + ":";

				AssertContains("Header has MessageError", headerCustomsOfficeMessageError, messageErrors);
				AssertContains("line1 - selected to send", grossWeightMessageError, messageErrors);
				AssertContains("line2 - selected to send", goodsDescriptionMessageError, messageErrors);

				parent.SendingObjectsCollection[0].ShouldSend = false;
				messageErrors = parent.BizObjValidationMessageErrors;
				AssertContains("Header MessageError still there", headerCustomsOfficeMessageError, messageErrors);
				AssertNotContains("line1 - not selected to send", grossWeightMessageError, messageErrors);
				AssertContains("line2 - selected to send", goodsDescriptionMessageError, messageErrors);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new FinalSumAWithAPreliminaryMessageSendingActionParent(header, declaration.CusTempStorageLines.Cast<CusTempStorageLine>(), Env.Security.CustomsTemporaryStorageSendWithMessageErrors);

		protected override void SetUp()
		{
			header = Factory.New<CusTempStorageJobHeader>();
			declaration = CUSPRLCusTempStorageDec.New(header);
			declaration.CusTempStorageLines.AddNew();
			declaration.CusTempStorageLines.AddNew();
		}
		CusTempStorageJobHeader header;
		CUSPRLCusTempStorageDec declaration;
	}
}
