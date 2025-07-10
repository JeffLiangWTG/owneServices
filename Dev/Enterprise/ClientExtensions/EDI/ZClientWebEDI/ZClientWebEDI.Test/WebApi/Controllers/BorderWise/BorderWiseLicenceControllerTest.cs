using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Results;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise;
using static Enterprise.ZClientWebCargoWiseEDI.BorderWise.BorderWiseLicenceControllerBase;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise.Testing
{
	public class BorderWiseLicenceControllerTest : BorderWiseLicenceControllerBaseTestCase<BorderWiseLicenceController>
	{
		#region GetContactFromRequest
		public void TestGetContactFromRequest()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_IsActive = true;
			const string password1 = "abc123";
			const string password2 = "xyz789";
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "U1";
			contact1.OC_Email = "u1@mail.box";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			BorderWiseUtilities.ChangeSecurityRight(contact1, true, Factory);
			contact1.SetHashedPassword(password1);
			var contact2 = (EDIOrgContact)org.Contacts.AddNew();
			contact2.OC_ContactName = "U2";
			contact2.OC_Email = "u2@mail.box";
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			BorderWiseUtilities.ChangeSecurityRight(contact2, true, Factory);
			contact2.SetHashedPassword(password2);
			Factory.Save();
			AssertEquals(contact1.PK, BorderWiseLicenceController.GetContactFromRequest("", "u1@mail.box", password1, out _).PK);
			AssertNull(BorderWiseLicenceController.GetContactFromRequest("", "u2@mail.box", password1, out _));
			AssertNull(BorderWiseLicenceController.GetContactFromRequest("", "u1@mail.box", password2, out _));
			AssertEquals(contact2.PK, BorderWiseLicenceController.GetContactFromRequest("", "u2@mail.box", password2, out _).PK);
			AssertNull(BorderWiseLicenceController.GetContactFromRequest("", "u2@mail.box", "password", out _));
			contact2.OC_PasswordHash = ZBlob.Empty;
			Factory.Save();
			AssertNull(BorderWiseLicenceController.GetContactFromRequest("", "u2@mail.box", password2, out _));
		}

		#endregion
		#region Book Review and Rejection

		public void TestRejectBook_ShouldIgnoreUserPassword_WhenItExistsForContact()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(c => c.SetHashedPassword("anything"), shouldBookRejectionFail: false, failureReason: null);
		}

		public void TestRejectBook_ForUserWithoutWebAccess_ShouldFail()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(c => c.OC_WebAccessEnabled = false, shouldBookRejectionFail: true, failureReason: "The user record could not be found.");
		}

		public void TestRejectBook_ForNonWiseTechUser_ShouldFail()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(c => c.OC_Email = "dave@daveeast.com", shouldBookRejectionFail: true, failureReason: "The user is not permitted to perform this action.");
		}

		public void TestRejectBook_ForInactiveUser_ShouldFail()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(c => c.OC_IsActive = false, shouldBookRejectionFail: true, failureReason: "The user record could not be found.");
		}

		public void TestRejectBook_ForInactiveOrganisation_ShouldFail()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(c => c.ParentOrg.OH_IsActive = false, shouldBookRejectionFail: true, failureReason: "The user record could not be found.");
		}

		public void TestRejectBook_WithNonFatalError_ShouldCreateIncident_AndReturnIncidentDetails()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(revokeAccessFunc: null, shouldBookRejectionFail: false, failureReason: null, nonFatalError: true);
		}

		public void TestRejectBook_ShouldCreateIncident_AndReturnIncidentDetails()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(revokeAccessFunc: null, shouldBookRejectionFail: false, failureReason: null);
		}

		public void TestRejectBook_ForInvalidBorderWiseWorkItem_ShouldFail()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(revokeAccessFunc: null, shouldBookRejectionFail: false, failureReason: null, workItemType: "ENT");
		}

		public void TestRejectBook_WithDetailsTextNotIncludingCarriageReturn_ShouldAddCarriageReturns()
		{
			var submittedRejectionDetails = "Gimme an A!\nA.\nGimme a B!\nB.\nGimme a C!\nC.\r\nWhat's that spell?\r\nABC!";
			var expectedIncidentDetails = "Gimme an A!\r\nA.\r\nGimme a B!\r\nB.\r\nGimme a C!\r\nC.\r\nWhat's that spell?\r\nABC!";
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(revokeAccessFunc: null, shouldBookRejectionFail: false, failureReason: null, rejectionDetails: submittedRejectionDetails, expectedIncidentDetails: expectedIncidentDetails);
		}

		public void TestRejectBook_WithNullDetails_ShouldNotThrowException()
		{
			RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(revokeAccessFunc: null, shouldBookRejectionFail: false, failureReason: null, rejectionDetails: null, expectedIncidentDetails: string.Empty);
		}

		public void TestRejectBook_WithInvalidApiKey_ShouldReturnForbidden()
		{
			var controller = new BorderWiseLicenceController();
			controller.Request = new HttpRequestMessage();
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.RejectBook(Guid.NewGuid(), "user@wisetechglobal.com", "app--alpha.borderwise.com", CreateValidRequestDetails()));
			controller.Request.Headers.Add("X-API-KEY", "wrong-key");
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.RejectBook(Guid.NewGuid(), "user@wisetechglobal.com", "app--alpha.borderwise.com", CreateValidRequestDetails()));
			BookRejectionRequestDetails CreateValidRequestDetails()
			{
				return new BookRejectionRequestDetails { WorkItemNumber = "WI00000000" };
			}
		}

		void RejectBook_OptionallyWithRevokedAccess_AssertingRejectionResult(Action<OrgContact> revokeAccessFunc, bool shouldBookRejectionFail, string failureReason, bool nonFatalError = false, string workItemType = "BOR", string workItemPriorityField = "REJ", string rejectionDetails = "", string expectedIncidentDetails = "")
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			org.OH_Code = "MAIFRISTORG";
			contact.OC_Email = "dave@wisetechglobal.com";
			contact.OC_WebAccessEnabled = true;
			revokeAccessFunc?.Invoke(contact);
			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI00000000";
			workItem.WKI_WorkItemType = workItemType;
			Factory.Save();
			if (string.IsNullOrEmpty(rejectionDetails))
			{
				rejectionDetails = @"1. Styling has not been passed correctly - we investigated and found that the folder name of the style has changed and needs to be fixed.
2.Investigation shows that the Watermarking characters are affecting the indentations.";
			}

			if (string.IsNullOrEmpty(expectedIncidentDetails))
			{
				expectedIncidentDetails = rejectionDetails;
			}

			using (var controller = new BorderWiseLicenceController())
			{
				controller.Request = new HttpRequestMessage();
				controller.SetApiKey();
				var requestDetails = new BookRejectionRequestDetails { BookName = "Customs and Excise Act 2018", BookId = "5e212bcf95fabf0020d35201", ContentId = "de_mah_content", RejectionReason = "Incorrect formatting", Details = rejectionDetails, CountryCode = "DE", WorkItemNumber = "WI00000000", Type = "book", ReviewedItemName = "Of all the books, by far the greatest", };
				if (nonFatalError)
				{
					var nonFatalResult = controller.RejectBook(contact.PK.ToGuid(), "dave@wisetechglobal.com", "app--alpha.borderwise.com", requestDetails, nonFatalError);
					var nonFatalIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_Description, SQLComparisonOperator.StartsWith, "Non-fatal error with book:"));
					if (shouldBookRejectionFail)
					{
						AssertType<NegotiatedContentResult<ForbiddenResponse>>(nonFatalResult);
						var notOkResponse = (NegotiatedContentResult<ForbiddenResponse>)nonFatalResult;
						AssertEquals(failureReason, notOkResponse.Content.Message);
						AssertNull(nonFatalIncident);
					}
					else
					{
						AssertType<OkNegotiatedContentResult<BookRejectionResultDetails>>(nonFatalResult);
						var okResponse = (OkNegotiatedContentResult<BookRejectionResultDetails>)nonFatalResult;
						AssertNotNull(nonFatalIncident);
						AssertNotNull(okResponse);
						const string expectedIncidentSummary = "Non-fatal error with book: Customs and Excise Act 2018";
						var expectedDetails = (@"Book name: Customs and Excise Act 2018
Book ID: 5e212bcf95fabf0020d35201
Content ID: de_mah_content
Non-fatal error raised by: dave@wisetechglobal.com
Reason for non-fatal error: Incorrect formatting
System URL: app--alpha.borderwise.com
Details: " + expectedIncidentDetails).TrimEnd();
						CombineAssertions("Incident details", () =>
						{
							AssertEquals("IM_Product", "BOR", nonFatalIncident.IM_Product);
							AssertEquals("ProductArea", "BOR", nonFatalIncident.ProductArea);
							AssertEquals("IM_Priority", "CR4", nonFatalIncident.IM_Priority);
							AssertEquals("IM_Module", "CDP", nonFatalIncident.IM_Module);
							AssertEquals("IM_Description", expectedIncidentSummary, nonFatalIncident.IM_Description);
							AssertEquals("DetailNoteText", expectedDetails, nonFatalIncident.DetailNoteText);
							AssertEquals("IM_OC_Contact", contact.PK, nonFatalIncident.IM_OC_Contact);
							AssertEquals("IM_OH_Client", org.PK, nonFatalIncident.IM_OH_Client);
							AssertNotEquals("IM_OA_BranchAddress", ZGuid.Empty, nonFatalIncident.IM_OA_BranchAddress);
						});
						CombineAssertions("Response values", () =>
						{
							AssertEquals("IncidentNumber", nonFatalIncident.IM_IncidentNumber, okResponse.Content.IncidentNumber);
							AssertStartsWith("Should include incident hyperlink so Content Service can link back to it", $"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK={nonFatalIncident.PK}&VersionNumber={new EnterpriseInformationRetriever().VersionNumber}&Hash=", okResponse.Content.IncidentHyperlink);
						});
					}
				}
				else
				{
					var result = controller.RejectBook(contact.PK.ToGuid(), "dave@wisetechglobal.com", "app--alpha.borderwise.com", requestDetails);
					var incident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_Description, SQLComparisonOperator.StartsWith, "Rejected book:"));
					if (shouldBookRejectionFail)
					{
						AssertType<NegotiatedContentResult<ForbiddenResponse>>(result);
						var notOkResponse = (NegotiatedContentResult<ForbiddenResponse>)result;
						AssertEquals(failureReason, notOkResponse.Content.Message);
						AssertNull(incident);
					}
					else
					{
						AssertType<OkNegotiatedContentResult<BookRejectionResultDetails>>(result);
						var okResponse = (OkNegotiatedContentResult<BookRejectionResultDetails>)result;
						AssertNotNull(incident);
						AssertNotNull(okResponse);
						var log = workItem.Logs.Find(x => x.SL_SE_NKEvent == Events.RejectBookCode).Single();
						AssertEquals("|NAM=Of all the books, by far the greatest|TYP=book", log.SL_Reference);
						const string expectedIncidentSummary = "Rejected book: Customs and Excise Act 2018";
						var expectedDetails = (@"Book name: Customs and Excise Act 2018
Book ID: 5e212bcf95fabf0020d35201
Content ID: de_mah_content
Rejected by: dave@wisetechglobal.com
Reason for rejection: Incorrect formatting
System URL: app--alpha.borderwise.com
Details: " + expectedIncidentDetails).TrimEnd();
						CombineAssertions("Incident details", () =>
						{
							AssertEquals("IM_Product", "BOR", incident.IM_Product);
							AssertEquals("ProductArea", "BOR", incident.ProductArea);
							AssertEquals("IM_Priority", "CR4", incident.IM_Priority);
							AssertEquals("IM_Module", "CDP", incident.IM_Module);
							AssertEquals("IM_Description", expectedIncidentSummary, incident.IM_Description);
							AssertEquals("DetailNoteText", expectedDetails, incident.DetailNoteText);
							AssertEquals("IM_OC_Contact", contact.PK, incident.IM_OC_Contact);
							AssertEquals("IM_OH_Client", org.PK, incident.IM_OH_Client);
							AssertNotEquals("IM_OA_BranchAddress", ZGuid.Empty, incident.IM_OA_BranchAddress);
						});
						CombineAssertions("Response values", () =>
						{
							AssertEquals("IncidentNumber", incident.IM_IncidentNumber, okResponse.Content.IncidentNumber);
							AssertStartsWith("Should include incident hyperlink so Content Service can link back to it", $"edient:Command=ShowEditForm&ControllerID=SupportIncident&BusinessEntityPK={incident.PK}&VersionNumber={new EnterpriseInformationRetriever().VersionNumber}&Hash=", okResponse.Content.IncidentHyperlink);
						});
					}
				}
			}
		}

		public void TestApproveBook_WithInvalidApiKey_ShouldReturnForbidden()
		{
			var controller = new BorderWiseLicenceController();
			controller.Request = new HttpRequestMessage();
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.ApproveBook(Guid.NewGuid(), CreateValidRequestDetails()));
			controller.Request.Headers.Add("X-API-KEY", "wrong-key");
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.ApproveBook(Guid.NewGuid(), CreateValidRequestDetails()));
			BookApprovalRequestDetails CreateValidRequestDetails()
			{
				return new BookApprovalRequestDetails { WorkItemNumber = "WI00000000" };
			}
		}

		public void TestApproveBook_AssertingRaiseApproveEvent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			org.OH_Code = "MAIFRISTORG";
			contact.OC_Email = "dave@wisetechglobal.com";
			contact.SetHashedPassword("133tPassw0rd69");
			contact.OC_WebAccessEnabled = true;
			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI00000000";
			workItem.WKI_WorkItemType = "BOR";
			Factory.Save();
			using (var controller = new BorderWiseLicenceController())
			{
				controller.Request = new HttpRequestMessage();
				controller.SetApiKey();
				var requestDetails = new BookApprovalRequestDetails { WorkItemNumber = "WI00000000", Type = "book", ReviewedItemName = "Of all the books, by far the greatest", };
				var result = controller.ApproveBook(contact.PK.ToGuid(), requestDetails);
				var log = workItem.Logs.Find(x => x.SL_SE_NKEvent == Events.ApproveBookCode).Single();
				AssertEquals("|NAM=Of all the books, by far the greatest|TYP=book", log.SL_Reference);
			}
		}

		public void TestBookPromotion_WithInvalidApiKey_ShouldReturnForbidden()
		{
			var controller = new BorderWiseLicenceController();
			controller.Request = new HttpRequestMessage();
			var bookPromotionRequestDetails = new BookPromotionRequestDetails { WorkItemNumber = "WI00000000", PromotedItemVersion = "book", PromotedItemName = "Tariff Book", Completed = true };
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.BookPromotion(bookPromotionRequestDetails));
			controller.Request.Headers.Add("X-API-KEY", "wrong-key");
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.BookPromotion(bookPromotionRequestDetails));
		}

		public void TestBookPromotion_AssertingRaisePromotionCompletedEvent()
		{
			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI00000000";
			workItem.WKI_WorkItemType = "BOR";
			Factory.Save();
			using (var controller = new BorderWiseLicenceController())
			{
				controller.Request = new HttpRequestMessage();
				controller.SetApiKey();
				var requestDetails = new BookPromotionRequestDetails { WorkItemNumber = "WI00000000", PromotedItemVersion = "book", PromotedItemName = "Tariff Book", Completed = true };
				var result = controller.BookPromotion(requestDetails);
				var log = workItem.Logs.Find(x => x.SL_SE_NKEvent == Events.PromotionCompletedCode).Single();
				AssertEquals("|NAM=Tariff Book|TYP=book", log.SL_Reference);
			}
		}

		public void TestBookPromotion_AssertingRaisePromotionFailedEvent()
		{
			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI00000000";
			workItem.WKI_WorkItemType = "BOR";
			Factory.Save();
			using (var controller = new BorderWiseLicenceController())
			{
				controller.Request = new HttpRequestMessage();
				controller.SetApiKey();
				var requestDetails = new BookPromotionRequestDetails { WorkItemNumber = "WI00000000", PromotedItemVersion = "book", PromotedItemName = "Tariff Book", Completed = false };
				var result = controller.BookPromotion(requestDetails);
				var log = workItem.Logs.Find(x => x.SL_SE_NKEvent == Events.PromotionFailedCode).Single();
				AssertEquals("|NAM=Tariff Book|TYP=book", log.SL_Reference);
			}
		}

		public void TestGetWorkItemSummary_Success()
		{
			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI00000000";
			workItem.WKI_WorkItemType = "BOR";
			workItem.WKI_Summary = "Something";
			Factory.Save();
			var controller = new BorderWiseLicenceController();
			var result = controller.GetWorkItemSummary(workItem.WKI_WorkItemNumber);
			AssertType<OkNegotiatedContentResult<ZString>>(result);
			var resultDetails = ((OkNegotiatedContentResult<ZString>)result).Content;
			AssertEquals("Something", resultDetails);
		}

		public void TestGetWorkItemSummary_InvalidProductType_ShouldFail()
		{
			var workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI00000000";
			workItem.WKI_WorkItemType = "ENT";
			Factory.Save();
			var controller = new BorderWiseLicenceController();
			var result = controller.GetWorkItemSummary("WI00000000");
			AssertEquals($"Invalid work item product type. It should be 'BOR'", ((BadRequestErrorMessageResult)result).Message);
		}

		public void TestGetWorkItemSummary_NoParameter_ShouldFail()
		{
			var controller = new BorderWiseLicenceController();
			var result = controller.GetWorkItemSummary("");
			AssertEquals("An argument must be provided.", ((BadRequestErrorMessageResult)result).Message);
		}

		public void TestGetWorkItemSummary_NotFound_ShouldFail()
		{
			var controller = new BorderWiseLicenceController();
			var result = controller.GetWorkItemSummary("WI00000000");
			AssertType<NotFoundResult>(result);
		}

		public void TestIsRejectionReasonResolved()
		{
			var borderWiseIncident = Factory.NewWithValidTestData<SupportIncident>();
			borderWiseIncident.IM_Product = "BOR";
			borderWiseIncident.IM_IncidentNumber = "CS00000069";
			borderWiseIncident.IM_Status = "WRK";
			var otherIncident = Factory.NewWithValidTestData<SupportIncident>();
			otherIncident.IM_IncidentNumber = "CS00000070";
			otherIncident.IM_Status = "WRK";
			Factory.Save();
			AssertRejectionReasonResolved("CS00000069", shouldBeResolved: false);
			AssertRejectionReasonResolved("CS00000070", shouldBeResolved: false);
			borderWiseIncident.IM_Status = "CLS";
			otherIncident.IM_Status = "CLS";
			Factory.Save();
			AssertRejectionReasonResolved("CS00000069", shouldBeResolved: true);
			AssertRejectionReasonResolved("CS00000070", shouldBeResolved: false);
		}

		static void AssertRejectionReasonResolved(string incidentNumber, bool shouldBeResolved)
		{
			var controller = new BorderWiseLicenceController();
			var result = controller.IsRejectionReasonResolved(incidentNumber);
			AssertType<OkNegotiatedContentResult<bool>>(result);
			AssertEquals(shouldBeResolved, ((OkNegotiatedContentResult<bool>)result).Content);
		}

		public void TestBookVersionUpdated_WithInvalidParameters_ShouldThrowExceptions()
		{
			var controller = new BorderWiseLicenceController();
			controller.Request = new HttpRequestMessage();
			controller.SetApiKey();
			AssertExceptionThrown<ArgumentNullException>(() => controller.BookVersionUpdated(null, null));
			AssertExceptionThrown<ArgumentException>(() => controller.BookVersionUpdated("", null));
			AssertExceptionThrown<ArgumentException>(() => controller.BookVersionUpdated("", CreateValidRequestDetails()));
			AssertExceptionThrown<ArgumentNullException>(() => controller.BookVersionUpdated("Eye Dee", null));
			AssertNoExceptionThrown(() => controller.BookVersionUpdated("Eye Dee", CreateValidRequestDetails()));
			var invalidRequestDetails = CreateValidRequestDetails();
			invalidRequestDetails.BookName = null;
			AssertExceptionThrown<ArgumentNullException>(() => controller.BookVersionUpdated("Eye Dee", invalidRequestDetails));
			invalidRequestDetails.BookName = "";
			AssertExceptionThrown<ArgumentException>(() => controller.BookVersionUpdated("Eye Dee", invalidRequestDetails));
			BookVersionUpdatedRequestDetails CreateValidRequestDetails()
			{
				return new BookVersionUpdatedRequestDetails { BookName = "My Wizardin' Days are Over", BookVersion = 42, };
			}
		}

		public void TestBookVersionUpdated_WithInvalidApiKey_ShouldReturnForbidden()
		{
			var controller = new BorderWiseLicenceController();
			controller.Request = new HttpRequestMessage();
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.BookVersionUpdated("Eye Dee", CreateValidRequestDetails()));
			controller.Request.Headers.Add("X-API-KEY", "wrong-key");
			AssertForbidden("Request missing authorization 'X-API-KEY' header.", controller.BookVersionUpdated("Eye Dee", CreateValidRequestDetails()));
			BookVersionUpdatedRequestDetails CreateValidRequestDetails()
			{
				return new BookVersionUpdatedRequestDetails { BookName = "My Wizardin' Days are Over", BookVersion = 42, };
			}
		}

		public void TestBookVersionUpdated_ForManualConversion_ShouldCreateWorkItem()
		{
			ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(requiresManualConversion: true, expectedWorkItemPriority: "UPM");
		}

		public void TestBookVersionUpdated_ForContentServiceConversion_ShouldCreateWorkItem()
		{
			ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(requiresManualConversion: false, expectedWorkItemPriority: "UPC");
		}

		public void TestBookVersionUpdated_WithCustomisedRegistryValueForManualConversion_ShouldCreateWorkItem()
		{
			EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToManuallyProcess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToAutomaticallyProcess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(requiresManualConversion: true, expectedWorkItemPriority: "ABC");
		}

		public void TestBookVersionUpdated_WithCustomisedRegistryValueForContentServiceConversion_ShouldCreateWorkItem()
		{
			EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToAutomaticallyProcess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DEF");
			EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToManuallyProcess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(requiresManualConversion: false, expectedWorkItemPriority: "DEF");
		}

		public void TestBookVersionUpdated_WithEmptyRegistryValueForManualConversion_ShouldNotCreateWorkItem()
		{
			EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToManuallyProcess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(requiresManualConversion: true, shouldHaveCreatedWorkItem: false);
		}

		public void TestBookVersionUpdated_WithEmptyRegistryValueForContentServiceConversion_ShouldNotCreateWorkItem()
		{
			EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToAutomaticallyProcess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(requiresManualConversion: false, shouldHaveCreatedWorkItem: false);
		}

		public void TestBookVersionUpdated_ForVeryLongBookName_ShouldTruncateWorkItemSummary()
		{
			var bookName = "Treat it with care. Don't let it get wet. Don't feed it red meat. And for the love of the Balance, do not hold it upside down when there's a full moon!";
			var expectedSummary = "Process book update: Treat it with care. Don't let it get wet. Don't feed it red";
			ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(requiresManualConversion: false, expectedWorkItemPriority: "UPC", bookName: bookName, expectedSummary: expectedSummary);
		}

		void ExecuteBookVersionUpdated_AssertingCreatedWorkItemDetails(bool requiresManualConversion, string bookName = "Farewell to My Wizardin' Days", string expectedWorkItemPriority = null, bool shouldHaveCreatedWorkItem = true, string expectedSummary = null)
		{
			var controller = new BorderWiseLicenceController();
			controller.Request = new HttpRequestMessage();
			controller.SetApiKey();
			var result = controller.BookVersionUpdated("ID123456", new BookVersionUpdatedRequestDetails { BookName = bookName, BookVersion = 42, RequiresManualConversion = requiresManualConversion, CountryCode = "NZ", ReasonDescription = "you know why..." });
			AssertType<OkNegotiatedContentResult<BookVersionUpdatedResultDetails>>(result);
			var resultDetails = ((OkNegotiatedContentResult<BookVersionUpdatedResultDetails>)result).Content;
			if (shouldHaveCreatedWorkItem)
			{
				var workItem = Factory.LoadTop1<WorkItem>(new ZQuery(WorkItemSchema.WKI_WorkItemNumber, resultDetails.WorkItemNumber));
				AssertNotNull(workItem);
				AssertEquals("BOR", workItem.WKI_WorkItemType);
				AssertEquals("CNT", workItem.WKI_WorkItemArea);
				AssertEquals("CNT", workItem.WKI_ActivitySubtype);
				AssertEquals(expectedWorkItemPriority, workItem.WKI_Priority);
				if (expectedSummary == null)
				{
					expectedSummary = $"{(requiresManualConversion ? "Manually process" : "Process")} book update: {bookName}";
				}

				AssertEquals(expectedSummary, workItem.WKI_Summary);
				AssertEquals($@"Book Name: {bookName}
Book ID: ID123456
Version: 42
Reason: you know why...", workItem.WKI_Details.ToUTF8());
				AssertStartsWith("Should include WI hyperlink so Content Service can link back to it", $"edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK={workItem.PK}&VersionNumber={new EnterpriseInformationRetriever().VersionNumber}&Hash=", resultDetails.WorkItemHyperlink);
				AssertEquals("NZ", workItem.WKI_ActivityType.ToString());
			}
			else
			{
				AssertNullOrEmpty(resultDetails.WorkItemNumber);
				AssertNullOrEmpty(resultDetails.WorkItemHyperlink);
			}
		}

		#endregion
		#region Implementation

		protected override BorderWiseLicenceController GetNewController(ZGuid contactPK)
		{
			return new BorderWiseLicenceController();
		}

		#endregion

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}
	}
}
