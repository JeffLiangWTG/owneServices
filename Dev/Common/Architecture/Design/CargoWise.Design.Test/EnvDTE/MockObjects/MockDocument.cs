using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockDocument : MarshalByRefObject, EnvDTE.Document
	{
		public MockDocument(MockDTE dte, MockProjectItem projectItem)
		{
			this.dte = dte;
			ProjectItem = projectItem;
		}

		EnvDTE.DTE EnvDTE.Document.DTE
		{ get { return dte; } }
		readonly MockDTE dte;

		EnvDTE.ProjectItem EnvDTE.Document.ProjectItem
		{ get { return ProjectItem; } }

		public readonly MockProjectItem ProjectItem;

		public MockTextDocument TextDocument
		{ get { return textDocument ?? (textDocument = new MockTextDocument(this)); } }
		MockTextDocument textDocument;

		object EnvDTE.Document.Object(string modelKind)
		{
			object result = null;
			if (modelKind == "TextDocument")
			{
				return TextDocument;
			}
			return result;
		}

		public bool SaveCalled;
		public string FileNameOfSave;
		public EnvDTE.vsSaveStatus Save(string FileName)
		{
			SaveCalled = true;
			FileNameOfSave = FileName;
			return EnvDTE.vsSaveStatus.vsSaveSucceeded;
		}

		object EnvDTE.Document.Selection
		{ get { return TextDocument.Selection; } }

		public bool CloseCalled;

		void EnvDTE.Document.Close(EnvDTE.vsSaveChanges save)
		{ Close(save); }

		public void Close(EnvDTE.vsSaveChanges save)
		{ CloseCalled = true; }

		#region Document Unsupported Members

		void EnvDTE.Document.Activate()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Window EnvDTE.Document.ActiveWindow
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.Document.ClearBookmarks()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Documents EnvDTE.Document.Collection
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.Document.ExtenderCATID
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Document.ExtenderNames
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.Document.FullName
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.Document.IndentSize
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.Document.Kind
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.Document.Language
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

		bool EnvDTE.Document.MarkText(string pattern, int flags)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string EnvDTE.Document.Name
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Window EnvDTE.Document.NewWindow()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string EnvDTE.Document.Path
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.Document.PrintOut()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.Document.ReadOnly
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

		bool EnvDTE.Document.Redo()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.Document.ReplaceText(string findText, string replaceText, int flags)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.Document.Saved
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

		int EnvDTE.Document.TabSize
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.Document.Type
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool EnvDTE.Document.Undo()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Windows EnvDTE.Document.Windows
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Document.get_Extender(string extenderName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
