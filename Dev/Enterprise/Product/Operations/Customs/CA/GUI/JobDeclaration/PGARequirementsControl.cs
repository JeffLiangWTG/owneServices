using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class PGARequirementsControl : ZUserControl
	{
		public PGARequirementsControl()
		{
			InitializeComponent();
			programCodesGrid.AfterBind += ProgramCodesGrid_AfterBind;
		}

		void ProgramCodesGrid_AfterBind(object sender, EventArgs e)
		{
			programCodesGrid.ListManager.CurrentChanged -= UpdateCurrentPGAProgramRequirement;
			programCodesGrid.ListManager.CurrentChanged += UpdateCurrentPGAProgramRequirement;

			UpdateCurrentPGAProgramRequirement(sender, e);
		}

		void UpdateCurrentPGAProgramRequirement(object sender, EventArgs e)
		{
			UnHookEvent();

			programRequirement = programCodesGrid.ListManager?.GetCurrent() as PGAProgramRequirement;

			HookEvent();
		}

		PGAProgramRequirement programRequirement;

		void HookEvent()
		{
			if (programRequirement != null)
			{
				programRequirement.ShouldUpdateIndicatorEvent -= ShouldUpdateIndicatorEvent;
				programRequirement.ShouldUpdateIndicatorEvent += ShouldUpdateIndicatorEvent;

				programRequirement.AfterShouldUpdateIndicatorEvent -= AfterShouldUpdateIndicatorEvent;
				programRequirement.AfterShouldUpdateIndicatorEvent += AfterShouldUpdateIndicatorEvent;
			}
		}

		void UnHookEvent()
		{
			if (programRequirement != null)
			{
				programRequirement.ShouldUpdateIndicatorEvent -= ShouldUpdateIndicatorEvent;
				programRequirement.AfterShouldUpdateIndicatorEvent -= AfterShouldUpdateIndicatorEvent;
			}
		}

		bool ShouldUpdateIndicatorEvent(ZString oldValue, ZString newValue, string programDescription)
		{
			var confirmationMessage = Res.GetString("f0d2b3c0-427b-4d57-9807-213f6c76cf2e", "Making this change may delete some existent data on {0}. Do you want to continue?", programDescription);
			return Globals.Message.Show(confirmationMessage, Res.GetString("3c289aa8-9258-4604-b587-f35a71fb75a0", "Do you want to proceed?"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
		}

		void AfterShouldUpdateIndicatorEvent(object sender, EventArgs args)
		{
			programCodesGrid.Focus();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			programCodesGrid.AfterBind -= ProgramCodesGrid_AfterBind;

			var listManager = programCodesGrid.ListManager;

			if (listManager != null)
			{
				listManager.CurrentChanged -= UpdateCurrentPGAProgramRequirement;
			}

			UnHookEvent();

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
