using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AFRBillDataContextManager))]
	class AFRBillDataContextManagerTest : ShipmentDataContextManagerTestCase<AFRBillDataContextManager, JPAFRBills>
	{
		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRBill</Type>
        </DataSource>
      </DataSourceCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return System.Array.Empty<RecipientRoleType>(); }
		}
	}
}
