using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public class NctsPreviousDocumentPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSubTypeNotMandatory()
		{
			var validation = new NctsPreviousDocumentPhase5ValidationForTest(Factory.New<NctsPreviousDocument>());
			AssertEquals("IsSubTypeMandatory is false in IE", false, validation.IsSubTypeMandatory_Exposed);
		}

		sealed class NctsPreviousDocumentPhase5ValidationForTest : NctsPreviousDocumentPhase5Validation
		{
			public NctsPreviousDocumentPhase5ValidationForTest(NctsPreviousDocument parent) : base(parent)
			{
			}

			public ZBool IsSubTypeMandatory_Exposed => IsSubTypeMandatory;
		}
	}
}
