using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class EnterpriseQualificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEnterpriseQualificationList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var epq = instruction.EnterpriseQualifications.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("List.Count", 34, epq.Lookups.CY_CodeList.Count);
			AssertContainsCorrectEnterpriseQualificationCodes(epq.Lookups.CY_CodeList, "IMP");
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("List.Count", 39, epq.Lookups.CY_CodeList.Count);
			AssertContainsCorrectEnterpriseQualificationCodes(epq.Lookups.CY_CodeList, "EXP");
			declaration.JE_MessageType = "OTH";
			AssertEquals("List.Count", 39, epq.Lookups.CY_CodeList.Count);
			AssertContainsCorrectEnterpriseQualificationCodes(epq.Lookups.CY_CodeList, "OTH");
			var emptyInstruction = Factory.New<CusEntryInstruction>();
			epq = emptyInstruction.EnterpriseQualifications.AddNew();
			AssertEquals("List.Count", 0, epq.Lookups.CY_CodeList.Count);
		}

		void AssertContainsCorrectEnterpriseQualificationCodes(CodeDescriptionPairList list, string messageType)
		{
			var isIMP = messageType == "IMP";
			CombineAssertions("The following codes should exists (or not exists)", () =>
			{
				Assert("Contains 100", list.ContainsCode("100"));
				Assert("Contains 101", list.ContainsCode("101"));
				Assert("Contains 102", list.ContainsCode("102") == !isIMP);
				Assert("Contains 200", list.ContainsCode("200"));
				Assert("Contains 300", list.ContainsCode("300"));
				Assert("Contains 301", list.ContainsCode("301") == !isIMP);
				Assert("Contains 302", list.ContainsCode("302") == !isIMP);
				Assert("Contains 303", list.ContainsCode("303") == isIMP);
				Assert("Contains 304", list.ContainsCode("304") == !isIMP);
				Assert("Contains 305", list.ContainsCode("305") == !isIMP);
				Assert("Contains 306", list.ContainsCode("306") == isIMP);
				Assert("Contains 307", list.ContainsCode("307") == isIMP);
				Assert("Contains 308", list.ContainsCode("308") == !isIMP);
				Assert("Contains 309", list.ContainsCode("309") == !isIMP);
				Assert("Contains 310", list.ContainsCode("310") == !isIMP);
				Assert("Contains 311", list.ContainsCode("311") == !isIMP);
				Assert("Contains 312", list.ContainsCode("312") == isIMP);
				Assert("Contains 315", list.ContainsCode("315") == !isIMP);
				Assert("Contains 317", list.ContainsCode("317"));
				Assert("Contains 318", list.ContainsCode("318") == !isIMP);
				Assert("Contains 319", list.ContainsCode("319") == isIMP);
				Assert("Contains 320", list.ContainsCode("320") == isIMP);
				Assert("Contains 321", list.ContainsCode("321") == isIMP);
				Assert("Contains 322", list.ContainsCode("322") == isIMP);
				Assert("Contains 323", list.ContainsCode("323") == !isIMP);
				Assert("Contains 324", list.ContainsCode("324") == !isIMP);
				Assert("Contains 326", list.ContainsCode("326") == isIMP);
				Assert("Contains 329", list.ContainsCode("329") == !isIMP);
				Assert("Contains 400", list.ContainsCode("400"));
				Assert("Contains 413", list.ContainsCode("413") == isIMP);
				Assert("Contains 414", list.ContainsCode("414") == isIMP);
				Assert("Contains 415", list.ContainsCode("415"));
				Assert("Contains 416", list.ContainsCode("416") == isIMP);
				Assert("Contains 417", list.ContainsCode("417") == !isIMP);
				Assert("Contains 418", list.ContainsCode("418"));
				Assert("Contains 419", list.ContainsCode("419") == !isIMP);
				Assert("Contains 421", list.ContainsCode("421"));
				Assert("Contains 500", list.ContainsCode("500"));
				Assert("Contains 501", list.ContainsCode("501") == !isIMP);
				Assert("Contains 502", list.ContainsCode("502") == !isIMP);
				Assert("Contains 503", list.ContainsCode("503") == !isIMP);
				Assert("Contains 504", list.ContainsCode("504") == !isIMP);
				Assert("Contains 505", list.ContainsCode("505") == !isIMP);
				Assert("Contains 506", list.ContainsCode("506") == !isIMP);
				Assert("Contains 507", list.ContainsCode("507") == !isIMP);
				Assert("Contains 508", list.ContainsCode("508") == isIMP);
				Assert("Contains 509", list.ContainsCode("509") == isIMP);
				Assert("Contains 510", list.ContainsCode("510") == isIMP);
				Assert("Contains 511", list.ContainsCode("511") == isIMP);
				Assert("Contains 512", list.ContainsCode("512") == !isIMP);
				Assert("Contains 513", list.ContainsCode("513") == isIMP);
				Assert("Contains 514", list.ContainsCode("514") == !isIMP);
				Assert("Contains 515", list.ContainsCode("515") == isIMP);
				Assert("Contains 518", !list.ContainsCode("518"));
				Assert("Contains 519", !list.ContainsCode("519"));
				Assert("Contains 520", list.ContainsCode("520") == !isIMP);
				Assert("Contains 524", list.ContainsCode("524") == isIMP);
				Assert("Contains 600", list.ContainsCode("600") == isIMP);
				Assert("Contains 602", list.ContainsCode("602") == !isIMP);
				Assert("Contains 603", list.ContainsCode("603"));
				Assert("Contains 700", list.ContainsCode("700"));
				Assert("Contains 601", list.ContainsCode("601") == isIMP);
				Assert("Contains 327", list.ContainsCode("327") == isIMP);
				Assert("Contains 800", !list.ContainsCode("800"));
				Assert("Contains 900", !list.ContainsCode("900"));
			}

			);
		}

		public void TestCY_DataList()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "A";
			orgHeader1.CustomsCodes.AddNew("CCD", "1234567890", "CN");
			orgHeader1.CustomsCodes.AddNew("101", "A0001", "CN");
			orgHeader1.CustomsCodes.AddNew("303", "A0002", "CN");
			orgHeader1.CustomsCodes.AddNew("306", "A0003", "CN");
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "B";
			orgHeader2.CustomsCodes.AddNew("USC", "123456789012345678", "CN");
			orgHeader2.CustomsCodes.AddNew("101", "B0001", "CN");
			orgHeader2.CustomsCodes.AddNew("303", "B0002", "CN");
			orgHeader2.CustomsCodes.AddNew("306", "B0003", "CN");
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "C";
			orgHeader3.CustomsCodes.AddNew("CCD", "1234567890", "CN");
			orgHeader3.CustomsCodes.AddNew("101", "C0001", "CN");
			orgHeader3.CustomsCodes.AddNew("303", "C0002", "CN");
			orgHeader3.CustomsCodes.AddNew("306", "C0003", "CN");
			var orgHeader4 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "D";
			orgHeader4.CustomsCodes.AddNew("USC", "123456789012345678", "CN");
			orgHeader4.CustomsCodes.AddNew("101", "D0001", "CN");
			orgHeader4.CustomsCodes.AddNew("303", "D0002", "CN");
			orgHeader4.CustomsCodes.AddNew("306", "D0003", "CN");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = orgHeader1.PK;
			declaration.JE_OH_Buyer = orgHeader2.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var qualification = instruction.EnterpriseQualifications.AddNew();
			AssertEquals(6, qualification.Lookups.CY_DataList.Count);
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("A0001", "(A)检疫处理单位审批")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("A0002", "(A)进境水果境外果园/包装厂注册登记")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("A0003", "(A)进口饲料和饲料添加剂生产企业注册登记")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("B0001", "(B)检疫处理单位审批")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("B0002", "(B)进境水果境外果园/包装厂注册登记")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("B0003", "(B)进口饲料和饲料添加剂生产企业注册登记")));
			qualification.CY_Code = "101";
			AssertEquals(2, qualification.Lookups.CY_DataList.Count);
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("A0001", "(A)检疫处理单位审批")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("B0001", "(B)检疫处理单位审批")));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = orgHeader3.PK;
			declaration.JE_OH_Manufacturer = orgHeader4.PK;
			qualification.CY_Code = ZString.Empty;
			AssertEquals(2, qualification.Lookups.CY_DataList.Count);
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("C0001", "(C)检疫处理单位审批")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("D0001", "(D)检疫处理单位审批")));
			qualification.CY_Code = "101";
			AssertEquals(2, qualification.Lookups.CY_DataList.Count);
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("C0001", "(C)检疫处理单位审批")));
			Assert(qualification.Lookups.CY_DataList.Contains(new CodeDescriptionPair("D0001", "(D)检疫处理单位审批")));
		}
	}
}
