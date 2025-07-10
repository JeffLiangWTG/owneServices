using System;

namespace Enterprise.Integration
{
	public interface IRegistryDataType
	{
		bool AllowNull { get; }
		string Code { get; }
		Type DataType { get; }
		IRegistryEditorInfo DefaultEditorInfo { get; }
		object DefaultValue { get; }
		bool HasDefaultEditorInfo { get; }
		bool IsFallBackMergeValuesImplemented { get; }
		bool IsFallBackMergeActualValuesImplemented { get; }
		bool IsValidatedOnSetEvenIfEqualDefaultValue { get; }
		bool IsDefaultValueImmutable { get; }

		void CheckConfigurationValid();
		bool ValuesAreEqual(object a, object b);
		object CloneValue(object value);
		object Deserialise(byte[] value);
		object FallBackMergeValues(object fallBackValue, object value);
		void Validate(IRegistryItem registryItem, object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK);
		void ValidateBeforeRegistryFormSave(IRegistryItem registryItem, object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK);
		byte[] Serialise(object value);
		IDisposable SuspendValidation();
		bool IsNullDataRepresentation(object value);
		bool IsDeserializedDataAlive(object value);

		Guid GetGuidValue(object value);
	}
}
