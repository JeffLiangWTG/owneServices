using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockProjects : List<EnvDTE.Project>, EnvDTE.Projects
	{
		public MockProjects(MockDTE dte)
		{ this.dte = dte; }

		public MockDTE DTE
		{ get { return dte; } }

		readonly MockDTE dte;

		EnvDTE.DTE EnvDTE.Projects.DTE
		{ get { return DTE; } }

		System.Collections.IEnumerator EnvDTE.Projects.GetEnumerator()
		{ return GetEnumerator(); }

		public MockProject AddNew()
		{
			MockProject result = new MockProject(DTE);
			Add(result);
			return result;
		}

		#region Projects Unsupported Members

		EnvDTE.Project EnvDTE.Projects.Item(object index)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string EnvDTE.Projects.Kind
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTE EnvDTE.Projects.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Properties EnvDTE.Projects.Properties
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
