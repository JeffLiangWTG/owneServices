using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class ManifestForwardModule : EDIMessageModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.CAManifestForward; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CA.CAManifestForward);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ManifestForwardEDIMessageCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ManifestForwardFilterBusinessObject();
		}

		protected override bool ShowRequeuingMenu
		{
			get { return false; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IFilterControl GetNewFilterControl()
		{
			return new ManifestForwardFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAManifestForward; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>();
			var createDeclarationMenuItem = new ZMenuItem(ResString.GetMultilingualString("b2d20991-415f-448d-ac2c-0057bc68e18e", "Find/Create Declaration from Forwarded Manifest"), new EventHandler(FindCreateDeclaration_Click));
			result.Add(createDeclarationMenuItem);
			return result.ToArray();
		}

		void FindCreateDeclaration_Click(object sender, EventArgs e)
		{
			var selectedElements = Grid.SelectedElements;
			if (selectedElements == null || selectedElements.Length != 1)
			{
				Globals.Message.ShowInformation(Res.GetString("f856027b-d162-4f33-b2cb-5f5e62ec7952", "Please select one Forwarded Manifest."), Res.GetString("23cbfa1a-2f7e-4bc3-88ef-8b9b29404714", "Select Manifest"));
			}
			else
			{
				var selectedForwardedManifest = selectedElements[0] as ACIHouseBillMessage;
				if (selectedForwardedManifest == null || selectedForwardedManifest.SNPType != SecondaryNotifyPartyTypeList.Codes.CustomsBroker || selectedForwardedManifest.CargoControlNumber.IsEmpty)
				{
					ShowSelectValidMessage();
				}
				else
				{
					var attachedEntry = selectedForwardedManifest.EM_LinkedObject as CusEntryHeader;
					if (attachedEntry != null)
					{
						ShowSelectValidMessage();
					}
					else
					{
						var supporter = new ForwardedManifestSupporter(selectedForwardedManifest);
						var declaration = supporter.FindOrCreateJobDeclarationMatchingOnCCN();
						if (declaration == null)
						{
							Globals.Message.ShowInformation(Res.GetString("ec794e29-cc6a-4fe3-aee8-fad4747bc0b8", "More than 1 declaration matching this House CCN already exists."), Res.GetString("801f94f6-5b09-4cf5-9160-f903ceb5b127", "Multiple Declarations"));
						}
						else
						{
							var controller = (JobDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
							if (declaration.IsInDatabase)
							{
								controller.ShowEditForm(declaration);
							}
							else
							{
								controller.ShowFormForNewEntity(declaration);
							}
						}
					}
				}
			}
		}

		void ShowSelectValidMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("49a3f0cb-2cb1-47e1-b374-f2c1980ce1a3", "Please select a valid Forwarded Manifest with SNP = CB, not already attached to a declaration and with a valid House CCN."), Res.GetString("23cbfa1a-2f7e-4bc3-88ef-8b9b29404714", "Select Manifest"));
		}

		#region Workflow

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CAeManifestWorkflowDescriptorCode; }
		}

		#endregion

	}
}
