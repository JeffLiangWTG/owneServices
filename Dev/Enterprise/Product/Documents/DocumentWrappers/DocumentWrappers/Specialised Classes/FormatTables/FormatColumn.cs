using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.DocumentWrappers.FormatTables
{
	[System.Diagnostics.DebuggerDisplay("{Heading}, {LeftPadding}, {InnerWidth}, {RightPadding}, {Options}")]
	internal sealed class FormatColumn : IEnumerable<string>
	{
		public FormatColumn(IEnumerable<string> sourceText, int innerWidth)
		{
			if (sourceText == null)
			{
				throw new ArgumentNullException(nameof(sourceText));
			}

			if (innerWidth < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(innerWidth), innerWidth, "innerWidth must not be negative");
			}

			this.sourceText = sourceText;
			this.innerWidth = innerWidth;
		}

		#region Properties

		public string Heading { get; set; }
		public FormatColumnOptions Options { get; set; }

		public int InnerWidth
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return innerWidth; }
		}
		public int LeftPadding
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return leftPadding; }
			set { leftPadding = Math.Max(0, value); }
		}
		public int RightPadding
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return rightPadding; }
			set { rightPadding = Math.Max(0, value); }
		}
		public int OuterWidth
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return leftPadding + innerWidth + rightPadding; }
		}

		#endregion

		#region IEnumerable<string> Members

		public IEnumerator<string> GetEnumerator()
		{
			return sourceText.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		[System.Diagnostics.DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		readonly IEnumerable<string> sourceText;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int innerWidth;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		int leftPadding;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		int rightPadding;
	}
}
