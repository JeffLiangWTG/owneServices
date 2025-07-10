using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageAdditionalInfo))]
	public class TemporaryStorageAdditionalInfoTest : EU.Business.CusTempStorage.Testing.TemporaryStorageAdditionalInfoAbstractTest<TemporaryStorageAdditionalInfo>
	{
		public void TestCSI_Code()
		{
			var addInfo = Factory.New<TemporaryStorageAdditionalInfo>();
			AssertEquals("Caption", "Code", addInfo.CSI_CodeInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestCSI_Description()
		{
			var addInfo = Factory.New<TemporaryStorageAdditionalInfo>();
			AssertEquals("Caption", "Description", addInfo.CSI_DescriptionInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestTemporaryStorageAdditionalInfoValidationType()
		{
			var addInfo = Factory.New<TemporaryStorageAdditionalInfo>();
			AssertType(typeof(TemporaryStorageAdditionalInfoValidation), addInfo.Validation);
		}

		protected override IEnumerable<TemporaryStorageAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			yield return (TemporaryStorageAdditionalInfo)addInfo;
		}

		protected override Type GetLookupsType => typeof(TemporaryStorageAdditionalInfoLookups);

		protected override (ZString Code, ZString DataGroupingCode, ZDateTime EffectiveDate) TransitionPeriodConfig => (Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today);
	}
}
