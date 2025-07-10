using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Moq;

namespace Enterprise.Customs.NL.Business.Testing;

static class DMSResponseMessageTestHelper
{
	public static Mock<IDMSIncomingDataProvider> MockValidAndEmptyDMSIncomingDataProvider()
	{
		var dataProviderMock = new Mock<IDMSIncomingDataProvider>();
		var additionalInformation = MockResponseAdditionalInformation().Object;
		dataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation });
		var control = MockResponseControl().Object;
		dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control });
		var controlResult = MockResponseControlResult().Object;
		dataProviderMock.Setup(x => x.ControlResults).Returns(new IDMSControlResult[] { controlResult });
		var status = MockResponseStatus().Object;
		dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status });
		var error = MockResponseError().Object;
		dataProviderMock.Setup(x => x.Errors).Returns(new IDMSError[] { error });
		return dataProviderMock;
	}

	public static Mock<IDMSDeclaration> MockResponseDeclaration(DateTime? issueDateTime = null)
	{
		var mock = new Mock<IDMSDeclaration>();
		mock.Setup(h => h.IssueDateTime).Returns(issueDateTime);
		return mock;
	}

	public static Mock<IDMSAdditionalInformation> MockResponseAdditionalInformation(string statementTypeCode = null, string statementDescription = null, DateTime? limitDate = null)
	{
		var mock = new Mock<IDMSAdditionalInformation>();
		mock.Setup(h => h.StatementTypeCode).Returns(statementTypeCode);
		mock.Setup(h => h.StatementDescription).Returns(statementDescription);
		mock.Setup(m => m.LimitDate).Returns(limitDate);
		return mock;
	}

	public static Mock<IDMSControl> MockResponseControl(DateTime? limitDate = null, string typeCode = null, DateTime? inspectionStartDate = null, string additionalInfoStatementDescription = null, string controlResultDescription = null, string controlResultID = null, DateTime? controlResultEffectiveTime = null, DateTime? controlResultExitTime = null)
	{
		var mock = new Mock<IDMSControl>();
		mock.Setup(h => h.LimitDate).Returns(limitDate);
		mock.Setup(h => h.TypeCode).Returns(typeCode);
		mock.Setup(h => h.InspectionStartDate).Returns(inspectionStartDate);
		mock.Setup(m => m.AdditionalInfoStatementDescription).Returns(additionalInfoStatementDescription);
		mock.Setup(m => m.ControlResultDescription).Returns(controlResultDescription);
		mock.Setup(m => m.ControlResultID).Returns(controlResultID);
		mock.Setup(m => m.ControlResultEffectiveTime).Returns(controlResultEffectiveTime);
		mock.Setup(m => m.ControlResultExitTime).Returns(controlResultExitTime);
		return mock;
	}

	public static Mock<IDMSControlResult> MockResponseControlResult(int? goodsItemNumeric = null, IReadOnlyCollection<IDMSControl> controls = null)
	{
		var mock = new Mock<IDMSControlResult>();
		mock.Setup(h => h.GoodsItemNumeric).Returns(goodsItemNumeric);
		mock.Setup(h => h.Controls).Returns(controls ?? new IDMSControl[] { MockResponseControlResultControl().Object });
		return mock;
	}

	public static Mock<IDMSControl> MockResponseControlResultControl(DateTime? inspectionStartDate = null, string typeCode = null,
		string additionalInfoStatementDescription = null, IReadOnlyCollection<IDMSControlDetail> controlDetails = null)
	{
		var mock = new Mock<IDMSControl>();
		mock.Setup(h => h.InspectionStartDate).Returns(inspectionStartDate);
		mock.Setup(h => h.TypeCode).Returns(typeCode);
		mock.Setup(m => m.AdditionalInfoStatementDescription).Returns(additionalInfoStatementDescription);
		mock.Setup(h => h.ControlDetails).Returns(controlDetails ?? new IDMSControlDetail[] { MockResponseControlResultControlControlDetail().Object });
		return mock;
	}

	public static Mock<IDMSControlDetail> MockResponseControlResultControlControlDetail(string correctedAttributeValue = null, string pointerLocation = null, string additionalInfoStatementDescription = null)
	{
		var mock = new Mock<IDMSControlDetail>();
		mock.Setup(h => h.CorrectedAttributeValue).Returns(correctedAttributeValue);
		mock.Setup(h => h.PointerLocation).Returns(pointerLocation);
		mock.Setup(m => m.AdditionalInfoStatementDescription).Returns(additionalInfoStatementDescription);
		return mock;
	}

	public static Mock<IDMSStatus> MockResponseStatus(DateTime? effectiveDateTime = null, string nameCode = null, DateTime? releaseDate = null)
	{
		var mock = new Mock<IDMSStatus>();
		mock.Setup(h => h.EffectiveDateTime).Returns(effectiveDateTime);
		mock.Setup(h => h.NameCode).Returns(nameCode);
		mock.Setup(h => h.ReleaseDate).Returns(releaseDate);
		return mock;
	}

	public static Mock<IDMSError> MockResponseError(string validationCode = null, string description = null, string originalAttributeValue = null, IReadOnlyCollection<string> pointerLocations = null)
	{
		var mock = new Mock<IDMSError>();
		mock.Setup(h => h.ValidationCode).Returns(validationCode);
		mock.Setup(h => h.Description).Returns(description);
		mock.Setup(h => h.OriginalAttributeValue).Returns(originalAttributeValue);
		mock.Setup(h => h.PointerLocations).Returns(pointerLocations ?? new string[] { string.Empty });
		return mock;
	}

	public static Mock<IDMSRequestedDocument> MockResponseRequestedDocument(string typeCode = null, string description = null)
	{
		var mock = new Mock<IDMSRequestedDocument>();
		mock.Setup(h => h.TypeCode).Returns(typeCode);
		mock.Setup(h => h.Description).Returns(description);
		return mock;
	}

	public static Mock<IControlIncomingDataProvider> MockValidAndEmptyControlIncomingDataProvider()
	{
		var dataProviderMock = new Mock<IControlIncomingDataProvider>();
		var documentMetaData = MockDocumentMetaData().Object;
		dataProviderMock.Setup(x => x.DocumentMetaData).Returns(documentMetaData);
		var response = MockResponse().Object;
		dataProviderMock.Setup(x => x.Response).Returns(response);
		return dataProviderMock;
	}

	public static Mock<IControlDocumentMetaData> MockDocumentMetaData(string wcoDataModelVersionCode = null, string responsibleCountryCode = null, string responsibleAgencyName = null, string agencyAssignedCustomizationVersionCode = null, string applicationReferenceId = null, DateTime? preparationDateTime = null)
	{
		var mock = new Mock<IControlDocumentMetaData>();
		mock.Setup(x => x.WcoDataModelVersionCode).Returns(wcoDataModelVersionCode);
		mock.Setup(x => x.ResponsibleCountryCode).Returns(responsibleCountryCode);
		mock.Setup(x => x.ResponsibleAgencyName).Returns(responsibleAgencyName);
		mock.Setup(x => x.AgencyAssignedCustomizationVersionCode).Returns(agencyAssignedCustomizationVersionCode);
		var communicationMetaData = MockCommunicationMetaData(applicationReferenceId, preparationDateTime).Object;
		mock.Setup(x => x.CommunicationMetaData).Returns(communicationMetaData);
		return mock;
	}

	public static Mock<IControlCommunicationMetaData> MockCommunicationMetaData(string applicationReferenceId = null, DateTime? preparationDateTime = null)
	{
		var mock = new Mock<IControlCommunicationMetaData>();
		mock.Setup(x => x.ApplicationReferenceId).Returns(applicationReferenceId);
		mock.Setup(x => x.PreparationDateTime).Returns(preparationDateTime);
		return mock;
	}

	public static Mock<IControlResponse> MockResponse(string functionalReferenceId = null)
	{
		var mock = new Mock<IControlResponse>();
		mock.Setup(x => x.FunctionalReferenceId).Returns(functionalReferenceId);
		var error = MockError().Object;
		mock.Setup(x => x.Errors).Returns(new IControlError[] { error });
		return mock;
	}

	public static Mock<IControlError> MockError(string description = null, string pointerLocation = null)
	{
		var mock = new Mock<IControlError>();
		mock.Setup(x => x.Description).Returns(description);
		var pointer = MockPointer(pointerLocation).Object;
		mock.Setup(x => x.Pointers).Returns(new IControlPointer[] { pointer });
		return mock;
	}

	public static Mock<IControlPointer> MockPointer(string location = null)
	{
		var mock = new Mock<IControlPointer>();
		mock.Setup(x => x.Location).Returns(location);
		return mock;
	}
}
