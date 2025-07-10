using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Messaging
{
	public class SyntaxError : ITableInterpretation
	{
		#region Constructor

		internal SyntaxError(ZString sourceMessage, string errorText)
		{
			if (!TrySetElementError(sourceMessage, errorText) && !TrySetSegmentError(sourceMessage, errorText))
			{
				ErrorMessage = errorText;
			}
		}

		bool TrySetElementError(ZString sourceMessage, string errorText)
		{
			var match = Regex.Match(errorText, @"^SEGMENT(?<Segment>\w+)LINE(?<Line>[0-9]+)(ELE\sPOS|ELEM[0-9]+\[)(?<Position>(?<Element>[0-9]+)[,.](?<SubElement>[0-9]+))\]?(?<Description>.+)$");
			if (!match.Success)
			{
				match = Regex.Match(errorText, @"^UMRN\((?<Msgnum>[0-9]+)\)SEGMENT(?<Segment>\w+)LINE(?<Line>[0-9]+)(ELE\sPOS|ELEM[0-9]+\()(?<Position>(?<Element>[0-9]+)[,.](?<SubElement>[0-9]+))\)?(?<Description>.+)$");
			}
			if (match.Success)
			{
				var segment = match.Groups["Segment"].Value;
				var element = match.Groups["Element"].Value;
				var component = match.Groups["SubElement"].Value;

				LineNumber = match.Groups["Line"].Value;
				ErrorMessage = match.Groups["Description"].Value;
				SegmentPosition = Res.GetString("28721199-c506-49c2-a753-f8dd7e8f5bd9", "{0} / L: {1}, P: {2},{3}", segment, LineNumber, element, component);

				if (!sourceMessage.IsEmpty)
				{
					SourceLine = sourceMessage.Split(Delimiter).ElementAtOrDefault(ZInt.ParseEmptyAsZero(LineNumber));
					if (SourceLine.StartsWith(segment))
					{
						var elementValue = SourceLine.Split('+').ElementAtOrDefault(ZInt.ParseEmptyAsZero(element));
						ComponentValue = elementValue.Split(':').ElementAtOrDefault(ZInt.ParseEmptyAsZero(component) - 1);
					}
				}
			}
			return match.Success;
		}

		bool TrySetSegmentError(ZString sourceMessage, string errorText)
		{
			var match = Regex.Match(errorText, @"^SEGMENT(?<Segment>\w+)(-\s?)?BYTE\sOFFSET(?<ByteOffset>[0-9]+)?(?<Description>.+)?$");
			if (match.Success)
			{
				var byteOffset = ZInt.ParseEmptyAsZero(match.Groups["ByteOffset"].Value);
				var segment = match.Groups["Segment"].Value;

				if (byteOffset < sourceMessage.Length)
				{
					int lineNum = 0, startIndex = 0, index = 0;
					while (index < sourceMessage.Length && (sourceMessage[index] != Delimiter || index < byteOffset))
					{
						if (sourceMessage[index++] == Delimiter)
						{
							lineNum++;
							startIndex = index;
						}
					}

					LineNumber = lineNum.ToString();
					SourceLine = sourceMessage.SubstringSafe(startIndex, index - startIndex);
					byteOffset = byteOffset - startIndex;
				}

				ErrorMessage = match.Groups["Description"].Value;
				SegmentPosition = Res.GetString("fb9d5eda-9c41-4945-8f7c-66d923be6e00", "{0} / L: {1}, I: {2}", segment, LineNumber, byteOffset);
			}
			return match.Success;
		}

		#endregion

		#region Implementation of ITableInterpretation

		string ITableInterpretation.Caption
		{
			get { return Res.GetString("c234aec1-defc-4d7d-b842-b192a18d79f6", "Syntax Error Messages"); }
		}

		IEnumerable<string> ITableInterpretation.Titles
		{
			get
			{
				yield return Res.GetString("589af635-5753-44ab-8fb9-614c7661a5cb", "Error Message");
				yield return Res.GetString("b9e52037-13d7-48ae-a107-13951245f0d2", "Component Value");
				yield return Res.GetString("433fec16-6a9d-47af-8f03-f13e2279a6b9", "Source Line");
				yield return Res.GetString("15761c51-48b9-4303-b14c-aa6f3ef0f68d", "Segment/Position");
			}
		}

		IEnumerable<object> ITableValues.Values
		{
			get { return new object[] { ErrorMessage, ComponentValue, SourceLine, SegmentPosition }; }
		}

		#endregion

		public override string ToString()
		{
			return Res.GetString("6b8b5747-1a47-4929-ac9b-3f6aed0ba135",
								 "Error Message: {0},  Component Value: '{1}', Segment/Position: {2}.",
								 ErrorMessage, ComponentValue, SegmentPosition);
		}

		public ZString LineNumber { get; private set; }
		internal ZString ErrorMessage { get; private set; }
		internal ZString ComponentValue { get; private set; }
		internal ZString SourceLine { get; private set; }
		internal ZString SegmentPosition { get; private set; }

		public const char Delimiter = '\'';
	}
}
