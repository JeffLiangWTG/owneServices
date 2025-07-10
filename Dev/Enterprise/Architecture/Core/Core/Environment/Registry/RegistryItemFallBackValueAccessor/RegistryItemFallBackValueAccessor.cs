using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryItemFallBackValueAccessor
	{
		public RegistryItemFallBackValueAccessor(IRegistryItem item, Guid companyPK, Guid branchPK, Guid departmentPK)
			: this(item, RegistryStorageFlags.BranchDepartment, companyPK, branchPK, departmentPK)
		{
		}

		public RegistryItemFallBackValueAccessor(IRegistryItem item, RegistryStorageFlags level, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			Item = item;
			Level = level;

			CompanyPK = companyPK;
			BranchPK = branchPK;
			DepartmentPK = departmentPK;

#if DEBUG
			if (level != RegistryStorageFlags.BranchDepartment &&
				level != RegistryStorageFlags.Branch &&
				level != RegistryStorageFlags.CompanyDepartment &&
				level != RegistryStorageFlags.Company &&
				level != RegistryStorageFlags.SystemDepartment &&
				level != RegistryStorageFlags.System)
			{
				throw new ArgumentException("You cannot select a combination of levels", nameof(level));
			}
#endif
		}

		/// <summary>
		/// Is this accessor at a non-department level and has an ambiguous match at a parent department level?
		/// </summary>
		public bool IsDefaultValueForThisLevelAmbiguous()
		{
			bool result = false;
			if (!IsAtADepartmentLevel() &&
				FallBackParent != null &&
				FallBackParent.IsAtADepartmentLevel())
			{
				if (FallBackParent.HasActualValueAtThisLevelForAnyDepartment())
				{
					result = true;
				}
				else
				{
					result = FallBackParent.IsDefaultValueForThisLevelAmbiguous();
				}
			}
			return result;
		}

		#region Storage/Value at this Level

		public bool HasStorageAtThisLevel()
		{
			return (Item.Storage & Level) != 0;
		}

		protected virtual object ValueAtThisLevel
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
					result = Item.GetValueWithoutFallback(Guid.Empty, BranchPK, DepartmentPK);
				}

				if (Level == RegistryStorageFlags.Branch)
				{
					result = Item.GetValueWithoutFallback(Guid.Empty, BranchPK, Guid.Empty);
				}

				if (Level == RegistryStorageFlags.CompanyDepartment)
				{
					result = Item.GetValueWithoutFallback(CompanyPK, Guid.Empty, DepartmentPK);
				}

				if (Level == RegistryStorageFlags.Company)
				{
					result = Item.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty);
				}

				if (Level == RegistryStorageFlags.SystemDepartment)
				{
					result = Item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, DepartmentPK);
				}

				if (Level == RegistryStorageFlags.System)
				{
					result = Item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				}

				return result;
			}
			set
			{
				if (!HasStorageAtThisLevel())
				{
					throw new InvalidOperationException("No value is stored at this level");
				}

				if (Level == RegistryStorageFlags.BranchDepartment)
				{
					Item.SetValue(Guid.Empty, BranchPK, DepartmentPK, value);
				}

				if (Level == RegistryStorageFlags.Branch)
				{
					Item.SetValue(Guid.Empty, BranchPK, Guid.Empty, value);
				}

				if (Level == RegistryStorageFlags.CompanyDepartment)
				{
					Item.SetValue(CompanyPK, Guid.Empty, DepartmentPK, value);
				}

				if (Level == RegistryStorageFlags.Company)
				{
					Item.SetValue(CompanyPK, Guid.Empty, Guid.Empty, value);
				}

				if (Level == RegistryStorageFlags.SystemDepartment)
				{
					Item.SetValue(Guid.Empty, Guid.Empty, DepartmentPK, value);
				}

				if (Level == RegistryStorageFlags.System)
				{
					Item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		public bool HasActualValueAtThisLevel()
		{
			return HasActualValueAtThisLevelCore();
		}

		protected virtual bool HasActualValueAtThisLevelCore()
		{
			bool result;

			switch (Level)
			{
				case RegistryStorageFlags.BranchDepartment:
					result = ItemInternals.HasActualValue(Guid.Empty, BranchPK, DepartmentPK);
					break;
				case RegistryStorageFlags.Branch:
					result = ItemInternals.HasActualValue(Guid.Empty, BranchPK, Guid.Empty);
					break;
				case RegistryStorageFlags.CompanyDepartment:
					result = ItemInternals.HasActualValue(CompanyPK, Guid.Empty, DepartmentPK);
					break;
				case RegistryStorageFlags.Company:
					result = ItemInternals.HasActualValue(CompanyPK, Guid.Empty, Guid.Empty);
					break;
				case RegistryStorageFlags.SystemDepartment:
					result = ItemInternals.HasActualValue(Guid.Empty, Guid.Empty, DepartmentPK);
					break;
				case RegistryStorageFlags.System:
					result = ItemInternals.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty);
					break;
				default:
					throw new InvalidCastException("Dont know how to handle level " + Level);
			}

			return result;
		}

		public FallbackValue GetFallBackValue()
		{
			FallbackValue result = new FallbackValue(RegistryStorageFlags.All, null);

			if (Item.DataType.IsFallBackMergeValuesImplemented)
			{
				result = GetFallbackMergedValue(result);
			}
			else if (Level == RegistryStorageFlags.BranchDepartment)
			{
				// do it this way for 'all levels' for performance reasons
				result = new FallbackValue(RegistryStorageFlags.BranchDepartment, ItemInternals.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
			}
			else if (HasActualValueAtThisLevel())
			{
				result = new FallbackValue(Level, ValueAtThisLevel);
			}
			else if (FallBackParent != null)
			{
				result = FallBackParent.GetFallBackValue();
			}

			if (result.Value == null && FallBackParent == null)
			{
				result = new FallbackValue(result.Level, ItemInternals.GetDefaultValue(CompanyPK, BranchPK, DepartmentPK));
			}

			if (Item.DataType.IsFallBackMergeActualValuesImplemented)
			{
				result = GetFallbackMergedValue(result);
			}

			return result;
		}

		#endregion

		#region Get Default New Value

		/// <summary>
		/// Get the default value to be used when creating a new value at this level. This looks at the value at the FallBackParent,
		/// then if that can't be found it uses the registry item's default value. If you're not in a department it won't return a parent
		/// department value because we don't know which department to get the value from.
		/// </summary>
		public FallbackValue GetDefaultNewValue()
		{
			FallbackValue result = GetDefaultNewValue(IsAtADepartmentLevel());

			if (result.Level == new RegistryStorageFlags())
			{
				result = new FallbackValue(RegistryStorageFlags.All, result.Value);
			}

			return result;
		}

		protected FallbackValue GetDefaultNewValue(bool requestOriginatedFromDepartment)
		{
			FallbackValue result;

			if (FallBackParent == null)
			{
				result = new FallbackValue(RegistryStorageFlags.All, ItemInternals.GetDefaultValue(CompanyPK, BranchPK, DepartmentPK));
			}
			else if (
				requestOriginatedFromDepartment &&
				!FallBackParent.IsAtADepartmentLevel() &&
				!FallBackParent.HasActualValueAtThisLevel())
			{
				result = FallBackParent.GetDefaultNewValue(requestOriginatedFromDepartment);
			}
			else if (
				FallBackParent != null &&
				(!FallBackParent.IsAtADepartmentLevel() || requestOriginatedFromDepartment))
			{
				result = FallBackParent.GetFallBackValue();
			}
			else
			{
				result = FallBackParent.GetDefaultNewValue(requestOriginatedFromDepartment);
			}

			return result;
		}

		#endregion

		#region FallBack Parent

		public RegistryItemFallBackValueAccessor FallBackParent
		{
			get
			{
				if (!fFallBackParentFound)
				{
					if (Level != RegistryStorageFlags.System)
					{
						fFallBackParent = GetFallBackParent();
						if (!fFallBackParent.HasStorageAtThisLevel())
						{
							fFallBackParent = fFallBackParent.FallBackParent;
						}
						fFallBackParentFound = true;
					}
				}

				return fFallBackParent;
			}
		}

		protected virtual RegistryItemFallBackValueAccessor GetFallBackParent()
		{
			return new RegistryItemFallBackValueAccessor(Item, GetParentLevel(Level), CompanyPK, BranchPK, DepartmentPK);
		}

		protected RegistryItemFallBackValueAccessor fFallBackParent;
		protected bool fFallBackParentFound;

		#endregion

		#region Fallback Merged Value

		FallbackValue GetFallbackMergedValue(FallbackValue currentFallbackValue)
		{
			FallbackValue result = currentFallbackValue;

			bool hasStorageAndActualValueAtThisLevel = HasStorageAtThisLevel() && HasActualValueAtThisLevel();

			result = new FallbackValue(new RegistryStorageFlags(), result.Value);

			if (hasStorageAndActualValueAtThisLevel)
			{
				result = new FallbackValue(Level, ValueAtThisLevel);
			}

			if (FallBackParent != null)
			{
				FallbackValue parentFallBackValue = FallBackParent.GetFallBackValue();
				RegistryStorageFlags storageToSet = result.Level | parentFallBackValue.Level;

				result = MergeCurrentAndParentFallbackLevel(hasStorageAndActualValueAtThisLevel, storageToSet, parentFallBackValue.IsMergedWithDefaultValue, result.Value, parentFallBackValue.Value);
			}

			return result;
		}

		FallbackValue MergeCurrentAndParentFallbackLevel(bool hasStorageAndActualValueAtThisLevel, RegistryStorageFlags level, bool isMergedWithDefaultValue, object currentValue, object parentFallbackValue)
		{
			return (hasStorageAndActualValueAtThisLevel) ?
				new FallbackValue(level, isMergedWithDefaultValue, Item.DataType.FallBackMergeValues(currentValue, parentFallbackValue)) :
				new FallbackValue(level, isMergedWithDefaultValue, parentFallbackValue);
		}

		#endregion

		#region Implementation

		protected RegistryStorageFlags GetParentLevel(RegistryStorageFlags level)
		{
			RegistryStorageFlags result;
			switch (level)
			{
				case RegistryStorageFlags.BranchDepartment:
					result = RegistryStorageFlags.Branch;
					break;
				case RegistryStorageFlags.Branch:
					result = (DepartmentPK == Guid.Empty) ?
													  RegistryStorageFlags.Company : RegistryStorageFlags.CompanyDepartment;
					break;
				case RegistryStorageFlags.CompanyDepartment:
					result = RegistryStorageFlags.Company;
					break;
				case RegistryStorageFlags.Company:
					result = (DepartmentPK == Guid.Empty) ?
													   RegistryStorageFlags.System : RegistryStorageFlags.SystemDepartment;
					break;
				case RegistryStorageFlags.SystemDepartment:
					result = RegistryStorageFlags.System;
					break;
				default:
					throw new InvalidOperationException("Don't know how to handle level " + level);
			}
			return result;
		}

		bool HasActualValueAtThisLevelForAnyDepartment()
		{
			if (!fHasActualValueAtThisLevelForAnyDepartmentPopulated)
			{
				Guid companyPKToUse = Guid.Empty;
				Guid branchPKToUse = Guid.Empty;
				if (Level == RegistryStorageFlags.Branch || Level == RegistryStorageFlags.BranchDepartment)
				{
					branchPKToUse = CompanyPK;
				}
				if (Level == RegistryStorageFlags.Company || Level == RegistryStorageFlags.CompanyDepartment)
				{
					companyPKToUse = CompanyPK;
				}
				fHasActualValueAtThisLevelForAnyDepartment = ItemInternals.HasActualValueAtThisLevelForAnyDepartment(
					companyPKToUse, branchPKToUse);
				fHasActualValueAtThisLevelForAnyDepartmentPopulated = true;
			}
			return fHasActualValueAtThisLevelForAnyDepartment;
		}

		protected bool IsAtADepartmentLevel()
		{
			return
				Level == RegistryStorageFlags.BranchDepartment ||
				Level == RegistryStorageFlags.CompanyDepartment ||
				Level == RegistryStorageFlags.SystemDepartment;
		}

		bool fHasActualValueAtThisLevelForAnyDepartment;
		bool fHasActualValueAtThisLevelForAnyDepartmentPopulated;

		protected IRegistryItemInternals ItemInternals
		{
			get { return (IRegistryItemInternals)Item; }
		}

		public readonly IRegistryItem Item;
		public readonly RegistryStorageFlags Level;
		public readonly Guid CompanyPK;
		public readonly Guid BranchPK;
		public readonly Guid DepartmentPK;

		#endregion
	}
}
