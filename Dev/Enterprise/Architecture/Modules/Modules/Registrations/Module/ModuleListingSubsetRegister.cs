using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Core.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Modules
{
	public static class ModuleListingSubsetRegister
	{
		public static IEnumerable<ModuleIdentifier> GetRegisteredModuleIdentifiers()
		{
			return RegisteredSubsets.SelectMany(s => s.ModuleIdentifiers);
		}

		public static IEnumerable<ModuleInfo> GetRegisteredModuleInfos()
		{
			return RegisteredSubsets.SelectMany(s => s.ModuleInfos);
		}

		public static IEnumerable<ControllerInfo> GetRegisteredControllerInfos()
		{
			return RegisteredSubsets.SelectMany(s => s.ControllerInfos);
		}

		public static void InitializeSecurityCheckpoints(IZSecurity security)
		{
			foreach (var subset in RegisteredSubsets)
			{
				subset.InitializeSecurityCheckpoints(security);
			}
		}

		public static void InitializeModuleTree(ModuleTreeCategories moduleTreeCategories, IZSecurity security)
		{
			foreach (var subset in RegisteredSubsets)
			{
				subset.InitializeModuleTree(moduleTreeCategories, security);
			}
		}

		static ImmutableArray<IModuleListingSubset> RegisteredSubsets
			=> registeredSubsets.IsDefault ? registeredSubsets = GetRegisteredModuleListingSubsets().ToImmutableArray() : registeredSubsets;

		[ThreadSafe]
		static ImmutableArray<IModuleListingSubset> registeredSubsets;

		static IEnumerable<IModuleListingSubset> GetRegisteredModuleListingSubsets()
		{
			var result = new ConcurrentHashSet<IModuleListingSubset>();
			Parallel.ForEach(RegisteredModuleListingSubsets, typeName =>
			{
				var type = Type.GetType(typeName, throwOnError: ThrowWhenSubsetTypeUnavailable);
				if (type is not null)
				{
					var subset = (IModuleListingSubset)Activator.CreateInstance(type);
					result.TryAdd(subset);
				}
			});

			return result;
		}

		static bool ThrowWhenSubsetTypeUnavailable =>
#if RELEASE
			true;
#elif DEBUG
			ThrowWhenSubsetTypeUnavailableOverridable.Value; // We permit missing types during testing since submodule binaries are not present while testing their ancestors

		public static readonly Overridable<bool> ThrowWhenSubsetTypeUnavailableOverridable = new(false);

		public static void ResetRegistrations()
		{
			registeredSubsets = default;
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Fully qualified class names")]
		static IEnumerable<string> RegisteredModuleListingSubsets
		{
			get
			{
#if DEBUG
				yield return "Enterprise.ZArchitecture.Modules.Testing.DummyModuleListingSubset, Enterprise.ZArchitecture.Modules.Testing";
#endif
				// Register implementations of IModuleListingSubset here:
				yield return "Enterprise.Customs.ZA.ModuleRegistration.ZAModuleListingSubset, Enterprise.Customs.ZA.ModuleRegistration";
				yield break;
			}
		}
	}
}
