using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if NETCOREAPP
using System.Runtime.Loader;
#endif
using System.Threading.Tasks;

namespace Enterprise.ReflectionTest.Utilities
{
	internal sealed class AssembliesContext
	{
#if NETCOREAPP
		sealed class CustomAssemblyLoadContext : AssemblyLoadContext
		{
			readonly string _assembliesPath;

			public CustomAssemblyLoadContext(string assembliesPath, bool isCollectible = true)
				: base(isCollectible)
			{
				_assembliesPath = assembliesPath;
			}

			protected override Assembly Load(AssemblyName assemblyName)
			{
				string assemblyPath = Path.Combine(_assembliesPath, $"{assemblyName.Name}.dll");
				if (File.Exists(assemblyPath))
				{
					return LoadFromAssemblyPath(assemblyPath);
				}

				return null;
			}
		}
#endif

		sealed class SimplePathAssemblyResolver : PathAssemblyResolver
		{
			public SimplePathAssemblyResolver(IEnumerable<string> assemblyPaths) : base(assemblyPaths)
			{
				//OK
			}

			/// <summary>
			/// if assembly is not resolved try resolve with stripped information
			/// </summary>
			public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName) =>
				base.Resolve(context, assemblyName) ?? base.Resolve(context, new AssemblyName(assemblyName.Name));
		}

		//does not need thread protection - customized nunit framework runs tests one by one only
#pragma warning disable CW1021 // Static Fields Are Thread Static Rule - nope, cannot risk having few instances on few threads
		static AssembliesContext st_instance;

		static AssembliesContext st_netcoreInstance;
#pragma warning restore CW1021

#if NETCOREAPP
		readonly CustomAssemblyLoadContext _ctx;
#else
		readonly MetadataLoadContext _ctx;
#endif
		readonly HashSet<string> _buildOutputAssemblyNames;
		readonly ConcurrentDictionary<string, AssemblyName> _assemblyNamesByName;

		AssembliesContext(string assembliesPath, IEnumerable<string> buildOutputAssemblyPaths, bool isNetCoreAssembly = false)
		{
#if NETCOREAPP
			_ctx = new CustomAssemblyLoadContext(assembliesPath);
#else
			_ctx = CreateMetadataLoadContext(assembliesPath, isNetCoreAssembly);
#endif
			_buildOutputAssemblyNames = buildOutputAssemblyPaths.Select(item => Path.GetFileNameWithoutExtension(item))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
			_assemblyNamesByName = new ConcurrentDictionary<string, AssemblyName>
			(
				buildOutputAssemblyPaths.AsParallel()
#if NETCOREAPP
					.Where(item => Path.GetExtension(item).Equals(".dll", StringComparison.OrdinalIgnoreCase))
#endif
					.Select
				(
					item => new KeyValuePair<string, AssemblyName>
					(
						Path.GetFileNameWithoutExtension(item),
						_ctx.LoadFromAssemblyPath($@"{assembliesPath}\{item}").GetName()
					)
				)
				.AsSequential(),
				StringComparer.OrdinalIgnoreCase
			);
		}

		/// <param name="assemblyName">simple name of the assembly</param>
		public Assembly GetAssembly(string assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException(nameof(assemblyName));
			}

			//MetadataLoadContext.LoadFromAssemblyName(string) always check on disk so it is slow even if assembly is already loaded
			//MetadataLoadContext.LoadFromAssemblyName(AssemblyName) is fast if assembly is already loaded
			//_assemblyNamesByName is to avoid calling LoadFromAssemblyName(string) multiple times, it is ok that it still may happen

			AssemblyName asmName = _assemblyNamesByName.GetOrAdd
			(
				assemblyName,
				key =>
				{
					try
					{
#if NETCOREAPP
						return _ctx.LoadFromAssemblyName(new AssemblyName(key)).GetName();
#else
						return _ctx.LoadFromAssemblyName(key).GetName();
#endif
					}
					catch (FileNotFoundException)
					{
						return null;
					}
				}
			);

