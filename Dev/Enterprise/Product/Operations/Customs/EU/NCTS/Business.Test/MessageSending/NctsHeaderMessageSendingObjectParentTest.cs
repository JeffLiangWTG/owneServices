using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
	public class NctsHeaderMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderMessageSendingObjectParent(null));
		}

		public void TestTopLevelBusinessObject()
		{
			var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			AssertSame(nctsHeader, sendingObjectParent.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			AssertEquals(Env.Security.EuNctsSendWithMessageErrors, sendingObjectParent.SecurityCheckpointToSendWithMessageError);
		}

		public void TestMessageErrorCollector()
		{
			var targetInfo = nctsHeader.MovementHeader.BM_InBondEntryTypeInfo;
			CombineAssertions(() =>
			{
				nctsHeader.RunPreSaveValidation();
				AssertEquals("NctsHeader has MessageErrors", expected: true, nctsHeader.HasMessageErrors());
				AssertEquals("BM_InBondEntryTypeInfo has MessageErrors", expected: true, targetInfo.HasMessageErrors());
				var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				AssertContains("BM_InBondEntryTypeInfo MessageError is collected and displayed", targetInfo.HumanReadableName + ": " + targetInfo.GetMessageErrors().First().Message, messageSendingObjectParent.BizObjValidationMessageErrors);

				nctsHeader.RunPreSaveValidation();
				AssertEquals("NctsHeader has MessageErrors", expected: true, nctsHeader.HasMessageErrors());
				AssertEquals("BM_InBondEntryTypeInfo has MessageErrors", expected: true, targetInfo.HasMessageErrors());
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = false;
				AssertNullOrEmpty("BizObjValidationMessageErrors empty", messageSendingObjectParent.BizObjValidationMessageErrors);
			});
		}

		public void TestAdditionalWarnings()
		{
			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, new Dictionary<string, bool> { { "ShouldFillAdditionalWarningsOnSendScreenCore", true } }))
			{
				CombineAssertions(() =>
				{
					AssertEquals("ShouldFillAdditionalWarningsOnSendScreen", expected: true, nctsHeader.Configuration.MessageSendingConfiguration.ShouldFillAdditionalWarningsOnSendScreen);

					ZString fakeCyData = "QQ000001";

					var movementHeader = nctsHeader.MovementHeader;
					movementHeader.CustomsOffices.RemoveAndDeleteAll();
					var customsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
					customsOffice.CY_Data = fakeCyData;
					var targetInfo = customsOffice.CY_DataInfo;

					nctsHeader.RunPreSaveValidation();
					var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
					messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
					AssertEquals("NctsHeader has Warnings", expected: true, nctsHeader.HasWarnings());
					AssertEquals("CY_DataInfo has Warnings", expected: true, targetInfo.HasWarnings());
					AssertContains("CY_DataInfo Warning is collected and displayed", targetInfo.HumanReadableName + ": " + targetInfo.GetWarnings().First().Message, messageSendingObjectParent.AdditionalWarnings);

					messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = false;
					AssertEquals("NctsHeader has Warnings", expected: true, nctsHeader.HasWarnings());
					AssertEquals("CY_DataInfo has Warnings", expected: true, targetInfo.HasWarnings());
					AssertNullOrEmpty("AdditionalWarnings empty", messageSendingObjectParent.AdditionalWarnings);
				});
			}
		}

		public void TestAdditionalWarnings_ShouldNotFillAdditionalWarningsOnSendScreen()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ShouldFillAdditionalWarningsOnSendScreen", expected: false, nctsHeader.Configuration.MessageSendingConfiguration.ShouldFillAdditionalWarningsOnSendScreen);

				ZString fakeCyData = "QQ000001";

				var movementHeader = nctsHeader.MovementHeader;
				movementHeader.CustomsOffices.RemoveAndDeleteAll();
				var customsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
				customsOffice.CY_Data = fakeCyData;
				var targetInfo = customsOffice.CY_DataInfo;

				nctsHeader.RunPreSaveValidation();
				var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				AssertEquals("NctsHeader has Warnings", expected: true, nctsHeader.HasWarnings());
				AssertEquals("CY_DataInfo has Warnings", expected: true, targetInfo.HasWarnings());
				AssertNullOrEmpty("AdditionalWarnings empty", messageSendingObjectParent.AdditionalWarnings);
			});
		}

		public void TestMessageSendingValidation()
		{
			var parent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			AssertType<MessageSendingValidation>(parent.MessageSendingValidation);
		}

		public void TestSupportReleaseRequest()
		{
			var messageTypeList = new CodeDescriptionPairList();
			messageTypeList.AddPairIfNotExist("011", "011");
			var mock = new Mock<MessageSendingConfiguration>();
			var messageSendingParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			mock.Setup(m => m.MessageTypeList(It.IsAny<NctsHeader>())).Returns(messageTypeList);
			mock.Setup(m => m.ReleaseRequestCode).Returns("054");

			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
			{
				AssertEquals("When ReleaseRequestCode is not present in the message type list", false, messageSendingParent.SupportReleaseRequest);
			}

			messageTypeList.AddPairIfNotExist("054", "054");
			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
			{
				AssertEquals("When ReleaseRequestCode is present in the message type list", true, messageSendingParent.SupportReleaseRequest);
			}
		}

		public void TestSupportJustification()
		{
			var messageTypeList = new CodeDescriptionPairList();
			var mock = new Mock<MessageSendingConfiguration>();
			mock.Protected().Setup<bool>("ShowJustificationCore", ItExpr.IsAny<NctsHeader>()).Returns(true);
			mock.Setup(m => m.MessageTypeList(It.IsAny<NctsHeader>())).Returns(messageTypeList);
			var messageSendingParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);

			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
			{
				AssertEquals("When ShowJustification is true, No 014 Code in message type list", false, messageSendingParent.SupportJustification);
			}

			messageTypeList.AddPairIfNotExist("014", "014");
			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
			{
				AssertEquals("When ShowJustification is true, 014 Code is in message type list", true, messageSendingParent.SupportJustification);
			}

			mock.Protected().Setup<bool>("ShowJustificationCore", ItExpr.IsAny<NctsHeader>()).Returns(false);
			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
			{
				AssertEquals("When ShowJustification is false, 014 Code is in message type list", false, messageSendingParent.SupportJustification);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NctsHeaderMessageSendingObjectParent(nctsHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		}
		NctsHeader nctsHeader;
	}
}
