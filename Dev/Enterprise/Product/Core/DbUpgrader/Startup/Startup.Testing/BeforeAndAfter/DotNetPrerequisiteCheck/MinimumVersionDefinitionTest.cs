using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck.Testing
{
	sealed class MinimumVersionDefinitionTest : TestCase
	{
		readonly ReadOnlyCollection<DotNetCoreRuntime> allValidSingle = new(new List<DotNetCoreRuntime>
		{
			new(new Version("8.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.AspNetCoreRuntime),
		});

		readonly ReadOnlyCollection<DotNetCoreRuntime> allValidMulti = new(new List<DotNetCoreRuntime>
		{
			new(new Version("5.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("7.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("10.1.2"), DotNetCoreRuntimeType.AspNetCoreRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("8.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.AspNetCoreRuntime),
		});

		readonly ReadOnlyCollection<DotNetCoreRuntime> oldDotNetRuntime = new(new List<DotNetCoreRuntime>
		{
			new(new Version("5.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("7.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.AspNetCoreRuntime)
		});

		readonly ReadOnlyCollection<DotNetCoreRuntime> oldAspNetCoreRuntime = new(new List<DotNetCoreRuntime>
		{
			new(new Version("5.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("7.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("8.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("7.1.2"), DotNetCoreRuntimeType.AspNetCoreRuntime),
		});

		readonly ReadOnlyCollection<DotNetCoreRuntime> oldDotNetDesktopRuntime = new(new List<DotNetCoreRuntime>
		{
			new(new Version("5.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("7.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("8.7.2"), DotNetCoreRuntimeType.DotNetRuntime),
			new(new Version("6.7.2"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			new(new Version("8.1.2"), DotNetCoreRuntimeType.AspNetCoreRuntime),
		});

		public void TestDotNetRuntimeIsSupported()
		{
			var dotNetRuntime = new MinimumDotNetRuntimeRequired();

			AssertEquals("DotNetRuntime is valid, should be true", true, dotNetRuntime.IsSupported(allValidSingle));
			AssertEquals("DotNetRuntime is valid, should be true", true, dotNetRuntime.IsSupported(allValidMulti));
			AssertEquals("DotNetRuntime is old, should be false", false, dotNetRuntime.IsSupported(oldDotNetRuntime));
			AssertEquals("DotNetRuntime is valid, should be true", true, dotNetRuntime.IsSupported(oldAspNetCoreRuntime));
			AssertEquals("DotNetRuntime is valid, should be true", true, dotNetRuntime.IsSupported(oldDotNetDesktopRuntime));
		}

		public void TestDotNetDesktopRuntimeIsSupported()
		{
			var dotNetRuntime = new MinimumDotNetDesktopRuntimeRequired();

			AssertEquals("DotNetDesktopRuntime is valid, should be true", true, dotNetRuntime.IsSupported(allValidSingle));
			AssertEquals("DotNetDesktopRuntime is valid, should be true", true, dotNetRuntime.IsSupported(allValidMulti));
			AssertEquals("DotNetDesktopRuntime is valid, should be true", true, dotNetRuntime.IsSupported(oldDotNetRuntime));
			AssertEquals("DotNetDesktopRuntime is valid, should be true", true, dotNetRuntime.IsSupported(oldAspNetCoreRuntime));
			AssertEquals("DotNetDesktopRuntime is old, should be false", false, dotNetRuntime.IsSupported(oldDotNetDesktopRuntime));
		}

		public void TestMinimumAspNetCoreRuntimeRequiredIsSupported()
		{
			var dotNetRuntime = new MinimumAspNetCoreRuntimeRequired();

			AssertEquals("AspNetCoreRuntime is valid, should be true", true, dotNetRuntime.IsSupported(allValidSingle));
			AssertEquals("AspNetCoreRuntime is valid, should be true", true, dotNetRuntime.IsSupported(allValidMulti));
			AssertEquals("AspNetCoreRuntime is valid, should be true", true, dotNetRuntime.IsSupported(oldDotNetRuntime));
			AssertEquals("AspNetCoreRuntime is old, should be false", false, dotNetRuntime.IsSupported(oldAspNetCoreRuntime));
			AssertEquals("AspNetCoreRuntime is valid, should be true", true, dotNetRuntime.IsSupported(oldDotNetDesktopRuntime));
		}
	}
}
