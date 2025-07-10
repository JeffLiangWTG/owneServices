using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.DocumentWrappers.FormatTables
{
	[System.Diagnostics.DebuggerDisplay("{SegmentText}")]
	internal sealed class FormatSegment
	{
		#region EnumerateSegments

		public static IEnumerator<FormatSegment> EnumerateSegments(string sourceText, int maxLength)
		{
			int start = 0;
			int lastWordBoundary = 0;

			for (int i = 0; i < sourceText.Length; i++)
			{
				if (sourceText[i] == ' ')
				{
					lastWordBoundary = i;
				}

				if (sourceText[i] == '\n')
				{
					yield return new FormatSegment(sourceText, start, i - start);
					start = i + 1;
					lastWordBoundary = 0;
				}
				else if (sourceText[i] == '\r')
				{
					yield return new FormatSegment(sourceText, start, i - start);

					if (i + 1 < sourceText.Length && sourceText[i + 1] == '\n')
					{
						i++;
					}

					start = i + 1;
					lastWordBoundary = 0;
				}
				else if ((i - start) == maxLength)
				{
					if (lastWordBoundary > 0)
					{
						i = lastWordBoundary;
					}

					yield return new FormatSegment(sourceText, start, i - start);
					start = lastWordBoundary > 0 ? lastWordBoundary + 1 : i;
					lastWordBoundary = 0;
				}
			}

			yield return new FormatSegment(sourceText, start, sourceText.Length - start);
		}

		#endregion

		FormatSegment(string sourceText, int offset, int length)
		{
			if (sourceText == null)
			{
				throw new ArgumentNullException(nameof(sourceText));
			}

			this.sourceText = sourceText;
			this.offset = offset;
			this.length = length;
		}

		public void AddTo(StringBuilder builder)
		{
			builder.Append(sourceText, offset, length);
		}

		#region Properties

		public int Offest
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return offset; }
		}

		public int Length
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return length; }
		}

		public string SourceText
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return sourceText; }
		}

		public string SegmentText
		{
			get { return sourceText.Substring(offset, length); }
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string sourceText;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int offset;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int length;
	}
}
