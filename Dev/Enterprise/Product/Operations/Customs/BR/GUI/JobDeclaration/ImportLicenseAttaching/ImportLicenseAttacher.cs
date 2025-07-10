using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public class ImportLicenseAttacher : ZRecordAttacher
	{
		public ImportLicenseAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, JobDeclaration declaration) : base(destinationCollection, findBoxList, moduleID)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		protected override void ShowCore(IZForm formToShowModalTo)
		{
			if (DestinationCollection != null && !((IBusinessObjectCollectionInternals)DestinationCollection).MastersAreDeleted)
			{
				if (DestinationCollection.ReadOnly)
				{
					Globals.Message.ShowInformation(Res.GetString("921049f2-c101-461f-ab8d-635a1e616e5f", "Sorry, Attached Licenses can be viewed but not attached."), Res.GetString("e4b091f7-8f2e-4d37-a03a-bb7ab27cab4f", "Cannot attach..."));
				}
				else
				{
					if (ShouldShow((ZForm)formToShowModalTo))
					{
						base.ShowCore(formToShowModalTo);
					}
				}
			}
		}

		bool ShouldShow(ZForm formToShowModalTo)
		{
			return !NeedToSave ||
					(ShowConfirmationForSaveBeforeAttach() == DialogResult.Yes &&
					formToShowModalTo.FireSaveButton() == ContinueWithSave.Yes);
		}

		bool NeedToSave
		{
			get { return DestinationCollection.HasChanges; }
		}

		DialogResult ShowConfirmationForSaveBeforeAttach()
		{
			string caption = Res.GetString("0f8c7302-b807-4bb6-9526-e854fa4d8572", "Save Confirmation");
			string message = Res.GetString("c5c3e31e-f2b9-462a-9f2c-a6f522529d35", "The form must be saved before an Import License can be attached. Do you wish to save the form?");
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);
		}

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var declaration = DestinationCollection.Factory.Load<JobDeclaration>(bizO.PK);
			if (declaration != null && !declaration.IsDeleted)
			{
				ImportLicenseAttachingForm.ShowDialog(new ImportLicenseAttachingObjectParent(declaration), this.declaration);
			}

			return true;
		}
	}
}
