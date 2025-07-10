using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusCodeCollection))]
	public class DocCusCodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCusCodeCollection>
	{
		public void TestIndexer()
		{
			RefCountry country1 = Factory.New<RefCountry>();
			country1.RN_Code = "Z1";

			RefCountry country2 = Factory.New<RefCountry>();
			country2.RN_Code = "Z2";

			var organisation = Factory.New<OrgHeader>();
			OrgCusCode cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = country1.Code;
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			cusCode1.OK_CustomsRegNo = "CODE1";

			OrgCusCode cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = country1.Code;
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;
			cusCode2.OK_CustomsRegNo = "CODE2";

			OrgCusCode cusCode3 = organisation.CustomsCodes.AddNew();
			cusCode3.OK_RN_NKCodeCountry = country2.Code;
			cusCode3.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			cusCode3.OK_CustomsRegNo = "CODE3";

			DocCusCodeCollection docCollection = new DocCusCodeCollection(organisation.CustomsCodes, Factory);
			AssertEquals("Count", 3, docCollection.Count);

			DocCusCode docCusCode = docCollection["asd"];
			AssertNull(docCusCode);
			docCusCode = docCollection[OrgCusCode.CodeTypes.GSTCode.ToLower() + "," + country1.Code];
			AssertEquals("CODE1", docCusCode.CustomsRegNo);

			docCusCode = docCollection[OrgCusCode.CodeTypes.GlobalTrackingName + "," + country1.Code.ToLower()];
			AssertEquals("CODE2", docCusCode.CustomsRegNo);

			docCusCode = docCollection[OrgCusCode.CodeTypes.BrokerageRegistration.ToUpper() + "," + country1.Code];
			AssertNull(docCusCode);

			docCusCode = docCollection[OrgCusCode.CodeTypes.BrokerageRegistration + "," + country2.Code];
			AssertEquals("CODE3", docCusCode.CustomsRegNo);
		}

		public void TestGetCustomsRegNoForCodeAndCountry()
		{
			var organisation = Factory.New<OrgHeader>();
			OrgCusCode decoyCusCode = organisation.CustomsCodes.AddNew();
			decoyCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
			decoyCusCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;

			OrgCusCode decoyCusCode2 = organisation.CustomsCodes.AddNew();
			decoyCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			decoyCusCode2.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;

			OrgCusCode cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;
			cusCode.OK_CustomsRegNo = "Correct match";

			DocCusCodeCollection docCollection = new DocCusCodeCollection(organisation.CustomsCodes, Factory);
			RefCountry malaysia = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Malaysia);
			ZString foundCusCode = docCollection.GetCustomsRegNoForCodeAndCountry(OrgCusCode.CodeTypes.GlobalTrackingName, malaysia);
			AssertEquals("Should find the correct DocCusCode", "Correct match", foundCusCode);
		}

		public void TestGetCustomsRegNoAndTypeForCodeAndCountry()
		{
			var organisation = Factory.New<OrgHeader>();
			OrgCusCode decoyCusCode = organisation.CustomsCodes.AddNew();
			decoyCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
			decoyCusCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;

			OrgCusCode decoyCusCode2 = organisation.CustomsCodes.AddNew();
			decoyCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			decoyCusCode2.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;

			OrgCusCode cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;
			cusCode.OK_CustomsRegNo = "Correct match";

			OrgCusCode brazilCusCode = organisation.CustomsCodes.AddNew();
			brazilCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			brazilCusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			brazilCusCode.OK_CustomsRegNo = "brazil cus code";

			DocCusCodeCollection docCollection = new DocCusCodeCollection(organisation.CustomsCodes, Factory);
			RefCountry malaysia = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Malaysia);
			ZString foundCusCode = docCollection.GetCustomsRegNoAndTypeForCodeAndCountry(OrgCusCode.CodeTypes.GlobalTrackingName, malaysia);
			AssertEquals("Should find the correct DocCusCode", "GTN: Correct match", foundCusCode);

			RefCountry brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);
			foundCusCode = docCollection.GetCustomsRegNoAndTypeForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, brazil);
			AssertEquals("Should find the correct DocCusCode", "CNPJ: brazil cus code", foundCusCode);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			return DocCusCode.New(orgCusCode, Factory);
		}

		protected override DocCusCodeCollection GetCollectionToTest()
		{
			var organisation = Factory.New<OrgHeader>();
			return new DocCusCodeCollection(organisation.CustomsCodes, Factory);
		}
	}
}
