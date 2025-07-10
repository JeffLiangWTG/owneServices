using System;
using System.Collections.Generic;
using CargoWise.Types;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Mapping
{
	public class MapTable
	{
		public MapTable(ZString typeTitle, ZString defaultFieldName, ZString leftColumnTitle, int leftColumnWidth, ZString rightColumnTitle, int rightColumnWidth, bool isNonGeneric)
		{
			TypeTitle = typeTitle;
			DefaultFieldName = defaultFieldName;
			LeftColumnTitle = leftColumnTitle;
			LeftColumnWidth = leftColumnWidth;
			RightColumnTitle = rightColumnTitle;
			RightColumnWidth = rightColumnWidth;
			IsNonGeneric = isNonGeneric;

			Lines = new SortedList<string, Line>();
			Errors = new List<ZString>();
		}

		public readonly ZString TypeTitle;
		public readonly ZString DefaultFieldName;
		public readonly ZString LeftColumnTitle;
		readonly int LeftColumnWidth;
		public readonly ZString RightColumnTitle;
		readonly int RightColumnWidth;
		public readonly ZBool IsNonGeneric;
		readonly SortedList<string, Line> Lines;

		int FullWidth
		{
			get { return LeftColumnWidth + 2 + RightColumnWidth; }
		}

		public ZString DefaultFieldIncludingTitle
		{
			get { return (DefaultFieldName.IsEmpty ? "" : Res.GetString("d195105d-110e-4c69-b4d7-7da5911602ae", "(Default Field: {0})", DefaultFieldName)); }
		}

		public override string ToString()
		{
			ZStringBuilder mapStringBuilder = new ZStringBuilder();

			ZString fullTitle = TypeTitle;
			ZString defaultFieldIncludingTitle = DefaultFieldIncludingTitle;
			if (!defaultFieldIncludingTitle.IsEmpty)
			{
				int spacesRequired = Math.Max(5, (FullWidth - (TypeTitle.Length + defaultFieldIncludingTitle.Length)));
				fullTitle = fullTitle + "".PadRight(spacesRequired) + defaultFieldIncludingTitle;
			}

			mapStringBuilder.Append(fullTitle);
			mapStringBuilder.Append("".PadRight(FullWidth, '='));
			mapStringBuilder.Append(LeftColumnTitle.PadRight(LeftColumnWidth) + "  " + RightColumnTitle);//.Left(RightColumnWidth));
			mapStringBuilder.Append("".PadRight(FullWidth, '-'));

			List<Line> lines = GetLines();
			foreach (Line line in lines)
			{
				if (line.LeftColumnValue.IsEmpty && line.RightColumnValue.IsEmpty)
				{
					mapStringBuilder.Append(ZString.Empty);
				}
				else
				{
					mapStringBuilder.Append(line.LeftColumnValue.PadRight(LeftColumnWidth) + "  " + line.RightColumnValue);//.Left(RightColumnWidth));
				}
			}

			return mapStringBuilder.ToStringWithNewLineBetweenAppends();
		}

		public void AddLine(string sortKey, ZString leftColumnValue, ZString rightColumnValue)
		{
			Line line = new Line(leftColumnValue, rightColumnValue);
			Line duplicateLine = null;
			if (Lines.TryGetValue(sortKey, out duplicateLine))
			{
				// This happens when there's a public new of a base class with the same name, so it's not an error as such.
				// Errors.Add("Cannot insert duplicate sortKey [" + sortKey + "] - {" + leftColumnValue + "} - {" + rightColumnValue + "}.\r\n"
				//    + "Already has sortKey [" + sortKey + "] - {" + duplicateLine.LeftColumnValue + "} - {" + duplicateLine.RightColumnValue + "}.");
				// If an error is added, an exception WILL be thrown in the Mapper.
			}
			else
			{
				Lines.Add(sortKey, line);
			}
		}

		public class Line
		{
			public Line(ZString leftColumnValue, ZString rightColumnValue)
			{
				LeftColumnValue = leftColumnValue;
				RightColumnValue = rightColumnValue;
			}
			public readonly ZString LeftColumnValue;
			public readonly ZString RightColumnValue;
		}

		public List<Line> GetLines()
		{
			List<Line> result = new List<Line>();
			foreach (KeyValuePair<string, Line> line in Lines)
			{
				result.Add(line.Value);
			}
			return result;
		}

		internal readonly List<ZString> Errors;
	}
}
