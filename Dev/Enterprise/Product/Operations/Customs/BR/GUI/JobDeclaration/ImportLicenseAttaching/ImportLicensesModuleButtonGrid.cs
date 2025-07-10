using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.GUI
{
	public class ImportLicensesModuleButtonGrid : ZModuleButtonGrid
	{
		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new ImportLicenseAttacher(destinationCollection, findBoxList, moduleID, Declaration);
		}

		JobDeclaration Declaration => DataSource as JobDeclaration;

		internal ZController LicenseController => controller ?? (controller = ZControllerFactory.Create(ControllerIDs.Customs.BR.License));
		ZController controller;

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				SelectFirstRowIfOnlyRowInGrid();

				var selected = InnerGrid.SelectedElements;
				if (selected.Length == 0)
				{
					ShowNotSelectedMessage();
				}
				else if (IsDetachAllowed())
				{
					var entryInstructions = selected.Cast<DeclarationRelatedImportLicenseEntryGenPivot>().Select(x => x.EntryInstruction).ToList();
					Declaration.DetachImportLicense(entryInstructions);
				}
			}
		}

		protected override void Edit(BusinessObject selected, object sender)
		{
			var declaration = (selected as DeclarationRelatedImportLicenseEntryGenPivot)?.EntryInstruction?.JobDeclaration;
			if (declaration != null)
			{
				LicenseController.ShowEditForm(declaration);
			}
		}
	}
}
