using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	[Flags]
	public enum RegistryStorageFlags
	{
		System = 1 << 0,
		SystemDepartment = 1 << 1,
		Company = 1 << 2,
		CompanyDepartment = 1 << 3,
		Branch = 1 << 4,
		BranchDepartment = 1 << 5,
		All = 0xFFFFFFF
	}

	public enum DepartmentFlags
	{
		Air,
		Sea,
		ImportAir,
		ImportSea,
		ExportAir,
		ExportSea,
		All
	}

	[Flags]
	public enum RegistryOptions
	{
		Default = 0,
		IsHidden = 1 << 0,
		NotCached = 1 << 1,
		NotLogged = 1 << 2,
		IsOnlyForDevelopers = 1 << 4,
		IsReadOnly = 1 << 5,
		IsValueMandatory = 1 << 6,
		IsValueOptional = 1 << 7,
		MustOverrideDefaultValue = 1 << 8,
		CannotCallParameterlessValueGetter = 1 << 9,
		IsOnlyForSupport = 1 << 11,
		IsOnlyEditableBySupportIfHosted = 1 << 12,
		IsOnlyForCargoWise = 1 << 13,
		IsOnlyForController = 1 << 14,
		PreserveTestValue = 1 << 15,
		IsPasswordVisibleForControllerUser = 1 << 16,
		NotMustOverrideDefaultValueForCompanies = 1 << 17,
		IsOnlyForClassic_eAdaptor = 1 << 18,
		IsOnlyFor_eAdaptorNext = 1 << 19,

		/// <summary>
		/// By default, registry default values are not cached and evaluated on every registry access. Setting this registry option will cache default values.
		/// This is useful when a dynamic default value is dependent on a database lookup of company / branch / department.
		/// Do not use if you have a constant default.
		/// </summary>
		CacheExpensiveDefaultValue = 1 << 20,
	}

	public enum ValueToUse
	{
		DefaultValue,
		SavedValue,
		ProposedValue
	}

	public interface IRegistryItem : IAuditDetails
	{
		string Name { get; set; }
		string Category { get; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		string[] Categories { get; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		string[] CategoriesUntranslated { get; }
		string Hint { get; }
		string Caption { get; }
		IRegistryDataType DataType { get; set; }
		IRegistryEditorInfo EditorInfo { get; set; }

		RegistryStorageFlags Storage { get; }
		RegistryOptions Options { get; set; }
		bool HasOption(RegistryOptions option);
		DepartmentFlags DepartmentsAllowed { get; set; }

		bool IsReadOnly { get; }
		bool IsLockedDown { get; }
		bool CanBeExported { get; }
		object DefaultValue { get; }

		bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department = null, IRegistryItemVisibility registryItemVisibility = null);
		string GetValidationErrorMessage(object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK);
		IEnumerable<Guid> CountryFilterPKs { get; set; }

		bool IsValueMandatory { get; }
		object GetFallBackValueAtAllLevels(Guid companyPK, Guid branchPK, Guid departmentPK);
		object GetValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK);
		void SetValue(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue);
		object Value { get; }
		string GetLocationInEnglish();
		string GetLocation();
	}

	public static class IRegistryItemExtension
	{
#if DEBUG
		public static IDisposable SetTemporaryValue(this IRegistryItem registryItem, Guid companyPk, Guid branchPk, Guid departmentPk, object temporaryValue)
		{
			if (!WTG.TestHelpers.TestingState.IsTest && !NUnit.Framework.TransactionedTestCase.InTransactionedTestCase)
			{
				throw new InvalidOperationException("SetTemporaryValue can only be used in a test case");
			}

			var previousValue = registryItem.GetValueWithoutFallback(companyPk, branchPk, departmentPk);
			registryItem.SetValue(companyPk, branchPk, departmentPk, temporaryValue);

			DisposableAction action = null;

			action = new DisposableAction(delegate
			{
				registryItem.SetValue(companyPk, branchPk, departmentPk, previousValue);
				CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(action);
			});

			if (!NUnit.Framework.TransactionedTestCase.InTransactionedTestCase) // A transaction test case will roll back any changes made to the registry.
			{
				CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(action);
			}

			return action;
		}
#endif
	}

	public interface IRegistryItemWithOtherChangedItems : IRegistryItemInternals
	{
		IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }
	}

	public delegate void RegistryUpdateActionDelegate(Guid companyPK, Guid branchPK, Guid departmentPK, object newValue);

	public interface IRegistryItemInternals : IRegistryItem
	{
		string Location { get; }
		string LocationForCategory(string category);
		void CheckConfigurationValid();

		object GetDefaultValue(Guid companyPK, Guid branchPK, Guid departmentPK);
		object GetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK);
		void SetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK, object value);
		RegistryStorageFlags GetFallBackLevel(Guid companyPK, Guid branchPK, Guid departmentPK);
		bool HasActualValueAtThisLevelForAnyDepartment(Guid companyPK, Guid branchPK);
		object GetCurrentValueFromProposedValueAccessor(Guid companyPK, Guid branchPK, Guid departmentPK);
		ValueToUse GetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK);
		void SetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK, ValueToUse value);

		bool HasValueForAnyLevel();
		bool HasActualValue(Guid companyPK, Guid branchPK, Guid departmentPK);
		void DeleteRecord(Guid companyPK, Guid branchPK, Guid departmentPK);
		void DeleteValue(Guid companyPK, Guid branchPK, Guid departmentPK);
		void ClearCache();
		void ClearCache(Guid companyPK, Guid branchPK, Guid departmentPK);
		void ClearProposedCache();
		void ClearCurrentValueToUseCache();

		Guid GetRegistryItemPK(Guid companyPK, Guid branchPK, Guid departmentPK);

		RegistryUpdateActionDelegate BeforeUpdateAction { get; set; }

		RegistryUpdateActionDelegate OnUpdateAction { get; set; }

		RegistryUpdateActionDelegate OnDeleteAction { get; set; }

		Action OnAllValuesSavedAction { get; set; }
	}

	public static class RegistryItemInternalExtensions
	{
		public static void BeforeUpdate(this IRegistryItemInternals registry, Guid companyPK, Guid branchPK, Guid departmentPK, object currentValue)
		{
			Argument.NotNull(registry, nameof(registry));
			registry.BeforeUpdateAction?.Invoke(companyPK, branchPK, departmentPK, currentValue);
		}

		public static void OnUpdate(this IRegistryItemInternals registry, Guid companyPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			Argument.NotNull(registry, nameof(registry));
			registry.OnUpdateAction?.Invoke(companyPK, branchPK, departmentPK, newValue);
		}

		public static void OnDelete(this IRegistryItemInternals registry, Guid companyPK, Guid branchPK, Guid departmentPK, object oldValue)
		{
			Argument.NotNull(registry, nameof(registry));
			registry.OnDeleteAction?.Invoke(companyPK, branchPK, departmentPK, oldValue);
		}

		public static void OnAllValuesSaved(this IRegistryItemInternals registry)
		{
			Argument.NotNull(registry, nameof(registry));
			registry.OnAllValuesSavedAction?.Invoke();
		}
	}
}
