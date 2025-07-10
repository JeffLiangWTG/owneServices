using CargoWise.Integration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		public override ICodeDescriptionPairList ValuationCodeList => Factory.GetTranNatureList(((ICanBeImportOrExport)Invoice).DataGroupingCode);

		public override CodeDescriptionPairList JZ_IncoTerm_List => Parent.JobDeclaration is JobDeclaration declaration ? Factory.GetCachedIncoTermListEU(declaration.IsUCC6) : base.JZ_IncoTerm_List;
	}
}