			return asmName != null ? _ctx.LoadFromAssemblyName(asmName) : null;
		}

		public Assembly GetAssembly(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException(nameof(assemblyName));
			}

			Assembly assembly = _ctx.LoadFromAssemblyName(assemblyName);

			_assemblyNamesByName.TryAdd(assemblyName.Name, assemblyName);

			return assembly;
		}

		/// <summary>
		/// from runnable type
		/// </summary>
		public Type GetReflectionType(Type type) =>
			_ctx.LoadFromAssemblyName(type.Assembly.GetName()).GetType(type.FullName, true);

		public Type GetReflectionType(string assemblyQualifiedTypeName)
		{
			string[] a = assemblyQualifiedTypeName.Split(',');

			return GetAssembly(a[1].Trim())?.GetType(a[0].Trim());
		}

		public ParallelQuery<Assembly> GetBuildOutputAssembliesLive() =>
			_buildOutputAssemblyNames.AsParallel().Select(GetAssembly);

		public Dictionary<Assembly, HashSet<Assembly>> GetAllReferencesByAssembly(IEnumerable<Assembly> assemblies)
		{
			Dictionary<Assembly, HashSet<Assembly>> referencesByAssembly = assemblies.AsParallel().ToDictionary
			(
				item => item,
				item => new HashSet<Assembly>(item.GetReferencedAssemblies().Select(name => GetAssembly(name)))
			);
			HashSet<Assembly> referees = new HashSet<Assembly>(referencesByAssembly.Keys);

			while (referees.Count > 0)
			{
				//item which references are not in referees
				var leaves = referees.AsParallel()
					.Select
					(
						item =>
						(
							leaf: item,
							references: referencesByAssembly[item]
						)
					)
					.Where
					(
						item => !referees.Overlaps(item.references)
					)
					.ToArray();

				referees.ExceptWith(leaves.Select(item => item.leaf));

				Parallel.ForEach
				(
					referees,
					item =>
					{
						HashSet<Assembly> references = referencesByAssembly[item];

						foreach (var leaf in leaves.Where(leaf => references.Contains(leaf.leaf)))
						{
							references.UnionWith(leaf.references);
						}
					}
				);
			}

			return referencesByAssembly;
		}

#if NETFRAMEWORK
		static MetadataLoadContext CreateMetadataLoadContext(string assembliesPath, bool isNetCore)
		{
			string frameworkPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

			var assemblyFiles = Directory.EnumerateFiles(frameworkPath, "*.dll")
				.Concat(Directory.EnumerateFiles(assembliesPath, "*.dll"))
				.Concat(Directory.EnumerateFiles(Path.Combine(frameworkPath, "WPF"), "*.dll"));

			assemblyFiles = !isNetCore ? assemblyFiles.Concat(Directory.EnumerateFiles(assembliesPath, "*.exe")) : assemblyFiles;

			SimplePathAssemblyResolver resolver = new SimplePathAssemblyResolver
			(
				assemblyFiles
			);

			return new MetadataLoadContext(resolver);
		}
#endif
		/// <summary>
		/// because of lack of Assembly level SetUp call before use in every test
		/// not supposed to TearDown
		/// does not need thread protection - customized nunit framework runs tests one by one only
		/// </summary>
		public static void EnsureInstance(string assembliesPath, IEnumerable<string> buildOutputAssemblyPaths, bool isNetCoreInstance = false)
		{
			if (isNetCoreInstance)
			{
				st_netcoreInstance ??= new AssembliesContext(assembliesPath, buildOutputAssemblyPaths, isNetCoreAssembly: true);
			}
			else
			{
				st_instance ??= new AssembliesContext(assembliesPath, buildOutputAssemblyPaths);
			}
		}

		public static AssembliesContext Instance =>
			st_instance ?? throw new InvalidOperationException($"{nameof(EnsureInstance)}  has to be called first");

		public static AssembliesContext NetCoreInstance =>
			st_netcoreInstance ?? throw new InvalidOperationException($"{nameof(EnsureInstance)} has to be called first");
	}
}
