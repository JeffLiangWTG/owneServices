//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEUEntryHeaderValidation
//
//    This class should be used for overriding validation in AutoCusEUEntryHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEUEntryHeaderValidation : AutoCusEUEntryHeaderValidation
	{
		public CusEUEntryHeaderValidation(AutoCusEUEntryHeader parent) : base(parent)
		{
		}

		protected override void CheckEUH_CH()
		{
			base.CheckEUH_CH();

			var parentID = Parent.EUH_CH;

			if (parentID.IsValid && !Parent.IsInDatabase)
			{
				var query = new ZQuery(CusEUEntryHeaderSchema.EUH_CH, parentID);
				query.AddToFilter(CusEUEntryHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.Factory.ExistsInDatabase(CusEUEntryHeaderSchema.Constants.TableName, query))
				{
					Parent.EUH_CHInfo.AddError(Res.GetString("D8C71FEF-0A59-48C4-AAB9-E3B9F368A266", "There is another record linked to the same Entry Header."));
				}
			}
		}
	}
}

