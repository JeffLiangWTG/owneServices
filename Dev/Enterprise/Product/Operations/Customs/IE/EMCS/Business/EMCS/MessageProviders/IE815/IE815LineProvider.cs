using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE815LineProvider : LineProvider, IIE815Line
	{
		public IE815LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine) : base(emcsInvoiceLine)
		{
			helper = new Message815LineProviderHelper(emcsInvoiceLine);
		}
		readonly Message815LineProviderHelper helper;

		public string Tariff => helper.Tariff;

		public decimal Quantity => helper.Quantity;

		public bool FiscalMarkUsedFlag => helper.FiscalMarkUsedFlag;

		public ITextAndLanguage FiscalMark => fiscalMark ?? (fiscalMark = new TextAndLanguageProvider(emcsInvoiceLine.ZG_FiscalMark));
		ITextAndLanguage fiscalMark;

		public ITextAndLanguage DesignationOfOrigin => designationOfOrigin ?? (designationOfOrigin = new TextAndLanguageProvider(emcsInvoiceLine.ZG_Origin));
		ITextAndLanguage designationOfOrigin;

		public ITextAndLanguage CommercialDescription => commercialDescription ?? (commercialDescription = new TextAndLanguageProvider(emcsInvoiceLine.JI_NDescription));
		ITextAndLanguage commercialDescription;

		public ITextAndLanguage BrandNameOfProducts => brandNameOfProducts ?? (brandNameOfProducts = new TextAndLanguageProvider(emcsInvoiceLine.JI_BrandName));
		ITextAndLanguage brandNameOfProducts;

		public decimal GrossWeight => helper.GrossWeight;

		public decimal NetWeight => helper.NetWeight;

		public decimal AlcoholicStrength => helper.AlcoholicStrength;

		public decimal DegreePlato => helper.DegreePlato;

		public decimal SizeOfProducer => helper.SizeOfProducer;

		public decimal Density => helper.Density;

		public IReadOnlyCollection<IEMCSPackage> Packages => packages ?? (packages = emcsInvoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Where(x => x.IsForInvoiceLine).Select(x => new PackageProvider(emcsInvoiceLine, (EMCSPackage)x.Package)).ToArray());
		IReadOnlyCollection<IEMCSPackage> packages;

		public IWineProduct WineProduct => CachedValueHelper.GetValue(ref wineProduct, () => (emcsInvoiceLine.IsWine && !emcsInvoiceLine.ZG_WineCategory.IsEmpty) ? new WineProductProvider(emcsInvoiceLine) : null);
		CachedValue<IWineProduct> wineProduct;

		public ITextAndLanguage MaturationPeriodOrAgeOfProducts => maturationPeriodOrAgeOfProducts ?? (maturationPeriodOrAgeOfProducts = new TextAndLanguageProvider(emcsInvoiceLine.ZG_MaturationPeriodOrAgeOfProducts));

		public ITextAndLanguage IndependentSmallProducersDeclaration => independentSmallProducersDeclaration ?? (independentSmallProducersDeclaration = new TextAndLanguageProvider(emcsInvoiceLine.ZG_IndependentSmallProducersDeclaration));

		ITextAndLanguage maturationPeriodOrAgeOfProducts;

		ITextAndLanguage independentSmallProducersDeclaration;
	}
}
