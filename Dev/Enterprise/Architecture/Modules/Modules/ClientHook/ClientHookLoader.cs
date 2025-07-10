using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.Integration;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Modules
{
	/// <summary>
	/// Loads Client specific behaviour from the respective ZClientXXX assembly.
	/// </summary>
	public sealed partial class ClientHookLoader : IClientHookLoader, ITableSchemaSource
	{
		#region Construction

		ClientHookLoader() { }

		public static ClientHookLoader Instance
		{
			get { return instance ?? (instance = new ClientHookLoader()); }
		}

		[SuppressThreadStaticFieldMessage]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Suppress the warning.")]
		static ClientHookLoader instance;

		#endregion //Construction

		#region Client Hook

		public Clients Client
		{
			get { return ClientHook?.Client ?? Clients.None; }
		}

		public event EventHandler ClientHookChanged;

		void OnClientHookChanged()
		{
			if (ClientHookChanged != null)
			{
				ClientHookChanged(this, EventArgs.Empty);
			}
		}

		IClientHook IClientHookLoader.ClientHook
		{
			get
			{
				if (!clientHookLoaded)
				{
					lock (clientHookMutex)
					{
						if (!clientHookLoaded)
						{
							clientHook = LoadClientHook();
							Thread.MemoryBarrier();
							clientHookLoaded = true;
							if (clientHook != null && !clientHook.IsInitialised)
							{
								clientHook.Initialise();
							}
						}
					}
				}

				return clientHook;
			}
		}

		public ClientHook ClientHook
		{
			get
			{
				return ((IClientHookLoader)this).ClientHook as ClientHook;
			}
		}

		readonly object clientHookMutex = new object();
		IClientHook clientHook;
		volatile bool clientHookLoaded;

		ClientHook LoadClientHook()
		{
			if (ClientAssembly != null)
			{
				return GetClientHookFromAssembly(ClientAssembly);
			}

			return null;
		}

		public ClientHook GetClientHookFromAssembly(Assembly assembly)
		{
			var clientOverrideType = assembly.GetType("Enterprise.Client.ClientOverride", true, false)
				?? throw new InvalidOperationException("Sorry, can't find 'Enterprise.Client.ClientOverride' in client DLL: " + assembly.FullName);

			var clientOverrideInstanceInfo = clientOverrideType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
				?? throw new InvalidOperationException("Sorry, can't find static 'Instance' property in 'Enterprise.Client.ClientOverride' in client DLL: " + assembly.FullName);

			var result = clientOverrideInstanceInfo.GetValue(null, null) as ClientHook
				?? throw new InvalidOperationException("ClientOverride class in Client specific dlls must implement ClientHook interface");

			return result;
		}

		#endregion //Client Hook

		#region Client Assembly

		public Assembly ClientAssembly
		{
			get
			{
				if (!clientAssemblyLoaded)
				{
#if DEBUG
					if (VisualStudioDetector.IsVisualStudio)
					{
						clientAssemblyLoaded = true;
						return null;
					}
#endif
					var retriever = ObjectFactory.Get<Enterprise.Integration.ZArchitecture.IEnterpriseCodeRetriever>();
					clientAssembly = FindAssembly(retriever.EnterpriseCodeFromRegistry);

					clientAssemblyLoaded = true;
				}

				return clientAssembly;
			}
		}

		Assembly clientAssembly;
		bool clientAssemblyLoaded;

		#endregion //Client Assembly

		#region ITableSchemaSource Members

		ITableSchema[] ITableSchemaSource.TableSchemas
		{
			get { return (ClientHook == null) ? Array.Empty<ITableSchema>() : ClientHook.TableSchemas; }
		}

		ITableSchema ITableSchemaSource.GetTableSchema(string tableName)
		{
			return (ClientHook == null) ? null : ClientHook.GetTableSchema(tableName);
		}

		#endregion //ITableSchemaSource Members

		#region Helper methods

		Assembly FindAssembly(string clientCode)
		{
			var fileName = "ZClient" + clientCode + ".dll"; // Client Assembly file format
			var fullPath = Path.Combine(ApplicationStartupDirectory, fileName);
			if (File.Exists(fullPath))
			{
				return GetAssemblyFromFileName(fullPath);
			}

			return null;
		}

		public Assembly GetAssemblyFromFileName(string fileName)
		{
			return Assembly.Load(AssemblyName.GetAssemblyName(fileName));
		}

		internal string ApplicationStartupDirectory
		{
			get
			{
				return AssemblyLoader.GetBinPath();
			}
		}

		public IExtensionObjects GetDbSchemaExtensionObjects(Assembly assembly)
		{
			return GetClientHookFromAssembly(assembly).DbSchemaExtensionObjects;
		}

		#endregion //Helper methods

		#region UI Hack
#if DEBUG
		static class VisualStudioDetector
		{
			public static bool IsVisualStudio
			{
				get
				{
					try
					{
						return isVisualStudio ?? (bool)(isVisualStudio = (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv"));
					}
					catch (System.ComponentModel.Win32Exception)
					{
						return false;
					}
					catch (InvalidOperationException)
					{
						return false;
					}
				}
				set { isVisualStudio = value; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Suppress the warning.")]
			static bool? isVisualStudio;
		}
#endif

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.ZArchitecture.Modules
{
	public sealed partial class ClientHookLoader
	{
		void UnloadClientHookIfLoaded()
		{
			if (clientHook != null)
			{
				if (clientHook.IsInitialised)
				{
					clientHook.Uninitialise();
				}

				clientHook = null;
			}

			clientHookLoaded = false;
		}

		/// <summary>
		/// WARNING: This should only be used for testing purposes within an #if DEBUG. You MUST set the client hook back
		/// the way it was when you have completed your test in a finally block.
		/// </summary>
		public IDisposable OverrideClientHookForTest(IClientHook newClientHook, bool loggedIn = false, bool overwriteToClientHook = false)
		{
			PushedClientHooks.Push(ClientHook);

			OverrideClientHook(newClientHook, loggedIn, overwriteToClientHook);

			return new DisposableAction(() => OverrideClientHook(PushedClientHooks.Pop(), loggedIn));
		}

		class ClientHookForTest(Clients client) : ClientHook
		{
			public override Clients Client { get; } = client;
			public override string ClientDisplayName { get; } = nameof(client);
		}

		void OverrideClientHook(IClientHook newClientHook, bool loggedIn, bool overwriteToClientHook = false)
		{
			UnloadClientHookIfLoaded();
			clientHook = newClientHook;
			clientHookLoaded = true;

			if (clientHook != null && overwriteToClientHook)
			{
				Enum.TryParse(newClientHook.UniqueId, out Clients client);
				clientHook = new ClientHookForTest(client);
				clientHookLoaded = clientHook != null;
			}

			clientHook?.Initialise(loggedIn);

			OnClientHookChanged();
		}

		Stack<IClientHook> PushedClientHooks
		{
			get { return pushedClientHooks ?? (pushedClientHooks = new Stack<IClientHook>()); }
		}
		Stack<IClientHook> pushedClientHooks;

		/// <summary>
		/// WARNING: This should only be used for testing purposes within an #if DEBUG. You MUST set the client assembly back
		/// the way it was when you have completed your test in a finally block.
		/// </summary>
		public IDisposable OverrideClientAssemblyForTest(Clients clientId)
		{
			return OverrideClientAssemblyFromCodeForTest(clientId.ToString());
		}

		/// <summary>
		/// WARNING: This should only be used for testing purposes within an #if DEBUG. You MUST set the client assembly back
		/// the way it was when you have completed your test in a finally block.
		/// </summary>
		public IDisposable OverrideClientAssemblyFromCodeForTest(string clientCode)
		{
			return OverrideClientAssemblyForTest(FindAssemblyForTest(clientCode));
		}

		/// <summary>
		/// If the assembly is any client assembly then install the appropriate main client override assembly, else does nothing and returns null.
		/// </summary>
		public IDisposable OverrideClientAssemblyForTestIfNeeded(Assembly newAssembly)
		{
			if (newAssembly != null)
			{
				var assemblyFullName = newAssembly.FullName;
				var assemblyInfo = GetClientOverrideAssemblyInfo(assemblyFullName);
				if (assemblyInfo.AssemblyType != ClientAssemblyType.NotClient)
				{
					Assembly parentAssembly = newAssembly;
					if (assemblyInfo.AssemblyType != ClientAssemblyType.Parent)
					{
						string entCode = assemblyFullName.Substring(assemblyInfo.EnterpriseCodeIndex, 3);
						parentAssembly = FindAssemblyForTest(entCode);
					}

					return OverrideClientAssemblyForTest(parentAssembly);
				}
			}

			return null;
		}

		/// <summary>
		/// WARNING: This should only be used for testing purposes within an #if DEBUG. You MUST set the client assembly back
		/// the way it was when you have completed your test in a finally block.
		/// </summary>
		public IDisposable OverrideClientAssemblyForTest(Assembly newAssembly)
		{
			PushedAssemblies.Push(ClientAssembly);

			try
			{
				OverrideClientAssembly(newAssembly);
				if (Instance.ClientHook != null && !Instance.ClientHook.IsInitialised)
				{
					Instance.ClientHook.IsCorrectCompanyForOverrides_ForTest = true;
					Instance.ClientHook.Initialise(true);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				OverrideClientAssembly(PushedAssemblies.Count > 0 ? PushedAssemblies.Pop() : null);
				throw;
			}

			return new DisposableAction(() => OverrideClientAssembly(PushedAssemblies.Count > 0 ? PushedAssemblies.Pop() : null));
		}

		void OverrideClientAssembly(Assembly newAssembly)
		{
			UnloadClientHookIfLoaded();

			clientAssembly = newAssembly;
			clientAssemblyLoaded = true;

			OnClientHookChanged();
		}

		public void RemoveOverrideClientAssembliesForTest()
		{
			clientAssembly = null;
			PushedAssemblies.Clear();
		}

		Stack<Assembly> PushedAssemblies
		{
			get { return pushedAssemblies ?? (pushedAssemblies = new Stack<Assembly>()); }
		}
		Stack<Assembly> pushedAssemblies;

		internal Assembly FindAssemblyForTest(Clients clientId)
		{
			return FindAssemblyForTest(clientId.ToString());
		}

		public Assembly FindAssemblyForTest(string clientCode)
		{
			var filePath = GetFilesFromBin($"ZClient{clientCode}.dll").FirstOrDefault();
			if (filePath != null)
			{
				return GetAssemblyFromFileName(filePath);
			}
			return null;
		}

		/// <summary>
		/// Returns just the one, parent assembly for each client code
		/// </summary>
		/// <returns></returns>
		public string[] GetAllClientSpecificAssemblyFileNames()
			=> GetFilesFromBin("ZClient???.dll");

		public static string[] GetFilesFromBin(string searchPattern)
			=> GetFilesFromBin(searchPattern, takeNetCoreFilesFirst: Environment.GetEnvironmentVariable("WINZOR_USE_NETCORE_LIBS") == "true");

		public static string[] GetFilesFromBin(string searchPattern, bool takeNetCoreFilesFirst)
		{
			if (string.IsNullOrWhiteSpace(searchPattern))
			{
				throw new ArgumentNullException(nameof(searchPattern));
			}

			var rootBinPath = Path.GetDirectoryName(typeof(ClientHookLoader).Assembly.Location);
			if (string.IsNullOrEmpty(rootBinPath))
			{
				throw new DirectoryNotFoundException("Could not determine root bin path.");
			}

			// Adjust rootBinPath if it's pointing to the NETCore subfolder
			if (rootBinPath.EndsWith(CommonAssemblyInfo.CWNetCoreSubfolder, StringComparison.OrdinalIgnoreCase))
			{
				rootBinPath = Directory.GetParent(rootBinPath)?.FullName
					?? throw new DirectoryNotFoundException("Could not find parent directory for NETCore subfolder.");
			}

			var netCoreDirectory = Path.Combine(rootBinPath, CommonAssemblyInfo.CWNetCoreSubfolder);

			var rootFiles = Directory.GetFiles(rootBinPath, searchPattern);
			var netCoreFiles = Directory.Exists(netCoreDirectory)
				? Directory.GetFiles(netCoreDirectory, searchPattern)
				: Array.Empty<string>();

			var files = takeNetCoreFilesFirst
				? netCoreFiles.Concat(rootFiles)
				: rootFiles.Concat(netCoreFiles);

			return files
				.GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
				.Select(g => g.First())
				.OrderBy(Path.GetFileName)
				.ToArray();
		}

		/// <summary>
		/// Get 3 character client code in uppercase, or null if not a client assembly
		/// </summary>
		public string GetClientCode(string assemblyName)
		{
			var assemblyInfo = GetClientOverrideAssemblyInfo(assemblyName);
			return assemblyInfo.AssemblyType != ClientAssemblyType.NotClient
				? assemblyName.Substring(assemblyInfo.EnterpriseCodeIndex, 3)
				: null;
		}

		public bool IsClientOverrideAssembly(Assembly assembly)
		{
			return IsClientOverrideAssembly(assembly.FullName);
		}

		public bool IsClientOverrideAssembly(string assemblyFullName)
		{
			var assemblyInfo = GetClientOverrideAssemblyInfo(assemblyFullName);
			return assemblyInfo.AssemblyType == ClientAssemblyType.Parent;
		}

		public bool IsAnyClientOverrideAssembly(string assemblyFileOrFullName)
		{
			return GetClientOverrideAssemblyInfo(assemblyFileOrFullName).AssemblyType != ClientAssemblyType.NotClient;
		}

		public bool IsAnyNonWebClientOverrideAssembly(string assemblyFileOrFullName, bool includingTestAssemblies)
		{
			var assemblyInfo = GetClientOverrideAssemblyInfo(assemblyFileOrFullName);
			return assemblyInfo.AssemblyType == ClientAssemblyType.Child
				|| assemblyInfo.AssemblyType == ClientAssemblyType.Parent
				|| (includingTestAssemblies &&
					(assemblyInfo.AssemblyType == ClientAssemblyType.ChildTest || assemblyInfo.AssemblyType == ClientAssemblyType.ParentTest));
		}

		enum ClientAssemblyType
		{
			NotClient,
			Parent, // ZClient{ent}[.dll]
			ParentTest, // ZClient{ent}.Test[.dll]
			Child, // ZClient{ent}.{area}[.dll]
			ChildTest, // ZClient{ent}.{area}.Test[.dll]
			Web, // ZClientWeb{ent}[.dll]
			WebTest // ZClientWeb{ent}.Test[.dll]
		}

		struct ClientAssemblyInfo
		{
			public ClientAssemblyInfo(ClientAssemblyType clientAssemblyType, int entCodeIndex = -1)
			{
				AssemblyType = clientAssemblyType;
				EnterpriseCodeIndex = entCodeIndex;
			}

			public ClientAssemblyType AssemblyType { get; private set; }
			public int EnterpriseCodeIndex { get; private set; }
		}

		/// <summary>
		/// Check name matches a client assembly name format. Name can be assembly full name or file name, like:
		///		ZClientUPE, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350
		///		ZClientUPE
		///		ZClientUPE.DLL
		/// </summary>
		ClientAssemblyInfo GetClientOverrideAssemblyInfo(string assemblyFileOrFullName)
		{
			if (!assemblyFileOrFullName.StartsWith("ZClient", StringComparison.OrdinalIgnoreCase))
			{
				return new ClientAssemblyInfo(ClientAssemblyType.NotClient);
			}

			var name = assemblyFileOrFullName;
			int nameLength = assemblyFileOrFullName.Length;
			int commaIndex = name.IndexOf(',');
			if (commaIndex != -1)
			{
				nameLength = commaIndex;
			}
			else if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			{
				nameLength = name.Length - 4;
			}

			const int NonWebPrefixLength = 7;
			const int WebPrefixLength = 10;
			const int EntCodeLength = 3;

			int expectedPrefixLength = NonWebPrefixLength;
			if (name.StartsWith("ZClientWeb", StringComparison.OrdinalIgnoreCase))
			{
				expectedPrefixLength = WebPrefixLength;
			}

			if (nameLength < expectedPrefixLength + EntCodeLength
				|| !char.IsUpper(name[expectedPrefixLength])
				|| !IsUpperLetterOrDigit(name[expectedPrefixLength + 1])
				|| !IsUpperLetterOrDigit(name[expectedPrefixLength + 2]))
			{
				return new ClientAssemblyInfo(ClientAssemblyType.NotClient);
			}

			// Must be a '.' after the ent code
			if (nameLength > expectedPrefixLength + EntCodeLength
				&& name[expectedPrefixLength + EntCodeLength] != '.')
			{
				return new ClientAssemblyInfo(ClientAssemblyType.NotClient);
			}

			int testIndex = name.IndexOf(".Test", 0, nameLength, StringComparison.OrdinalIgnoreCase);
			bool isTest = testIndex != -1;
			bool hasChildPart = isTest
				? testIndex > expectedPrefixLength + EntCodeLength
				: nameLength > expectedPrefixLength + EntCodeLength;
			ClientAssemblyType assemblyType;
			if (expectedPrefixLength == NonWebPrefixLength)
			{
				if (nameLength == NonWebPrefixLength + EntCodeLength)
				{
					assemblyType = ClientAssemblyType.Parent;
				}
				else if (hasChildPart)
				{
					assemblyType = isTest ? ClientAssemblyType.ChildTest : ClientAssemblyType.Child;
				}
				else
				{
					assemblyType = ClientAssemblyType.ParentTest;
				}
			}
			else // web
			{
				assemblyType = isTest ? ClientAssemblyType.WebTest : ClientAssemblyType.Web;
			}

			return new ClientAssemblyInfo(assemblyType, expectedPrefixLength);
		}

		static bool IsUpperLetterOrDigit(char ch) => char.IsDigit(ch) || char.IsUpper(ch);
	}
}

#endif
#endregion //Test
