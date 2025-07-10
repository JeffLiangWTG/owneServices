using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public class CloneWorkflowMenuItem : CloneMenuItem
	{
		public CloneWorkflowMenuItem(ZGrid parentGrid, Func<IBusinessObjectCollection> collectionGetter)
			: base(parentGrid, collectionGetter)
		{
		}

		protected override BusinessObject Clone(BusinessObject source)
		{
			if (source is ProcessJobHeader)
			{
				Globals.Message.Show(Res.GetString("8d291224-bf5b-4710-a75e-8f6982a54cc4", "Cannot clone the Job-level workflow."));
				return null;
			}
			else
			{
				return ((ProcessHeader)source).CloneWorkflow();
			}
		}

		protected override void OnElementsCloned(Collection<CloneResult> cloneResults)
		{
			base.OnElementsCloned(cloneResults);

			var workflowResults = cloneResults.Select(r => Tuple.Create((ProcessHeader)r.Source, (ProcessHeader)r.Clone)).ToArray();
			var linksToClone = workflowResults.SelectMany(x => x.Item1.LinksFromMeToOthers.Where(l => workflowResults.Any(tuple => tuple.Item1.PK == l.FP_FH_HeaderTo))).ToArray();

			foreach (var link in linksToClone)
			{
				var newLink = (ProcessHeaderLink)link.Clone();
				newLink.FP_FH_HeaderFrom = workflowResults.First(x => x.Item1.PK == link.FP_FH_HeaderFrom).Item2.PK;
				newLink.FP_FH_HeaderTo = workflowResults.First(x => x.Item1.PK == link.FP_FH_HeaderTo).Item2.PK;
			}
		}
	}
}
