namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AuthorityToActValidator : Customs.Business.AuthorityToActValidator
	{
		public AuthorityToActValidator()
			: base(Res.GetString("9E919293-E757-4162-9257-9CB82FC01737", "written authority to act"), string.Empty)
		{
		}
	}
}
