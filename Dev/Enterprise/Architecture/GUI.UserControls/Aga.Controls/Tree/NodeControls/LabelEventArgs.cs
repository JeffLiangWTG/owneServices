using System;

namespace Aga.Controls.Tree.NodeControls
{
	public class LabelEventArgs : EventArgs
	{
		private readonly object _subject;
		public object Subject
		{
			get { return _subject; }
		}

		private readonly string _oldLabel;
		public string OldLabel
		{
			get { return _oldLabel; }
		}

		private readonly string _newLabel;
		public string NewLabel
		{
			get { return _newLabel; }
		}

		public LabelEventArgs(object subject, string oldLabel, string newLabel)
		{
			_subject = subject;
			_oldLabel = oldLabel;
			_newLabel = newLabel;
		}
	}
}
