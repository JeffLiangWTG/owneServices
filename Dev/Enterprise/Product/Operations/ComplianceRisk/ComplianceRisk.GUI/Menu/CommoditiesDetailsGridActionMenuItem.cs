using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.GUI
{
	public class CommoditiesDetailsGridActionMenuItem
	{
		ZMenuItem updateRiskStatusToBlockedMenuItem;
		ZMenuItem updateRiskStatustoReleasedMenuItem;

		readonly ZGrid commoditiesDetailsGrid;
		readonly ComplianceRiskPlugInBusinessObject plugInBizO;

		public CommoditiesDetailsGridActionMenuItem(ComplianceRiskPlugInBusinessObject plugInBizO, ZGrid commoditiesDetailsGrid)
		{
			this.plugInBizO = plugInBizO;
			this.commoditiesDetailsGrid = commoditiesDetailsGrid;
		}

		public void AddMenuItems()
		{
			if (plugInBizO.ComplianceItemRiskStatusProvider.ComplianceRiskSupport.IsSupportInitialization())
			{
				updateRiskStatusToBlockedMenuItem = new ZMenuItem(ResString.GetMultilingualString("41F73077-8A0F-43C2-942A-4480FDDC3D18", "Set Commodity Risk Status to Blocked"));
				updateRiskStatusToBlockedMenuItem.Click += updateRiskStatusToBlockedMenuItem_Click;
				updateRiskStatusToBlockedMenuItem.Enabled = false;

				updateRiskStatustoReleasedMenuItem = new ZMenuItem(ResString.GetMultilingualString("1E7D4948-1B8F-4EDF-B8AB-E117C696BCC0", "Set Commodity Risk Status to Released"));
				updateRiskStatustoReleasedMenuItem.Click += updateRiskStatustoReleasedMenuItem_Click;
				updateRiskStatustoReleasedMenuItem.Enabled = false;

				var actionMenuItems = new ZMenuItem(ResString.GetMultilingualString("F5B03FE4-89BC-442E-90F1-2721ACB010BA", "Actions"));
				actionMenuItems.MenuItems.AddRange([updateRiskStatusToBlockedMenuItem, updateRiskStatustoReleasedMenuItem]);

				commoditiesDetailsGrid.ContextMenu.MenuItems.Add(actionMenuItems);
				commoditiesDetailsGrid.SelectedRowsChangedInMouseDown += UpdateMenuItem_OnSelectedElements;
			}
		}

		void updateRiskStatustoReleasedMenuItem_Click(object sender, EventArgs e)
		{
			UpdateCommoditiesRiskStatusWithChangeCheck(Codes.Released);
		}

		void updateRiskStatusToBlockedMenuItem_Click(object sender, EventArgs e)
		{
			UpdateCommoditiesRiskStatusWithChangeCheck(Codes.Blocked);
		}

		void UpdateCommoditiesRiskStatusWithChangeCheck(string newRiskStatus)
		{
			if (plugInBizO.HostBusinessEntity.HasChanges)
			{
				var updateStatus = newRiskStatus == Codes.Released ? Descriptions.Released : Descriptions.Blocked;
				Globals.Message.ShowInformation(Res.GetString("C2CC6E1F-51CD-45B1-8038-26BA9AD808DB", "Please save the form before updating the Commodity Risk Status to {0}.", updateStatus));
			}
			else if (!(plugInBizO.HostBusinessEntity as IComplianceJobDirectionProvider).IsInternational)
			{
				Globals.Message.ShowInformation(Res.GetString("488F530E-7A47-4E3B-A3A7-6A0D33CE97CB", "Compliance Assessment is not available on this Job."));
			}
			else
			{
				UpdateCommoditiesRiskStatus(newRiskStatus);
			}
		}

		void UpdateCommoditiesRiskStatus(string newRiskStatus)
		{
			if (!ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError(plugInBizO.HostBusinessEntity, showErrorWhenNotAllowed: true))
			{
				return;
			}

			var notPreceedStatus = newRiskStatus == Codes.Released ? Codes.Clear : Codes.Blocked;
			var warningMessage = Res.GetString("7EF0ED36-65E0-4883-B4FF-F0C19993191E", "Are you sure you want to run this action on {0} commodities?", commoditiesDetailsGrid.SelectedElements.Length);
			var clearMessage = Res.GetString("26DA65AA-4747-475F-84C1-D1D61612C87F", $@"Commodities marked as ""Clear"" are unable to be updated to ""Released"".
Do you wish to proceed and update the other commodities?");
			var blockedMessage = Res.GetString("2BB8C8FA-F5F5-4CA6-9DA2-783B4889BA5A", $@"Commodities marked as ""Blocked"" are unable to be updated to ""Blocked"".
Do you wish to proceed and update the other commodities?");

			if (Globals.Message.Show(warningMessage, Res.GetString("C7C5CBBE-9310-4B78-9283-45DD9019EF5C", "Bulk Assessment"), ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Warning) == ZDialogResult.OK)
			{
				if (commoditiesDetailsGrid.SelectedElements.Any(x => ((ComplianceCommodityDetail)x).CCD_RiskStatus == notPreceedStatus)
					&& Globals.Message.Show(newRiskStatus == Codes.Blocked ? blockedMessage : clearMessage, Res.GetString("8C949995-1DA6-4244-9111-9F2E1351C7BD", "Information"), ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Warning) == ZDialogResult.Cancel)
				{
					return;
				}

				var relatedJobCount = 0;

				foreach (var item in commoditiesDetailsGrid.SelectedElements.Cast<ComplianceCommodityDetail>())
				{
					if (item.CommodityType != CommodityType.RelatedJobLink)
					{
						item.CCD_RiskStatus = item.CCD_RiskStatus == notPreceedStatus ? item.CCD_RiskStatus : newRiskStatus;
						continue;
					}
					relatedJobCount++;
				}

				if (relatedJobCount > 0)
				{
					Globals.Message.Show(Res.GetString("B44ACF80-3AE3-438F-8B54-9261CA9AFD40", "{0} commodities belong to a related job are unable to be updated.", relatedJobCount));
				}

				ZExceptionReporting.ProcessWithSaveExceptionHandling(() => { plugInBizO.HostBusinessEntity.Factory.Save(); }, null);
			}
		}

		void UpdateMenuItem_OnSelectedElements(object sender, EventArgs e)
		{
			var statuses = commoditiesDetailsGrid.SelectedElements.Select(x => ((ComplianceCommodityDetail)x).CCD_RiskStatus);
			if (!plugInBizO.ComplianceRiskStatus.IsAssessmentInitialized || !statuses.Any())
			{
				updateRiskStatustoReleasedMenuItem.Enabled = false;
				updateRiskStatusToBlockedMenuItem.Enabled = false;
			}
			else if (statuses.All(x => x == Codes.PotentialRisk))
			{
				updateRiskStatustoReleasedMenuItem.Enabled = true;
				updateRiskStatusToBlockedMenuItem.Enabled = true;
			}
			else if (statuses.All(x => x == Codes.Clear || x == Codes.Released))
			{
				updateRiskStatustoReleasedMenuItem.Enabled = false;
				updateRiskStatusToBlockedMenuItem.Enabled = true;
			}
			else if (statuses.All(x => x == Codes.Blocked || x == Codes.PotentialRisk))
			{
				updateRiskStatustoReleasedMenuItem.Enabled = true;
				updateRiskStatusToBlockedMenuItem.Enabled = false;
			}
			else
			{
				updateRiskStatustoReleasedMenuItem.Enabled = true;
				updateRiskStatusToBlockedMenuItem.Enabled = true;
			}
		}
	}
}
