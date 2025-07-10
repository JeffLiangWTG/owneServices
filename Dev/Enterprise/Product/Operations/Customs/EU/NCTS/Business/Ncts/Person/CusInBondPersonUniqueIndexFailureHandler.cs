using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondPersonUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public CusInBondPersonUniqueIndexFailureHandler(CusInBondPerson cusInBondPerson)
		{
			this.cusInBondPerson = Argument.NotNull(cusInBondPerson, nameof(cusInBondPerson));
		}
		readonly CusInBondPerson cusInBondPerson;

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return CusInBondPersonSchema.Constants.Indexes.NR_UX__CP_BH_Header_CP_Type;
			}
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var existingItem = GetExistingItem();
			if (existingItem != null)
			{
				notifier.ReportInformation(GenerateMessage(existingItem), Res.GetString("2FE64CAF-E3E6-4812-B833-8CFA26B46B4A", "Save Error"));
				if (cusInBondPerson.HasChanges)
				{
					existingItem.CP_FullName = cusInBondPerson.CP_FullName;
					existingItem.CP_Phone = cusInBondPerson.CP_Phone;
					existingItem.CP_Email = cusInBondPerson.CP_Email;
				}
				cusInBondPerson.Delete();
			}
		}

		string GenerateMessage(CusInBondPerson existingItem)
		{
			var lastEditUserAndTime = existingItem.CP_SystemLastEditUser + " @ " + existingItem.CP_SystemLastEditTimeUtc;
			return Res.GetString("93435569-8341-42DE-9F91-9275EDB9B79E",
				"While you were working, Goods Location at Departure Contact Info has already been entered on '{0}' by another user ({1}). Your changes have been merged, please review your changes and save again.",
				existingItem.NctsHeader.HumanReadableName,
				lastEditUserAndTime);
		}

		CusInBondPerson GetExistingItem()
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondPerson));
			query.AddToFilter(CusInBondPersonSchema.PK, SQLComparisonOperator.NotEqual, cusInBondPerson.PK);
			query.AddToFilter(CusInBondPersonSchema.CP_BH_Header, cusInBondPerson.CP_BH_Header);
			query.AddToFilter(CusInBondPersonSchema.CP_Type, cusInBondPerson.CP_Type);

			return cusInBondPerson.Factory.LoadTop1<CusInBondPerson>(query);
		}
	}
}
