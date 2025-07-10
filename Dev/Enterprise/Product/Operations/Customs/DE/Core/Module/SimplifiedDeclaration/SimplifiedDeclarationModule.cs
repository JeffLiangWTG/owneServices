using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module
{
	public class SimplifiedDeclarationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.DE.SimplifiedDeclaration;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.MonthlyClosing;

		public override IZForm ShowPopup()
		{
			var form = new SimplifiedDeclarationModuleForm();
			form.MinimumSize = EmbeddedControl.Size;
			form.FilterControlPanel.Controls.Add(EmbeddedControl);
			form.Text = Description;
			return form;
		}

		public override ZBool HasActions => false;

		public override bool AllowView => false;

		public override bool AllowEdit => false;

		protected override bool ShouldLoadFilterBusinessObjectDefaults => true;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var gridCollection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
			var query = new ZQuery(CusReconEntrySchema.CRE_CRD, null);
			if (TargetDeclaration != null)
			{
				query.AddToFilter(CusReconEntrySchema.CRE_GB_Branch, TargetDeclaration.CRD_GB_Branch);
			}
			gridCollection.AdditionalFilter = query;
			return gridCollection;
		}

		protected override IFilterControl GetNewFilterControl() => new SimplifiedDeclarationFilterStripControl(GridCollection as ActiveBusinessObjectCollection<CusReconEntry>, FilterBusinessObject as SimplifiedDeclarationFilterStripBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new SimplifiedDeclarationFilterStripBusinessObject();

		protected override BusinessObjectFactory GetNewFactory() => TargetDeclaration?.Factory ?? base.GetNewFactory();

		CusReconDeclaration TargetDeclaration => (ParentModalForm as ZForm)?.BusinessEntity as CusReconDeclaration;
	}
}
