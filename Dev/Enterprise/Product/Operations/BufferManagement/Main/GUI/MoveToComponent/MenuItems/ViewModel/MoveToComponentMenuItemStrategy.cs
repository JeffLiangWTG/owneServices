using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public delegate IEnumerable<ProcessHeader> ProcessHeaderCollectionGetter();

	class MoveToComponentMenuItemStrategy
	{
		internal IEnumerable<MenuItem> GetMoveToComponentMenuItems(ZFilterGridModule module)
		{
			var processHeaders = new ProcessHeaderCollectionGetter(() => GetSelectedProcessHeaders(module));
			var menuItem = new MoveToComponentMenuItemMenuTree(processHeaders) { Name = MoveToComponentMenuItemProvider.MoveToComponentMenuItemName };

			return new[] { menuItem };
		}

		IEnumerable<ProcessHeader> GetSelectedProcessHeaders(ZFilterGridModule module)
		{
			var processHeaders = module.GetSelectedBusinessObjects().OfType<ProcessHeader>();
			var jobHeaders = processHeaders.OfType<ProcessJobHeader>();
			var workflows = processHeaders.Except(jobHeaders);
			var workflowsInJobHeaders = jobHeaders.SelectMany(h => h.ProcessHeaders);

			workflows = workflows.Concat(workflowsInJobHeaders);

			return workflows;
		}
	}
}
