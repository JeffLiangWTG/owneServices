using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobDeclarationForCustomsOfficeRequirementTest : JobDeclaration
	{
		public JobDeclarationForCustomsOfficeRequirementTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new CustomsOfficeRequirementHelperForTest(this);

		public new CustomsOfficeRequirementHelperForTest CustomsOfficeRequirementHelper => (CustomsOfficeRequirementHelperForTest)base.CustomsOfficeRequirementHelper;
	}
}
