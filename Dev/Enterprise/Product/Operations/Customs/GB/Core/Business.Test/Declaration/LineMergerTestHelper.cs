using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class LineMergerTestHelper : EU.Business.Testing.LineMergerTestHelper
	{
		public LineMergerTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override JobDeclaration GetJobDeclaration()
		{
			var declaration =  base.GetJobDeclaration();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}
	}
}
