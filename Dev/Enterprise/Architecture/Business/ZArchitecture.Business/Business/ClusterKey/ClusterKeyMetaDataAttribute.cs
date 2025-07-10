using System;
using System.Xml.Serialization;
using CargoWise.Definitions;

namespace Enterprise.ZArchitecture.Business
{
	[Serializable, XmlSerializerAssembly("Enterprise.ZArchitecture.Business.XmlSerializers")]
	public sealed class ClusterKeyMetaDataAttribute : AssemblyMetaDataAttributeWithType, IEquatable<ClusterKeyMetaDataAttribute>
	{
		public string ParentTableName { get; set; }

		public string ParentFkColumnName { get; set; }

		public string SecondaryParentTableName { get; set; }

		public string SecondaryParentFkColumnName { get; set; }

		public string TableName { get; set; }

		public ClusterKeyMetaDataAttribute() { }

		public ClusterKeyMetaDataAttribute(Type type) : base(type)
		{
		}

		#region Equality members

		public bool Equals(ClusterKeyMetaDataAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) &&
				   (ParentTableName,
					   ParentFkColumnName,
					   SecondaryParentTableName,
					   SecondaryParentFkColumnName,
					   TableName)
				   .Equals((other.ParentTableName,
					   other.ParentFkColumnName,
					   other.SecondaryParentTableName,
					   other.SecondaryParentFkColumnName,
					   other.TableName));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is ClusterKeyMetaDataAttribute other && Equals(other);
		}

		public override int GetHashCode()
			=> (ParentTableName,
					ParentFkColumnName,
					SecondaryParentTableName,
					SecondaryParentFkColumnName,
					TableName).GetHashCode();

		#endregion
	}
}
