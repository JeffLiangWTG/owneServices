using System;
using System.Collections;
using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UniqueIdentifierOutturnMessageLinePopulator
	{
		public void Populate(SegmentGroup message, UniqueIdentifierMessageLine[] lines)
		{
			Populate(message, lines, null);
		}

		public void Populate(SegmentGroup message, UniqueIdentifierMessageLine[] lines, UniqueIdentifierMessageLine[] oldLines)
		{
			Array.Sort(lines, LineComparer);

			Hashtable newLinesHashtable = new Hashtable();

			foreach (UniqueIdentifierMessageLine line in lines)
			{
				if (!newLinesHashtable.Contains(line.UniqueIdentifier))
				{
					newLinesHashtable.Add(line.UniqueIdentifier, line);
				}
			}

			Hashtable oldLinesHashtable = new Hashtable();

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
						if (!line.LastMessageDate.IsEmpty)
						{
							line.Populate(line.GetNewSegmentGroup(message), LineAction.Delete);
						}
					}
					else
					{
						UniqueIdentifierMessageLine newLine = (UniqueIdentifierMessageLine)newLinesHashtable[lineIdentifier];
						if (newLine.StringValue != lineText && !newLine.LastMessageDate.IsEmpty)
						{
							newLine.Populate(newLine.GetNewSegmentGroup(message), LineAction.Amend);
						}
					}
				}
			}

			foreach (UniqueIdentifierMessageLine line in lines)
			{
				if (!oldLinesHashtable.ContainsKey(line.UniqueIdentifier) || line.LastMessageDate.IsEmpty)
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

			Hashtable splitLinesHashtable = new Hashtable();
			foreach (UniqueIdentifierMessageLine line in splitMsgLines)
			{
				if (!splitLinesHashtable.Contains(line.UniqueIdentifier))
				{
					splitLinesHashtable.Add(line.UniqueIdentifier, line);
				}
			}

			Hashtable newLinesHashtable = new Hashtable();
			foreach (UniqueIdentifierMessageLine line1 in lines)
			{
				if (!newLinesHashtable.Contains(line1.UniqueIdentifier))
				{
					newLinesHashtable.Add(line1.UniqueIdentifier, line1);
				}
			}

			Hashtable oldLinesHashtable = new Hashtable();
			foreach (UniqueIdentifierMessageLine oldLine in oldLines)
			{
				if (!oldLinesHashtable.Contains(oldLine.UniqueIdentifier))
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
					UniqueIdentifierMessageLine oldLine1 = (UniqueIdentifierMessageLine)oldLinesHashtable[lineIdentifier];
					if (oldLine1 != null && oldLine1.StringValue != lineText && !oldLine1.LastMessageDate.IsEmpty)
					{
						splitMsgLine.Populate(splitMsgLine.GetNewSegmentGroup(message), LineAction.Amend);
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
