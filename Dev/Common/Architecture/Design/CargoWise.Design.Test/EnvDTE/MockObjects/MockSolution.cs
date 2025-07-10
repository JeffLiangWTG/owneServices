using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockSolution : MarshalByRefObject, EnvDTE.Solution
	{
		public MockSolution(MockDTE dte)
		{ this.dte = dte; }

		[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:ParameterNamesMustBeginWithLowerCaseLetter", Justification = "Suppression due to conflicts with CA1725:ParameterNamesShouldMatchBaseDeclaration")]
		public EnvDTE.ProjectItem FindProjectItem(string FileName)
		{
			foreach (EnvDTE.Project project in Projects)
			{
				foreach (EnvDTE.ProjectItem projectItem in project.ProjectItems)
				{
					if (projectItem.get_FileNames(1).ToLower() == FileName.ToLower())
					{
						return projectItem;
					}
				}
			}
			return null;
		}

		public MockDTE DTE
		{ get { return dte; } }

		readonly MockDTE dte;

		EnvDTE.DTE EnvDTE._Solution.DTE
		{ get { return DTE; } }

		public MockProjects Projects
		{ get { return projects ?? (projects = new MockProjects(DTE)); } }
		MockProjects projects;

		EnvDTE.Projects EnvDTE._Solution.Projects
		{ get { return Projects; } }

		string EnvDTE._Solution.FullName
		{ get { return fullName; } }

		readonly string fullName = "MockSolution";

		#region Unsupported Members

		EnvDTE.Project EnvDTE._Solution.AddFromFile(string fileName, bool exclusive)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Project EnvDTE._Solution.AddFromTemplate(string fileName, string destination, string projectName, bool exclusive)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.AddIns EnvDTE._Solution.AddIns
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE._Solution.Close(bool saveFirst)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int EnvDTE._Solution.Count
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE._Solution.Create(string destination, string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string EnvDTE._Solution.ExtenderCATID
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE._Solution.ExtenderNames
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE._Solution.FileName
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		System.Collections.IEnumerator EnvDTE._Solution.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Globals EnvDTE._Solution.Globals
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool EnvDTE._Solution.IsDirty
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		bool EnvDTE._Solution.IsOpen
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Project EnvDTE._Solution.Item(object index)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE._Solution.Open(string fileName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE._Solution.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE._Solution.ProjectItemsTemplatePath(string projectKind)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Properties EnvDTE._Solution.Properties
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE._Solution.Remove(EnvDTE.Project proj)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE._Solution.SaveAs(string fileName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE._Solution.Saved
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		EnvDTE.SolutionBuild EnvDTE._Solution.SolutionBuild
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE._Solution.get_Extender(string extenderName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string EnvDTE._Solution.get_TemplatePath(string projectType)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
