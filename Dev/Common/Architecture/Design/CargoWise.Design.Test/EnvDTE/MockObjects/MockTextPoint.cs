using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockTextPoint : MarshalByRefObject, EnvDTE.TextPoint
	{
		public MockTextPoint(MockTextDocument parent)
		{ this.parent = parent; }

		EnvDTE.TextDocument EnvDTE.TextPoint.Parent
		{ get { return Parent; } }

		public MockTextDocument Parent
		{ get { return parent; } }

		readonly MockTextDocument parent;

		public EnvDTE.EditPoint CreateEditPoint()
		{ return new MockEditPoint(Parent); }

		public int Line
		{ get { return line; } }
		int line;

		public void SetLine(int line)
		{ this.line = line; }

		#region Unsupported Members

		public int LineLength
		{ get { throw new NotSupportedException(); } }

		public bool AtEndOfDocument
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public int LineCharOffset
		{ get { throw new NotSupportedException(); } }

		public bool LessThan(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		public bool AtStartOfDocument
		{ get { throw new NotSupportedException(); } }

		public int AbsoluteCharOffset
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElement get_CodeElement(EnvDTE.vsCMElement Scope)
		{ throw new NotSupportedException(); }

		public bool EqualTo(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		public int DisplayColumn
		{ get { throw new NotSupportedException(); } }

		public bool AtStartOfLine
		{ get { throw new NotSupportedException(); } }

		public bool TryToShow(EnvDTE.vsPaneShowHow How, object PointOrCount)
		{ throw new NotSupportedException(); }

		public bool AtEndOfLine
		{ get { throw new NotSupportedException(); } }

		public bool GreaterThan(EnvDTE.TextPoint Point)
		{ throw new NotSupportedException(); }

		#endregion
	}
}
