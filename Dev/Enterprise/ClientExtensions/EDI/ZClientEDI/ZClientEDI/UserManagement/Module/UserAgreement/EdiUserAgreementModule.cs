using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class EdiUserAgreementModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.UserAgreements;
		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.UserAgreements;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.UserAgreements);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EdiUserAgreementCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EdiUserAgreementFilterControl(GridCollection, (EdiUserAgreementFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EdiUserAgreementFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (EDISecurityCheckpoints.UserAgreementsAcceptForOrganisation.IsAllowed)
			{
				var acceptForOrgMenuItem = new ZMenuItem(ResString.GetMultilingualString("251e46d6-e1bc-443f-be0e-2ba40131da1e", "Accept on behalf of client"), AcceptForClient_PerformClick);
				result.Add(acceptForOrgMenuItem);
			}

			return result.ToArray();
		}

		void AcceptForClient_PerformClick(object sender, EventArgs e)
		{
			var selectedAgreements = GetSelectedUserAgreements();

			if (!selectedAgreements.Any())
			{
				Globals.Message.ShowError(Res.GetString("38c29a4e-80eb-4289-80e9-d90ee806653d", "Please select at least one agreement to accept."));
				return;
			}

			var organisation = GetOrganisation();

			if (organisation != null)
			{
				var result = Globals.Message.Show(
					Res.GetString("a7fc5c51-6619-446e-bd3d-3bf7e1037598", "Are you sure you want to accept the user agreement on behalf of {0}?", organisation.OH_FullName),
					Res.GetString("b4dd647d-776b-4125-8467-a459cc5cbdd3", "Confirm selection"), MessageBoxButtons.YesNo, DialogResult.No);

				if (result == DialogResult.Yes)
				{
					var skippedAgreements = new StringBuilder();
					foreach (var agreement in selectedAgreements)
					{
						if (agreement.HasBeenAcceptedByOrganisation(organisation))
						{
							skippedAgreements.Append(agreement.ERA_Title);
							skippedAgreements.AppendLine();
						}
						else
						{
							agreement.AcceptOnBehalfOfOrganisation(organisation);
						}
					}

					Factory.Save();

					if (skippedAgreements.Length > 0)
					{
						Globals.Message.Show(Res.GetString("469d0bbe-c60e-48df-9afd-e172d70fcb4b",
							"The following agreements were skipped as they have already been accepted for this organization:\r\n{0}", skippedAgreements.ToString()));
					}
				}
			}
		}

		protected virtual EdiUserAgreement[] GetSelectedUserAgreements()
		{
			return Grid.SelectedElements.Cast<EdiUserAgreement>().ToArray();
		}

		protected virtual OrgHeader GetOrganisation()
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation);

			using (var popup = new EmbeddedModulePopup(module))
			{
				var strategy = new SelectedOrganisationStrategy(popup);
				popup.EmbeddedModulePopupOKButtonStrategy = strategy;
				ZFormModaliser.ShowDialogWithoutDispose(popup);
				return strategy.SelectedOrganisation;
			}
		}

		class SelectedOrganisationStrategy : IEmbeddedModulePopupOKButtonStrategy
		{
			public SelectedOrganisationStrategy(EmbeddedModulePopup popup)
			{
				this.popup = popup;
			}
			readonly EmbeddedModulePopup popup;

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
			{
				if (selectedBusinessObjects == null || selectedBusinessObjects.Length == 0 || selectedBusinessObjects.Length > 1)
				{
					Globals.Message.ShowInformation(SelectOneOrganisationMessage);
				}
				else
				{
					SelectedOrganisation = selectedBusinessObjects[0] as OrgHeader;
					popup.Close();
				}
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
				Globals.Message.ShowInformation(SelectOneOrganisationMessage);
			}

			public OrgHeader SelectedOrganisation { get; private set; }

			ZString SelectOneOrganisationMessage => Res.GetString("04677088-fc67-472d-adef-c2618220d986", "Please select one organization.");
		}
	}
}
