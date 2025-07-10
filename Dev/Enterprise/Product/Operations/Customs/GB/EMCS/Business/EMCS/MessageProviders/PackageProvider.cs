using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PackageProvider : IEMCSPackage
	{
		public PackageProvider(EMCSJobComInvoiceLine emcsInvoiceLine, EMCSPackage emcsPackage)
		{
			this.emcsInvoiceLine = Argument.NotNull(emcsInvoiceLine, nameof(emcsInvoiceLine));
			this.emcsPackage = Argument.NotNull(emcsPackage, nameof(emcsPackage));
		}
		readonly EMCSJobComInvoiceLine emcsInvoiceLine;
		readonly EMCSPackage emcsPackage;

		public string KindOfPackages => emcsPackage.B5_UnitType;

		public long? NumberOfPackages => emcsInvoiceLine.ZG_IsMainPack ? (emcsPackage.IsCountable() ? emcsPackage.B5_UnitCount : null) : 0;

		public string SealNumber => emcsPackage.B5_SealNumber;

		public ITextAndLanguage SealInformation => description ?? (description = new TextAndLanguageProvider(emcsPackage.B5_SealComment));
		ITextAndLanguage description;

		public string ShippingMarks => emcsPackage.IsCountable() ? emcsPackage.B5_MarksAndNumbers : null;
	}
}
