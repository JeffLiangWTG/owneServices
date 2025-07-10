using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract class ComplianceDocumentController : ZController
	{
		protected virtual ComplianceDocumentForm GetComplianceDocumentForm(AccComplianceDocumentHeader complianceDocument) => new ComplianceDocumentForm(complianceDocument);

		protected override IZForm GetForm(IBusiness businessEntity) => GetComplianceDocumentForm(businessEntity as AccComplianceDocumentHeader);

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (CheckLoginCompanyNotMatch(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowViewForm(sourceEntity);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (CheckLoginCompanyNotMatch(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowEditForm(sourceEntity);
		}

		bool CheckLoginCompanyNotMatch(BusinessObject sourceEntity)
		{
			var result = true;
			var complianceDocumentHeader = sourceEntity as AccComplianceDocumentHeader;
			if (complianceDocumentHeader != null)
			{
				var company = complianceDocumentHeader.Company;
				if (result = company != null && company.PK != Env.CurrentCompany.PK)
				{
					Globals.Message.ShowError(
							ResString.GetMultilingualString("296B8538-A4E4-499B-BD01-1A433861B307", "This Compliance Document is posted into the ledgers of another company on this database. You must login to the following company to view this transaction: {0} - {1}.", company.GC_Code, company.GC_Name),
							ResString.GetMultilingualString("B71F8FFC-91EA-4990-A212-3425686F82C0", "Access Denied: Incorrect login company")
						);
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForNew => null;

		protected virtual SecurityCheckpoint CheckPointForVoid => null;

		public virtual IZForm ShowVoidForm(BusinessObject sourceEntity)
		{
			SecurityCheckpoint checkpoint = CheckPointForVoid;
			ComplianceDocumentForm result = null;

			if (checkpoint.IsAllowed)
			{
				var complianceDocumentHeader = sourceEntity as AccComplianceDocumentHeader;
				var voidErrorMessage = complianceDocumentHeader.CheckCanVoid();
				if (voidErrorMessage.IsEmpty)
				{
					result = (ComplianceDocumentForm)ShowLoadedForm(sourceEntity, FormAction.Delete);
					PrepareFormForVoiding(result);
				}
				else
				{
					Globals.Message.ShowError(voidErrorMessage, ResString.GetMultilingualString("5780D43A-D480-4C76-9A87-6D4B3F9E84E2", "Cannot void the record"));
				}
			}
			else
			{
				checkpoint.ShowError();
			}
			return result;
		}

		void PrepareFormForVoiding(ComplianceDocumentForm complianceDocumentForm)
		{
			var originalVerb = complianceDocumentForm.FormVerb;

			complianceDocumentForm.VoidInsteadOfDelete = true;
			var newDeleteText = Res.GetString("8298C2EA-FD44-49FE-BFF8-9E150C64F9DA", "Void");

			complianceDocumentForm.Text = complianceDocumentForm.Text.Replace(originalVerb, complianceDocumentForm.FormVerb);

			var form = (IPostingButtonsProvider)complianceDocumentForm;
			var originalDeleteText = form.CommandButtonPost.Text;
			form.CommandButtonPost.Text = newDeleteText;

			var form2 = (IFileMenuItemsProvider)complianceDocumentForm;
			foreach (MenuItem item in form2.FileMenuItem.MenuItems)
			{
				if (item.Text == originalDeleteText)
				{
					item.Text = newDeleteText;
				}
			}

			if (complianceDocumentForm.BusinessEntity is ARComplianceDocumentHeader accComplianceDocumentHeader)
			{
				complianceDocumentForm.ComplianceDocumentUserControl.SetSpecialVoidingPanel();
				accComplianceDocumentHeader.AddWritableProperties(new string[] { nameof(accComplianceDocumentHeader.IsSpecialVoiding) });
			}
		}
	}
}
