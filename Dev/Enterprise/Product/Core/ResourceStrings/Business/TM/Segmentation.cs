using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Enterprise.ResourceStrings.Business
{
	public static class Segmentation
	{
		public static SegmentInfo[] Segment(string paragraph)
		{
			if (paragraph == null)
			{
				throw new NullReferenceException(nameof(paragraph));
			}

			var segments = new List<SegmentInfo>();
			var start = 0;
			var match = segmentBreakRegex.Match(paragraph);
			int startOffset;
			while (match.Success)
			{
				var seg = Trim(paragraph.Substring(start, (match.Index - start) + match.Length), out startOffset);
				if (!string.IsNullOrEmpty(seg))
				{
					segments.Add(new SegmentInfo(seg, start + startOffset));
				}
				start = match.Index + match.Length;
				match = segmentBreakRegex.Match(paragraph, start);
			}
			var tail = Trim(paragraph.Substring(start), out startOffset);
			if (!string.IsNullOrEmpty(tail))
			{
				segments.Add(new SegmentInfo(tail, start + startOffset));
			}

			return segments.ToArray();
		}

		static string Trim(string seg, out int startOffset)
		{
			startOffset = 0;
			while (seg.Length > startOffset && char.IsWhiteSpace(seg[startOffset]))
			{
				startOffset++;
			}
			return seg.Substring(startOffset).TrimEnd();
		}

		public static string ReplaceSegment(string paragraph, string originalSegment, string replacementSegment)
		{
			if (paragraph == null)
			{
				throw new NullReferenceException(nameof(paragraph));
			}

			var startOffset = 0;
			foreach (var segment in Segment(paragraph))
			{
				if (segment.Text == originalSegment.Trim())
				{
					var startIndex = segment.StartIndex + startOffset;
					paragraph = paragraph.Substring(0, startIndex) + replacementSegment + paragraph.Substring(startIndex + segment.Text.Length);
					startOffset += (replacementSegment?.Length ?? 0) - originalSegment.Length;
				}
			}
			return paragraph;
		}

		static readonly Regex segmentBreakRegex;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810")]
		static Segmentation()
		{
			StringBuilder abbreviations = new StringBuilder();
			var asm = Assembly.GetExecutingAssembly();
			using (var reader = new StreamReader(asm.GetManifestResourceStream(asm.GetName().Name + ".TM.Abbreviations.txt")))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					line = line.Trim();
					if (!string.IsNullOrEmpty(line))
					{
						if (abbreviations.Length > 0)
						{
							abbreviations.Append("|");
						}
						abbreviations.Append(Regex.Escape(line.TrimEnd('.')));
					}
				}
			}
			segmentBreakRegex = new Regex(string.Format(@"(?:(?<!(?:^|\W+)(?:{0}))\.)\s|[\?\!\u3002\uFF01\uFF1F\r\n]", abbreviations.ToString()), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
		}

		public class SegmentInfo
		{
			public SegmentInfo(string text, int startIndex)
			{
				Text = text;
				StartIndex = startIndex;
			}

			public string Text { get; }
			public int StartIndex { get; }
		}
	}
}
