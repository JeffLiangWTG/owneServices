using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Common;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.BufferManagement.Service.Shared.Common.Dtos;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Service
{
	//TODO: *** CHECK SECURITY RIGHTS **** IMPORTANT!
	public class WorkItemService : IWorkItemService
	{
		public ContentWithHashDto GetDetails(Guid workItemId)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService), RefreshEnabled = false };
			var workItem = factory.Load<IWorkItem>(workItemId);
			var (hash, details) = workItem.WKI_Details.BlobToHtmlWithHash();

			return new ContentWithHashDto()
			{
				Hash = hash,
				Content = details
			};
		}

		public TryUpdateDetailsResult TryUpdateDetails(Guid workItemId, UpdateContentWithHashRequest updateRequest)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService), RefreshEnabled = false };
			var workItem = factory.Load<IWorkItem>(workItemId);

			if (string.IsNullOrWhiteSpace(updateRequest.PreviousHash))
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			if (updateRequest.NewContent == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			var hash = HashHelper.GetHash(workItem.WKI_Details.ToUTF8());

			if (hash != updateRequest.PreviousHash)
			{
				return new TryUpdateDetailsResult(LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage.WrapWithEnumerable());
			}

			workItem.WKI_Details = updateRequest.NewContent.HtmlToZBlob();

			factory.Save();

			var (newHash, details) = workItem.WKI_Details.BlobToHtmlWithHash();

			return new TryUpdateDetailsResult(new ContentWithHashDto { Hash = newHash, Content = details });
		}

		public bool TryUpdateSummary(Guid workItemId, UpdateContentWithHashRequest updateRequest, out PaveError error)
		{
			error = null;
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService), RefreshEnabled = false };
			var workItem = factory.Load<IWorkItem>(workItemId);

			if (updateRequest.PreviousHash == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			if (updateRequest.NewContent == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.NewContent));
			}

			var hash = HashHelper.GetHash(workItem.WKI_Summary);
			if (hash != updateRequest.PreviousHash)
			{
				error = new PaveError(PaveErrorCode.ValidationError, new string[] { LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage });
				return false;
			}

			workItem.WKI_Summary = updateRequest.NewContent;

			factory.Save();

			return true;
		}

		#region WorkItem Selection Criteria

		public IEnumerable<CodeDescriptionDto> GetWorkItemTypes()
		{
			var list = ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemTypes(activeOnly: false);
			return ConvertToStringPairs(list);
		}

		public IEnumerable<CodeDescriptionDto> GetWorkItemAreas(string workItemType)
		{
			var list = ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemAreas(workItemType, activeOnly: false);
			return ConvertToStringPairs(list);
		}

		public IEnumerable<CodeDescriptionDto> GetActivityTypes(string workItemType, string workItemArea)
		{
			var list = ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetActivityTypes(workItemType, workItemArea, activeOnly: false);
			return ConvertToStringPairs(list);
		}

		public IEnumerable<CodeDescriptionDto> GetActivitySubtypes(string workItemType, string workItemArea, string activityType)
		{
			var list = ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetActivitySubtypes(workItemType, workItemArea, activityType, activeOnly: false);
			return ConvertToStringPairs(list);
		}

		public IEnumerable<CodeDescriptionDto> GetPriorities(string workItemType, string workItemArea, string activityType, string activitySubType)
		{
			var list = ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetPriorities(workItemType, workItemArea, activityType, activitySubType, activeOnly: false);
			return ConvertToStringPairs(list);
		}

		static IEnumerable<CodeDescriptionDto> ConvertToStringPairs(CodeDescriptionPairList pairList) => pairList.ToArray().Select(e => new CodeDescriptionDto() { Code = e.Code, Description = e.Description });

		#endregion
	}
}
