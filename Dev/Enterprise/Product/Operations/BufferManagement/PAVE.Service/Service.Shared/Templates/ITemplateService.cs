using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Service.Shared.Templates.Dtos;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface ITemplateService
	{
		string ApplyWorkItemNoteTemplate(Guid workItemId, ApplyTemplateRequest request);
		string ApplyTaskNoteTemplate(Guid taskId, ApplyTemplateRequest request);
		string ApplyTemplate(BusinessObject[] objects, ApplyTemplateRequest request);
	}
}
