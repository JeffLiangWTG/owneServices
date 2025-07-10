using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class MockReference : MarshalByRefObject, VSLangProj.Reference
	{
		public MockProject SourceProject
		{
			get { return sourceProject; }
			set { sourceProject = value; }
		}
		MockProject sourceProject;

		EnvDTE.Project VSLangProj.Reference.SourceProject
		{ get { return SourceProject; } }

		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		string name;

		string VSLangProj.Reference.Name
		{ get { return Name; } }

		public string Path
		{
			get { return path; }
			set { path = value; }
		}
		string path;

		string VSLangProj.Reference.Path
		{ get { return Path; } }

		#region Reference Unsupported Members

		int VSLangProj.Reference.BuildNumber
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		VSLangProj.References VSLangProj.Reference.Collection
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Project VSLangProj.Reference.ContainingProject
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool VSLangProj.Reference.CopyLocal
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

		string VSLangProj.Reference.Culture
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTE VSLangProj.Reference.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string VSLangProj.Reference.Description
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string VSLangProj.Reference.ExtenderCATID
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object VSLangProj.Reference.ExtenderNames
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string VSLangProj.Reference.Identity
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int VSLangProj.Reference.MajorVersion
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int VSLangProj.Reference.MinorVersion
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string VSLangProj.Reference.PublicKeyToken
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void VSLangProj.Reference.Remove()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int VSLangProj.Reference.RevisionNumber
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool VSLangProj.Reference.StrongName
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		VSLangProj.prjReferenceType VSLangProj.Reference.Type
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string VSLangProj.Reference.Version
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object VSLangProj.Reference.this[string extenderName]
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
