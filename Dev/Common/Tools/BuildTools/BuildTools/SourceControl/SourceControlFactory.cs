using System;
using LibGit2Sharp;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.BuildTools
{
	[Immutable]
	public sealed class SourceControlFactory : ISourceControlFactory
	{
		public static SourceControlFactory Instance { get; } = new SourceControlFactory();

		SourceControlFactory()
		{
		}

		public ISourceControl GetSourceControl()
		{
			return GetSourceControl(BuildConstants.LocalEnterprisePath);
		}

		public ISourceControl GetSourceControl(string localPath) => new GitSourceControl(Discover(localPath));

		public ISourceControl WithAdditionalRepository(ISourceControl sourceControl, string localPath)
		{
			if (!(sourceControl is IAggregatableSourceControl agg))
			{
				throw new InvalidOperationException("SourceControl aggregation not supported by the current instance.");
			}

			var sc = new GitSourceControl(Discover(localPath));
			return agg.WithAdditionalRepository(sc, sc.WorkingDirectory);
		}

		static string Discover(string localPath)
		{
			return Repository.Discover(localPath)
				?? throw new ArgumentException("Provided path does not lead to a valid git repository.");
		}
	}
}
