using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(BulkLookupChanger))]
	sealed class BulkLookupChangerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidationGeneral()
		{
			BusinessObjectForTest.RunPreSaveValidation();
			Assert(BusinessObjectForTest.TreatmentCodeInfo.HasErrors());
			Assert(BusinessObjectForTest.RemoveTreatmentCodeInfo.HasErrors());
			BusinessObjectForTest.TreatmentCode = "123";
			Assert(!BusinessObjectForTest.TreatmentCodeInfo.HasErrors());
			Assert(!BusinessObjectForTest.RemoveTreatmentCodeInfo.HasErrors());
			BusinessObjectForTest.RemoveTreatmentCode = true;
			Assert(BusinessObjectForTest.TreatmentCodeInfo.HasErrors());
			Assert(BusinessObjectForTest.RemoveTreatmentCodeInfo.HasErrors());
			BusinessObjectForTest.TreatmentCode = ZString.Empty;
			Assert(!BusinessObjectForTest.TreatmentCodeInfo.HasErrors());
			Assert(!BusinessObjectForTest.RemoveTreatmentCodeInfo.HasErrors());
			BusinessObjectForTest.RemoveTreatmentCode = false;
			Assert(BusinessObjectForTest.TreatmentCodeInfo.HasErrors());
			Assert(BusinessObjectForTest.RemoveTreatmentCodeInfo.HasErrors());
		}

		#region Implementation

		#region BusinessObjectForTest

		BulkLookupChanger BusinessObjectForTest
		{
			get
			{
				if (businessObjectForTest == null)
				{
					businessObjectForTest = new BulkLookupChanger(Factory);
				}
				return businessObjectForTest;
			}
		}
		BulkLookupChanger businessObjectForTest;

		#endregion

		#endregion

		public void TestChangeTreatmentCodesPreconditions()
		{
			AssertEquals("123", class1.TreatmentCode);
			AssertEquals("123", class2.TreatmentCode);
			AssertEquals("456", class3.TreatmentCode);
			AssertEquals("123", class4.TreatmentCode);
			AssertEquals(ZString.Empty, class5.TreatmentCode);
			AssertEquals("123", pivot1.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot1.TreatmentCode);
			AssertEquals("555", pivot2.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("555", pivot2.TreatmentCode);
			AssertEquals(ZString.Empty, pivot3.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot3.TreatmentCode);
			AssertEquals("456", pivot4.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("456", pivot4.TreatmentCode);
			AssertEquals("123", pivot5.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot5.TreatmentCode);
			AssertEquals("666", pivot6.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("666", pivot6.TreatmentCode);
			AssertEquals(ZString.Empty, pivot7.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot7.TreatmentCode);
		}

		public void TestChangeTreatmentCodesA()
		{
			changer.TreatmentCode = "789";
			changer.ChangeLookups(query);
			AssertEquals("789", class1.TreatmentCode);
			AssertEquals("789", class2.TreatmentCode);
			AssertEquals("789", class3.TreatmentCode);
			AssertEquals("789", class4.TreatmentCode);
			AssertEquals("789", class5.TreatmentCode);
			AssertEquals("789", pivot1.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot1.TreatmentCode);
			AssertEquals("555", pivot2.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("555", pivot2.TreatmentCode);
			AssertEquals(ZString.Empty, pivot3.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot3.TreatmentCode);
			AssertEquals("789", pivot4.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot4.TreatmentCode);
			AssertEquals("789", pivot5.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot5.TreatmentCode);
			AssertEquals("666", pivot6.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("666", pivot6.TreatmentCode);
			AssertEquals(ZString.Empty, pivot7.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot7.TreatmentCode);
		}

		public void TestChangeTreatmentCodesB()
		{
			changer.RemoveTreatmentCode = true;
			changer.ChangeLookups(query);
			AssertEquals(ZString.Empty, class1.TreatmentCode);
			AssertEquals(ZString.Empty, class2.TreatmentCode);
			AssertEquals(ZString.Empty, class3.TreatmentCode);
			AssertEquals(ZString.Empty, class4.TreatmentCode);
			AssertEquals(ZString.Empty, class5.TreatmentCode);
			AssertEquals(ZString.Empty, pivot1.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot1.TreatmentCode);
			AssertEquals("555", pivot2.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("555", pivot2.TreatmentCode);
			AssertEquals(ZString.Empty, pivot3.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot3.TreatmentCode);
			AssertEquals(ZString.Empty, pivot4.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot4.TreatmentCode);
			AssertEquals(ZString.Empty, pivot5.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot5.TreatmentCode);
			AssertEquals("666", pivot6.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("666", pivot6.TreatmentCode);
			AssertEquals(ZString.Empty, pivot7.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot7.TreatmentCode);
		}

		public void TestChangeTreatmentCodesC()
		{
			changer.TreatmentCode = "789";
			query.AddToFilter(Enterprise.ZArchitecture.Schema.CusClassificationSchema.CC_TariffNum, "1");
			changer.ChangeLookups(query);
			AssertEquals("789", class1.TreatmentCode);
			AssertEquals("789", class2.TreatmentCode);
			AssertEquals("789", class3.TreatmentCode);
			AssertEquals("123", class4.TreatmentCode);
			AssertEquals(ZString.Empty, class5.TreatmentCode);
			AssertEquals("789", pivot1.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot1.TreatmentCode);
			AssertEquals("555", pivot2.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("555", pivot2.TreatmentCode);
			AssertEquals(ZString.Empty, pivot3.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot3.TreatmentCode);
			AssertEquals("789", pivot4.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("789", pivot4.TreatmentCode);
			AssertEquals("123", pivot5.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot5.TreatmentCode);
			AssertEquals("666", pivot6.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("666", pivot6.TreatmentCode);
			AssertEquals(ZString.Empty, pivot7.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot7.TreatmentCode);
		}

		public void TestChangeTreatmentCodesD()
		{
			changer.RemoveTreatmentCode = true;
			query.AddToFilter(Enterprise.ZArchitecture.Schema.CusClassificationSchema.CC_TariffNum, "2");
			changer.ChangeLookups(query);
			AssertEquals("123", class1.TreatmentCode);
			AssertEquals("123", class2.TreatmentCode);
			AssertEquals("456", class3.TreatmentCode);
			AssertEquals(ZString.Empty, class4.TreatmentCode);
			AssertEquals(ZString.Empty, class5.TreatmentCode);
			AssertEquals("123", pivot1.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot1.TreatmentCode);
			AssertEquals("555", pivot2.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("555", pivot2.TreatmentCode);
			AssertEquals(ZString.Empty, pivot3.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot3.TreatmentCode);
			AssertEquals("456", pivot4.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("456", pivot4.TreatmentCode);
			AssertEquals(ZString.Empty, pivot5.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot5.TreatmentCode);
			AssertEquals("666", pivot6.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("666", pivot6.TreatmentCode);
			AssertEquals(ZString.Empty, pivot7.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals(ZString.Empty, pivot7.TreatmentCode);
		}

		public void TestChangeTreatmentCodesE()
		{
			changer.TreatmentCode = "888";
			query.AddToFilter(Enterprise.ZArchitecture.Schema.CusClassificationSchema.CC_TariffNum, "2");
			changer.ChangeLookups(query);
			AssertEquals("123", class1.TreatmentCode);
			AssertEquals("123", class2.TreatmentCode);
			AssertEquals("456", class3.TreatmentCode);
			AssertEquals("888", class4.TreatmentCode);
			AssertEquals("888", class5.TreatmentCode);
			AssertEquals("123", pivot1.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot1.TreatmentCode);
			AssertEquals("555", pivot2.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("555", pivot2.TreatmentCode);
			AssertEquals(ZString.Empty, pivot3.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("123", pivot3.TreatmentCode);
			AssertEquals("456", pivot4.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("456", pivot4.TreatmentCode);
			AssertEquals("888", pivot5.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("888", pivot5.TreatmentCode);
			AssertEquals("666", pivot6.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("666", pivot6.TreatmentCode);
			AssertEquals(ZString.Empty, pivot7.AddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("888", pivot7.TreatmentCode);
		}

		ZQuery query;
		BulkLookupChanger changer;
		Classification class1;
		Classification class2;
		Classification class3;
		Classification class4;
		Classification class5;
		CusClassPartPivot pivot1;
		AUOrgSupplierPart part1;
		CusClassPartPivot pivot2;
		AUOrgSupplierPart part2;
		CusClassPartPivot pivot3;
		AUOrgSupplierPart part3;
		CusClassPartPivot pivot4;
		AUOrgSupplierPart part4;
		CusClassPartPivot pivot5;
		AUOrgSupplierPart part5;
		CusClassPartPivot pivot6;
		AUOrgSupplierPart part6;
		CusClassPartPivot pivot7;
		AUOrgSupplierPart part7;
		protected override void SetUp()
		{
			base.SetUp();
			class1 = Factory.New<Classification>();
			class1.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			class1.CC_TariffNum = "1";
			class1.TreatmentCode = "123";
			class1.CC_LookupCode = "C1";
			class2 = Factory.New<Classification>();
			class2.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			class2.CC_TariffNum = "1";
			class2.TreatmentCode = "123";
			class2.CC_LookupCode = "C2";
			class3 = Factory.New<Classification>();
			class3.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			class3.CC_TariffNum = "1";
			class3.TreatmentCode = "456";
			class3.CC_LookupCode = "C3";
			class4 = Factory.New<Classification>();
			class4.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			class4.CC_TariffNum = "2";
			class4.TreatmentCode = "123";
			class4.CC_LookupCode = "C4";
			class5 = Factory.New<Classification>();
			class5.CC_ClassificationType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.IMP;
			class5.CC_TariffNum = "2";
			class5.CC_LookupCode = "C5";
			part1 = Factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "P1";
			pivot1 = Factory.New<CusClassPartPivot>();
			pivot1.CI_CC = class1.PK;
			pivot1.CI_OP = part1.PK;
			pivot1.CI_RN_NKCountry = class1.CC_RN_NKCountryCode;
			pivot1.AddInfo.ZA_TreatmentCode_Hidden = "123";
			part2 = Factory.New<AUOrgSupplierPart>();
			part2.OP_PartNum = "P2";
			pivot2 = Factory.New<CusClassPartPivot>();
			pivot2.CI_CC = class1.PK;
			pivot2.CI_OP = part2.PK;
			pivot2.CI_RN_NKCountry = class1.CC_RN_NKCountryCode;
			pivot2.AddInfo.ZA_TreatmentCode_Hidden = "555";
			part3 = Factory.New<AUOrgSupplierPart>();
			part3.OP_PartNum = "P3";
			pivot3 = Factory.New<CusClassPartPivot>();
			pivot3.CI_CC = class1.PK;
			pivot3.CI_OP = part3.PK;
			pivot3.CI_RN_NKCountry = class1.CC_RN_NKCountryCode;
			part4 = Factory.New<AUOrgSupplierPart>();
			part4.OP_PartNum = "P4";
			pivot4 = Factory.New<CusClassPartPivot>();
			pivot4.CI_CC = class3.PK;
			pivot4.CI_OP = part4.PK;
			pivot4.CI_RN_NKCountry = class1.CC_RN_NKCountryCode;
			pivot4.AddInfo.ZA_TreatmentCode_Hidden = "456";
			part5 = Factory.New<AUOrgSupplierPart>();
			part5.OP_PartNum = "P5";
			pivot5 = Factory.New<CusClassPartPivot>();
			pivot5.CI_CC = class4.PK;
			pivot5.CI_OP = part5.PK;
			pivot5.CI_RN_NKCountry = class1.CC_RN_NKCountryCode;
			pivot5.AddInfo.ZA_TreatmentCode_Hidden = "123";
			part6 = Factory.New<AUOrgSupplierPart>();
			part6.OP_PartNum = "P6";
			pivot6 = Factory.New<CusClassPartPivot>();
			pivot6.CI_CC = class5.PK;
			pivot6.CI_OP = part6.PK;
			pivot6.CI_RN_NKCountry = class1.CC_RN_NKCountryCode;
			pivot6.AddInfo.ZA_TreatmentCode_Hidden = "666";
			part7 = Factory.New<AUOrgSupplierPart>();
			part7.OP_PartNum = "P7";
			pivot7 = Factory.New<CusClassPartPivot>();
			pivot7.CI_CC = class5.PK;
			pivot7.CI_OP = part7.PK;
			pivot7.CI_RN_NKCountry = class1.CC_RN_NKCountryCode;
			Factory.Save();
			query = new ZQuery();
			changer = new BulkLookupChanger(Factory);
		}
	}
}
