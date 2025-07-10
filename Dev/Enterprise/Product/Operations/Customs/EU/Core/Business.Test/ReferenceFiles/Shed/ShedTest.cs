using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(Shed))]
	public class ShedTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var shed1 = CreateShed(Factory, "GB", "LHRAAS", "AMERICAN AIRLINES at Heathrow");
			AssertEquals("LHRAAS", shed1.Code);
			AssertEquals("LHR", shed1.PortCode);
			AssertEquals("AAS", shed1.ShedCode);
			AssertEquals("AMERICAN AIRLINES at Heathrow", shed1.Name);
			AssertEquals(false, shed1.IsETSF);
			AssertEquals(false, shed1.IsDEP);
			AssertEquals("", shed1.ChiefPort);
			AssertEquals("", shed1.ChiefShed);
			AssertEquals("", shed1.ACPCode);
			AssertEquals("", shed1.AirportName);
			Assert(shed1.IsAtLondonHeathrow);
			Assert(!shed1.IsEtsfAtLondonHeathrow);

			var shed2 = CreateShed(Factory, "GB", "LHRACS", "AIR CANADA at Heathrow", portName: "Heathrow", acpCode: "H", isETSF: true, isDEP: true, chiefPort: "XXX", chiefShed: "YYY");
			AssertEquals("LHRACS", shed2.Code);
			AssertEquals("LHR", shed2.PortCode);
			AssertEquals("ACS", shed2.ShedCode);
			AssertEquals("AIR CANADA at Heathrow", shed2.Name);
			AssertEquals(true, shed2.IsETSF);
			AssertEquals(true, shed2.IsDEP);
			AssertEquals("H", shed2.ACPCode);
			AssertEquals("XXX", shed2.ChiefPort);
			AssertEquals("YYY", shed2.ChiefShed);
			AssertEquals("Heathrow", shed2.AirportName);
			Assert(shed2.IsAtLondonHeathrow);
			Assert(shed2.IsEtsfAtLondonHeathrow);
		}

		public void TestLoadByCode()
		{
			var shed1 = CreateShed(Factory, "GB", "LHRAAS", "GB AMERICAN AIRLINES at Heathrow");
			var shed2 = CreateShed(Factory, "GB", "LHRACS", "GB AIR CANADA  at Heathrow");
			var shed3 = CreateShed(Factory, "FR", "LHRAAS", "FR AMERICAN AIRLINES at Heathrow");
			Factory.Save();

			AssertEquals("GB AMERICAN AIRLINES at Heathrow", Business.Shed.LoadByCode(Factory, "GB", "LHRAAS").Name);
			AssertEquals("GB AIR CANADA  at Heathrow", Business.Shed.LoadByCode(Factory, "GB", "LHRACS").Name);
			AssertEquals("FR AMERICAN AIRLINES at Heathrow", Business.Shed.LoadByCode(Factory, "FR", "LHRAAS").Name);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Shed(Factory.New<ZZRefCusCodeListCombined>());
		}

		public static Shed CreateShed(BusinessObjectFactory factory, ZString dataGroupingCode, ZString shedCode, ZString shedDescription,
			ZDateTime? startDate = null, ZDateTime? endDate = null,
			string portName = "", string acpCode = "", bool isETSF = false, bool isDEP = false, string chiefPort = "", string chiefShed = ""
			, string[] transportModes = null
			, string siteIdAttribute = "")
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = dataGroupingCode;
			cusCodeList.ZZD_CodeType = UniversalReferenceConstants.RefCusCodeListType.Shed;
			cusCodeList.ZZD_Code = shedCode;
			cusCodeList.ZZD_Description = shedDescription;
			cusCodeList.ZZD_StartDate = startDate ?? ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = endDate ?? ZDateTime.Today.AddYears(10);

			if (!string.IsNullOrEmpty(portName))
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.ShedAttributes.AirportName, portName);
			}

			if (!string.IsNullOrEmpty(acpCode))
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.ShedAttributes.ACPCode, acpCode);
			}

			if (!string.IsNullOrEmpty(chiefShed))
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.ShedAttributes.ChiefShed, chiefShed);
			}

			if (!string.IsNullOrEmpty(chiefPort))
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.ShedAttributes.Chief, chiefPort);
			}

			if (!string.IsNullOrEmpty(siteIdAttribute))
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.ShedAttributes.SITECODE, siteIdAttribute);
			}

			if (isETSF)
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.ShedAttributes.ETSF, ZString.Empty);
			}

			if (isDEP)
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.ShedAttributes.DEP, ZString.Empty);
			}

			if (transportModes != null)
			{
				foreach (var mode in transportModes)
				{
					cusCodeList.Attributes.AddNew(UniversalReferenceConstants.PortAttributes.Type, mode);
				}
			}
			return new Shed(cusCodeList);
		}
	}
}
