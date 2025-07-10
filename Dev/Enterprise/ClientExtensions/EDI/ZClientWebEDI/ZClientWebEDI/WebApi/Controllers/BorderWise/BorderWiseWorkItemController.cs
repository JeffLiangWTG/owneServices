using System;
using System.Net;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZClientWebCargoWiseEDI.BorderWise;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers.BorderWise
{
	[RoutePrefix("api/BorderWiseWorkItems")]
	public class BorderWiseWorkItemController : BorderWiseApiBaseController
	{
		[Route("create-work-item")]
		[HttpPost]
		public IHttpActionResult CreateWorkItem([FromBody] CreateWorkItemRequest model)
		{
			_ = model ?? throw new ArgumentNullException(nameof(model));

			if (!IsValidApiKey(model.ApiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			if (model.Product != "BOR")
			{
				return BadRequest("This controller currently only supports BorderWise WIs");
			}

			if (model.Product.Length != 3) // redundant for now
			{
				return BadRequest("Product argument invalid. Three character product code must be provided");
			}

			if (string.IsNullOrWhiteSpace(model.ChangeType))
			{
				return BadRequest("ChangeType argument must be provided");
			}

			if (string.IsNullOrWhiteSpace(model.Summary))
			{
				return BadRequest("Summary argument must be provided");
			}

			if (string.IsNullOrWhiteSpace(model.Description))
			{
				return BadRequest("Description argument must be provided");
			}

			using (Db.DisposableActionForDbConnection())
			{
				var workItem = CreateWorkItemCore(DataFactory, model);

				/* 
				 * Have to do a save here since these custom fields only
				 * exist once a template has been loaded based on above fields
				*/
				DataFactory.Save();

				if (model.DueDate != null)
				{
					SetCustomFieldValue(workItem, "Due Date", "DAT", new ZDate(model.DueDate));
				}

				if (!string.IsNullOrEmpty(model.CapDev))
				{
					SetCustomFieldValue(workItem, "CapDev", "STR", new ZString(model.CapDev));
				}

				if (!string.IsNullOrEmpty(model.RND))
				{
					SetCustomFieldValue(workItem, "R&D", "STR", new ZString(model.RND));
				}

				DataFactory.Save();

				return Ok(workItem.WKI_WorkItemNumber.ToString());
			}
		}

		void SetCustomFieldValue(WorkItem workItem, string fieldName, string fieldType, IZType fieldValue)
		{
			var fieldAccessor = workItem.GetCustomFieldAccessor(fieldName, fieldType);
			if (fieldAccessor != null)
			{
				fieldAccessor.SetValue(fieldValue);
			}
		}

		WorkItem CreateWorkItemCore(BusinessObjectFactory factory, CreateWorkItemRequest model)
		{
			var workItem = factory.New<WorkItem>();

			workItem.WKI_WorkItemType = model.Product;
			workItem.WKI_WorkItemArea = model.ProductArea;
			workItem.WKI_ActivityType = model.Module;
			workItem.WKI_ActivitySubtype = model.ChangeType;
			workItem.WKI_Priority = model.Priority;
			workItem.WKI_Summary = model.Summary;
			workItem.WKI_Details = ZBlob.FromUTF8(model.Description.TrimEnd());

			return workItem;
		}
	}
}
