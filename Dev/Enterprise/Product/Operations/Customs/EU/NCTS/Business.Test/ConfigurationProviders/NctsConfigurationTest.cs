using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsConfiguration))]
	public abstract class NctsConfigurationAbstractTest<C, G> : TestCaseWithFactory
		where C : NctsConfiguration
		where G : GoodsItemsConfiguration
	{
		public abstract void TestMiscAdditionalInfosSupport();

		public abstract void TestMiscSupportingDocumentsSupport();

		public abstract void TestMiscPreviousDocumentsSupport();

		public abstract void TestMiscGuaranteesSupport();

		public abstract void TestUseUniversalFeeCalculation();

		public abstract void TestReceiveIE043UnloadingPermissionDetailsMessage();

		public abstract void TestDocDataPlugInSupport();

		public abstract void TestFullLoadPortSupport();

		public abstract void TestMiscTabPageSupport();

		public abstract void TestUseAdditionalDeclarationType();

		public abstract void TestUsePresentationDateTime();

		public virtual void TestUseCompanyOrgProxyFallBack()
		{
			AssertEquals(false, configuration.UseCompanyOrgProxyFallBack);
		}

		public virtual void TestAllowMixedCaseAuthorisationNumbers()
		{
			AssertEquals(false, configuration.AllowMixedCaseAuthorisationNumbers);
		}

		public virtual void TestUseDeclarantFallBack()
		{
			AssertEquals(false, configuration.UseDeclarantFallBack);
		}

		public virtual void TestUseBranchOrgProxyFallBack()
		{
			AssertEquals(false, configuration.UseBranchOrgProxyFallBack);
		}

		public virtual void TestUseGuaranteeGridValidation()
		{
			AssertEquals(false, configuration.UseGuaranteeGridValidation);
		}

		public virtual void TestClearExistingGuaranteesConfiguration()
		{
			AssertEquals(false, configuration.ClearExistingGuaranteesConfiguration);
		}

		public virtual void TestIsBondedWarehouseSupported()
		{
			AssertEquals(false, configuration.IsBondedWarehouseSupported);
		}

		public virtual void TestIsDepartureRetransmissionSupported()
		{
			AssertEquals(false, configuration.IsDepartureRetransmissionSupported);
		}

		public void TestGoodsItemsConfiguration()
		{
			AssertType<G>(configuration.GoodsItemsConfiguration);
		}

		public void TestGuaranteeConfiguration()
		{
			AssertType(GetGuaranteeConfigurationTypeForTest(), configuration.GuaranteeConfiguration);
		}

		public void TestMessageSendingConfiguration()
		{
			AssertType(GetMessageSendingConfigurationTypeForTest(), configuration.MessageSendingConfiguration);
		}

		public void TestValidationRuleConfiguration()
		{
			AssertType(GetExpectedValidationRuleConfigurationTypeForTest(), configuration.ValidationRuleConfiguration);
		}

		public void TestNctsEuOfficeCodeConfiguration()
		{
			AssertType(GetNctsEuOfficeCodeConfigurationTypeForTest(), configuration.NctsEuOfficeCodeConfiguration);
		}

		public void TestCountryOfRoutingConfiguration()
		{
			AssertType(GetCountryOfRoutingConfigurationTypeForTest(), configuration.CountryOfRoutingConfiguration);
		}

		public void TestCusSealConfiguration()
		{
			AssertType(GetCusSealConfigurationTypeForTest(), configuration.CusSealConfiguration);
		}

		public void TestCusSupplyChainActorReferenceConfiguration()
		{
			AssertType(GetCusSupplyChainActorReferenceConfigurationTypeForTest(), configuration.CusSupplyChainActorReferenceConfiguration);
		}

		public void TestCusTransportMeansConfiguration()
		{
			AssertType(GetCusTransportMeansConfigurationForTest(), configuration.CusTransportMeansConfiguration);
		}

		public void TestCusAuthorizationUsageConfiguration()
		{
			AssertType(GetCusAuthorizationUsageConfigurationTypeForTest(), configuration.CusAuthorizationUsageConfiguration);
		}

		public void TestGetNewInventorySelectionHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBill = nctsHeader.Bills.AddNew();
			AssertType(GetExpectedInventorySelectionHeaderTypeForTest(), configuration.GetNewInventorySelectionHeader(nctsBill));
		}

		public void TestGetNewOrderInventorySelectionHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBill = nctsHeader.Bills.AddNew();
			AssertType(GetExpectedOrderInventorySelectionHeaderTypeForTest(), configuration.GetNewOrderInventorySelectionHeader(nctsBill));
		}

		public void TestMovementHeaderConfiguration()
		{
			AssertType(GetMovementHeaderConfigurationForTest(), configuration.MovementHeaderConfiguration);
		}

		public void TestBillConfiguration()
		{
			AssertType(GetBillConfigurationTypeForTest(), configuration.BillConfiguration);
		}

		public void TestNctsPackageConfiguration()
		{
			AssertType(GetNctsPackageConfigurationTypeForTest(), configuration.NctsPackageConfiguration);
		}

		public void TestLocationOfGoodsFromAuthorisationDefaulterConfiguration()
		{
			AssertType(GetLocationOfGoodsFromAuthorisationDefaulterConfigurationTypeForTest(), configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration);
		}

		public void TestNctsContainerConfiguration()
		{
			AssertType(GetNctsContainerConfigurationTypeForTest(), configuration.NctsContainerConfiguration);
		}

		public void TestCommonPreviousDocumentConfiguration()
		{
			AssertType(GetCommonPreviousDocumentConfigurationTypeForTest(), configuration.CommonPreviousDocumentConfiguration);
		}

		protected virtual Type GetBillConfigurationTypeForTest() => typeof(BillConfiguration);

		protected virtual Type GetMovementHeaderConfigurationForTest() => typeof(MovementHeaderConfiguration);

		protected virtual Type GetNctsEuOfficeCodeConfigurationTypeForTest() => typeof(NctsEuOfficeCodeConfiguration);

		protected virtual Type GetCountryOfRoutingConfigurationTypeForTest() => typeof(CountryOfRoutingConfiguration);

		protected virtual Type GetMessageSendingConfigurationTypeForTest() => typeof(MessageSendingConfiguration);

		protected virtual Type GetExpectedValidationRuleConfigurationTypeForTest() => typeof(ValidationRuleConfiguration);

		protected virtual Type GetExpectedInventorySelectionHeaderTypeForTest() => typeof(NctsInventorySelectionHeader);

		protected virtual Type GetExpectedOrderInventorySelectionHeaderTypeForTest() => typeof(NctsOrderInventorySelectionHeader);

		protected virtual Type GetGuaranteeConfigurationTypeForTest() => typeof(GuaranteeConfiguration);

		protected virtual Type GetNctsPackageConfigurationTypeForTest() => typeof(NctsPackageConfiguration);

		protected virtual Type GetLocationOfGoodsFromAuthorisationDefaulterConfigurationTypeForTest() => typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration);

		protected virtual Type GetNctsContainerConfigurationTypeForTest() => typeof(NctsContainerConfiguration);

		protected virtual Type GetCommonPreviousDocumentConfigurationTypeForTest() => typeof(CommonPreviousDocumentConfiguration);

		protected virtual Type GetCusSealConfigurationTypeForTest() => typeof(CusSealConfiguration);

		protected virtual Type GetCusSupplyChainActorReferenceConfigurationTypeForTest() => typeof(CusSupplyChainActorReferenceConfiguration);

		protected virtual Type GetCusTransportMeansConfigurationForTest() => typeof(CusTransportMeansConfiguration);

		protected virtual Type GetCusAuthorizationUsageConfigurationTypeForTest() => typeof(CusAuthorizationUsageConfiguration);

		public void TestUseLocalReferenceNumberIgnoreInDatabaseCheck()
		{
			AssertEquals("UseLocalReferenceNumberIgnoreInDatabaseCheck", ExpectedUseLocalReferenceNumberIgnoreInDatabaseCheck, configuration.UseLocalReferenceNumberIgnoreInDatabaseCheck);
		}
		protected virtual ZBool ExpectedUseLocalReferenceNumberIgnoreInDatabaseCheck => false;

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (C)Activator.CreateInstance(typeof(C));
		}

		protected C configuration;
	}

	[TestedType(typeof(NctsConfiguration))]
	sealed class NctsConfigurationBaseOnlyTest : NctsConfigurationAbstractTest<NctsConfiguration, GoodsItemsConfiguration>
	{
		public void TestIsMultipleMovementsEnabled()
		{
			AssertEquals("Default", false, configuration.IsMultipleMovementsEnabled);
			using (NctsCustomsDataRegistry.Instance.EnableMultipleMovements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Cached", false, configuration.IsMultipleMovementsEnabled);
				configuration = Activator.CreateInstance<NctsConfiguration>();
				AssertEquals("When set", true, configuration.IsMultipleMovementsEnabled);
			}
			AssertEquals("Cached", true, configuration.IsMultipleMovementsEnabled);
		}

		public void TestIsImportBondedWarehouseOrderAvailable()
		{
			Assert("Default", configuration.IsImportBondedWarehouseOrderAvailable);
		}

		public void TestGetLineSupporter()
		{
			CombineAssertions(() =>
			{
				var configuration = NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.EuropeanUnion);
				AssertEquals("Country Code EU returns base", "Enterprise.Customs.EU.NCTS.Business", configuration.GetType().Namespace);
				configuration = NctsConfiguration.GetConfiguration(Factory, ZString.Empty);
				AssertEquals("Empty Country Code returns base", "Enterprise.Customs.EU.NCTS.Business", configuration.GetType().Namespace);
				configuration = NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Germany);
				AssertEquals("Implemented Country Code returns country class", "Enterprise.Customs.DE.NCTS.Business", configuration.GetType().Namespace);
				configuration = NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Spain);
				AssertEquals("Implemented Country Code returns country class", "Enterprise.Customs.ES.NCTS.Business", configuration.GetType().Namespace);
				configuration = NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Ireland);
				AssertEquals("Implemented Country Code returns country class", "Enterprise.Customs.IE.NCTS.Business", configuration.GetType().Namespace);
				configuration = NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Belgium);
				AssertEquals("Implemented Country Code returns country class", "Enterprise.Customs.BE.NCTS.Business", configuration.GetType().Namespace);
				configuration = NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Poland);
				AssertEquals("Implemented Country Code returns country class", "Enterprise.Customs.PL.NCTS.Business", configuration.GetType().Namespace);
				configuration = NctsConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.France);
				AssertEquals("Implemented Country Code returns country class", "Enterprise.Customs.FR.NCTS.Configurations", configuration.GetType().Namespace);
			});
		}

		public override void TestMiscAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.MiscAdditionalInfosSupport(header));
		}

		public override void TestMiscSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscSupportingDocumentsSupport(header));
		}

		public override void TestMiscPreviousDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscPreviousDocumentsSupport(header));
		}

		public override void TestMiscGuaranteesSupport()
		{
			AssertEquals(false, configuration.MiscGuaranteesSupport(header));
		}

		public override void TestUseUniversalFeeCalculation()
		{
			AssertEquals(false, configuration.UseUniversalFeeCalculation);
		}

		public override void TestUseAdditionalDeclarationType() => AssertEquals(false, configuration.UseAdditionalDeclarationType);

		public override void TestUsePresentationDateTime() => AssertEquals(false, configuration.UsePresentationDateTime);

		public override void TestReceiveIE043UnloadingPermissionDetailsMessage()
		{
			AssertEquals(true, configuration.ReceiveIE043UnloadingPermissionDetailsMessage);
		}

		public override void TestDocDataPlugInSupport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When nctsHeader is null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(nctsHeader: null));
				AssertEquals("When nctsHeader is not null, DocDataPlugInSupport", false, configuration.DocDataPlugInSupport(header));
			});
		}

		public override void TestFullLoadPortSupport()
		{
			AssertEquals(false, configuration.FullLoadPortSupport);
		}

		public override void TestMiscTabPageSupport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When nctsHeader is null, MiscTabPageSupport", false, configuration.MiscTabPageSupport(nctsHeader: null));
				AssertEquals("When nctsHeader is not null, MiscTabPageSupport", false, configuration.MiscTabPageSupport(header));
			});
		}

		public void TestGetDefaultMessageStatusForArrival()
		{
			CombineAssertions(() =>
			{
				var arrivalHeader = Factory.New<NctsHeader>();
				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertEquals("Phase4", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, configuration.GetDefaultMessageStatusForArrival(arrivalHeader));

				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Phase5", NctsMessageStatusList.Codes.Unknown, configuration.GetDefaultMessageStatusForArrival(arrivalHeader));
			});
		}

		public void TestValidationDecider_DeparturePhase5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsHeaderDeparturePhase5ValidationDecider>(configuration.GetValidationDecider(header));
		}

		public void TestValidationDecider_ArrivalPhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsHeaderArrivalPhase5ValidationDecider>(configuration.GetValidationDecider(header));
		}

		public void TestEnRouteIncidentConfiguration()
		{
			AssertType<EnRouteIncidentConfiguration>(configuration.EnRouteIncidentConfiguration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
	}
}
