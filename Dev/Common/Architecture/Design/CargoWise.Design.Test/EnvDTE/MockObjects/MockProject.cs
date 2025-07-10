using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	internal static class MockProjectExtensions
	{
		public static MockVSProject GetVSProject(this MockProject project)
		{
			return (MockVSProject)(project.VSProject ?? (project.VSProject = new MockVSProject()));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockProject : MarshalByRefObject, EnvDTE.Project
	{
		public MockProject()
		{ DTE = new MockDTE(); }

		public MockProject(MockDTE dte)
		{ DTE = dte; }

		public void SetCodeModel(MockCodeModel codeModel)
		{
			this.codeModel = codeModel;
			codeModelSet = true;
		}

		public MockProjectItems ProjectItems
		{ get { return projectItems ?? (projectItems = new MockProjectItems(this)); } }
		MockProjectItems projectItems;

		public MockProjectItem AProjectItem
		{
			get
			{
				if (aProjectItem == null)
				{
					aProjectItem = new MockProjectItem(this);
					ProjectItems.Add(aProjectItem);
				}
				return aProjectItem;
			}
		}
		MockProjectItem aProjectItem;

		EnvDTE.ProjectItems EnvDTE.Project.ProjectItems
		{ get { return ProjectItems; } }

		public object VSProject { get; set; }

		#region Project Members

		public object Object
		{ get { return VSProject; } }

		public MockDTE DTE;

		EnvDTE.DTE EnvDTE.Project.DTE
		{ get { return DTE; } }

		string EnvDTE.Project.FileName
		{ get { return FileName; } }

		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}
		string fileName;

		EnvDTE.CodeModel EnvDTE.Project.CodeModel
		{ get { return CodeModel; } }

		public virtual MockCodeModel CodeModel
		{
			get
			{
				if (codeModel == null && !codeModelSet)
				{
					codeModel = new MockCodeModel(this);
				}
				return codeModel;
			}
		}
		MockCodeModel codeModel;
		bool codeModelSet;

		#region Unsupported Members

		public bool Saved
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public string Kind
		{ get { throw new NotSupportedException(); } }

		public object get_Extender(string ExtenderName)
		{ throw new NotSupportedException(); }

		public EnvDTE.ProjectItem ParentProjectItem
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.Projects Collection
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.Globals Globals
		{ get { throw new NotSupportedException(); } }

		public string FullName
		{ get { throw new NotSupportedException(); } }

		public void SaveAs(string NewFileName)
		{ throw new NotSupportedException(); }

		public MockConfigurationManager ConfigurationManager
		{
			get
			{
				if (configurationManager == null)
				{
					configurationManager = new MockConfigurationManager();
				}
				return configurationManager;
			}
		}
		MockConfigurationManager configurationManager;

		EnvDTE.ConfigurationManager EnvDTE.Project.ConfigurationManager
		{ get { return ConfigurationManager; } }

		public object ExtenderNames
		{ get { throw new NotSupportedException(); } }

		public string Name
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public void Delete()
		{ throw new NotSupportedException(); }

		public EnvDTE.Properties Properties
		{ get { throw new NotSupportedException(); } }

		public bool IsDirty
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public void Save(string FileName)
		{ throw new NotSupportedException(); }

		public string ExtenderCATID
		{ get { throw new NotSupportedException(); } }

		public string UniqueName
		{ get { throw new NotSupportedException(); } }

		#endregion

		#endregion
	}
}
