using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class JobDeclarationForm : BaseJobDeclarationForm
	{
		public JobDeclarationForm()
			: base()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
			declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
			HandleVisibilityWhenJE_MessageTypeIsChanged();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Declaration is JobDeclaration declaration)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				}
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			HandleVisibilityWhenJE_MessageTypeIsChanged();
		}

		void HandleVisibilityWhenJE_MessageTypeIsChanged()
		{
			var plugIn = PlugIns.GetPlugIn(ControllerIDs.Routing);
			if (plugIn != null)
			{
				plugIn.Enabled = JobDeclaration != null && !(JobDeclaration.IsImportLicense || JobDeclaration.IsLPCO);
			}
		}

		JobDeclaration JobDeclaration => Declaration as JobDeclaration;

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (JobDeclaration?.IsImportLicense ?? false)
			{
				var unsplitter = new ImportLicenseEntryInstructionUnsplitter(JobDeclaration);
				if (unsplitter.HasEntryInstructionsNeedToUnsplit())
				{
					var message = Res.GetString("F84B5657-475D-461F-9538-70281366CD9E", "Any change made to an already split Entry Instruction will cause the system to revert the changes. Do you want to proceed?");
					var caption = Res.GetString("120f3ce7-6ea5-45f8-8ef7-3f37194c3b28", "Undo Split");
					var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

					if (result == DialogResult.Yes)
					{
						unsplitter.UnsplitEntryInstructions();
					}
					return ContinueWithSave.No;
				}
			}
			return base.ShowPreSaveDialogs();
		}

		protected override Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsController() => new SendsMessagesToCustomsGUI();

		protected override void AddPlugins()
		{
			if (JobDeclaration != null && (JobDeclaration.IsImportLicense || JobDeclaration.IsLPCO))
			{
				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, RoutingTabIndex);
				PlugIns.Add(ControllerIDs.LandedCosting);
				PlugIns.AddJobInvoicing(Declaration.InvoicingSupporter);
				PlugIns.Add(ControllerIDs.DocAddresses);
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);
				PlugIns.Add(ControllerIDs.DtbBooking);
			}
			else
			{
				base.AddPlugins();
			}
		}
	}
}
