using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	#region RegistryEditor Attribute

	public sealed class RegistryEditorAttribute : Attribute
	{
		/// <summary>
		/// Specifies the RegistryItemEditor to use for this RegistryDataType.
		/// </summary>
		/// <param name="typeName">The fully qualified type name of the editor.</param>
		public RegistryEditorAttribute(string typeName)
		{
			this.TypeName = typeName;
		}

		public string TypeName;
	}

	#endregion

	public abstract class WeaklyTypedNonPersistentBusinessObjectRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IRegistryBusiness>
	{
		protected WeaklyTypedNonPersistentBusinessObjectRegistryDataType(Type dataType)
			: base((IRegistryBusiness)Activator.CreateInstance(dataType))
		{
			this.dataType = dataType;
		}

		protected override Type DataTypeCore
		{
			get { return dataType; }
		}

		readonly Type dataType;
	}

	public abstract class NonPersistentBusinessObjectRegistryDataType<T> : RegistryDataType<T> where T : IRegistryBusiness
	{
		#region Construction

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected NonPersistentBusinessObjectRegistryDataType()
				: this(Activator.CreateInstance<T>())
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected NonPersistentBusinessObjectRegistryDataType(T defaultValue)
				: this(RegistryDataTypes.Codes.Binary, defaultValue)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected NonPersistentBusinessObjectRegistryDataType(string code, T defaultValue)
				: base(code, defaultValue)
		{
			if (IsDefaultValueImmutable)
			{
				defaultValue.HasChangesChanged += DefaultValue_HasChangesChanged;
			}
		}

		void DefaultValue_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			throw new InvalidOperationException("Do not change the default value of a Registry Item. Clone first.");
		}

		#endregion

		#region Serialise/Deserialise

		protected override byte[] SerialiseCore(T value)
		{
			byte[] result = null;
			ZXmlSerializer serialiser = ZXmlSerializer.New(DataType);

			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, new UnicodeEncoding(false, false)))
			{
				serialiser.Serialize(writer, value);
				writer.Flush();
				result = stream.ToArray();
			}

			return result;
		}

		protected override T DeserialiseCore(byte[] value)
		{
			T result = default(T); // the value will definately be assigned below

			if (value.Length > 0)
			{
				ZXmlSerializer serialiser = ZXmlSerializer.New(DataType);

				using (MemoryStream stream = new MemoryStream(value))
				using (XmlTextReader reader = new XmlTextReader(stream))
				{
					result = (T)serialiser.Deserialize(reader);
				}
			}
			else
			{
				result = DefaultValue;
			}

			return result;
		}

		#endregion

		protected override void ValidateCore(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			proposedValue.RunPreSaveValidation();
			if (proposedValue.HasErrors())
			{
				IEnumerable<INotification> errors = new ZNotificationCollector(proposedValue, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				throw new RegistryValidationException(errors.ToUniqueMessageListString());
			}
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override bool AllowNullCore
		{
			get { return false; }
		}

		protected override T CloneValue(T value)
		{
			if (value != null)
			{
				return (T)value.Clone(null, null);
			}

			return value;
		}
	}

	public abstract class NonPersistentBusinessObjectCollectionRegistryDataType<T> : NonPersistentBusinessObjectRegistryDataType<T> where T : RegistryBusinessObjectCollection
	{
		protected override T DeserialiseCore(byte[] value)
		{
			T result = base.DeserialiseCore(value);
			var emptyItems = (from RegistryBusinessObject item in result
							  where item.Code.IsEmpty && item.Description.IsEmpty
							  select item).ToArray();

			foreach (var emptyItem in emptyItems)
			{
				result.Remove(emptyItem);
			}

			return result;
		}
	}
}
