using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalMovementHeader))]
	sealed class NctsArrivalMovementHeaderTest : EU.NCTS.Business.Testing.NctsArrivalMovementHeaderAbstractTest
	{
		public void TestGuaranteesForArrival()
		{
			var guarantee = arrivalMovement.GuaranteesForArrival.AddNew();
			AssertType<Guarantee>("Guarantee is DE type", guarantee);
		}

		public void TestAuthorizationOwner()
		{
			var cusAuthorizationHeader1 = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			cusAuthorizationHeader1.CPH_OH_PermitHolder = ZGuid.BrettsGuid;
			arrivalMovement.AuthorizationCode = "ACE";
			arrivalMovement.AuthorizationOwner = ZGuid.BrettsGuid;

			AssertEquals(ZGuid.BrettsGuid, arrivalMovement.GoodsLocation.AddressIdentificationHolderPK);
		}

		public void TestAuthorizationNumber()
		{
			arrivalMovement.AuthorizationNumber = "ZZ123456";

			AssertEquals("Authorization is retrieved correctly", "ZZ123456", arrivalMovement.GoodsLocation.AddressAuthorisationNumber);
		}

		public void TestCusGoodsLocation()
		{
			AssertType<CusGoodsLocation>(arrivalMovement.GoodsLocation);
		}

		public void TestGoodsItems()
		{
			AssertType<NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>>(arrivalMovement.GoodsItems);
		}

		public void TestSupportingDocuments()
		{
			AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(arrivalMovement.SupportingDocuments);
		}

		public void TestAdditionalDocuments()
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(arrivalMovement.AdditionalDocuments);
		}

		public void TestBM_UnloadingCompleted()
		{
			AssertEquals("ReadOnly", true, arrivalMovement.BM_UnloadingCompletedInfo.ReadOnly);
		}

		[TestDate(2023, 06, 02, 17, 07, 32)]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				arrivalMovement = Factory.New<NctsArrivalMovementHeader>();
				AssertEquals("BM_NoChangesToReport", true, arrivalMovement.BM_NoChangesToReport);
				AssertEquals("BM_StateOfSealsBoolean", true, arrivalMovement.BM_StateOfSealsBoolean);
				AssertEquals("BM_UnloadingCompleted", true, arrivalMovement.BM_UnloadingCompleted);
				AssertEquals("BM_ArrivalDate", ZDateTime.Empty, arrivalMovement.BM_ArrivalDate);
			});
		}

		[TestDate(2023, 06, 02, 17, 07, 32)]
		public void TestSetDefaultValuesAfterNctsHeaderIsSet()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsSimplifiedNctsProcedure", true, arrivalMovement.IsSimplifiedNctsProcedure);
				AssertEquals("BM_ArrivalDate", new ZDateTime(2023, 06, 02, 17, 07, 32), arrivalMovement.BM_ArrivalDate);
			});
		}

		public void TestSetDefaultValuesAfterNctsHeaderIsSet_TargetAuthorizationHeaderExist()
		{
			const string authorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_OH_PermitHolder = GlbBranch.CurrentBranch.OrgProxy.PK;
			cusAuthorisationHeader.CPH_Type = authorizationCode;
			cusAuthorisationHeader.CPH_Number = "0123456789";
			Factory.Save();

			CombineAssertions(() =>
			{
				var nctsHeader = CreatePhase5ArrivalHeader();
				var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
				AssertEquals("AuthorizationCode", authorizationCode, arrivalMovementHeader.AuthorizationCode);
				AssertEquals("AuthorizationOwner", GlbBranch.CurrentBranch.OrgProxy.PK, arrivalMovementHeader.AuthorizationOwner);
				AssertEquals("AuthorizationNumber", "0123456789", arrivalMovementHeader.AuthorizationNumber);
			});
		}

		public void TestSetDefaultValuesAfterNctsHeaderIsSet_TargetAuthorizationHeaderNotExist()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AuthorizationCode", ZString.Empty, arrivalMovement.AuthorizationCode);
				AssertEquals("AuthorizationOwner", ZGuid.Empty, arrivalMovement.AuthorizationOwner);
				AssertEquals("AuthorizationNumber", ZString.Empty, arrivalMovement.AuthorizationNumber);
			});
		}

		public void TestLookups()
		{
			AssertType<NctsArrivalMovementHeaderLookups>(arrivalMovement.Lookups);
		}

		public void TestArrivalDetailsReadOnly()
		{
			var header = CreatePhase5ArrivalHeader();
			var movementHeader = header.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
				AssertEquals("Customs Status different from 'UAP' or 'CL1', BM_CarnetTotalPages should be enabled", false, movementHeader.BM_CarnetTotalPagesInfo.ReadOnly);
				AssertEquals("Customs Status different from 'UAP' or 'CL1', BM_DischargeType should be enabled", false, movementHeader.BM_DischargeTypeInfo.ReadOnly);
				AssertEquals("Customs Status different from 'UAP' or 'CL1', Goods Location should be enabled", false, movementHeader.GoodsLocation.ReadOnly);

				header = CreatePhase5ArrivalHeader();
				movementHeader = header.ArrivalMovementHeader;
				movementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				AssertEquals("Customs Status 'UAP', BM_CarnetTotalPages should be readonly", true, movementHeader.BM_CarnetTotalPagesInfo.ReadOnly);
				AssertEquals("Customs Status 'UAP', BM_DischargeType should be readonly", true, movementHeader.BM_DischargeTypeInfo.ReadOnly);
				AssertEquals("Customs Status 'UAP', Goods Location should be readonly", true, newFactory.Load<NctsArrivalMovementHeader>(movementHeader.PK).GoodsLocation.ReadOnly);

				header = CreatePhase5ArrivalHeader();
				movementHeader = header.ArrivalMovementHeader;
				movementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				Factory.Save();
				newFactory = new BusinessObjectFactory();
				AssertEquals("Customs Status 'CL1', BM_CarnetTotalPages should be readonly", true, movementHeader.BM_CarnetTotalPagesInfo.ReadOnly);
				AssertEquals("Customs Status 'CL1', BM_DischargeType should be readonly", true, movementHeader.BM_DischargeTypeInfo.ReadOnly);
				AssertEquals("Customs Status 'CL1', Goods Location (description) should be readonly", true, newFactory.Load<NctsArrivalMovementHeader>(movementHeader.PK).GoodsLocation.ReadOnly);
			});
		}

		public void TestIsArrivalDetailsReadOnly()
		{
			var header = CreatePhase5ArrivalHeader();
			var movementHeader = header.ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				header.BH_HeaderType = "D";
				AssertEquals("When is Departure, the result should be false", false, movementHeader.IsArrivalDetailsReadOnly);
				header.BH_HeaderType = "A";

				movementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'UAP', the result should be true", true, movementHeader.IsArrivalDetailsReadOnly);
				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'CL1', the result should be true", true, movementHeader.IsArrivalDetailsReadOnly);

				movementHeader.BM_CustomsStatus = ZString.Empty;
				AssertEquals("When is Arrival and BM_CustomsStatus is empty (not UAP or CL1), the result should be false.", false, movementHeader.IsArrivalDetailsReadOnly);
			});
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, arrivalMovementHeader.AreUnloadingRemarksFullyAccepted);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("Accepted", true, arrivalMovementHeader.AreUnloadingRemarksFullyAccepted);
			});
		}

		public void TestBM_UnloadingDate_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_UnloadingDateInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_UnloadingDateInfo.ReadOnly);
			});
		}

		public void TestBM_NoChangesToReport_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_NoChangesToReportInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_NoChangesToReportInfo.ReadOnly);
			});
		}

		public void TestBM_StateOfSealsBoolean_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_StateOfSealsBooleanInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_StateOfSealsBooleanInfo.ReadOnly);
			});
		}

		public void TestBM_UnloadingCompleted_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Unloading Completed is always readonly", true, arrivalMovementHeader.BM_UnloadingCompletedInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("Unloading Completed is always readonly", true, arrivalMovementHeader.BM_UnloadingCompletedInfo.ReadOnly);
			});
		}

		public void TestBM_ArrivalDate_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_ArrivalDateInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_ArrivalDateInfo.ReadOnly);
			});
		}

		public void TestBM_UnloadingRemarks_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_UnloadingRemarksInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_UnloadingRemarksInfo.ReadOnly);
			});
		}

		public void TestOtherThingsToReport_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.OtherThingsToReportInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.OtherThingsToReportInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_TransportAtDepartureID_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_TransportAtDepartureIDInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_TransportAtDepartureIDInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureIDNationality_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer1RegNo_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer2RegNo_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_AircraftIDAtDeparture_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_AircraftIDAtDepartureInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_AircraftIDAtDepartureInfo.ReadOnly);
			});
		}

		public void TestUnloadedVesselNameAtDeparture_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedVesselNameAtDepartureInfo.ReadOnly);
				MakeUnloadingRemarksAreFullyAcceptedByCustoms(arrivalMovementHeader);
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedVesselNameAtDepartureInfo.ReadOnly);
			});
		}
		public void TestReadOnlyForSentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				arrivalMovementHeader.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
				AssertEquals("Status ACC", false, arrivalMovementHeader.IsUnloadingRemarksReadOnly);
				arrivalMovementHeader.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Status SNT", false, arrivalMovementHeader.IsUnloadingRemarksReadOnly);
			});
		}

		public void TestPreviousDocuments_ReadOnly()
		{
			var nctsBill = CreatePhase5ArrivalHeader().Bills.AddNew();
			var previousDocumentsBills = nctsBill.PreviousDocuments.AddNew();
			var arrivalGoodsItems = nctsBill.ArrivalGoodsItems.AddNew();
			var previousDocumentsGoodsItems = arrivalGoodsItems.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("PreviousDocuments in bills are readonly", true, previousDocumentsBills.ReadOnly);
				AssertEquals("PreviousDocuments in goods items are readonly", true, previousDocumentsGoodsItems.ReadOnly);
			});
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
				&& info.Name != "DestinationCustomsOfficeCodeForArrival")
			{
				base.TestBizObjectField(info);
			}
		}

		public void TestAddCusGoodsLocationAdditionalIdentifierDescriptionFetchHintsForViewCore()
		{
			var cgl1 = Factory.NewWithValidTestData<CusGoodsLocation>();
			cgl1.CGL_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
			cgl1.CGL_ParentID = arrivalMovement.PK;
			var of1 = arrivalMovement.CustomsOffices.AddNew();
			of1.CY_Code = "DSA";
			of1.CY_Data = "OFFIC1";

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(NctsMovementType.Codes.Arrival);
			var of2 = nctsHeader2.ArrivalMovementHeader.CustomsOffices.AddNew();
			of2.CY_Code = "DSA";
			of2.CY_Data = "OFFIC1";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var headerCollection = new NctsHeaderCollection(factory);
			headerCollection.Load();
			headerCollection.ForEach(x => x.ReadOnly = true);
			var fetchStrategy = headerCollection.FetchStrategy;
			fetchStrategy.FetchForView(headerCollection.ToArray(), [new TableColumn(string.Empty, nameof(CusGoodsLocation) + "+" + nameof(CusGoodsLocation.AdditionalIdentifierDescription))]);

			_ = headerCollection.Cast<NctsHeader>().Select(x => x.CusGoodsLocation.AdditionalIdentifierDescription).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Table CusCodeData should have 1 hit", 1, factory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				AssertEquals("Table CusAuthorizationUsage should have 1 hit", 1, factory.GetTableHitCount(CusAuthorizationUsageSchema.Constants.TableName));
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => arrivalMovement;

		protected override BusinessObject GetNewBusinessObject() => arrivalMovement;

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new LightValidationTesterExcludingJobDocAddress(bizObjToTest);
		}

		NctsHeader CreatePhase5ArrivalHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header;
		}

		static void MakeUnloadingRemarksAreFullyAcceptedByCustoms(NctsArrivalMovementHeader arrivalMovementHeader)
		{
			arrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			arrivalMovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = CreatePhase5ArrivalHeader();
			arrivalMovement = header.ArrivalMovementHeader;
		}
		NctsArrivalMovementHeader arrivalMovement;

		class LightValidationTesterExcludingJobDocAddress : LightValidationTester
		{
			public LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var objectType = info.BizObj.GetType();
				if (objectType == typeof(CusGoodsLocation) || objectType == typeof(CusGoodsLocationAddress))
				{
					return false;
				}
				else
				{
					return base.ShouldTestProperty(info);
				}
			}
		}
	}
}
