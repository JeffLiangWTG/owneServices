using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryItemProposedValueAccessor : RegistryItemFallBackValueAccessor
	{
		public RegistryItemProposedValueAccessor(IRegistryItem item, Guid companyPK, Guid branchPK, Guid departmentPK)
			: this(item, RegistryStorageFlags.BranchDepartment, companyPK, branchPK, departmentPK)
		{
		}

		public RegistryItemProposedValueAccessor(IRegistryItem item, FallbackLevel fallback)
			: this(item, fallback.Level, fallback.CompanyPK(false), fallback.BranchPK, fallback.DepartmentPK)
		{
		}

		public RegistryItemProposedValueAccessor(IRegistryItem item, RegistryStorageFlags level, Guid companyPK, Guid branchPK, Guid departmentPK)
			: base(item, level, companyPK, branchPK, departmentPK)
		{
		}

		#region Overrides

		protected override RegistryItemFallBackValueAccessor GetFallBackParent()
		{
			return new RegistryItemProposedValueAccessor(Item, GetParentLevel(Level), CompanyPK, BranchPK, DepartmentPK);
		}

		protected override object ValueAtThisLevel
		{
			get
			{
				object result = null;

				if (!HasStorageAtThisLevel())
				{
					throw new InvalidOperationException("No value is stored at this level");
				}

				if (Level == RegistryStorageFlags.BranchDepartment)
				{
					result = HasProposedValue(ItemInternals, Guid.Empty, BranchPK, DepartmentPK) ?
						ItemInternals.GetProposedValue(Guid.Empty, BranchPK, DepartmentPK) :
						Item.GetValueWithoutFallback(Guid.Empty, BranchPK, DepartmentPK);
				}

				if (Level == RegistryStorageFlags.Branch)
				{
					result = HasProposedValue(ItemInternals, Guid.Empty, BranchPK, Guid.Empty) ?
						ItemInternals.GetProposedValue(Guid.Empty, BranchPK, Guid.Empty) :
						Item.GetValueWithoutFallback(Guid.Empty, BranchPK, Guid.Empty);
				}

				if (Level == RegistryStorageFlags.CompanyDepartment)
				{
					result = HasProposedValue(ItemInternals, CompanyPK, Guid.Empty, DepartmentPK) ?
						ItemInternals.GetProposedValue(CompanyPK, Guid.Empty, DepartmentPK) :
						Item.GetValueWithoutFallback(CompanyPK, Guid.Empty, DepartmentPK);
				}

				if (Level == RegistryStorageFlags.Company)
				{
					result = HasProposedValue(ItemInternals, CompanyPK, Guid.Empty, Guid.Empty) ?
						ItemInternals.GetProposedValue(CompanyPK, Guid.Empty, Guid.Empty) :
						Item.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty);
				}

				if (Level == RegistryStorageFlags.SystemDepartment)
				{
					result = HasProposedValue(ItemInternals, Guid.Empty, Guid.Empty, DepartmentPK) ?
						ItemInternals.GetProposedValue(Guid.Empty, Guid.Empty, DepartmentPK) :
						Item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, DepartmentPK);
				}

				if (Level == RegistryStorageFlags.System)
				{
					result = HasProposedValue(ItemInternals, Guid.Empty, Guid.Empty, Guid.Empty) ?
						ItemInternals.GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty) :
						Item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				}

				return result;
			}
			set { base.ValueAtThisLevel = value; }
		}

		protected override bool HasActualValueAtThisLevelCore()
		{
			bool result;

			switch (Level)
			{
				case RegistryStorageFlags.BranchDepartment:
					{
						result = HasValue(ItemInternals, Guid.Empty, BranchPK, DepartmentPK);
						break;
					}
				case RegistryStorageFlags.Branch:
					{
						result = HasValue(ItemInternals, Guid.Empty, BranchPK, Guid.Empty);
						break;
					}
				case RegistryStorageFlags.CompanyDepartment:
					{
						result = HasValue(ItemInternals, CompanyPK, Guid.Empty, DepartmentPK);
						break;
					}
				case RegistryStorageFlags.Company:
					{
						result = HasValue(ItemInternals, CompanyPK, Guid.Empty, Guid.Empty);
						break;
					}
				case RegistryStorageFlags.SystemDepartment:
					{
						result = HasValue(ItemInternals, Guid.Empty, Guid.Empty, DepartmentPK);
						break;
					}
				case RegistryStorageFlags.System:
					{
						result = HasValue(ItemInternals, Guid.Empty, Guid.Empty, Guid.Empty);
						break;
					}
				default:
					throw new InvalidCastException("Dont know how to handle level " + Level);
			}

			return result;
		}

		#endregion

		#region Get Current Value

		public FallbackValue GetCurrentValue()
		{
			FallbackValue result;

			if (HasActualValueAtThisLevel())
			{
				result = new FallbackValue(Level, ValueAtThisLevel);
			}
			else
			{
				result = GetDefaultNewValue(IsAtADepartmentLevel());
			}

			if (result.Level == new RegistryStorageFlags())
			{
				result = new FallbackValue(RegistryStorageFlags.All, result.Value);
			}

			return result;
		}

		public object GetCurrentValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (companyPK != Guid.Empty && branchPK != Guid.Empty)
			{
				throw new OdysseyException("Either CompanyPK or BranchPK must be set to Guid.Empty");
			}

			object result;

			if (HasValue(ItemInternals, companyPK, branchPK, departmentPK))
			{
				result = HasProposedValue(ItemInternals, companyPK, branchPK, departmentPK) ?
					ItemInternals.GetProposedValue(companyPK, branchPK, departmentPK) :
					Item.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
			}
			else
			{
				result = ItemInternals.GetDefaultValue(companyPK, branchPK, departmentPK);
			}

			return result;
		}

		#endregion

		#region Implementation

		bool HasValue(IRegistryItemInternals item, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			companyPK = (branchPK == Guid.Empty) ? companyPK : Guid.Empty;
			ValueToUse valueStatus = item.GetCurrentValueToUse(companyPK, branchPK, departmentPK);
			return (valueStatus != ValueToUse.DefaultValue);
		}

		bool HasProposedValue(IRegistryItemInternals item, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			companyPK = (branchPK == Guid.Empty) ? companyPK : Guid.Empty;
			ValueToUse valueStatus = item.GetCurrentValueToUse(companyPK, branchPK, departmentPK);
			return (valueStatus == ValueToUse.ProposedValue);
		}

		#endregion
	}
}
