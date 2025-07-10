using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class OrganisationsLayoutBuilder : CommonOrganisationsLayoutBuilder<JobDeclaration>
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(OrganisationsControlBag.Instance.DefermentPartyDocAddressControl, jobDec => jobDec.IsImport, jobDec => jobDec.JE_MessageTypeInfo);
		}
	}
}
