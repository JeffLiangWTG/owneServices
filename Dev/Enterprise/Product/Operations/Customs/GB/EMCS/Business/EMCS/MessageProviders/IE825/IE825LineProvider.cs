using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE825LineProvider : LineProvider, IIE825Line
	{
		public IE825LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine) { }

		public string BodyRecordUniqueReference => emcsInvoiceLine.JI_LineNo.ToString();

		public string CnCode => emcsInvoiceLine.JI_Tariff;

		public decimal Quantity => emcsInvoiceLine.JI_CustomsQuantity.Normalize();

		public decimal GrossWeight => emcsInvoiceLine.JI_Weight.Normalize();

		public decimal NetWeight => emcsInvoiceLine.NetWeightInKG.Normalize();

		public ITextAndLanguage FiscalMark => fiscalMark ?? (fiscalMark = new TextAndLanguageProvider(emcsInvoiceLine.ZG_FiscalMark));
		ITextAndLanguage fiscalMark;

		public bool FiscalMarkUsedFlag => emcsInvoiceLine.ZG_FiscalMarkUsed;

		public decimal Density => emcsInvoiceLine.ZG_Density.Normalize();

		public bool DensitySpecified => Density != 0;

		public ITextAndLanguage CommercialDescription => commercialDescription ?? (commercialDescription = new TextAndLanguageProvider(emcsInvoiceLine.JI_NDescription));
		ITextAndLanguage commercialDescription;

		public ITextAndLanguage BrandNameOfProducts => brandNameOfProducts ?? (brandNameOfProducts = new TextAndLanguageProvider(emcsInvoiceLine.JI_BrandName));
		ITextAndLanguage brandNameOfProducts;

		public IReadOnlyCollection<IEMCSPackage> Packages => packages ?? (packages = emcsInvoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Where(x => x.IsForInvoiceLine).Select(x => new PackageProvider(emcsInvoiceLine, x.Package)).ToArray());
		IReadOnlyCollection<IEMCSPackage> packages;
	}
}
