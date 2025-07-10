using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Retrieve
{
	public class RetieverTest : TestCase
	{
		public void TestCheckCriterias_EmptyCriteria()
		{
			AssertExceptionThrown(
				"Should throw exception when there is no criteria",
				typeof(NativeXMLUserVisibleException),
				() => retriever.CheckCriterias(definition, System.Array.Empty<EntityCriteria>())
				);
		}

		Retriever retriever;
		EntitySetDefinition definition;
		protected override void SetUp()
		{
			base.SetUp();
			retriever = new DummyRetiever();
			definition = TestUtil.GetEntitySetDefinition("Organization");
		}
	}
}
