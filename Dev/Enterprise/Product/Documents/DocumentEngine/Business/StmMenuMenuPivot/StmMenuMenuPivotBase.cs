using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	public class StmMenuMenuPivotBase : StmMenuMenuPivot, IStmMenuMenuPivotBase
	{
		public StmMenuMenuPivotBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
		}

		#region SF_Calc_ChildName

		public ZString SF_Calc_ChildName
		{
			get { return Outward != null ? Outward.SU_MenuName : ZString.Empty; }
		}

		public ZPropertyInfo SF_Calc_ChildNameInfo
		{
			get { return Outward != null ? Outward.SU_MenuNameInfo : null; }
		}

		#endregion

		#region SF_Calc_ChildMenuPath

		public ZString SF_Calc_ChildMenuPath
		{
			get { return Outward != null ? Outward.SU_MenuPath : ZString.Empty; }
		}

		public ZPropertyInfo SF_Calc_ChildMenuPathInfo
		{
			get { return Outward != null ? Outward.SU_MenuPathInfo : null; }
		}

		#endregion

		#region SF_Calc_ChildHint

		public ZString SF_Calc_ChildHint
		{
			get { return Outward != null ? Outward.SU_Hint : ZString.Empty; }
		}

		public ZPropertyInfo SF_Calc_ChildHintInfo
		{
			get { return Outward != null ? Outward.SU_HintInfo : null; }
		}

		#endregion

		#region SF_Calc_ChildBusinessContext

		public ZString SF_Calc_ChildBusinessContext
		{
			get
			{
				if (!SF_OverriddenBusinessContext.IsEmpty)
				{
					return SF_OverriddenBusinessContext;
				}

				return Outward != null ? Outward.SU_BusinessContext : ZString.Empty;
			}
		}

		public ZPropertyInfo SF_Calc_ChildBusinessContextInfo
		{
			get { return Outward != null ? Outward.SU_BusinessContextInfo : null; }
		}

		#endregion

		#region SF_Calc_ChildMenuType

		public ZString SF_Calc_ChildMenuType
		{
			get { return Outward?.SU_MenuType ?? ZString.Empty; }
		}

		public ZString ChildMenuTypeDescription
		{
			get { return Outward?.MenuTypeDescription ?? ZString.Empty; }
		}

		public ZPropertyInfo SF_Calc_ChildMenuTypeInfo
		{
			get { return Outward != null ? Outward.SU_MenuTypeInfo : null; }
		}

		#endregion

		public MenuEditingMode EditingMode
		{
			get
			{
				return editingMode;
			}
			set
			{
				editingMode = value;

				switch (value)
				{
					case MenuEditingMode.AllowEditingOfClientSpecificOnly:
						ReadOnly = (SF_IsSystemDefined && !SF_IsClientSpecific);
						break;

					case MenuEditingMode.AllowEditingOfSystemDefinedOnly:
						ReadOnly = (SF_IsSystemDefined && SF_IsClientSpecific);
						break;

					case MenuEditingMode.NotAllowEditingOfSystemOrClientMenus:
						ReadOnly = (SF_IsSystemDefined || SF_IsClientSpecific);
						break;

					case MenuEditingMode.AllowAll:
					default:
						ReadOnly = false;
						break;
				}
			}
		}

		protected bool SF_IsSystemDefined_ReadOnly
		{
			get
			{
				return EditingMode == MenuEditingMode.AllowEditingOfClientSpecificOnly ||
					EditingMode == MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			}
		}

		protected bool SF_IsClientSpecific_ReadOnly
		{
			get
			{
				return EditingMode == MenuEditingMode.AllowEditingOfSystemDefinedOnly ||
					EditingMode == MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			}
		}

		MenuEditingMode editingMode = MenuEditingMode.AllowAll;

		public override StmMenuItem Outward => Factory.Load<StmMenuItemBase>(SF_SU_Outward);
		IStmMenuItem IStmMenuMenuPivotBase.Outward => Outward;

		#region SystemDefined and ClientSpecific

		public override ZBool SF_IsSystemDefined
		{
			get
			{
				return base.SF_IsSystemDefined;
			}
			set
			{
				base.SF_IsSystemDefined = value;

				if (!isInAnotherSetter && SF_IsClientSpecific)
				{
					isInAnotherSetter = true;

					try
					{
						SF_IsClientSpecific = value;
					}
					finally
					{
						isInAnotherSetter = false;
					}
				}
			}
		}

		protected override StmMenuMenuPivotValidation GetNewValidation()
		{
			return new StmMenuMenuPivotBaseValidation(this);
		}

		public override ZBool SF_IsClientSpecific
		{
			get
			{
				return base.SF_IsClientSpecific;
			}
			set
			{
				base.SF_IsClientSpecific = value;

				if (!isInAnotherSetter)
				{
					isInAnotherSetter = true;

					try
					{
						SF_IsSystemDefined = value;
					}
					finally
					{
						isInAnotherSetter = false;
					}
				}
			}
		}

		bool isInAnotherSetter;

		#endregion

		public override bool CanDelete
		{
			get { return !ReadOnly; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("72bd6788-8913-4f4b-b612-af3a9636e502", "{0} is read-only.", HumanReadableName);
			}
		}
	}
}
