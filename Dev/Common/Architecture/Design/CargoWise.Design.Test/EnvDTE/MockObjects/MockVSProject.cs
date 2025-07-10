using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class MockVSProject : MarshalByRefObject, VSLangProj.VSProject
	{
		#region VSProject Members

		public MockReferences References
		{ get { return references ?? (references = new MockReferences()); } }
		public MockReferences references;

		VSLangProj.References VSLangProj.VSProject.References
		{ get { return References; } }

		#region Unsupported Members

		public virtual VSLangProj.VSProjectEvents Events
		{ get { throw new NotSupportedException(); } }

		public virtual EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public virtual VSLangProj.Imports Imports
		{ get { throw new NotSupportedException(); } }

		public virtual string GetUniqueFilename(object pDispatch, string bstrRoot, string bstrDesiredExt)
		{ throw new NotSupportedException(); }

		public virtual string TemplatePath
		{ get { throw new NotSupportedException(); } }

		public virtual EnvDTE.ProjectItem WebReferencesFolder
		{ get { throw new NotSupportedException(); } }

		public virtual void GenerateKeyPairFiles(string strPublicPrivateFile, string strPublicOnlyFile)
		{ throw new NotSupportedException(); }

		public virtual EnvDTE.ProjectItem CreateWebReferencesFolder()
		{ throw new NotSupportedException(); }

		public virtual void Exec(VSLangProj.prjExecCommand command, int bSuppressUi, object varIn, out object pVarOut)
		{ throw new NotSupportedException(); }

		public virtual void CopyProject(string bstrDestFolder, string bstrDestUncPath, VSLangProj.prjCopyProjectOption copyProjectOption, string bstrUsername, string bstrPassword)
		{ throw new NotSupportedException(); }

		public virtual EnvDTE.ProjectItem AddWebReference(string bstrUrl)
		{ throw new NotSupportedException(); }

		public virtual void Refresh()
		{ throw new NotSupportedException(); }

		public virtual bool WorkOffline
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public virtual EnvDTE.Project Project
		{ get { throw new NotSupportedException(); } }

		public virtual VSLangProj.BuildManager BuildManager
		{ get { throw new NotSupportedException(); } }

		#endregion

		#endregion
	}
}
