using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Wizards.EIDR;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.GUI.Wizards;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module
{
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("674DD3E1-F161-4366-A9E6-8DD3672123BF", "New EIDR Wizard"), CreateNewEidr));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("C2EFAF3C-42BF-4FF9-A665-CEDEEBB66955", "Create CDS FSD type 'Q' declaration"), CreateCDSFsd));
			}
			return result;
		}

		void CreateNewEidr(object sender, EventArgs e)
		{
			var declaration = Factory.New<JobDeclaration>();
			var eidrWizardManager = GetNewEidrWizardManager(declaration);
			ZFormModaliser.ShowDialogAndDispose(new EidrWizardForm(eidrWizardManager));
			if (eidrWizardManager.ExecutedSuccessfully)
			{
				SaveFactoryAndShowDeclarationForEdit(declaration);
			}
		}

		void CreateCDSFsd(object sender, EventArgs e)
		{
			var declaration = Factory.New<JobDeclaration>();
			var cdsFsdWizardManager = GetNewCDSFsdWizardManager(declaration);
			ZFormModaliser.ShowDialogAndDispose(new CdsFsdWizardForm(cdsFsdWizardManager));
			if (cdsFsdWizardManager.ExecutionResult == CdsFsdCreatedResult.Success)
			{
				SaveFactoryAndShowDeclarationForEdit(declaration);
			}
			if (cdsFsdWizardManager.ExecutionResult == CdsFsdCreatedResult.Warning)
			{
				Globals.Message.ShowWarning("It was not possible to find exactly one matching Customs Authorisation.  " +
					$"A place holder with value '{CDSFinalSupplementaryDeclarationHelperManager.DefaultAuthorisationNumber}' has been set on the FSD's Authorisation record under the Entry Instruction.  " +
					"Please replace this with the correct value prior to transmission.");
				SaveFactoryAndShowDeclarationForEdit(declaration);
			}
		}

		protected virtual EidrWizardManager GetNewEidrWizardManager(JobDeclaration declaration) => new EidrWizardManager(declaration);

		protected CDSFinalSupplementaryDeclarationHelperManager GetNewCDSFsdWizardManager(JobDeclaration declaration) => new CDSFinalSupplementaryDeclarationHelperManager(declaration);

		void SaveFactoryAndShowDeclarationForEdit(JobDeclaration declaration)
		{
			Factory.Save();
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

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
