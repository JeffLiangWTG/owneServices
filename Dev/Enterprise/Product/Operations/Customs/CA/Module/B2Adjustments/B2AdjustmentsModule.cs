using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
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
	public class B2AdjustmentsModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public B2AdjustmentsModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CA.B2Adjustments);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new B2AdjustmentsFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new B2AdjustmentsFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.B2Adjustments;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.CAB2Adjustments;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Insert(result.Count, new ZMenuItem(CopyNewVersionForB2Helper.CopyToNewVersionForB2Caption, CopyToNewVersionForB2));
			return result.ToArray();
		}

		void CopyToNewVersionForB2(object sender, EventArgs e)
		{
			var selectedElements = GetSelectedElements();

			if (selectedElements != null && selectedElements.Length == 1)
			{
				var sourceDeclaration = (JobDeclaration)selectedElements[0];

				var copiedDeclaration = CopyNewVersionForB2Helper.CopyToNewVersionForB2(sourceDeclaration);
				if (copiedDeclaration != null)
				{
					GetControllerForStandAlone().ShowNewCopyToB2Form(copiedDeclaration);
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("93068B3C-7C4A-4619-8A90-362C5A15DE30", "Please select one Declaration to copy."), CopyNewVersionForB2Helper.CopyToNewVersionForB2Caption);
			}
		}

		protected virtual JobDeclarationController GetControllerForStandAlone() => (JobDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);

		protected virtual BusinessObject[] GetSelectedElements() => Grid.SelectedElements;

		protected override IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
		{
			var declaration = CurrentBusinessObjectInGrid as JobDeclaration;
			if (declaration != null && declaration.IsIM2)
			{
				Globals.Message.ShowError(CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, Res.GetString("19726fb7-e45a-4c98-bf47-8f17b6a3bae0", "Copy not allowed"));
				return null;
			}
			else
			{
				return base.ShowTemplateCopyForm(selectedBusinessObject);
			}
		}

		public OperationalActionSupporter OperationalActionSupporter => new B2AdjustmentsOperationalActionSupporter();
	}
}
