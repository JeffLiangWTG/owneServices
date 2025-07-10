using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class OutwardProcessingProvider : IOutwardProcessing
	{
		public static OutwardProcessingProvider NewOrNull(CusEntryInstruction entryInstruction) => entryInstruction == null ? null : new OutwardProcessingProvider(entryInstruction);

		OutwardProcessingProvider(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = entryInstruction;
		}
		readonly CusEntryInstruction entryInstruction;

		public IReadOnlyCollection<string> ReimportCountries
		{
			get
			{
				if (reimportCountries == null)
				{
					reimportCountries = entryInstruction.ReimportCountryCodes.Cast<ReimportCountryCode>().Select(x => (string)x.CY_Code).Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).ToArray();
				}
				return reimportCountries;
			}
		}
		IReadOnlyCollection<string> reimportCountries;

		public IReadOnlyCollection<IIdentificationMeans> IdentificationMeans
		{
			get
			{
				if (identificationMeans == null)
				{
					identificationMeans = entryInstruction.IdentificationMeanCodes.Cast<IdentificationMeansCode>().OrderBy(x => x.CY_Order).ThenBy(x => x.PK).Select(IdentificationMeanProvider.NewOrNull).ToArray<IIdentificationMeans>();
				}
				return identificationMeans;
			}
		}
		IReadOnlyCollection<IIdentificationMeans> identificationMeans;

		public IReadOnlyCollection<IProduct> Products
		{
			get
			{
				if (products == null)
				{
					products = entryInstruction.Products.Cast<ProductSupportingInfo>().OrderBy(x => x.CSI_LineNo).ThenBy(x => x.PK).Select(ProductProvider.NewOrNull).ToArray<IProduct>();
				}
				return products;
			}
		}
		IReadOnlyCollection<IProduct> products;
	}
}
