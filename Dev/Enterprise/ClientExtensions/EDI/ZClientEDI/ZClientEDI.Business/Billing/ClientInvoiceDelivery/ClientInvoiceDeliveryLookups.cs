//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientInvoiceDeliveryLookups
//
//    This class should be used for overriding collections in AutoClientInvoiceDeliveryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientInvoiceDeliveryLookups : AutoClientInvoiceDeliveryLookups
	{
		public ClientInvoiceDeliveryLookups(AutoClientInvoiceDelivery parent) : base(parent)
		{
		}

		protected new ClientInvoiceDelivery Parent
		{
			get { return (ClientInvoiceDelivery)base.Parent; }
		}

		public CodeDescriptionPairList SystemCodes
		{
			get
			{
				var result = BillingConstants.GetAllBillingSystems();
				result.AddPair(BillingConstants.PriceHeaderType.LDaaS, "Logistics Devices as a Service");
				result.AddPairsIfNotExist(EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.GetPriceListCodeDescriptionPairList().OfType<ICodeDescription>());
				return result;
			}
		}

		public CodeDescriptionPairList ServerCodes
		{
			get { return Parent.GetServerCodes(); }
		}

		public override AccTaxRateCollection TaxIds
		{
			get
			{
				GlbBranch branch = Parent.InvoicingBranch;
				ZString countryCode = branch != null
					? branch.Company.GC_RN_NKCountryCode
					: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return new AccTaxRateCollection(Factory, countryCode);
			}
		}

		public override AccChargeCodeCollection SalesTaxChargeCodes
		{
			get
			{
				GlbBranch branch = Parent.InvoicingBranch;
				return new AccChargeCodeCollection(Factory, new ZQuery(), (branch != null ? branch.GB_GC : GlbCompany.CurrentCompany.PK).ToGuid());
			}
		}

		public CodeDescriptionPairList GroupByCodes
		{
			get { return GetGroupByCodes(); }
		}

		public static CodeDescriptionPairList CreateGroupByCodes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(ClientInvoiceDelivery.GroupBy.All, "Always group");
			result.AddPair(ClientInvoiceDelivery.GroupBy.Org, "By Organization");
			result.AddPair(ClientInvoiceDelivery.GroupBy.Ent, "By Enterprise");
			result.AddPair(ClientInvoiceDelivery.GroupBy.Lic, "By Licence");
			result.AddPair(ClientInvoiceDelivery.GroupBy.Db, "By Database");
			return result;
		}

		public static CodeDescriptionPairList GetGroupByCodes()
		{
			return groupByCodes ?? (groupByCodes = CreateGroupByCodes());
		}

		[ThreadStatic]
		static CodeDescriptionPairList groupByCodes;
	}
}

