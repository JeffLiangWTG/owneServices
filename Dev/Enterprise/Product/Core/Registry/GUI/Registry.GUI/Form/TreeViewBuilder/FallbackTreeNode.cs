using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public enum FallbackStatus
	{
		Active,
		NotActive,
		NotAFallback
	}

	public class FallbackTreeNode : TreeNode
	{
		readonly ZGuid CompanyPK;
		readonly ZGuid BranchPK;
		readonly ZGuid DepartmentPK;
		readonly FallbackStatus fStatus;

		public FallbackTreeNode(string nodeText) : base(nodeText)
		{
			fStatus = FallbackStatus.NotAFallback;
		}

		public FallbackTreeNode(string nodeText, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK, bool isActive) : base(nodeText)
		{
			this.CompanyPK = companyPK;
			this.BranchPK = branchPK;
			this.DepartmentPK = departmentPK;
			fStatus = isActive ? FallbackStatus.Active : FallbackStatus.NotActive;
		}

		public FallbackStatus Status
		{
			get { return fStatus; }
		}

		public string Comparator
		{
			get
			{
				string parentComparator = (Parent == null) ? "" : ((FallbackTreeNode)Parent).Comparator;
				return parentComparator + Text + CompanyPK + BranchPK + DepartmentPK;
			}
		}

		public RegistryItemProposedValueAccessor GetFallbackAccessor(RegistryItemTag item)
		{
			RegistryItemProposedValueAccessor fallbackAccessor = new RegistryItemProposedValueAccessor(item.RegistryItem, GetFallbackLevel());
			return fallbackAccessor;
		}

		public FallbackLevel GetFallbackLevel()
		{
			Guid companyGuid = (CompanyPK.IsEmpty) ? Guid.Empty : CompanyPK.ToGuid();
			Guid branchGuid = (BranchPK.IsEmpty) ? Guid.Empty : BranchPK.ToGuid();
			Guid departmentGuid = (DepartmentPK.IsEmpty) ? Guid.Empty : DepartmentPK.ToGuid();
			return new FallbackLevel(companyGuid, branchGuid, departmentGuid);
		}
	}
}
