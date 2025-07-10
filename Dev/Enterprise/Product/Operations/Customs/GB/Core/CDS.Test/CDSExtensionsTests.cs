using System;
using CargoWise.Application;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSExtensionsTests : TestCaseWithFactory
	{
		public void TestGetEori_Consol()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.SendingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
			AssertEquals("GB999999999888", consol.GetEori());
		}

		public void TestGetLatestMessageToUseForComparingAmendment()
		{
			var entry = Factory.New<CusEntryHeader>();
			var message1 = entry.Messages.AddNew();
			message1.EM_MessageType = "NEW";
			AssertEquals(message1.PK, entry.GetLatestMessageForComparison().PK);

			var message2 = entry.Messages.AddNew();
			message2.EM_MessageType = "NAM";

			var message3 = entry.Messages.AddNew();
			message3.EM_MessageType = "AMD";
			AssertEquals(message2.PK, entry.GetLatestMessageForComparison().PK);

			var message4 = entry.Messages.AddNew();
			message4.EM_MessageType = "NAM";

			var message5 = entry.Messages.AddNew();
			message5.EM_MessageType = "AMD";
			AssertEquals(message4.PK, entry.GetLatestMessageForComparison().PK);
		}

		public void TestGetApplicationCodeForMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;

			declaration.ZG_Gateway = "CDS";
			AssertEquals(EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices, entry.GetApplicationCodeForMessage());

			declaration.ZG_Gateway = "CCSUK";
			AssertEquals(EDIMessage.ApplicationCodes.GbCDSViaCCSUK, entry.GetApplicationCodeForMessage());

			declaration.ZG_Gateway = "CNS";
			AssertEquals(EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices, entry.GetApplicationCodeForMessage());
		}

		public void TestGetCredentialsKey()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "ABC",
					ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
				}
			}))
			{
				var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
				AssertEquals("HYECMT.ABC", consol.GetCredentialsKey("ABC"));
				AssertEquals("HYECMT.GB999999999888.ABC", consol.GetCredentialsKey("GB999999999888.ABC"));
				_ = consol.SendingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
				AssertEquals("HYECMT.GB999999999888.ABC", consol.GetCredentialsKey("ABC"));
				AssertEquals("HYECMT.GB999999999888.ABC", consol.GetCredentialsKey("GB999999999888.ABC"));
				AssertEquals("HYECMT.GB999999999888.GB999999999999.ABC", consol.GetCredentialsKey("GB999999999999.ABC"));
			}
		}

		public void TestGetMessageAttacheeFromMRN()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "18GBJCM3USAFD2WD51";

			var entry = dec.CustomsEntryHeaders.AddNew();
			var mrn = CusEntryNumber.LoadOrCreate(entry, "MRN", "GB");
			mrn.CE_EntryNum = "MRN123456789000-S0001000";
			mrn.CE_IssueDate = ZDate.Today;
			Factory.Save();

			var detail = new DeclarationStatusResponseDeclarationStatusDetails
			{
				Declaration = new DeclarationStatusResponseDeclarationStatusDetailsDeclaration
				{
					ID = new DeclarationIdentificationIDType1
					{
						Value = "MRN123456789000-S0001000"
					}
				}
			};

			AssertEquals(entry, detail.GetMessageAttacheeFromMRN(Factory));
		}

		public void TestGetCusEntryNumberFromLRN()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "18GBJCM3USAFD2WD51";

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "MRN123456789000-S0001000";

			var lrn = CusEntryNumber.LoadOrCreate(entry, "LRN", "GB");
			lrn.CE_EntryNum = "MRN123456789000-S0001000";
			lrn.CE_IssueDate = ZDate.Today;
			Factory.Save();

			var response = new Response
			{
				Declaration = new ResponseDeclaration
				{
					FunctionalReferenceID = new DeclarationFunctionalReferenceIDType1
					{
						Value = "MRN123456789000-S0001000"
					}
				}
			};

			AssertEquals(lrn, response.GetCusEntryNumberFromLRN(Factory));
		}

		public void TestGetCusEntryNumberFromMRN()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "18GBJCM3USAFD2WD51";

			var entry = dec.CustomsEntryHeaders.AddNew();
			var mrn = CusEntryNumber.LoadOrCreate(entry, "MRN", "GB");
			mrn.CE_EntryNum = "MRN123456789000-S0001000";
			mrn.CE_IssueDate = ZDate.Today;
			Factory.Save();

			var response = new Response
			{
				Declaration = new ResponseDeclaration
				{
					ID = new DeclarationIdentificationIDType1
					{
						Value = "MRN123456789000-S0001000"
					}
				}
			};

			AssertEquals(mrn, response.GetCusEntryNumberFromMRN(Factory));
		}

		public void TestConvertFromUnToChiefCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", isReadonly: false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.Monaco, Core.Constants.CountryCodes.France,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.SvalbardAndJanMayen, Core.Constants.CountryCodes.Norway,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			Factory.Save();

			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.Declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			AssertEquals("Empty returns Empty", ZString.Empty, entry.ConvertFromUnToChiefCountry(ZString.Empty));
			AssertEquals("Unchanged when not in mapping", Core.Constants.CountryCodes.UnitedKingdom, entry.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("MC-FR", Core.Constants.CountryCodes.France, entry.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.Monaco));
			AssertEquals("PR-US", Core.Constants.CountryCodes.UnitedStates, entry.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.PuertoRico));
			AssertEquals("SJ-NO", Core.Constants.CountryCodes.Norway, entry.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.SvalbardAndJanMayen));
		}
	}
}
