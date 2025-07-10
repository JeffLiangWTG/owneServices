using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class RegistryDataType<T> : IRegistryDataType
	{
		protected RegistryDataType(string code, T defaultValue)
		{
			this.code = code;
			this.defaultValue = defaultValue;
		}

		public string Code
		{
			get { return code; }
		}

		public Type DataType
		{
			get { return DataTypeCore; }
		}

		public virtual bool IsNullDataRepresentation(object value)
		{
			return false;
		}

		public virtual bool IsDeserializedDataAlive(object value)
		{
			return true;
		}

		public T DefaultValue
		{
			get { return defaultValue; }
		}

		public virtual bool IsDefaultValueImmutable => false;

		public bool AllowNull
		{
			get { return AllowNullCore; }
		}

		public bool HasDefaultEditorInfo
		{
			get { return HasDefaultEditorInfoCore; }
		}

		public IRegistryEditorInfo DefaultEditorInfo
		{
			get
			{
				if (defaultEditorInfo == null)
				{
					defaultEditorInfo = GetNewDefaultEditorInfo();
				}
				return defaultEditorInfo;
			}
		}

		public bool IsFallBackMergeValuesImplemented
		{
			get { return IsFallBackMergeValuesImplementedCore; }
		}

		public bool IsFallBackMergeActualValuesImplemented
		{
			get { return IsFallBackMergeActualValuesImplementedCore; }
		}

		public bool IsValidatedOnSetEvenIfEqualDefaultValue
		{
			get { return IsValidatedOnSetEvenIfEqualDefaultValueCore; }
		}

		public void CheckConfigurationValid()
		{
			CheckConfigurationValidCore();
		}

		public bool ValuesAreEqual(object a, object b)
		{
			if (AllowNull)
			{
				var aIsNull = (a == null || IsNullDataRepresentation(a));
				var bIsNull = (b == null || IsNullDataRepresentation(b));

				if (aIsNull != bIsNull)
				{
					return false;
				}
				else if (aIsNull)
				{
					return true;
				}
			}

			return ValuesAreEqualCore(CastProposedToT(a, null), CastProposedToT(b, null));
		}

		protected virtual bool ValuesAreEqualCore(T a, T b)
		{
			return ValuesSerializeToTheSameByteArray(a, b);
		}

		protected bool ValuesSerializeToTheSameByteArray(T a, T b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			var aSerialized = Serialise(a);
			var bSerialized = Serialise(b);

			if (aSerialized.Length != bSerialized.Length)
			{
				return false;
			}

			for (int i = 0; i < aSerialized.Length; i++) // I <3 Linq but speed matters
			{
				if (aSerialized[i] != bSerialized[i])
				{
					return false;
				}
			}

			return true;
		}

		public T FallBackMergeValues(T fallBackValue, T value)
		{
			return FallBackMergeValuesCore(fallBackValue, value);
		}

		public void Validate(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!ValidationSuspended)
			{
				/* This violates the basic "Open/Closed" OO princible
				 * This princible states that parent/higher level classes shouldn't have any child-specific implementations or references
				 * As only the Guid data type can have a find box, the decision was made to implement (easily extendable) value validation on find boxes and call it directly, instead of the alternatives below
				 * A) Implementing validation all the way down and merging data type validation with value validation
				 * B) Making a different registry data type for every possible find box type and modifying hundreds (or thousands?) of individual registry items
				 * C) Further extending deprecated registry implementations, interfaces and types where there are no common base classes to inherit from
				 */
				if (registryItem?.EditorInfo is GuidFindBoxRegistryEditorInfo info)
				{
					if (info.FindBoxCollection is RegistryFindBoxCollectionWithValidation<T> findBox)
					{
						findBox.Validate(registryItem, proposedValue, companyPK, branchPK, departmentPK);
					}
				}
				ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
				Validating?.Invoke(this, new RegistryDataTypeValidatingEventArgs<T>(registryItem, proposedValue, companyPK, branchPK, departmentPK));
			}
		}

		public void ValidateBeforeRegistryFormSave(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!ValidationSuspended)
			{
				ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
				Validating?.Invoke(this, new RegistryDataTypeValidatingEventArgs<T>(registryItem, proposedValue, companyPK, branchPK, departmentPK));
			}
		}

		protected virtual Guid GetGuidValue(object value)
		{
			return Guid.Empty;
		}

		public IDisposable SuspendValidation()
		{
			if (ValidationSuspended)
			{
				throw new InvalidOperationException("Validation has already been suspended.");
			}
			return new ValidationSuspender(this);
		}

		public byte[] Serialise(T value)
		{
			return SerialiseCore(value);
		}

		public T Deserialise(byte[] value)
		{
			return DeserialiseCore(value);
		}

		protected virtual bool AllowNullCore
		{
			get { return false; }
		}

		protected virtual Type DataTypeCore
		{
			get { return typeof(T); }
		}

		protected virtual bool HasDefaultEditorInfoCore
		{
			get { return true; }
		}

		protected virtual bool IsFallBackMergeValuesImplementedCore
		{
			get { return false; }
		}

		protected virtual bool IsFallBackMergeActualValuesImplementedCore
		{
			get { return false; }
		}

		protected virtual bool IsValidatedOnSetEvenIfEqualDefaultValueCore
		{
			get { return false; }
		}

		protected bool ValidationSuspended
		{
			get { return validationSuspended; }
		}

		public event EventHandler<RegistryDataTypeValidatingEventArgs<T>> Validating;

		protected virtual void ValidateCore(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
		}

		protected virtual void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected virtual T FallBackMergeValuesCore(T fallBackValue, T value)
		{
			return value;
		}

		protected virtual T CloneValue(T value)
		{
			return value;
		}

		protected virtual void CheckConfigurationValidCore()
		{
		}

		protected virtual IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			throw new NotSupportedException("There is no default editor info available for this data type.");
		}

		protected abstract byte[] SerialiseCore(T value);
		protected abstract T DeserialiseCore(byte[] value);
		bool validationSuspended;
		IRegistryEditorInfo defaultEditorInfo;
		readonly string code;
		readonly T defaultValue;

		#region class ValidationSuspender

		class ValidationSuspender : Disposable
		{
			public ValidationSuspender(RegistryDataType<T> dataType)
			{
				this.dataType = dataType;
				this.dataType.validationSuspended = true;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					dataType.validationSuspended = false;
				}
			}

			readonly RegistryDataType<T> dataType;
		}

		#endregion

		#region IRegistryDataType Members

		object IRegistryDataType.DefaultValue
		{
			get { return DefaultValue; }
		}

		object IRegistryDataType.CloneValue(object value)
		{
			object result;
			if (value == null)
			{
				result = null;
			}
			else
			{
				result = CloneValue((T)value);
			}
			return result;
		}

		object IRegistryDataType.Deserialise(byte[] value)
		{
			return Deserialise(value);
		}

		object IRegistryDataType.FallBackMergeValues(object fallBackValue, object value)
		{
			return FallBackMergeValues((T)fallBackValue, (T)value);
		}

		void IRegistryDataType.Validate(IRegistryItem registryItem, object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			Validate(registryItem, CastProposedToT(proposedValue, registryItem.Name), companyPK, branchPK, departmentPK);
		}

		void IRegistryDataType.ValidateBeforeRegistryFormSave(IRegistryItem registryItem, object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			ValidateBeforeRegistryFormSave(registryItem, CastProposedToT(proposedValue, registryItem.Name), companyPK, branchPK, departmentPK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message.")]
		protected T CastProposedToT(object proposedValue, string nameForDebugging)
		{
			try
			{
				if (proposedValue == null)
				{
					return default(T);
				}
				else if (proposedValue is T)
				{
					return (T)proposedValue;
				}

				var typeDescriptor = TypeDescriptor.GetConverter(proposedValue);
				if (typeDescriptor.CanConvertTo(typeof(T)))
				{
					return (T)typeDescriptor.ConvertTo(proposedValue, typeof(T));
				}

				typeDescriptor = TypeDescriptor.GetConverter(typeof(T));
				if (typeDescriptor.CanConvertFrom(proposedValue.GetType()))
				{
					return (T)typeDescriptor.ConvertFrom(proposedValue);
				}

				throw new ArgumentException("Parameters should be of type " + typeof(T) + " but was of type " + (proposedValue.GetType().FullName ?? "NULL"));
			}
			catch (NullReferenceException ex)
			{
				ex.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(ex, string.Format(CultureInfo.InvariantCulture, "Item Name: {0}. Type: {1}. Caused exception with following message: {2}", nameForDebugging, this, ex.Message));
				throw;
			}
		}

		byte[] IRegistryDataType.Serialise(object value)
		{
			return Serialise(CastProposedToT(value, null));
		}

		Guid IRegistryDataType.GetGuidValue(object value)
		{
			return GetGuidValue(value);
		}

		#endregion
	}

	public class RegistryDataTypeValidatingEventArgs<T> : EventArgs
	{
		public RegistryDataTypeValidatingEventArgs(IRegistryItem registryItem, T proposedValue, Guid companyPK,
			Guid branchPK, Guid departmentPK)
		{
			RegistryItem = registryItem;
			ProposedValue = proposedValue;
			CompanyPK = companyPK;
			BranchPK = branchPK;
			DepartmentPK = departmentPK;
		}

		public IRegistryItem RegistryItem { get; }
		public T ProposedValue { get; }
		public Guid CompanyPK { get; }
		public Guid BranchPK { get; }
		public Guid DepartmentPK { get; }
	}
}
