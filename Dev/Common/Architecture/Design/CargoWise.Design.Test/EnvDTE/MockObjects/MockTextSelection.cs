using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockTextSelection : MarshalByRefObject, EnvDTE.TextSelection
	{
		public MockTextSelection(MockTextDocument parent)
		{ this.parent = parent; }

		public MockTextDocument Parent
		{ get { return parent; } }

		readonly MockTextDocument parent;

		EnvDTE.TextDocument EnvDTE.TextSelection.Parent
		{ get { return Parent; } }

		public bool SelectAllCalled;
		public void SelectAll()
		{ SelectAllCalled = true; }

		void EnvDTE.TextSelection.StartOfDocument(bool extend)
		{
		}

		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				if (value.Length > 128)
				{
					throw new ArgumentException("Cannot set Text with a large amount of text otherwise it gets really really slow.");
				}
			}
		}
		string text = "";

		public void Insert(string Text, int vsInsertFlagsCollapseToEndValue)
		{
			if (vsInsertFlagsCollapseToEndValue == (int)EnvDTE.vsInsertFlags.vsInsertFlagsContainNewText)
			{
				text += Text;
			}
		}

		#region Unsupported Members

		EnvDTE.VirtualPoint EnvDTE.TextSelection.ActivePoint
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.TextSelection.AnchorColumn
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.VirtualPoint EnvDTE.TextSelection.AnchorPoint
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.TextSelection.Backspace(int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int EnvDTE.TextSelection.BottomLine
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.VirtualPoint EnvDTE.TextSelection.BottomPoint
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.TextSelection.Cancel()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.ChangeCase(EnvDTE.vsCaseOptions how)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.CharLeft(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.CharRight(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.ClearBookmark()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.Collapse()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.Copy()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int EnvDTE.TextSelection.CurrentColumn
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.TextSelection.CurrentLine
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.TextSelection.Cut()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.TextSelection.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.TextSelection.Delete(int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.DeleteLeft(int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.DeleteWhitespace(EnvDTE.vsWhitespaceOptions direction)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.DestructiveInsert(string text)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.EndOfDocument(bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.EndOfLine(bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextSelection.FindPattern(string pattern, int vsFindOptionsValue, ref EnvDTE.TextRanges tags)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextSelection.FindText(string pattern, int vsFindOptionsValue)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.GotoLine(int line, bool select)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.Indent(int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.InsertFromFile(string file)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextSelection.IsActiveEndGreater
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool EnvDTE.TextSelection.IsEmpty
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.TextSelection.LineDown(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.LineUp(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.vsSelectionMode EnvDTE.TextSelection.Mode
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

		void EnvDTE.TextSelection.MoveTo(int line, int column, bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.MoveToAbsoluteOffset(int offset, bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.MoveToDisplayColumn(int line, int column, bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.MoveToLineAndOffset(int line, int offset, bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.MoveToPoint(EnvDTE.TextPoint point, bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.NewLine(int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextSelection.NextBookmark()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.OutlineSection()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.PadToColumn(int column)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.PageDown(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.PageUp(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.Paste()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextSelection.PreviousBookmark()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextSelection.ReplacePattern(string pattern, string replace, int vsFindOptionsValue, ref EnvDTE.TextRanges tags)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.TextSelection.ReplaceText(string pattern, string replace, int vsFindOptionsValue)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.SelectLine()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.SetBookmark()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.SmartFormat()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.StartOfLine(EnvDTE.vsStartOfLineOptions where, bool extend)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.SwapAnchor()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.Tabify()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.TextPane EnvDTE.TextSelection.TextPane
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.TextRanges EnvDTE.TextSelection.TextRanges
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.TextSelection.TopLine
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.VirtualPoint EnvDTE.TextSelection.TopPoint
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.TextSelection.Unindent(int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.Untabify()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.WordLeft(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.TextSelection.WordRight(bool extend, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
