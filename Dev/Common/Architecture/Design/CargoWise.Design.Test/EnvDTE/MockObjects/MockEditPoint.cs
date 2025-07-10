using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockEditPoint : MarshalByRefObject, EnvDTE.EditPoint
	{
		public MockEditPoint(MockTextDocument parent)
		{ this.parent = parent; }

		EnvDTE.TextDocument EnvDTE.EditPoint.Parent
		{ get { return Parent; } }

		EnvDTE.TextDocument EnvDTE.TextPoint.Parent
		{ get { return Parent; } }

		public MockTextDocument Parent
		{ get { return parent; } }

		readonly MockTextDocument parent;

		public string GetText(object PointOrCount)
		{
			string result = null;
			if (Parent is MockTextDocument doc && PointOrCount == Parent.EndPoint)
			{
				result = doc.Text;
			}
			if (result == null)
			{
				throw new NotSupportedException("Only Parent.EndPoint is allowed here where Parent is of type " + nameof(MockTextDocument));
			}
			return result;
		}

		public void ReplaceText(object PointOrCount, string Text, int Flags)
		{
			if (PointOrCount != Parent.EndPoint)
			{
				throw new NotSupportedException();
			}
			Parent.Text = Text;
		}

		#region Unsupported Members

		public void DeleteWhitespace(EnvDTE.vsWhitespaceOptions Direction)
		{ throw new NotSupportedException(); }

		public bool ReadOnly(object PointOrCount)
		{ throw new NotSupportedException(); }

		public void MoveToAbsoluteOffset(int Offset)
		{ throw new NotSupportedException(); }

		public void MoveToPoint(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		public int LineLength
		{ get { throw new NotSupportedException(); } }

		public void SmartFormat(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		public bool AtEndOfDocument
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public void WordRight(int Count)
		{ throw new NotSupportedException(); }

		public void Insert(string Text)
		{ throw new NotSupportedException(); }

		public int LineCharOffset
		{ get { throw new NotSupportedException(); } }

		public bool FindPattern(string Pattern, int vsFindOptionsValue, ref EnvDTE.EditPoint EndPoint, ref EnvDTE.TextRanges Tags)
		{ throw new NotSupportedException(); }

		public bool PreviousBookmark()
		{ throw new NotSupportedException(); }

		public void EndOfLine()
		{ throw new NotSupportedException(); }

		public void Copy(object PointOrCount, bool Append)
		{ throw new NotSupportedException(); }

		public void ClearBookmark()
		{ throw new NotSupportedException(); }

		public EnvDTE.EditPoint CreateEditPoint()
		{ throw new NotSupportedException(); }

		public void OutlineSection(object PointOrCount)
		{ throw new NotSupportedException(); }

		public void Unindent(EnvDTE.TextPoint Point, int Count)
		{ throw new NotSupportedException(); }

		public bool LessThan(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		public bool AtStartOfDocument
		{ get { throw new NotSupportedException(); } }

		public bool ReplacePattern(EnvDTE.TextPoint Point, string Pattern, string Replace, int vsFindOptionsValue, ref EnvDTE.TextRanges Tags)
		{ throw new NotSupportedException(); }

		public void CharRight(int Count)
		{ throw new NotSupportedException(); }

		public int AbsoluteCharOffset
		{ get { throw new NotSupportedException(); } }

		public void ChangeCase(object PointOrCount, EnvDTE.vsCaseOptions How)
		{ throw new NotSupportedException(); }

		public void MoveToLineAndOffset(int Line, int Offset)
		{ throw new NotSupportedException(); }

		public void EndOfDocument()
		{ throw new NotSupportedException(); }

		public void LineDown(int Count)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElement get_CodeElement(EnvDTE.vsCMElement Scope)
		{ throw new NotSupportedException(); }

		public void PadToColumn(int Column)
		{ throw new NotSupportedException(); }

		public bool EqualTo(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		public int Line
		{ get { throw new NotSupportedException(); } }

		public bool NextBookmark()
		{ throw new NotSupportedException(); }

		public int DisplayColumn
		{ get { throw new NotSupportedException(); } }

		public void InsertFromFile(string File)
		{ throw new NotSupportedException(); }

		public void LineUp(int Count)
		{ throw new NotSupportedException(); }

		public bool AtStartOfLine
		{ get { throw new NotSupportedException(); } }

		public void Cut(object PointOrCount, bool Append)
		{ throw new NotSupportedException(); }

		public bool TryToShow(EnvDTE.vsPaneShowHow How, object PointOrCount)
		{ throw new NotSupportedException(); }

		public void Delete(object PointOrCount)
		{ throw new NotSupportedException(); }

		public bool AtEndOfLine
		{ get { throw new NotSupportedException(); } }

		public void Indent(EnvDTE.TextPoint Point, int Count)
		{ throw new NotSupportedException(); }

		public string GetLines(int Start, int ExclusiveEnd)
		{ throw new NotSupportedException(); }

		public void StartOfLine()
		{ throw new NotSupportedException(); }

		public void StartOfDocument()
		{ throw new NotSupportedException(); }

		public void Paste()
		{ throw new NotSupportedException(); }

		public void SetBookmark()
		{ throw new NotSupportedException(); }

		public void WordLeft(int Count)
		{ throw new NotSupportedException(); }

		public bool GreaterThan(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		public void CharLeft(int Count)
		{ throw new NotSupportedException(); }

		#endregion
	}
}
