using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.GUI
{
	public class AppendCommissionAgreementItemModuleDecisionProvider : IModuleDecisionProvider
	{
		public AppendCommissionAgreementItemModuleDecisionProvider(EmbeddedModulePopup popup, CommissionAgreementApprovalWizard wizard)
		{
			Argument.NotNull(popup, "popup");
			Argument.NotNull(wizard, "wizard");

			this.popup = popup;
			this.wizard = wizard;
		}

		readonly EmbeddedModulePopup popup;
		readonly CommissionAgreementApprovalWizard wizard;

		public bool AllowExcelExport
		{
			get { return false; }
		}

		public bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public IBusinessObjectCollection List
		{
			get { return null; }
		}

		public bool ShouldDisplayNotifications
		{
			get { return false; }
		}

		public bool ShouldIgnoreAdditionalFilter
		{
			get { return false; }
		}

		public bool ShouldLoadFilterBizObj
		{
			get { return false; }
		}

		public bool ShouldSaveFilterBizObj
		{
			get { return false; }
		}

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
		{
			var agreements = selectedBusinessObjects.OfType<OrgCommissionAgreement>();
			if (agreements.Any())
			{
				var currentAgreementPksAlreadyInCollection = new HashSet<ZGuid>(wizard.CommissionAgreementApprovalItems.Select(x => x.CommissionAgreement.PK));
				foreach (var agreement in agreements)
				{
					if (!currentAgreementPksAlreadyInCollection.Contains(agreement.PK))
					{
						var agreementInWizardFactory = wizard.Factory.Load<OrgCommissionAgreement>(agreement.PK);
						wizard.CommissionAgreementApprovalItemCollection.Add(new CommissionAgreementApprovalItem(wizard, agreementInWizardFactory));
					}
				}

				popup.Close();
			}
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
		{
			HandleDefaultAction(selectedBusinessObject);
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		public void InitialiseFindBoxControllerLink(ZController controller)
		{
		}

		public void SetFindBoxCodeDescription(BusinessObject bizo)
		{
		}
	}
}
