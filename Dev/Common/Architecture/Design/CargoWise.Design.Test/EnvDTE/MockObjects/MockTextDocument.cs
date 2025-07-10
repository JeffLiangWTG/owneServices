using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockTextDocument : MarshalByRefObject, EnvDTE.TextDocument
	{
		public MockTextDocument(MockDocument parent)
		{ this.parent = parent; }

		public string Text;

		EnvDTE.TextPoint EnvDTE.TextDocument.StartPoint
		{ get { return StartPoint; } }

		public EnvDTE.TextPoint StartPoint
		{
			get
			{
				if (startPoint == null)
				{
					startPoint = new MockTextPoint(this);
				}
				return startPoint;
			}
		}
		MockTextPoint startPoint;

		EnvDTE.TextPoint EnvDTE.TextDocument.EndPoint
		{ get { return EndPoint; } }

		public EnvDTE.TextPoint EndPoint
		{
			get
			{
				if (endPoint == null)
				{
					endPoint = new MockTextPoint(this);
				}
				return endPoint;
			}
		}
		MockTextPoint endPoint;

		public MockTextSelection Selection
		{ get { return selection ?? (selection = new MockTextSelection(this)); } }
		MockTextSelection selection;

		EnvDTE.Document EnvDTE.TextDocument.Parent
		{ get { return Parent; } }

		public MockDocument Parent
		{ get { return parent; } }

		readonly MockDocument parent;

		#region Unsupported EnvDTE.TextDocument Members

		void EnvDTE.TextDocument.ClearBookmarks()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.EditPoint EnvDTE.TextDocument.CreateEditPoint(EnvDTE.TextPoint textPoint)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.TextDocument.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.TextDocument.IndentSize
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.TextDocument.Language
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

		bool EnvDTE.TextDocument.MarkText(string pattern, int vsFindOptionsValue)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextDocument.PrintOut()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextDocument.ReplacePattern(string pattern, string replace, int vsFindOptionsValue, ref EnvDTE.TextRanges tags)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextDocument.ReplaceText(string findText, string replaceText, int vsFindOptionsValue)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.TextSelection EnvDTE.TextDocument.Selection
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.TextDocument.TabSize
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.TextDocument.Type
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
