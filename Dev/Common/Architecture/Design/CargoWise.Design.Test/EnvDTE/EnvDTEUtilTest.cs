using System.IO;
using NUnit.Framework;

namespace CargoWise.Design.DTE.Testing
{
	class EnvDTEUtilTest : TestCase
	{
		#region CodeTypeFromFullName
		public void TestCodeTypeFromFullName_FromLoadedDocument()
		{
			MockProjectItem projectItemInOpenDocument = new MockProjectItem(DTE);
			DTE.Documents.Add(new MockDocument(DTE, projectItemInOpenDocument));
			projectItemInOpenDocument.FileCodeModel.CodeElements.Add(CodeType);
			AssertEquals("EnvDTE.CodeType returned from open document first, for performance", CodeType, EnvDTEUtil.CodeTypeFromFullName(Project, CodeType.FullName));
		}

		public void TestCodeTypeFromFullName_FromProject()
		{
			ProjectItem.FileCodeModel.CodeElements.Add(CodeType);
			AssertEquals("EnvDTE.CodeType returned from project", CodeType, EnvDTEUtil.CodeTypeFromFullName(Project, CodeType.FullName));
		}

		public void TestCodeTypeFromFullName_FromReferencedProject()
		{
			MockProjectItem projectItemInReferencedProject = new MockProjectItem(ReferencedProject);
			ReferencedProject.ProjectItems.Add(projectItemInReferencedProject);
			projectItemInReferencedProject.FileCodeModel.CodeElements.Add(CodeType);
			AssertEquals("EnvDTE.CodeType returned from referenced project", CodeType, EnvDTEUtil.CodeTypeFromFullName(Project, CodeType.FullName));
		}

		#endregion
		#region WriteTextToProjectItem
		public void TestWriteTextToProjectItem_SaveToProjectItem()
		{
			MockProject project = DTE.Solution.Projects.AddNew();
			project.ProjectItems.Add(ProjectItem);
			ProjectItem.FileNames.Add(TempFile);
			ProjectItem.Document.TextDocument.Text = "original_file_content";
			File.SetAttributes(TempFile, FileAttributes.ReadOnly);
			EnvDTEUtil.WriteTextToProjectItem(DTE, TempFile, new string('x', 1024));
			AssertEquals("Document must be closed before writing to the file", true, ProjectItem.Document.CloseCalled);
			AssertEquals("Text written to the file", new string('x', 1024), File.ReadAllText(TempFile));
			AssertEquals("File checked out", TempFile, DTE.SourceControl.LastSingleItemCheckedOut);
		}

		#endregion
		#region Implementation
		MockDTE DTE
		{
			get
			{
				return dte ?? (dte = new MockDTE());
			}
		}

		MockDTE dte;
		MockProject Project
		{
			get
			{
				if (project == null)
				{
					project = new MockProject(DTE);
				}

				return project;
			}
		}

		MockProject project;
		MockProject ReferencedProject
		{
			get
			{
				if (referencedProject == null)
				{
					referencedProject = new MockProject(DTE);
					Project.GetVSProject().References.AddProject(referencedProject);
				}

				return referencedProject;
			}
		}

		MockProject referencedProject;
		MockProjectItem ProjectItem
		{
			get
			{
				if (projectItem == null)
				{
					projectItem = new MockProjectItem(Project);
					Project.ProjectItems.Add(projectItem);
				}

				return projectItem;
			}
		}

		MockProjectItem projectItem;
		MockCodeClass CodeType
		{
			get
			{
				return codeType ?? (codeType = new MockCodeClass(null, "CodeType"));
			}
		}

		MockCodeClass codeType;
		string TempFile
		{
			get
			{
				return tempFile ?? (tempFile = TempForTest.GetTempFileName());
			}
		}

		string tempFile;
		protected override void TearDown()
		{
			base.TearDown();
			if (tempFile != null && File.Exists(tempFile))
			{
				File.SetAttributes(tempFile, FileAttributes.Normal);
				File.Delete(tempFile);
			}
		}
		#endregion
	}
}
