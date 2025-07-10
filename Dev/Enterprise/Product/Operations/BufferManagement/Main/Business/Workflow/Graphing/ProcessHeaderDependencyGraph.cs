using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Implementation;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderDependencyGraph : DependencyGraphBase<ProcessHeader, ProcessHeaderLink>
	{
		public ProcessHeaderDependencyGraph(ProcessHeader source)
			: base(source)
		{
		}

		readonly ProcessHeaderDescendantsStrategy descendantStrategy = new ProcessHeaderDescendantsStrategy();

		protected override IEnumerable<ProcessHeader> GetVertexes()
		{
			return ((ILinkEntity)Source).Children(descendantStrategy).Cast<ProcessHeader>();
		}

		protected override IEnumerable<ProcessHeaderLink> GetLinks()
		{
			return GetVertexes().SelectMany(h => h.PostrequisiteLinks);
		}
	}
}
