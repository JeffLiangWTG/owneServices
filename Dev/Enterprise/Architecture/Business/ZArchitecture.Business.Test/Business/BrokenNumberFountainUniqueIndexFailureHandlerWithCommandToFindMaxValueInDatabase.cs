using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BrokenNumberFountainUniqueIndexFailureHandlerWithCommandToFindMaxValueInDatabase : BrokenNumberFountainUniqueIndexFailureHandler
	{
		public BrokenNumberFountainUniqueIndexFailureHandlerWithCommandToFindMaxValueInDatabase(string uniqueIndex, BusinessObject bizObjCausingError)
			: base(uniqueIndex, bizObjCausingError)
		{
		}

		protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
		{
			return Db.Connection.Command("");
		}
	}
}
