using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.CA.GUI
{
	public class CAOrgSupplierPartFormCustomsPlugin : OrgSupplierPartFormCustomsPlugin
	{
		public CAOrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
			this.part = part;
		}
		readonly OrgSupplierPart part;

		protected override Control GetNewUserControl() => new CAOrgSupplierPartFormCustomsControl();

		protected override Customs.Business.BaseCusClassPartPivot[] CusClassPartPivots
			=> part != null ? part.GetPivots<Customs.Business.BaseCusClassPartPivot>(Core.Constants.CountryCodes.Canada) : Array.Empty<Customs.Business.BaseCusClassPartPivot>();

		protected override ZBool IsPromptAuditOnSavedEnabled
		{
			get
			{
				var companyPK = MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid();
				var branchPK = MasterFiles.Business.GlbBranch.CurrentBranch.PK.ToGuid();
				return part != null && part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Canada).Length > 0 &&
					(CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction ||
					CACustomsDataRegistry.Instance.ReleaseHighValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction ||
					CACustomsDataRegistry.Instance.EntryLowValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction ||
					CACustomsDataRegistry.Instance.EntryHighValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction);
			}
		}
	}
}
