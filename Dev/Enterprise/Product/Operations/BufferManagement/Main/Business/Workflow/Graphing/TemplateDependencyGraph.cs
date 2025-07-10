using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class TemplateDependencyGraph : JobDependencyGraph
	{
		internal TemplateDependencyGraph(ProcessJobHeader processJobHeader)
			: base(processJobHeader)
		{
			this.template = processJobHeader.Template;

			if (template == null)
			{
				throw new ArgumentException("ProcessJobHeader.Template was null", $"{nameof(processJobHeader)}.{nameof(processJobHeader.Template)}");
			}
		}

		readonly ProcessTaskTemplate template;

		protected override IEnumerable<ProcessHeader> GetVertexes()
		{
			return template.ProcessHeaders.Cast<ProcessHeader>().Except(new[] { base.Source });
		}

		protected override IEnumerable<ProcessHeaderLink> GetLinks()
		{
			return template.ProcessHeaderLinks.Cast<ProcessHeaderLink>();
		}
	}
}
