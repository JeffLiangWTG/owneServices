using System;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this attribute to a bound property to describe a meta-data property accessor to
	/// that will also be bound.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class BindingMetaDataPropertyAttribute : Attribute
	{
		/// <param name="metaDataTypeId">The unique identifier of the meta data type.</param>
		/// <param name="metaDataPropertyName">The name of the meta-data property on the control.</param>
		public BindingMetaDataPropertyAttribute(string metaDataTypeId, string metaDataPropertyName)
		{
			Argument.NotNullOrEmpty(metaDataTypeId, nameof(metaDataTypeId));
			Argument.NotNullOrEmpty(metaDataPropertyName, nameof(metaDataPropertyName));
			MetaDataTypeId = metaDataTypeId;
			MetaDataPropertyName = metaDataPropertyName;
			Enabled = true;
		}

		/// <summary>
		/// Get the unique identifier for the MetaDataType of the meta data.
		/// </summary>
		public string MetaDataTypeId { get; private set; }

		/// <summary>
		/// The name of the meta-data property on the control.
		/// </summary>
		public string MetaDataPropertyName { get; private set; }

		/// <summary>
		/// Get whether binding of the meta-data is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		#region Match / Equals / GetHashCode

		public override bool Match(object obj)
		{
			var rhs = obj as BindingMetaDataPropertyAttribute;
			return
				rhs != null &&
				MetaDataTypeId == rhs.MetaDataTypeId &&
				MetaDataPropertyName == rhs.MetaDataPropertyName;
		}

		public override bool Equals(object obj)
		{
			var rhs = obj as BindingMetaDataPropertyAttribute;
			return rhs != null && Match(obj) && Enabled == rhs.Enabled;
		}

		public override int GetHashCode()
		{
			return MetaDataPropertyName.GetHashCode();
		}

		#endregion
	}
}
