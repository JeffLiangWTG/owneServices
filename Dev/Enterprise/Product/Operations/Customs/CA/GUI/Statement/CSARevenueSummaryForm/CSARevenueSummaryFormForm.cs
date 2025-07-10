using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CSARevenueSummaryFormForm : ZTemplateForm, IPostingButtonsProvider
	{
		public CSARevenueSummaryFormForm(CusStatementHeader csaRevenueSummaryForm)
			: base(csaRevenueSummaryForm)
		{
			this.csaRevenueSummaryForm = csaRevenueSummaryForm;
			WorkflowTabPage.Initialize(csaRevenueSummaryForm);
		}

		readonly CusStatementHeader csaRevenueSummaryForm;

		public override string FormCaption => Res.GetString("0548172E-70F8-4AC5-8E7B-CA023706C4CB", "CSA Revenue Summary Form");

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			messagingMenu = new ZMenuItem(ResString.GetMultilingualString("AD65BB24-BEDE-43DF-9878-6B5E1A469B1A", "Messaging"));
			MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, messagingMenu);
			sendOriginalRSFMessageMenu = new ZMenuItem(ResString.GetMultilingualString("2171A190-5928-4C42-BC99-957704D40370", "Send Original RSF Message"), new EventHandler(SendOriginalRSFMessageMenu_Clicked));
			sendAdjustmentRSFMessageMenu = new ZMenuItem(ResString.GetMultilingualString("99A828DF-8F52-436B-A25D-E2C3FF79C874", "Send Adjustment RSF Message"), new EventHandler(SendAdjustmentRSFMessageMenu_Clicked));
			messagingMenu.MenuItems.Add(sendOriginalRSFMessageMenu);
			messagingMenu.MenuItems.Add(sendAdjustmentRSFMessageMenu);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		ZMenuItem messagingMenu;
		ZMenuItem sendOriginalRSFMessageMenu;
		ZMenuItem sendAdjustmentRSFMessageMenu;
		readonly ZString messageBoxCaption = "Success!";

		void SendOriginalRSFMessageMenu_Clicked(object sender, EventArgs e)
		{
			if (SaveJob())
			{
				new CSARevenueSummaryFormMessageManager(csaRevenueSummaryForm, MessageSubTypes.Create).SendMessage();
				ShowSuccessInformation();
			}
		}

		void SendAdjustmentRSFMessageMenu_Clicked(object sender, EventArgs e)
		{
			if (SaveJob())
			{
				new CSARevenueSummaryFormMessageManager(csaRevenueSummaryForm, MessageSubTypes.Change).SendMessage();
				ShowSuccessInformation();
			}
		}

		void ShowSuccessInformation()
		{
			Globals.Message.ShowInformation(Res.GetString("69CDAAC8-F9CE-45E3-9298-771C634E9F66", "Original CSA Revenue Summary Form message has been created."), messageBoxCaption);
		}

		bool SaveJob()
		{
			return (!csaRevenueSummaryForm?.HasChanges ?? false) ||
				(Globals.Message.Show(Res.GetString("4389FC3A-BA09-4D99-82ED-9AF658DDA267", "The CSA Revenue Summary Form has not yet been saved. Do you want to save and proceed?"), Res.GetString("F55D5B28-AF2C-41F7-A9C9-3435898823F6", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes
				&& FireSaveButton() == ContinueWithSave.Yes);
		}
	}
}
