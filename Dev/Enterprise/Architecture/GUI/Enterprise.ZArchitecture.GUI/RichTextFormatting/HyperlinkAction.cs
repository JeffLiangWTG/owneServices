using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	internal abstract class HyperlinkAction<T> : IHyperlinkAction
		where T : LogHyperlink
	{
		public HyperlinkAction(T hyperlink, string key)
		{
			if (hyperlink == null)
			{
				throw new ArgumentNullException(nameof(hyperlink));
			}

			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be null or empty", nameof(key));
			}

			this.hyperlink = hyperlink;
			this.key = key;
		}

		public string Key
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return key; }
		}

		public T Hyperlink
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return hyperlink; }
		}

		public abstract void DoAction();

		#region IHyperlinkAction Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		LogHyperlink IHyperlinkAction.Hyperlink
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return Hyperlink; }
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string key;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly T hyperlink;
	}
}
