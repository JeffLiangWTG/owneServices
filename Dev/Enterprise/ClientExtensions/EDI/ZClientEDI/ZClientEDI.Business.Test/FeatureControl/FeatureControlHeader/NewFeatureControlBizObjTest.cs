using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(NewFeatureControlBizObj))]
	public class NewFeatureControlBizObjTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var newBizO = new NewFeatureControlBizObj();
			return newBizO;
		}

		public void TestUniqueCode()
		{
			var control = Factory.New<FeatureControlHeader>();
			control.FCM_FeatureControlCode = "C02";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			control.FCM_GG_ReleaseGroup = group.PK;
			Factory.Save();

			var newBizO = new NewFeatureControlBizObjForTest();
			AssertEquals(true, newBizO.FeatureControlCodeList.ContainsCode("C01"));
			AssertEquals(false, newBizO.FeatureControlCodeList.ContainsCode("C02"));
			AssertEquals(true, newBizO.FeatureControlCodeList.ContainsCode("C03"));
		}

		public void TestValidation()
		{
			var newBizO = new NewFeatureControlBizObjForTest();
			newBizO.FeatureControlCode = "C01";
			AssertNoErrors(newBizO.FeatureControlCodeInfo);
			newBizO.FeatureControlCode = "";
			AssertHasError(newBizO.FeatureControlCodeInfo, "Please enter a Code.");
			newBizO.FeatureControlCode = "123";
			AssertHasError(newBizO.FeatureControlCodeInfo, "Enter a valid Code.");
			newBizO.FeatureControlCode = "C02";
			AssertNoErrors(newBizO.FeatureControlCodeInfo);
		}

		public void TestCodesFromRegistry()
		{
			var regValue = new CodeDescriptionPairList();
			regValue.AddPair("REG01", "REG01 - DESC.");
			regValue.AddPair("REG02", "REG02 - DESC.");
			regValue.AddPair("REG03", "REG03 - DESC.");
			EDIDataRegistry.Instance.FeatureControlCodeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			var newBizO = new NewFeatureControlBizObjForTest();
			AssertEquals(true, newBizO.FeatureControlCodeList.ContainsCode("REG01"));
			AssertEquals(true, newBizO.FeatureControlCodeList.ContainsCode("REG02"));
			AssertEquals(true, newBizO.FeatureControlCodeList.ContainsCode("REG03"));
			AssertEquals(false, newBizO.FeatureControlCodeList.ContainsCode("REG04"));
		}

		class NewFeatureControlBizObjForTest : NewFeatureControlBizObj
		{
			protected override CodeDescriptionPairList GetCodeList()
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("C01", "C01 - DESC.");
				list.AddPair("C02", "C02 - DESC.");
				list.AddPair("C03", "C03 - DESC.");
				list.AddRange(base.GetCodeList());
				return list;
			}
		}
	}
}
