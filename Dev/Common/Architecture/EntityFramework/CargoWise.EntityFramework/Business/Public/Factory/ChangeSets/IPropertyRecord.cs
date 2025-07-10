namespace CargoWise.EntityFramework
{
	public interface IPropertyRecord
	{
		/// <summary>
		/// Gets the corresponding property info.
		/// </summary>
		/// <value>The property info.</value>
		ZPropertyInfo PropertyInfo { get; }

		/// <summary>
		/// Gets the corresponding data column name.
		/// </summary>
		/// <value>The property info.</value>
		string ColumnName { get; }

		/// <summary>
		/// Gets the display name of this property.
		/// </summary>
		/// <value>The display name.</value>
		string DisplayName { get; }

		/// <summary>
		/// Gets the original (session) value.
		/// </summary>
		/// <value>The original value.</value>
		object OriginalValue { get; }

		/// <summary>
		/// Gets the current (session) value.
		/// </summary>
		/// <value>The current value.</value>
		object CurrentValue { get; }

		/// <summary>
		/// Gets the database value.
		/// </summary>
		/// <value>The database value.</value>
		object DatabaseValue { get; }

		/// <summary>
		/// Gets the use who last saved the change in the database.
		/// </summary>
		/// <value>The last modified user.</value>
		string LastModified { get; }

		/// <summary>
		/// Gets a value indicating whether this instance has changed in database.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has changed in database; otherwise, <c>false</c>.
		/// </value>
		bool HasChangedInDatabase { get; }

		/// <summary>
		/// Answers whether this property's value has changed in session.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if has changed in session; otherwise, <c>false</c>.
		/// </value>
		bool HasChangedInSession { get; }

		/// <summary>
		/// Answers whether this property's value is consistent with one in database.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if consistent with database; otherwise, <c>false</c>.
		/// </value>
		bool IsConsistentWithDatabase { get; }

		/// <summary>
		/// Gets a value indicating whether merge is allowed for this record.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if merge is allowed; otherwise, <c>false</c>.
		/// </value>
		bool IsMergeAllowed { get; }

		/// <summary>
		/// Merges this record.
		/// </summary>
		void Merge();
	}
}
