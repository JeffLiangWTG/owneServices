using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	abstract class CusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
	{
		protected abstract string MessageType { get; }

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
		}
		protected JobDeclaration declaration;
	}
}
