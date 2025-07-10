using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	sealed class CusSCAOceanBillsHandlerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			var header1 = GetBill(consol1, "A0000");
			header1[CusSCAOceanBillSchema.CB_IsActive] = true;
			var header2 = GetBill(consol2, "A0001");
			header2[CusSCAOceanBillSchema.CB_IsActive] = false;
			Factory.Save();
			var newFactory = NewFactory();
			consol1 = newFactory.Load<ForwardingConsol>(consol1.PK);
			consol2 = newFactory.Load<ForwardingConsol>(consol2.PK);
			consol3 = newFactory.Load<ForwardingConsol>(consol3.PK);
			var handler = new CusSCAOceanBillsHandler();
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(consol1, true).Single()).PK, Is.EqualTo(header1.PK), "Should find the header1.");
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(consol1, false).Single()).PK, Is.EqualTo(header1.PK), "Should find the header1.");
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(consol2, true).Single()).PK, Is.EqualTo(header2.PK), "Should find the header2.");
			NUnit.Framework.Assert.That(((BusinessObject)handler.Load(consol2, false).Single()).PK, Is.EqualTo(header2.PK), "Should find the header2.");
			NUnit.Framework.Assert.That(handler.Load(consol3, true).FirstOrDefault(), Is.EqualTo(default(ICancellable)), "Should be null as it does not have any related OceanBill. - should be [null]");
			NUnit.Framework.Assert.That(handler.Load(consol3, false).FirstOrDefault(), Is.EqualTo(default(ICancellable)), "Should be null as it does not have any related OceanBill. - should be [null]");
		}

		BusinessObject GetBill(ForwardingConsol consol, string masterHouseBill)
		{
			var header = Factory.New<Integration.Customs.Shared.IBaseCusSCAOceanBill>();
			((BusinessObject)header).FillWithValidTestData();
			header.CB_ParentId = consol.PK;
			header.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			header.CB_MasterHouseBill = masterHouseBill;
			return (BusinessObject)header;
		}
	}
}
