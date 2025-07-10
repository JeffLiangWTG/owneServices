using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public static class UniversalExtensions
	{
		public static IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing(this BusinessObjectFactory factory)
		{
			IEnumerable<string> result = null;
			if (factory == null)
			{
				result = Enumerable.Empty<string>();
			}
			else
			{
				result = factory.GetCachedValue<IEnumerable<string>>("CAInvoiceLineAddInfosApplicableForInwardWarehousing", () =>
				{
					var invoiceLineSchemaDictionary = factory.GetAddInfoSchemaDictionary<JobComInvoiceLine>(CAAddInfoSchema.Instance);
					var list = new List<string>(invoiceLineSchemaDictionary.Keys.OrderBy(o => o));
					foreach (var keyToRemove in InvoiceLineAddInfoFieldsNotApplicableToInwardWarehousing.Select(x => x.Substring(3)))
					{
						list.Remove(keyToRemove);
					}
					return list;
				});
			}
			return result;
		}

		static string[] InvoiceLineAddInfoFieldsNotApplicableToInwardWarehousing
		{
			get
			{
				return new string[]
				{
					JobComInvoiceLine.Schema.CA_CustomsValue,
					JobComInvoiceLine.Schema.CA_CustomsValueOvr,
					JobComInvoiceLine.Schema.CA_CVforCurrConv,
					JobComInvoiceLine.Schema.CA_CVforCurrConvOvr,
					JobComInvoiceLine.Schema.CA_OriginalLineNo,
					JobComInvoiceLine.Schema.CA_OGDStatus,
					JobComInvoiceLine.Schema.CA_PreviousB3LineNo,
					JobComInvoiceLine.Schema.CA_PreviousB3SubHeaderNo,
					JobComInvoiceLine.Schema.CA_PreviousLineNo,
					JobComInvoiceLine.Schema.CA_SIMADumpingNum
				};
			}
		}
	}
}
