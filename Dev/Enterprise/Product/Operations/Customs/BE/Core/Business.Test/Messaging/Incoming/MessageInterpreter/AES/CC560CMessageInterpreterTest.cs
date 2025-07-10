using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC560CMessageInterpreter))]
sealed class CC560CMessageInterpreterTest : MessageInterpreterTestCase<CC560CMessageInterpreter, ICC560CDataProvider>
{
	public override void TestInterpret()
	{
		var mockCC560C = new Mock<ICC560CDataProvider>();
		mockCC560C.Setup(x => x.AnticipatedControlDate).Returns(new DateTime(2023, 02, 13));
		mockCC560C.Setup(x => x.ControlNotificationDateTime).Returns(new DateTime(2023, 02, 09, 15, 02, 36));
		mockCC560C.Setup(x => x.MRN).Returns("23BE04661013170000001");
		mockCC560C.Setup(x => x.LRN).Returns("23021255220001");
		mockCC560C.Setup(x => x.NotificationType).Returns(NotificationTypesList.Codes.DecisionToControl);
		mockCC560C.Setup(x => x.Text).Returns("Control includes checking of the necessary documents");
		var mockCC560CTypeOfControl = new Mock<ITypeOfControlsDataProvider>();
		mockCC560CTypeOfControl.Setup(x => x.Type).Returns(TypeOfControlsList.Codes.DocumentControl);
		mockCC560CTypeOfControl.Setup(x => x.Text).Returns("Make sure you have all licenses/certificates and transport documents at hand");
		var mockCC560CRequestedDocument = new Mock<IRequestedDocumentDataProvider>();
		mockCC560CRequestedDocument.Setup(x => x.DocumentType).Returns("N380");
		mockCC560CRequestedDocument.Setup(x => x.Description).Returns("Value on invoices will be checked");
		mockCC560C.Setup(x => x.TypeOfControls).Returns(new List<ITypeOfControlsDataProvider> { mockCC560CTypeOfControl.Object });
		mockCC560C.Setup(x => x.RequestedDocuments).Returns(new List<IRequestedDocumentDataProvider> { mockCC560CRequestedDocument.Object });

		var decl = Factory.New<JobDeclaration>();
		var entry = decl.CustomsEntryHeaders.AddNew();
		entry.CH_EntryStatus = StatusCodes.DecisionToControl;
		var message = entry.Messages.AddNew();

		var result = Interpreter.Interpret(mockCC560C.Object, message);
		AssertEquals("Status is set to DCC<br />Customs decision to control (0 - Decision to control (and requested documents if needed)) was taken on 09/02/2023 15:02:36<br />The anticipated date of control will be on 13/02/2023<br />Control includes checking of the necessary documents<br /><br />Type of control: 10 - Document Control<br />Additional info: Make sure you have all licenses/certificates and transport documents at hand<br /><br />Document: N380<br />Description: Value on invoices will be checked<br /><br />", result);
	}
}
