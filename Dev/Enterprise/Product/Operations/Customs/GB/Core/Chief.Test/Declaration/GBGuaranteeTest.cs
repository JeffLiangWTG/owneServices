using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	[TestedType(typeof(GBGuarantee))]
	public class GBGuaranteeTest : Business.Declaration.Testing.GBGuaranteeTest
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration.Guarantees.AddNew();
		}
	}
}
