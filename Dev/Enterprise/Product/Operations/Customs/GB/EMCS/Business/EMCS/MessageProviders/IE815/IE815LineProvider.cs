using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE815LineProvider : LineProvider, IIE815Line
	{
		public IE815LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine) { }

		public string Tariff => emcsInvoiceLine.JI_Tariff;

		public decimal Quantity => emcsInvoiceLine.JI_CustomsQuantity.Normalize();

		public bool FiscalMarkUsedFlag => emcsInvoiceLine.ZG_FiscalMarkUsed;

		public ITextAndLanguage FiscalMark => fiscalMark ?? (fiscalMark = new TextAndLanguageProvider(emcsInvoiceLine.ZG_FiscalMark));
		ITextAndLanguage fiscalMark;

		public ITextAndLanguage DesignationOfOrigin => designationOfOrigin ?? (designationOfOrigin = new TextAndLanguageProvider(emcsInvoiceLine.ZG_Origin));
		ITextAndLanguage designationOfOrigin;

		public ITextAndLanguage CommercialDescription => commercialDescription ?? (commercialDescription = new TextAndLanguageProvider(emcsInvoiceLine.JI_NDescription));
		ITextAndLanguage commercialDescription;

		public ITextAndLanguage BrandNameOfProducts => brandNameOfProducts ?? (brandNameOfProducts = new TextAndLanguageProvider(emcsInvoiceLine.JI_BrandName));
		ITextAndLanguage brandNameOfProducts;

		public decimal GrossWeight => emcsInvoiceLine.JI_Weight.Normalize();

		public decimal NetWeight => emcsInvoiceLine.NetWeightInKG.Normalize();

		public decimal AlcoholicStrength => emcsInvoiceLine.ZG_AlcoholicStrength.Normalize();

		public decimal DegreePlato => emcsInvoiceLine.ZG_DegreePlato.Normalize();

		public decimal SizeOfProducer => emcsInvoiceLine.ZG_SizeOfProducer;

		public decimal Density => emcsInvoiceLine.ZG_Density.Normalize();

		public IReadOnlyCollection<IEMCSPackage> Packages => packages ?? (packages = emcsInvoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Where(x => x.IsForInvoiceLine).Select(x => new PackageProvider(emcsInvoiceLine, x.Package)).ToArray());
		IReadOnlyCollection<IEMCSPackage> packages;

		public IWineProduct WineProduct => CachedValueHelper.GetValue(ref wineProduct, () => (emcsInvoiceLine.IsWine && !emcsInvoiceLine.ZG_WineCategory.IsEmpty) ? new WineProductProvider(emcsInvoiceLine) : null);

		public ITextAndLanguage MaturationPeriodOrAgeOfProducts => maturationPeriodOrAgeOfProducts ?? (maturationPeriodOrAgeOfProducts = new TextAndLanguageProvider(emcsInvoiceLine.ZG_MaturationPeriodOrAgeOfProducts));
		ITextAndLanguage maturationPeriodOrAgeOfProducts;

		public ITextAndLanguage IndependentSmallProducersDeclaration => independentSmallProducersDeclaration ??= new TextAndLanguageProvider(emcsInvoiceLine.ZG_IndependentSmallProducersDeclaration);
		ITextAndLanguage independentSmallProducersDeclaration;

		CachedValue<IWineProduct> wineProduct;
	}
}
