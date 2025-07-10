using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(ClickableNctsElectronicFolderStatusRequestContext))]
sealed class ClickableNctsElectronicFolderStatusRequestContextTest : ClickableRequestContextAbstractTest
{
	public void TestVisible()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		AssertEquals("When NctsHeader is not departure", expected: false, clickableContext.Visible);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		AssertEquals("When NctsHeader is departure non phase5", expected: false, clickableContext.Visible);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertEquals("When NctsHeader is departure and phase5, Visible", expected: true, clickableContext.Visible);
	}

	public void TestEnabled()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
		AssertEquals("When MRN is empty", expected: false, clickableContext.Enabled);

		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";
		AssertEquals("When MRN has value", expected: true, clickableContext.Enabled);
	}

	public void TestExecute_CreateEfStatusRequestMessage()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";
		SetupCryptokiCertificate();

		var clickableContextWithPin = new ClickableNctsElectronicFolderStatusRequestContextForTest(nctsHeader, "PQE123");
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		clickableContextWithPin.Execute(GetClickableItem(clickableContextWithPin));
		AssertRequestCreation(nctsHeader, EDIMessageTypeList.Codes.ElectronicFolderQuery, "EF Status Request has been sent to customs.");
	}

	public void TestExecute_CreateEfStatusRequestMessage_FormPreSaved()
	{
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
		clickableContext.Execute(GetClickableItem());
		AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestExecute_CreateEfStatusRequestMessage_AskForPinIfNotFoundInMemory()
	{
		SetupCryptokiCertificate();
		clickableContext.Execute(GetClickableItem());
		AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
	}

	public void TestExecute_CreateEfStatusRequestMessage_DoesNotAskPinWhenUserHasAutomaticSignature()
	{
		SetupCryptokiCertificate();
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		clickableContext.Execute(GetClickableItem());
		AssertRequestCreation(nctsHeader, EDIMessageTypeList.Codes.ElectronicFolderQuery, "EF Status Request has been sent to customs.");
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		clickableContext = new ClickableNctsElectronicFolderStatusRequestContextForTest(nctsHeader);
	}

	NctsHeader nctsHeader;
	IClickableContext clickableContext;

	protected override IClickableContext GetClickableContext() => clickableContext;
	protected override string GetExpectedName() => "EfStatusRequest";
	protected override string GetExpectedCaption() => "EF Status Request";
}

sealed class ClickableNctsElectronicFolderStatusRequestContextForTest : ClickableNctsElectronicFolderStatusRequestContext
{
	public ClickableNctsElectronicFolderStatusRequestContextForTest(NctsHeader header, string certificatePin = null) : base(header)
	{
		SetXadesCertificatePinHandler(new XadesCertificatePinHandlerForTest(null, "0123456789", new UserEnterableTokenPin { Pin = certificatePin }));
	}

	protected override IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> CreateEFStatusRequestContext(NctsHeader nctsHeader)
	{
		var provider = new GlbCertificateProvider();
		var mauCertificate = provider.GetMauCertificatePassword("1234");
		var cryptokeiCertificate = provider.GetCryptokiCertificate();

		var signedBytes = Encoding.UTF8.GetBytes("<soap></soap>");
		var xmlSignerMock = new Mock<IAidaXmlSigner>();
		xmlSignerMock
			.Setup(x => x.Sign(It.IsAny<byte[]>(), It.IsAny<ICryptokiGlbExternalPassword>(), It.IsAny<DateTime>()))
			.Returns(signedBytes);

		var contextMock = new Mock<IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader>>();
		contextMock.Setup(ctx => ctx.MessageParent).Returns(nctsHeader.MovementHeader);
		contextMock.Setup(ctx => ctx.BusinessObject).Returns(nctsHeader);
		contextMock.Setup(ctx => ctx.CryptokiCertificate).Returns(cryptokeiCertificate);
		contextMock.Setup(ctx => ctx.MauCertificate).Returns(mauCertificate);
		contextMock.Setup(ctx => ctx.ServiceId).Returns("SERVICE_ID_EFQ");
		contextMock.Setup(ctx => ctx.XmlSigner).Returns(xmlSignerMock.Object);

		return contextMock.Object;
	}
}
