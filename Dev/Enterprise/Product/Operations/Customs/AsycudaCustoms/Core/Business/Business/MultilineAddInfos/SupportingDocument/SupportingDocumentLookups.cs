using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SupportingDocumentLookups : CusSupportingInfoLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent) : base(parent) { }

		new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public ICodeDescriptionPairList SupportingDocumentCodeList
		{
			get
			{
				var parent = Parent.Parent;
				var invoice = parent as JobComInvoiceHeader ?? (parent as JobComInvoiceLine)?.InvoiceHeader;
				var dataGroupingCode = invoice?.GetDefaultDataGroupingCode() ?? ZString.Empty;
				return Factory.GetCachedValue(string.Join("_", "AsycudaCustoms.SupportingDocument.CodeList", dataGroupingCode, ZDateTime.Today),
					() =>
					{
						var result = new CodeDescriptionPairList();
						var list = ZZRefCusCodeListCombined.Loader.Load(
							Factory,
							dataGroupingCode,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UserDefinedSupportingDocuments,
							ZDateTime.Today
						);
						result.AddRange(list);
						return result;
					});
			}
		}
	}
}
