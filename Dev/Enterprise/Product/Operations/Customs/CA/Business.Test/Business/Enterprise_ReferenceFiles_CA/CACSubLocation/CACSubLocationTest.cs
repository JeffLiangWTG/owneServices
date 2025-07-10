using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACSubLocation))]
	public class CACSubLocationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CACSubLocation(Factory.New<ZZRefCusCodeListCombined>());
		}

		public void TestHumanReadableName()
		{
			var subLocation = CreateSubLocation(Factory, "1234");
			AssertEquals("HumanReadableName", "Sub-Location Code: '1234'", subLocation.HumanReadableName);
		}

		public void TestProperties()
		{
			var cusCodeList = CreateRefCusCodeListForCACSubLocation(Factory, "4570", "YUSEN LOGISTICS (CANADA ) INC.", "0497");
			cusCodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Division, "1300");
			cusCodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Names.City, "BRAMPTON");
			cusCodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Province, "ON");
			cusCodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Street, "261 PARKHURST SQUARE");
			cusCodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Names.PostCode, "L6T5H5");
			cusCodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Type, "CW");

			var subLocation = new CACSubLocation(cusCodeList);
			AssertEquals("4570", subLocation.Code);
			AssertEquals("YUSEN LOGISTICS (CANADA ) INC.", subLocation.Description);
			AssertEquals("1300", subLocation.SL_Division);
			AssertEquals("0497", subLocation.SL_Port);
			AssertEquals("261 PARKHURST SQUARE", subLocation.SL_Address);
			AssertEquals("BRAMPTON", subLocation.SL_City);
			AssertEquals(ZDateTime.Today.AddYears(-10), subLocation.SL_EffectiveDate);
			AssertEquals("L6T5H5", subLocation.SL_PostCode);
			AssertEquals("ON", subLocation.SL_Province);
			AssertEquals("CW", subLocation.SL_Type);
		}

		static ZZRefCusCodeListCombined CreateRefCusCodeListForCACSubLocation(BusinessObjectFactory factory, ZString code, string description = "", string port = "")
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			cusCodeList.ZZD_CodeType = UniversalReferenceConstants.RefCusCodeListType.Codes.SubLocation;
			cusCodeList.ZZD_Code = code;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);

			if (!string.IsNullOrEmpty(port))
			{
				cusCodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Port, port);
			}

			return cusCodeList;
		}

		public static CACSubLocation CreateSubLocation(BusinessObjectFactory factory, ZString code, string description = "", string port = "")
		{
			return new CACSubLocation(CreateRefCusCodeListForCACSubLocation(factory, code, description, port));
		}
	}
}
