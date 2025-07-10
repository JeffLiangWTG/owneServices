using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class CUSCARMessageInterpretation : HtmlTableCreator
{
	public CUSCARMessageInterpretation(SegmentGroup message, UNCharacterSet characterSet)
		: base(new string[2]
		{
			Res.GetString("3d287e4b-5df9-4b45-bfb3-f734a179bb45", "Line No."),
			Res.GetString("fcb78850-1b24-4b41-a423-d776c3de7578", "Segment")
		})
	{
		this.message = message;
		this.characterSet = characterSet;
	}
	readonly SegmentGroup message;
	readonly UNCharacterSet characterSet;

	public new string ToHtml()
	{
		var segments = GetSegements(message);
		ToHtml(segments);
		return "<br>" + base.ToHtml();
	}

	void ToHtml(IEnumerable<Segment> segments)
	{
		foreach(var line in segments.Select((e, i) => (segment: e, lineNo: i + 1)))
		{
			WriteRow(line.lineNo, line.segment.ToString(characterSet));
		}
	}

	IEnumerable<Segment> GetSegements(SegmentGroup group)
	{
		var messageSections = group.MessageSections;
		for (var i = 0; i < messageSections.Length; i++)
		{
			var enumerable = (IEnumerable)messageSections[i];
			foreach (var item in enumerable)
			{
				if (item is Segment segment)
				{
					yield return segment;
				}
				else if (item is SegmentGroup group2)
				{
					foreach(var item2 in GetSegements(group2))
					{
						yield return item2;
					}
				}
			}
		}
	}
}
