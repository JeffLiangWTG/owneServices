using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
		, Integration.Customs.GB.IPreviousDocument
		, ILookupsResetter
	{
		//		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public PreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

		void ILookupsResetter.Reset()
		{
			fLookups = null;
		}

		protected override bool IsLookupsCachedInBase => false;

		Customs.Business.CusSupportingInfoLookups fLookups;
		protected override Customs.Business.CusSupportingInfoLookups GetNewLookups()
		{
			if (fLookups == null)
			{
				var declaration = (Parent as JobDeclaration ?? ((Parent as JobComInvoiceHeader)?.JobDeclaration)) ?? ((Parent as JobComInvoiceLine)?.Declaration);

				fLookups = new PreviousDocumentLookups(this);
			}
			return fLookups;
		}
	}
}
