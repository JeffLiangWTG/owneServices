using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class HyperlinkActionFactory
	{
		public static IHyperlinkAction New(LogHyperlink hyperlink, string key)
		{
			if (hyperlink == null)
			{
				throw new ArgumentNullException(nameof(hyperlink));
			}

			{
				var link = hyperlink as LogControllerLink;
				if (link != null)
				{
					return new ControllerAction(link, key);
				}
			}

			{
				var link = hyperlink as LogUrlLink;
				if (link != null)
				{
					return new UrlAction(link, key);
				}
			}

			throw new ArgumentException("dont know how to handle " + hyperlink.GetType());
		}
	}
}
