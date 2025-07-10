using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D23A.Segments;
using Enterprise.Edifact.Generic.V4;

namespace Enterprise.Customs.AE.Business;

public static class Utils
{
	public static string GetFormattedEDIFactText(string text)
	{
		var segmentDelimiter = AECharacterSet.New().SegmentDelimiter;
		return Regex.Replace(text, $@"(?<!\?){segmentDelimiter}", $"{segmentDelimiter}" + System.Environment.NewLine).Trim();
	}

	public static IEnumerable<string> SplitString(this string text, int chunkSize)
	{
		var textLength = text.Length;
		for (var i = 0; i < textLength; i += chunkSize)
		{
			yield return text.Substring(i, Math.Min(chunkSize, textLength - i));
		}
	}

	public static IEnumerable<ZString> SplitSegment(ZString messageText, AECharacterSet characterSet)
	{
		if (messageText.IsEmpty)
		{
			return Array.Empty<ZString>();
		}

		var interchangeBody = messageText.Replace("\r", "").Replace("\n", "");
		return interchangeBody.SplitIgnoringEscapedDelimiter(characterSet.SegmentDelimiterChar, characterSet.EscapeCharacterChar, leaveSplitCharacter: true);
	}

	public static UNBSegment RetrieveUNBSegment(string message)
	{
		var unbSegment = new UNBSegment();
		try
		{
			var charset = AECharacterSet.New();
			var segments = SplitSegment(message, charset);
			var uNB = segments.First(x => x.StartsWith("UNB"));
			unbSegment.Parse(charset, uNB.Substring(0, uNB.Length - 1));
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
		}
		return unbSegment;
	}

	public static BGMSegment RetrieveBGMSegment(string message)
	{
		var bgmSegment = new BGMSegment();
		try
		{
			var charset = AECharacterSet.New();
			var segments = SplitSegment(message, charset);
			var bgm = segments.First(x => x.StartsWith("BGM"));
			bgmSegment.Parse(charset, bgm.Substring(0, bgm.Length - 1));
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
		}
		return bgmSegment;
	}

	public static RFFSegment RetrieveRFFSegment(string message)
	{
		var rffSegment = new RFFSegment();
		try
		{
			var charset = AECharacterSet.New();
			var segments = SplitSegment(message, charset);
			var rff = segments.First(x => x.StartsWith("RFF"));
			rffSegment.Parse(charset, rff.Substring(0, rff.Length - 1));
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
		}
		return rffSegment;
	}

	public static GEISegment RetrieveGEISegment(string message)
	{
		var geiSegment = new GEISegment();
		try
		{
			var charset = AECharacterSet.New();
			var segments = SplitSegment(message, charset);
			var gei = segments.First(x => x.StartsWith("GEI"));
			geiSegment.Parse(charset, gei.Substring(0, gei.Length - 1));
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
		}
		return geiSegment;
	}
}
