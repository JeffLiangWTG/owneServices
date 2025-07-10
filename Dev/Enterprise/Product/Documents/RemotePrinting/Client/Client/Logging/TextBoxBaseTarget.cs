using System.Windows.Forms;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Enterprise.RemotePrinting.Client
{
	[Target("TextBoxBaseTarget")]
	public sealed class TextBoxBaseTarget : TargetWithLayout
	{
		public TextBoxBaseTarget(TextBoxBase textBox)
		{
			this.TextBoxBaseControl = textBox;
		}

		[RequiredParameter]
		public TextBoxBase TextBoxBaseControl { get; set; }

		protected override void Write(LogEventInfo logEvent)
		{
			var message = this.Layout.Render(logEvent);

			if (TextBoxBaseControl.MaxLength < TextBoxBaseControl.Text.Length + message.Length)
			{
				TextBoxBaseControl.Text = "";
			}

			TextBoxBaseControl.AppendText(message + System.Environment.NewLine);
			TextBoxBaseControl.ScrollToCaret();
		}
	}
}
