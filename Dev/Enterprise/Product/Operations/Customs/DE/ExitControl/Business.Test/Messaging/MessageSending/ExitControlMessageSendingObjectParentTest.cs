using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	[TestedType(typeof(ExitControlMessageSendingObjectParent))]
	sealed class ExitControlMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendAndSave()
		{
			var messageSendingObjectParent = (ExitControlMessageSendingObjectParent)GetNewBusinessObject();
			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				var numberOfMessages = messageSendingObjectParent.SendAndSaveMessages();
				AssertEquals(1, numberOfMessages);
			}
		}

		public void TestMessageSendingObjectProperties()
		{
			var messageSendingObjectParent = (ExitControlMessageSendingObjectParent)GetNewBusinessObject();
			var properties = messageSendingObjectParent.MessageSendingObjectProperties.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Expecting 8 columns for ExitControlMessageSendingObject", 8, properties.Length);
				AssertMessageSendingObjectProperty(properties[0], expectedColumnName: nameof(ExitControlMessageSendingObject.Type), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[1], expectedColumnName: nameof(ExitControlMessageSendingObject.TransportID), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[2], expectedColumnName: nameof(ExitControlMessageSendingObject.Location), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[3], expectedColumnName: nameof(ExitControlMessageSendingObject.DateTime), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[4], expectedColumnName: nameof(ExitControlMessageSendingObject.ExitOffice), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[5], expectedColumnName: nameof(ExitControlMessageSendingObject.MRN_LRN), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[6], expectedColumnName: nameof(ExitControlMessageSendingObject.CustomsStatus), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[7], expectedColumnName: nameof(ExitControlMessageSendingObject.MessageStatus), expectedIsMandatory: true, expectedWidth: 80);
			});

			void AssertMessageSendingObjectProperty(MessageSendingObjectProperty property, string expectedColumnName, bool expectedIsMandatory, int expectedWidth)
			{
				AssertEquals("PropertyName", expectedColumnName, property.PropertyName);
				AssertEquals("IsMandatory", expectedIsMandatory, property.IsMandatory);
				AssertEquals("ColumnWidth", expectedWidth, property.ColumnWidth);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.New<CusExitHeader>();
			var carrierOrg = Factory.New<OrgHeader>();
			carrierOrg.OH_Code = "CARRIER";
			carrierOrg.CustomsCodes.AddNew("EOR", "EOR123", Core.Constants.CountryCodes.Greece);
			carrierOrg.MainAddress.CustomsCodes.AddNew("EBS", "EBS123", Core.Constants.CountryCodes.Germany);
			exitHeader.CXH_OA_Carrier = carrierOrg.MainAddress.PK;
			var consignment = exitHeader.CusExitConsignments.AddNew();
			var cusExitReport = exitHeader.CusExitReports.AddNew();
			cusExitReport.CER_CXC_Consignment = consignment.PK;
		}

		protected override BusinessObject GetNewBusinessObject() => new ExitControlMessageSendingObjectParent(exitHeader);

		CusExitHeader exitHeader;
	}
}
