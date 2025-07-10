using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.Invoices.AddNew();
			var line = dec.InvoiceLines.AddNew();
			var add = line.AdditionalInfos.AddNew();
			Factory.Save();
			yield return add;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.Invoices.AddNew();
			var line = dec.InvoiceLines.AddNew();
			var add = line.AdditionalInfos.AddNew();
			return add;
		}

		public void TestGetNewValidation()
		{
			var dec = Factory.New<JobDeclaration>();
			var additionalInfo = dec.AdditionalInfos.AddNew();
			AssertType<AdditionalInfoValidation>(additionalInfo.Validation);

			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEAdditionalInfoValidation>(additionalInfo.Validation);
		}

		public void TestIsLine_WhenDeclarationIsNonUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var additionalInfo = declaration.AdditionalInfos.AddNew();

			Assert("When declaration is not UCC6, AdditionalInfo.IsLine should be true.", additionalInfo.IsLine);
		}

		public void TestIsLine_WhenDeclarationIsUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo = declaration.AdditionalInfos.AddNew();

			Assert("When declaration is UCC6, AdditionalInfo.IsLine should be false.", !additionalInfo.IsLine);
		}

		public void TestIsHeaderOnly_WhenDeclarationIsNonUCC6()
		{
			var cusCodeListConfig = new RefDataConfig(
				dataGroupings: [new(code: "FR", description: "France", parent: "")],
				cusCodeTypes:
				[
					new(
						typeCode: "ADDIN", description: "Additional Info.", attributeTypes: [new(name: "Level", dataGrouping: "FR")],
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "FR", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE2", dataGrouping: "FR", attributes: [new(name: "", value: "")]),
							new(code: "CODE3", dataGrouping: "FR", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
						]
					)
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(Factory, cusCodeListConfig);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var additionalInfo1 = declaration.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "CODE1";

			var additionalInfo2 = declaration.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "CODE2";

			var additionalInfo3 = declaration.AdditionalInfos.AddNew();
			additionalInfo3.CSI_Code = "CODE3";

			CombineAssertions("When declaration is not UCC6, we should follow EU logic for IsHeaderOnly.", () =>
			{
				Assert("additionalInfo1.IsHeaderOnly should be true because Level = Header attribute is configured and Level = Item is not.", additionalInfo1.IsHeaderOnly);
				Assert("additionalInfo2.IsHeaderOnly should be false because no Level = Header attribute is configured.", !additionalInfo2.IsHeaderOnly);
				Assert("additionalInfo3.IsHeaderOnly should be false because both Level = Header & Level = Item attributes are configured.", !additionalInfo3.IsHeaderOnly);
			});
		}

		public void TestIsHeaderOnly_WhenDeclarationIsUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo = declaration.AdditionalInfos.AddNew();

			CombineAssertions(() =>
			{
				Assert("When declaration is UCC6, AdditionalInfo.IsHeaderOnly should always be true.", additionalInfo.IsHeaderOnly);
			});
		}

		public void TestIsLineOnlyCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var additionalInfo = declaration.AdditionalInfos.AddNew();
			AssertEquals(true, additionalInfo.IsLineOnly);
		}
	}
}
