using System;

namespace Enterprise.DataTransfer.Native.Common.CodeMappings
{
	public class CodeMapping
	{
		public string TableName { get; set; }
		public string Relationship { get; set; }
		public string RelationshipResolver { get; set; }
		public string PropertyName { get; set; }
		public string ForeignCode { get; set; }
		public string LocalCode { get; set; }
		public Guid LocalGuid { get; set; }
		public string OwnerCode { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is CodeMapping))
			{
				return false;
			}
			return base.Equals(obj);
		}

		public bool Equals(CodeMapping other)
		{
			if (other == null)
			{
				return false;
			}
			if (other.TableName != TableName || other.PropertyName != PropertyName)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			int hashTableName = TableName == null ? 0 : TableName.GetHashCode();
			int hashPropertyName = PropertyName == null ? 0 : PropertyName.GetHashCode();

			return hashTableName ^ hashPropertyName;
		}
	}
}