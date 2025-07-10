using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class OrgHeaderExportTest : TestCaseWithFactory
	{
		public void TestPasswordIsNotExportedFromOrganization()
		{
			var xsdGenerator = new NativeXsdGenerator();
			var definition = TestUtil.GetEntitySetDefinition("Organization");
			var schema = xsdGenerator.Generate(definition);
			var passwordElements = schema.DescendantsAndSelf().Where(element => GetElementName(element) == "Password");
			AssertEquals("Password element should exist in schema so it can be set, but should not be exported."
				, "CusBondDetail.Password, EDICommunicationsMode.Password" // CusBondDetail is probably ok, it's visible in the application.
				, string.Join(", ", passwordElements.Select((o) => GetElementName(o.Parent.Parent.Parent) + "." + GetElementName(o))));

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Freddy Fudrucker";
			contact.SetHashedPassword("PlainTextMadness");
			var cusAccount = organisation.DefermentAccountNumberCollection.AddNew();
			cusAccount.CZ_Account = "Jon Snow";
			cusAccount.CZ_Password = "You Know Nothing";
			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			string actualMessage = "";
			using (var dataStream = xmlSerializer.SerializeToStream(organisation))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			AssertContains("Precondition: Freddy Fudrucker Contact should be there", "Freddy Fudrucker", actualMessage);
			AssertNotContains("Password should not be included", "PlainTextMadness", actualMessage);

			AssertContains("Precondition: Jon Snow CusAccount should be there", "Jon Snow", actualMessage);
			AssertNotContains("Password should not be included", "You Know Nothing", actualMessage);
		}

		static string GetElementName(XElement element)
		{
			return element.Attributes().FirstOrDefault(o => o.Name.LocalName == "name")?.Value;
		}

		public void TestSchemaDoesNotHaveCreditCardDetailsInIt()
		{
			var xsdGenerator = new NativeXsdGenerator();
			var definition = TestUtil.GetEntitySetDefinition("Organization");
			var schema = xsdGenerator.Generate(definition);
			var creditCardElements = schema.DescendantsAndSelf().Where(element => element.Attributes().Any(o => o.Name.LocalName == "name" && o.Value.Contains("CreditCard")));
			if (creditCardElements.Any())
			{
				Fail("Found Credit Card Elements - SECURITY BREACH!!\r\n" + string.Join("\r\n", creditCardElements.Select(o => o.ToString())));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestIncludesAPAccountDetails()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "FRED SIMPSON";

			var companyData = orgHeader.CompanyData;
			companyData.OB_IsCreditor = true;

			var apAccountDetails1 = companyData.AccountDetailsCollection.AddNew();
			apAccountDetails1.A1_AccountName = "JOHN SAMPLE";

			var apAccountDetails2 = companyData.AccountDetailsCollection.AddNew();
			apAccountDetails2.A1_AccountName = "TIM SIMPLE";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 APAccountDetailsCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "APAccountDetailsCollection"));
					AssertEquals("Should have 2 APAccountDetails Elements.", 2, element.Descendants().Count(e => e.Name.LocalName == "APAccountDetails"));
				});
			}
		}

		public void TestJobRequiredDocument()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "FRED SIMPSON";

			var jobRequiredDocument = orgHeader.RequiredDocuments.AddNew();
			jobRequiredDocument.EQ_DocCategory = "XXX";

			jobRequiredDocument = orgHeader.RequiredDocuments.AddNew();
			jobRequiredDocument.EQ_DocCategory = "YYY";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 JobRequiredDocument Collection.", 1, element.Descendants().Count(e => e.Name.LocalName == "JobRequiredDocumentCollection"));
					AssertEquals("Should have 2 JobRequiredDocument Elements.", 2, element.Descendants().Count(e => e.Name.LocalName == "JobRequiredDocument"));
				});
			}
		}

		public void TestCusBondDetail()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "FRED SIMPSON";

			var bondDetails = new CusBondDetailCollection(orgHeader);
			var bondDetail = bondDetails.AddNew();
			bondDetail.PW_BondNumber = "123456";

			bondDetail = bondDetails.AddNew();
			bondDetail.PW_BondNumber = "123457";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 CusBondDetail Collection.", 1, element.Descendants().Count(e => e.Name.LocalName == "CusBondDetailCollection"));
					AssertEquals("Should have 2 CusBondDetail Elements.", 2, element.Descendants().Count(e => e.Name.LocalName == "CusBondDetail"));
				});
			}
		}

		public void TestNotes()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "FRED SIMPSON";

			var noteCustom = orgHeader.Notes.AddNew();
			noteCustom.ST_IsCustomDescription = true;
			noteCustom.ST_Description = "Customs Note Description";
			noteCustom.ST_NoteContextModuleCaption = "C - CFS";
			noteCustom.ST_NoteContextDirectionCaption = "E - Export";
			noteCustom.ST_NoteContextFreightModeCaption = "L - LCL";
			noteCustom.ST_NoteDataAsText = "String Of Data somehow relating to this test";

			var note = orgHeader.Notes.AddNew();
			note.ST_IsCustomDescription = false;
			note.ST_Description = "Special Instructions";
			note.ST_NoteContextModuleCaption = "D - Customs/Declarations";
			note.ST_NoteContextDirectionCaption = "X - Cross Trade";
			note.ST_NoteContextFreightModeCaption = "W - Rail";
			note.ST_NoteText = "Special Instructions Note Text Field";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				AssertEquals("Should have 1 StmNoteCollection Collection.", 1, element.Descendants().Count(e => e.Name.LocalName == "StmNoteCollection"));
				AssertEquals("Should have 2 StmNote Elements.", 2, element.Descendants().Count(e => e.Name.LocalName == "StmNote"));

				var notes = element.Descendants().Where(e => e.Name.LocalName == "StmNote").ToArray();
				var note1 = notes[0];
				AssertEquals("Customs Note Description", note1.Descendants().First(e => e.Name.LocalName == "Description").Value);
				AssertEquals("String Of Data somehow relating to this test", ORtfTextUtil.Base64RtfToText(note1.Descendants().First(e => e.Name.LocalName == "NoteData").Value));
				AssertEquals("", note1.Descendants().First(e => e.Name.LocalName == "NoteText").Value);
				AssertEquals("INT", note1.Descendants().First(e => e.Name.LocalName == "NoteType").Value);
				AssertEquals("CEL", note1.Descendants().First(e => e.Name.LocalName == "NoteContext").Value);
				AssertEquals("true", note1.Descendants().First(e => e.Name.LocalName == "IsCustomDescription").Value);

				var note2 = notes[1];
				AssertEquals("Special Instructions", note2.Descendants().First(e => e.Name.LocalName == "Description").Value);
				AssertEquals("", note2.Descendants().First(e => e.Name.LocalName == "NoteData").Value);
				AssertEquals("Special Instructions Note Text Field", note2.Descendants().First(e => e.Name.LocalName == "NoteText").Value);
				AssertEquals("PUB", note2.Descendants().First(e => e.Name.LocalName == "NoteType").Value);
				AssertEquals("DXW", note2.Descendants().First(e => e.Name.LocalName == "NoteContext").Value);
				AssertEquals("false", note2.Descendants().First(e => e.Name.LocalName == "IsCustomDescription").Value);
			}
		}

		public void TestCFXConfigurations()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";

			var acc1 = orgHeader.CompanyData.AccCFXConfigurations.AddNew();
			acc1.JCF_CFXPercentage = 8;
			acc1.JCF_RX_NKCurrency = "USD";
			acc1.JCF_ServiceDirection = "IMP";
			acc1.JCF_TransportMode = "AIR";
			AssertEquals("Should belong to current company", GlbCompany.CurrentCompany.PK, acc1.JCF_GC);

			var acc2 = orgHeader.CompanyData.AccCFXConfigurations.AddNew();
			acc2.JCF_CFXPercentage = 5;
			acc2.JCF_RX_NKCurrency = "USD";
			acc2.JCF_ServiceDirection = "EXP";
			acc2.JCF_TransportMode = "SEA";
			AssertEquals("Should belong to current company", GlbCompany.CurrentCompany.PK, acc2.JCF_GC);

			var otherCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), otherCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var acc3 = orgHeader.CompanyData.AccCFXConfigurations.AddNew();
				acc3.JCF_CFXPercentage = 15;
				acc3.JCF_RX_NKCurrency = "USD";
				acc3.JCF_ServiceDirection = "IMP";
				acc3.JCF_TransportMode = "SEA";
				AssertEquals("Should belong to other company", otherCompanyBranch.GB_GC, acc3.JCF_GC);

				orgHeader.CompanyData.OB_ARCreditApproved = !orgHeader.CompanyData.OB_ARCreditApproved;
				Assert("Should be saved to DB.", orgHeader.CompanyData.HasChanges);
			}

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 2 OrgCompanyData.", 2, element.Descendants().Count(e => e.Name.LocalName == "OrgCompanyData"));

					AssertEquals("Should have 1 AccCFXConfigurations.", 1, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationViewCollection"));
					AssertEquals("Should have 3 AccCFXConfiguration.", 3, element.Descendants().Count(e => e.Name.LocalName == "AccCFXUpliftConfigurationView"));
				});
			}
		}

		public void TestJobBillingExRateConfigurations()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";

			var apConfig = orgHeader.CompanyData.AccAPExchangeRateConfigurations.AddNew();
			apConfig.JCE_Preference = "TDR";
			apConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = "BUY";
			apConfig.JCE_ServiceDirection = "IMP";
			apConfig.JCE_TransportMode = "AIR";
			AssertEquals("Should belong to current company", GlbCompany.CurrentCompany.PK, apConfig.JCE_GC);

			var arConfig = orgHeader.CompanyData.AccAPExchangeRateConfigurations.AddNew();
			arConfig.JCE_Preference = "TDR";
			arConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = "SEL";
			arConfig.JCE_ServiceDirection = "EXP";
			arConfig.JCE_TransportMode = "SEA";
			AssertEquals("Should belong to current company", GlbCompany.CurrentCompany.PK, arConfig.JCE_GC);

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 OrgCompanyData.", 1, element.Descendants().Count(e => e.Name.LocalName == "OrgCompanyData"));
					AssertEquals("Should have 1 AccExchangeRateConfigurationViewCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "AccExchangeRateConfigurationViewCollection"));
					AssertEquals("Should have 2 AccExchangeRateConfigurationView.", 2, element.Descendants().Count(e => e.Name.LocalName == "AccExchangeRateConfigurationView"));
				});
			}
		}

		public void TestOrgInvTypeDeferredCharges()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "FRED SIMPSON";

			var companyData = orgHeader.CompanyData;
			companyData.OB_IsDebtor = true;

			var invoiceType = Factory.NewWithValidTestData<OrgInvoiceType>();
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			var charge1 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			charge1.PO_PI = invoiceType.PK;
			charge1.PO_ChargeGroup = "CDS";
			var charge2 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			charge2.PO_PI = invoiceType.PK;
			charge2.PO_ChargeGroup = "BRK";
			var collection = new OrgInvTypeDeferredChargesCollection(Factory);
			collection.Add(charge1);
			collection.Add(charge2);

			orgHeader.CompanyData.InvoiceTypes.Add(invoiceType);

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 OrgInvTypeDeferredChargesCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "OrgInvTypeDeferredChargesCollection"));
					AssertEquals("Should have 2 OrgInvTypeDeferredCharges.", 2, element.Descendants().Count(e => e.Name.LocalName == "OrgInvTypeDeferredCharges"));
				});
			}
		}

		public void TestSerializeToStream_WhenGettingAddressWithSuppressValidationError_ShouldNotIncludeThatProperty()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.MainAddress.OA_SuppressAddressValidationError = true;

			var deliveryAddress = header.Addresses.AddNew(OrgAddressType.Delivery, ZBool.False);
			deliveryAddress.OA_Address1 = "[_MOCK_DELIVERY_ADDRESS_]";
			deliveryAddress.OA_SuppressAddressValidationError = false;

			Factory.Save();

			var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter { DefinitionFinder = definitionFinder };
			var serializer = new NativeXmlSerializer { Converter = converter };

			var xmlns = (XNamespace)NativeXmlInfo.Namespace_2011_11;

			using (var stream = serializer.SerializeToStream(header))
			{
				stream.Position = 0;

				AssertNotEquals("Serialized XML must not be empty.", 0, stream.Length);

				var addresses = XDocument
					.Load(stream)
					.Descendants(xmlns + nameof(OrgAddress))
					.ToArray();

				AssertEquals("Should find 2 addresses from XML.", 2, addresses.Length);

				var hasSuppressionFlag = addresses
					.SelectMany(address => address.Descendants(xmlns + "SuppressAddressValidationError"))
					.Any();

				AssertEquals("Should not include <SuppressAddressValidationError> flag.", false, hasSuppressionFlag);
			}
		}

		public void TestIncludesTranslatedAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "UNIT 1, 1ST FLOOR";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var translatedAddress = Factory.New<OrgTranslatedAddress>();
			address1.TranslatedAddresses.Add(translatedAddress);
			translatedAddress.Address1 = "一楼一单元";
			translatedAddress.Language = Core.SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				var translatedAddresses = element.Descendants().Where(e => e.Name.LocalName == "OrgTranslatedAddress").ToArray();
				AssertEquals("一楼一单元", translatedAddresses.Descendants().First(e => e.Name.LocalName == "Address1").Value);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, translatedAddresses.Descendants().First(e => e.Name.LocalName == "Language").Value);
			}
		}

		public void TestIncludesEDICommunicationsMode()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var ediCommunicationsMode = orgHeader.EDICommunicationsModes.AddNew();
			ediCommunicationsMode.EK_Module = "BKN";
			ediCommunicationsMode.EK_MessagePurpose = "EVT";
			ediCommunicationsMode.EK_CommsDirection = "TRX";
			ediCommunicationsMode.EK_FileFormat = "XUE";
			ediCommunicationsMode.EK_CommunicationsTransport = "HUB";
			ediCommunicationsMode.EK_Destination = "EDI9999";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 EDICommunicationsModeCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "EDICommunicationsModeCollection"));
					AssertEquals("Should have 2 EDICommunicationsMode Elements.", 1, element.Descendants().Count(e => e.Name.LocalName == "EDICommunicationsMode"));
				});
			}
		}

		public void TestIncludesGlbGroupLink()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "TestOC";
			Factory.Save();

			var groupOrg = Factory.New<GlbGroup>();
			var groupOC = Factory.New<GlbGroup>();
			groupOrg.GG_Code = "OrgGroup";
			groupOC.GG_Code = "OCGroup";

			var orgLink = Factory.New<GlbGroupOrgLink>();
			orgLink.GOK_GG_Group = groupOrg.PK;
			orgLink.GOK_OH_Org = orgHeader.PK;

			var orgLink2 = Factory.New<GlbGroupOrgLink>();
			orgLink2.GOK_GG_Group = groupOC.PK;
			orgLink2.GOK_OH_Org = orgHeader.PK;

			var ocLink = Factory.New<GlbGroupOrgContactLink>();
			ocLink.GCK_GG_Group = groupOC.PK;
			ocLink.GCK_OC_Contact = contact.PK;
			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);
				var orgLinkElement = element.Descendants().Where(x => x.Name.LocalName.Equals("GlbGroupOrgLinkCollection"));
				var contactLinkElement = element.Descendants().Where(x => x.Name.LocalName.Equals("GlbGroupOrgContactLinkCollection"));

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 GlbGroupOrgLinkCollection element", 1, orgLinkElement.Count());
					AssertEquals("Should have 1 GlbGroupOrgContactLinkCollection element", 1, contactLinkElement.Count());
					AssertEquals("Should have 3 Group elements(Org:2; Contact:1).", 3, element.Descendants().Count(e => e.Name.LocalName == "Group"));
					AssertEquals("GlbGroupOrgLinkCollection should contain 2 Group element", 2, orgLinkElement.First().Descendants().Count(x => x.Name.LocalName == "Group"));
					AssertEquals("GlbGroupOrgContactLinkCollection should contain 1 Group element", 1, contactLinkElement.First().Descendants().Count(x => x.Name.LocalName == "Group"));
				});
			}
		}

		public void TestDoesNotIncludeOrgPartyScreeningStatus()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var stmEntityScreeningLog = Factory.New<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_SourceID = orgHeader.PK;
			stmEntityScreeningLog.PJ_SourceTableCode = OrgHeaderSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_ParentID = orgHeader.PK;
			stmEntityScreeningLog.PJ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 0 OrgPartyScreeningStatusCollection.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgPartyScreeningStatusCollection"));
					AssertEquals("Should have 0 OrgPartyScreeningStatus Elements.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgPartyScreeningStatus"));
					AssertEquals("Should have 0 StmEntityScreeningLogCollection.", 0, element.Descendants().Count(e => e.Name.LocalName == "StmEntityScreeningLogCollection"));
					AssertEquals("Should have 0 StmEntityScreeningLog Elements.", 0, element.Descendants().Count(e => e.Name.LocalName == "StmEntityScreeningLog"));
				});
			}
		}

		public void TestOrgCompetitor()
		{
			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var company = Factory.NewWithValidTestData<GlbCompany>();

			var orgCompetitor = orgHeader.Competitors.AddNew();
			orgCompetitor.OCP_Type = "TST";
			orgCompetitor.OCP_OH_Competitor = orgHeader2.PK;

			var orgCompetitor2 = orgHeader.Competitors.AddNew();
			orgCompetitor2.OCP_Type = "CMB";
			orgCompetitor2.OCP_OH_Competitor = orgHeader2.PK;

			var orgCompetitor3 = orgHeader.Competitors.AddNew();
			orgCompetitor3.OCP_Type = "COM";
			orgCompetitor3.OCP_OH_Competitor = orgHeader2.PK;
			orgCompetitor3.OCP_GC_Company = company.PK;

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			using (var reader = new XmlTextReader(dataStream))
			{
				var element = XElement.Load(reader);

				CombineAssertions(delegate
				{
					AssertEquals("Should have 1 OrgCompetitorCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "OrgCompetitorCollection"));
					AssertEquals("Should have 3 OrgCompetitor.", 3, element.Descendants().Count(e => e.Name.LocalName == "OrgCompetitor"));
					AssertEquals("Should have 1 OrgCompetitor with TST type.", 1, element.Descendants().Count(e => e.Value == "TST"));
					AssertEquals("Should have 1 OrgCompetitor with CMB type.", 1, element.Descendants().Count(e => e.Value == "CMB"));
					AssertEquals("Should have 1 OrgCompetitor with COM type.", 1, element.Descendants().Count(e => e.Value == "COM"));
				});
			}
		}
	}
}
