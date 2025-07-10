using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Common.Design;
using CargoWise.Common.Testing;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// An enumerator across all types available from a given project, and all project/assembly references
	/// from that project. Not all types in a project are normal RuntimeType objects, as they are not yet
	/// compiled. For these types, fake types are created.
	/// </summary>

	public class DesignTimeTypeEnumerable : IEnumerable<Type>
	{
		public DesignTimeTypeEnumerable(IServiceProvider serviceProvider, EnvDTE.Project project)
		{
			this.serviceProvider = serviceProvider;
			Project = project;
		}

		/// <summary>
		/// Get the project the types come from.
		/// </summary>
		public EnvDTE.Project Project { get; set; }

		#region GetCachedInstance

		public static IEnumerable<Type> GetCachedEnumerableFromCurrentProject(IServiceProvider serviceProvider)
		{
			EnvDTE.DTE dte = (EnvDTE.DTE)serviceProvider.GetService(typeof(EnvDTE.DTE));
			EnvDTE.Project[] selectedProjects = EnvDTEUtil.GetSelectedProjects(dte);

			foreach (EnvDTE.Project project in selectedProjects)
			{
				foreach (Type type in DesignTimeTypeEnumerable.GetCachedEnumerable(serviceProvider, project))
				{
					yield return type;
				}
			}
		}

		public static IEnumerable<Type> GetCachedEnumerable(IServiceProvider serviceProvider, EnvDTE.Project project)
		{
			CachedInstances.TryGetValue(project, out var result);
			if (result == null)
			{
				result = new CachedEnumerableWrapper<Type>(new DesignTimeTypeEnumerable(serviceProvider, project));
				CachedDteEventManagers.TryGetValue(project.DTE, out var manager);
				if (manager == null)
				{
					manager = new DteEventManagerForCacheInvalidation(project.DTE);
					CachedDteEventManagers[project.DTE] = manager;
					manager.Enabled = true;
				}
				manager.Enabled = true;
				CachedInstances[project] = result;
			}
			return result;
		}

		static void InvalidateCache()
		{
			lock (selectedChangeMutex)
			{
				foreach (KeyValuePair<EnvDTE.DTE, DteEventManagerForCacheInvalidation> entry in CachedDteEventManagers)
				{
					entry.Value.Enabled = false;
				}
				CachedDteEventManagers.Clear();
				CachedInstances.Clear();
			}
		}

		class DteEventManagerForCacheInvalidation
		{
			readonly EnvDTE.DTE dte;
			EnvDTE.TextEditorEvents textEditorEvents;
			EnvDTE.SelectionEvents selectionEvents;

			public DteEventManagerForCacheInvalidation(EnvDTE.DTE dte)
			{ this.dte = dte; }

			public bool Enabled
			{
				get { return enabled; }
				set
				{
					if (value != Enabled)
					{
						if (value)
						{
							textEditorEvents = dte.Events.get_TextEditorEvents(null);
							selectionEvents = dte.Events.SelectionEvents;
							textEditorEvents.LineChanged += new EnvDTE._dispTextEditorEvents_LineChangedEventHandler(OnTextEditorEvents_LineChanged);
							selectionEvents.OnChange += new EnvDTE._dispSelectionEvents_OnChangeEventHandler(OnSelectionEvents_OnChange);
						}
						else
						{
							textEditorEvents.LineChanged -= new EnvDTE._dispTextEditorEvents_LineChangedEventHandler(OnTextEditorEvents_LineChanged);
							selectionEvents.OnChange -= new EnvDTE._dispSelectionEvents_OnChangeEventHandler(OnSelectionEvents_OnChange);
						}
						enabled = value;
					}
				}
			}
			bool enabled;

			void OnTextEditorEvents_LineChanged(EnvDTE.TextPoint startPoint, EnvDTE.TextPoint endPoint, int hint)
			{ DesignTimeTypeEnumerable.InvalidateCache(); }

			void OnSelectionEvents_OnChange()
			{ DesignTimeTypeEnumerable.InvalidateCache(); }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator<Type> GetEnumerator()
		{
			if (serviceProvider == null)
			{
				throw new InvalidOperationException(nameof(IServiceProvider) + " not available at this time");
			}
			if (TypeResolutionService == null)
			{
				throw new InvalidOperationException(nameof(ITypeResolutionService) + " not available at this time");
			}

			foreach (Type type in new ProjectItemsTypesEnumerable(serviceProvider, Project.ProjectItems))
			{
				yield return type;
			}
			for (int i = 0; i < VSProject.References.Count; i++)
			{
				VSLangProj.Reference reference = VSProject.References.Item(i + 1);
				foreach (Type type in GetTypesFromReference(reference))
				{
					yield return type;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (Type type in this)
			{
				yield return type;
			}
		}

		#endregion

		#region Implementation

		[SuppressThreadStaticFieldMessage]
		static readonly object selectedChangeMutex = new object();
		readonly IServiceProvider serviceProvider;

		static Dictionary<EnvDTE.Project, IEnumerable<Type>> CachedInstances
		{ get { return cachedInstances ?? (cachedInstances = new Dictionary<EnvDTE.Project, IEnumerable<Type>>()); } }
		[ThreadStatic]
		static Dictionary<EnvDTE.Project, IEnumerable<Type>> cachedInstances;

		static Dictionary<EnvDTE.DTE, DteEventManagerForCacheInvalidation> CachedDteEventManagers
		{ get { return cachedDteEventManagers ?? (cachedDteEventManagers = new Dictionary<EnvDTE.DTE, DteEventManagerForCacheInvalidation>()); } }
		[ThreadStatic]
		static Dictionary<EnvDTE.DTE, DteEventManagerForCacheInvalidation> cachedDteEventManagers;

		VSLangProj.VSProject VSProject
		{ get { return (VSLangProj.VSProject)Project.Object; } }

		IEnumerable<Type> GetTypesFromReference(VSLangProj.Reference reference)
		{
			if (reference.SourceProject != null)
			{
				foreach (Type type in new ProjectItemsTypesEnumerable(serviceProvider, reference.SourceProject.ProjectItems))
				{
					yield return type;
				}
			}
			else
			{
				Assembly assembly = EnvDTEUtil.TryLoadAssemblyFromReference(TypeResolutionService, reference);
				if (assembly != null)
				{
					foreach (Type type in new TypeEnumerable(false, true, assembly))
					{
						yield return type;
					}
				}
			}
		}

		ITypeResolutionService TypeResolutionService
		{ get { return serviceProvider == null ? null : TypeResolutionServiceLocator.Get(serviceProvider); } }
		#endregion
	}
}
