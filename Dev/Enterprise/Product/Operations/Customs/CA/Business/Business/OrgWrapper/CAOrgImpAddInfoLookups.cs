//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCAOrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoCAOrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	using Enterprise.MasterFiles.Business.Customs;
	using Enterprise.ZArchitecture.Core;

	public class CAOrgImpAddInfoLookups : AutoCAOrgImpAddInfoLookups
	{
		public CAOrgImpAddInfoLookups(AutoCAOrgImpAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CFIAPaymentMethods
		{
			get { return Factory.GetCachedValue<CFIAPaymentMethods>(); }
		}

		public CodeDescriptionPairList LVSInvoiceDetailCodes
		{
			get { return Factory.GetCachedValue<LVSInvoiceDetailCodes>(); }
		}

		public CodeDescriptionPairList DelayIntervalTypeCodesListForHVS
		{
			get
			{
				return Factory.GetCachedValue("DelayIntervalTypeCodesListForHVS", () =>
					{
						var result = new DelayIntervalTypeCodes();
						result.RemoveCode(DelayIntervalTypeCodes.Codes.DAS);
						result.RemoveCode(DelayIntervalTypeCodes.Codes.DAY);
						return result;
					});
			}
		}

		public CodeDescriptionPairList DelayIntervalTypeCodesListForCON
		{
			get
			{
				return Factory.GetCachedValue("DelayIntervalTypeCodesListForCON", () =>
				{
					var result = new DelayIntervalTypeCodes();
					result.RemoveCode(DelayIntervalTypeCodes.Codes.DAR);
					result.RemoveCode(DelayIntervalTypeCodes.Codes.DAS);
					return result;
				});
			}
		}

		public CodeDescriptionPairList UltimateConsigneeReferenceQualifierList
		{
			get { return Factory.GetCachedValue<UltimateConsigneeReferenceQualifierList>(); }
		}

		public CodeDescriptionPairList VendorReferenceQualifierList
		{
			get { return Factory.GetCachedValue<VendorReferenceQualifierList>(); }
		}

		public CodeDescriptionPairList DeferedB3SendActionList
		{
			get { return Factory.GetCachedValue<DeferredB3SendActionListOverride>(); }
		}

		public CodeDescriptionPairList ProductAuditActions
		{
			get { return Factory.GetCachedValue<ProductAuditActions>(); }
		}

		public CodeDescriptionPairList AccountingOptions
		{
			get { return Factory.GetCachedValue<CSARSFAccountingOptionList>(); }
		}
	}
}
