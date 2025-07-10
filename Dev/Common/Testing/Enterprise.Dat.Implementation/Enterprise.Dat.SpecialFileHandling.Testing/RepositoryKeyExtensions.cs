using System;
using Dat.Integration;
using WTG.DevTools.Definitions;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	static class RepositoryKeyExtensions
	{
		public static IRepositoryKey ForDat(this RepositoryKey repositoryKey)
		{
			if (repositoryKey is null)
			{
				throw new ArgumentNullException(nameof(repositoryKey));
			}

			return new RepositoryKeyWrapper(repositoryKey);
		}

		sealed class RepositoryKeyWrapper : IRepositoryKey
		{
			public RepositoryKeyWrapper(RepositoryKey key)
			{
				inner = key;
			}

			string IRepositoryKey.TargetRepository => inner.TargetRepository;
			string IRepositoryKey.Branch => inner.Branch;
			string IRepositoryKey.Path => inner.Path;
			bool IRepositoryKey.IsReleaseBranch => inner.IsReleaseBranch;
			string IRepositoryKey.GetServerPath(string relativePath) => inner.GetServerPath(relativePath);
			public string GetServerPathRoot() => inner.Path;

			readonly RepositoryKey inner;
		}
	}
}
