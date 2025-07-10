using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class SetAsFailedFromTransmissionMenuItemCreator
	{
		public SetAsFailedFromTransmissionMenuItemCreator(IReportsGridUserControlProvider provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
		}
		readonly IReportsGridUserControlProvider provider;

		public ZMenuItem Create() => new ZMenuItem(ResString.GetMultilingualString("1783E439-2871-4E53-9DB0-0822880EAA29", "Set Entry as Failed From Transmission"), SetAsFailedFromTransmission_Click);

		void SetAsFailedFromTransmission_Click(object sender, EventArgs e)
		{
			var reportsGrid = provider.UserControl.ReportsGrid;

			var selectedElements = reportsGrid.SelectedElements;
			if (selectedElements.Length == 0)
			{
				Globals.Message.Show(CommonPromptMessages.SelectARowMessage);
			}
			else
			{
				var hasConfirmed = false;
				var reportsChanged = 0;

				var selectedElementsLength = selectedElements.Length;

				var confirmationMessage = GetConfirmationMessage(selectedElementsLength);
				var caption = ResString.GetMultilingualString("40802B3B-6F60-46D2-A8C8-52BA9C01B229", "Failed from Transmission");
				var confirmationPrompt = GetConfirmationPromptMessage(selectedElementsLength);
				var confirmationString = ResString.GetMultilingualString("D8BEF1F0-5B59-4689-8FEB-CFE2ABED1200", "yes");

				foreach (CusExitReport report in selectedElements)
				{
					if (report.CER_MessageStatus == LogicalStatusList.Codes.Sent
						&& (hasConfirmed || Globals.Message.ShowConfirmation(confirmationMessage, caption, confirmationPrompt, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK))
					{
						report.CER_MessageStatus = LogicalStatusList.Codes.Failed;
						reportsChanged++;
						hasConfirmed = true;
					}
				}
				if (reportsChanged == 1)
				{
					Globals.Message.Show(ResString.GetMultilingualString("AA4D8341-9998-4D26-9C77-7EC6F69BB8A5", "1 Report was set to Failed from Transmission"));
				}
				else
				{
					Globals.Message.Show(ResString.GetMultilingualString("2AEA7E17-61C9-481D-A13F-1C7EF0D82FB7", "{0} Reports were set to Failed from Transmission", reportsChanged.ToString(Culture.Current)));
				}
			}
		}

		ZString GetConfirmationMessage(int elements)
		{
			return elements == 1
				? ResString.GetMultilingualString("D47FF9CA-186E-4349-B3E1-F477B63273F1", "Are you sure you want to set this Report as Failed from Transmission?")
				: ResString.GetMultilingualString("2BCA1225-56F0-44C5-8315-7E764EBEF5AF", "Are you sure you want to set these Reports as Failed from Transmission?");
		}

		ZString GetConfirmationPromptMessage(int elements)
		{
			return elements == 1
				? ResString.GetMultilingualString("B135AD59-B1A7-4A60-B7E8-3B2BAA7D755A", "If you are absolutely sure you want to set this Report as Failed From Transmission, please type:")
				: ResString.GetMultilingualString("A672731E-6A63-4634-919C-6806D0AB3841", "If you are absolutely sure you want to set these Reports as Failed From Transmission, please type:");
		}
	}
}
