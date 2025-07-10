using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public static class MenuHelper
	{
		public static void ShowNFEImportForm(JobDeclaration declaration)
		{
			var nfeImportObjectParent = new NFEImportObjectParent(declaration);
			using (var form = new NFEImportForm(nfeImportObjectParent))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		public static void ShowImportLicenseLoadResponseForm(JobDeclaration declaration)
		{
			var importLicenseObjectParent = new ImportLicenseAcceptResponseObjectParent(declaration);
			using (var form = new ImportLicenseResponseMessageForm(importLicenseObjectParent))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		public static void ShowUpdateImportLicenseStatusForm(JobDeclaration declaration)
		{
			var importLicenseObjectParent = new ImportLicenseStatusResponseObjectParent(declaration);
			using (var form = new ImportLicenseResponseMessageForm(importLicenseObjectParent))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		public static void ShowNFeExportForm(JobDeclaration declaration)
		{
			declaration.NFeExportObject.Refresh();

			using (var form = new NFeExportForm(declaration.NFeExportObject))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		public static void ShowImportLicenseFromXMLForm(JobDeclaration declaration)
		{
			var importLicenseLoadingObjectParent = new ImportLicenseLoadingObjectParent(declaration);
			using (var form = new ImportLicenseFromXMLForm(importLicenseLoadingObjectParent))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		public static void ShowUpdateImportEntryNumberForm(JobDeclaration declaration)
		{
			var updateImportEntryNumberObject = new UpdateImportEntryNumberObject(declaration);
			using (var form = new UpdateImportEntryNumberForm(updateImportEntryNumberObject))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		public static void ShowUpdateImportEntryStatusForm(JobDeclaration declaration)
		{
			var updateImportEntryStatusObject = new UpdateImportEntryStatusObject(declaration);
			using (var form = new UpdateImportEntryStatusForm(updateImportEntryStatusObject))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
