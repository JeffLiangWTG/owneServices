using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.GUI;

public static class MessageTabUserControlHelper
{
	public static string GetHtmlFormattedText(string textToFormat)
	{
		if (textToFormat != null && !textToFormat.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase) && !textToFormat.TrimStart().StartsWith((NoResString)"<?xml", StringComparison.OrdinalIgnoreCase))
		{
			textToFormat = HtmlTemplate.Replace("(*HtmlStyleSheet*)", MessagePrettyFormatterHelper.StyleSheet)
				.Replace("(*HtmlBody*)", textToFormat);
		}
		return textToFormat;
	}

	const string HtmlTemplate = @"<html>
<head>
	<style type='text/css'>
		<!--
			(*HtmlStyleSheet*)
		-->
	</style>
</head>
<body>
	(*HtmlBody*)
</body>
</html>";
}
