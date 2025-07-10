using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.Customs.EU.GUI.SADH;
using Enterprise.Customs.EU.GUI.SingleLineEntry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("D44C9A21-F4B9-4556-A67A-373E5730DF23", "New Declaration"), HandleNewClick));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("871C70B4-86CF-4906-BB4E-CF50FF84A322", "New Single-Line Export Wizard"), CreateSingleLineEntryExport));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("B6D89869-8346-4BDE-BEFF-1CB7BBCC39D1", "New Single-Line Import Wizard"), CreateSingleLineEntryImport));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("29E35532-7C37-4DA5-8480-1000A4AE05F3", "SAD/H Data Entry Form"), CreateUsingSADHForm));
			}
			return result;
		}

		void CreateUsingSADHForm(object sender, EventArgs e)
		{
			var declaration = Factory.New<JobDeclaration>();
			var dataManager = GetNewSadhManager(declaration);

			using (var sadhEntryForm = new SADHEntryForm(dataManager))
			{
				ZFormModaliser.ShowDialogAndDispose(sadhEntryForm);
			}

			if (dataManager.ExecutedSuccessfully)
			{
				SaveFactoryAndShowDeclarationForEdit(declaration);
			}
		}

		void CreateSingleLineEntryExport(object sender, EventArgs e)
		{
			CreateSingleLineEntry("EXP");
		}

		void CreateSingleLineEntryImport(object sender, EventArgs e)
		{
			CreateSingleLineEntry("IMP");
		}

		void CreateSingleLineEntry(string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var singleLineEntryManager = GetNewSingleLineEntryManager(declaration);

			using (var singleEntryForm = GetSingleLineEntryForm(singleLineEntryManager))
			{
				ZFormModaliser.ShowDialogAndDispose(singleEntryForm);
			}

			if (singleLineEntryManager.ExecutedSuccessfully)
			{
				SaveFactoryAndShowDeclarationForEdit(declaration);
			}
		}

		protected virtual SingleLineEntryForm GetSingleLineEntryForm(ISingleLineEntryManager manager) => new SingleLineEntryForm(manager);

		protected virtual SingleLineEntryManager GetNewSingleLineEntryManager(JobDeclaration declaration) => new SingleLineEntryManager(declaration);

		protected virtual SADHFormDataManager GetNewSadhManager(JobDeclaration declaration) => new SADHFormDataManager(declaration);

		void SaveFactoryAndShowDeclarationForEdit(JobDeclaration declaration)
		{
			try
			{
				Save();

				var controller = GetControllerForStandAlone();
				if (Globals.IsTest)
				{
					ZFormModaliser.ShowDialogAndDispose((Form)controller.ShowEditForm(declaration));
				}
				else
				{
					controller.ShowEditForm(declaration);
				}
			}
			catch (ZSaveException ex)
			{
				Globals.Message.Show(Res.GetString("4CD766B4-89F3-40C6-A2D1-99DD76ED291D", "The following error was encountered while saving the changes:") + ex.Message);
			}
		}

		protected virtual void Save() => Factory.Save();

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		}
	}
}
