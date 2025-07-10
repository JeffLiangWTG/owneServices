using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Aggregator;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.GUI.ComplianceReport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module
{
	public class AccComplianceReportModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccComplianceReport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Accountant; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ComplianceReports; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccComplianceReportFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccComplianceReportFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccComplianceReportCollection(Factory);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccComplianceReport);
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.AccComplianceReportCode; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			var reportTypeList = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Cast<ComplianceReportConfiguration>()
				.OrderByDescending(x => x.IsDefaultReportType)
				.Select(x => new {
					typeCode = x.ReportCode,
					description = x.ReportTitle
				});

			foreach (var reportType in reportTypeList)
			{
				var code = reportType.typeCode;
				var description = ResString.GetMultilingualString("a4759ee8-2d4a-4dee-97e0-e18e149531e6", "New {0}", reportType.description);
				NewMenuItem.MenuItems.Add(new ZMenuItem(description, new EventHandler((sender, e) => HandleNewReportByType(sender, e, code))));
			}
			
			return menuItems.ToArray();
		}

		protected void HandleNewReportByType(object sender, EventArgs e, ZString typeCode)
		{
			var controller = (AccComplianceReportController)ZControllerFactory.Create(ControllerIDs.AccComplianceReport);
			controller.ShowNewForm(typeCode);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.ReQueueMenuItemText, new EventHandler(HandleReQueue)));
			menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.GenerateMenuItemText, new EventHandler(HandleGenerate)));
			menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.FinalizeMenuItemText, new EventHandler(HandleFinalize)));

			if (AccountingUtils.DoesCountrySupportSAFTGeneration)
			{
				menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.GenerateSAFTXmlMenuItemText, new EventHandler(HandleGenerateSAFTXml)));
				menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.GenerateAnnualSAFTXmlMenuItemText, new EventHandler(HandleGenerateAnnualSAFTXml)));
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy)
			{
				menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.GenerateEsterometroXmlMenuItemText, new EventHandler(HandleGenerateEsterometroXml)));
			}
			else if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.Taiwan)
			{
				menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.ExportVATMenuItemText, new EventHandler(HandleExportVATDataFile)));
			}
			else if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.Israel)
			{
				menuItems.Add(new ZMenuItem(AccComplianceReportGuiHelper.ExportOpenFormatFileLabel, new EventHandler(HandleExportOpenFormatFile)));
			}

			return menuItems.ToArray();
		}

		AccComplianceReport CurrentlySelectedReport
		{
			get
			{
				return Grid.ListManager.Count > 0 ? Grid.ListManager.GetCurrent() as AccComplianceReport : null;
			}
		}

		void HandleReQueue(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				if (CurrentlySelectedReport.IsUsingGLDTablePrefix)
				{
					Globals.Message.Show(Res.GetString("3258770C-1500-4F20-AF7E-377069E82C23", "The compliance report is generated using accounting journals data source. The queue and re-queue actions are not applicable."),
										 Res.GetString("5abe7eb4-abd2-41a8-9a86-81d5fbd7215f", "Re-Queue Report"),
										 MessageBoxButtons.OK,
										 MessageBoxIcon.Warning);
				}
				else
				{
					CurrentlySelectedReport.Reload();
					CurrentlySelectedReport.HandleReQueue();
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("09872555-8f56-45ad-bc08-4480cd4e637e", "Please select a Report to re-queue."));
			}
		}

		void HandleGenerate(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				CurrentlySelectedReport.Reload();
				CurrentlySelectedReport.HandleGenerate();
			}
			else
			{
				Globals.Message.Show(Res.GetString("419ec1aa-2e4f-4247-a9b2-5ffa49227de3", "Please select a Report to generate."));
			}
		}

		void HandleGenerateSAFTXml(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				PerformAggregationBeforeGenerate();
				CurrentlySelectedReport.Reload();
				CurrentlySelectedReport.HandleGenerateSAFTXml(LocateMainForm());
			}
			else
			{
				Globals.Message.Show(Res.GetString("edb9c4c9-40a3-4f37-ae47-02b83b10204f", "Please select a Report to generate Monthly Transaction XML."));
			}
		}

		void HandleGenerateAnnualSAFTXml(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				PerformAggregationBeforeGenerate();
				CurrentlySelectedReport.Reload();
				CurrentlySelectedReport.HandleGenerateAnnualSAFTXml(LocateMainForm());
			}
			else
			{
				Globals.Message.Show(Res.GetString("162fdd86-e3f1-4ef2-803b-c2bcb0608d1b", "Please select a Report to generate Annual SAFT XML."));
			}
		}

		void PerformAggregationBeforeGenerate()
		{
			if (((ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get())?.IsNeedAggregateBeforeGenerate ?? false)
			{
				new AggregateController().PerformAggregationIfRequired();
			}
		}

		void HandleGenerateEsterometroXml(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				CurrentlySelectedReport.Reload();
				CurrentlySelectedReport.HandleGenerateEsterometroXml(LocateMainForm());
			}
			else
			{
				Globals.Message.Show(Res.GetString("dd0adc42-be6a-4699-8169-0b8844514028", "Please select a Report to generate Esterometro XML."));
			}
		}

		void HandleExportOpenFormatFile(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				CurrentlySelectedReport.Reload();
				CurrentlySelectedReport.HandleExportOpenFormatFile(LocateMainForm());
			}
			else
			{
				Globals.Message.Show(Res.GetString("eef41de2-8f88-43ea-91e7-946cebd6ff6f", "Please select a report to export open format files."));
			}
		}

		void HandleFinalize(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				CurrentlySelectedReport.Reload();
				CurrentlySelectedReport.HandleFinalize();
			}
			else
			{
				Globals.Message.Show(Res.GetString("5973bb33-128b-483d-a209-ed1247f13487", "Please select a Report to finalize."));
			}
		}

		void HandleExportVATDataFile(object sender, EventArgs e)
		{
			if (CurrentlySelectedReport != null)
			{
				CurrentlySelectedReport.Reload();
				switch (CurrentlySelectedReport.ACR_ReportType)
				{
					case ComplianceReportTypes.PurchaseAndSalesVATForTW:
						CurrentlySelectedReport.HandleExportPurchaseAndSalesVATDataFile(LocateMainForm());
						break;
					case ComplianceReportTypes.ZeroRatedSalesVATForTW:
						CurrentlySelectedReport.HandleExportZeroRatedSalesVATDataFile(LocateMainForm());
						break;
					default:
						break;
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("D3570A58-2607-4DC3-AED6-D4E6973DE905", "Please select a Report to export."));
			}
		}
	}
}


