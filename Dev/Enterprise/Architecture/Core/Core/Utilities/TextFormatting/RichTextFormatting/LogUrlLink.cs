using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core
{
	[System.Diagnostics.DebuggerDisplay("{Text}({Url})")]
	public sealed class LogUrlLink : LogHyperlink, IEquatable<LogUrlLink>
	{
		public LogUrlLink(Uri url)
			: this(url != null ? url.OriginalString : null, url) { }

		public LogUrlLink(string text, Uri url)
			: base(text)
		{
			Argument.NotNull(url, "url");
			this.url = url;
		}

		public Uri Url
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return url; }
		}

		public bool Equals(LogUrlLink other)
		{
			return other != null
				&& other.Text == Text
				&& other.url == url;
		}
		public sealed override bool Equals(LogHyperlink other)
		{
			return Equals(other as LogUrlLink);
		}
		public sealed override bool Equals(object obj)
		{
			return Equals(obj as LogUrlLink);
		}
		public sealed override int GetHashCode()
		{
			return Text.GetHashCode() ^ url.GetHashCode();
		}

		readonly Uri url;
	}
}
