using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// This interface is used to control linking to objects where there is no enforced foreign key.
	/// The key is made up of the Guid and the tablename.
	/// This interface is implemented on BusinessObject to return PK and TableName
	/// </summary>
	public interface ILinkable
	{
		/// <summary>
		/// The PK of the object for linking
		/// </summary>
		ZGuid LinkPK { get; }

		/// <summary>
		/// The tablename of the object for linking
		/// </summary>
		string LinkTableName { get; }

		/// <summary>
		/// The tableprefix of the object for linking
		/// </summary>
		string LinkTablePrefix { get; }

		/// <summary>
		///  Is the object for linking in the database?
		/// </summary>
		bool LinkIsInDatabase { get; }
	}
}
