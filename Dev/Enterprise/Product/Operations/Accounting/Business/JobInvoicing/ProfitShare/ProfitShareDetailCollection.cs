using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareDetailCollection : NonPersistentBusinessObjectCollection<ProfitShareDetail>
	{
		public ProfitShareDetailCollection()
			: base(new BusinessObjectFactory())
		{
		}

		#region Retrieval

		public ProfitShareDetail GetProfitShareForOrg(OrgHeader org, string partyType = "")
		{
			ProfitShareDetail result = null;

			foreach (ProfitShareDetail detail in this)
			{
				if (detail.ProfitShareParty.PK == org.PK && string.IsNullOrEmpty(partyType))
				{
					partyType = detail.PartyType;
				}
				if (detail.ProfitShareParty.PK != org.PK || partyType != detail.PartyType)
				{
					continue;
				}
				result = detail;
				break;
			}
			return result;
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProfitShareDetail(null, Factory, null);
		}

		#endregion
	}
}
