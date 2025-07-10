using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public interface IBox16CountryOfOriginEvaluator
	{
		ZString Evaluate();
	}

	public class SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator : IBox16CountryOfOriginEvaluator
	{
		public SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Argument.NotNull(entryHeader.MergedLines, nameof(entryHeader.MergedLines));
		}

		public ZString Evaluate()
		{
			var box16CountryOfOriginCore = ZString.Empty;
			var distinctedCountriesOfOrigin = entryHeader
						.MergedLines.Cast<Enterprise.Customs.Business.CusEntryLine>()
						.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>())
						.Select(x => x.JI_CountryOfOrigin)
						.Distinct();

			if (!distinctedCountriesOfOrigin.Skip(1).Any())
			{
				var countryCode = distinctedCountriesOfOrigin.SingleOrDefault();
				var refCountry = !countryCode.IsEmpty ? RefCountry.LoadFromCountryCode(entryHeader.Factory, countryCode) : null;

				box16CountryOfOriginCore = refCountry?.RN_Desc ?? ZString.Empty;
			}

			return box16CountryOfOriginCore;
		}

		readonly CusEntryHeader entryHeader;
	}
}
