using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(EnRouteIncident))]
	sealed class EnRouteIncidentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBN_EventPlace_Caption()
		{
			AssertEquals("Event Place", DataBoundResourceStrings.GetDataForProperty(enRouteIncident.BN_EventPlaceInfo).Caption);
		}

		public void TestBN_EventCountryCode_Phase4Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(enRouteIncident.BN_EventCountryCodeInfo, NctsHeader.Phase4CaptionKey, "Event Ctry./Rgn.");
		}

		public void TestBN_EventCountryCode_Phase5Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(enRouteIncident.BN_EventCountryCodeInfo, NctsHeader.Phase5CaptionKey, "Country/Region where incident was reported", "Ctry./Rgn. Rep.", "Ctry./Rgn. Report");
		}

		public void TestBN_IsInNCTS_Caption()
		{
			AssertEquals("In NCTS?", DataBoundResourceStrings.GetDataForProperty(enRouteIncident.IsInNCTSInfo).Caption);
		}

		public void TestBN_EndorsementDate_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_EndorsementDateInfo, NctsHeader.Phase4CaptionKey, "Event Date", string.Empty, string.Empty);
		}

		public void TestBN_EndorsementDate_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_EndorsementDateInfo, NctsHeader.Phase5CaptionKey, "Date incident occurred", "Date", "Date");
		}

		public void TestBN_EndorsementAuthority_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_EndorsementAuthorityInfo, NctsHeader.Phase4CaptionKey, "Reported By", string.Empty, string.Empty);
		}

		public void TestBN_EndorsementAuthority_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_EndorsementAuthorityInfo, NctsHeader.Phase5CaptionKey, "Authority", "Authority", "Authority");
		}

		public void TestBN_EndorsementPlace_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_EndorsementPlaceInfo, NctsHeader.Phase4CaptionKey, "Place Reported", string.Empty, string.Empty);
		}

		public void TestBN_EndorsementPlace_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_EndorsementPlaceInfo, NctsHeader.Phase5CaptionKey, "Place where incident was reported", "Place", "Place");
		}

		public void TestBN_EndorsementCountryCode_Phase4Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(enRouteIncident.BN_EndorsementCountryCodeInfo, NctsHeader.Phase4CaptionKey, "Ctry./Rgn. Reported");
		}

		public void TestBN_EndorsementCountryCode_Phase5Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(enRouteIncident.BN_EndorsementCountryCodeInfo, NctsHeader.Phase5CaptionKey, "Country/Region where incident was reported", "Ctry./Rgn.", "Country/Region");
		}

		public void TestBN_IncidentCode_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_IncidentCodeInfo, NctsHeader.Phase5CaptionKey, "Incident Code", "Incident", "Incident");
		}

		public void TestBN_Information_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_InformationInfo, NctsHeader.Phase5CaptionKey, "Information", "Info", "Info");
		}

		public void TestBN_CustomsStatus_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.BN_CustomsStatusInfo, NctsHeader.Phase5CaptionKey, "Created by", "Created", "Created");
		}

		public void TestBN_Information_Caption()
		{
			AssertEquals("Info", DataBoundResourceStrings.GetDataForProperty(enRouteIncident.BN_InformationInfo).Caption);
		}

		public void TestIncidentContainers()
		{
			AssertEquals(true, enRouteIncident.IsRegisteredEditableChildObject(enRouteIncident.IncidentContainers));
		}

		public void TestLookups()
		{
			AssertType<EnRouteIncidentLookups>(enRouteIncident.Lookups);
		}

		public void TestValidation()
		{
			AssertType<EnRouteIncidentValidation>(enRouteIncident.Validation);
		}

		public void TestSeals()
		{
			var seals = enRouteIncident.Seals;
			CombineAssertions(() =>
			{
				AssertType<CusSealCollection>("Type", seals);
				AssertEquals("IsRegisteredEditableChildObject", true, enRouteIncident.IsRegisteredEditableChildObject(seals));
				AssertSame("Cached", seals, enRouteIncident.Seals);
				AssertEquals("IsLoaded", true, seals.IsLoaded);
			});
		}

		public void TestCusSealType()
		{
			AssertType(((ICusSealTypeSupporter)enRouteIncident).CusSealType, enRouteIncident.Seals.AddNew());
		}

		public void TestBN_IncidentCode_MaxLength()
		{
			AssertEquals(1, enRouteIncident.BN_IncidentCodeInfo.MaxLength);
		}

		public void TestCanDelete_Phase5_WhenNotCreatedByCustomsMessage()
		{
			var header = CreateNcts5Header();
			var incident = header.EnRouteIncidents.AddNew();

			AssertEquals("Can delete when created by user", true, incident.CanDelete);
		}

		public void TestCanDelete_Phase5_WhenCreatedByCustomsMessage()
		{
			var header = CreateNcts5Header();
			var incident = header.EnRouteIncidents.AddNew();
			incident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.CUS;

			AssertEquals("Can delete when created by customs message", false, incident.CanDelete);
		}

		public void TestGoodsLocationDescription_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.GoodsLocationDescriptionInfo, NctsHeader.Phase4CaptionKey, "Location of Goods", string.Empty, "Location");
		}

		public void TestGoodsLocationDescription_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(enRouteIncident.GoodsLocationDescriptionInfo, NctsHeader.Phase5CaptionKey, "Location", "Location", "Loc.");
		}

		public void TestGoodsLocation()
		{
			var incident = (EnRouteIncident)GetNewBusinessObject();
			var goodsLocation = incident.GoodsLocation;

			CombineAssertions(() =>
			{
				AssertEquals("CGL_ParentID", incident.PK, goodsLocation.CGL_ParentID);
				AssertEquals("CGL_ParentTableCode", CusInBondEventSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
				AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.Arrival, goodsLocation.CGL_LocationUse);
				AssertSame("Cached", goodsLocation, incident.GoodsLocation);
				AssertEquals("IsRegisteredEditableChildObject", true, incident.IsRegisteredEditableChildObject(goodsLocation));
			});
		}

		public void TestGoodsLocationDescription()
		{
			var incident = (EnRouteIncident)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertNull("GoodsLocation doesn't exist", CusGoodsLocation.Load<CusGoodsLocation>(incident, CusGoodsLocationUseList.Codes.Arrival));
				AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, incident.GoodsLocationDescription);

				var goodsLocation = incident.GoodsLocation;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertEquals("GoodsLocationDescription when there's GoodsLocation", "Z", incident.GoodsLocationDescription);
			});
		}

		public void TestGoodsLocationDescription_RegisterEditableChildObject()
		{
			var incident = (EnRouteIncident)GetNewBusinessObject();

			CusGoodsLocation.New<CusGoodsLocation>(incident, CusGoodsLocationUseList.Codes.Arrival);
			_ = incident.GoodsLocationDescription;
			AssertEquals("IsRegisteredEditableChildObject", true, incident.IsRegisteredEditableChildObject(incident.GoodsLocation));
		}

		public void TestGoodsLocationDescriptionInfo()
		{
			var incident = (EnRouteIncident)GetNewBusinessObject();

			AssertEquals(nameof(NctsArrivalMovementHeader.GoodsLocationDescription), incident.GoodsLocationDescriptionInfo.Name);
		}

		public void TestICusGoodsLocationProvider_ProviderKey()
		{
			var incident = (EnRouteIncident)GetNewBusinessObject();

			AssertEquals("LVNCTS", (incident as ICusGoodsLocationProvider).ProviderKey);
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				var enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
				AssertEquals("Enterprise.Customs.BE.NCTS.Business.CusGoodsLocation", (enRouteIncident as ICusGoodsLocationTypeSupporter).GoodsLocationType.FullName);
			}
		}

		public void TestBN_CustomsStatus_Phase5_ReadOnly()
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals(true, enRouteIncident.BN_CustomsStatusInfo.ReadOnly);
		}

		public void TestBN_IncidentCode_Phase5_ReadOnly() => AssertPhase5FieldsReadOnly(x => x.BN_IncidentCodeInfo);

		public void TestBN_Information_Phase5_ReadOnly() => AssertPhase5FieldsReadOnly(x => x.BN_InformationInfo);

		public void TestBN_EndorsementDate_Phase5_ReadOnly() => AssertPhase5FieldsReadOnly(x => x.BN_EndorsementDateInfo);

		public void TestBN_EndorsementAuthority_Phase5_ReadOnly() => AssertPhase5FieldsReadOnly(x => x.BN_EndorsementAuthorityInfo);

		public void TestBN_EndorsementPlace_Phase5_ReadOnly() => AssertPhase5FieldsReadOnly(x => x.BN_EndorsementPlaceInfo);

		public void TestBN_EndorsementCountryCode_Phase5_ReadOnly() => AssertPhase5FieldsReadOnly(x => x.BN_EndorsementCountryCodeInfo);

		public void TestBN_EventCountryCode_Phase5_ReadOnly() => AssertPhase5FieldsReadOnly(x => x.BN_EventCountryCodeInfo);

		public void TestBN_IncidentCode_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_IncidentCodeInfo, nctsHeader);
		}

		public void TestBN_Information_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_InformationInfo, nctsHeader);
		}

		public void TestBN_EndorsementDate_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_EndorsementDateInfo, nctsHeader);
		}

		public void TestBN_EndorsementAuthority_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_EndorsementAuthorityInfo, nctsHeader);
		}

		public void TestBN_EndorsementPlace_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_EndorsementPlaceInfo, nctsHeader);
		}

		public void TestBN_EndorsementCountryCode_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_EndorsementCountryCodeInfo, nctsHeader);
		}

		public void TestBN_EventCountryCode_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_EventCountryCodeInfo, nctsHeader);
		}

		public void TestGoodsLocation_Phase5_ReadOnly()
		{
			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				enRouteIncident.BN_CustomsStatus = "ONA";
				nctsHeader.BH_ExportFlag = "Y";
				AssertEquals("NCTS5 + Incident = 'Y' + Customs Status = 'ONA'", false, enRouteIncident.GoodsLocation.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				enRouteIncident.BN_CustomsStatus = "ONA";
				nctsHeader.BH_ExportFlag = "Y";
				AssertEquals("NCTS4 + Incident = 'Y' + Customs Status = 'ONA'", false, enRouteIncident.GoodsLocation.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_ExportFlag = "N";
				enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
				enRouteIncident.BN_CustomsStatus = "ONA";
				AssertEquals("NCTS5 + Incident = 'N' + Customs Status = 'ONA'", true, enRouteIncident.GoodsLocation.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				enRouteIncident.BN_CustomsStatus = "XXX";
				nctsHeader.BH_ExportFlag = "Y";
				AssertEquals("NCTS5 + Incident = 'Y' + Customs Status = 'XXX'", true, enRouteIncident.GoodsLocation.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_ExportFlag = "N";
				enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
				AssertEquals("NCTS5 + Incident = 'N' + Customs Status = 'AUP'", true, enRouteIncident.GoodsLocation.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				nctsHeader.BH_ExportFlag = "Y";
				AssertEquals("NCTS5 + Incident = 'Y' + Customs Status = 'ART'", true, enRouteIncident.GoodsLocation.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.BH_ExportFlag = "N";
				enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
				AssertEquals("NCTS4 + Incident = 'N' + Customs Status = 'AUP'", false, enRouteIncident.GoodsLocation.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				nctsHeader.BH_ExportFlag = "Y";
				AssertEquals("NCTS4 + Incident = 'Y' + Customs Status = 'ART'", false, enRouteIncident.GoodsLocation.ReadOnly);
			});
		}

		void AssertPhase5FieldsReadOnly(Func<EnRouteIncident, ZPropertyInfo> fieldProvider)
		{
			var field = fieldProvider.Invoke(enRouteIncident);
			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_ExportFlag = "Y";
				enRouteIncident.BN_CustomsStatus = "ONA";
				AssertEquals("NCTS5 + Incident = 'Y' + Customs Status = 'ONA'", false, field.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.BH_ExportFlag = "Y";
				enRouteIncident.BN_CustomsStatus = "ONA";
				AssertEquals("NCTS4 + Incident = 'Y' + Customs Status = 'ONA'", false, field.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_ExportFlag = "N";
				enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
				field = fieldProvider.Invoke(enRouteIncident);
				enRouteIncident.BN_CustomsStatus = "ONA";
				AssertEquals("NCTS5 + Incident = 'N' + Customs Status = 'ONA'", true, field.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_ExportFlag = "Y";
				enRouteIncident.BN_CustomsStatus = "XXX";
				AssertEquals("NCTS5 + Incident = 'Y' + Customs Status = 'XXX'", true, field.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_ExportFlag = "Y";
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
				AssertEquals("NCTS5 + Incident = 'Y' + Customs Status = 'AUP'", true, field.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.BH_ExportFlag = "Y";
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				AssertEquals("NCTS5 + Incident = 'Y' + Customs Status = 'ART'", true, field.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.BH_ExportFlag = "Y";
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
				AssertEquals("NCTS4 + Incident = 'Y' + Customs Status = 'AUP'", false, field.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.BH_ExportFlag = "Y";
				enRouteIncident.BN_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
				AssertEquals("NCTS4 + Incident = 'Y' + Customs Status = 'ART'", false, field.ReadOnly);
			});
		}

		public void TestBN_TransportAtDepartureType_Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(enRouteIncident.BN_TransportAtDepartureTypeInfo, "Transport Means Type", "Type", "Type", "Type of Identification of the Transport Means");
		}

		public void TestBN_TransportAtDepartureID_Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(enRouteIncident.BN_TransportAtDepartureIDInfo, "Transport Means Identification", "Identification", "Identification", "Identification of the Transport Means");
		}

		public void TestBN_RN_NKTransportAtDepartureIDNationality_Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(enRouteIncident.BN_RN_NKTransportAtDepartureIDNationalityInfo, "Transport Means Nationality", "Nationality", "Nationality", "Nationality of the Transport Means");
		}

		public void TestBN_RN_NKTransportAtDepartureIDNationality_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_RN_NKTransportAtDepartureIDNationalityInfo, nctsHeader);
		}

		public void TestBN_TransportAtDepartureID_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_TransportAtDepartureIDInfo, nctsHeader);
		}

		public void TestBN_TransportAtDepartureType_ReadOnly_IsArrivalNotificationDisabled()
		{
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.ONA;
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(enRouteIncident.BN_TransportAtDepartureTypeInfo, nctsHeader);
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var route = factory.NewWithValidTestData<EnRouteIncident>();
			route.BN_Type = CusInBondEventTypes.Codes.Transshipment;
			factory.Save();
			return route;
		}

		protected override BusinessObject GetNewBusinessObject() => enRouteIncident;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header.EnRouteIncidents.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ExportFlag = "Y";
			enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
		}

		NctsHeader nctsHeader;
		EnRouteIncident enRouteIncident;

		NctsHeader CreateNcts5Header()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

			return nctsHeader;
		}
	}
}
