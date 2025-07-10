using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.GUI.DataImport;
using Enterprise.Client.UPE.Modules.AirCargo;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEAirCargoModule : UPEAirCargoCalloutBaseModule
	{
		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (Globals.IsDebugMode)
			{
				result.Add(new ZMenuItem("Developer Tools <Debug Only>", new EventHandler(DeveloperDebugTools)));
			}

			if (Env.CurrentUser.IsDeveloper)
			{
				result.Add(new ZMenuItem("Soundex <Test Use Only>", new EventHandler(SoundexTester)));
			}

			return result.ToArray();
		}

		void DeveloperDebugTools(object sender, EventArgs args)
		{
			new DeveloperToolsForm().Show();
		}

		void SoundexTester(object sender, EventArgs args)
		{
			using (SoundexTesterForm form = new SoundexTesterForm())
			{
				form.ShowDialog();
			}
		}

		public UPEAirCargoModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("Level 1", new EventHandler(HandleLevel1FileImport));
		}

		void HandleLevel1FileImport(object sender, EventArgs e)
		{
			using (Level1DataImportForm form = new Level1DataImportForm(new Level1DataImport(Factory)))
			{
				form.ShowDialog(EmbeddedControl);
			}
		}

		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.HouseAirCargo; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new UPEAirCargoController();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UPEAirCargoFilterControl(GridCollection, (UPEAirCargoFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UPEAirCargoFilterBusinessObject();
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CustomsHouseAirCargoCode; }
		}
	}
}
