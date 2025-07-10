using CargoWise.Common;
using CargoWise.Common.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	/// <summary>
	/// Retrieves the Controlling Customer for Profit Share purposes from a Shipment.
	/// Calculates this by looking for a DocAddress on the Host, if none found, then by looking at the "Controlling Customer"
	/// on the Freight Charge Debtor.
	/// Once located, it will be added to the DocAddress collection of the host for future reference.
	/// </summary>
#if DEBUG
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public static class ControllingCustomerRetriever
	{
		public static OrgHeader GetControllingCustomer(IJobInvoicingSupporter jobInvoicingSupporter)
		{
			Argument.NotNull(jobInvoicingSupporter, nameof(jobInvoicingSupporter));

			var result = jobInvoicingSupporter.ControllingCustomer;
			if (result == null)
			{
				var job = jobInvoicingSupporter.Job as Job;
				if (job != null)
				{
					if (job.LocalCharges != null)
					{
						result = GetControllingCustomerFromOrg(job.LocalCharges, jobInvoicingSupporter);
					}
					else
					{
						var freightChargeCode = job.Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
						Charge freightCharge = job.Charges.ContainsChargeCode(freightChargeCode);
						if (freightCharge != null && freightCharge.SellAccount != null)
						{
							result = GetControllingCustomerFromOrg(freightCharge.SellAccount, jobInvoicingSupporter);
						}
					}
				}
			}

			return result;
		}

		#region Implementation

		static OrgHeader GetControllingCustomerFromOrg(OrgHeader org, IJobInvoicingSupporter jobInvoicingSupporter)
		{
			OrgRelatedParty party = null;

			if (jobInvoicingSupporter.IsExport)
			{
				party = org.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Pickup);
			}
			else if (jobInvoicingSupporter.IsImport)
			{
				party = org.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Delivery);
			}

			if (party == null)
			{
				party = org.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.PickupAndDelivery);
			}

			return party != null ? party.RelatedParty : null;
		}

		#endregion
	}
}
