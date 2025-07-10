using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class ChartSectionConfigurationControl : ZUserControl
	{
		readonly ZLabel alternativeLabel;

		public ChartSectionConfigurationControl()
		{
			if (!ObjectFactory.Get<IBMSRegistry>().EnableMENTSections)
			{
				alternativeLabel = new ZLabel();
				alternativeLabel.Text = Res.GetString("CCF901D7-04EE-4E64-82DC-8761FCAB1E43", "MENT sections are no longer supported.");
				alternativeLabel.Dock = DockStyle.Fill;
				alternativeLabel.TextAlign = ContentAlignment.MiddleCenter;
				Controls.Add(alternativeLabel);
			}
			else
			{
				InitializeComponent();
			}
		}

		void ChartSectionConfigurationControl_Load(object sender, EventArgs e)
		{
			EnableConfiguration(DataSource as ChartSectionConfiguration);

			var parentControl = this.GetParent<BoardSectionConfigControl>();
			if (parentControl != null)
			{
				parentControl.SectionConfigChanged += SectionConfigControl_SectionConfigChanged;
			}

			var chartSectionConfiguration = DataSource as ChartSectionConfiguration;
			if (chartSectionConfiguration != null)
			{
				chartSectionConfiguration.ExtractionPKInfo.ValueChanged += ExtractionPK_ValueChanged;
				overrideVisualisationCheckBox.Enabled = !chartSectionConfiguration.ExtractionPK.IsEmpty;
			}

			extractionZGuidFindBox.Validated += EnableConfiguration;
		}

		void ExtractionPK_ValueChanged(object sender, EventArgs e)
		{
			var chartSectionConfiguration = DataSource as ChartSectionConfiguration;
			if (chartSectionConfiguration != null)
			{
				overrideVisualisationCheckBox.Enabled = chartSectionConfiguration.Extraction != null;
				EnableConfiguration(DataSource as ChartSectionConfiguration);
			}
		}

		void SectionConfigControl_SectionConfigChanged(object sender, EventArgs e)
		{
			var parentControl = sender as BoardSectionConfigControl;

			if (parentControl != null)
			{
				var selectedSection = parentControl.GetSelectedSection();
				if (selectedSection != null)
				{
					EnableConfiguration(DataSource as ChartSectionConfiguration);
				}
			}
		}

		void OverrideVisualisationCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			EnableConfiguration(DataSource as ChartSectionConfiguration);
		}

		#region Implementation

		void EnableConfiguration(object sender, EventArgs e)
		{
			EnableConfiguration(DataSource as ChartSectionConfiguration);
		}

		void EnableConfiguration(ChartSectionConfiguration configuration)
		{
			if (configuration != null)
			{
				var readOnly = !configuration.OverrideDefaultVisualisation;
				configuration.RelatedVisualisation?.SetReadOnlyIncludingChildren(readOnly);
			}
		}

		#endregion

	}
}
