#if DEBUG

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public static class ConsolPostingChargeDistributor_TestHelper
	{
		public static void CheckCreatedBasicKey(PostingChargeKey key)
		{
			var allowedFields = new List<string>()
			{
				"Org",
				"OrgAddress",
				"OrgContact",
				"InvoiceType",
				"JobNumber",
				"TaxRatePostingGroupId",
				"Branch",
				"PlaceOfSupply",
				"TaxBranch",
				"SellReference",
				"SellCurrency",
			};

			var fields = typeof(PostingChargeKey).GetFields();

			foreach (var field in fields)
			{
				var value = field.GetValue(key).ToString();
				bool showError = false;

				if (field.Name == "IsCommentChargeKey" && value == "False")
				{
					showError = false;
				}
				else if (!allowedFields.Contains(field.Name))
				{
					if (!string.IsNullOrEmpty(value) && value != "0" && value != ZGuid.Empty.ToString())
					{
						showError = true;
					}
				}

				if (showError)
				{
					throw new DeveloperNotificationException($@"Please add post manager validation to avoid posting transaction with different invoice currencies grouping by new group filter '{field.Name}', which will lead to incorrect transaction amount due to wrong currency exchange rate.
You can
· Update the allowedFields if you didn't update the PostingChargeKey in ConsolPostingChargeDistributor.CreateBasicKey.
· Or refer to class ConsolInvoicingPostManager -> method ProcessEligibleCharges for example about how to write such post manager validation.");
				}
			}
		}
	}
}

#endif
