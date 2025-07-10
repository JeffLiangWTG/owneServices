namespace Enterprise.Customs.EU.EMCS.Business
{
	public class HeaderProviderHelper
	{
		public HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration)
		{
			this.emcsJobDeclaration = emcsJobDeclaration;
		}
		protected readonly EMCSJobDeclaration emcsJobDeclaration;

		public string AdministrativeReferenceCode => emcsJobDeclaration.EADNumber;
	}
}
