using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// Class for data to be stored in XP_ApprovalRequestData for a CreditControlledDocumentsApproval. Contains both
	/// request and response data.
	/// </summary>
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CreditControlledDocumentsApprovalData
	{
		// Request Data
		public ZGuid MenuItemPK { get; set; }
		public OrganizationsForCreditCheckWithAmountsCollection OrganizationsForCreditCheckWithAmountsCollection { get; set; }

		// Response Data
		public bool ApproveAllDocuments { get; set; }
		public ZString RejectionReason { get; set; }
	}

	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class OrganizationsForCreditCheckWithAmounts
	{
		public ZGuid OrgPK { get; set; }
		public ZDecimal PostedAmount { get; set; }
		public ZDecimal UnpostedAmount { get; set; }
	}

	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class OrganizationsForCreditCheckWithAmountsCollection : List<OrganizationsForCreditCheckWithAmounts>
	{
		public bool IsSubsetOf(OrganizationsForCreditCheckWithAmountsCollection matchingCollection, bool onlyIfAmountIncreases)
		{
			if (matchingCollection == null || this.Count == 0)
			{
				return false;
			}

			var result = true;
			foreach (var org in this)
			{
				var orgWithAmountsInMatchingCollection = matchingCollection.FirstOrDefault(x => x.OrgPK == org.OrgPK);
				if (orgWithAmountsInMatchingCollection == null)
				{
					return false;
				}

				if (onlyIfAmountIncreases)
				{
					result &= orgWithAmountsInMatchingCollection.UnpostedAmount + orgWithAmountsInMatchingCollection.PostedAmount >= org.UnpostedAmount + org.PostedAmount;
				}
				else
				{
					result &= orgWithAmountsInMatchingCollection.UnpostedAmount + orgWithAmountsInMatchingCollection.PostedAmount == org.UnpostedAmount + org.PostedAmount;
				}
			}

			return result;
		}
	}
}