using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoJobComInvoiceHeaderLookups : EUAddInfoLookups
	{
		public AddInfoJobComInvoiceHeaderLookups(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList TransportChargesMethodOfPaymentList
		{
			get
			{
				var dataGroupingCode = InvoiceHeader.GetDefaultDataGroupingCode();
				return Factory.GetCachedValue("AddInfoJobComInvoiceHeaderLookups.TransportChargesMethodOfPaymentList." + dataGroupingCode, () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory,
						dataGroupingCode,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment,
						ZDateTime.Now));
					result.Sort();
					return result;
				});
			}
		}

		public ICollection AgreedPlaceCodeList => InvoiceHeader.AgreedPlaceCodeSupport
			? Parent.ZG_AgreedPlaceCode.Length == 2
				? new RefCountryCollection(Factory)
				: new RefUNLOCOCollection(Factory)
			: Factory.GetAgreedPlaceCodeList(InvoiceHeader.GetDefaultDataGroupingCode());

		new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

		JobComInvoiceHeader InvoiceHeader => Parent.Parent;
	}
}
