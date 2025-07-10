using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module;

public class ImportFromTemporaryStorageRegisterModule : ZFilterGridModule
{
	public override ModuleIdentifier ID => ModuleIDs.Customs.ImportFromTemporaryStorageRegister;

	public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EFTATemporaryStorageRegister;

	public override IZForm ShowPopup()
	{
		var form = new ImportFromTemporaryStorageRegisterModuleForm();
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
		var customsOffice = string.Empty;
		var temporaryStorageApplicationCode = string.Empty;
		if (ParentModalForm is ZForm parentForm && parentForm.BusinessEntity is ICanImportFromTemporaryStorageRegister temporaryStorageRegisterImporter)
		{
			temporaryStorageApplicationCode = temporaryStorageRegisterImporter.TemporaryStorageApplicationCode;
			customsOffice = temporaryStorageRegisterImporter.DepartureCustomsOfficeCode;
		}
		return TemporaryStorageRegisterHelper.GetCusTempStorageRegLineCollection(customsOffice, temporaryStorageApplicationCode, Factory);
	}

	protected override IFilterControl GetNewFilterControl() => new ImportFromTemporaryStorageRegisterFilterStripControl(GridCollection as ActiveBusinessObjectCollection<CusTempStorageRegLine>, FilterBusinessObject as ImportFromTemporaryStorageRegisterFilterStripBusinessObject);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new ImportFromTemporaryStorageRegisterFilterStripBusinessObject();

	protected override BusinessObjectFactory GetNewFactory() => ParentBusinessEntity?.Factory ?? base.GetNewFactory();

	IBusiness ParentBusinessEntity => (ParentModalForm as ZForm)?.BusinessEntity;
}
