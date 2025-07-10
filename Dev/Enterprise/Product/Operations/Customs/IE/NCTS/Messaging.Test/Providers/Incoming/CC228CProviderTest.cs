using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC228C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC228CProviderTest : TestCaseWithFactory
	{
		public void TestGuaranteeReference()
		{
			AssertEquals("GuaranteeReferences should have 1 item.", 1, provider.GuaranteeReferences.Count);
			var defaultGuaranteeReference = provider.GuaranteeReferences.First();
			AssertEquals("Should have created GuaranteeReferences items correctly from input.", "12GRNCC055C012345A678901", defaultGuaranteeReference.GRN);
		}

		public void TestGuarantor()
		{
			var defaultGuarantor = provider.Guarantor;
			AssertEquals("Should have created Guarantor correctly from input.", "BOB THE BUILDER", defaultGuarantor.Name);
		}

		CC228CProvider provider;

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC228CProvider(CreateStandardProvider());
		}

		public static Cc228CType CreateStandardProvider() => new Cc228CType
		{
			MessageType = MessageTypes.Cc228C,
			GuaranteeReference = new Collection<GuaranteeReferenceType10>
			{
				CC228CGuaranteeReferenceProviderTest.CreateStandardProvider()
			},
			Guarantor = CC228CGuarantorProviderTest.CreateStandardProvider(),
		};
	}
}
