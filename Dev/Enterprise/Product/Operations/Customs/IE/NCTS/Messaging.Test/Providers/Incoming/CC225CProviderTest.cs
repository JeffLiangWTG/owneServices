using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC225C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC225CProviderTest : TestCaseWithFactory
	{
		public void TestGuaranteeReferences()
		{
			AssertEquals("No GuaranteeReferences", 0, new CC225CProvider(new Cc225CType()).GuaranteeReferences.Count);
			AssertEquals("Count GuaranteeReferences", 2, provider.GuaranteeReferences.Count);
			AssertEquals("1st GuaranteeReference", "GRN001", provider.GuaranteeReferences.ElementAt(0).GRN);
			AssertEquals("2nd GuaranteeReference", "GRN002", provider.GuaranteeReferences.ElementAt(1).GRN);
			AssertSame("Is cached", provider.GuaranteeReferences, provider.GuaranteeReferences);
		}

		public static Cc225CType CreateStandardProvider() => new Cc225CType
		{
			MessageType = MessageTypes.Cc225C,
			HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType01
			{
				IdentificationNumber = "IN231"
			},
			GuaranteeReference = new Collection<GuaranteeReferenceType09>
			{
				CC225CGuaranteeReferenceProviderTest.CreateStandardProvider()
			}
		};

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC225CProvider(new Cc225CType()
			{
				GuaranteeReference = new Collection<GuaranteeReferenceType09>()
				{
					new GuaranteeReferenceType09()
					{
						Grn = "GRN001",
						ValidityDate = new System.DateTime(2023, 1, 1),
						InvalidityDate = new System.DateTime(2024, 1, 31),
					},
					new GuaranteeReferenceType09()
					{
						Grn = "GRN002",
						ValidityDate = new System.DateTime(2023, 2, 1),
						InvalidityDate = new System.DateTime(2024, 2, 28),
					},
				},
			});
		}
		CC225CProvider provider;
	}
}
