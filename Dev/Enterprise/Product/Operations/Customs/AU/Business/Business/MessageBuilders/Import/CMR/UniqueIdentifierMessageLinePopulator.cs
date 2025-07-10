using System;
using System.Collections.Generic;
using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UniqueIdentifierMessageLinePopulator
	{
		public void Populate(SegmentGroup message, UniqueIdentifierMessageLine[] lines)
		{
			Populate(message, lines, null, false);
		}

		public void Populate(SegmentGroup message, UniqueIdentifierMessageLine[] lines, UniqueIdentifierMessageLine[] oldLines)
		{
			Populate(message, lines, oldLines, false);
		}

		public void Populate(SegmentGroup message, UniqueIdentifierMessageLine[] lines, UniqueIdentifierMessageLine[] oldLines, bool allowAmend)
		{
			Array.Sort(lines, LineComparer);
			var newLinesHashtable = new Dictionary<string, UniqueIdentifierMessageLine>();

			foreach (UniqueIdentifierMessageLine line in lines)
			{
				UniqueIdentifierMessageLine result;
				if (!newLinesHashtable.TryGetValue(line.UniqueIdentifier, out result))
				{
					newLinesHashtable.Add(line.UniqueIdentifier, line);
				}
			}

			var oldLinesHashtable = new Dictionary<string, string>();
			if (oldLines != null)
			{
				Array.Sort(oldLines, LineComparer);
				foreach (UniqueIdentifierMessageLine line in oldLines)
				{
					string lineIdentifier = line.UniqueIdentifier;
					string lineText = line.StringValue;
					if (!oldLinesHashtable.ContainsKey(lineIdentifier))
					{
						oldLinesHashtable.Add(lineIdentifier, lineText);
					}

					if (!newLinesHashtable.ContainsKey(lineIdentifier))
					{
						line.Populate(line.GetNewSegmentGroup(message), LineAction.Delete);
					}
					else
					{
						UniqueIdentifierMessageLine newLine = newLinesHashtable[lineIdentifier];
						if (newLine.StringValue != lineText)
						{
							if (allowAmend)
							{
								newLine.Populate(newLine.GetNewSegmentGroup(message), LineAction.Amend);
							}
							else
							{
								line.Populate(line.GetNewSegmentGroup(message), LineAction.Delete);
								newLine.Populate(newLine.GetNewSegmentGroup(message), LineAction.Insert);
							}
						}
					}
				}
			}

			foreach (UniqueIdentifierMessageLine line in lines)
			{
				if (!oldLinesHashtable.ContainsKey(line.UniqueIdentifier))
				{
					line.Populate(line.GetNewSegmentGroup(message), LineAction.Insert);
				}
			}
		}

		public void PopulateSplitMsgLines(SegmentGroup message, UniqueIdentifierMessageLine[] splitMsgLines, UniqueIdentifierMessageLine[] lines, UniqueIdentifierMessageLine[] oldLines)
		{
			Array.Sort(splitMsgLines, LineComparer);
			Array.Sort(lines, LineComparer);
			Array.Sort(oldLines, LineComparer);

			var splitLinesHashtable = new Dictionary<string, UniqueIdentifierMessageLine>();
			foreach (UniqueIdentifierMessageLine line in splitMsgLines)
			{
				UniqueIdentifierMessageLine result;
				if (!splitLinesHashtable.TryGetValue(line.UniqueIdentifier, out result))
				{
					splitLinesHashtable.Add(line.UniqueIdentifier, line);
				}
			}

			var newLinesHashtable = new Dictionary<string, UniqueIdentifierMessageLine>();
			foreach (UniqueIdentifierMessageLine line in lines)
			{
				UniqueIdentifierMessageLine result;
				if (!newLinesHashtable.TryGetValue(line.UniqueIdentifier, out result))
				{
					newLinesHashtable.Add(line.UniqueIdentifier, line);
				}
			}

			var oldLinesHashtable = new Dictionary<string, UniqueIdentifierMessageLine>();
			foreach (UniqueIdentifierMessageLine oldLine in oldLines)
			{
				UniqueIdentifierMessageLine result;
				if (!oldLinesHashtable.TryGetValue(oldLine.UniqueIdentifier, out result))
				{
					oldLinesHashtable.Add(oldLine.UniqueIdentifier, oldLine);
				}
			}

			// determine if lines truly need to be deleted/amended or inserted, or if they are simply in another of the split messages
			foreach (UniqueIdentifierMessageLine splitMsgLine in splitMsgLines)
			{
				string lineIdentifier = splitMsgLine.UniqueIdentifier;
				string lineText = splitMsgLine.StringValue;

				if (!newLinesHashtable.ContainsKey(lineIdentifier))
				{
					if (!splitMsgLine.LastMessageDate.IsEmpty)
					{
						splitMsgLine.Populate(splitMsgLine.GetNewSegmentGroup(message), LineAction.Delete);
					}
				}
				else
				{
					UniqueIdentifierMessageLine oldLine;
					oldLinesHashtable.TryGetValue(lineIdentifier, out oldLine);
					if (oldLine != null && oldLine.StringValue != lineText)
					{
						oldLine.Populate(oldLine.GetNewSegmentGroup(message), LineAction.Delete);
						splitMsgLine.Populate(splitMsgLine.GetNewSegmentGroup(message), LineAction.Insert);
					}
					else
					{
						splitMsgLine.Populate(splitMsgLine.GetNewSegmentGroup(message), LineAction.Insert);
					}
				}
			}
		}

		UniqueIdentifierMessageLineComparer LineComparer
		{
			get
			{
				if (fLineComparer == null)
				{
					fLineComparer = new UniqueIdentifierMessageLineComparer();
				}
				return fLineComparer;
			}
		}
		UniqueIdentifierMessageLineComparer fLineComparer;
	}
}
