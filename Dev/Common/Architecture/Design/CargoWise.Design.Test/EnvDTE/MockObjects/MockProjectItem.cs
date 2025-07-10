using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockProjectItem : MarshalByRefObject, EnvDTE.ProjectItem
	{
		public MockProjectItem() : this(new MockProject())
		{
		}

		public MockProjectItem(MockDTE dte)
			: this(new MockProject(dte))
		{
		}

		public MockProjectItem(MockProject containingProject)
		{ this.containingProject = containingProject; }

		#region ProjectItem Members

		public MockProject ContainingProject
		{ get { return containingProject; } }

		readonly MockProject containingProject;

		EnvDTE.Project EnvDTE.ProjectItem.ContainingProject
		{ get { return containingProject; } }

		public MockFileCodeModel FileCodeModel
		{
			get
			{
				if (fileCodeModel == null && !fileCodeModelSet)
				{
					fileCodeModel = new MockFileCodeModel();
				}
				return fileCodeModel;
			}
		}
		MockFileCodeModel fileCodeModel;
		bool fileCodeModelSet;

		public void SetFileCodeModel(MockFileCodeModel value)
		{
			fileCodeModel = value;
			fileCodeModelSet = true;
		}

		EnvDTE.FileCodeModel EnvDTE.ProjectItem.FileCodeModel
		{ get { return FileCodeModel; } }

		public EnvDTE.ProjectItems ProjectItems
		{ get { return projectItems ?? (projectItems = new MockProjectItems(ContainingProject)); } }
		EnvDTE.ProjectItems projectItems;

		public short FileCount
		{ get { return (short)FileNames.Count; } }

		public string get_FileNames(short index)
		{ return FileNames[index - 1]; }

		public MockDocument Document
		{ get { return document ?? (document = new MockDocument(DTE, this)); } }
		MockDocument document;

		EnvDTE.Document EnvDTE.ProjectItem.Document
		{ get { return Document; } }

		public MockDTE DTE
		{ get { return ContainingProject.DTE; } }

		EnvDTE.DTE EnvDTE.ProjectItem.DTE
		{ get { return DTE; } }

		public readonly StringCollection FileNames = new StringCollection();

		public string SaveCalledWithFilename;
		public void Save(string FileName)
		{ SaveCalledWithFilename = FileName; }

		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}
		bool isDirty;

		public object Object { get; set; }

		#region Unsupported Members

		public bool get_IsOpen(string ViewKind)
		{ throw new NotSupportedException(); }

		public bool Saved
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public string Kind
		{ get { throw new NotSupportedException(); } }

		public object get_Extender(string ExtenderName)
		{ throw new NotSupportedException(); }

		public EnvDTE.ProjectItems Collection
		{ get { throw new NotSupportedException(); } }

		public void Remove()
		{ throw new NotSupportedException(); }

		public bool SaveAs(string NewFileName)
		{ throw new NotSupportedException(); }

		public EnvDTE.Window Open(string ViewKind)
		{ throw new NotSupportedException(); }

		public void ExpandView()
		{ throw new NotSupportedException(); }

		public EnvDTE.ConfigurationManager ConfigurationManager
		{ get { throw new NotSupportedException(); } }

		public object ExtenderNames
		{ get { throw new NotSupportedException(); } }

		public string Name
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public void Delete()
		{ throw new NotSupportedException(); }

		public EnvDTE.Project SubProject
		{ get { throw new NotSupportedException(); } }

		public MockProperties Properties
		{ get { return properties ?? (properties = new MockProperties(DTE)); } }
		MockProperties properties;

		EnvDTE.Properties EnvDTE.ProjectItem.Properties
		{ get { return Properties; } }

		public string ExtenderCATID
		{ get { throw new NotSupportedException(); } }

		#endregion

		#endregion
	}
}
