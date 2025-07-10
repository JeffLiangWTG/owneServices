using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class AddInfoJobComInvoiceHeader : EU.Business.Declaration.AddInfoJobComInvoiceHeader
	{
		public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}

		public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		[MaxLength(nameof(ZG_AgreedPlaceCodeMaxLength))]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set => base.ZG_AgreedPlaceCode = value;
		}
		int ZG_AgreedPlaceCodeMaxLength => Parent.IsImport ? 1 : 5;

		public new AddInfoJobComInvoiceHeaderValidation Validation => (AddInfoJobComInvoiceHeaderValidation)base.Validation;

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceHeaderValidation(this);

		public new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceHeaderLookups(this);
	}
}
