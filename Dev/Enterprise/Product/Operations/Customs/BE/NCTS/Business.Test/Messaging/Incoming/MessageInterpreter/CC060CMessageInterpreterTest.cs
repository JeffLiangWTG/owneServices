using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC060CMessageInterpreter))]
	sealed class CC060CMessageInterpreterTest : MessageInterpreterTestCase<CC060CMessageInterpreter, ICC060CDataProvider>
	{
		public override void TestInterpret()
		{
			var listTypeOfControls = new List<TypeOfControlsXmlProvider>
			{
				TypeOfControlsXmlProvider.New(new TypeOfControlsType
				{
					SequenceNumber = "1", Type = NCTS5TypeOfControlTypes.Codes.DocumentaryControls, Text = "Text TOC1"
				}),
				TypeOfControlsXmlProvider.New(new TypeOfControlsType
				{
					SequenceNumber = "2", Type = NCTS5TypeOfControlTypes.Codes.IdentificationOfConsignmentAndSeals, Text = "Text TOC2"
				})
			};
			var listRequestedDocument = new List<RequestedDocumentXmlProvider>
			{
				RequestedDocumentXmlProvider.New(new RequestedDocumentType
				{
					SequenceNumber = "1", DocumentType = "DT1", Description = "Doc Type 1"
				}),
				RequestedDocumentXmlProvider.New(new RequestedDocumentType
				{
					SequenceNumber = "2", DocumentType = "DT2", Description = "Doc Type 2"
				})
			};
			var controlDateAndTime = new ZDateTime(2022, 4, 1, 10, 34, 56, DateTimeKind.Utc);
			var mockCC060C = new Mock<ICC060CDataProvider>();
			mockCC060C.Setup(m => m.ControlNotificationDateAndTimeUtc).Returns(controlDateAndTime.ToDateTime());
			mockCC060C.Setup(m => m.NotificationType).Returns(NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest);
			mockCC060C.Setup(m => m.TypeOfControls).Returns(listTypeOfControls);
			mockCC060C.Setup(m => m.RequestedDocument).Returns(listRequestedDocument);
			var result = Interpreter.Interpret(mockCC060C.Object, null);
			AssertContains($"New Customs Status: Decision to Control Notification</br>Status granted on {controlDateAndTime.ToLocalBranchTimeOffset():dd-MM-yyyy hh:mm:ss \"UTC\"z}</br>Type of Notification: 1 Additional documents request</br></br>Type of Control 1: 10 Documentary controls Text TOC1</br>Type of Control 2: 41 Identification of consignment and seals Text TOC2</br>Document 1: DT1 Doc Type 1</br>Document 2: DT2 Doc Type 2", result);
			mockCC060C.VerifyAll();
		}
	}
}
