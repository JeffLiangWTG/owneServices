using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockDocuments : List<EnvDTE.Document>, EnvDTE.Documents
	{
		System.Collections.IEnumerator EnvDTE.Documents.GetEnumerator()
		{ return GetEnumerator(); }

		#region Documents Unsupported Members

		EnvDTE.Document EnvDTE.Documents.Add(string kind)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.Documents.CloseAll(EnvDTE.vsSaveChanges save)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.Documents.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Document EnvDTE.Documents.Item(object index)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Document EnvDTE.Documents.Open(string pathName, string kind, bool readOnly)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.Documents.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.Documents.SaveAll()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
