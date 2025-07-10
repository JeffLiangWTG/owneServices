using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class PackageProvider : IEMCSPackage
	{
		public PackageProvider(EMCSJobComInvoiceLine emcsInvoiceLine, EMCSPackage emcsPackage)
		{
			var invoiceLine = Argument.NotNull(emcsInvoiceLine, nameof(emcsInvoiceLine));
			this.emcsPackage = Argument.NotNull(emcsPackage, nameof(emcsPackage));
			helper = new PackageProviderHelper(invoiceLine, emcsPackage);
		}
		readonly EMCSPackage emcsPackage;
		readonly PackageProviderHelper helper;

		public string KindOfPackages => helper.KindOfPackages;

		public long? NumberOfPackages => helper.NumberOfPackages;

		public string SealNumber => helper.SealNumber;

		public ITextAndLanguage SealInformation => description ?? (description = new TextAndLanguageProvider(emcsPackage.B5_SealComment));
		ITextAndLanguage description;

		public string ShippingMarks => helper.ShippingMarks;
	}
}
