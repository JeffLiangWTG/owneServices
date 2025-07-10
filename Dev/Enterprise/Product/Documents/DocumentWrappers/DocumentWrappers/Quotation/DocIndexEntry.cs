using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocIndexEntry : DocBaseWrapper
	{
		DocIndexEntry(PricingPage page, BusinessObjectFactory factoryToWrap)
			: base(page, factoryToWrap) { }

		public static DocIndexEntry New(PricingPage page, BusinessObjectFactory factoryToWrap)
		{
			return page == null ? null : new DocIndexEntry(page, factoryToWrap);
		}

		public override string ToString()
		{
			return "";
		}

		public ZString Header
		{
			get
			{
				switch (Page.Style)
				{
					case PricingPageStyle.Standard:
						return Res.GetString("4cc60098-65fb-4989-8eb7-5c61d593c433", "{0} Quotations", FirstEntry.FreightType);
					case PricingPageStyle.Landscape:
						return DocTableQuotation.GetHeader(FirstEntry);
					default:
						return ZString.Empty;
				}
			}
		}

		RateEntry FirstEntry
		{
			get
			{
				if (pageFirstEntry == null)
				{
					using (var e = Page.RateEntries.GetEnumerator())
					{
						if (e.MoveNext())
						{
							pageFirstEntry = e.Current;
						}
					}
				}

				return pageFirstEntry;
			}
		}
		RateEntry pageFirstEntry;

		public ZString Description
		{
			get
			{
				switch (Page.Style)
				{
					case PricingPageStyle.Standard:
						return FirstEntry.QuotationIndexDescription;

					case PricingPageStyle.Landscape:
						ILocation localPort = FirstEntry.LocalPort;
						return localPort == null ? ZString.Empty : localPort.Description;

					default: return ZString.Empty;
				}
			}
		}

		#region Implementation

		PricingPage Page
		{
			get { return WrappedObject as PricingPage; }
		}

		#endregion
	}
}
