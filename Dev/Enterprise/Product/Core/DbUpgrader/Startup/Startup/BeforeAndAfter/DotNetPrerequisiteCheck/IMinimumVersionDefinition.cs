using System;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	public interface IMinimumVersionDefinition
	{
		string DisplayName { get; }

		bool IsSupported(ReadOnlyCollection<DotNetCoreRuntime> dotNetCoreRuntimes);
	}

	public abstract class BaseDotNetVersionDefinition : IMinimumVersionDefinition
	{
		public abstract string MinRequired { get; }
		public abstract string DisplayName { get; }
		public abstract DotNetCoreRuntimeType RuntimeType { get; }

		public bool IsSupported(ReadOnlyCollection<DotNetCoreRuntime> dotNetCoreRuntimes)
		{
			var actualVersion = dotNetCoreRuntimes.Where(r => r.Type == RuntimeType).OrderByDescending(r => r.Version).FirstOrDefault()?.Version;

			return actualVersion >= new Version(MinRequired);
		}

		public override string ToString()
		{
			return $"{DisplayName} (v{MinRequired})";
		}
	}

	public sealed class MinimumDotNetRuntimeRequired : BaseDotNetVersionDefinition
	{
		public override string MinRequired => "8.0.0";
		public override string DisplayName => ".NET Runtime 8.0";
		public override DotNetCoreRuntimeType RuntimeType => DotNetCoreRuntimeType.DotNetRuntime;
	}

	public sealed class MinimumDotNetDesktopRuntimeRequired : BaseDotNetVersionDefinition
	{
		public override string MinRequired => "8.0.0";
		public override string DisplayName => ".NET Desktop Runtime 8.0";
		public override DotNetCoreRuntimeType RuntimeType => DotNetCoreRuntimeType.DotNetDesktopRuntime;
	}

	public sealed class MinimumAspNetCoreRuntimeRequired : BaseDotNetVersionDefinition
	{
		public override string MinRequired => "8.0.0";
		public override string DisplayName => "ASP.NET Core Runtime (Hosting Bundle) 8.0";
		public override DotNetCoreRuntimeType RuntimeType => DotNetCoreRuntimeType.AspNetCoreRuntime;
	}
}
