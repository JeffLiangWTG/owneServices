using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageCFXWrapperCollection : GenericWrapperCollection<PricingPageCFXWrapper>
	{
		public PricingPageCFXWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public PricingPageCFXWrapperCollection(PricingPage pricingPage, BusinessObjectFactory factory)
			: base(factory)
		{
			if (pricingPage != null && Env.Registry.Rating.IncludeCFXOnQuote)
			{
				bool containsImportAir = false;
				bool containsExportAir = false;
				bool containsImportSea = false;
				bool containsExportSea = false;
				RatingHeader header = null;

				foreach (var entry in pricingPage.RateEntries)
				{
					if (header == null)
					{
						header = entry.Parent;
					}

					if (entry.IsImport())
					{
						containsImportAir |= entry.IsAir();
						containsImportSea |= entry.IsSea();
					}

					if (entry.IsExport())
					{
						containsExportAir |= entry.IsAir();
						containsExportSea |= entry.IsSea();
					}
				}

				if (header != null)
				{
					if (containsImportAir)
					{
						Add(new PricingPageCFXWrapper(factory, Res.GetString("baf6a841-632c-4e9c-952f-47ead0af6b79", "Import - Air"), header.TH_AirCFX));
					}

					if (containsImportSea)
					{
						Add(new PricingPageCFXWrapper(factory, Res.GetString("df9ef57c-b779-4b01-8dfb-441777a34246", "Import - Sea"), header.TH_SeaCFX));
					}

					if (containsExportAir)
					{
						Add(new PricingPageCFXWrapper(factory, Res.GetString("9d504793-8574-4f72-88c8-5e5d2f8f0f7b", "Export - Air"), header.TH_ExportAirCFX));
					}

					if (containsExportSea)
					{
						Add(new PricingPageCFXWrapper(factory, Res.GetString("14cc5512-c1c3-4ccc-91e8-3232224c843f", "Export - Sea"), header.TH_ExportSeaCFX));
					}
				}
			}
		}
	}
}
