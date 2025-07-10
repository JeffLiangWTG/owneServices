using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BulkAddDiscountBizO : NonPersistentBusinessObject
	{
		public BulkAddDiscountBizO(BusinessObject[] selectedOrgs)
			: base(new BusinessObjectFactory())
		{
			this.SelectedOrgs = Factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK, selectedOrgs.Select(x => x.PK)));
			this.DiscountsMap = new Dictionary<BulkAddDiscount, List<ClientLicenceBillingDiscount>>();
		}

		readonly EDIOrgHeader[] SelectedOrgs;
		readonly Dictionary<BulkAddDiscount, List<ClientLicenceBillingDiscount>> DiscountsMap;

		public BulkAddDiscountCollection NewDiscountCollection
		{
			get
			{
				if (newDiscountCollection == null)
				{
					newDiscountCollection = new BulkAddDiscountCollection();
					this.RegisterEditableChildObject(newDiscountCollection);
				}
				return newDiscountCollection;
			}
		}
		BulkAddDiscountCollection newDiscountCollection;

		#region Validate and Save

		public bool ValidateAndSave()
		{
			bool result = true;

			DeleteBillingDiscountsIfNewDiscountIsRemoved();
			foreach (BulkAddDiscount newDiscount in NewDiscountCollection)
			{
				CreateBillingDiscountsIfNotExist(newDiscount);
				result &= PopulateAndValidate(newDiscount);
			}

			if (result)
			{
				Factory.Save();
			}

			return result;
		}

		void DeleteBillingDiscountsIfNewDiscountIsRemoved()
		{
			foreach (var newDiscount in DiscountsMap.Keys.ToArray())
			{
				if (!NewDiscountCollection.Contains(newDiscount))
				{
					foreach (var billingDiscount in DiscountsMap[newDiscount])
					{
						billingDiscount.Delete();
					}
					DiscountsMap.Remove(newDiscount);
				}
			}
		}

		void CreateBillingDiscountsIfNotExist(BulkAddDiscount newDiscount)
		{
			if (!DiscountsMap.ContainsKey(newDiscount))
			{
				var billingDiscountList = new List<ClientLicenceBillingDiscount>(SelectedOrgs.Length);
				foreach (var org in SelectedOrgs)
				{
					if (org.LicCompany != null)
					{
						var billing = org.LicCompany.SelfBilling;
						var billingDiscount = billing.BillingDiscounts.AddNew();
						billingDiscountList.Add(billingDiscount);
					}
				}
				DiscountsMap.Add(newDiscount, billingDiscountList);
			}
		}

		bool PopulateAndValidate(BulkAddDiscount newDiscount)
		{
			bool result = true;

			foreach (var billingDiscount in DiscountsMap[newDiscount])
			{
				newDiscount.PopulateClientLicenceBillingDiscount(billingDiscount);
				billingDiscount.Validation.ValidateAll();
				result &= !billingDiscount.HasErrors;
			}

			var validation = new BulkAddDiscountValidation(newDiscount, DiscountsMap[newDiscount].ToArray());
			validation.ValidateAll();

			return result;
		}

		#endregion

	}
}

