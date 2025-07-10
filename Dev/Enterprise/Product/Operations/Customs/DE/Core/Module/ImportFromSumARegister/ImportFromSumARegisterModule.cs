using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	public class ImportFromSumARegisterModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.DE.ImportFromSumARegister;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.DESumARegister;

		public override IZForm ShowPopup()
		{
			var form = new ImportFromSumARegisterModuleForm();
			form.FilterControlPanel.Controls.Add(EmbeddedControl);
			form.Text = Description;
			return form;
		}

		public override ZBool HasActions => ZBool.False;

		public override bool AllowView => false;

		public override bool AllowEdit => false;

		protected override bool ShouldLoadFilterBusinessObjectDefaults => true;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var customsOffice = ZString.Empty;
			if (ParentModalForm is ZForm parentForm)
			{
				if (parentForm.BusinessEntity is JobDeclaration declaration)
				{
					customsOffice = declaration.JE_CustomsOffice;
				}
				else if (parentForm.BusinessEntity is IDepartureCustomsOfficeCodeProvider nctsHeader)
				{
					customsOffice = nctsHeader.DepartureCustomsOfficeCode;
				}
			}
			return TemporaryStorageHelper.GetCusTempStorageRegLineCollection(customsOffice, Factory);
		}

		protected override IFilterControl GetNewFilterControl() => new ImportFromSumARegisterFilterStripControl(GridCollection as ActiveBusinessObjectCollection<CusTempStorageRegLine>, FilterBusinessObject as ImportFromSumARegisterFilterStripBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ImportFromSumARegisterFilterStripBusinessObject();

		protected override BusinessObjectFactory GetNewFactory() => ParentBusinessEntity?.Factory ?? base.GetNewFactory();

		IBusiness ParentBusinessEntity => (ParentModalForm as ZForm)?.BusinessEntity;
	}
}
