using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// The base class for meta-data locator attributes that provide only 1 value or member.
	/// </summary>
	public abstract class SingleMetaDataAttribute : MetaDataBaseAttribute
	{
		protected SingleMetaDataAttribute(string metaDataTypeId)
		{
			Argument.NotNull(metaDataTypeId, nameof(metaDataTypeId));
			MetaDataTypeId = metaDataTypeId;
		}

		/// <summary>
		/// Get the enumeration value that identifies the type of meta data.
		/// </summary>
		public string MetaDataTypeId { get; private set; }

		/// <summary>
		/// Get the MetaDataType object associated with the identifier passed into the constructor.
		/// </summary>
		public MetaDataType MetaDataType
		{
			get
			{
				return MetaDataType.GetMetaDataType(MetaDataTypeId);
			}
		}

		public override bool Match(object obj)
		{
			return obj != null && GetType() == obj.GetType() && ((SingleMetaDataAttribute)obj).MetaDataTypeId == MetaDataTypeId;
		}
	}
}
