using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeader))]
	sealed class TemporaryStorageHeaderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageHeaderAbstractTest<TemporaryStorageHeader>
	{
		public void TestGoodsLocationType()
		{
			var header = TemporaryStorageHeader.New(Factory);
			AssertType<Declaration.CusGoodsLocation>(header.GoodsLocation);
		}

		public void TestNewHeader()
		{
			var header1 = TemporaryStorageHeader.New(Factory);
			AssertEquals("Default AMA_ManifestType should be IST.", FRConstants.TemporaryStorage.AppCodeIST, header1.AMA_ManifestType);
			var header2 = TemporaryStorageHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeLAD);
			AssertEquals("AMA_ManifestType should be LADT.", FRConstants.TemporaryStorage.AppCodeLAD, header2.AMA_ManifestType);
		}

		public void TestDefaultValue()
		{
			var tempHeader = Factory.New<TemporaryStorageHeader>();
			AssertEquals("Default AMA_ManifestType should be IST.", FRConstants.TemporaryStorage.AppCodeIST, tempHeader.AMA_ManifestType);
		}

		public void TestCorrelationID()
		{
			var header1 = Factory.New<TemporaryStorageHeader>();
			var header2 = Factory.New<TemporaryStorageHeader>();
			Factory.Save();
			Assert(header1.CorrelationID != header2.CorrelationID);
			AssertType<ZString>(header1.CorrelationID);
		}

		public void TestCorrelationIDPrefix()
		{
			var header = TemporaryStorageHeader.New(Factory);
			AssertEquals(ZString.Empty, header.CorrelationIDPrefix);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new TemporaryStorageHeaderLightValidationTester(bizObjToTest);
		}

		sealed class TemporaryStorageHeaderLightValidationTester : LightValidationTester
		{
			public TemporaryStorageHeaderLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return base.ShouldTestProperty(info)
					&& propertyName != EU.Business.CusTempStorage.TemporaryStorageBill.Schema.ABL_RL_NKPortOfDischarge;
			}
		}

		public void TestValidationType()
		{
			AssertType<FRTemporaryStorageHeaderValidation>(Factory.New<TemporaryStorageHeader>().Validation);
		}

		public void TestConfigurationType()
		{
			AssertType<TemporaryStorageConfiguration>(Factory.New<TemporaryStorageHeader>().Configuration);
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<EU.Business.CusTempStorage.TemporaryStorageContainer, EU.Business.CusTempStorage.TemporaryStorageHeader>);

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaManifestHeader.Schema.AMA_OA_Declarant };
		}

		public void TestAuthorizationType_NoValueSetWithDefaultAuthorizationPropertiesApplied()
		{
			var header = TemporaryStorageHeader.New(Factory);
			header.GoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;

			AssertNullOrEmpty(header.AuthorizationType);
		}

		public void TestBills()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertType<EU.Business.CusTempStorage.TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>>(header.Bills);
		}

		public void TestMasterBill()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertType<TemporaryStorageBill>(header.MasterBill);
		}

		public void TestGetBillType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals(typeof(TemporaryStorageBill), header.GetBillType());
		}
	}
}
