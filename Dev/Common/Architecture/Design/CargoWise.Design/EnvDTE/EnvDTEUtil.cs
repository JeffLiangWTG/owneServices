using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Common.Collections;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// Utility methods for the objects in the EnvDTE namespace.
	/// </summary>

	public static class EnvDTEUtil
	{
		/// <summary>
		/// Get the currently selected projects from the EnvDTE.DTE object. The last known selected object
		/// is returned if selected projects can't be found.
		/// </summary>
		public static EnvDTE.Project[] GetSelectedProjects(EnvDTE.DTE dte)
		{
			if (dteToLastKnownSelectedProject == null)
			{
				dteToLastKnownSelectedProject = new WeakReferencedKeyDictionary<EnvDTE.DTE, EnvDTE.Project>();
			}

			List<EnvDTE.Project> result = new List<EnvDTE.Project>();
			if (dte != null)
			{
				foreach (EnvDTE.Project project in (Array)dte.ActiveSolutionProjects)
				{
					result.Add(project);
					dteToLastKnownSelectedProject[dte] = project;
				}
			}
			if (result.Count == 0 && dteToLastKnownSelectedProject[dte] is EnvDTE.Project lastKnown)
			{
				result.Add(lastKnown);
			}
			return result.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[ThreadStatic]
		static WeakReferencedKeyDictionary<EnvDTE.DTE, EnvDTE.Project> dteToLastKnownSelectedProject = new WeakReferencedKeyDictionary<EnvDTE.DTE, EnvDTE.Project>();

		internal static Assembly TryLoadAssemblyFromReference(ITypeResolutionService typeResolutionService, VSLangProj.Reference reference)
		{
			string referenceName = reference.Name;
			Assembly result = null;
			try
			{
				using (new TypeResolver(typeResolutionService))
				{
					result = typeResolutionService.GetAssembly(new AssemblyName(referenceName));
					if (result != null)
					{
						result.GetExportedTypes();
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			return result;
		}

		sealed class TypeResolver : IDisposable
		{
			public TypeResolver(ITypeResolutionService typeResolutionService)
			{
				this.typeResolutionService = typeResolutionService;
				AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			}

			public void Dispose()
			{ AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve); }

			Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
			{
				Assembly result = null;
				if (!inCurrentDomain_AssemblyResolve)
				{
					inCurrentDomain_AssemblyResolve = true;
					try
					{
						result = typeResolutionService.GetAssembly(new AssemblyName(args.Name));
					}
					finally
					{
						inCurrentDomain_AssemblyResolve = false;
					}
				}
				return result;
			}
			bool inCurrentDomain_AssemblyResolve;
			readonly ITypeResolutionService typeResolutionService;
		}

		public static Assembly[] GetReferencedAssemblies(ITypeResolutionService typeResolutionService, params EnvDTE.Project[] projects)
		{
			List<Assembly> result = new List<Assembly>();
			foreach (EnvDTE.Project project in projects)
			{
				if (project.Object is VSLangProj.VSProject vsproject)
				{
					for (int i = 0; i < vsproject.References.Count; i++)
					{
						VSLangProj.Reference reference = vsproject.References.Item(i + 1);
						if (reference.SourceProject == null)
						{
							Assembly ass = TryLoadAssemblyFromReference(typeResolutionService, reference);
							if (ass != null)
							{
								result.Add(ass);
							}
						}
					}
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// This is a poorman's way to get the namespaces specified by using statements in the given project
		/// item. As well as using statements at the top of the file, it also includes the first defined
		/// namespace in the file.
		/// </summary>
		public static string[] GetUsingNamespacesFromProjectItem(EnvDTE.ProjectItem item)
		{ return GetUsingNamespacesFromCodeElements(item.FileCodeModel.CodeElements); }

		static string[] GetUsingNamespacesFromCodeElements(EnvDTE.CodeElements elements)
		{
			List<string> result = new List<string>();
			foreach (EnvDTE.CodeElement element in elements)
			{
				if (element is EnvDTE80.CodeImport import)
				{
					if (string.IsNullOrEmpty(import.Alias))
					{
						result.Add(import.Namespace);
					}
				}

				if (element is EnvDTE.CodeNamespace ns)
				{
					result.Add(ns.FullName);
					GetUsingNamespacesFromCodeElements(ns.Children);
					break;
				}
			}
			return result.ToArray();
		}

		public static IEnumerable<EnvDTE.CodeType> GetAllCodeTypes(EnvDTE.CodeElements codeElements)
		{
			foreach (EnvDTE.CodeElement element in codeElements)
			{
				if (element is EnvDTE.CodeNamespace ns)
				{
					foreach (EnvDTE.CodeType next in GetAllCodeTypes(ns.Members))
					{
						yield return next;
					}
				}
				else if (element is EnvDTE.CodeType type)
				{
					yield return type;
					foreach (EnvDTE.CodeType next in GetAllCodeTypes(type.Members))
					{
						yield return next;
					}
				}
			}
		}

		public static bool IsTypeExistsInSolution(IServiceProvider serviceProvider, string typeName)
		{
			if (serviceProvider != null)
			{
				EnvDTE.DTE dte = (EnvDTE.DTE)serviceProvider.GetService(typeof(EnvDTE.DTE));
				if (dte != null)
				{
					foreach (EnvDTE.Project project in dte.Solution.Projects)
					{
						try
						{
							if (project.CodeModel != null && project.CodeModel.CodeTypeFromFullName(typeName) != null)
							{
								return true;
							}
						}
						catch (COMException)
						{
						}
					}
				}
			}
			return false;
		}

		#region CodeTypeFromFullName

		/// <summary>
		/// Find an EnvDTE.CodeType from a project given it's name. The return value's
		/// ProjectItem will return a valid value
		/// (unlike EnvDTE.Project.CodeModel.CodeTypeFromFullName()).
		/// </summary>
		public static EnvDTE.CodeType CodeTypeFromFullName(EnvDTE.Project project, string typeName)
		{
			var result = CodeTypeFromFullName(project.DTE.Documents, typeName) ?? CodeTypeFromFullName(project.ProjectItems, typeName);			
			if (result == null)
			{
				VSLangProj.VSProject vsproject = (VSLangProj.VSProject)project.Object;
				result = CodeTypeFromFullName(vsproject.References, typeName);
			}
			return result;
		}

		static EnvDTE.CodeType CodeTypeFromFullName(VSLangProj.References references, string typeName)
		{
			foreach (VSLangProj.Reference reference in references)
			{
				if (reference.SourceProject != null)
				{
					EnvDTE.CodeType type = CodeTypeFromFullName(reference.SourceProject, typeName);
					if (type != null)
					{
						return type;
					}
				}
			}
			return null;
		}

		static EnvDTE.CodeType CodeTypeFromFullName(EnvDTE.Documents documents, string typeName)
		{
			foreach (EnvDTE.Document document in documents)
			{
				EnvDTE.ProjectItem projectItem = null;
				try
				{
					projectItem = document.ProjectItem;
				}
				catch (ArgumentException)
				{
				}
				if (projectItem != null)
				{
					EnvDTE.CodeType type = CodeTypeFromFullName(document.ProjectItem, typeName);
					if (type != null)
					{
						return type;
					}
				}
			}
			return null;
		}

		static EnvDTE.CodeType CodeTypeFromFullName(EnvDTE.ProjectItems projectItems, string typeName)
		{
			foreach (EnvDTE.ProjectItem projectItem in projectItems)
			{
				EnvDTE.CodeType type = CodeTypeFromFullName(projectItem, typeName);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		static EnvDTE.CodeType CodeTypeFromFullName(EnvDTE.ProjectItem projectItem, string typeName)
		{
			if (projectItem.FileCodeModel != null)
			{
				foreach (EnvDTE.CodeType next in EnvDTEUtil.GetAllCodeTypes(projectItem.FileCodeModel.CodeElements))
				{
					if (next.FullName == typeName)
					{
						return next;
					}
				}
			}
			if (projectItem.ProjectItems != null)
			{
				EnvDTE.CodeType type = CodeTypeFromFullName(projectItem.ProjectItems, typeName);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		#endregion

		#region WriteTextToProjectItem

		public static void WriteTextToProjectItem(IServiceProvider serviceProvider, string fileName, string content)
		{
			EnvDTE.DTE dte = (serviceProvider == null) ? null : (EnvDTE.DTE)serviceProvider.GetService(typeof(EnvDTE.DTE));
			WriteTextToProjectItem(dte, fileName, content);
		}

		public static void WriteTextToProjectItem(EnvDTE.DTE dte, string fileName, string content)
		{
			EnvDTE.ProjectItem projectItem = dte?.Solution.FindProjectItem(fileName);
			if (projectItem != null && projectItem.Document != null)
			{
				projectItem.Document.Close(EnvDTE.vsSaveChanges.vsSaveChangesNo);
			}
			if (projectItem == null ||
				(File.Exists(fileName) && (File.GetAttributes(fileName) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly) ||
				projectItem.DTE.SourceControl.CheckOutItem(fileName))
			{
				File.WriteAllText(fileName, content);
				if (projectItem != null)
				{
					if (projectItem.Object is VSLangProj.VSProjectItem vsProjectItem)
					{
						vsProjectItem.RunCustomTool();
					}
				}
			}

			// Other things tried:
			// projectItem.IsDirty = true; - found to not be needed, but threw a 'project item is not open' exception, even when it is open
			// projectItem.Saved = false; - found to not be needed, but threw a 'cannot set Saved on a ProjectItem'
			// projectItem.ContainingProject.IsDirty = true; - found to not be needed
			// projectItem.Document.Selection.Text = content; - causes a memory leak!
			// projectItem.Document.StartPoint.CreateEditPoint().ReplaceText(projectItem.Document.EndPoint, content, etc); - causes a memory leak!
		}

		#endregion
	}
}
