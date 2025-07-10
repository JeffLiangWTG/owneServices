using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class MockReferences : List<VSLangProj.Reference>, VSLangProj.References
	{
		public MockReference Item(object index)
		{ return (MockReference)this[(int)index - 1]; }

		VSLangProj.Reference VSLangProj.References.Item(object index)
		{ return Item(index); }

		IEnumerator IEnumerable.GetEnumerator()
		{ return GetEnumerator(); }

		public MockReference AddProject(MockProject pProject)
		{
			MockReference reference = new MockReference();
			reference.SourceProject = pProject;
			Add(reference);
			return reference;
		}

		VSLangProj.Reference VSLangProj.References.AddProject(EnvDTE.Project pProject)
		{ return AddProject((MockProject)pProject); }

		#region References Unsupported Members

		VSLangProj.Reference VSLangProj.References.Add(string bstrPath)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		VSLangProj.Reference VSLangProj.References.AddActiveX(string bstrTypeLibGuid, int lMajorVer, int lMinorVer, int lLocaleId, string bstrWrapperTool)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Project VSLangProj.References.ContainingProject
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTE VSLangProj.References.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		VSLangProj.Reference VSLangProj.References.Find(string bstrIdentity)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		object VSLangProj.References.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
