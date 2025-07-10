namespace Enterprise.AuditDataServices.Business
{
	using System;
	using System.Text.RegularExpressions;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;

	public class AuditEntity
	{
		public AuditEntity(SchemaColumn keyColumn, SchemaStringColumn infoColumn)
		{
			if (keyColumn == null)
			{
				throw new ArgumentNullException(nameof(keyColumn));
			}
			if (keyColumn is not SchemaGuidColumn && keyColumn is not SchemaIntColumn)
			{
				throw new ArgumentException("keyColumn must be a Guid or Int column.", nameof(keyColumn));
			}
			if (keyColumn is SchemaIntColumn && !keyColumn.Name.EndsWith(Schema.ClusterKeyColumnSuffix))
			{
				throw new ArgumentException("Int keyColumns must be cluster key columns.", nameof(keyColumn));
			}

			this.keyColumn = keyColumn;
			this.infoColumn = infoColumn;
		}

		public SchemaColumn KeyColumn
		{
			get { return keyColumn; }
		}
		readonly SchemaColumn keyColumn;

		public SchemaStringColumn InfoColumn
		{
			get { return infoColumn; }
		}
		readonly SchemaStringColumn infoColumn;

		public override string ToString()
		{
			if (description == null)
			{
				description =
					DataBoundResourceStrings.GetTableDescriptiveName(keyColumn.TableName)
					+ (
						singleKeyEntityRegex.IsMatch(keyColumn.Name) || keyColumn.Name.EndsWith(Schema.ClusterKeyColumnSuffix)
							? ""
							: " | " + ZPropertyInfo.GetFriendlyColumnNameShared(keyColumn.Name)
					);
			}

			return description;
		}
		string description;

		static readonly Regex singleKeyEntityRegex = new Regex(@"^[A-Z0-9]{2,3}_[A-Z0-9]{2,3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}
}
