using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Enterprise.Upgrades;

namespace CWNUnit.TestAdapter
{
	public class RedirectAssemblyResolver : IDisposable
	{
		public static RedirectAssemblyResolver HookCW1AssemblyBindingRedirects()
		{
			var configFileName = GetCW1ExeConfigFileName();
			if (!string.IsNullOrEmpty(configFileName) && File.Exists(configFileName))
			{
				var bindingRedirects = GetBindingRedirects(configFileName);
				return new RedirectAssemblyResolver(bindingRedirects);
			}

			return null;
		}

		public static string GetCW1ExeConfigFileName()
		{
			return Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				ExeFileNames.CargoWiseWindowsDesktopExe + ".config");
		}

		public RedirectAssemblyResolver(IEnumerable<BindingRedirectInfo> bindingRedirects)
		{
			var bindingsDict = bindingRedirects
				.ToLookup(info => $"{info.AssemblyName}|{info.PublicKeyToken}", StringComparer.OrdinalIgnoreCase)
				.ToDictionary(info => info.Key, info => info.FirstOrDefault(), StringComparer.OrdinalIgnoreCase);

			if (bindingsDict.Count > 0)
			{
				bindingRedirectInfos = bindingsDict;

				// Unhook other event handlers (e.g. TestPlatform), to guarantee our handler is run first, to approximate CW1 .config file behaviour
#pragma warning disable CW1157 // Do not use System.AppDomain.
				var assemblyResolveBackingHandler = (ResolveEventHandler)typeof(AppDomain).GetField("_AssemblyResolve", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(AppDomain.CurrentDomain);
#pragma warning restore CW1157 // Do not use System.AppDomain.
				var invocationList = assemblyResolveBackingHandler?.GetInvocationList().OfType<ResolveEventHandler>().ToList();
				invocationList?.ForEach(h => AppDomain.CurrentDomain.AssemblyResolve -= h);

				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

				// Append the existing handlers after our handler
				invocationList?.ForEach(h => AppDomain.CurrentDomain.AssemblyResolve += h);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML NameSpace constants, Non-critical error, but need to output message somewhere")]
		public static IEnumerable<BindingRedirectInfo> GetBindingRedirects(string configFileName)
		{
			try
			{
				var result = new List<BindingRedirectInfo>();

				var xd = new XmlDocument();
				xd.Load(configFileName);

				var nsmgr = new XmlNamespaceManager(xd.NameTable);
				nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

				var bindingNodes = xd.DocumentElement.SelectNodes("//asm:dependentAssembly", nsmgr);
				foreach (XmlNode asmNode in bindingNodes)
				{
					var assemblyNode = asmNode.SelectSingleNode("asm:assemblyIdentity", nsmgr);
					if (assemblyNode != null)
					{
						var bindingInfo = new BindingRedirectInfo();
						bindingInfo.AssemblyName = assemblyNode.Attributes["name"].Value;
						bindingInfo.PublicKeyToken = assemblyNode.Attributes["publicKeyToken"].Value;

						var redirectNode = asmNode.SelectSingleNode("asm:bindingRedirect", nsmgr);
						if (redirectNode != null)
						{
							var oldVersions = redirectNode.Attributes["oldVersion"].Value.Split('-');

							if (Version.TryParse(oldVersions[0], out var oldVersionFrom))
							{
								bindingInfo.OldVersionFrom = oldVersionFrom;
							}
							else
							{
								bindingInfo.OldVersionFrom = new Version(0, 0, 0, 0);
							}

							if (Version.TryParse(oldVersions[oldVersions.Length > 1 ? 1 : 0], out var oldVersionTo))
							{
								bindingInfo.OldVersionTo = oldVersionTo;
							}
							else
							{
								bindingInfo.OldVersionTo = bindingInfo.OldVersionFrom;
							}

							bindingInfo.NewVersionString = redirectNode.Attributes["newVersion"].Value;

							result.Add(bindingInfo);
						}
					}
				}

				return result;
			}
			catch (IOException ex)
			{
				Console.WriteLine($"Cannot load config file for {configFileName}: {ex}");
				return Enumerable.Empty<BindingRedirectInfo>();
			}
		}

		readonly Dictionary<string, BindingRedirectInfo> bindingRedirectInfos;

		Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var bindingInfo = FindMatchingBindingRedirectInfo(args.Name);
			if (bindingInfo != null)
			{
				if (bindingInfo.ResolvedAssembly == null)
				{
					bindingInfo.ResolvedAssembly = ResolveAssembly(bindingInfo);
				}
				return bindingInfo.ResolvedAssembly;
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Assembly full name part constant, BindingRedirectInfo entry key")]
		BindingRedirectInfo FindMatchingBindingRedirectInfo(string assemblyName)
		{
			var nameParts = assemblyName.Split(',');
			var name = nameParts[0];
			var token = "";
			var versionString = "";
			for (int i = 1; i < nameParts.Length; i++)
			{
				var partElements = nameParts[i].Trim().Split('=');
				if (partElements.Length == 2)
				{
					var elementKey = partElements[0].Trim();
					if (elementKey.Equals("Version", StringComparison.OrdinalIgnoreCase))
					{
						versionString = partElements[1].Trim();
					}
					else if (elementKey.Equals("PublicKeyToken", StringComparison.OrdinalIgnoreCase))
					{
						token = partElements[1].Trim();
					}
				}
			}

			if (bindingRedirectInfos.TryGetValue($"{name}|{token}", out var bindingInfo))
			{
				if (!Version.TryParse(versionString, out var requestedVersion) ||
					(requestedVersion >= bindingInfo.OldVersionFrom && requestedVersion <= bindingInfo.OldVersionTo))
				{
					return bindingInfo;
				}
			}

			return null;
		}

		public Assembly ResolveAssembly(BindingRedirectInfo bindingInfo)
		{
			var fullName = $"{bindingInfo.AssemblyName}, Version={bindingInfo.NewVersionString}, Culture=neutral, PublicKeyToken={bindingInfo.PublicKeyToken}";
			return Assembly.Load(fullName);
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
		}
	}

	// Need to be class instead of structure to be able to update property ResolvedAssembly when it is stored in Dictionary.
	public class BindingRedirectInfo
	{
		public string AssemblyName { get; set; }
		public string PublicKeyToken { get; set; }
		public Version OldVersionFrom { get; set; }
		public Version OldVersionTo { get; set; }
		public string NewVersionString { get; set; }

		public Assembly ResolvedAssembly { get; set; }
	}
}
