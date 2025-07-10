using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecSpecialMentionDataProvider : IEdecSpecialMention
{
	public static IEnumerable<EdecSpecialMentionDataProvider> NewCollection(IEnumerable<ISpecialMentions> specialMentionsList)
	{
		if (specialMentionsList != null)
		{
			var sequenceNumber = 0;

			foreach (var specialMentions in specialMentionsList)
			{
				if (specialMentions != null && !specialMentions.SpecialMentions.IsEmpty)
				{
					foreach (var line in SpecialMentionsHelper.SplitIntoLines(specialMentions.SpecialMentions))
					{
						if (!line.IsEmpty)
						{
							yield return new EdecSpecialMentionDataProvider() { SequenceNumber = ++sequenceNumber, Text = line };
							if (sequenceNumber >= SpecialMentionsHelper.MaxLines)
							{
								yield break;
							}
						}
					}
				}
			}
		}
	}

	public int SequenceNumber { get; private set; }

	public string Text { get; private set; }
}
