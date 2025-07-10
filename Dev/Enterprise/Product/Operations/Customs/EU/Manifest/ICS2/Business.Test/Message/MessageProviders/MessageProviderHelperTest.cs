using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class MessageProviderHelperTest : TestCaseWithFactory
	{
		public void TestGetAdditionalInformationCollection()
		{
			var additionalInfo1 = Factory.New<CusSupportingInfo>();
			var additionalInfo2 = Factory.New<CusSupportingInfo>();

			var additionalInfoList = new List<CusSupportingInfo>();
			additionalInfoList.AddRange([additionalInfo1, additionalInfo2]);

			var additionalInformationCollection = MessageProviderHelper.GetAdditionalInformationCollection(additionalInfoList);

			AssertEquals("Number of entries", 2, additionalInformationCollection.Count);

			AssertType<AdditionalInformationProvider>(additionalInformationCollection.First());
		}

		public void TestGetSupportingDocuments()
		{
			var additionalInfo1 = Factory.New<SupportingDocument>();
			var additionalInfo2 = Factory.New<SupportingDocument>();

			var supportingDocument = new List<SupportingDocument>();
			supportingDocument.AddRange([additionalInfo1, additionalInfo2]);

			var supportingDocumentCollection = MessageProviderHelper.GetSupportingDocuments(supportingDocument);

			AssertEquals("Number of entries", 2, supportingDocumentCollection.Count);

			AssertType<SupportingDocumentProvider>(supportingDocumentCollection.First());
		}

		public void TestGetFirstAdditionalFiscalReference()
		{
			var fiscalReference1 = Factory.New<AdditionalFiscalReference>();
			var fiscalReference2 = Factory.New<AdditionalFiscalReference>();
			fiscalReference1.CFR_Reference = "FR1";
			fiscalReference2.CFR_Reference = "FR2";

			var fiscalReference = new List<AdditionalFiscalReference>();
			fiscalReference.AddRange([fiscalReference1, fiscalReference2]);

			var firstAdditionalFiscalReference = MessageProviderHelper.GetFirstAdditionalFiscalReference(fiscalReference);

			AssertEquals("First Fiscal Reference", firstAdditionalFiscalReference.Identifier, fiscalReference.FirstOrDefault().CFR_Reference);

			AssertType<AdditionalFiscalReferenceProvider>(firstAdditionalFiscalReference);
		}

		public void TestGetICS2SenderEORI()
		{
			var memberState1Config = Factory.New<RefSysConfig>();
			memberState1Config.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SID;
			memberState1Config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			memberState1Config.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			memberState1Config.ZRC_StringValue = "ICSSenderEORI";

			var memberState1Config2 = Factory.New<RefSysConfig>();
			memberState1Config2.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
			memberState1Config2.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			memberState1Config2.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			memberState1Config2.ZRC_StringValue = "ICS2SMS1";

			AssertEquals("ICSSenderEORI", MessageProviderHelper.GetICS2SenderEORI(Factory));
		}

		public void TestGetICS2SenderMemberState()
		{
			CombineAssertions(() =>
			{
				using (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var idConfig = Factory.New<RefSysConfig>();
					idConfig.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
					idConfig.ZRC_StartDate = new ZDateTime(1900, 01, 01);
					idConfig.ZRC_EndDate = new ZDateTime(2079, 06, 06);
					idConfig.ZRC_StringValue = "ICS2SenderMemberState1";

					AssertEquals("IncludeMemberStateInSiteID=false", ZString.Empty, MessageProviderHelper.GetICS2SenderMemberState(Factory, null));
				}

				using (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var factory1 = new BusinessObjectFactory();

					var idConfigF1 = factory1.New<RefSysConfig>();
					idConfigF1.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
					idConfigF1.ZRC_StartDate = new ZDateTime(1900, 01, 01);
					idConfigF1.ZRC_EndDate = new ZDateTime(2079, 06, 06);
					idConfigF1.ZRC_StringValue = "ICS2SenderMemberState1";

					AssertEquals("ICS2SenderMemberState1", MessageProviderHelper.GetICS2SenderMemberState(factory1, null));

					var factory2 = new BusinessObjectFactory();

					var idConfigF2 = factory2.New<RefSysConfig>();
					idConfigF2.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS2;
					idConfigF2.ZRC_StartDate = new ZDateTime(1900, 01, 01);
					idConfigF2.ZRC_EndDate = new ZDateTime(2079, 06, 06);
					idConfigF2.ZRC_StringValue = "ICS2SenderMemberState2";

					var idConfig2F2 = factory2.New<RefSysConfig>();
					idConfig2F2.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
					idConfig2F2.ZRC_StartDate = new ZDateTime(1900, 01, 01);
					idConfig2F2.ZRC_EndDate = new ZDateTime(2079, 06, 06);
					idConfig2F2.ZRC_StringValue = "ICS2SenderMemberState1";

					AssertEquals("ICS2SenderMemberState2", MessageProviderHelper.GetICS2SenderMemberState(factory2, null));
				}
			});
		}

		public void TestGetICS2SystemID()
		{
			CombineAssertions(() =>
			{
				var factory1 = Factory;

				var idConfigF1 = factory1.New<RefSysConfig>();
				idConfigF1.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
				idConfigF1.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfigF1.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfigF1.ZRC_StringValue = "WTG";

				var idConfig2F1 = factory1.New<RefSysConfig>();
				idConfig2F1.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SSYS;
				idConfig2F1.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfig2F1.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfig2F1.ZRC_StringValue = "ICS2SystemIDWTG";

				AssertEquals("ICS2SystemIDWTG", MessageProviderHelper.GetICS2SystemID(factory1));

				var factory2 = new BusinessObjectFactory();

				var idConfigF2 = factory2.New<RefSysConfig>();
				idConfigF2.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
				idConfigF2.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfigF2.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfigF2.ZRC_StringValue = "NO";

				AssertEquals(ZString.Empty, MessageProviderHelper.GetICS2SystemID(factory2));

				var factory3 = new BusinessObjectFactory();

				var idConfigF3 = factory3.New<RefSysConfig>();
				idConfigF3.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
				idConfigF3.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfigF3.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfigF3.ZRC_StringValue = "XXX";

				AssertEquals($"{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}", MessageProviderHelper.GetICS2SystemID(factory3));
			});
		}

		public void TestGetFromParty()
		{
			using (ICS2CustomsDataRegistry.Instance.SenderPartyId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SenderPartyID"))
			{
				AssertEquals("SenderPartyID", MessageProviderHelper.GetFromParty(Factory, null));
			}

			using (ICS2CustomsDataRegistry.Instance.SenderPartyId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory1 = new BusinessObjectFactory();

				var idConfigF1 = factory1.New<RefSysConfig>();
				idConfigF1.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
				idConfigF1.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfigF1.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfigF1.ZRC_StringValue = "ICS2SenderMemberState1";

				var idConfig2F1 = factory1.New<RefSysConfig>();
				idConfig2F1.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
				idConfig2F1.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfig2F1.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfig2F1.ZRC_StringValue = "NO";

				AssertEquals("ICS2SenderMemberState1", MessageProviderHelper.GetFromParty(factory1, null));

				var factory2 = new BusinessObjectFactory();

				var memberState1Config = factory2.New<RefSysConfig>();
				memberState1Config.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SID;
				memberState1Config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				memberState1Config.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				memberState1Config.ZRC_StringValue = "ICSSenderEORI";

				var idConfig2F2 = factory2.New<RefSysConfig>();
				idConfig2F2.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
				idConfig2F2.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfig2F2.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfig2F2.ZRC_StringValue = "NO";

				AssertEquals("ICSSenderEORI", MessageProviderHelper.GetFromParty(factory2, null));

				var factory3 = new BusinessObjectFactory();

				var idConfigF3 = factory3.New<RefSysConfig>();
				idConfigF3.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
				idConfigF3.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfigF3.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfigF3.ZRC_StringValue = "WTG";

				var idConfig2F3 = factory3.New<RefSysConfig>();
				idConfig2F3.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SSYS;
				idConfig2F3.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfig2F3.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfig2F3.ZRC_StringValue = "ICS2SystemIDWTG";

				AssertEquals("ICS2SystemIDWTG", MessageProviderHelper.GetFromParty(factory3, null));

				var factory4 = new BusinessObjectFactory();

				var idConfigF4 = factory4.New<RefSysConfig>();
				idConfigF4.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
				idConfigF4.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfigF4.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfigF4.ZRC_StringValue = "ICS2SenderMemberState1";

				var idConfig2F4 = factory4.New<RefSysConfig>();
				idConfig2F4.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
				idConfig2F4.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfig2F4.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfig2F4.ZRC_StringValue = "WTG";

				var idConfig3F4 = factory4.New<RefSysConfig>();
				idConfig3F4.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SID;
				idConfig3F4.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfig3F4.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfig3F4.ZRC_StringValue = "ICSSenderEORI";

				var idConfig4F4 = factory4.New<RefSysConfig>();
				idConfig4F4.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SSYS;
				idConfig4F4.ZRC_StartDate = new ZDateTime(1900, 01, 01);
				idConfig4F4.ZRC_EndDate = new ZDateTime(2079, 06, 06);
				idConfig4F4.ZRC_StringValue = "ICS2SystemIDWTG";

				AssertEquals("ICS2SystemIDWTG@ICSSenderEORI@ICS2SenderMemberState1", MessageProviderHelper.GetFromParty(factory4, null));
			}
		}

		public void TestToNullableDateTime_ZDateTime()
		{
			var testZDateTime = new ZDateTime(2022, 12, 23, 08, 35, 51, DateTimeKind.Utc);
			AssertEquals("Valid input", new DateTime(2022, 12, 23, 08, 35, 51), testZDateTime.ToNullableDateTime());
			AssertEquals("Invalid input", null, ZDateTime.Empty.ToNullableDateTime());
		}

		public void TestToNullableDateTime_ZDateTimeOffset()
		{
			var expected = new DateTime(2022, 12, 23, 08, 35, 51);
			var testZDateTimeOffset = new ZDateTimeOffset(expected, DateTimeKind.Utc);
			AssertEquals("Valid input", expected, testZDateTimeOffset.ToNullableDateTime());
			AssertEquals("Invalid input", null, ZDateTimeOffset.Empty.ToNullableDateTime());
		}

		public void TestUtcDateTime()
		{
			CombineAssertions(() =>
			{
				var testZDateTime = new ZDateTime(2022, 12, 23, 08, 35, 51, DateTimeKind.Utc);
				AssertEquals("Valid input", new DateTime(2022, 12, 23, 08, 35, 51, DateTimeKind.Utc), testZDateTime.UtcDateTime());
				AssertEquals("Invalid input", default(DateTime), ZDateTime.Empty.UtcDateTime());
			});
		}

		public void TestToArray()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AdditionalInfos.AddNew();
			manifestHeader.AdditionalInfos.AddNew();
			manifestHeader.Containers.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("2 AdditionalInfos", 2, manifestHeader.AdditionalInfos.ToArray<AdditionalInfo, IAdditionalInformation>(c => new AdditionalInformationProvider(c)).Count);
				AssertEquals("1 Container", 1, manifestHeader.Containers.ToArray<AsycudaContainer, ITransportEquipment>(TransportEquipmentProvider.NewOrNull).Count);
			});
		}

		public void TestGetCustomsMessageType()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F10, MessageTypes.Codes.A10, MessageTypes.Codes.F10);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F11, MessageTypes.Codes.A11, MessageTypes.Codes.F11);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F12, MessageTypes.Codes.A12, MessageTypes.Codes.F12);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F13, MessageTypes.Codes.A13, MessageTypes.Codes.F13);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F14, MessageTypes.Codes.A14, MessageTypes.Codes.F14);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F15, MessageTypes.Codes.A15, MessageTypes.Codes.F15);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F16, MessageTypes.Codes.A16, MessageTypes.Codes.F16);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F17, MessageTypes.Codes.A17, MessageTypes.Codes.F17);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F20, MessageTypes.Codes.A20, MessageTypes.Codes.F20);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F21, MessageTypes.Codes.A21, MessageTypes.Codes.F21);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F22, MessageTypes.Codes.A22, MessageTypes.Codes.F22);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F23, MessageTypes.Codes.A23, MessageTypes.Codes.F23);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F24, MessageTypes.Codes.A24, MessageTypes.Codes.F24);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F25, MessageTypes.Codes.F25, MessageTypes.Codes.F25);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F26, MessageTypes.Codes.A26, MessageTypes.Codes.F26);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F27, MessageTypes.Codes.A27, MessageTypes.Codes.F27);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F28, MessageTypes.Codes.A28, MessageTypes.Codes.F28);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F29, MessageTypes.Codes.A29, MessageTypes.Codes.F29);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F30, MessageTypes.Codes.A30, MessageTypes.Codes.F30);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F31, MessageTypes.Codes.A31, MessageTypes.Codes.F31);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F32, MessageTypes.Codes.A32, MessageTypes.Codes.F32);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F33, MessageTypes.Codes.A33, MessageTypes.Codes.F33);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F34, MessageTypes.Codes.A34, MessageTypes.Codes.F34);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F40, MessageTypes.Codes.A40, MessageTypes.Codes.F40);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F41, MessageTypes.Codes.A41, MessageTypes.Codes.F41);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F42, MessageTypes.Codes.A42, MessageTypes.Codes.F42);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F43, MessageTypes.Codes.A43, MessageTypes.Codes.F43);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F44, MessageTypes.Codes.A44, MessageTypes.Codes.F44);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F45, MessageTypes.Codes.A45, MessageTypes.Codes.F45);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F50, MessageTypes.Codes.A50, MessageTypes.Codes.F50);
				CheckCustomsMessageType(manifestHeader, EUICS2SpecificCircumstanceList.Codes.F51, MessageTypes.Codes.A51, MessageTypes.Codes.F51);
				CheckCustomsMessageType(manifestHeader, "XXX", "XXX", "XXX");
			});
		}

		void CheckCustomsMessageType(AsycudaManifestHeader manifestHeader, ZString specificCircumstanceIndicator, ZString messageTypeIsAmend, ZString messageTypeIsNotAmend)
		{
			manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
			AssertEquals($"indicator={specificCircumstanceIndicator}, isAmend", messageTypeIsAmend, MessageProviderHelper.GetCustomsMessageType(manifestHeader, true));
			AssertEquals($"indicator={specificCircumstanceIndicator}, isNotAmend", messageTypeIsNotAmend, MessageProviderHelper.GetCustomsMessageType(manifestHeader, false));
		}
	}
}
