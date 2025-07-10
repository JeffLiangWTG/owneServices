using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderLoopsChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "Baseline")]
		public void CheckLoops(IProgressReporterProvider progressReporterProvider, ProcessHeader[] processHeaders)
		{
			var detectedHierarchicCycles = new List<IEnumerable<ProcessHeader>>();
			var detectedDependencyCycles = new List<IEnumerable<ProcessHeader>>();

			using (var progressReporter = progressReporterProvider.CreateProgressReporter(Res.GetString("A10271B8-9216-489C-8710-7AEE14F209C7", "Checking workflow loops"), processHeaders.Length))
			{
				var comparer = new ProcessHeaderListComparer();

				foreach (var header in processHeaders)
				{
					if (progressReporter.IsCancelled)
					{
						return;
					}

					var hierarchic = header.GetHierarchicCycles().ToList();
					var dependency = header.GetDependencyCycles().ToList();

					if (hierarchic.Count > 1 && !detectedHierarchicCycles.Contains(hierarchic, comparer))
					{
						detectedHierarchicCycles.Add(hierarchic);
					}

					if (dependency.Count > 1 && !detectedDependencyCycles.Contains(dependency, comparer))
					{
						detectedDependencyCycles.Add(dependency);
					}

					progressReporter.ReportOneItemProcessed();
				}
			}

			if (!detectedHierarchicCycles.Any() && !detectedDependencyCycles.Any())
			{
				Globals.Message.Show(Res.GetString("85A93DB2-BF4E-49C4-9ECE-EF29DD64D8F2", "Manually invoked workflow loop validation completed with no errors."));
			}
			else
			{
				string hierarchicString = string.Empty;
				if (detectedHierarchicCycles.Any())
				{
					hierarchicString = string.Join(System.Environment.NewLine,
						detectedHierarchicCycles.Select(c => "- " + string.Join(", ", c.Select(p => p.Name)) + "."));
				}

				string dependencyString = string.Empty;
				if (detectedDependencyCycles.Any())
				{
					dependencyString = string.Join(System.Environment.NewLine,
						detectedDependencyCycles.Select(c => "- " + string.Join(", ", c.Select(p => p.Name)) + "."));
				}

				Globals.Message.Show(string.Format("{0}{1}{2}{3}{4}",
					Res.GetString("2E021D32-4D5F-4CD5-8726-02BA5EA16577", "Manually invoked workflow loop validation completed with the following errors:"),
					System.Environment.NewLine, System.Environment.NewLine,
					(string.IsNullOrEmpty(hierarchicString) ? string.Empty : Res.GetString("55ec4dee-2d6c-4d97-b9ef-f542e2f4dc11", "hierarchic cycles:{0}{1}{2}{3}", System.Environment.NewLine, hierarchicString, System.Environment.NewLine, System.Environment.NewLine)),
					(string.IsNullOrEmpty(dependencyString) ? string.Empty : Res.GetString("854dfe4b-84fd-4382-9d26-cfff97ed6ed1", "dependency cycles:{0}{1}", System.Environment.NewLine, dependencyString))));
			}
		}

		class ProcessHeaderListComparer : IEqualityComparer<IEnumerable<ProcessHeader>>
		{
			public bool Equals(IEnumerable<ProcessHeader> p1, IEnumerable<ProcessHeader> p2)
			{
				var l1 = p1.Select(p => p.PK);
				var l2 = p2.Select(p => p.PK);
				return !l1.Except(l2).Any() && !l2.Except(l1).Any();
			}

			public int GetHashCode(IEnumerable<ProcessHeader> p)
			{
				return p.GetHashCode();
			}
		}
	}
}
