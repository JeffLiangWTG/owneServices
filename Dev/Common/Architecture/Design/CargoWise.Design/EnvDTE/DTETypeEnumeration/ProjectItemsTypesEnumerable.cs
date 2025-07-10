using System;
using System.Collections;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// An enumerable across fake Type objects in a collection of EnvDTE.ProjectItems.
	/// </summary>

	public class ProjectItemsTypesEnumerable : IEnumerable
	{
		readonly IServiceProvider serviceProvider;

		public ProjectItemsTypesEnumerable(IServiceProvider serviceProvider, EnvDTE.ProjectItems items)
		{
			this.serviceProvider = serviceProvider;
			ProjectItems = items;
		}

		public EnvDTE.ProjectItems ProjectItems { get; private set; }

		public IEnumerator GetEnumerator()
		{
			foreach (EnvDTE.ProjectItem item in ProjectItems)
			{
				if (item.ProjectItems != null)
				{
					foreach (Type type in new ProjectItemsTypesEnumerable(serviceProvider, item.ProjectItems))
					{
						yield return type;
					}
				}
				if (item.FileCodeModel != null)
				{
					foreach (Type type in new VSCodeTypesEnumerable(serviceProvider, item.FileCodeModel.CodeElements))
					{
						yield return type;
					}
				}
			}
		}
	}
}
