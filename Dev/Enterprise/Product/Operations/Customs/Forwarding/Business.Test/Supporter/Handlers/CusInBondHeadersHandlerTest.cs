using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	sealed class CusInBondHeadersHandlerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var header1 = GetCusInBondHeader(shipment1, "I0000");
			header1[CusInBondHeaderSchema.BH_IsActive] = true;
			var header2 = GetCusInBondHeader(shipment2, "I0001");
			header2[CusInBondHeaderSchema.BH_IsActive] = false;
			Factory.Save();
			var newFactory = NewFactory();
			shipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
			shipment2 = newFactory.Load<ForwardingShipment>(shipment2.PK);
			shipment3 = newFactory.Load<ForwardingShipment>(shipment3.PK);
			var handler = new CusInBondHeadersHandler();
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(shipment1, true).Single()).PK, Is.EqualTo(header1.PK), "Should find the header1.");
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(shipment1, false).Single()).PK, Is.EqualTo(header1.PK), "Should find the header1.");
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(shipment2, true).Single()).PK, Is.EqualTo(header2.PK), "Should find the header2.");
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(shipment2, false).Single()).PK, Is.EqualTo(header2.PK), "Should find the header2.");
			NUnit.Framework.Assert.That(handler.Load(shipment3, true).FirstOrDefault(), Is.EqualTo(default(ICancellable)), "Should be null as it does not have any related InBondHeader. - should be [null]");
			NUnit.Framework.Assert.That(handler.Load(shipment3, false).FirstOrDefault(), Is.EqualTo(default(ICancellable)), "Should be null as it does not have any related InBondHeader. - should be [null]");
		}

		BusinessObject GetCusInBondHeader(ForwardingShipment shipment, string jobReference)
		{
			var cusInBondHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			((BusinessObject)cusInBondHeader).FillWithValidTestData();
			cusInBondHeader.BH_JobReference = jobReference;
			cusInBondHeader.BH_ParentID = shipment.PK;
			cusInBondHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			return (BusinessObject)cusInBondHeader;
		}
	}
}
