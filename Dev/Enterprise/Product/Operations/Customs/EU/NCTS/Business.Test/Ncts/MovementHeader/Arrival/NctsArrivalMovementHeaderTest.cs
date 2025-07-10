using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsArrivalMovementHeader))]
	public abstract class NctsArrivalMovementHeaderAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOverrideAdditionalInfoTypeForOptimisation()
		{
			var arrivalMovementHeader = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var arrivalMovementHeaderType = arrivalMovementHeader.GetType();
			if (arrivalMovementHeaderType != typeof(NctsArrivalMovementHeader))
			{
				var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)arrivalMovementHeader).GetCusSupportingInfoTypes();
				if (cusSupportingInfoTypes.TryGetValue(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, out var additionalInfoType)
					&& additionalInfoType != typeof(NctsAdditionalInfo)
					&& arrivalMovementHeaderType.GetProperty("AdditionalInfoType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
				{
					Fail($"Override {arrivalMovementHeaderType.FullName}.AdditionalInfoType to return {additionalInfoType.FullName} for better performance instead of relying on NctsTypeDecider");
				}
			}
			Assert(true);
		}

		public void TestOverrideSupportingDocumentTypeForOptimisation()
		{
			var arrivalMovementHeader = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var arrivalMovementHeaderType = arrivalMovementHeader.GetType();
			if (arrivalMovementHeaderType != typeof(NctsArrivalMovementHeader))
			{
				var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)arrivalMovementHeader).GetCusSupportingInfoTypes();
				if (cusSupportingInfoTypes.TryGetValue(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, out var supportingDocumentType))
				{
					if(supportingDocumentType != typeof(NctsSupportingDocument)
					&& arrivalMovementHeaderType.GetProperty("SupportingDocumentType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
					{
						Fail($"Override {arrivalMovementHeaderType.FullName}.SupportingDocumentType to return {supportingDocumentType.FullName} for better performance instead of relying on NctsTypeDecider");
					}

					if (supportingDocumentType !=
						arrivalMovementHeader.SupportingDocuments.GetType().GenericTypeArguments[0])
					{
						Fail($"Type mismatch between collection type argument for {arrivalMovementHeaderType.FullName}.SupportingDocuments and {arrivalMovementHeaderType.FullName}.SupportingDocumentType.");
					}
				}
			}
			Assert(true);
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("BaseCusInBondHeader, GlbCompany and GlbBranch should be loaded.", true);
		}

		public void TestYesNoListsAreTranslatable()
		{
			var arrivalMovementHeader = (NctsArrivalMovementHeader)GetNewBusinessObject();
			NCTSTestHelper.AssertYesNoListsAreTranslatable(arrivalMovementHeader);
		}
	}

	[TestedType(typeof(NctsArrivalMovementHeader))]
	sealed class NctsArrivalMovementHeaderTest : NctsArrivalMovementHeaderAbstractTest
	{
		public void TestBM_NoChangesToReport_ResetUnloadedWeightToDeclared()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			arrivalMovementHeader.BM_NoChangesToReport = false;

			arrivalMovementHeader.BM_GrossWeightUnloaded = 1000m;
			arrivalMovementHeader.BM_GrossWeight = 500m;

			arrivalMovementHeader.BM_NoChangesToReport = true;

			AssertEquals("BM_GrossWeightUnloaded", 0m, arrivalMovementHeader.BM_GrossWeightUnloaded);

			arrivalMovementHeader.BM_NoChangesToReport = false;

			AssertEquals("BM_GrossWeightUnloaded", 500m, arrivalMovementHeader.BM_GrossWeightUnloaded);
		}

		public void TestAuthorizationCodeAttribute()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			var authorizationCodeInfo = arrivalMovementHeader.AuthorizationCodeInfo;

			NCTSTestHelper.AssertCaptions(authorizationCodeInfo, "Authorization Code", "Code", ZString.Empty);
			AssertEquals(nameof(NctsHeader.Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.AuthorizationCodeList), authorizationCodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AuthorizationCode MaxLength", CusAuthorizationUsage.Schema.AGC_CodeMaxLength, authorizationCodeInfo.MaxLength);
		}

		public void TestAuthorizationNumberAttribute()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			var authorizationNumberInfo = arrivalMovementHeader.AuthorizationNumberInfo;

			NCTSTestHelper.AssertCaptions(authorizationNumberInfo, "Authorization Number", "Authorization No.", ZString.Empty);
			AssertEquals(nameof(NctsHeader.Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.AuthorizationNumberList), authorizationNumberInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AuthorizationNumber MaxLength", CusAuthorizationUsage.Schema.AGC_NumberMaxLength, authorizationNumberInfo.MaxLength);
		}

		public void TestAuthorizationOwnerAttribute()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			var authorizationOwnerInfo = arrivalMovementHeader.AuthorizationOwnerInfo;

			NCTSTestHelper.AssertCaptions(arrivalMovementHeader.AuthorizationOwnerInfo, "Authorization Owner", "Owner", "Own.");
			AssertEquals(nameof(NctsHeader.Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.Organisations), authorizationOwnerInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestAuthorizationLocationAttribute()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			var authorizationLocationInfo = arrivalMovementHeader.AuthorizationLocationInfo;

			AssertEquals("AuthorizationLocation MaxLength", CusAuthorizationUsage.Schema.AGC_LocationMaxLength, authorizationLocationInfo.MaxLength);
		}

		public void TestAuthorizationPropertiesGetter()
		{
			var header = CreatePhase5ArrivalHeader();
			var arrivalMovementHeader = header.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				var authorization = Factory.New<CusAuthorizationUsage>();
				authorization.AGC_ParentID = header.PK;
				authorization.AGC_ParentTableCode = header.TablePrefix;
				authorization.AGC_Number = "testnumber";
				authorization.AGC_Code = "code";
				authorization.AGC_OH_Owner = ZGuid.BrettsGuid;
				authorization.AGC_Location = "loc";
				AssertEquals("Has authorization, AuthorizationCode", "code", arrivalMovementHeader.AuthorizationCode);
				AssertEquals("Has authorization, AuthorizationNumber", "testnumber", arrivalMovementHeader.AuthorizationNumber);
				AssertEquals("Has authorization, AuthorizationOwner", ZGuid.BrettsGuid, arrivalMovementHeader.AuthorizationOwner);
				AssertEquals("Has authorization, AuthorizationLocation", "loc", arrivalMovementHeader.AuthorizationLocation);

				authorization.Delete();
				AssertEquals("No authorization, AuthorizationCode", ZString.Empty, arrivalMovementHeader.AuthorizationCode);
				AssertEquals("No authorization, AuthorizationNumber", ZString.Empty, arrivalMovementHeader.AuthorizationNumber);
				AssertEquals("No authorization, AuthorizationOwner", ZGuid.Empty, arrivalMovementHeader.AuthorizationOwner);
				AssertEquals("No authorization, AuthorizationLocation", ZString.Empty, arrivalMovementHeader.AuthorizationLocation);
			});
		}

		public void TestAuthorizationPropertiesSetter()
		{
			var header = CreatePhase5ArrivalHeader();
			var arrivalMovementHeader = header.ArrivalMovementHeader;

			var existingAuthorization = Factory.New<CusAuthorizationUsage>();
			existingAuthorization.AGC_ParentID = header.PK;
			existingAuthorization.AGC_ParentTableCode = header.TablePrefix;

			CombineAssertions(() =>
			{
				arrivalMovementHeader.AuthorizationNumber = "testnumber";
				arrivalMovementHeader.AuthorizationCode = "code";
				arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
				arrivalMovementHeader.AuthorizationLocation = "loc";

				var loadedAuthorization = RetrieveLinkedCusAuthorizationUsage(header);
				AssertEquals("Update existing authorization", existingAuthorization.PK, loadedAuthorization.PK);
				AssertEquals("Existing authorization, AuthorizationCode", "code", loadedAuthorization.AGC_Code);
				AssertEquals("Existing authorization, AuthorizationNumber", "testnumber", loadedAuthorization.AGC_Number);
				AssertEquals("Existing authorization, AuthorizationOwner", ZGuid.BrettsGuid, loadedAuthorization.AGC_OH_Owner);
				AssertEquals("Existing authorization, AuthorizationLocation", "loc", loadedAuthorization.AGC_Location);

				existingAuthorization.Delete();
				arrivalMovementHeader.AuthorizationNumber = "testnumber";
				arrivalMovementHeader.AuthorizationCode = "code";
				arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
				arrivalMovementHeader.AuthorizationLocation = "loc";
				var createdAuthorization = RetrieveLinkedCusAuthorizationUsage(header);
				AssertNotEquals("Authorization is created when needed", existingAuthorization.PK, createdAuthorization.PK);
				AssertEquals("New authorization, AuthorizationCode", "code", createdAuthorization.AGC_Code);
				AssertEquals("New authorization, AuthorizationNumber", "testnumber", createdAuthorization.AGC_Number);
				AssertEquals("New authorization, AuthorizationOwner", ZGuid.BrettsGuid, createdAuthorization.AGC_OH_Owner);
				AssertEquals("New authorization, AuthorizationLocation", "loc", createdAuthorization.AGC_Location);
			});
		}

		CusAuthorizationUsage RetrieveLinkedCusAuthorizationUsage(NctsHeader header)
		{
			var authorizationQuery = new ZQuery(new ZQuery(CusAuthorizationUsageSchema.AGC_ParentID, header.PK));
			authorizationQuery.AddToFilter(new ZQuery(CusAuthorizationUsageSchema.AGC_ParentTableCode, header.TablePrefix));
			return Factory.Load<CusAuthorizationUsage>(authorizationQuery).SingleOrDefault();
		}

		public void TestBM_GONumberChangeOnAuthorizationCompletion()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				AssertEquals("BM_GONumber should still be empty", string.Empty, arrivalMovementHeader.BM_GONumber);
				arrivalMovementHeader.AuthorizationCode = "ACE";
				AssertEquals("BM_GONumber should still be empty, only code filled in", string.Empty, arrivalMovementHeader.BM_GONumber);
				arrivalMovementHeader.AuthorizationNumber = "TEST";
				AssertEquals("BM_GONumber should still be empty, only code & number filled in", string.Empty, arrivalMovementHeader.BM_GONumber);
				arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
				AssertEquals("BM_GONumber should be A3, all fields filled in", NctsControlResult.Codes.AuthorizedTrader, arrivalMovementHeader.BM_GONumber);
				arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
				AssertEquals("BM_GONumber should be blank again, not all fields filled in", string.Empty, arrivalMovementHeader.BM_GONumber);
			});
		}

		public void TestAuthorizationDeletedWhenEmpty()
		{
			var header = CreatePhase5ArrivalHeader();
			var arrivalMovementHeader = header.ArrivalMovementHeader;

			arrivalMovementHeader.AuthorizationNumber = "testnumber";
			arrivalMovementHeader.AuthorizationCode = "code";
			arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
			arrivalMovementHeader.AuthorizationLocation = "loc";

			CombineAssertions(() =>
			{
				arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
				AssertNotNull("Authorization not deleted when only AuthorizationNumber is empty", RetrieveLinkedCusAuthorizationUsage(header));
				arrivalMovementHeader.AuthorizationNumber = "testnumber";
				arrivalMovementHeader.AuthorizationCode = ZString.Empty;
				AssertNotNull("Authorization not deleted when only AuthorizationCode is empty", RetrieveLinkedCusAuthorizationUsage(header));
				arrivalMovementHeader.AuthorizationCode = "code";
				arrivalMovementHeader.AuthorizationOwner = ZGuid.Empty;
				AssertNotNull("Authorization not deleted when only AuthorizationOwner is empty", RetrieveLinkedCusAuthorizationUsage(header));
				arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
				arrivalMovementHeader.AuthorizationLocation = ZString.Empty;
				AssertNotNull("Authorization not deleted when only AuthorizationLocation is empty", RetrieveLinkedCusAuthorizationUsage(header));

				arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
				arrivalMovementHeader.AuthorizationCode = ZString.Empty;
				arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
				arrivalMovementHeader.AuthorizationLocation = "loc";
				AssertNotNull("Authorization not deleted when only AuthorizationNumber and AuthorizationCode are empty", RetrieveLinkedCusAuthorizationUsage(header));
				arrivalMovementHeader.AuthorizationCode = "code";
				arrivalMovementHeader.AuthorizationOwner = ZGuid.Empty;
				AssertNotNull("Authorization not deleted when only AuthorizationNumber and AuthorizationOwner are empty", RetrieveLinkedCusAuthorizationUsage(header));
				arrivalMovementHeader.AuthorizationOwner = ZGuid.BrettsGuid;
				arrivalMovementHeader.AuthorizationLocation = ZString.Empty;
				AssertNotNull("Authorization not deleted when only AuthorizationNumber and AuthorizationLocation are empty", RetrieveLinkedCusAuthorizationUsage(header));

				arrivalMovementHeader.AuthorizationNumber = "testnumber";
				arrivalMovementHeader.AuthorizationCode = ZString.Empty;
				arrivalMovementHeader.AuthorizationOwner = ZGuid.Empty;
				arrivalMovementHeader.AuthorizationLocation = "loc";
				AssertNotNull("Authorization not deleted when only AuthorizationCode and AuthorizationOwner are empty", RetrieveLinkedCusAuthorizationUsage(header));
				arrivalMovementHeader.AuthorizationNumber = "testnumber";
				arrivalMovementHeader.AuthorizationLocation = ZString.Empty;
				AssertNotNull("Authorization not deleted when only AuthorizationCode and AuthorizationLocation are empty", RetrieveLinkedCusAuthorizationUsage(header));

				arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
				AssertNull("Authorization deleted when AuthorizationNumber, AuthorizationCode, AuthorizationOwner and AuthorizationLocation are empty", RetrieveLinkedCusAuthorizationUsage(header));
			});
		}

		public void TestDestinationTraderChangedOnAuthorizationOwnerChanged()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgHeader2 = Factory.New<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var header = CreatePhase5ArrivalHeader();
			var arrivalMovementHeader = header.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				header.DestinationTrader.OrganisationPK = orgHeader2.PK;
				AssertEquals("When DestinationTrader.OrganisationPK is set and no code or number are set, owner is not set", true, arrivalMovementHeader.AuthorizationOwner.IsEmpty);
				arrivalMovementHeader.AuthorizationCode = "abc";
				AssertEquals("When only code is set and flag is true (EU), owner is defaulted to DestinationTrader.OrganisationPK", orgHeader2.PK, arrivalMovementHeader.AuthorizationOwner);
				AssertEquals("When only code is set, DestinationTrader.OrganisationPK is not changed", orgHeader2.PK, header.DestinationTrader.OrganisationPK);

				header.DestinationTrader.OrganisationPK = ZGuid.Empty;
				AssertEquals("When DestinationTrader.OrganisationPK is set to empty, owner is set to empty", true, arrivalMovementHeader.AuthorizationOwner.IsEmpty);
				AssertEquals("When DestinationTrader.OrganisationPK", true, header.DestinationTrader.OrganisationPK.IsEmpty);

				arrivalMovementHeader.AuthorizationNumber = "123";
				AssertEquals("When number is set after code, owner is defaulted", orgHeader.PK, arrivalMovementHeader.AuthorizationOwner);
				AssertEquals("When number is set after code and flag is true (EU), DestinationTrader.OrganisationPK is defaulted", orgHeader.PK, header.DestinationTrader.OrganisationPK);

				arrivalMovementHeader.AuthorizationOwner = orgHeader2.PK;
				AssertEquals("When owner is changed and not empty and flag is true (EU), DestinationTrader.OrganisationPK is changed", orgHeader2.PK, header.DestinationTrader.OrganisationPK);

				AssertEquals("Before removing code and number thre is one authorization in the header", 1, header.CusAuthorizationUsages.Count);

				arrivalMovementHeader.AuthorizationCode = ZString.Empty;
				arrivalMovementHeader.AuthorizationNumber = ZString.Empty;
				AssertEquals("When code and number are set to empty and flag is true (EU), the whole authorization is removed", 0, header.CusAuthorizationUsages.Count);
				AssertNull("Authorization deleted when AuthorizationNumber, AuthorizationCode and AuthorizationLocation are empty", RetrieveLinkedCusAuthorizationUsage(header));
				AssertEquals("When owner is changed and empty and flag is true (EU) (authorization removed), DestinationTrader.OrganisationPK is NOT changed", orgHeader2.PK, header.DestinationTrader.OrganisationPK);
			});
		}

		public void TestShouldSyncDestinationTraderWithAuthorization()
		{
			AssertEquals("Flag is true for EU", true, CreatePhase5ArrivalHeader().ArrivalMovementHeader.ShouldSyncDestinationTraderWithAuthorization);
		}

		public void TestSupportingDocuments()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var supportingDocumentCollection = arrivalMovement.SupportingDocuments;
			var supportingDocument = supportingDocumentCollection.AddNew();

			CombineAssertions(() =>
			{
				AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>("Type", supportingDocumentCollection);
				AssertEquals("Collection IsRegisteredEditableChildObject", true, arrivalMovement.IsRegisteredEditableChildObject(supportingDocumentCollection));
				AssertEquals("Collection IsLoaded", true, supportingDocumentCollection.IsLoaded);
				AssertEquals("Collection ReadOnly", true, supportingDocumentCollection.ReadOnly);
				AssertEquals("Document CSI_Type", Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, supportingDocument.CSI_Type);
				AssertEquals("Document Parent", arrivalMovement, supportingDocument.Parent);
			});
		}

		public void TestAdditionalDocuments()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var additionalDocumentCollection = arrivalMovement.AdditionalDocuments;
			var additionalDocument = additionalDocumentCollection.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Collection IsRegisteredEditableChildObject", true, arrivalMovement.IsRegisteredEditableChildObject(additionalDocumentCollection));
				AssertEquals("Collection IsLoaded", true, additionalDocumentCollection.IsLoaded);
				AssertEquals("Collection ReadOnly", true, additionalDocumentCollection.ReadOnly);
				AssertEquals("Document CSI_Type", Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, additionalDocument.CSI_Type);
				AssertEquals("Document Parent", arrivalMovement, additionalDocument.Parent);
			});
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).AreUnloadingRemarksFullyAccepted, arrivalMovementHeader);
		}

		public void TestBM_UnloadingDate_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).BM_UnloadingDateInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestBM_NoChangesToReport_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).BM_NoChangesToReportInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestBM_StateOfSealsBoolean_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).BM_StateOfSealsBooleanInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestBM_StateOfSealsBoolean_EnableOrDisableContainerSealsEdit()
		{
			CombineAssertions(() =>
			{
				var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
				var container = arrivalMovementHeader.Header.ArrivalHeaderContainers.AddNew();

				arrivalMovementHeader.BM_StateOfSealsBoolean = false;
				AssertEquals("BM_StateOfSeals == 'N' -> Seals readonly", false, container.Seals.ReadOnly);

				arrivalMovementHeader.BM_StateOfSealsBoolean = true;
				AssertEquals("BM_StateOfSeals != 'N' -> Seals readonly", true, container.Seals.ReadOnly);
			});
		}

		public void TestBM_NoChangesToReportBoolean_EnableOrDisableGrids()
		{
			CombineAssertions(() =>
			{
				var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
				var transport = arrivalMovementHeader.ArrivalTransportInfos.AddNew();
				var container = arrivalMovementHeader.Header.ArrivalHeaderContainers.AddNew();
				var additionalDocuments = arrivalMovementHeader.AdditionalDocuments.AddNew();
				var supportingDocuments = arrivalMovementHeader.SupportingDocuments.AddNew();

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					arrivalMovementHeader.BM_NoChangesToReport = false;
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals == 'N' -> Transport readonly", false, transport.ReadOnly);
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals == 'N' -> Container readonly", false, container.ReadOnly);
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals == 'N' -> AdditionalDocuments readonly", true, additionalDocuments.ReadOnly);
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals == 'N' -> SupportingDocuments readonly", true, supportingDocuments.ReadOnly);

					arrivalMovementHeader.BM_NoChangesToReport = true;
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals != 'N' -> Transport readonly", false, transport.ReadOnly);
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals != 'N' -> Container readonly", false, container.ReadOnly);
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals != 'N' -> AdditionalDocuments readonly", true, additionalDocuments.ReadOnly);
					AssertEquals("NCTSTransitionPeriod = true and BM_StateOfSeals != 'N' -> SupportingDocuments readonly", true, supportingDocuments.ReadOnly);
				}
			});
		}

		public void TestBM_UnloadingCompleted_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).BM_UnloadingCompletedInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestBM_ArrivalDate_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_ArrivalDateInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_ArrivalDateInfo.ReadOnly);
			});
		}

		public void TestBM_UnloadingRemarks_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).BM_UnloadingRemarksInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestOtherThingsToReport_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).OtherThingsToReportInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedB9_TransportAtDepartureID_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedB9_TransportAtDepartureIDInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureIDNationality_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer1RegNo_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedB9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer2RegNo_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedB9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedB9_AircraftIDAtDeparture_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedB9_AircraftIDAtDepartureInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadedVesselNameAtDeparture_ReadOnly()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsArrivalMovementHeader)x).UnloadedVesselNameAtDepartureInfo.ReadOnly, arrivalMovementHeader);
		}

		public void TestUnloadingRemarksAreSentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Not Accepted", false, arrivalMovementHeader.IsUnloadingRemarksReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Accepted", true, arrivalMovementHeader.IsUnloadingRemarksReadOnly);
			});
		}

		public void TestBM_UnloadingDate_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_UnloadingDateInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_UnloadingDateInfo.ReadOnly);
			});
		}

		public void TestBM_NoChangesToReport_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_NoChangesToReportInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_NoChangesToReportInfo.ReadOnly);
			});
		}

		public void TestBM_StateOfSealsBoolean_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_StateOfSealsBooleanInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_StateOfSealsBooleanInfo.ReadOnly);
			});
		}

		public void TestBM_UnloadingCompleted_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_UnloadingCompletedInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_UnloadingCompletedInfo.ReadOnly);
			});
		}

		public void TestBM_UnloadingRemarks_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.BM_UnloadingRemarksInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.BM_UnloadingRemarksInfo.ReadOnly);
			});
		}

		public void TestOtherThingsToReport_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.OtherThingsToReportInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.OtherThingsToReportInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_TransportAtDepartureID_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_TransportAtDepartureIDInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_TransportAtDepartureIDInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureIDNationality_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer1RegNo_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer1RegNoInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer2RegNo_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_TransportAtDepartureTrailer2RegNoInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo.ReadOnly);
			});
		}

		public void TestUnloadedB9_AircraftIDAtDeparture_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedB9_AircraftIDAtDepartureInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedB9_AircraftIDAtDepartureInfo.ReadOnly);
			});
		}

		public void TestUnloadedVesselNameAtDeparture_ReadOnly_SentToCustoms()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, arrivalMovementHeader.UnloadedVesselNameAtDepartureInfo.ReadOnly);
				arrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, arrivalMovementHeader.UnloadedVesselNameAtDepartureInfo.ReadOnly);
			});
		}

		public void TestMovementDetails()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<CusInBondMoveDetailCollection>(header.ArrivalMovementHeader.MovementDetails);
		}

		public void TestMovementDetailType_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull(header.ArrivalMovementHeader.MovementDetailType);
		}

		public void TestMovementDetailType_Phase5()
		{
			var header = CreatePhase5ArrivalHeader();
			AssertEquals(typeof(CusInBondMoveDetail), header.ArrivalMovementHeader.MovementDetailType);
		}

		public void TestSynchronizeArrivalAndUnloadingGoodsItems()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Arrival);

				header.ArrivalMovementHeader.AutoPopulatedArrivalGoodsItems = false;

				var goodsItems = header.ArrivalMovementHeader.GoodsItems;
				var goodsItem = goodsItems.AddNew();
				goodsItem.BY_Description = "Test1";

				var container = goodsItem.Containers.AddNew();
				container.BC_ContainerNum = "Container1";

				var package = goodsItem.Packages.AddNew();
				package.B5_MarksAndNumbers = "Package1";

				var suppDoc = goodsItem.SupportingDocuments.AddNew();
				suppDoc.CSI_Description = "SuppDoc1";

				var goodsItem2 = goodsItems.AddNew();
				goodsItem2.BY_HarmonisedTariff = "Test2";

				AssertEquals("Prereq: 2 arrival goods items", 2, goodsItems.Count);
				AssertEquals("Prereq: 0 unloading goods items", 0, header.UnloadingMovementHeader.GoodsItems.Count);

				header.ArrivalMovementHeader.SynchronizeArrivalAndUnloadingGoodsItems();

				var unloadingGoodsItems = header.UnloadingMovementHeader.GoodsItems;

				if (!header.Configuration.ReceiveIE043UnloadingPermissionDetailsMessage)
				{
					AssertEquals("Expected 2 copied unloading goods items", 2, unloadingGoodsItems.Count);
					var unloadingGoodsItem1 = unloadingGoodsItems[0];
					AssertEquals("IsNew is false for unloadingGoods", false, unloadingGoodsItem1.IsNew);
					AssertEquals("1 Container with same ContainerNum", "Container1", unloadingGoodsItem1.Containers[0].BC_ContainerNum);
					AssertEquals("1 Package with same Marks", "Package1", unloadingGoodsItem1.Packages[0].B5_MarksAndNumbers);
					AssertEquals("1 SuppDoc with same Description", "SuppDoc1", unloadingGoodsItem1.SupportingDocuments[0].CSI_Description);

					AssertEquals("IsNew is false for unloadingGoods", false, unloadingGoodsItems[1].IsNew);
				}
				else
				{
					AssertEquals("Expected 0 copied unloading goods items", 0, unloadingGoodsItems.Count);
				}

				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

				header.ArrivalMovementHeader.AutoPopulatedArrivalGoodsItems = true;

				goodsItems = header.ArrivalMovementHeader.GoodsItems;
				goodsItem = goodsItems.AddNew();
				goodsItem.BY_Description = "Test1";

				AssertEquals("Prereq: 1 arrival goods items", 1, goodsItems.Count);
				AssertEquals("Prereq: 0 unloading goods items", 0, header.UnloadingMovementHeader.GoodsItems.Count);

				header.ArrivalMovementHeader.SynchronizeArrivalAndUnloadingGoodsItems();

				unloadingGoodsItems = header.UnloadingMovementHeader.GoodsItems;

				AssertEquals("Expected 0 copied unloading goods items", 0, unloadingGoodsItems.Count);
			});
		}

		public void TestPopulateArrivalGoodsItemsFromDeparture()
		{
			var headerDeparture = Factory.New<NctsHeader>();
			headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			var departureGoodsItems = headerDeparture.MovementHeader.GoodsItems;
			var departureGoodsItem = departureGoodsItems.AddNew();
			departureGoodsItem.BY_Description = "Test1";

			var departureGoodsItem2 = departureGoodsItems.AddNew();
			departureGoodsItem2.BY_HarmonisedTariff = "Test2";

			var headerArrival = Factory.New<NctsHeader>();
			headerArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalGoodsItem = headerArrival.ArrivalMovementHeader.GoodsItems.AddNew();
			arrivalGoodsItem.BY_Description = "Test3";

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: 2 departure goods items", 2, departureGoodsItems.Count);
				AssertEquals("Prereq: 1 arrival goods items", 1, headerArrival.ArrivalMovementHeader.GoodsItems.Count);

				headerArrival.ArrivalMovementHeader.PopulateArrivalGoodsItemsFromDeparture(headerDeparture.MovementHeader);
				var arrivalGoodsItems = headerArrival.ArrivalMovementHeader.GoodsItems;
				AssertEquals("expected 2 arrival goods items", 2, arrivalGoodsItems.Count);
				AssertEquals("expected same description for the first goodsItem", "Test1", arrivalGoodsItems[0].BY_Description);
				AssertEquals("expected same description for the second goodsItem", "Test2", arrivalGoodsItems[1].BY_Description);

				var unloadingGoodsItems = headerArrival.UnloadingMovementHeader.GoodsItems;
				AssertEquals("expected 2 unloading goods items", 2, unloadingGoodsItems.Count);
				AssertEquals("expected isNew set to false for the first goodsItem", false, arrivalGoodsItems[0].IsNew);
				AssertEquals("expected isNew set to false for the second goodsItem", false, arrivalGoodsItems[1].IsNew);
			});
		}

		public void TestPopulateArrivalGoodsItemsFromDepartureCopyContainersPackagesAndDocuments()
		{
			var headerDeparture = Factory.New<NctsHeader>();
			headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			var depatureGoodsItems = headerDeparture.MovementHeader.GoodsItems;
			var departureGoodsItem = depatureGoodsItems.AddNew();
			departureGoodsItem.ContainersPivots.AddNew().Container = Factory.New<NctsDepartureHeaderContainer>();
			departureGoodsItem.ContainersPivots.AddNew().Container = Factory.New<NctsDepartureHeaderContainer>();

			departureGoodsItem.SupportingDocuments.AddNew();
			departureGoodsItem.SupportingDocuments.AddNew();
			departureGoodsItem.SupportingDocuments.AddNew();

			var goodsItem2 = depatureGoodsItems.AddNew();
			goodsItem2.Packages.AddNew();
			goodsItem2.Packages.AddNew();
			goodsItem2.Packages.AddNew();

			var headerArrival = Factory.New<NctsHeader>();
			headerArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: 2 departure goods items", 2, depatureGoodsItems.Count);

				headerArrival.ArrivalMovementHeader.PopulateArrivalGoodsItemsFromDeparture(headerDeparture.MovementHeader);
				var arrivalGoodsItems = headerArrival.ArrivalMovementHeader.GoodsItems;
				AssertEquals("expected 2 arrival goods items", 2, arrivalGoodsItems.Count);
				AssertEquals("expected 2 Containers for the first goodsItem", 2, arrivalGoodsItems[0].Containers.Count);
				AssertEquals("expected 3 Documents for the first goodsItem", 3, arrivalGoodsItems[0].SupportingDocuments.Count);
				AssertEquals("expected 0 Packages for the first goodsItem", 0, arrivalGoodsItems[0].Packages.Count);

				AssertEquals("expected 0 Containers for the second goodsItem", 0, arrivalGoodsItems[1].Containers.Count);
				AssertEquals("expected 0 Documents for the second goodsItem", 0, arrivalGoodsItems[1].SupportingDocuments.Count);
				AssertEquals("expected 3 Packages for the second goodsItem", 3, arrivalGoodsItems[1].Packages.Count);
			});
		}

		public void TestBM_RL_NKDestinationPortReadOnly()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals(false, arrivalMovement.BM_RL_NKDestinationPortInfo.ReadOnly);

			var goodItem = arrivalMovement.GoodsItems.AddNew();
			AssertEquals(false, arrivalMovement.BM_RL_NKDestinationPortInfo.ReadOnly);

			goodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
			AssertEquals(false, arrivalMovement.BM_RL_NKDestinationPortInfo.ReadOnly);
		}

		public void TestBM_CustomsStatus_MaxLength()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals(3, arrivalMovement.BM_CustomsStatusInfo.MaxLength);
		}

		public void TestBM_CustomsStatus_ReadOnly()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals(true, arrivalMovement.BM_CustomsStatusInfo.ReadOnly);
		}

		public void TestBM_ArrivalDateCaption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			NCTSTestHelper.AssertCaptions(arrivalMovement.BM_ArrivalDateInfo, "Arrival Date&Time", "Arrival Date", "Arr. D&T");
		}

		public void TestBM_EntryDateCaption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			NCTSTestHelper.AssertCaptions(arrivalMovement.BM_EntryDateInfo, "Acceptance Date", "Accept. Date", "Acc. Date");
		}

		public void TestBM_CarnetTotalPagesCaption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			NCTSTestHelper.AssertCaptions(arrivalMovement.BM_CarnetTotalPagesInfo, "Total Page Number", "Page Number", "Page No.");
		}

		public void TestArrivalStatusDescription()
		{
			CombineAssertions(() =>
			{
				var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

				AssertEquals("No Status", ZString.Empty, arrivalMovement.ArrivalStatusDescription);
				arrivalMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
				AssertEquals("Unkown Status", ZString.Empty, arrivalMovement.ArrivalStatusDescription);
				arrivalMovement.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
				AssertEquals("Unloading Description", NCTS5ArrivalCustomsStatusList.Descriptions.UnloadPermissionGranted, arrivalMovement.ArrivalStatusDescription);
			});
		}

		public void TestIsSimplifiedNctsProcedure_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("Simplified Arrival?", DataBoundResourceStrings.GetDataForProperty(arrivalMovement.IsSimplifiedNctsProcedureInfo).Caption);
		}

		public void TestBM_CustomsSubPlace_MaxLength()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals(17, arrivalMovement.BM_CustomsSubPlaceInfo.MaxLength);
		}

		public void TestBM_CustomsSubPlace_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

				arrivalMovement.IsSimplifiedNctsProcedure = false;
				AssertEquals("Not Simplified", true, arrivalMovement.BM_CustomsSubPlaceInfo.ReadOnly);
				arrivalMovement.IsSimplifiedNctsProcedure = true;
				AssertEquals("Simplified", false, arrivalMovement.BM_CustomsSubPlaceInfo.ReadOnly);
			});
		}

		public void TestBM_DischargeType()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_DischargeTypeInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Discharge TIR", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Discharge", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Discharge", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_CarnetTotalPages()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_CarnetTotalPagesInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Total Page Number", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Page Number", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Page No.", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_CustomsStatus()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_CustomsStatusInfo);
			AssertEquals("Caption", "Arrival Status", captionResourceString.Caption);
		}

		public void TestBM_DischargeType_MaxLength()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals(2, arrivalMovement.BM_DischargeTypeInfo.MaxLength);
		}

		public void TestBM_LocationOfGoodsCode_MaxLength()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals(17, arrivalMovement.BM_LocationOfGoodsCodeInfo.MaxLength);
		}

		public void TestSetDefaultValues()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Default BM_SubApplicationCode", NctsMoveHeaderType.Codes.Arrival, arrivalMovement.BM_SubApplicationCode);
				AssertEquals("Default BM_NoChangesToReport", true, arrivalMovement.BM_NoChangesToReport);
				AssertEquals("Default BM_StateOfSeals", true, arrivalMovement.BM_StateOfSealsBoolean);
				AssertEquals("Default BM_UnloadingCompleted", true, arrivalMovement.BM_UnloadingCompleted);
				AssertEquals("Default BM_GrossWeightUQ", Core.Constants.Weight.Kilograms, arrivalMovement.BM_GrossWeightUQ);
			});
		}

		public void TestLookups()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertType<NctsArrivalMovementHeaderLookups>(arrivalMovement.Lookups);
		}

		public void TestValidation()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertType<NctsArrivalMovementHeaderValidation>(arrivalMovement.Validation);
		}

		public void TestSeals()
		{
			CombineAssertions(() =>
			{
				var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

				AssertType<SealCollection<Seal>>(arrivalMovement.Seals);
				AssertType<NctsArrivalMovementHeader>(arrivalMovement.Seals.Master);
			});
		}

		public void TestGoodsLocationDescription_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(typeof(NctsArrivalMovementHeader), nameof(NctsArrivalMovementHeader.GoodsLocationDescription));
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Location of Goods", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Location", captionResourceString.ShortCaption);
			});
		}

		public void TestGoodsLocation()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var goodsLocation = arrivalMovement.GoodsLocation;

			CombineAssertions(() =>
			{
				AssertEquals("CGL_ParentID", arrivalMovement.PK, goodsLocation.CGL_ParentID);
				AssertEquals("CGL_ParentTableCode", CusInBondMoveHeaderSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
				AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.Arrival, goodsLocation.CGL_LocationUse);
				AssertSame("Cached", goodsLocation, arrivalMovement.GoodsLocation);
				AssertEquals("IsRegisteredEditableChildObject", true, arrivalMovement.IsRegisteredEditableChildObject(goodsLocation));
				AssertType<CusGoodsLocation>(goodsLocation);
			});
		}

		public void TestGoodsLocationDescription()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, arrivalMovement.GoodsLocationDescription);
				AssertNull("GoodsLocation doesn't exist", CusGoodsLocation.Load<EU.Business.CusGoodsLocation>(arrivalMovement, CusGoodsLocationUseList.Codes.Arrival));

				var goodsLocation = arrivalMovement.GoodsLocation;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertEquals("GoodsLocationDescription when there's GoodsLocation", "Z", arrivalMovement.GoodsLocationDescription);
			});
		}

		public void TestGoodsLocationDescription_RegisterEditableChildObject()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			Customs.Business.CusGoodsLocation.New<CusGoodsLocation>(arrivalMovement, CusGoodsLocationUseList.Codes.Arrival);
			_ = arrivalMovement.GoodsLocationDescription;
			AssertEquals("IsRegisteredEditableChildObject", true, arrivalMovement.IsRegisteredEditableChildObject(arrivalMovement.GoodsLocation));
		}

		public void TestGoodsLocationDescriptionInfo()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals(nameof(NctsArrivalMovementHeader.GoodsLocationDescription), arrivalMovement.GoodsLocationDescriptionInfo.Name);
		}

		public void TestValidateGoodsLocationDescription()
		{
			var arrivalMovementHeader = CreatePhase5ArrivalHeader().ArrivalMovementHeader;
			arrivalMovementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			arrivalMovementHeader.ValidateGoodsLocationDescription();
			AssertHasMessageError(arrivalMovementHeader.GoodsLocationDescriptionInfo, "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.");
		}

		public void TestICusGoodsLocationProvider_ProviderKey()
		{
			var arrivalMovementHeader = GetNewBusinessObject();
			AssertEquals("LVNCTS", (arrivalMovementHeader as ICusGoodsLocationProvider).ProviderKey);
		}

		public void TestOtherThingsToReport()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			arrivalMovement.OtherThingsToReport = "I'm reporting some other things that might be important";
			AssertEquals("I'm reporting some other things that might be important", arrivalMovement.OtherThingsToReport);

			Factory.Save();

			var otherThingsToReport = arrivalMovement.Notes;
			AssertEquals("Length", 1, otherThingsToReport.DatabaseCount);

			var note = otherThingsToReport.FindByDescription(PredefinedNoteTypes.Instance.OtherThingsToReport.Description)[0];
			AssertEquals("I'm reporting some other things that might be important", note.ST_NoteText);

			var otherFactory = new BusinessObjectFactory();
			var arrivalMovementCopy = otherFactory.Load<NctsArrivalMovementHeader>(arrivalMovement.PK);
			AssertEquals("Before getting OtherThingsToReport, arrivalMovementCopy.HasChanges = false", false, arrivalMovementCopy.HasChanges);
			AssertEquals("I'm reporting some other things that might be important", arrivalMovementCopy.OtherThingsToReport);
			AssertEquals("After getting OtherThingsToReport, arrivalMovementCopy.HasChanges = false", false, arrivalMovementCopy.HasChanges);

			arrivalMovementCopy.OtherThingsToReport = "Changed the text";
			otherFactory.Save();

			var thirdFactory = new BusinessObjectFactory();
			var arrivalMovementCopy2 = thirdFactory.Load<NctsArrivalMovementHeader>(arrivalMovement.PK);
			AssertEquals("Changed the text", arrivalMovementCopy2.OtherThingsToReport);
		}

		public void TestNoteTypes()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			AssertEquals("OtherThingsToReport", true, arrivalMovement.NoteTypes.IsPredefinedNoteTypeByDescription(PredefinedNoteTypes.Instance.OtherThingsToReport.Description));
		}

		public void TestBM_UnloadingDate_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_UnloadingDateInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Date of unloading", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Unloading Date", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Date", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_NoChangesToReport_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_NoChangesToReportInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Unloaded cargo conforms to declaration", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Unloading conforms", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Conforms", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_StateOfSealsBoolean_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_StateOfSealsBooleanInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "All seals present and undamaged", captionResourceString.Caption);
				AssertEquals("MediumCaption", "All seals are OK", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Seals OK", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_UnloadingCompleted_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_UnloadingCompletedInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Unloading complete", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Completed", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_UnloadingRemarks_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.BM_UnloadingRemarksInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Enter the remarks regarding the unloading", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Unloading remarks", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Remarks", captionResourceString.ShortCaption);
			});
		}

		public void TestOtherThingsToReport_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(arrivalMovement.OtherThingsToReportInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Report here the things that happened outside the unloading but had/have an effect on the cargo unloaded", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Other things to report", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Other info", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_InlandTransportMode_ReadOnly()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("ReadOnly", true, arrivalMovement.BM_InlandTransportModeInfo.ReadOnly);
		}

		public void TestBM_InlandTransportMode_Caption()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			NCTSTestHelper.AssertCaptions(arrivalMovement.BM_InlandTransportModeInfo, "Transport Mode (Inland)", string.Empty, "Inland M.O.T.");
		}

		public void TestDeclaredMovementDetail()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertNull("No movement details", arrivalMovement.DeclaredMovementDetail);

				var movementDetail = arrivalMovement.MovementDetails.AddNew();
				movementDetail.B9_B0 = Factory.New<NctsBill>().PK;
				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertNull("B9_B0 not null", arrivalMovement.DeclaredMovementDetail);

				movementDetail.B9_B0 = ZGuid.Empty;
				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertNull("B9_UnloadedState <> 'DEC'", arrivalMovement.DeclaredMovementDetail);

				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				var declaredMovementDetail = arrivalMovement.DeclaredMovementDetail;
				AssertEquals("DeclaredMovementDetail", movementDetail.PK, declaredMovementDetail.PK);
				AssertEquals("ReadOnly", true, declaredMovementDetail.ReadOnly);
				AssertSame("Cached", declaredMovementDetail, arrivalMovement.DeclaredMovementDetail);
			});
		}

		public void TestUnloadedMovementDetail_NEW()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertNull("No movement details", arrivalMovement.UnloadedMovementDetail);

				var movementDetail = arrivalMovement.MovementDetails.AddNew();
				movementDetail.B9_B0 = Factory.New<NctsBill>().PK;
				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertNull("B9_B0 not null", arrivalMovement.UnloadedMovementDetail);

				movementDetail.B9_B0 = ZGuid.Empty;
				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertNull("B9_UnloadedState not in ('NEW', 'DIF')", arrivalMovement.UnloadedMovementDetail);

				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				var unloadedMovementDetail = arrivalMovement.UnloadedMovementDetail;
				AssertEquals("UnloadedMovementDetail", movementDetail.PK, unloadedMovementDetail.PK);
				AssertSame("Cached", unloadedMovementDetail, arrivalMovement.UnloadedMovementDetail);
			});
		}

		public void TestUnloadedMovementDetail_DIF()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertNull("No movement details", arrivalMovement.UnloadedMovementDetail);

				var movementDetail = arrivalMovement.MovementDetails.AddNew();
				movementDetail.B9_B0 = Factory.New<NctsBill>().PK;
				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertNull("B9_B0 not null", arrivalMovement.UnloadedMovementDetail);

				movementDetail.B9_B0 = ZGuid.Empty;
				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertNull("B9_UnloadedState not in ('NEW', 'DIF')", arrivalMovement.UnloadedMovementDetail);

				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedMovementDetail = arrivalMovement.UnloadedMovementDetail;
				AssertEquals("UnloadedMovementDetail", movementDetail.PK, unloadedMovementDetail.PK);
				AssertSame("Cached", unloadedMovementDetail, arrivalMovement.UnloadedMovementDetail);
			});
		}

		public void TestUnloadedB9_TransportAtDepartureID()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedB9_TransportAtDepartureIDInfo,
				movementDetail => movementDetail.B9_TransportAtDepartureIDInfo);
		}

		public void TestUnloadedB9_TransportAtDepartureID_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_TransportAtDepartureIDMaxLength, arrivalMovement.UnloadedB9_TransportAtDepartureIDInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedB9_TransportAtDepartureID), "Transport ID", string.Empty, string.Empty);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureIDNationality()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo,
				movementDetail => movementDetail.B9_RN_NKTransportAtDepartureIDNationalityInfo);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureIDNationality_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_RN_NKTransportAtDepartureIDNationalityMaxLength, arrivalMovement.UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedB9_RN_NKTransportAtDepartureIDNationality), "Nationality", string.Empty, string.Empty);
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer1RegNo()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedB9_TransportAtDepartureTrailer1RegNoInfo,
				movementDetail => movementDetail.B9_TransportAtDepartureTrailer1RegNoInfo);
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer1RegNo_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_TransportAtDepartureTrailer1RegNoMaxLength, arrivalMovement.UnloadedB9_TransportAtDepartureTrailer1RegNoInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedB9_TransportAtDepartureTrailer1RegNo), "Trailer ID 1", string.Empty, string.Empty);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo,
				movementDetail => movementDetail.B9_RN_NKTransportAtDepartureTrailer1NationalityInfo);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_RN_NKTransportAtDepartureTrailer1NationalityMaxLength, arrivalMovement.UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality), "Nationality", string.Empty, string.Empty);
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer2RegNo()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedB9_TransportAtDepartureTrailer2RegNoInfo,
				movementDetail => movementDetail.B9_TransportAtDepartureTrailer2RegNoInfo);
		}

		public void TestUnloadedB9_TransportAtDepartureTrailer2RegNo_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_TransportAtDepartureTrailer2RegNoMaxLength, arrivalMovement.UnloadedB9_TransportAtDepartureTrailer2RegNoInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedB9_TransportAtDepartureTrailer2RegNo), "Trailer ID 2", string.Empty, string.Empty);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo,
				movementDetail => movementDetail.B9_RN_NKTransportAtDepartureTrailer2NationalityInfo);
		}

		public void TestUnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_RN_NKTransportAtDepartureTrailer2NationalityMaxLength, arrivalMovement.UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality), "Nationality", string.Empty, string.Empty);
		}

		public void TestUnloadedB9_AircraftIDAtDeparture()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedB9_AircraftIDAtDepartureInfo,
				movementDetail => movementDetail.B9_AircraftIDAtDepartureInfo);
		}

		public void TestUnloadedB9_AircraftIDAtDeparture_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_AircraftIDAtDepartureMaxLength, arrivalMovement.UnloadedB9_AircraftIDAtDepartureInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedB9_AircraftIDAtDeparture), "Aircraft ID", string.Empty, string.Empty);
		}

		public void TestUnloadedVesselNameAtDeparture()
		{
			AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(movementHeader => movementHeader.UnloadedVesselNameAtDepartureInfo,
				movementDetail => movementDetail.VesselNameAtDepartureInfo);
		}

		public void TestUnloadedVesselNameAtDeparture_Properties()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

			AssertEquals("MaxLength", CusInBondMoveDetail.Schema.B9_TransportAtDepartureIDMaxLength, arrivalMovement.UnloadedVesselNameAtDepartureInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(typeof(NctsArrivalMovementHeader), nameof(arrivalMovement.UnloadedVesselNameAtDeparture), "Vessel", string.Empty, string.Empty);
		}

		public void TestTotalNumberOfPackages_Phase4()
		{
			var bulkType = Factory.SetupBulkCusCode();
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var goodsItem1 = arrivalMovement.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();

			package1.B5_UnitCount = 10;

			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitType = bulkType;
			package2.B5_UnitCount = 5;

			var goodsItem2 = arrivalMovement.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitType = bulkType;

			AssertEquals(16, arrivalMovement.TotalNumberOfPackages);
		}

		public void TestTotalNumberOfPackages_Phase5()
		{
			var bulkType = Factory.SetupBulkCusCode();
			var header = CreatePhase5ArrivalHeader();
			var bill1 = header.Bills.AddNew();
			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			package1.B5_UnitCount = 10;

			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			package2.B5_UnitType = bulkType;

			var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			package3.B5_UnitType = bulkType;
			package3.B5_UnitCount = 3;

			var bill2 = header.Bills.AddNew();
			var goodsItem3 = bill2.ArrivalGoodsItems.AddNew();
			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			package4.B5_UnitCount = 5;

			var package5 = goodsItem3.Packages.AddNew();
			package5.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			package5.B5_B5_ParentPackage = package4.PK;
			package5.B5_UnitCount = 8;

			var package6 = goodsItem3.Packages.AddNew();
			package6.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package6.B5_UnitCount = 100;

			var package7 = goodsItem3.Packages.AddNew();
			package7.B5_UnitCount = 300;

			AssertEquals(19, header.ArrivalMovementHeader.TotalNumberOfPackages);
		}

		public void TestTotalUnloadedGrossMassInKilograms()
		{
			var header = CreatePhase5ArrivalHeader();
			var bill1 = header.Bills.AddNew();
			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 300;
			goodsItem1.BY_GrossWeightUnit = "KG";

			var bill2 = header.Bills.AddNew();
			var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 10;
			goodsItem2.BY_GrossWeightUnit = "KG";
			var goodsItem3 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_GrossWeight = 600;
			goodsItem3.BY_GrossWeightUnit = "G";

			var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			goodsItem4.BY_GrossWeight = 60;
			goodsItem4.BY_GrossWeightUnit = "KG";

			var goodsItem5 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem5.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem5.BY_GrossWeight = 100;
			goodsItem5.BY_GrossWeightUnit = "KG";

			AssertEquals(410.6m, header.ArrivalMovementHeader.TotalUnloadedGrossMassInKilograms);
		}

		public void TestTotalUnloadedGrossMassInKilograms_DIF()
		{
			var header = CreatePhase5ArrivalHeader();
			var bill1 = header.Bills.AddNew();
			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 300;
			goodsItem1.BY_GrossWeightUnit = "KG";

			var bill2 = header.Bills.AddNew();
			var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 10;
			goodsItem2.BY_GrossWeightUnit = "KG";
			var goodsItem3 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem3.BY_GrossWeight = 2;
			goodsItem3.BY_GrossWeightUnit = "KG";
			var unloadedGoodsItem = goodsItem3.UnloadedGoodsItem;
			unloadedGoodsItem.BY_GrossWeight = 10;
			unloadedGoodsItem.BY_GrossWeightUnit = "KG";

			var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			goodsItem4.BY_GrossWeight = 60;
			goodsItem4.BY_GrossWeightUnit = "KG";
			AssertEquals(320m, header.ArrivalMovementHeader.TotalUnloadedGrossMassInKilograms);
		}

		public void TestTotalUnloadedNumberOfPackages()
		{
			var bulkType = Factory.SetupBulkCusCode();
			var header = CreatePhase5ArrivalHeader();

			var bill1 = header.Bills.AddNew();

			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			package1.B5_UnitCount = 10;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package2.B5_UnitType = bulkType;

			var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 20;

			var goodsItem3 = bill1.ArrivalGoodsItems.AddNew();
			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_UnitCount = 30;

			var bill2 = header.Bills.AddNew();

			var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			var package5 = goodsItem4.Packages.AddNew();
			package5.B5_UnitCount = 60;

			AssertEquals(101, header.ArrivalMovementHeader.TotalUnloadedNumberOfPackages);
		}

		public void TestTotalUnloadedNumberOfPackagesExcludesDIFTypeOfDifference()
		{
			var header = CreatePhase5ArrivalHeader();
			var bill1 = header.Bills.AddNew();
			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			package1.B5_UnitCount = 10;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package2.B5_UnitCount = 20;

			var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			package3.PackDifference.B5_UnitCount = 30;
			AssertEquals(20, header.ArrivalMovementHeader.TotalUnloadedNumberOfPackages);
		}

		public void TestArrivalDetailsReadOnly()
		{
			var header = CreatePhase5ArrivalHeader();
			var movementHeader = header.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				movementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Unknown;
				AssertEquals("Customs Status different from 'ACK' or 'SNT', BM_CarnetTotalPages should be enabled", false, movementHeader.BM_CarnetTotalPagesInfo.ReadOnly);
				AssertEquals("Customs Status different from 'ACK' or 'SNT', BM_DischargeType should be enabled", false, movementHeader.BM_DischargeTypeInfo.ReadOnly);
				AssertEquals("Customs Status different from 'ACK' or 'SNT', Goods Location should be enabled", false, movementHeader.GoodsLocation.ReadOnly);
				AssertEquals("Customs Status different from 'ACK' or 'SNT', Authorization Code should be enabled", false, movementHeader.AuthorizationCodeInfo.ReadOnly);
				AssertEquals("Customs Status different from 'ACK' or 'SNT', Authorization Number should be enabled", false, movementHeader.AuthorizationNumberInfo.ReadOnly);
				AssertEquals("Customs Status different from 'ACK' or 'SNT', Authorization Owner should be enabled", false, movementHeader.AuthorizationOwnerInfo.ReadOnly);

				header = CreatePhase5ArrivalHeader();
				movementHeader = header.ArrivalMovementHeader;
				var goodsLocation = movementHeader.GoodsLocation;
				AssertNotNull("Goods Location should not be null", goodsLocation);
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
				AssertEquals("Customs Status 'ACK', BM_CarnetTotalPages should be readonly", true, movementHeader.BM_CarnetTotalPagesInfo.ReadOnly);
				AssertEquals("Customs Status 'ACK', BM_DischargeType should be readonly", true, movementHeader.BM_DischargeTypeInfo.ReadOnly);
				AssertEquals("Customs Status 'ACK', Goods Location should be readonly", true, movementHeader.GoodsLocation.ReadOnly);
				AssertEquals("Customs Status 'ACK', Authorization Code should be readonly", true, movementHeader.AuthorizationCodeInfo.ReadOnly);
				AssertEquals("Customs Status 'ACK', Authorization Number should be readonly", true, movementHeader.AuthorizationNumberInfo.ReadOnly);
				AssertEquals("Customs Status 'ACK', Authorization Owner should be readonly", true, movementHeader.AuthorizationOwnerInfo.ReadOnly);

				header = CreatePhase5ArrivalHeader();
				movementHeader = header.ArrivalMovementHeader;
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Customs Status 'SNT', BM_CarnetTotalPages should be readonly", true, movementHeader.BM_CarnetTotalPagesInfo.ReadOnly);
				AssertEquals("Customs Status 'SNT', BM_DischargeType should be readonly", true, movementHeader.BM_DischargeTypeInfo.ReadOnly);
				AssertEquals("Customs Status 'SNT', Goods Location (description) should be readonly", true, movementHeader.GoodsLocation.ReadOnly);
				AssertEquals("Customs Status 'SNT', Authorization Code should be readonly", true, movementHeader.AuthorizationCodeInfo.ReadOnly);
				AssertEquals("Customs Status 'SNT', Authorization Number should be readonly", true, movementHeader.AuthorizationNumberInfo.ReadOnly);
				AssertEquals("Customs Status 'SNT', Authorization Owner should be readonly", true, movementHeader.AuthorizationOwnerInfo.ReadOnly);
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

				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'ACK', the result should be true", true, movementHeader.IsArrivalDetailsReadOnly);
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When is Arrival and BM_CustomsStatus is 'SNT', the result should be true", true, movementHeader.IsArrivalDetailsReadOnly);

				movementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Unknown;
				AssertEquals("When is Arrival and BM_CustomsStatus is empty (not ACK or SNT), the result should be false.", false, movementHeader.IsArrivalDetailsReadOnly);
			});
		}

		public void TestArrivalTransportInfoList()
		{
			var header = CreatePhase5ArrivalHeader();
			var movementHeader = header.ArrivalMovementHeader;
			var arrivalTransportInfos = movementHeader.ArrivalTransportInfos;
			CombineAssertions(() =>
			{
				AssertType<ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>>("Type", arrivalTransportInfos);
				AssertEquals("IsRegisteredEditableChildObject", true, movementHeader.IsRegisteredEditableChildObject(arrivalTransportInfos));
				AssertSame("Cached", arrivalTransportInfos, movementHeader.ArrivalTransportInfos);
				AssertEquals("IsLoaded", true, arrivalTransportInfos.IsLoaded);
			});
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var header = CreatePhase5ArrivalHeader();
			var movementHeader = header.ArrivalMovementHeader;

			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)movementHeader).GetCusSupportingInfoTypes();
			CombineAssertions(() =>
			{
				AssertEquals("#CusSupportingInfoTypes", 2, cusSupportingInfoTypes.Count);
				AssertEquals("SUP", typeof(NctsSupportingDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals("OTH", typeof(NctsAdditionalInfo), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			});
		}

		public void TestConstructor_SetReadOnlyForUnloadingDifferencesData_WhenBM_NoChangesToReport_True() => CombineAssertions(() =>
		{
			var header = CreatePhase5ArrivalHeader();
			var bill = header.Bills.AddNew();
			var arrivalMovementHeader = header.ArrivalMovementHeader;
			arrivalMovementHeader.BM_NoChangesToReport = true;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedHeader = newFactory.Load<NctsHeader>(header.PK);
			var loadedArrivalMovementHeader = loadedHeader.ArrivalMovementHeader;
			var loadedBill = loadedHeader.Bills[0];
			AssertEquals("Bill readonly", expected: true, loadedBill.ReadOnly);
			AssertEquals("Bill.ArrivalTransportInfos readonly", expected: true, loadedBill.ArrivalTransportInfos.ReadOnly);
			AssertEquals("Bill.ArrivalGoodsItems (as one example of Bill children) readonly", expected: true, loadedBill.ArrivalGoodsItems.ReadOnly);
		});

		public void TestConstructor_SetReadOnlyForUnloadingDifferencesData_WhenBM_NoChangesToReport_True_IsPhase5TP() => CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				var header = CreatePhase5ArrivalHeader();
				var bill = header.Bills.AddNew();
				var arrivalMovementHeader = header.ArrivalMovementHeader;
				arrivalMovementHeader.BM_NoChangesToReport = true;
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedHeader = newFactory.Load<NctsHeader>(header.PK);
				var loadedArrivalMovementHeader = loadedHeader.ArrivalMovementHeader;
				var loadedBill = loadedHeader.Bills[0];
				AssertEquals("Bill readonly", expected: true, loadedBill.ReadOnly);
				AssertEquals("Bill.ArrivalTransportInfos readonly due to Phase5TP", expected: true, loadedBill.ArrivalTransportInfos.ReadOnly);
				AssertEquals("Bill.ArrivalGoodsItems (as one example of Bill children) readonly", expected: true, loadedBill.ArrivalGoodsItems.ReadOnly);
			}
		});

		public void TestConstructor_SetReadOnlyForUnloadingDifferencesData_WhenBM_NoChangesToReport_False() => CombineAssertions(() =>
		{
			var header = CreatePhase5ArrivalHeader();
			var bill = header.Bills.AddNew();
			var arrivalMovementHeader = header.ArrivalMovementHeader;
			arrivalMovementHeader.BM_NoChangesToReport = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedHeader = newFactory.Load<NctsHeader>(header.PK);
			var loadedArrivalMovementHeader = loadedHeader.ArrivalMovementHeader;
			var loadedBill = loadedHeader.Bills[0];
			AssertEquals("Bill not readonly", expected: false, loadedBill.ReadOnly);
			AssertEquals("Bill.ArrivalTransportInfos not readonly", expected: false, loadedBill.ArrivalTransportInfos.ReadOnly);
			AssertEquals("Bill.ArrivalGoodsItems (as one example of Bill children) not readonly", expected: false, loadedBill.ArrivalGoodsItems.ReadOnly);
		});

		public void TestBM_ArrivalDateReadOnly_ArrivalNotificationDisabled()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsHeader.ArrivalMovementHeader.BM_ArrivalDateInfo, nctsHeader);
		}

		public void TestBM_CarnetTotalPagesReadOnly_ArrivalNotificationDisabled()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsHeader.ArrivalMovementHeader.BM_CarnetTotalPagesInfo, nctsHeader);
		}

		public void TestBM_DischargeTypeReadOnly_ArrivalNotificationDisabled()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsHeader.ArrivalMovementHeader.BM_DischargeTypeInfo, nctsHeader);
		}

		public void TestAuthorizationCodeReadOnly_ArrivalNotificationDisabled()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsHeader.ArrivalMovementHeader.AuthorizationCodeInfo, nctsHeader);
		}

		public void TestAuthorizationNumberReadOnly_ArrivalNotificationDisabled()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsHeader.ArrivalMovementHeader.AuthorizationNumberInfo, nctsHeader);
		}

		public void TestAuthorizationOwnerReadOnly_ArrivalNotificationDisabled()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsHeader.ArrivalMovementHeader.AuthorizationOwnerInfo, nctsHeader);
		}

		public void TestBM_PaperlessInbondNum_ReadOnly()
		{
			var arrivalMovementHeader = (NctsArrivalMovementHeader)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					arrivalMovementHeader.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("Enabled for Phase4 - registry = false (default)", false, arrivalMovementHeader.BM_PaperlessInbondNumInfo.ReadOnly);
					AssertEquals("Enabled for Phase4 - registry = false (default)", false, arrivalMovementHeader.ShouldGenerateLocalReferenceNumberOnSaving);

					arrivalMovementHeader.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("Disabled for Phase5 - registry = false (default)", true, arrivalMovementHeader.BM_PaperlessInbondNumInfo.ReadOnly);
					AssertEquals("Disabled for Phase5 - registry = false (default)", true, arrivalMovementHeader.ShouldGenerateLocalReferenceNumberOnSaving);
				}

				using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					arrivalMovementHeader.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("Enabled for Phase4 - registry = true", false, arrivalMovementHeader.BM_PaperlessInbondNumInfo.ReadOnly);
					AssertEquals("Enabled for Phase4 - registry = true", false, arrivalMovementHeader.ShouldGenerateLocalReferenceNumberOnSaving);

					arrivalMovementHeader.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("Enabled for Phase5 - registry = true", false, arrivalMovementHeader.BM_PaperlessInbondNumInfo.ReadOnly);
					AssertEquals("Enabled for Phase5 - registry = true", false, arrivalMovementHeader.ShouldGenerateLocalReferenceNumberOnSaving);
				}
			});
		}

		public void TestArrivalCustomerReferenceNumberFountain()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			AssertEquals("Arrival Number Fountain", "EULocalReferenceNumber", (nctsHeader.ArrivalMovementHeader as ILRNGenerator).LrnNumberFountain.Name);
		}

		public void TestBM_PaperlessInbondNum_NotAutomaticallyGeneratedWhenManualArrivalCustomerReferenceIsEnabled()
		{
			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var nctsHeader = CreatePhase5ArrivalHeader();
				nctsHeader.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22334455", GlbCompany.CurrentCompany.Country);
				Factory.Save();

				AssertEquals("LNR Nbr not generated", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			}
		}

		[TestDate(2023, 11, 22)]
		public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedWhenEmpty()
		{
			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var nctsHeader = CreatePhase5ArrivalHeader();
				nctsHeader.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22334455", GlbCompany.CurrentCompany.Country);
				Factory.Save();

				AssertEquals("Autogenerated LNR Nbr", "2322334455000000000001", nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			}
		}

		public void TestUnloadingDifferenceDataReadOnly()
		{
			CombineAssertions(() =>
			{
				var arrivalMovementHeader = (NctsArrivalMovementHeader)GetNewBusinessObject();
				arrivalMovementHeader.BM_NoChangesToReport = true;
				AssertEquals("BM_NoChangesToReport true", expected: true, arrivalMovementHeader.UnloadingDifferenceDataReadOnly);

				arrivalMovementHeader.BM_NoChangesToReport = false;
				AssertEquals("BM_NoChangesToReport false", expected: false, arrivalMovementHeader.UnloadingDifferenceDataReadOnly);
			});
		}

		public void TestBM_PaperlessInbondNum_NotAutomaticallyGeneratedWhenCustomerReferenceIsNotEmpty()
		{
			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var nctsHeader = CreatePhase5ArrivalHeader();
				nctsHeader.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22334455", GlbCompany.CurrentCompany.Country);
				nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "XYZ";
				Factory.Save();

				AssertEquals("Autogenerated LNR Nbr", "XYZ", nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
			}
		}

		[TestDate(2022, 12, 31)]
		public void TestRollbackBM_PaperlessInbondNumOnSavingFailed()
		{
			var nctsHeader = CreatePhase5ArrivalHeader();
			nctsHeader.BH_OA_Importer = ZGuid.NewZGuid();
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;

			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				nctsHeader.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", GlbCompany.CurrentCompany.Country);

				var referenceNumber = ZString.Empty;
				arrivalMovement.BM_PaperlessInbondNumInfo.ValueChanged += (o, e) =>
				{
					if (!arrivalMovement.BM_PaperlessInbondNum.IsEmpty)
					{
						referenceNumber = arrivalMovement.BM_PaperlessInbondNum;
					}
				};

				AssertExceptionThrown<ZSaveException>("Saving failed", nctsHeader.Factory.Save);
				AssertEquals("local reference number had been set on saving", "2212345000000000000001", referenceNumber);
				AssertEquals("reset local reference number", ZString.Empty, arrivalMovement.BM_PaperlessInbondNum);
			}
		}

		public void TestICanBeImportOrExport()
		{
			var arrivalMovementHeader = (NctsArrivalMovementHeader)GetNewBusinessObject();
			EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport importOrExport = arrivalMovementHeader;

			AssertEquals("CountryCode", arrivalMovementHeader.CountryCode, importOrExport.TrueCountryCode);
			AssertEquals("Data Grouping", arrivalMovementHeader.CountryCode, importOrExport.DataGroupingCode);
		}

		public void TestGuaranteesForArrival()
		{
			var arrivalHeader = CreatePhase5ArrivalHeader();
			var arrivalMovement = arrivalHeader.ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("GuaranteesForArrival is empty", 0, arrivalMovement.GuaranteesForArrival.Count);

				var guarantee = arrivalMovement.GuaranteesForArrival.AddNew();

				arrivalMovement.SingleGuaranteeForArrival.PW_BondNumber = "AAA";
				AssertEquals("GuaranteesForArrival[0] shows the correct data PW_BondNumber", "AAA", arrivalMovement.GuaranteesForArrival[0].PW_BondNumber);
				AssertEquals("SingleGuaranteeForArrival PW_BondNumber is set correctly ", "AAA", arrivalMovement.SingleGuaranteeForArrival.PW_BondNumber);
			});
		}

		public void TestShouldGuaranteeForArrivalBeVisible()
		{
			var arrivalHeader = CreatePhase5ArrivalHeader();
			AssertEquals("ShouldGuaranteeForArrivalBeVisible is false for EU", false, arrivalHeader.ArrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible);
		}

		public void TestEffectiveGrossWeightUnloaded()
		{
			var arrivalMovement = PrepareBOsForUpdateGrossWeightsUnloadedTest();
			CombineAssertions(() =>
			{
				arrivalMovement.BM_NoChangesToReport = false;
				arrivalMovement.BM_GrossWeightUnloaded = 1;
				AssertEquals("BM_NoChangesToReport false", 1m, arrivalMovement.EffectiveGrossWeightUnloaded);

				arrivalMovement.BM_NoChangesToReport = true;
				AssertEquals("BM_NoChangesToReport false", 31.1m, arrivalMovement.EffectiveGrossWeightUnloaded);
			});
		}

		public void TestEffectiveGrossWeightUnloaded_ReadOnly()
		{
			var header = CreatePhase5ArrivalHeader();
			var movementHeader = header.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				movementHeader.BM_NoChangesToReport = true;
				AssertEquals("BM_NoChangesToReport = true; IsUnloadingRemarksReadOnly = false", expected: true, movementHeader.EffectiveGrossWeightUnloadedInfo.ReadOnly);

				movementHeader.BM_NoChangesToReport = false;
				AssertEquals("BM_NoChangesToReport = false; IsUnloadingRemarksReadOnly = false", expected: false, movementHeader.EffectiveGrossWeightUnloadedInfo.ReadOnly);

				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("BM_NoChangesToReport = false; IsUnloadingRemarksReadOnly = true", expected: true, movementHeader.EffectiveGrossWeightUnloadedInfo.ReadOnly);
			});
		}

		public void TestUpdateGrossWeightsUnloaded_BM_GrossWeightUnloaded()
		{
			var movementHeader = PrepareBOsForUpdateGrossWeightsUnloadedTest();
			CombineAssertions(() =>
			{
				movementHeader.BM_NoChangesToReport = false;
				movementHeader.BM_GrossWeightUnloaded = 1;
				movementHeader.UpdateGrossWeightsUnloaded();
				AssertEquals("BM_NoChangesToReport false", 31.1m, movementHeader.BM_GrossWeightUnloaded);

				movementHeader.BM_NoChangesToReport = true;
				movementHeader.BM_GrossWeightUnloaded = 1;
				movementHeader.UpdateGrossWeightsUnloaded();
				AssertEquals("BM_NoChangesToReport true", ZDecimal.Zero, movementHeader.BM_GrossWeightUnloaded);
			});
		}

		public void TestUpdateGrossWeightsUnloaded_B0_GrossWeightUnloaded()
		{
			var movementHeader = PrepareBOsForUpdateGrossWeightsUnloadedTest();
			var header = movementHeader.Header;
			CombineAssertions(() =>
			{
				movementHeader.UpdateGrossWeightsUnloaded();
				AssertEquals("Bill1.B0_GrossWeightUnloaded: DIF", 1.1m, header.Bills[0].B0_GrossWeightUnloaded);
				AssertEquals("Bill2.B0_GrossWeightUnloaded: DEC", 30m, header.Bills[1].B0_GrossWeightUnloaded);
			});
		}

		public void TestUpdateBM_GrossWeightUnloadedFromTotalGrossMassInKilograms()
		{
			var movementHeader = PrepareBOsForUpdateGrossWeightsUnloadedTest();
			CombineAssertions(() =>
			{
				movementHeader.BM_NoChangesToReport = false;
				movementHeader.BM_GrossWeightUnloaded = 1;
				movementHeader.UpdateBM_GrossWeightUnloadedFromTotalGrossMassInKilograms();
				AssertEquals("BM_NoChangesToReport false", 31.1m, movementHeader.BM_GrossWeightUnloaded);

				movementHeader.BM_NoChangesToReport = true;
				movementHeader.BM_GrossWeightUnloaded = 1;
				movementHeader.UpdateBM_GrossWeightUnloadedFromTotalGrossMassInKilograms();
				AssertEquals("BM_NoChangesToReport true", ZDecimal.Zero, movementHeader.BM_GrossWeightUnloaded);
			});
		}

		public void TestOnFactorySaving_UpdateGrossWeightsUnloaded_BM_GrossWeightUnloadedAsItIsNotGreaterThanTotalUnloadedGrossMassInKilograms()
		{
			var movementHeader = PrepareBOsForUpdateGrossWeightsUnloadedTest();
			CombineAssertions(() =>
			{
				movementHeader.BM_NoChangesToReport = false;
				movementHeader.BM_GrossWeightUnloaded = 1;
				movementHeader.Factory.Save();
				AssertEquals("BM_NoChangesToReport false", 31.1m, movementHeader.BM_GrossWeightUnloaded);

				movementHeader.BM_NoChangesToReport = true;
				movementHeader.BM_GrossWeightUnloaded = 1;
				movementHeader.Factory.Save();
				AssertEquals("BM_NoChangesToReport true", ZDecimal.Zero, movementHeader.BM_GrossWeightUnloaded);
			});
		}

		[GuiTest]
		public void TestOnFactorySaving_FiresOnFactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms()
		{
			var movementHeader = PrepareBOsForUpdateGrossWeightsUnloadedTest();
			movementHeader.OnFactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms += (sender, e) => { movementHeader.UpdateGrossWeightsUnloaded(); };
			CombineAssertions(() =>
			{
				movementHeader.BM_NoChangesToReport = false;
				movementHeader.BM_GrossWeightUnloaded = 32;
				movementHeader.Factory.Save();
				AssertEquals("BM_NoChangesToReport false", 31.1m, movementHeader.BM_GrossWeightUnloaded);

				movementHeader.BM_NoChangesToReport = true;
				movementHeader.BM_GrossWeightUnloaded = 32;
				movementHeader.Factory.Save();
				AssertEquals("BM_NoChangesToReport true", ZDecimal.Zero, movementHeader.BM_GrossWeightUnloaded);
			});
		}

		public void TestAllowMixedCaseAuthorisationNumbers()
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationAllowMixedCaseAuthorisationNumbers(Factory, false))
			{
				AssertEquals("AllowMixedCaseAuthorisationNumbers", false, arrivalMovement.AllowMixedCaseAuthorisationNumbers);
			}
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationAllowMixedCaseAuthorisationNumbers(Factory, true))
			{
				AssertEquals("AllowMixedCaseAuthorisationNumbers", true, arrivalMovement.AllowMixedCaseAuthorisationNumbers);
			}
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
				&& info.Name != "DestinationCustomsOfficeCodeForArrival")
			{
				base.TestBizObjectField(info);
			}
		}

		#region Custom Fields Test
		[TestedType(typeof(NctsArrivalMovementHeader))]
		sealed class NctsArrivalMovementHeaderCustomFieldsTest : MasterFiles.Business.Testing.TestICustomFieldProvider
		{
		}
		#endregion

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var arrivalHeader = (NctsArrivalMovementHeader)GetNewBusinessObject();
			arrivalHeader.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
			return arrivalHeader;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.ArrivalMovementHeader;
		}

		NctsArrivalMovementHeader PrepareBOsForUpdateGrossWeightsUnloadedTest()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill1 = header.Bills.AddNew();
			bill1.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			bill1.B0_WeightUQ = "KG";
			var bill2 = header.Bills.AddNew();
			bill2.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			bill2.B0_WeightUQ = "KG";

			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem1.BY_GrossWeight = 1.1;
			goodsItem1.BY_GrossWeightUnit = "KG";

			var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem2.BY_GrossWeight = 1;
			goodsItem2.BY_GrossWeightUnit = "KG";
			var unloadedGoodsItem1 = goodsItem2.UnloadedGoodsItem;
			unloadedGoodsItem1.BY_GrossWeight = 20;
			unloadedGoodsItem1.BY_GrossWeightUnit = "KG";

			var goodsItem3 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem3.BY_GrossWeight = 10;
			goodsItem3.BY_GrossWeightUnit = "KG";

			return header.ArrivalMovementHeader;
		}

		void AssertUnloadedMovementDetailUpdatedOrCreatedIfNotExists(Func<NctsArrivalMovementHeader, ZPropertyInfo> movementHeaderPropertyInfoGetter, Func<CusInBondMoveDetail, ZPropertyInfo> movementDetailPropertyInfoGetter)
		{
			var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
			var movementHeaderPropertyInfo = movementHeaderPropertyInfoGetter.Invoke(arrivalMovement);

			CombineAssertions(() =>
			{
				AssertEquals("No UnloadedMovementDetail", ZString.Empty, movementHeaderPropertyInfo.Value);
				AssertNull("Getter doesn't create UnloadedMovementDetail", arrivalMovement.UnloadedMovementDetail);

				AssertNull("DeclaredMovementDetail null", arrivalMovement.DeclaredMovementDetail);
				movementHeaderPropertyInfo.Value = (ZString)"AA";
				var unloadedMovementDetail = arrivalMovement.UnloadedMovementDetail;
				var movementDetailPropertyInfo = movementDetailPropertyInfoGetter.Invoke(unloadedMovementDetail);
				AssertEquals("Setter creates UnloadedMovementDetail with B9_UnloadedState='NEW'", NctsUnloadedStateList.Codes.NEW, unloadedMovementDetail.B9_UnloadedState);
				AssertEquals("Setter updates UnloadedMovementDetail", "AA", movementDetailPropertyInfo.Value);
				AssertEquals("Getter reads from UnloadedMovementDetail", "AA", movementHeaderPropertyInfo.Value);

				arrivalMovement.MovementDetails.DeleteAll();
				var declaredMovementDetail = arrivalMovement.MovementDetails.AddNew();
				declaredMovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertNotNull("DeclaredMovementDetail not null", arrivalMovement.DeclaredMovementDetail);
				movementHeaderPropertyInfo.Value = (ZString)"BB";
				unloadedMovementDetail = arrivalMovement.UnloadedMovementDetail;
				movementDetailPropertyInfo = movementDetailPropertyInfoGetter.Invoke(unloadedMovementDetail);
				AssertEquals("Setter creates UnloadedMovementDetail with B9_UnloadedState='DIF'", NctsUnloadedStateList.Codes.DIF, unloadedMovementDetail.B9_UnloadedState);
				AssertEquals("Setter creates UnloadedMovementDetail with B9_B9_InBondMoveDetail=DeclaredMovementDetail.PK", declaredMovementDetail.PK, unloadedMovementDetail.B9_B9_InBondMoveDetail);
				AssertEquals("Setter updates UnloadedMovementDetail", "BB", movementDetailPropertyInfo.Value);
				AssertEquals("Getter reads from UnloadedMovementDetail", "BB", movementHeaderPropertyInfo.Value);
			});
		}

		NctsHeader CreatePhase5ArrivalHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header;
		}
	}
}
