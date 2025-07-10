using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public static class CDSMessageSenderTestHelper
	{
		public static void TestProcess(Action process)
		{
			TestProcess(process, entry => entry.Factory.Load<EDIInterchange>(new ZQuery { OrderBy = EDIInterchange.Schema.EI_InterchangeNum }));
		}

		public static void TestProcess(Action process, Func<CusEntryHeader, EDIInterchange[]> getInterchanges)
		{
			var decRefSuffix = 1;

			foreach (var gateway in GetGateways().Where(g => g != GatewayList.Codes.CCSUKviaNTMsgGW))
			{
				HaveATest("VICTEST" + decRefSuffix++, gateway, process, getInterchanges);
				HaveATest_CDSILEPhase2("VICTEST" + decRefSuffix++, gateway, process, getInterchanges);
			}
		}

		public static void TestProcess(Action process, Action<CusEntryHeader, string> doSomeAssertions)
		{
			var decRefSuffix = 1;

			foreach (var gateway in GetGateways().Where(g => g != GatewayList.Codes.CCSUKviaNTMsgGW))
			{
				HaveATest("VICTEST" + decRefSuffix++, gateway, process, doSomeAssertions);
			}
		}

		public static string GetEncryptedPasswordFromGBCustomsRequestXml(ZString xml)
		{
			const string PasswordNodeXPath = "//*[local-name()='GBCustomsRequest']/*[local-name()='Credentials']/*[local-name()='Password']";
			var encryptedPassword = string.Empty;
			var xmlDoc = new XmlDocument();
			if (xmlDoc != null && !xml.IsEmpty)
			{
				xmlDoc.LoadXml(xml);
				encryptedPassword = xmlDoc.DocumentElement?.SelectNodes(PasswordNodeXPath)?.Cast<XmlNode>().FirstOrDefault()?.InnerText;
			}
			return encryptedPassword;
		}

		static void HaveATest(ZString decRef, ZString gateway, Action process, Func<CusEntryHeader, EDIInterchange[]> getInterchanges)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				HaveATest(decRef, gateway, process, (entry, decRef) =>
				{
					AssertionWithHtml.CombineAssertions(() =>
					{
						var interchanges = getInterchanges.Invoke(entry);
						Assertion.AssertEquals("NumberOfInterchanges", 7, interchanges.Length);

						AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.NewDeclaration)
							, "<MetaData><Declaration/></MetaData>"
							, ZString.Empty
							, "HQU"
							, "NewDeclaration"
							, decRef
							, gateway);
						AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.AmendDeclaration)
							, "<MetaData><Declaration/></MetaData>"
							, ZString.Empty
							, "HQU"
							, "AmendDeclaration"
							, decRef
							, gateway);
						AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.CancelDeclaration)
							, "<MetaData><Declaration/></MetaData>"
							, ZString.Empty
							, "HQU"
							, "CancelDeclaration"
							, decRef
							, gateway);

						if (gateway == GatewayList.Codes.MCP_CUSDECOnly
							|| gateway == GatewayList.Codes.CNS_CUSDECOnly
							|| gateway == GatewayList.Codes.Pentant)
						{
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest)
								, "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryConsolidation"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest)
								, "<MetaData><inventoryLinkingMovementRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryMovement"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest)
								, "<MetaData><inventoryLinkingQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryQuery"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.MasterQueryDeclaration)
								, "<MetaData><inventoryLinkingMasterQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryQuery"
								, decRef
								, gateway); 
						}
						else
						{
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest)
								, "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest)
								, "<MetaData><inventoryLinkingMovementRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest)
								, "<MetaData><inventoryLinkingQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.MasterQueryDeclaration)
								, "<MetaData><inventoryLinkingMasterQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
						}
					});
				});
			}
		}

		static void HaveATest_CDSILEPhase2(ZString decRef, ZString gateway, Action process, Func<CusEntryHeader, EDIInterchange[]> getInterchanges)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				HaveATest(decRef, gateway, process, (entry, decRef) =>
				{
					AssertionWithHtml.CombineAssertions(() =>
					{
						var interchanges = getInterchanges.Invoke(entry);
						Assertion.AssertEquals("NumberOfInterchanges", 7, interchanges.Length);

						AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.NewDeclaration)
							, "<MetaData><Declaration/></MetaData>"
							, ZString.Empty
							, "HQU"
							, "NewDeclaration"
							, decRef
							, gateway);
						AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.AmendDeclaration)
							, "<MetaData><Declaration/></MetaData>"
							, ZString.Empty
							, "HQU"
							, "AmendDeclaration"
							, decRef
							, gateway);
						AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.CancelDeclaration)
							, "<MetaData><Declaration/></MetaData>"
							, ZString.Empty
							, "HQU"
							, "CancelDeclaration"
							, decRef
							, gateway);

						if (gateway == GatewayList.Codes.MCP_CUSDECOnly
							|| gateway == GatewayList.Codes.CNS_CUSDECOnly
							|| gateway == GatewayList.Codes.Pentant)
						{
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest)
								, "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryConsolidation"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest)
								, "<MetaData><inventoryLinkingMovementRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryMovement"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest)
								, "<MetaData><inventoryLinkingQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryQuery"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.MasterQueryDeclaration)
								, "<MetaData><inventoryLinkingMasterQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventoryQuery"
								, decRef
								, gateway);
						}
						else
						{
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest)
								, "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest)
								, "<MetaData><inventoryLinkingMovementRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest)
								, "<MetaData><inventoryLinkingQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
							AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == CDSEDIMessageTypeList.Codes.MasterQueryDeclaration)
								, "<MetaData><inventoryLinkingMasterQueryRequest/></MetaData>"
								, ZString.Empty
								, "HQU"
								, "ExportInventory"
								, decRef
								, gateway);
						}
					});
				});
			}
		}

		static void HaveATest(ZString decRef, ZString gateway, Action process, Action<CusEntryHeader, string> doSomeAssertions)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "ABC",
					RL_PortCode = "GBLBA",
					Direction = "IMP",
					CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW,
					MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
					ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
				}
			}))
			using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
			{
				new CredentialsSetting
				{
					BadgeCode = "ABC",
					Printer = "Location",
					Company = "Role",
					Username = "Username",
					Password = "Password"
				}
			}))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = decRef;
				declaration.JE_CustomsProfile = "ABC";
				declaration.ZG_Gateway = gateway;
				declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);

				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				var newDeclarationMessage = factory.New<CDSNewDeclarationEDIMessage>();
				newDeclarationMessage.EM_MessageText = "<MetaData><Declaration/></MetaData>";
				newDeclarationMessage.EM_MessageNum = "1";
				newDeclarationMessage.EM_MessageOwner = "ABC";
				var amendDeclarationMessage = factory.New<CDSAmendDeclarationEDIMessage>();
				amendDeclarationMessage.EM_MessageText = "<MetaData><Declaration/></MetaData>";
				amendDeclarationMessage.EM_MessageNum = "2";
				amendDeclarationMessage.EM_MessageOwner = "ABC";
				var cancelDeclarationMessage = factory.New<CDSCancelDeclarationEDIMessage>();
				cancelDeclarationMessage.EM_MessageText = "<MetaData><Declaration/></MetaData>";
				cancelDeclarationMessage.EM_MessageNum = "3";
				cancelDeclarationMessage.EM_MessageOwner = "ABC";
				var inventoryLinkingConsolidationMessage = factory.New<CDSInventoryLinkingConsolidationRequestEDIMessage>();
				inventoryLinkingConsolidationMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
				inventoryLinkingConsolidationMessage.EM_MessageNum = "4";
				inventoryLinkingConsolidationMessage.EM_MessageOwner = "ABC";
				var inventoryLinkingMovementMessage = factory.New<CDSInventoryLinkingMovementRequestEDIMessage>();
				inventoryLinkingMovementMessage.EM_MessageText = "<MetaData><inventoryLinkingMovementRequest/></MetaData>";
				inventoryLinkingMovementMessage.EM_MessageNum = "5";
				inventoryLinkingMovementMessage.EM_MessageOwner = "ABC";
				var inventoryLinkingQueryMessage = factory.New<CDSInventoryLinkingQueryRequestEDIMessage>();
				inventoryLinkingQueryMessage.EM_MessageText = "<MetaData><inventoryLinkingQueryRequest/></MetaData>";
				inventoryLinkingQueryMessage.EM_MessageNum = "6";
				inventoryLinkingQueryMessage.EM_MessageOwner = "ABC";
				var inventoryLinkingMasterQueryMessage = factory.New<CDSInventoryLinkingMasterQueryRequestEDIMessage>();
				inventoryLinkingMasterQueryMessage.EM_MessageText = "<MetaData><inventoryLinkingMasterQueryRequest/></MetaData>";
				inventoryLinkingMasterQueryMessage.EM_MessageNum = "7";
				inventoryLinkingMasterQueryMessage.EM_MessageOwner = "ABC";

				entryHeader.Messages.AddRange(newDeclarationMessage
					, amendDeclarationMessage
					, cancelDeclarationMessage
					, inventoryLinkingConsolidationMessage
					, inventoryLinkingMovementMessage
					, inventoryLinkingQueryMessage
					, inventoryLinkingMasterQueryMessage);
				factory.Save();

				process?.Invoke();

				doSomeAssertions?.Invoke(entryHeader, decRef);
			}

			Reset();
		}

		static void Reset()
		{
			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);
		}

		static void AssertInterchange(EDIInterchange interchange, ZString bodyText, ZString footerText, ZString status, string service, string decRef, string gateway)
		{
			var providerType = GBCustomsRequestFactory.GetProviderType(gateway);
			var provider = providerType.ToString();
			var credentialsNode = providerType == ProviderType.Direct
				? @"  <Credentials Key=""HYECMT.GB999999999888.ABC"" />"
				: @"  <Credentials Key=""HYECMT.GB999999999888.ABC"">
    <User>Username</User>
    <Password>" + GetEncryptedPasswordFromGBCustomsRequestXml(interchange.EI_HeaderText) + $@"</Password>
    <Topic>Location</Topic>
    <Badge>{(providerType == ProviderType.Pentant ? "ABC" : "Role")}</Badge>
  </Credentials>";

			var headerText = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>{provider}</Provider>
  <Service>{service}</Service>
{credentialsNode}
  <JobNumber>{decRef}</JobNumber>
</GBCustomsRequest>";

			XmlComparison.CompareAndAssertXml(errorMessage: $"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Header" , expectedXmlResult: headerText, actualXmlResult: interchange.EI_HeaderText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Body", bodyText, interchange.EI_BodyText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Footer", footerText, interchange.EI_FooterText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Status", status, interchange.EI_Status);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.EI_To", Constants.EDIInterchange.GBCustoms, interchange.EI_To);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.EI_To", "ABC", interchange.EI_From);
		}

		public static IEnumerable<ZString> GetGateways()
		{
			foreach (ICodeDescription item in new GatewayList())
			{
				yield return item.Code;
			}

			yield return "CDS";
			yield return ZString.Empty;
		}
	}
}
