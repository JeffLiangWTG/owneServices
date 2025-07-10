using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	/// <summary>
	/// Module for Statements
	/// </summary>
	public class SumAModule : TemporaryStorageModule
	{
		public SumAModule()
		{
		}
		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		public override bool AllowView => false;

		public override bool AllowUniversalCopy => false;

		protected override IFilterControl GetNewFilterControl()
		{
			return new SumAFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SumAFilterStripBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.TemporaryStorage);
		}

		#region New

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				var sumAMenuitem = new ZMenuItem(ResString.GetMultilingualString("b0a70a69-00d0-4f54-b039-f5046f678ca3", "New SumA Job"), SumAMenuItem_Click);
				var rexMenuitem = new ZMenuItem(ResString.GetMultilingualString("745c6a92-520a-42ed-a875-185699049c86", "New Re-Export Job"), REXMenuItem_Click);

				NewMenuItem.MenuItems.Add(sumAMenuitem);
				NewMenuItem.MenuItems.Add(rexMenuitem);

				sumAMenuitem.DefaultItem = true;
			}

			return result;
		}

		void SumAMenuItem_Click(object sender, EventArgs e)
		{
			GetNewController(Business.CusTempStorage.TemporaryStorageApplicationCodeList.Codes.SumA).ShowNewForm();
		}

		void REXMenuItem_Click(object sender, EventArgs e)
		{
			GetNewController(Business.CusTempStorage.TemporaryStorageApplicationCodeList.Codes.REX).ShowNewForm();
		}

		protected virtual ZController GetNewController(string appCode)
		{
			return new SumAController(appCode);
		}

		#endregion

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsTemporaryStorage;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => new CusTempStorageJobHeaderWorkflowDescriptor().Code;
	}
}
