//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientIncidentQuoteLookups
//
//    This class should be used for overriding collections in AutoClientIncidentQuoteLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientIncidentQuoteLookups : AutoClientIncidentQuoteLookups
	{
		public ClientIncidentQuoteLookups(AutoClientIncidentQuote parent) : base(parent)
		{
		}

		public new ClientIncidentQuote Parent
		{
			get { return (ClientIncidentQuote)base.Parent; }
		}

		public CodeDescriptionPairList PaymentTypes
		{
			get
			{
				return Factory.GetCachedValue(
				"PaymentTypes",
				() =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(EDIDataRegistry.Instance.PaymentTypesAndPaymentTerms.Value);
					return result;
				});
			}
		}

		public CodeDescriptionPairList PaymentTerms
		{
			get { return GetPaymentTerms(Parent != null ? Parent.CIQ_Type : ZString.Empty); }
		}

		CodeDescriptionPairList GetPaymentTerms(ZString type)
		{
			return Factory.GetCachedValue(
				"QuotePaymentTerms:" + type,
				() =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(EDIDataRegistry.Instance.PaymentTypesAndPaymentTerms.Value.GetChildList(type));
					return result;
				});
		}
	}
}

