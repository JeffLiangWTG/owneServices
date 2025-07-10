using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	public abstract class ConfigFileBindingRedirectTest : TestCase
	{
		public ConfigFileBindingRedirectTest()
		{
			SkipAssemblies = new string[] {
				"FSharp.Core",
				"Enterprise.RemoteDesktopServices.Shared",
				"WTG.DevTools.Database",
			};
		}

		public void TestAllDependentAssembliesAndBindingRedirectPresent()
		{
			var errorMessages = new List<string>();

			var appConfigAssemblyBindings = GetBindingRedirectsLookup(SettingsFilePath);
			var webConfigAssemblyBindings = GetBindingRedirectsLookup(DebugFilePath);

			foreach (var assemblyIdentity in appConfigAssemblyBindings)
			{
				// Dependent assembly in SettingsFile config
				var appConfigAssemblyIdentity = assemblyIdentity.First().Name;
				// Dependent assembly in Web.Base.config
				var webConfigDependentAssembly = webConfigAssemblyBindings[assemblyIdentity.Key];

				if (!webConfigDependentAssembly.Any())
				{
					errorMessages.Add($"Assembly Identity or Binding Redirect for {assemblyIdentity.Key} is missing in {DebugFilePath} when it is present in {SettingsFilePath}.\r\n");
				}
				else if (!(webConfigDependentAssembly.First().Name == appConfigAssemblyIdentity))
				{
					errorMessages.Add($"Binding Redirect not same for {assemblyIdentity.Key}." +
						$"\r\n\tWas {webConfigDependentAssembly.First()} in {DebugFilePath}" +
						$"\r\n\tExpected {assemblyIdentity.First()} as in App.Config {SettingsFilePath}.\r\n");
				}
			}

			foreach (var assemblyIdentity in webConfigAssemblyBindings)
			{
				// Dependent assembly in SettingsFile config
				var appConfigDependentAssembly = appConfigAssemblyBindings[assemblyIdentity.Key];

				if (!appConfigDependentAssembly.Any())
				{
					errorMessages.Add($"Extra AssemblyIdentity {assemblyIdentity.Key} present in {DebugFilePath} when it is not present in {SettingsFilePath}.");
				}
			}
			AssertEquals($"Error(s) in {DebugFilePath}\r\n\r\n{string.Join("\r\n", errorMessages)} ", 0, errorMessages.Count);
		}

		public void TestBindingRedirectVersions()
		{
			var errorMessages = new List<string>();
			var baseConfigAssemblyBindings = GetBindingRedirectsLookup(SettingsFilePath);
			var configAssemblyBindingsToTest = GetBindingRedirectsLookup(DebugFilePath);

			foreach (var assemblyIdentity in configAssemblyBindingsToTest)
			{
				try
				{
					if (SkipAssemblies.Contains(assemblyIdentity.Key, StringComparer.OrdinalIgnoreCase))
					{
						continue;
					}

					var redirect = assemblyIdentity.Select(e => BindingRedirectVersions.GetVersionValues(e)).First();

					var appConfigAssemblyIdentity = baseConfigAssemblyBindings[assemblyIdentity.Key].ToList();
					var correctRedirect = appConfigAssemblyIdentity.Select(e => BindingRedirectVersions.GetVersionValues(e)).FirstOrDefault();

					if (!redirect.VersionInRange(redirect.NewVersion) || correctRedirect?.NewVersion > redirect.NewVersion)
					{
						var errorMessage = $"Binding Redirect that exists for {assemblyIdentity.Key} is incorrect. The binding redirect is\r\n" +
							$"{assemblyIdentity.First()}.\r\n";

						if (appConfigAssemblyIdentity.Count == 0)
						{
							errorMessage += $"There is no equivalent binding redirect in App.Config ({SettingsFilePath})";
						}
						else
						{
							errorMessage += $"Replace the existing redirect with the below configuration \r\n" +
							$"{appConfigAssemblyIdentity[0]} as in App.config ({SettingsFilePath})";
						}

						errorMessages.Add(errorMessage + "\r\n\r\n");
					}
				}
				catch (InvalidBindingRedirectElementException e)
				{
					errorMessages.Add($"Binding Redirect that exists for {assemblyIdentity.Key} has an {e.Message}.\r\n\r\n");
				}
			}
			AssertEquals($"Error(s) in {DebugFilePath}\r\n\r\n{string.Join("\r\n", errorMessages)} ", 0, errorMessages.Count);
		}

		protected abstract string DebugFilePath { get; }
		protected string[] SkipAssemblies { get; set; }
		string SettingsFilePath => Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.WindowsDesktop.exe.config");

		static ILookup<string, XElement> GetBindingRedirectsLookup(string filePath) => XMLHelper.GetExistingRedirects(filePath).
			Select(r => new { Name = XMLHelper.GetAttribute(r, "assemblyIdentity", "name"), BindingRedirect = XMLHelper.GetElement(r, "bindingRedirect") })
			.Where(o => o.Name != null).ToLookup(o => o.Name, o => o.BindingRedirect);
	}

	class XMLHelper
	{
		public static IEnumerable<XElement> GetExistingRedirects(string configFilePath)
		{
			var config = XDocument.Load(configFilePath);
			var runtimeElement = config?.Element("configuration")?.Element("runtime");

			return runtimeElement?.Elements(Namespace + "assemblyBinding").SelectMany(s => s.Elements(Namespace + "dependentAssembly"))
				?? Enumerable.Empty<XElement>();
		}

		public static XElement GetElement(XElement element, string name) => element?.Element(Namespace + name);

		public static string GetAttribute(XElement element, string name, string attribute) => GetElement(element, name)?.Attribute(attribute)?.Value;

		static XNamespace Namespace => "urn:schemas-microsoft-com:asm.v1";
	}
}
