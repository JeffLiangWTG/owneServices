using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Templates.Dtos;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.BufferManagement.Service.Templates
{
	public class TemplateService : ITemplateService
	{
		readonly BusinessObjectFactory factory = new BusinessObjectFactory() { NameForDebugging = nameof(TemplateService), RefreshEnabled = false };

		public string ApplyWorkItemNoteTemplate(Guid workItemId, ApplyTemplateRequest request)
		{
			var workItem = factory.Load<IWorkItem>(workItemId) as BusinessObject;
			return ApplyTemplate(new[] { workItem }, request);
		}

		public string ApplyTaskNoteTemplate(Guid taskId, ApplyTemplateRequest request)
		{
			var task = factory.Load<ProcessTask>(taskId);
			var parent = task?.Parent as BusinessObject ?? throw new ArgumentNullException(nameof(taskId));
			return ApplyTemplate(new[] { task, parent }, request);
		}

		public string ApplyTemplate(BusinessObject[] objects, ApplyTemplateRequest request)
		{
			if (string.IsNullOrEmpty(request?.Template))
			{
				return null;
			}

			var textMacroProcessor = ObjectFactory.Get<ITextMacroProcessor>();
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(TemplateService), RefreshEnabled = false };

			var result = string.Empty;

			result = textMacroProcessor.Replace(request.Template, objects, shouldEscapeAllSpecialCharacters: false);

			return result;
		}
	}
}
