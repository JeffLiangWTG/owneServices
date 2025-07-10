using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class ForwardingShipmentCustomsStatusProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShipmentWithManyDecsOnlyReturnsEuDec()
		{
			NUnit.Framework.Assert.That(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, NUnit.Framework.Is.EqualTo("LV").Using(CustomComparers.TypeComparison), "Pre-req - test is running as LV");

			var aussieCompany = Factory.New<GlbCompany>();
			aussieCompany.GC_RN_NKCountryCode = "AU";
			var sydneyBranch = aussieCompany.Branches.AddNew();
			sydneyBranch.GB_RL_NKHomePort = "AUSYD";
			var aussieDec = Factory.New<FakeAussieDeclaration>();
			aussieDec.JE_GB = sydneyBranch.PK;
			aussieDec.JE_MessageType = "EXP";

			var germanCompany = Factory.New<GlbCompany>();
			germanCompany.GC_RN_NKCountryCode = "DE";
			var berlinBranch = germanCompany.Branches.AddNew();
			berlinBranch.GB_RL_NKHomePort = "DEBER";
			var germanDecIsStillEuRemember = Factory.New<JobDeclaration>();
			germanDecIsStillEuRemember.JE_GB = berlinBranch.PK;

			var latvianCompany = Factory.New<GlbCompany>();
			latvianCompany.GC_RN_NKCountryCode = "LV";
			var rigaBranch = latvianCompany.Branches.AddNew();
			rigaBranch.GB_RL_NKHomePort = "LVRIX";
			var latvianDec = Factory.New<JobDeclaration>();
			latvianDec.JE_GB = rigaBranch.PK;
			latvianDec.JE_MessageType = "IMP";

			var shipment = Factory.New<ForwardingShipment>();

			var fscsp = new ForwardingShipmentCustomsStatusProvider(shipment);
			NUnit.Framework.Assert.That(fscsp.CustomsMessageStatus(), NUnit.Framework.Is.EqualTo(ZString.Empty), "The shipment has no declaration");

			latvianDec.JE_JS = shipment.PK;
			germanDecIsStillEuRemember.JE_JS = shipment.PK;
			aussieDec.JE_JS = shipment.PK;

			NUnit.Framework.Assert.That(latvianDec.Country.Code, NUnit.Framework.Is.EqualTo("LV").Using(CustomComparers.TypeComparison), "prereq");
			NUnit.Framework.Assert.That(aussieDec.Country.Code, NUnit.Framework.Is.EqualTo("AU").Using(CustomComparers.TypeComparison), "prereq");
			NUnit.Framework.Assert.That(germanDecIsStillEuRemember.Country.Code, NUnit.Framework.Is.EqualTo("DE").Using(CustomComparers.TypeComparison), "prereq");

			aussieDec.JE_MessageStatus = "AAA";
			latvianDec.JE_MessageStatus = "LLL";
			germanDecIsStillEuRemember.JE_MessageStatus = "DDD";

			fscsp = new ForwardingShipmentCustomsStatusProvider(shipment);
			NUnit.Framework.Assert.That(fscsp.CustomsMessageStatus(), NUnit.Framework.Is.EqualTo("LLL").Using(CustomComparers.TypeComparison), "If the ForwardingShipmentCustomsStatusProvider is pulling the Latvian dec off the shipment, and not pulling the AU or DE one, then the customs status of the shipment is that of the LLL-atvian declaration");
		}

		class FakeAussieDeclaration : Customs.Business.BaseJobDeclaration
		{
			public FakeAussieDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}
	}
}
