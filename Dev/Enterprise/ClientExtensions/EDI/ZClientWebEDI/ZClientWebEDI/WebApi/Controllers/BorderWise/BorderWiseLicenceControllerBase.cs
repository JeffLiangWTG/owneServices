using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise
{
	public abstract class BorderWiseLicenceControllerBase : ControllerWithEnvironment
	{
		const string apiKey = "RdLYzusLICs8cl/wO49inQ7DGjDEQCGXLTL7rSlHI44=";

		protected bool ApiKeyIsValid()
		{
			if (Request?.Headers != null)
			{
				Request.Headers.TryGetValues("X-API-KEY", out var values);
				var requestApikey = values?.FirstOrDefault();
				return requestApikey == apiKey;
			}
			return false;
		}

#if DEBUG
		public void SetApiKey()
		{
			Request.Headers.Add("X-API-KEY", apiKey);
		}
#endif

		#region For Test
#if DEBUG
		public AutoResetEvent IsValidLicenceWaiter { get; } = new AutoResetEvent(false);
#endif
		#endregion

		#region Book Review

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "It comes in as a string in JSON and is processed as a string.")]
		protected IHttpActionResult RejectBookCore(OrgContact contact, BookRejectionRequestDetails requestDetails, string contactEmail, string systemUrl, bool nonFatalError)
		{
			Argument.NotNull(requestDetails, nameof(requestDetails));
			Argument.NotNull(requestDetails.BookName, nameof(requestDetails.BookName));
			Argument.NotNull(requestDetails.BookId, nameof(requestDetails.BookId));
			Argument.NotNull(requestDetails.RejectionReason, nameof(requestDetails.RejectionReason));
			Argument.NotNull(requestDetails.WorkItemNumber, nameof(requestDetails.WorkItemNumber));
			Argument.NotNull(contactEmail, nameof(contactEmail));
			Argument.NotNull(systemUrl, nameof(systemUrl));

			var details = requestDetails.Details?.Replace("\n", "\r\n")?.Replace("\r\r\n", "\r\n");
			var fullDetails = FormattableString.Invariant($@"Book name: {requestDetails.BookName}
Book ID: {requestDetails.BookId}
Content ID: {requestDetails.ContentId}
{(nonFatalError ? "Non-fatal error raised" : "Rejected")} by: {contactEmail}
Reason for {(nonFatalError ? "non-fatal error" : "rejection")}: {requestDetails.RejectionReason}
System URL: {systemUrl}
Details: {details}");

			var factory = new BusinessObjectFactory { NameForDebugging = GetType().FullName };
			var workItem = GetWorkItem(factory, requestDetails.WorkItemNumber);
			if (workItem == null)
			{
				return Content(HttpStatusCode.NotFound, "WorkItem does not exist.");
			}

			RaiseBookEvent(workItem, Events.RejectBook, requestDetails.Type, requestDetails.ReviewedItemName);

			var incident = factory.New<SupportIncident>();
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Product = "BOR";
			incident.IM_Priority = "CR4";
			incident.ProductArea = "BOR";
			incident.IM_Module = "CDP";
			incident.IM_Description = (nonFatalError ? "Non-fatal error with book: " : "Rejected book: ") + requestDetails.BookName;
			incident.DetailNoteText = fullDetails;

			factory.Save();

			var incidentHyperlink = ShowEditFormUrlHandler.Instance.Create(ClientControllerRegistration.SupportIncident, incident.PK);

			return Ok(new BookRejectionResultDetails
			{
				IncidentNumber = incident.IM_IncidentNumber,
				IncidentHyperlink = incidentHyperlink,
			});
		}

		protected static WorkItem CreateBorderWiseContentWorkItem(BusinessObjectFactory factory, string workItemPriority, ZString summary, string details, string countryCode)
		{
			var workItem = factory.New<WorkItem>();

			workItem.WKI_WorkItemType = "BOR";
			workItem.WKI_WorkItemArea = "CNT";
			workItem.WKI_ActivityType = countryCode;
			workItem.WKI_ActivitySubtype = "CNT";
			workItem.WKI_Priority = workItemPriority;
			workItem.WKI_Summary = summary.SubstringSafe(0, WorkItemSchema.WKI_Summary.MaxLength);
			workItem.WKI_Details = ZBlob.FromUTF8(details?.TrimEnd());

			return workItem;
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "It comes in as a string in JSON and is processed as a string.")]
		protected IHttpActionResult ApproveBookCore(BookApprovalRequestDetails requestDetails)
		{
			Argument.NotNull(requestDetails, nameof(requestDetails));
			Argument.NotNullOrEmpty(requestDetails.WorkItemNumber, nameof(requestDetails.WorkItemNumber));
			Argument.NotNullOrEmpty(requestDetails.Type, nameof(requestDetails.Type));
			Argument.NotNullOrEmpty(requestDetails.ReviewedItemName, nameof(requestDetails.ReviewedItemName));

			var factory = new BusinessObjectFactory { NameForDebugging = GetType().FullName };
			var workItem = GetWorkItem(factory, requestDetails.WorkItemNumber);
			if (workItem == null)
			{
				return Content(HttpStatusCode.NotFound, "WorkItem does not exist.");
			}

			RaiseBookEvent(workItem, Events.ApproveBook, requestDetails.Type, requestDetails.ReviewedItemName);

			factory.Save();

			return Ok("Approve book event was successfully raised");
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "It comes in as a string in JSON and is processed as a string.")]
		protected IHttpActionResult BookPromotionCore(BookPromotionRequestDetails requestDetails)
		{
			Argument.NotNull(requestDetails, nameof(requestDetails));
			Argument.NotNullOrEmpty(requestDetails.WorkItemNumber, nameof(requestDetails.WorkItemNumber));
			Argument.NotNullOrEmpty(requestDetails.PromotedItemVersion, nameof(requestDetails.PromotedItemVersion));
			Argument.NotNullOrEmpty(requestDetails.PromotedItemName, nameof(requestDetails.PromotedItemName));
			Argument.NotNull(requestDetails.Completed, nameof(requestDetails.Completed));

			var factory = new BusinessObjectFactory { NameForDebugging = GetType().FullName };
			var workItem = GetWorkItem(factory, requestDetails.WorkItemNumber);
			if (workItem == null)
			{
				return Content(HttpStatusCode.NotFound, "WorkItem does not exist.");
			}

			if (requestDetails.Completed)
			{
				RaiseBookEvent(workItem, Events.PromotionCompleted, requestDetails.PromotedItemVersion, requestDetails.PromotedItemName);
			}
			else
			{
				RaiseBookEvent(workItem, Events.PromotionFailed, requestDetails.PromotedItemVersion, requestDetails.PromotedItemName);
			}

			factory.Save();

			return Ok("Book promotion event was successfully raised");
		}

		static void RaiseBookEvent(WorkItem workItem, Event @event, string itemType, string itemName)
		{
			workItem.Logs.AddNew(@event, new[]
			{
				new KeyValuePair<string, string>("TYP", itemType),
				new KeyValuePair<string, string>("NAM", itemName),
			});
		}

		#endregion

		#region Implementation

		protected WorkItem GetWorkItem(BusinessObjectFactory factory, string workItemNumber)
		{
			var query = new ZQuery(WorkItemSchema.WKI_WorkItemNumber, workItemNumber);
			return factory.LoadTop1<WorkItem>(query);
		}

		protected NegotiatedContentResult<ForbiddenResponse> Forbidden(string message)
		{
			return Content(
				HttpStatusCode.Forbidden,
				new ForbiddenResponse
				{
					Message = message
				});
		}

		#endregion

		#region Data Structures

		public class ForbiddenResponse
		{
			public string Message { get; set; }
		}

		#endregion
	}
}
