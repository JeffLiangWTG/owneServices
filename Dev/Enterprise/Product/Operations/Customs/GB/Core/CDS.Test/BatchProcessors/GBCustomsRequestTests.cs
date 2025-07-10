using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class GBCustomsRequestTests : TestCaseWithFactory
	{
		public void TestNew_Consol()
		{
			CreateLicenceKey();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				foreach (var gateway in CDSMessageSenderTestHelper.GetGateways().Where(x => !x.IsEmpty))
				{
					using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, CreateTemporaryRegistryBadge(gateway)))
					using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateTemporaryRegistryBadgeCredential()))
					{
						var consol = Factory.NewWithValidTestData<ForwardingConsol>();
						consol.JK_TransportMode = Core.Constants.TransportModes.Air;
						consol.JK_MasterBillNum = "MUCR";
						consol.JK_RL_NKLoadPort = "GBLHR";
						consol.JK_RL_NKDischargePort = "AUSYD";
						var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
						sendingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
						consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

						var wrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
						wrapper.MawbExportHelper.ME_Profile = "ABC";

						var inventoryLinkingConsolidationMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						inventoryLinkingConsolidationMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						inventoryLinkingConsolidationMessage.EM_MessageNum = "4";
						inventoryLinkingConsolidationMessage.EM_MessageOwner = "ABC";
						var inventoryLinkingMovementMessage = (CDSInventoryLinkingMovementRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingMovementRequestEDIMessage));
						inventoryLinkingMovementMessage.EM_MessageText = "<MetaData><inventoryLinkingMovementRequest/></MetaData>";
						inventoryLinkingMovementMessage.EM_MessageNum = "5";
						inventoryLinkingMovementMessage.EM_MessageOwner = "ABC";
						var inventoryLinkingQueryMessage = (CDSInventoryLinkingQueryRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingQueryRequestEDIMessage));
						inventoryLinkingQueryMessage.EM_MessageText = "<MetaData><inventoryLinkingQueryRequest/></MetaData>";
						inventoryLinkingQueryMessage.EM_MessageNum = "6";
						inventoryLinkingQueryMessage.EM_MessageOwner = "ABC";

						var providerType = GBCustomsRequestFactory.GetProviderType(gateway);
						var credentials = CreateTemporaryCredentials(gateway, "HYECMT.GB999999999888.ABC", "Username", "Password", "CUKFFW98000ZPE", "Role", string.Empty);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingConsolidationMessage), (CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage)), providerType, consol.JK_UniqueConsignRef, credentials);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingMovementMessage), (CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingMovementRequestEDIMessage)), providerType, consol.JK_UniqueConsignRef, credentials);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingQueryMessage), (CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingQueryRequestEDIMessage)), providerType, consol.JK_UniqueConsignRef, credentials);
					}
				}
			}
		}

		public void TestNew_Consol_CDSILEPhase2()
		{
			CreateLicenceKey();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				foreach (var gateway in CDSMessageSenderTestHelper.GetGateways().Where(x => !x.IsEmpty))
				{
					using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, CreateTemporaryRegistryBadge(gateway)))
					using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateTemporaryRegistryBadgeCredential()))
					{
						var consol = Factory.NewWithValidTestData<ForwardingConsol>();
						consol.JK_TransportMode = Core.Constants.TransportModes.Air;
						consol.JK_MasterBillNum = "MUCR";
						consol.JK_RL_NKLoadPort = "GBLHR";
						consol.JK_RL_NKDischargePort = "AUSYD";
						var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
						sendingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
						consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

						var wrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
						wrapper.MawbExportHelper.ME_Profile = "ABC";

						var inventoryLinkingConsolidationMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						inventoryLinkingConsolidationMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						inventoryLinkingConsolidationMessage.EM_MessageNum = "4";
						inventoryLinkingConsolidationMessage.EM_MessageOwner = "ABC";
						var inventoryLinkingMovementMessage = (CDSInventoryLinkingMovementRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingMovementRequestEDIMessage));
						inventoryLinkingMovementMessage.EM_MessageText = "<MetaData><inventoryLinkingMovementRequest/></MetaData>";
						inventoryLinkingMovementMessage.EM_MessageNum = "5";
						inventoryLinkingMovementMessage.EM_MessageOwner = "ABC";
						var inventoryLinkingQueryMessage = (CDSInventoryLinkingQueryRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingQueryRequestEDIMessage));
						inventoryLinkingQueryMessage.EM_MessageText = "<MetaData><inventoryLinkingQueryRequest/></MetaData>";
						inventoryLinkingQueryMessage.EM_MessageNum = "6";
						inventoryLinkingQueryMessage.EM_MessageOwner = "ABC";

						var providerType = GBCustomsRequestFactory.GetProviderType(gateway);
						var credentials = CreateTemporaryCredentials(gateway, "HYECMT.GB999999999888.ABC", "Username", "Password", "CUKFFW98000ZPE", "Role", string.Empty);

						if (gateway == GatewayList.Codes.MCP_CUSDECOnly
							|| gateway == GatewayList.Codes.CNS_CUSDECOnly
							|| gateway == GatewayList.Codes.Pentant)
						{
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingConsolidationMessage), ServiceType.ExportInventoryConsolidation, providerType, consol.JK_UniqueConsignRef, credentials);

							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingMovementMessage), ServiceType.ExportInventoryMovement, providerType, consol.JK_UniqueConsignRef, credentials);

							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingQueryMessage), ServiceType.ExportInventoryQuery, providerType, consol.JK_UniqueConsignRef, credentials);
						}
						else
						{
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingConsolidationMessage), ServiceType.ExportInventory, providerType, consol.JK_UniqueConsignRef, credentials);

							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingMovementMessage), ServiceType.ExportInventory, providerType, consol.JK_UniqueConsignRef, credentials);

							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(inventoryLinkingQueryMessage), ServiceType.ExportInventory, providerType, consol.JK_UniqueConsignRef, credentials);
						}
					}
				}
			}
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestCredentialPasswordIsEncrypted()
		{
			CDSMessageSenderTestHelper.TestProcess(null, (entry, decRef) =>
			{
				var providerType = GBCustomsRequestFactory.GetProviderType(entry.Declaration.ZG_Gateway);
				var credentials = CreateTemporaryCredentials(entry.Declaration.ZG_Gateway, "HYECMT.GB999999999888.ABC", "Username", "Password", "Location", "Role", string.Empty);
				var encryptedPassword = GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single()).Credentials.Password;
				if (providerType == ProviderType.Direct)
				{
					AssertEquals("Encrypted password not used", null, encryptedPassword);
				}
				else
				{
					CombineAssertions("Encrypted password expected", () =>
					{
						AssertNotEquals("Encrypted password cannot be null", null, encryptedPassword);
						AssertNotEquals("Encrypted password cannot be empty", string.Empty, encryptedPassword);
						AssertEquals("Encrypted password longer than original ", true, encryptedPassword.Length > 8);
						AssertNotEquals("Encrypted password not the same as the original", "Password", encryptedPassword);
					});
				}
			});
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestNew()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				CDSMessageSenderTestHelper.TestProcess(null, (entry, decRef) =>
				{
					var providerType = GBCustomsRequestFactory.GetProviderType(entry.Declaration.ZG_Gateway);
					var credentials = CreateTemporaryCredentials(entry.Declaration.ZG_Gateway, "HYECMT.GB999999999888.ABC", "Username", "Password", "Location", "Role", string.Empty);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single())
						, ServiceType.NewDeclaration, providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSAmendDeclarationEDIMessage>().Single())
						, ServiceType.AmendDeclaration, providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSCancelDeclarationEDIMessage>().Single())
						, ServiceType.CancelDeclaration, providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingConsolidationRequestEDIMessage>().Single())
						, (CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage)), providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingMovementRequestEDIMessage>().Single())
						, (CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingMovementRequestEDIMessage)), providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingQueryRequestEDIMessage>().Single())
						, (CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingQueryRequestEDIMessage)), providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingMasterQueryRequestEDIMessage>().Single())
						, (CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingMasterQueryRequestEDIMessage)), providerType, decRef, credentials);
				});
			}
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestNew_CDSILEPhase2()
		{
			CreateLicenceKey();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				CDSMessageSenderTestHelper.TestProcess(null, (entry, decRef) =>
				{
					var providerType = GBCustomsRequestFactory.GetProviderType(entry.Declaration.ZG_Gateway);
					var credentials = CreateTemporaryCredentials(entry.Declaration.ZG_Gateway, "HYECMT.GB999999999888.ABC", "Username", "Password", "Location", "Role", string.Empty);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single())
						, ServiceType.NewDeclaration, providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSAmendDeclarationEDIMessage>().Single())
						, ServiceType.AmendDeclaration, providerType, decRef, credentials);

					AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSCancelDeclarationEDIMessage>().Single())
						, ServiceType.CancelDeclaration, providerType, decRef, credentials);

					if (entry.Declaration.ZG_Gateway == GatewayList.Codes.MCP_CUSDECOnly
						|| entry.Declaration.ZG_Gateway == GatewayList.Codes.CNS_CUSDECOnly
						|| entry.Declaration.ZG_Gateway == GatewayList.Codes.Pentant)
					{
						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingConsolidationRequestEDIMessage>().Single())
						, ServiceType.ExportInventoryConsolidation, providerType, decRef, credentials);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingMovementRequestEDIMessage>().Single())
							, ServiceType.ExportInventoryMovement, providerType, decRef, credentials);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingQueryRequestEDIMessage>().Single())
							, ServiceType.ExportInventoryQuery, providerType, decRef, credentials);
					}
					else
					{
						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingConsolidationRequestEDIMessage>().Single())
						, ServiceType.ExportInventory, providerType, decRef, credentials);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingMovementRequestEDIMessage>().Single())
							, ServiceType.ExportInventory, providerType, decRef, credentials);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingQueryRequestEDIMessage>().Single())
							, ServiceType.ExportInventory, providerType, decRef, credentials);
					}
				});
			}
		}

		void AssertSerializedGBCustomsRequest(CusEntryHeader entry, string decRef, string serializedCustomsRequest, string service)
		{
			var providerType = GBCustomsRequestFactory.GetProviderType(entry.Declaration.ZG_Gateway);
			var provider = providerType.ToString();
			var credentialsNode = providerType == ProviderType.Direct
				? @"  <Credentials Key=""HYECMT.GB999999999888.ABC"" />"
				: @"  <Credentials Key=""HYECMT.GB999999999888.ABC"">
    <User>Username</User>
    <Password>" + CDSMessageSenderTestHelper.GetEncryptedPasswordFromGBCustomsRequestXml(serializedCustomsRequest) + $@"</Password>
    <Topic>Location</Topic>
    <Badge>{(providerType == ProviderType.Pentant ? "ABC" : "Role")}</Badge>
  </Credentials>";
			var expectedXml = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>{provider}</Provider>
  <Service>{service}</Service>
{credentialsNode}
  <JobNumber>{decRef}</JobNumber>
</GBCustomsRequest>";
			AssertXmlEquals(
				expectedXml,
				serializedCustomsRequest);
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestSerialize()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				CDSMessageSenderTestHelper.TestProcess(null, (entry, decRef) =>
				{
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single()).Serialize(), "NewDeclaration");
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSAmendDeclarationEDIMessage>().Single()).Serialize(), "AmendDeclaration");
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSCancelDeclarationEDIMessage>().Single()).Serialize(), "CancelDeclaration");
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingConsolidationRequestEDIMessage>().Single()).Serialize(), GBCustomsRequestFactory.GetServiceType((CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage)), GBCustomsRequestFactory.GetProviderType(entry.Declaration.ZG_Gateway)).ToString());
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingMovementRequestEDIMessage>().Single()).Serialize(), GBCustomsRequestFactory.GetServiceType((CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingMovementRequestEDIMessage)), GBCustomsRequestFactory.GetProviderType(entry.Declaration.ZG_Gateway)).ToString());
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingQueryRequestEDIMessage>().Single()).Serialize(), GBCustomsRequestFactory.GetServiceType((CDSEDIMessage)Factory.New(typeof(CDSInventoryLinkingQueryRequestEDIMessage)), GBCustomsRequestFactory.GetProviderType(entry.Declaration.ZG_Gateway)).ToString());
				});
			}
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestSerialize_CDSILEPhase2()
		{
			CreateLicenceKey();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				CDSMessageSenderTestHelper.TestProcess(null, (entry, decRef) =>
				{
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single()).Serialize(), "NewDeclaration");
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSAmendDeclarationEDIMessage>().Single()).Serialize(), "AmendDeclaration");
					AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSCancelDeclarationEDIMessage>().Single()).Serialize(), "CancelDeclaration");
					if (entry.Declaration.ZG_Gateway == GatewayList.Codes.MCP_CUSDECOnly
						|| entry.Declaration.ZG_Gateway == GatewayList.Codes.CNS_CUSDECOnly
						|| entry.Declaration.ZG_Gateway == GatewayList.Codes.Pentant)
					{
						AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingConsolidationRequestEDIMessage>().Single()).Serialize(), "ExportInventoryConsolidation");
						AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingMovementRequestEDIMessage>().Single()).Serialize(), "ExportInventoryMovement");
						AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingQueryRequestEDIMessage>().Single()).Serialize(), "ExportInventoryQuery");
					}
					else
					{
						AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingConsolidationRequestEDIMessage>().Single()).Serialize(), "ExportInventory");
						AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingMovementRequestEDIMessage>().Single()).Serialize(), "ExportInventory");
						AssertSerializedGBCustomsRequest(entry, decRef, GBCustomsRequestFactory.New(entry.Messages.OfType<CDSInventoryLinkingQueryRequestEDIMessage>().Single()).Serialize(), "ExportInventory");
					}
				});
			}
		}

		static void AssertNewGBCustomsRequest(GBCustomsRequest request, CDSEDIMessage msg, ProviderType providerType, ZString jobNumber, Credentials credentials)
		{
			AssertEquals("Service", GBCustomsRequestFactory.GetServiceType(msg, providerType), request.Service);
			AssertNewGBCustomsRequestCore(request, providerType, jobNumber, credentials);
		}

		static void AssertNewGBCustomsRequest(GBCustomsRequest request, ServiceType serviceType, ProviderType providerType, ZString jobNumber, Credentials credentials)
		{
			AssertEquals("Service", serviceType, request.Service);
			AssertNewGBCustomsRequestCore(request, providerType, jobNumber, credentials);
		}

		static void AssertNewGBCustomsRequestCore(GBCustomsRequest request, ProviderType providerType, ZString jobNumber, Credentials credentials)
		{
			AssertEquals("Provider", providerType, request.Provider);
			AssertEquals("JobNumber", jobNumber, request.JobNumber);
			AssertEquals("Credentials.Key", credentials.Key, request.Credentials.Key);
			AssertEquals("Credentials.User", credentials.User, request.Credentials.User);
			if (providerType == ProviderType.Direct)
			{
				AssertEquals("Credentials.Password not used", null, request.Credentials.Password);
			}
			else
			{
				CombineAssertions("Credentials.Password expected", () =>
				{
					AssertNotEquals("Credentials.Password cannot be null", null, request.Credentials.Password);
					AssertNotEquals("Credentials.Password cannot be empty", string.Empty, request.Credentials.Password);
					AssertEquals("Credentials.Password longer than original ", true, request.Credentials.Password.Length > credentials.Password.Length);
					AssertNotEquals("Credentials.Password not the same as the original", credentials.Password, request.Credentials.Password);
				});
			}

			AssertEquals("Credentials.Topic", credentials.Topic, request.Credentials.Topic);
			AssertEquals("Credentials.Badge", providerType == ProviderType.Pentant ? "ABC" : credentials.Badge, request.Credentials.Badge);
		}

		[TestDate(2015, 8, 22)]
		public void TestServiceType_for_CDSInventoryLinkingConsolidationRequests()
		{
			CreateLicenceKey();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				var declarationsList = new List<JobDeclaration>();
				foreach (var gateway in CDSMessageSenderTestHelper.GetGateways().Where(x => !x.IsEmpty))
				{
					var dec = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
					declarationsList.Add(dec);
				}

				int i = 0;
				foreach (var gateway in CDSMessageSenderTestHelper.GetGateways().Where(x => !x.IsEmpty))
				{
					using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, CreateTemporaryRegistryBadge(gateway)))
					using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateTemporaryRegistryBadgeCredential()))
					{
						var declaration = declarationsList[i];
						declaration.JE_MessageType = "EXP";
						declaration.ZG_Gateway = "CDS";
						declaration.JE_TransportMode = "AIR";
						declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
						declaration.JE_DeclarationType = ExportDeclarationTypeList.Codes.DeclarationForExport;
						declaration.JE_RL_NKPortOfLoading = "GBLHR";
						declaration.JE_RL_NKPortOfArrival = "AUSYD";
						declaration.JE_MasterUCR = "Master_UCR_Value_01";
						declaration.JE_UCR = "UcrValue01";
						declaration.ZG_Gateway = gateway;
						declaration.JE_CustomsProfile = "ABC";

						var password = Factory.New<GlbExternalPassword_GB>();
						password.GP_GC = declaration.CompanyPK;
						password.Badge = declaration.JE_CustomsProfile;
						password.EORI = declaration.DeclarantTraderId;
						password.StatusMessage = "Status message.";
						password.Status = PasswordStatusList.Codes.Valid;
						password.GP_ExpiryDate = new ZDate(2015, 9, 1);
						password.GP_IssueDate = new ZDate(2014, 3, 20);

						declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
						var entry01 = declaration.ActiveEntryHeaders.AddNew();
						var entry02 = declaration.ActiveEntryHeaders.AddNew();
						var entry03 = declaration.ActiveEntryHeaders.AddNew();

						var declarant = Factory.NewWithValidTestData<OrgHeader>();
						declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
						declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
						declaration?.Branch?.OrgProxy?.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);

						var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);

						var associateMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)entry01.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						associateMessage.EM_MessageType = GbCusDecMessageFunctionsList.Codes.Associate;
						associateMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						associateMessage.EM_MessageNum = "7";
						associateMessage.EM_MessageOwner = "ABC";

						var disassociateMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)entry02.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						disassociateMessage.EM_MessageType = GbCusDecMessageFunctionsList.Codes.Disassociate;
						disassociateMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						disassociateMessage.EM_MessageNum = "8";
						disassociateMessage.EM_MessageOwner = "ABC";

						var closeMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)entry03.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						closeMessage.EM_MessageType = GbCusDecMessageFunctionsList.Codes.Close;
						closeMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						closeMessage.EM_MessageNum = "9";
						closeMessage.EM_MessageOwner = "ABC";

						var providerType = GBCustomsRequestFactory.GetProviderType(gateway);
						var credentials = CreateTemporaryCredentials(gateway, "HYECMT.GB999999999888.ABC", "Username", "Password", "CUKFFW98000ZPE", "Role", string.Empty);

						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(associateMessage), GBCustomsRequestFactory.GetServiceType(associateMessage, providerType), providerType, declaration.JE_DeclarationReference, credentials);
						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(disassociateMessage), GBCustomsRequestFactory.GetServiceType(disassociateMessage, providerType), providerType, declaration.JE_DeclarationReference, credentials);
						AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(closeMessage), GBCustomsRequestFactory.GetServiceType(closeMessage, providerType), providerType, declaration.JE_DeclarationReference, credentials);
						i++;
					}
				}
			}
		}

		[TestDate(2015, 8, 22)]
		public void TestServiceType_for_CDSInventoryLinkingConsolidationRequests_CDSILEPhase2()
		{
			CreateLicenceKey();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				var declarationsList = new List<JobDeclaration>();
				foreach (var gateway in CDSMessageSenderTestHelper.GetGateways().Where(x => !x.IsEmpty))
				{
					var dec = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
					declarationsList.Add(dec);
				}

				int i = 0;
				foreach (var gateway in CDSMessageSenderTestHelper.GetGateways().Where(x => !x.IsEmpty))
				{
					using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, CreateTemporaryRegistryBadge(gateway)))
					using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateTemporaryRegistryBadgeCredential()))
					{
						var declaration = declarationsList[i];
						declaration.JE_MessageType = "EXP";
						declaration.ZG_Gateway = "CDS";
						declaration.JE_TransportMode = "AIR";
						declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
						declaration.JE_DeclarationType = ExportDeclarationTypeList.Codes.DeclarationForExport;
						declaration.JE_RL_NKPortOfLoading = "GBLHR";
						declaration.JE_RL_NKPortOfArrival = "AUSYD";
						declaration.JE_MasterUCR = "Master_UCR_Value_01";
						declaration.JE_UCR = "UcrValue01";
						declaration.ZG_Gateway = gateway;
						declaration.JE_CustomsProfile = "ABC";

						var password = Factory.New<GlbExternalPassword_GB>();
						password.GP_GC = declaration.CompanyPK;
						password.Badge = declaration.JE_CustomsProfile;
						password.EORI = declaration.DeclarantTraderId;
						password.StatusMessage = "Status message.";
						password.Status = PasswordStatusList.Codes.Valid;
						password.GP_ExpiryDate = new ZDate(2015, 9, 1);
						password.GP_IssueDate = new ZDate(2014, 3, 20);

						declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
						var entry01 = declaration.ActiveEntryHeaders.AddNew();
						var entry02 = declaration.ActiveEntryHeaders.AddNew();
						var entry03 = declaration.ActiveEntryHeaders.AddNew();

						var declarant = Factory.NewWithValidTestData<OrgHeader>();
						declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
						declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
						declaration?.Branch?.OrgProxy?.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);

						var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);

						var associateMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)entry01.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						associateMessage.EM_MessageType = GbCusDecMessageFunctionsList.Codes.Associate;
						associateMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						associateMessage.EM_MessageNum = "7";
						associateMessage.EM_MessageOwner = "ABC";

						var disassociateMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)entry02.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						disassociateMessage.EM_MessageType = GbCusDecMessageFunctionsList.Codes.Disassociate;
						disassociateMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						disassociateMessage.EM_MessageNum = "8";
						disassociateMessage.EM_MessageOwner = "ABC";

						var closeMessage = (CDSInventoryLinkingConsolidationRequestEDIMessage)entry03.Messages.AddNew(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage));
						closeMessage.EM_MessageType = GbCusDecMessageFunctionsList.Codes.Close;
						closeMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
						closeMessage.EM_MessageNum = "9";
						closeMessage.EM_MessageOwner = "ABC";

						var providerType = GBCustomsRequestFactory.GetProviderType(gateway);
						var credentials = CreateTemporaryCredentials(gateway, "HYECMT.GB999999999888.ABC", "Username", "Password", "CUKFFW98000ZPE", "Role", string.Empty);

						if (gateway == GatewayList.Codes.MCP_CUSDECOnly
							|| gateway == GatewayList.Codes.CNS_CUSDECOnly
							|| gateway == GatewayList.Codes.Pentant)
						{
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(associateMessage), ServiceType.ExportInventoryConsolidation, providerType, declaration.JE_DeclarationReference, credentials);
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(disassociateMessage), ServiceType.ExportInventoryConsolidation, providerType, declaration.JE_DeclarationReference, credentials);
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(closeMessage), ServiceType.ExportInventoryConsolidation, providerType, declaration.JE_DeclarationReference, credentials);
						}
						else
						{
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(associateMessage), ServiceType.ExportInventory, providerType, declaration.JE_DeclarationReference, credentials);
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(disassociateMessage), ServiceType.ExportInventory, providerType, declaration.JE_DeclarationReference, credentials);
							AssertNewGBCustomsRequest(GBCustomsRequestFactory.New(closeMessage), ServiceType.ExportInventory, providerType, declaration.JE_DeclarationReference, credentials);
						}

						i++;
					}
				}
			}
		}

		public void TestGBCustomsRequestForPentant()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789", Core.Constants.CountryCodes.UnitedKingdom);
			orgProxy.Factory.Save();
			CreateLicenceKey();
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, CreateTemporaryRegistryBadge(GatewayList.Codes.Pentant)))
			using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateTemporaryRegistryBadgeCredential()))
			{
				var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
				declaration.JE_CustomsProfile = "ABC";
				declaration.ZG_Gateway = GatewayList.Codes.Pentant;
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entry = declaration.ActiveEntryHeaders.AddNew();
				var newDeclarationMessage = Factory.New<CDSNewDeclarationEDIMessage>();
				newDeclarationMessage.EM_MessageText = "<MetaData><Declaration/></MetaData>";
				newDeclarationMessage.EM_MessageNum = "1";
				newDeclarationMessage.EM_MessageOwner = "ABC";
				entry.Messages.Add(newDeclarationMessage);

				var request = GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single());
				AssertEquals("123456789", request.Credentials.PartyID);

				var rawXml = request.Serialize();
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(rawXml);
				var node = xmlDoc.SelectSingleNode("//*[local-name()='GBCustomsRequest']/*[local-name()='Credentials']/*[local-name()='PartyID']/text()");
				AssertEquals("123456789", node.Value);
			}
		}

		public void TestServiceType()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				CombineAssertions(() =>
				{
					AssertServiceType<CDSNewDeclarationEDIMessage>(ProviderType.Direct, ServiceType.NewDeclaration);
					AssertServiceType<CDSNewDeclarationEDIMessage>(ProviderType.MCP, ServiceType.NewDeclaration);
					AssertServiceType<CDSAmendDeclarationEDIMessage>(ProviderType.Direct, ServiceType.AmendDeclaration);
					AssertServiceType<CDSAmendDeclarationEDIMessage>(ProviderType.MCP, ServiceType.AmendDeclaration);
					AssertServiceType<CDSCancelDeclarationEDIMessage>(ProviderType.Direct, ServiceType.CancelDeclaration);
					AssertServiceType<CDSCancelDeclarationEDIMessage>(ProviderType.MCP, ServiceType.CancelDeclaration);
					AssertServiceType<CDSInventoryLinkingQueryRequestEDIMessage>(ProviderType.Direct, ServiceType.ExportInventory);
					AssertServiceType<CDSInventoryLinkingQueryRequestEDIMessage>(ProviderType.MCP, ServiceType.ExportInventoryQuery);
					AssertServiceType<CDSInventoryLinkingConsolidationRequestEDIMessage>(ProviderType.Direct, ServiceType.ExportInventory);
					AssertServiceType<CDSInventoryLinkingConsolidationRequestEDIMessage>(ProviderType.MCP, ServiceType.ExportInventoryConsolidation);
					AssertServiceType<CDSInventoryLinkingMovementRequestEDIMessage>(ProviderType.Direct, ServiceType.ExportInventory);
					AssertServiceType<CDSInventoryLinkingMovementRequestEDIMessage>(ProviderType.MCP, ServiceType.ExportInventoryMovement);
					AssertServiceType<CDSArrivalAmendmentDeclarationEDIMessage>(ProviderType.Direct, ServiceType.GoodsPresentationNotification);
					AssertServiceType<CDSArrivalAmendmentDeclarationEDIMessage>(ProviderType.MCP, ServiceType.NewDeclaration);
					AssertServiceType<CDSFECAmendmentDeclarationEDIMessage>(ProviderType.Direct, ServiceType.AmendDeclaration);
					AssertServiceType<CDSFECAmendmentDeclarationEDIMessage>(ProviderType.MCP, ServiceType.AmendDeclaration);
					AssertServiceType<CDSNilAmendmentDeclarationEDIMessage>(ProviderType.Direct, ServiceType.AmendDeclaration);
					AssertServiceType<CDSNilAmendmentDeclarationEDIMessage>(ProviderType.MCP, ServiceType.AmendDeclaration);
					AssertServiceType<CDSArrivalAmendmentDeclarationEDIMessage>(ProviderType.Pentant, ServiceType.PentantArrival);
					AssertServiceType<CDSPentantAcaMessage>(ProviderType.Pentant, ServiceType.PentantACA);
				});
			}
		}

		public void TestServiceType_CDSILEPhase2()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				CombineAssertions(() =>
				{
					AssertServiceType<CDSNewDeclarationEDIMessage>(ProviderType.Direct, ServiceType.NewDeclaration);
					AssertServiceType<CDSNewDeclarationEDIMessage>(ProviderType.MCP, ServiceType.NewDeclaration);
					AssertServiceType<CDSAmendDeclarationEDIMessage>(ProviderType.Direct, ServiceType.AmendDeclaration);
					AssertServiceType<CDSAmendDeclarationEDIMessage>(ProviderType.MCP, ServiceType.AmendDeclaration);
					AssertServiceType<CDSCancelDeclarationEDIMessage>(ProviderType.Direct, ServiceType.CancelDeclaration);
					AssertServiceType<CDSCancelDeclarationEDIMessage>(ProviderType.MCP, ServiceType.CancelDeclaration);
					AssertServiceType<CDSInventoryLinkingQueryRequestEDIMessage>(ProviderType.MCP, ServiceType.ExportInventoryQuery);
					AssertServiceType<CDSInventoryLinkingQueryRequestEDIMessage>(ProviderType.CNS, ServiceType.ExportInventoryQuery);
					AssertServiceType<CDSInventoryLinkingQueryRequestEDIMessage>(ProviderType.Pentant, ServiceType.ExportInventoryQuery);
					AssertServiceType<CDSInventoryLinkingConsolidationRequestEDIMessage>(ProviderType.MCP, ServiceType.ExportInventoryConsolidation);
					AssertServiceType<CDSInventoryLinkingConsolidationRequestEDIMessage>(ProviderType.CNS, ServiceType.ExportInventoryConsolidation);
					AssertServiceType<CDSInventoryLinkingConsolidationRequestEDIMessage>(ProviderType.Pentant, ServiceType.ExportInventoryConsolidation);
					AssertServiceType<CDSInventoryLinkingMovementRequestEDIMessage>(ProviderType.MCP, ServiceType.ExportInventoryMovement);
					AssertServiceType<CDSInventoryLinkingMovementRequestEDIMessage>(ProviderType.CNS, ServiceType.ExportInventoryMovement);
					AssertServiceType<CDSInventoryLinkingMovementRequestEDIMessage>(ProviderType.Pentant, ServiceType.ExportInventoryMovement);
					AssertServiceType<CDSArrivalAmendmentDeclarationEDIMessage>(ProviderType.Direct, ServiceType.GoodsPresentationNotification);
					AssertServiceType<CDSArrivalAmendmentDeclarationEDIMessage>(ProviderType.MCP, ServiceType.NewDeclaration);
					AssertServiceType<CDSFECAmendmentDeclarationEDIMessage>(ProviderType.Direct, ServiceType.AmendDeclaration);
					AssertServiceType<CDSFECAmendmentDeclarationEDIMessage>(ProviderType.MCP, ServiceType.AmendDeclaration);
					AssertServiceType<CDSNilAmendmentDeclarationEDIMessage>(ProviderType.Direct, ServiceType.AmendDeclaration);
					AssertServiceType<CDSNilAmendmentDeclarationEDIMessage>(ProviderType.MCP, ServiceType.AmendDeclaration);
					AssertServiceType<CDSArrivalAmendmentDeclarationEDIMessage>(ProviderType.Pentant, ServiceType.PentantArrival);
					AssertServiceType<CDSPentantAcaMessage>(ProviderType.Pentant, ServiceType.PentantACA);
				});
			}
		}

		public void TestUseILEPhase2Services()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				AssertEquals(false, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.CCSUK));
				AssertEquals(false, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.Direct));
				AssertEquals(true, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.CNS));
				AssertEquals(true, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.MCP));
				AssertEquals(true, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.Pentant));
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				AssertEquals(false, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.CCSUK));
				AssertEquals(false, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.Direct));
				AssertEquals(true, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.CNS));
				AssertEquals(true, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.MCP));
				AssertEquals(true, GBCustomsRequestFactory.IsSendingViaCspAndViaEhub(ProviderType.Pentant));
			}
		}

		public void TestGetCredentials()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			_ = orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789", Core.Constants.CountryCodes.UnitedKingdom);
			CreateLicenceKey();
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, CreateTemporaryRegistryBadge(GatewayList.Codes.CDS)))
			{
				var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
				declaration.JE_CustomsProfile = "ABC";
				declaration.ZG_Gateway = GatewayList.Codes.Pentant;
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var entry = declaration.ActiveEntryHeaders.AddNew();
				var newDeclarationMessage = Factory.New<CDSNewDeclarationEDIMessage>();
				newDeclarationMessage.EM_MessageText = "<MetaData><Declaration/></MetaData>";
				newDeclarationMessage.EM_MessageNum = "1";
				newDeclarationMessage.EM_MessageOwner = "ABC";
				entry.Messages.Add(newDeclarationMessage);

				GBCustomsRequest request = default;
				var credentials = CreateTemporaryCredentials(GatewayList.Codes.Pentant, "HYECMT.GB999999999888.ABC", string.Empty, null, string.Empty, string.Empty, string.Empty);
				AssertNoExceptionThrown("Get Credential Gateways: CCSUK, CNS, MCP, PNT", () => { request = GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single()); });
				AssertCredentials(request, ProviderType.Pentant, credentials);

				declaration.ZG_Gateway = GatewayList.Codes.CDS;
				credentials = CreateTemporaryCredentials(GatewayList.Codes.CDS, "HYECMT.GB999999999888.ABC", string.Empty, null, string.Empty, string.Empty, string.Empty);
				AssertNoExceptionThrown("Get Credential Gateways: CDS, NES", () => { request = GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single()); });
				AssertCredentials(request, ProviderType.Direct, credentials);

				using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateTemporaryRegistryBadgeCredential()))
				{
					declaration.ZG_Gateway = GatewayList.Codes.Pentant;
					credentials = CreateTemporaryCredentials(GatewayList.Codes.Pentant, "HYECMT.GB999999999888.ABC", "Username", "password", "CUKFFW98000ZPE", "ABC", string.Empty);
					AssertNoExceptionThrown("Get Credential Gateways: CCSUK, CNS, MCP, PNT", () => { request = GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single()); });
					AssertCredentials(request, ProviderType.Pentant, credentials, true);

					declaration.ZG_Gateway = GatewayList.Codes.CDS;
					credentials = CreateTemporaryCredentials(GatewayList.Codes.CDS, "HYECMT.GB999999999888.ABC", null, null, null, null, string.Empty);
					AssertNoExceptionThrown("Get Credential Gateways: CDS, NES", () => { request = GBCustomsRequestFactory.New(entry.Messages.OfType<CDSNewDeclarationEDIMessage>().Single()); });
					AssertCredentials(request, ProviderType.Direct, credentials, true);
				}
			}
		}

		static void AssertCredentials(GBCustomsRequest request, ProviderType providerType, Credentials credentials, bool hasGBCustomsDataRegistryInstanceCredentials = false)
		{
			AssertEquals("Provider", providerType, request.Provider);
			AssertEquals("Credentials.Key", credentials.Key, request.Credentials.Key);
			AssertEquals("Credentials.User", credentials.User, request.Credentials.User);
			if (providerType == ProviderType.Direct)
			{
				AssertEquals("Credentials.Password not used", null, request.Credentials.Password);
			}
			else if (hasGBCustomsDataRegistryInstanceCredentials)
			{
				CombineAssertions("Credentials.Password expected", () =>
				{
					AssertNotEquals("Credentials.Password cannot be null", null, request.Credentials.Password);
					AssertNotEquals("Credentials.Password cannot be empty", string.Empty, request.Credentials.Password);
					AssertEquals("Credentials.Password longer than original ", true, request.Credentials.Password.Length > credentials.Password.Length);
					AssertNotEquals("Credentials.Password not the same as the original", credentials.Password, request.Credentials.Password);
				});
			}

			AssertEquals("Credentials.Topic", credentials.Topic, request.Credentials.Topic);
			AssertEquals("Credentials.Badge", credentials.Badge, request.Credentials.Badge);
			AssertEquals("Credentials.PartyID", credentials.PartyID, request.Credentials.PartyID);
		}

		void AssertServiceType<T>(ProviderType provider, ServiceType expectedServiceType) where T : CDSEDIMessage
		{
			var msg = (CDSEDIMessage)Factory.New(typeof(T));
			var actual = GBCustomsRequestFactory.GetServiceType(msg, provider);

			AssertEquals(System.FormattableString.Invariant($"Message Type: {typeof(T).Name} Provider: {provider}"), expectedServiceType, actual);
		}

		static void AssertXmlEquals(string expectedXml, string actualXml)
		{
			var expected = NormalizeNamespaces(XElement.Parse(expectedXml));
			var actual = NormalizeNamespaces(XElement.Parse(actualXml));

			if (!XNode.DeepEquals(expected, actual))
			{
				throw new Exception("XML content does not match.\n\nExpected:\n" + expected + "\n\nActual:\n" + actual);
			}

			Assert(true);
		}

		static XElement NormalizeNamespaces(XElement element, XNamespace defaultNamespace = null)
		{
			var currentNs = element.Name.Namespace;
			var effectiveNs = currentNs != XNamespace.None ? currentNs : defaultNamespace ?? XNamespace.None;

			var normalized = new XElement(
				effectiveNs + element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration),
				element.Nodes().Select(n =>
				{
					if (n is XElement child)
					{
						return NormalizeNamespaces(child, effectiveNs);
					}
					else if (n is XText text)
					{
						return new XText(text.Value.Trim());
					}
					else
					{
						return n;
					}
				})
			);

			return normalized;
		}

		static CredentialsSettingCollection CreateTemporaryRegistryBadgeCredential()
		{
			return new CredentialsSettingCollection
				{
					new CredentialsSetting
					{
						BadgeCode = "ABC",
						PIMA = "CUKFFW98000",
						Printer = "CUKFFW98000ZPE",
						Company = "Role",
						Username = "Username",
						Password = "Password"
					}
				};
		}

		static BadgeCodeSettingCollection CreateTemporaryRegistryBadge(ZString gateway)
		{
			return new BadgeCodeSettingCollection
				{
					new BadgeCodeSetting
					{
						BadgeCode = "ABC",
						RL_PortCode = "GBLHR",
						Direction = "EXP",
						CSPCode = gateway,
						MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
						ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
					}
				};
		}

		static Credentials CreateTemporaryCredentials(ZString gateway, string key, string user, string password, string topic, string badge, string partyID)
		{
			var providerType = GBCustomsRequestFactory.GetProviderType(gateway);
			return providerType == ProviderType.Direct
				? new Credentials
				{
					Key = key
				}
				: new Credentials
				{
					Key = key,
					User = user,
					Password = password,
					Topic = topic,
					Badge = badge,
					PartyID = partyID
				};
		}

		static void CreateLicenceKey()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
		}
	}
}
