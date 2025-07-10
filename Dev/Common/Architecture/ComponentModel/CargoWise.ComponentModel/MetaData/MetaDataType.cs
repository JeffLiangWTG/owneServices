using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Describes a meta data item type that can be returned from MetaData.
	/// <see>CargoWise.ComponentModel.MetaData</see>
	/// </summary>
	public class MetaDataType
	{
		/// <summary>
		/// Constructor to associate the identifier and meta data return type that
		/// is used to match the meta data accessor property.
		/// </summary>
		/// <param name="id">The unique identifier for this meta data item.</param>
		/// <param name="dataType">The data type of the meta data.</param>
		/// <param name="defaultValue">The default value of the meta-data.</param>
		public MetaDataType(string id, Type dataType, object defaultValue)
		{
			Id = id;
			DataType = dataType;
			DefaultValue = defaultValue;
		}

		/// <summary>
		/// Constructor that also takes an array of meta data type IDs that must also exist on
		/// the same property.
		/// </summary>
		public MetaDataType(string id, Type dataType, object defaultValue, string[] otherRequiredMetaDataIds)
			: this(id, dataType, defaultValue)
		{
			Argument.NotNull(otherRequiredMetaDataIds, nameof(otherRequiredMetaDataIds));
			this.otherRequiredMetaDataIds = otherRequiredMetaDataIds;
		}

		#region Factory Methods

		readonly static CopyOnWrite<ImmutableHashSet<MetaDataType>> metaDataTypes = new CopyOnWrite<ImmutableHashSet<MetaDataType>>(ImmutableHashSet<MetaDataType>.Empty);

		static MetaDataType()
		{
			MetaDataTypes.RegisterTypes();
		}

		/// <summary>
		/// Register a MetaDataType with the system. You cannot register the same MetaDataType twice.
		/// </summary>
		public static void RegisterMetaDataType(MetaDataType type)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
			var existingInfo = TryGetMetaDataType(type.Id);
			if (existingInfo != null && !object.Equals(type, existingInfo))
			{
				throw new ArgumentException("The MetaDataType '" + type.Id + "' is already defined");
			}
			metaDataTypes.UpdateValue((ImmutableHashSet<MetaDataType> set) =>
			{
				if (!set.Contains(type))
				{
					set = set.Add(type);
				}
				return set;
			});
		}

		/// <summary>
		/// Get a MetaDataType object given its unique identifier.
		/// </summary>
		public static MetaDataType GetMetaDataType(string id)
		{
			var result = TryGetMetaDataType(id) ??
				throw new ArgumentException(
					"Meta data type '" + id +
					"' is not registered. To register a MetaDataType, call " +
					nameof(MetaDataType) +
					".RegisterMetaDataType before the KPropertyDescriptor is " +
					"initially created. Typically this should be done on application " +
					"initialisation.");
			return result;
		}

		/// <summary>
		/// Get a MetaDataType object given its unique identifier.
		/// </summary>
		public static MetaDataType TryGetMetaDataType(string id)
		{
			var metaDataTypes = GetRegisteredMetaDataTypes();
			foreach (var metaDataType in metaDataTypes)
			{
				if (metaDataType.Id == id)
				{
					return metaDataType;
				}
			}
			return null;
		}

		/// <summary>
		/// Does the meta-data item with the given ID exist?
		/// </summary>
		public static bool IsRegistered(string id)
		{
			return TryGetMetaDataType(id) != null;
		}

		/// <summary>
		/// Get all the MetaDataType objects that are currently registered.
		/// </summary>
		public static IEnumerable<MetaDataType> GetRegisteredMetaDataTypes()
		{
			var result = metaDataTypes.GetValue();
			return result;
		}

		#endregion

		/// <summary>
		/// The unique identifier for this meta data type.
		/// </summary>
		public string Id { get; private set; }

		/// <summary>
		/// Get an array of meta data type IDs that must also be defined on the same property.
		/// </summary>
		public string[] OtherRequiredMetaDataIds
		{
			get { return (string[])otherRequiredMetaDataIds.Clone(); }
		}
		readonly string[] otherRequiredMetaDataIds = Array.Empty<string>();

		/// <summary>
		/// Get the type of the value that is returned by a meta data accessor property.
		/// </summary>
		public Type DataType { get; private set; }

		public override bool Equals(object obj)
		{
			var rhs = obj as MetaDataType;
			return rhs != null &&
				Id == rhs.Id &&
				GetType() == rhs.GetType() &&
				DataType == rhs.DataType;
		}

		public override int GetHashCode()
		{
			return Id != null ? Id.GetHashCode() : -1;
		}

		/// <summary>
		/// The default value to use if the meta-data value is not specified. Note that this
		/// value is not used if a collection with no items is bound, because .NET automatically
		/// uses DBNull.Value in this case.
		/// </summary>
		public object DefaultValue { get; private set; }

		/// <summary>
		/// Validate the value of the meta-data. This is typically used for the entity verifier,
		/// not at runtime.
		/// <returns>Null if the type if valid or a reason why it is not valid.</returns>
		/// </summary>
		public virtual string ValidateMetaDataValue(Type entityType, PropertyDescriptor property, object value)
		{
			return null;
		}

		/// <summary>
		/// Get whether the meta-data cannot be calculated.
		/// </summary>
		public virtual bool AllowConstantValueOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Get whether the meta-data type must be specified on a bound property if the control requests it
		/// (ie no default value available).
		/// </summary>
		public virtual bool IsMandatory
		{
			get { return false; }
		}

		/// <summary>
		/// Get whether to allow a sub-class of a component to re-direct the accessor member of the meta-data.
		/// </summary>
		public virtual bool AllowReintroduceMember
		{
			get { return true; }
		}

		/// <summary>
		/// Get whether when binding to the meta data, the data should flow only from the data source to the control and not vice versa.
		/// </summary>
		public virtual bool IsOneWay
		{
			get { return true; }
		}
	}
}
