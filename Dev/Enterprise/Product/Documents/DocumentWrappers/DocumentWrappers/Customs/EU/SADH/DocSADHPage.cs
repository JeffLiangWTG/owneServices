using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using EUCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	[AllowNoStaticNew]
	public class DocSADHPage : DocBaseWrapper
	{
		public static DocSADHPage New(BusinessObjectFactory factory, CusEntryLine entryLine1)
		{
			return new DocSADHPage(factory, DocSADHLine.New(entryLine1, factory), null, null);
		}

		public static DocSADHPage New(BusinessObjectFactory factory, EUCusEntryLineCollection entryLines, int startFrom)
		{
			return new DocSADHPage(factory
				, GetElementSafe(entryLines, startFrom, factory)
				, GetElementSafe(entryLines, startFrom + 1, factory)
				, GetElementSafe(entryLines, startFrom + 2, factory)
				);
		}

		protected DocSADHPage(BusinessObjectFactory factory, DocSADHLine line1, DocSADHLine line2, DocSADHLine line3)
			: base(null, factory)
		{
			Line1 = line1;
			Line2 = line2;
			Line3 = line3;
		}

		public DocSADHLine Line1
		{
			get;
			private set;
		}

		public DocSADHLine Line2
		{
			get;
			private set;
		}

		public DocSADHLine Line3
		{
			get;
			private set;
		}

		public ZString BISCaption => BISCaptionCore;
		protected virtual ZString BISCaptionCore => ZString.Empty;

		public DocSADHLineTaxCollection Box47TaxesTotals => Box47TaxesTotalsCore;
		protected virtual DocSADHLineTaxCollection Box47TaxesTotalsCore => new DocSADHLineTaxCollection(Enumerable.Empty<IDocSADHLineTaxBoxSupporter>(), Factory);

		public ZString Box47TotalAmount => Box47TotalAmountCore;
		protected virtual ZString Box47TotalAmountCore => ZString.Empty;

		public ZString Box47TotalMethodOfPayment => Box47TotalMethodOfPaymentCore;
		protected virtual ZString Box47TotalMethodOfPaymentCore => ZString.Empty;

		static DocSADHLine GetElementSafe(EUCusEntryLineCollection entryLines, int index, BusinessObjectFactory factory)
		{
			CusEntryLine entryLine = index < entryLines.Count ? entryLines[index] : null;
			return DocSADHLine.New(entryLine, factory);
		}
	}
}
