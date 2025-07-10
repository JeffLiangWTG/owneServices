using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	[SuppressFormsLocalizedTest]
	sealed partial class ActionLog : ZUserControl, ILogger
	{
		public ActionLog()
		{
			hyperlinkActions = new HyperlinkActionCollection();

			InitializeComponent();
			logTextBox.LinkClicked += RichTextActionManager.CreateClickHandler(hyperlinkActions, true);
		}

		#region Properties

		[Browsable(false)]
		[Bindable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableDebugLogging
		{
			get { return debugLoggingLabel.Visible; }
			set { debugLoggingLabel.Visible = value; }
		}

		#endregion

		#region IOperationalActionSectionLog Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void Notify(string text, bool isError)
		{
			RtfStringBuilder builder = new RtfStringBuilder(hyperlinkActions);
			if (isError)
			{
				builder.SetForgroundColour(Color.DarkRed);
			}
			builder.Append(text);
			builder.AppendNewLine();
			builder.SetForgroundColour(null);

			RichTextActionManager.AppendBuilderContent(logTextBox, builder);

			Application.DoEvents();
		}

		#endregion

		readonly HyperlinkActionCollection hyperlinkActions;
	}
}
