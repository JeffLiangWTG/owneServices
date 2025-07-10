using System;
using System.Linq;
using System.Reflection;
using TestCaseBaseClass = NUnit.Framework.TestCase;

namespace CWNUnit.TestAdapter.Tests
{
	public class RedirectAssemblyResolverTestCase : TestCaseBaseClass
	{
		public void TestCanReadCW1ConfigFileAssemblyBindings()
		{
			var bindingRedirectInfos = RedirectAssemblyResolver.GetBindingRedirects(RedirectAssemblyResolver.GetCW1ExeConfigFileName()).ToList();

			AssertGreaterThan("Should have loaded binding redirects", bindingRedirectInfos.Count, 0);
			AssertNotNull("Should have expected assembly binding", bindingRedirectInfos.FirstOrDefault(info => info.AssemblyName.Equals("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase)));

			foreach (var info in bindingRedirectInfos)
			{
				Assert("Should have valid name", !string.IsNullOrEmpty(info.AssemblyName));
				Assert("Should have valid public key token", !string.IsNullOrEmpty(info.PublicKeyToken));
				AssertNotNull("Should have old version from", info.OldVersionFrom);
				AssertNotNull("Should have old version to", info.OldVersionTo);
				Assert("Should have new version", !string.IsNullOrEmpty(info.NewVersionString));
			}
		}

		public void TestResolveAssembly()
		{
			var info = new BindingRedirectInfo
			{
				AssemblyName = "Newtonsoft.Json",
				PublicKeyToken = "30ad4fe6b2a6aeed",
				OldVersionFrom = new Version(0, 0, 0, 0),
				OldVersionTo = new Version(13, 0, 0, 0),
				NewVersionString = "13.0.0.0",
			};

			using (var resolver = new RedirectAssemblyResolver(new[] { info }))
			{
				var assembly = resolver.ResolveAssembly(info);
				AssertNotNull("Should use binding redirect", assembly);
				AssertEquals("Should load correct assembly version", 13, assembly.GetName().Version.Major);
			}
		}

		public void TestResolveAssembly_ReordersResolvers()
		{
			// Arrange
			// Unhook other event handlers (e.g. TestPlatform + test runner)
			var assemblyResolveBackingHandler = (ResolveEventHandler)typeof(AppDomain).GetField("_AssemblyResolve", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(AppDomain.CurrentDomain);
			var invocationList = assemblyResolveBackingHandler?.GetInvocationList().OfType<ResolveEventHandler>().ToList();
			invocationList?.ForEach(h => AppDomain.CurrentDomain.AssemblyResolve -= h);

			var resolveAttempts = 0;
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

			try
			{
				var info = new BindingRedirectInfo
				{
					AssemblyName = "Newtonsoft.Json",
					PublicKeyToken = "30ad4fe6b2a6aeed",
					OldVersionFrom = new Version(0, 0, 0, 0),
					OldVersionTo = new Version(1000, 0, 0, 0), // Set a non-existent future version to ensure the assembly is not loaded normally
					NewVersionString = "13.0.0.0",
				};

				// Act
				using (var resolver = new RedirectAssemblyResolver(new[] { info }))
				{
					// Assert
					var fullName = $"{info.AssemblyName}, Version=999.0.0.0, Culture=neutral, PublicKeyToken={info.PublicKeyToken}";
					var assembly = Assembly.Load(fullName);
					AssertNotNull("Should use binding redirect", assembly);
					AssertEquals("Should load correct assembly version", 13, assembly.GetName().Version.Major);
					AssertEquals("Should not have hit earlier hooked assembly resolver.", 0, resolveAttempts);

					AssertExceptionThrown<System.IO.FileNotFoundException>(() =>
					{
						var fullName = "Newtonsoft.Json.NotExisting, Version=100.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed";
						Assembly.Load(fullName);
					});
					AssertEquals("Should have hit earlier hooked assembly resolver.", 1, resolveAttempts);
				}
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
				invocationList?.ForEach(h => AppDomain.CurrentDomain.AssemblyResolve += h);
			}

			Assembly AssemblyResolve(object sender, ResolveEventArgs args)
			{
				resolveAttempts++;
				return null;
			}
		}

		public void TestResolveAssemblyDuplicate()
		{
			var info1 = new BindingRedirectInfo
			{
				AssemblyName = "Newtonsoft.Json",
				PublicKeyToken = "30ad4fe6b2a6aeed",
				OldVersionFrom = new Version(0, 0, 0, 0),
				OldVersionTo = new Version(13, 0, 0, 0),
				NewVersionString = "13.0.0.0",
			};

			var info2 = new BindingRedirectInfo
			{
				AssemblyName = "Newtonsoft.Json",
				PublicKeyToken = "30AD4FE6B2A6AEED",
				OldVersionFrom = new Version(0, 0, 0, 0),
				OldVersionTo = new Version(13, 0, 0, 0),
				NewVersionString = "13.0.0.0",
			};

			var info3 = new BindingRedirectInfo
			{
				AssemblyName = "Newtonsoft.Json",
				PublicKeyToken = "30ad4fe6b2a6aeed",
				OldVersionFrom = new Version(0, 0, 0, 0),
				OldVersionTo = new Version(13, 0, 0, 0),
				NewVersionString = "13.0.0.0",
			};

			using (var resolver = new RedirectAssemblyResolver(new[] { info1, info2, info3 }))
			{
				var assembly = resolver.ResolveAssembly(info1);
				AssertNotNull("Should use binding redirect", assembly);
				AssertEquals("Should load correct assembly version", 13, assembly.GetName().Version.Major);

				assembly = resolver.ResolveAssembly(info2);
				AssertNotNull("Should use binding redirect", assembly);
				AssertEquals("Should load correct assembly version", 13, assembly.GetName().Version.Major);

				assembly = resolver.ResolveAssembly(info3);
				AssertNotNull("Should use binding redirect", assembly);
				AssertEquals("Should load correct assembly version", 13, assembly.GetName().Version.Major);
			}
		}
	}
}
