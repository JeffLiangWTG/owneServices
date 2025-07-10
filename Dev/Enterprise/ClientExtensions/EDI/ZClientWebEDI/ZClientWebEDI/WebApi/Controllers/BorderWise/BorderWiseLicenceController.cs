using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise
{
	// TODO - Remove after User Management Portal project is completed.
	[RoutePrefix("api/BorderWiseLicence")]
	public class BorderWiseLicenceController : BorderWiseLicenceControllerBase
	{
		[Route("")]
		public string Get() => "BorderWise License Controller V1";

		#region Book Review and Rejection

		[HttpPost]
		[Route("RejectBook")]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "It comes in as a string from a URL parameter and is processed as a string.")]
		public IHttpActionResult RejectBook(Guid contactIdentifier, string contactEmail, string systemUrl, [FromBody] BookRejectionRequestDetails requestDetails, bool nonFatalError = false)
		{
			if (string.IsNullOrWhiteSpace(contactEmail))
			{
				return BadRequest("All arguments must be provided.");
			}

			return DoWithinAuthenticatedContext(contactIdentifier, contact =>
			{
				if (contact.OC_Email.EndsWith("@wisetechglobal.com", StringComparison.OrdinalIgnoreCase))
				{
					return RejectBookCore(contact,
						requestDetails,
						contactEmail,
						systemUrl,
						nonFatalError
						);
				}
				else
				{
					return Forbidden("The user is not permitted to perform this action.");
				}
			});
		}

		[HttpGet]
		[Route("workItems/{workItemNumber}")]
		public IHttpActionResult GetWorkItemSummary(string workItemNumber)
		{
			if (string.IsNullOrWhiteSpace(workItemNumber))
			{
				return BadRequest("An argument must be provided.");
			}

			using (Db.DisposableActionForDbConnection())
			{
				var workItem = GetWorkItem(new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseLicenceController) }, workItemNumber);

				if (workItem == null)
				{
					return NotFound();
				}

				if (workItem.WKI_WorkItemType != "BOR")
				{
					return BadRequest("Invalid work item product type. It should be 'BOR'");
				}

				return Ok(workItem.WKI_Summary);
			}
		}

		[HttpGet]
		[Route("book-rejection/{incidentNumber}/is-resolved")]
		public IHttpActionResult IsRejectionReasonResolved(string incidentNumber)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseLicenceController) };
			var query = new ZQuery(IncidentMainSchema.IM_IncidentNumber, incidentNumber)
				.AddToFilter(IncidentMainSchema.IM_Product, "BOR")
				.AddToFilter(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed);

			var exists = factory.Exists(typeof(SupportIncident), query);

			return Ok(exists);
		}

		[HttpPost]
		[Route("books/{bookId}/update")]
		public IHttpActionResult BookVersionUpdated(string bookId, [FromBody] BookVersionUpdatedRequestDetails requestDetails)
		{
			if (!ApiKeyIsValid())
			{
				return Forbidden("Request missing authorization 'X-API-KEY' header.");
			}

			Argument.NotNullOrEmpty(bookId, nameof(bookId));
			Argument.NotNull(requestDetails, nameof(requestDetails));
			Argument.NotNullOrEmpty(requestDetails.BookName, nameof(requestDetails.BookName));

			var workItemPriority = requestDetails.RequiresManualConversion
				? EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToManuallyProcess.Value
				: EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToAutomaticallyProcess.Value;

			if (!string.IsNullOrEmpty(workItemPriority))
			{
				var countryCode = requestDetails.CountryCode ?? string.Empty;
				var summary = (requestDetails.RequiresManualConversion ? "Manually process book update" : "Process book update") + ": " + requestDetails.BookName;
				var reasonDescription = !string.IsNullOrEmpty(requestDetails.ReasonDescription) ? requestDetails.ReasonDescription : "n/a";
				var details = FormattableString.Invariant(
$@"Book Name: {requestDetails.BookName}
Book ID: {bookId}
Version: {requestDetails.BookVersion}
Reason: {reasonDescription}");

				var factory = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseLicenceController) };
				var workItem = CreateBorderWiseContentWorkItem(factory, workItemPriority, summary, details, countryCode: countryCode);

				factory.Save();

				var workItemHyperlink = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.WorkItem, workItem.PK);

				return Ok(new BookVersionUpdatedResultDetails
				{
					WorkItemNumber = workItem.WKI_WorkItemNumber,
					WorkItemHyperlink = workItemHyperlink,
				});
			}
			else
			{
				return Ok(new BookVersionUpdatedResultDetails());
			}
		}

		#endregion

		#region Book Approval

		[HttpPost]
		[Route("ApproveBook")]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "It comes in as a string from a URL parameter and is processed as a string.")]
		public IHttpActionResult ApproveBook(Guid contactIdentifier, [FromBody] BookApprovalRequestDetails requestDetails)
		{
			return DoWithinAuthenticatedContext(contactIdentifier, contact =>
			{
				if (contact.OC_Email.EndsWith("@wisetechglobal.com", StringComparison.OrdinalIgnoreCase))
				{
					return ApproveBookCore(requestDetails);
				}
				else
				{
					return Forbidden("The user is not permitted to perform this action.");
				}
			});
		}

		#endregion

		#region Promotion Completed or Failed
		[HttpPost]
		[Route("BookPromotion")]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "It comes in as a string from a URL parameter and is processed as a string.")]
		public IHttpActionResult BookPromotion([FromBody] BookPromotionRequestDetails requestDetails)
		{
			if (!ApiKeyIsValid())
			{
				return Forbidden("Request missing authorization 'X-API-KEY' header.");
			}

			return BookPromotionCore(requestDetails);
		}
		#endregion

		#region Implementation

		internal static EDIOrgContact GetContactFromRequest(Guid contactPK, string contactPassword, out string failureMessage)
		{
			var contactQuery = GetContactsFilter(new ZQuery(OrgContactSchema.PK, contactPK), null);

			return GetContactFromRequestCore(contactQuery, contactPassword, out failureMessage);
		}

		internal static EDIOrgContact GetContactFromRequest(string orgCode, string contactEmail, string contactPassword, out string failureMessage)
		{
			Argument.NotNullOrEmpty(contactEmail, nameof(contactEmail));

			var contactQuery = GetContactsFilter(new ZQuery(OrgContactSchema.OC_Email, contactEmail), orgCode);

			return GetContactFromRequestCore(contactQuery, contactPassword, out failureMessage);
		}

		static EDIOrgContact GetContactFromRequestCore(ZQuery contactQuery, string contactPassword, out string failureMessage)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseLicenceController) };
			var contacts = factory.Load<EDIOrgContact>(contactQuery);
			if (!string.IsNullOrEmpty(contactPassword))
			{
				contacts = contacts.Where(contact => contact.VerifyPassword(contactPassword)).ToArray();
			}

			if (contacts.Length == 1)
			{
				var contact = contacts[0];

				var isRightGranted = OrgContactWebUser.IsRightGrantedWithoutCache(EDIWebSecurityRightsList.BorderWise, contact);
				if (isRightGranted)
				{
					failureMessage = null;
					return contact;
				}
				else
				{
					failureMessage = "You do not have the relevant security rights to access BorderWise. Please contact WiseTech Global.";
				}
			}
			else
			{
				failureMessage = "The user record could not be found.";
			}

			return null;
		}

		static ZQuery GetContactsFilter(ZQuery contactSubQuery, string orgCode)
		{
			var contactQuery = new ZDBOnlyQuery(typeof(EDIOrgContact));
			contactQuery.AddToFilter(contactSubQuery);
			contactQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			contactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);

			var orgSubQuery = new ZDBOnlySubQuery(typeof(EDIOrgHeader), OrgContactSchema.OC_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			if (!string.IsNullOrEmpty(orgCode))
			{
				orgSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, orgCode);
			}
			contactQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			return contactQuery;
		}

		IHttpActionResult DoWithinAuthenticatedContext(Guid contactPK, Func<OrgContact, IHttpActionResult> serviceFunction)
		{
			if (!ApiKeyIsValid())
			{
				return Forbidden("Request missing authorization 'X-API-KEY' header.");
			}

			if (contactPK == default)
			{
				return BadRequest("All arguments must be provided.");
			}

			using (Db.DisposableActionForDbConnection())
			{
				var contact = GetContactFromRequest(contactPK, contactPassword: string.Empty, out string failureMessage);

				if (!string.IsNullOrEmpty(failureMessage))
				{
					return Forbidden(failureMessage);
				}

				return serviceFunction.Invoke(contact);
			}
		}

		#endregion
	}
}
