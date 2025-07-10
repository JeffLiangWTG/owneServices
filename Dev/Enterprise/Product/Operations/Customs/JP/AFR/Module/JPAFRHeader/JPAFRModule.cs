using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class JPAFRModule : ZFilterGridModule
	{
		public MenuItem NewVOCCItem { get; private set; }
		public MenuItem NewNVOCCItem { get; private set; }

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result;
			var aFR = selectedBusinessObject as JPAFRHeader;
			var consol = aFR?.Consol;
			if (consol == null)
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.JP.AFR);
				((JPAFRController)result).CreateVOCCAFR = createVOCCAFR;
			}
			else
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
			}
			return result;
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Insert(1, NewNVOCCItem = new ZMenuItem(ResString.GetMultilingualString("ModuleGrid.NewAFRNVOCC", "New &NVOCC"), HandleNewClick));
			result.Insert(1, NewVOCCItem = new ZMenuItem(ResString.GetMultilingualString("ModuleGrid.NewAFRVOCC", "New &VOCC"), ShowNewVOCCForm));

			return result.ToArray();
		}

		void ShowNewVOCCForm(object sender, EventArgs e)
		{
			try
			{
				createVOCCAFR = true;
				ShowNewForm();
			}
			finally
			{
				createVOCCAFR = false;
			}
		}
		bool createVOCCAFR;

		protected override void SetupButtonDetailForItem(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			if (item == NewNVOCCItem || (item == NewVOCCItem && NewVOCCItem != null))
			{
				buttonImage = IconTypes.NewButtonRest;
				buttonImageActive = IconTypes.NewButtonActive;
			}
			else
			{
				base.SetupButtonDetailForItem(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JPAFRFilterStrip();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JPAFRFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JPAFRHeaderModuleCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.JP.AFR; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.JPAFRReporting; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.JPAFRWorkflowDescriptorCode; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}
	}
}
