using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IObjectChangeSet
	{
		/// <summary>
		/// Gets a value indicating whether BO is exists in database.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if BO is exists in database; otherwise, <c>false</c>.
		/// </value>
		bool IsExistsInDatabase { get; }

		/// <summary>
		/// Gets a value indicating whether BO is modified in database.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if BO is modified in database; otherwise, <c>false</c>.
		/// </value>
		/// <exception cref="ObjectNotFoundInDatabaseException">
		///  If corresponding database row do not exists (was deleted) in database
		/// </exception>
		bool IsModifiedInDatabase { get; }

		/// <summary>
		/// Gets the display name of object type.
		/// </summary>
		/// <value>The display name.</value>
		string DisplayName { get; }

		/// <summary>
		/// Gets the session instance of BO.
		/// </summary>
		/// <value>The session instance.</value>
		BusinessObject SessionInstance { get; }

		/// <summary>
		/// Gets the database instance of BO.
		/// </summary>
		/// <value>The database instance.</value>
		/// <remarks>
		/// Will return <c>null</c> if corresponding database row do not exists (was deleted) in database
		/// </remarks>
		BusinessObject DatabaseInstance { get; }

		/// <summary>
		/// Gets the list of modified not-mergeable properties (database changes).
		/// </summary>
		/// <value>The critical properties.</value>
		/// <exception cref="ObjectNotFoundInDatabaseException">
		///  If corresponding database row do not exists (was deleted) in database
		/// </exception>
		List<IPropertyRecord> NonMergeableProperties { get; }

		/// <summary>
		/// Gets the list of modified mergeable properties (database changes).
		/// </summary>
		/// <value>The modified properties.</value>
		/// <exception cref="ObjectNotFoundInDatabaseException">
		///  If corresponding database row do not exists (was deleted) in database
		/// </exception>
		List<IPropertyRecord> MergeableProperties { get; }

		/// <summary>
		/// Gets the last modification inofrmation (user and time).
		/// </summary>
		/// <value>The last modified text.</value>
		string LastModified { get; }

		/// <summary>
		/// Merge values from database to BO.
		/// </summary>
		/// <exception cref="ObjectNotFoundInDatabaseException">
		///  If corresponding database row do not exists (was deleted) in database
		/// </exception>
		void Merge();

		/// <summary>
		/// Determines whether it is possible to merge or not.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if BO can be merged; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ObjectNotFoundInDatabaseException">
		///  If corresponding database row do not exists (was deleted) in database
		/// </exception>
		bool CanMerge();

		/// <summary>
		/// Deletes session instance of BO from factory.
		/// </summary>
		/// <remarks>
		/// BO will be detached from factory and thus ignored on save.
		/// </remarks>
		void Delete();
	}
}
