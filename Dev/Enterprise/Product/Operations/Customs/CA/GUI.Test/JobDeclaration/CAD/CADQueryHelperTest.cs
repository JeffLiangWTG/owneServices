using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CADQueryHelperTest : TestCaseWithFactory
	{
		public void TestManagerValidate()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://Test.com"))
			using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
			using (var form = new ZForm(Declaration))
			{
				var queryHelper = new CADQueryHelperForTest(Declaration.B3EntryHeader, form);
				queryHelper.ManagerValidateForTest();
				AssertProgressForm(string.Empty, int.MinValue, false);
			}
		}

		public void TestManagerQuery()
		{
			Declaration.B3EntryHeader.CH_EntrySubmittedDate = DateTime.Now;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://Test.com"))
			using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
			using (var form = new ZForm(Declaration))
			{
				var queryHelper = new CADQueryHelperForTest(Declaration.B3EntryHeader, form, HttpStatusCode.BadGateway);
				queryHelper.ManagerQueryForTest();
				AssertProgressForm("CAD Query Failed:Test Response Content", 100);

				var responseContent = $"<DocumentMetaData xmlns=\"urn:wco:datamodel:WCO:Declaration:1\">\r\n    <ResponsibleAgencyName>CBSA</ResponsibleAgencyName>\r\n    <AgencyAssignedCustomizationCode>CAD</AgencyAssignedCustomizationCode>\r\n    <AgencyAssignedCustomizationVersionCode>001</AgencyAssignedCustomizationVersionCode>\r\n    <FunctionalDefinition>CAD-OUT</FunctionalDefinition>\r\n    <CommunicationMetaData/>\r\n    <Response>\r\n        <IssueDateTime>\r\n            <DateTimeString>20240102005453</DateTimeString>\r\n        </IssueDateTime>\r\n        <Error>\r\n            <Description>No Data Found or Access Denied</Description>\r\n            <ValidationCode>077</ValidationCode>\r\n            <Pointer>\r\n                <Location>DocumentMetaData/Declaration</Location>\r\n            </Pointer>\r\n        </Error>\r\n        <Status>\r\n            <NameCode>41</NameCode>\r\n        </Status>\r\n    </Response>\r\n</DocumentMetaData>";
				queryHelper = new CADQueryHelperForTest(Declaration.B3EntryHeader, form, HttpStatusCode.BadRequest, responseContent);
				queryHelper.ManagerQueryForTest();
				AssertProgressForm("CAD Query Complete", 100);
			}
		}

		public void TestManagerProcess()
		{
			Declaration.B3EntryHeader.CH_EntrySubmittedDate = DateTime.Now;
			Declaration.Logger = new LoggingInformation();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://Test.com"))
			using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
			using (var form = new ZForm(Declaration))
			{
				var queryHelper = new CADQueryHelperForTest(Declaration.B3EntryHeader, form, responseContent: "<DocumentMetaData xmlns=\"urn:wco:datamodel:WCO:Declaration:1\"></DocumentMetaData>");
				AssertNoExceptionThrown(() =>
				{
					queryHelper.SendCADQuery_Click();
				});
			}
		}

		void AssertProgressForm(string status, int percentComplete, bool isProgressFormExist = true)
		{
			if (isProgressFormExist)
			{
				using (var progressForm = (ProgressForm)ZApplication.GetOpenForms().FirstOrDefault(x => x.GetType() == typeof(ProgressForm)))
				{
					AssertNotNull(progressForm);
					AssertEquals(status, progressForm.Status);
					AssertEquals(percentComplete, progressForm.PercentComplete);
				}
			}
			else
			{
				AssertEquals(0, ZApplication.GetOpenForms().Length);
			}
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Enterprise.Customs.CA.Business.JobMessageTypeList.Codes.Import;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = declaration.InvoiceLines.AddNew();
					invoiceLine.JI_CustomsQuantity = 100m;
					invoiceLine.JI_CustomsUnitQty = "KGM";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.DoMerge();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}

	class CADQueryHelperForTest : CADQueryHelper
	{
		public CADQueryHelperForTest(Business.CusEntryHeader cadEntry, ZForm parentForm, HttpStatusCode httpStatusCode = HttpStatusCode.OK, string responseContent = "Test Response Content") : base(cadEntry, parentForm, new HttpClient(GetMockHttpMsgHandler(httpStatusCode, responseContent)))
		{
		}

		public void ManagerValidateForTest()
		{
			manager.Validate();
		}

		public void ManagerQueryForTest()
		{
			manager.Query();
		}

		internal override void ProcessFinish(int recordsToExport, string message)
		{
			progressForm.SetStatusAndPercentComplete(message, recordsToExport);
		}

		static HttpMessageHandler GetMockHttpMsgHandler(HttpStatusCode responseStatus, string responseContent)
		{
			var mockHttpMsgHandler = new Mock<HttpMessageHandler>();
			mockHttpMsgHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage { StatusCode = responseStatus, Content = new StringContent(responseContent) });

			return mockHttpMsgHandler.Object;
		}
	}
}
