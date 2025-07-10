using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryLineValidation : Customs.Business.CusEntryLineValidation
	{
		public CusEntryLineValidation(CusEntryLine parent)
			: base(parent)
		{
		}

		public CusEntryLine EntryLine
		{
			get { return Parent; }
		}

		protected new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
		}

		public IEntryLineValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<IEntryLineValidationDecider> validationDeciderCached;

		IEntryLineValidationDecider GetValidationDecider() => Parent.Declaration is JobDeclaration declaration ? declaration?.Configuration.EntryLineConfiguration.GetValidationDecider(Parent) : null;
	}
}
