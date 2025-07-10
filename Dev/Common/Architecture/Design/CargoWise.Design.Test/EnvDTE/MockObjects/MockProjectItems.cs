using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockProjectItems : List<EnvDTE.ProjectItem>, EnvDTE.ProjectItems
	{
		public MockProjectItems(MockProject containingProject)
		{ this.containingProject = containingProject; }

		public MockProjectItem AddNew(string fileName)
		{
			MockProjectItem result = new MockProjectItem(containingProject);
			result.FileNames.Add(fileName);
			Add(result);
			return result;
		}

		#region ProjectItems Members

		public EnvDTE.Project ContainingProject
		{ get { return containingProject; } }

		public EnvDTE.ProjectItem Item(object index)
		{ return this[(int)index - 1]; }

		System.Collections.IEnumerator EnvDTE.ProjectItems.GetEnumerator()
		{ return GetEnumerator(); }

		#region Unsupported Members

		string EnvDTE.ProjectItems.Kind
		{ get { throw new NotSupportedException(); } }

		EnvDTE.DTE EnvDTE.ProjectItems.DTE
		{ get { throw new NotSupportedException(); } }

		EnvDTE.ProjectItem EnvDTE.ProjectItems.AddFromFileCopy(string filePath)
		{ throw new NotSupportedException(); }

		EnvDTE.ProjectItem EnvDTE.ProjectItems.AddFromFile(string fileName)
		{ throw new NotSupportedException(); }

		EnvDTE.ProjectItem EnvDTE.ProjectItems.AddFolder(string name, string kind)
		{ throw new NotSupportedException(); }

		EnvDTE.ProjectItem EnvDTE.ProjectItems.AddFromDirectory(string directory)
		{ throw new NotSupportedException(); }

		object EnvDTE.ProjectItems.Parent
		{ get { throw new NotSupportedException(); } }

		EnvDTE.ProjectItem EnvDTE.ProjectItems.AddFromTemplate(string fileName, string name)
		{ throw new NotSupportedException(); }

		#endregion

		#endregion

		#region Implementation

		readonly MockProject containingProject;

		#endregion
	}
}
