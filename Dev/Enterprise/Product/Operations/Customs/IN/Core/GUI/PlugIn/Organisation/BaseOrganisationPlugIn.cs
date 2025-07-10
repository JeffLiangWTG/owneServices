using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.IN.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.GUI;

public abstract class BaseOrganisationPlugIn : CustomsPlugIn
{
	protected BaseOrganisationPlugIn(OrgHeader organisation) : base(organisation)
	{
		organisation.MainAddress.OA_RN_NKCountryCodeInfo.ValueChanged += OnChangeTheVisibilityRequired;
		ChangeTheVisibility();
	}

	public override string Name => Res.GetString("DC4FDE26-23D0-4A10-9665-F48E39A26065", "Customs Defaults");

	protected override ZBool HasUserControl => true;

	INOrgImpAddInfo AddInfo => addInfo ??= INOrgImpAddInfo.Get((OrgHeader)HostBusinessEntity);
	INOrgImpAddInfo addInfo;

	protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

	protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

	protected override void ChangeTheVisibilityCore()
	{
		Enabled = ((OrgHeader)HostBusinessEntity).MainAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.India;
	}
}
