using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryHeaderValidation : Customs.Business.CusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public IEntryHeaderValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<IEntryHeaderValidationDecider> validationDeciderCached;

		IEntryHeaderValidationDecider GetValidationDecider() => Parent.Declaration is JobDeclaration declaration ? declaration.Configuration.EntryHeaderConfiguration.GetValidationDecider(Parent) : null;
	}
}
