using System;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Common;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.BufferManagement.Service.Shared.Common.Dtos;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.BufferManagement.Service
{
	public class ProjectService : IProjectService
	{
		public ContentWithHashDto GetDetails(Guid projectId)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(ProjectService), RefreshEnabled = false };
			var project = factory.Load<IProject>(projectId);

			if (project == null)
			{
				throw new ArgumentNullException(nameof(projectId), nameof(project.WKP_Details));
			}

			var (hash, details) = project.WKP_Details.BlobToHtmlWithHash();

			return new ContentWithHashDto()
			{
				Hash = hash,
				Content = details
			};
		}

		public TryUpdateDetailsResult TryUpdateDetails(Guid projectId, UpdateContentWithHashRequest updateRequest)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(ProjectService), RefreshEnabled = false };
			var project = factory.Load<IProject>(projectId);

			if (string.IsNullOrWhiteSpace(updateRequest.PreviousHash))
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			if (updateRequest.NewContent == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			var hash = HashHelper.GetHash(project.WKP_Details.ToUTF8());

			if (hash != updateRequest.PreviousHash)
			{
				return new TryUpdateDetailsResult(LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage.WrapWithEnumerable());
			}

			project.WKP_Details = updateRequest.NewContent.HtmlToZBlob();

			factory.Save();

			var (newHash, details) = project.WKP_Details.BlobToHtmlWithHash();

			return new TryUpdateDetailsResult(new ContentWithHashDto { Hash = newHash, Content = details });
		}

		public bool TryUpdateSummary(Guid projectId, UpdateContentWithHashRequest updateRequest, out PaveError error)
		{
			error = null;

			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(ProjectService), RefreshEnabled = false };
			var foundProject = factory.Load<IProject>(projectId);

			if(foundProject == null)
			{
				throw new ArgumentNullException(nameof(projectId), nameof(foundProject));
			}

			if(string.IsNullOrWhiteSpace(updateRequest.PreviousHash))
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.PreviousHash));
			}

			if(updateRequest.NewContent == null)
			{
				throw new ArgumentNullException(nameof(updateRequest), nameof(updateRequest.NewContent));
			}

			var expectedHash = HashHelper.GetHash(foundProject.WKP_Summary);

			if(expectedHash != updateRequest.PreviousHash)
			{
				error = new PaveError(PaveErrorCode.ValidationError, new string[] { LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage });
				return false;
			}

			foundProject.WKP_Summary = updateRequest.NewContent;
			factory.Save();

			return true;
		}
	}
}
