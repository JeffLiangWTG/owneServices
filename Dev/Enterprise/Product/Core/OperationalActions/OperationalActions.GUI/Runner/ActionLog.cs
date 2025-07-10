using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	[SuppressFormsLocalizedTest]
	internal sealed partial class ActionLog : ZUserControl, IOperationalActionLog
	{
		public ActionLog()
		{
			hyperlinkActions = new HyperlinkActionCollection();

			InitializeComponent();
			logTextBox.LinkClicked += RichTextActionManager.CreateClickHandler(hyperlinkActions, true);
		}

		void Clear()
		{
			logTextBox.Clear();
			hyperlinkActions.Clear();
		}

		static Color? ColourForErrorLevel(OperationalActionLogErrorLevel errorLevel)
		{
			switch (errorLevel)
			{
				case OperationalActionLogErrorLevel.Debug: return Color.DarkViolet;
				case OperationalActionLogErrorLevel.Success: return Color.Green;
				case OperationalActionLogErrorLevel.Warning: return Color.DarkOrange;
				case OperationalActionLogErrorLevel.Error: return Color.DarkRed;
				default: return null;
			}
		}

		#region Properties

		[Browsable(false)]
		[Bindable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowMainProgressBar
		{
			get
			{
				return masterProgressBar.Visible;
			}
			set
			{
				masterProgressBar.Visible = value;
				spacer1.Visible = value;
			}
		}

		[Browsable(false)]
		[Bindable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowSectionProgressBar
		{
			get
			{
				return sectionProgressBar.Visible;
			}
			set
			{
				sectionProgressBar.Visible = value;
				spacer1.Visible = value;
			}
		}

		[Browsable(false)]
		[Bindable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableDebugLogging
		{
			get { return debugLoggingLabel.Visible; }
			set { debugLoggingLabel.Visible = value; }
		}

		#endregion

		#region IOperationalActionLog Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void SetMasterProgressMax(int max)
		{
			Clear();
			highestErrorLevelEncountered = OperationalActionLogErrorLevel.Informational;

			masterProgressBar.Value = 0;
			masterProgressBar.Maximum = max;

			sectionProgressBar.Value = 0;
			sectionProgressBar.Maximum = 0;

			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void BumpMasterProgress()
		{
			masterProgressBar.Value++;

			int value = (masterProgressBar.Value < masterProgressBar.Maximum) ? 0 : 1;
			sectionProgressBar.Maximum = value;
			sectionProgressBar.Value = value;

			Application.DoEvents();
		}

		public OperationalActionLogErrorLevel HighestErrorLevelEncountered
		{
			get { return highestErrorLevelEncountered; }
		}

		#endregion

		#region IOperationalActionSectionLog Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void SetSectionProgressMax(int max)
		{
			sectionProgressBar.Value = 0;
			sectionProgressBar.Maximum = max;

			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void BumpSectionProgress()
		{
			sectionProgressBar.Value++;

			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
		{
			if (errorLevel >= OperationalActionLogErrorLevel.Informational || EnableDebugLogging)
			{
				RtfStringBuilder builder = new RtfStringBuilder(hyperlinkActions);
				builder.SetForgroundColour(ColourForErrorLevel(errorLevel));
				builder.AppendFormat(format, args);
				builder.AppendNewLine();
				builder.SetForgroundColour(null);
				 
				RichTextActionManager.AppendBuilderContent(logTextBox, builder);
				if (errorLevel > highestErrorLevelEncountered)
				{
					highestErrorLevelEncountered = errorLevel;
				}

				Application.DoEvents();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void Notify(OperationalActionLogErrorLevel errorLevel, string text)
		{
			if (errorLevel >= OperationalActionLogErrorLevel.Informational || EnableDebugLogging)
			{
				RtfStringBuilder builder = new RtfStringBuilder(hyperlinkActions);
				builder.SetForgroundColour(ColourForErrorLevel(errorLevel));
				builder.Append(text);
				builder.AppendNewLine();
				builder.SetForgroundColour(null);

				RichTextActionManager.AppendBuilderContent(logTextBox, builder);

				if (errorLevel > highestErrorLevelEncountered)
				{
					highestErrorLevelEncountered = errorLevel;
				}

				Application.DoEvents();
			}
		}

		#endregion

		OperationalActionLogErrorLevel highestErrorLevelEncountered;
		readonly HyperlinkActionCollection hyperlinkActions;
	}
}

#region Test
#if DEBUG

#region Debug Members

namespace Enterprise.Services.OperationalActions.GUI
{
	partial class ActionLog
	{
		public override ISite Site
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Site; }
			set
			{
				base.Site = value;

				if (value != null && value.DesignMode)
				{
					RtfStringBuilder builder = new RtfStringBuilder(hyperlinkActions);

					OperationalActionLogErrorLevel[] levels = (OperationalActionLogErrorLevel[])Enum.GetValues(typeof(OperationalActionLogErrorLevel));
					Array.Sort(levels);

					foreach (OperationalActionLogErrorLevel level in levels)
					{
						builder.SetForgroundColour(ColourForErrorLevel(level));
						builder.Append(string.Format("'{0}' text.", level));
						builder.SetForgroundColour(null);
						builder.AppendNewLine();
					}

					logTextBox.Clear();
					#if !WINZOR
					logTextBox.Rtf = builder.ToString();
					#else
					logTextBox.Html = builder.ToString();
					#endif
				}
			}
		}
	}
}

#endregion

#endif
#endregion
