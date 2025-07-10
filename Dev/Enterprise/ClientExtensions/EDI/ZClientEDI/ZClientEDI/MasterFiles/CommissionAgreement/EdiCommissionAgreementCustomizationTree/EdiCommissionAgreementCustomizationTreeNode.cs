using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	class EdiCommissionAgreementCustomizationTreeNode : ZNode<EdiCommissionAgreementTreeBizObjWrapper>
	{
		public EdiCommissionAgreementCustomizationTreeNode(EdiCommissionAgreementCustomizationTreeModel model, EdiCommissionAgreementTreeBizObjWrapper bizObj)
			: base(model, bizObj)
		{
		}

		public new EdiCommissionAgreementCustomizationTreeModel TreeModel
		{
			get { return (EdiCommissionAgreementCustomizationTreeModel)base.TreeModel; }
		}

		#region Properties

		public bool IsForNewDatabases
		{
			get
			{
				var databaseWrapper = BizObjForBinding as EdiCommissionAgreementDatabaseWrapper;
				if (databaseWrapper != null)
				{
					return databaseWrapper.LicenceDatabase == null;
				}

				var companyWrapper = BizObjForBinding as EdiCommissionAgreementCountryWrapper;
				if (companyWrapper != null)
				{
					return companyWrapper.LicenceDatabase == null;
				}

				return false;
			}
		}

		#region Selected

		public CheckState Selected
		{
			get
			{
				if (TreeModel.Customization.EZN_IsAllCompanies || BizObjForBinding.Selected)
				{
					return CheckState.Checked;
				}
				else if (ChildNodes.Any())
				{
					if (ChildNodes.Cast<EdiCommissionAgreementCustomizationTreeNode>().All(x => x.Selected == CheckState.Checked))
					{
						return CheckState.Checked;
					}
					else if (ChildNodes.Cast<EdiCommissionAgreementCustomizationTreeNode>().Any(x => x.Selected != CheckState.Unchecked))
					{
						return CheckState.Indeterminate;
					}
				}

				return CheckState.Unchecked;
			}
			set
			{
				var boolValue = value == CheckState.Checked;
				BizObjForBinding.Selected = boolValue;

				foreach (var childNode in ChildNodes.Cast<EdiCommissionAgreementCustomizationTreeNode>())
				{
					childNode.Selected = value;
				}
			}
		}

		public bool Selected_Visible
		{
			get
			{
				var databaseWrapper = BizObjForBinding as EdiCommissionAgreementDatabaseWrapper;
				if (databaseWrapper != null && databaseWrapper.LicenceDatabase == null)
				{
					return false;
				}

				return true;
			}
		}

		public bool Selected_ReadOnly
		{
			get
			{
				if (TreeModel.Customization.EZN_IsAllCompanies)
				{
					return true;
				}

				if (!BizObjForBinding.Selected_Enabled)
				{
					if (!ChildNodes.Cast<EdiCommissionAgreementCustomizationTreeNode>().Any(x => !x.Selected_ReadOnly))
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		public string Code
		{
			get { return BizObjForBinding.Code; }
		}

		public string Description
		{
			get { return BizObjForBinding.Description; }
		}

		#region ShouldAutoAdd

		public CheckState ShouldAutoAdd
		{
			get
			{
				if (TreeModel.Customization.EZN_IsAllCompanies || BizObjForBinding.ShouldAutoAdd)
				{
					return CheckState.Checked;
				}
				else if (ParentNode != null)
				{
					if (((EdiCommissionAgreementCustomizationTreeNode)ParentNode).BizObjForBinding.ShouldAutoAdd)
					{
						return CheckState.Checked;
					}
				}
				else if (ChildNodes.Any())
				{
					if (ChildNodes.Cast<EdiCommissionAgreementCustomizationTreeNode>().Any(x => x.BizObjForBinding.ShouldAutoAdd))
					{
						return CheckState.Indeterminate;
					}
				}

				return CheckState.Unchecked;
			}
			set
			{
				var boolValue = value == CheckState.Checked;

				BizObjForBinding.ShouldAutoAdd = boolValue;

				if (ParentNode == null && value == CheckState.Unchecked)
				{
					foreach (var childNode in ChildNodes.Cast<EdiCommissionAgreementCustomizationTreeNode>())
					{
						childNode.ShouldAutoAdd = CheckState.Unchecked;
					}
				}
			}
		}

		public bool ShouldAutoAdd_Visible
		{
			get
			{
				if (BizObjForBinding.ShouldAutoAdd_Enabled)
				{
					return true;
				}

				if (ChildNodes.Cast<EdiCommissionAgreementCustomizationTreeNode>().Any(x => x.ShouldAutoAdd_Visible))
				{
					return true;
				}

				return false;
			}
		}

		public bool ShouldAutoAdd_ReadOnly
		{
			get
			{
				if (TreeModel.Customization.EZN_IsAllCompanies)
				{
					return true;
				}

				var parentNode = ParentNode as EdiCommissionAgreementCustomizationTreeNode;
				if (parentNode != null)
				{
					return parentNode.ShouldAutoAdd == CheckState.Checked;
				}

				return false;
			}
		}

		#endregion

		#region IncludeDatabaseUsage

		public bool IncludeDatabaseUsage
		{
			get
			{
				return TreeModel.Customization.EZN_IsAllDatabases || BizObjForBinding.IncludeDatabaseUsage;
			}
			set { BizObjForBinding.IncludeDatabaseUsage = value; }
		}

		public bool IncludeDatabaseUsage_Visible
		{
			get { return BizObjForBinding.IncludeDatabaseUsage_Enabled; }
		}

		public bool IncludeDatabaseUsage_ReadOnly
		{
			get { return TreeModel.Customization.EZN_IsAllDatabases; }
		}

		#endregion

		#endregion

		#region Overrides

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(EdiCommissionAgreementTreeBizObjWrapper previousParent, EdiCommissionAgreementTreeBizObjWrapper newParent, bool checkValid)
		{
			throw new NotImplementedException("Not required as does not support re-ordering");
		}

		protected override IEnumerable<EdiCommissionAgreementTreeBizObjWrapper> LoadChildBizObjs()
		{
			return BizObjForBinding.Children ?? Enumerable.Empty<EdiCommissionAgreementTreeBizObjWrapper>();
		}

		protected override EdiCommissionAgreementTreeBizObjWrapper LoadParentBizObj()
		{
			return null;
		}

		#endregion
	}
}
