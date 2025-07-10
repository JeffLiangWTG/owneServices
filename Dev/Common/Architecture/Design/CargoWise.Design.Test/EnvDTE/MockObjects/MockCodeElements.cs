using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeElements : List<EnvDTE.CodeElement>, EnvDTE.CodeElements
	{
		public MockCodeElements(params EnvDTE.CodeElement[] codeElements)
			: base(codeElements)
		{
		}

		public void AddRange(params EnvDTE.CodeElement[] elements)
		{ AddRange((IEnumerable<EnvDTE.CodeElement>)elements); }

		#region CodeElements Members

		public EnvDTE.CodeElement Item(object index)
		{ return this[(int)index - 1]; }

		System.Collections.IEnumerator EnvDTE.CodeElements.GetEnumerator()
		{ return GetEnumerator(); }

		#region Unsupported Members

		public bool CreateUniqueID(string Prefix, ref string NewName)
		{ throw new NotSupportedException(); }

		public void Reserved1(object Element)
		{ throw new NotSupportedException(); }

		public EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public object Parent
		{ get { throw new NotSupportedException(); } }

		#endregion

		#endregion
	}
}
