using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Utilities.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Workflow
{
	public class ProcessHeaderLinkCopierForUniversalCopy : IUniversalCopyCustomFinishCopyAction
	{
		public int Priority { get => 2; }

		public void FinishCopyAction(Dictionary<object, object> copiedEntities)
		{
			var additionalCopiedEntities = new Dictionary<object, object>();

			if (copiedEntities is not null)
			{
				foreach (var source in copiedEntities.Keys)
				{
					if (source is IWorkflowProviderCore sourceWorkflowProvider)
					{
						var sourceBusinessObject = source as BusinessObject;
						var factory = sourceBusinessObject.Factory;

						var processJobHeaderProvider = ObjectFactory.Get<IProcessJobHeaderProvider>();
						var sourceWorkflows = processJobHeaderProvider.GetWorkflowsForParent(sourceWorkflowProvider, factory).ToArray();

						var links = sourceWorkflows.SelectMany(sourceWorkflow => ((Business.ProcessHeader)sourceWorkflow).LinksFromMeToOthers.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency && sourceWorkflows.Contains(l.HeaderTo)).ToArray())
							.Cast<ProcessHeaderLink>().ToArray();

						CloneLinks(links, copiedEntities, additionalCopiedEntities);
					}
				}

				foreach (var keyValuePair in additionalCopiedEntities)
				{
					copiedEntities.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}

		static void CloneLinks(ProcessHeaderLink[] links, Dictionary<object, object> copiedEntities, Dictionary<object, object> additionalCopiedEntities)
		{
			foreach (var link in links)
			{
				if (copiedEntities.Keys.Any(k => k is BusinessObject bizo && bizo.PK == link.PK))
				{
					continue;
				}

				var headerFrom = copiedEntities.Keys.FirstOrDefault(k => k is Business.ProcessHeader processHeader && processHeader.PK.Equals(link.HeaderFrom.PK));
				var headerTo = copiedEntities.Keys.FirstOrDefault(k => k is Business.ProcessHeader processHeader && processHeader.PK.Equals(link.HeaderTo.PK));

				if (headerFrom is not null && copiedEntities[headerFrom] is Business.ProcessHeader copiedHeaderFrom)
				{
					if (headerTo is not null && copiedEntities[headerTo] is Business.ProcessHeader copiedHeaderTo)
					{
						var cloneLink = (ProcessHeaderLink)link.Clone(new BusinessObjectCloneArgs(new[] { ProcessHeaderLinkSchema.FP_FH_HeaderFrom.Name, ProcessHeaderLinkSchema.FP_FH_HeaderTo.Name }));

						cloneLink.SuspendValidation();
						cloneLink.Template = link.Template;
						cloneLink.FP_FH_HeaderFrom = copiedHeaderFrom.PK;
						cloneLink.FP_FH_HeaderTo = copiedHeaderTo.PK;
						cloneLink.ResumeValidation();

						additionalCopiedEntities.Add(link, cloneLink);
					}
				}
			}
		}
	}
}
