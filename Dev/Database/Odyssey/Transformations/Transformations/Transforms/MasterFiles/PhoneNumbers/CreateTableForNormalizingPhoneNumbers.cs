using System.Globalization;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterFiles
{
	/// <summary>
	/// Contains the base implementation to create a client table for further phone numbers normalization.
	/// </summary>
	public abstract class CreateTableForNormalizingPhoneNumbers : DataTransformation
	{
		#region Interface Implementation

		protected override void OfflinePostUpgradeTransform()
		{
			using (var command = Db.Connection.Command(MainSql)) // This is an online data transformation.
			{
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region Configurations

		/// <summary>
		/// Gets the name of the client table.
		/// </summary>
		internal abstract string ClientTableName { get; }

		/// <summary>
		/// Gets the name of the column that stores the PKs of the phone number rows.
		/// </summary>
		internal abstract string ClientPkColumnName { get; }

		/// <summary>
		/// Gets the SQL that inserts data into the client table.
		/// </summary>
		protected abstract string InsertDataSql { get; }

		#endregion

		#region Implementations

		string MainSql
		{
			get { return string.Format(CultureInfo.InvariantCulture, @"IF OBJECT_ID(N'{0}', N'U') IS NOT NULL
BEGIN
	DROP TABLE [{0}];
END
CREATE TABLE [{0}] (
	[{1}] uniqueidentifier not null,
);
ALTER TABLE [{0}]
	ADD CONSTRAINT [PK_UX__{1}] PRIMARY KEY CLUSTERED ([{1}]);
{2}", ClientTableName, ClientPkColumnName, InsertDataSql); }
		}

		#endregion
	}
}
