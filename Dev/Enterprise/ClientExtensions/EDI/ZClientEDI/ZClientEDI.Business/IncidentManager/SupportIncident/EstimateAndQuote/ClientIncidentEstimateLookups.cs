//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientIncidentEstimateLookups
//
//    This class should be used for overriding collections in AutoClientIncidentEstimateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientIncidentEstimateLookups : AutoClientIncidentEstimateLookups
	{
		public ClientIncidentEstimateLookups(AutoClientIncidentEstimate parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PaymentTerms
		{
			get
			{
				return Factory.GetCachedValue(
				"EstimatePaymentTerms",
				() =>
				{
					var result = new CodeDescriptionPairList();
					foreach (ICodeDescriptionBool parentProduct in EDIDataRegistry.Instance.PaymentTypesAndPaymentTerms.Value)
					{
						result.AddRange(EDIDataRegistry.Instance.PaymentTypesAndPaymentTerms.Value.GetChildList(parentProduct.Code));
					}
					return result;
				});
			}
		}
	}
}

