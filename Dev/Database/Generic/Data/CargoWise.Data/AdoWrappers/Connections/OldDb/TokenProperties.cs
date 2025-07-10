using CargoWise.DataLink;

namespace CargoWise.Data
{
	//
	// This is part of the old Db class, various static getters that have no other deps.
	//
	// In the future, many of these constants need to be re-considered and grouped into their relevant locations.
	//
	// There are no dependencies to other parts of Db.
	//

	public partial class Db
	{
		public static string SaValue
		{
			get
			{
				using (var token = DataLinkManager.Instance.NewToken())
				{
					return token.GetProperty(token[DataLinkEnum.SAV]);
				}
			}
		}
	}
}
