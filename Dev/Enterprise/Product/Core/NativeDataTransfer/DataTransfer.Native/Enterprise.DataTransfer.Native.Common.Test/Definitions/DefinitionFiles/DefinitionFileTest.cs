using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.DefinitionFiles
{
	public class DefinitionFileTest : TestCase
	{
		public void TestAllDefinitionsArePresentInNativeDataTypeList()
		{
			var nativeDataTypeListValues = string.Join("\r\n", new NativeDataTypeList().OfType<ICodeDescription>().Select(pair => pair.Code).OrderBy(v => v).ToArray());

			var globalDefinitions = GlobalDefinition.Instance;
			var definitions = globalDefinitions.TableMapping;
			var allDefinitionNames = string.Join("\r\n", definitions.Select(definition => definition.Value).OrderBy(v => v).ToArray());

			AssertMultilineASCIIEquals("NativeDataTypeList must have a complete list of Native DataSet Names", nativeDataTypeListValues, allDefinitionNames);
		}

		public void TestAllDefinitionShouldExcludeGeoLocationFromNativeXml()
		{
			var globalDefinitions = GlobalDefinition.Instance;
			var definitions = globalDefinitions.TableMapping;
			var loader = new DefinitionAssemblyLoader();
			var errorMessage = "{0}SetDefinition.xml: You should exclude {1}'s {2} from native XML";
			var errorMessageList = new StringBuilder();
			foreach (var definition in definitions)
			{
				if (!string.IsNullOrEmpty(definition.Value))
				{
					byte[] definitionData = loader.Load(definition.Value);
					XElement xmlElement;
					using (var stream = new MemoryStream(definitionData))
					using (var reader = XmlReader.Create(stream))
					{
						xmlElement = XElement.Load(reader);
					}
					var entityInfos = new EntityInfoLoader(xmlElement, null).GetEntityInfos();
					foreach (var entity in entityInfos)
					{
						var tableSchema = EnterpriseSchema.GetTableSchema(entity.TableName);
						foreach (var column in tableSchema.All)
						{
							if (column.ColumnType == SchemaColumnType.Geography)
							{
								if (!entity.PropertyIsExcluded(column.Name))
								{
									errorMessageList.Append(string.Format(errorMessage, definition.Value, entity.TableName, column.Name) + "\r\n");
								}
							}
						}
					}
				}
			}
			Assert(errorMessageList.ToString(), errorMessageList.Length == 0);
		}

		public void TestAllDefinitionShouldExcludeSpecificColumnsFromNativeXml()
		{
			var globalDefinitions = GlobalDefinition.Instance;
			var definitions = globalDefinitions.TableMapping;
			var loader = new DefinitionAssemblyLoader();
			var errorMessage = "{0}SetDefinition.xml: You should exclude {1}'s {2} from native XML";
			var errorMessageList = new StringBuilder();
			foreach (var definition in definitions)
			{
				if (!string.IsNullOrEmpty(definition.Value))
				{
					byte[] definitionData = loader.Load(definition.Value);
					XElement xmlElement;
					using (var stream = new MemoryStream(definitionData))
					using (var reader = XmlReader.Create(stream))
					{
						xmlElement = XElement.Load(reader);
					}
					var entityInfos = new EntityInfoLoader(xmlElement, null).GetEntityInfos();
					foreach (var entity in entityInfos)
					{
						var tableSchema = EnterpriseSchema.GetTableSchema(entity.TableName);
						foreach (var column in tableSchema.All)
						{
							foreach (var specificColumn in ExcludeColumnsFromNativeXml)
							{
								if (column.Name == TableNameHelper.GetPrefixFromTableName(entity.TableName) + "_" + specificColumn.Key)
								{
									if (!entity.PropertyIsExcluded(column.Name))
									{
										if (specificColumn.Value != null && specificColumn.Value.Contains(definition.Value))
										{
											continue;
										}

										errorMessageList.Append(string.Format(errorMessage, definition.Value, entity.TableName, column.Name) + "\r\n");
									}
								}
							}
						}
					}
				}
			}
			Assert(errorMessageList.ToString(), errorMessageList.Length == 0);
		}

		Dictionary<string, List<string>> ExcludeColumnsFromNativeXml => new Dictionary<string, List<string>>()
		{
			{ "ValidationStatus", new List<string>() { "Country" } }
		};

		public void TestContainerDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("Container");
			var root = info.Root;

			AssertFullNamesShouldBeUnique(info);

			AssertNotNull("Container Set should have a root entity", root);
			AssertEquals("root.Name should be RefContainer", "RefContainer", root.EntityName);
			AssertEquals("Root should have two children", 3, root.Children.Count());
			AssertEquals("Root should not have parent", 0, root.Parents.Count());
			Assert("Root should have child named RefEquipment", root.Children.Any(c => c.EntityName.Equals("RefEquipment")));
			Assert("Root should have child named RefContainerStock", root.Children.Any(c => c.EntityName.Equals("RefContainerStock")));
			Assert("Root should have child named RefContainerCodeMap", root.Children.Any(c => c.EntityName.Equals("RefContainerCodeMap")));

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in Container EntitySet"
				, @"
RefContainer
RefContainer.RefContainerCodeMap
RefContainer.RefContainerCodeMap.Country
RefContainer.RefContainerStock
RefContainer.RefContainerStock.Owner
RefContainer.RefEquipment
RefContainer.RefEquipment.Owner
RefContainer.RefEquipment.PackTypeExternal
RefContainer.RefEquipment.PreferredDriver
RefContainer.RefEquipment.RegistrationCountry
				".Trim()
				, string.Join("\r\n", entities.OrderBy(entity => entity.FullName).Select(entity => entity.FullName).ToArray()));
		}

		public void TestOrganizationDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("Organization");

			var root = info.Root;
			AssertNotNull(root);
			Assert(info.Root.EntityName == "OrgHeader");

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in Organization EntitySet"
				, @"
OrgHeader
OrgHeader.AccCFXUpliftConfigurationView
OrgHeader.AccCFXUpliftConfigurationView.Currency
OrgHeader.AccCFXUpliftConfigurationView.DestinationCountry
OrgHeader.AccCFXUpliftConfigurationView.GlbCompany
OrgHeader.AccCFXUpliftConfigurationView.OriginCountry
OrgHeader.AccExchangeRateConfigurationView
OrgHeader.AccExchangeRateConfigurationView.GlbCompany
OrgHeader.ClosestPort
OrgHeader.CusBondDetail
OrgHeader.CusBondDetail.CountryOfIssue
OrgHeader.CusBondDetail.Currency
OrgHeader.CusBondDetail.Guarantee
OrgHeader.EDICommunicationsMode
OrgHeader.EDICommunicationsMode.GlbGroup
OrgHeader.EDICommunicationsMode.MessageVAN
OrgHeader.GlbGroupOrgLink
OrgHeader.GlbGroupOrgLink.Group
OrgHeader.JobRequiredDocument
OrgHeader.JobRequiredDocument.DocumentOwner
OrgHeader.JobRequiredDocument.RelatedCountry
OrgHeader.OrgAddress
OrgHeader.OrgAddress.CountryCode
OrgHeader.OrgAddress.OrgAddressAdditionalInfo
OrgHeader.OrgAddress.OrgAddressAdditionalInfo.OrgTranslatedAddressAdditionalInfo
OrgHeader.OrgAddress.OrgAddressCapability
OrgHeader.OrgAddress.OrgTimetable
OrgHeader.OrgAddress.OrgTranslatedAddress
OrgHeader.OrgAddress.RelatedPortCode
OrgHeader.OrgAppointedAgentPorts
OrgHeader.OrgAppointedAgentPorts.AgentOfficeAddress
OrgHeader.OrgAppointedAgentPorts.AgentOfficeAddress.OrgHeader
OrgHeader.OrgAppointedAgentPorts.GlbCompany
OrgHeader.OrgAppointedAgentPorts.OrgExclusiveGatewayService
OrgHeader.OrgAppointedAgentPorts.OrgExclusiveGatewayService.GatewayService
OrgHeader.OrgAppointedAgentPorts.OrgExclusiveGatewayService.ShipmentServiceLevel
OrgHeader.OrgAppointedAgentPorts.OrgParkContainerType
OrgHeader.OrgAppointedAgentPorts.OrgParkContainerType.CYWorkOrderApprovalLimitCurrency
OrgHeader.OrgAppointedAgentPorts.OrgParkContainerType.CYWorkOrderApprovedBy
OrgHeader.OrgAppointedAgentPorts.OrgParkContainerType.CYWorkOrderApprovedBy.OrgHeader
OrgHeader.OrgBrandOrRelatedName
OrgHeader.OrgCarrierAccount_BillToParty
OrgHeader.OrgCarrierAccount_BillToParty.Carrier
OrgHeader.OrgCarrierAccount_Carrier
OrgHeader.OrgCarrierAccount_Carrier.BillToParty
OrgHeader.OrgCarrierNamedAccount
OrgHeader.OrgCarrierNamedAccount.Organization
OrgHeader.OrgCompanyData
OrgHeader.OrgCompanyData.APAccountDetails
OrgHeader.OrgCompanyData.APAccountDetails.AccountCurrency
OrgHeader.OrgCompanyData.APAccountDetails.CountryCode
OrgHeader.OrgCompanyData.APCreditorGroup
OrgHeader.OrgCompanyData.APDefaultBankAccount
OrgHeader.OrgCompanyData.APDefaultBankAccount.AccGLHeader
OrgHeader.OrgCompanyData.APDefaultBankAccount.GlbCompany
OrgHeader.OrgCompanyData.APDefaultChargeCode
OrgHeader.OrgCompanyData.APDefaultChargeCode.GlbCompany
OrgHeader.OrgCompanyData.APDefltCurrency
OrgHeader.OrgCompanyData.APTaxTemplate
OrgHeader.OrgCompanyData.APTaxTemplate.Company
OrgHeader.OrgCompanyData.ARDDefltCurrency
OrgHeader.OrgCompanyData.ARDebtorGroup
OrgHeader.OrgCompanyData.ARPayToAccount
OrgHeader.OrgCompanyData.ARPayToAccount.AccGLHeader
OrgHeader.OrgCompanyData.ARPayToAccount.GlbCompany
OrgHeader.OrgCompanyData.ARTaxTemplate
OrgHeader.OrgCompanyData.ARTaxTemplate.Company
OrgHeader.OrgCompanyData.ControllingBranch
OrgHeader.OrgCompanyData.GlbCompany
OrgHeader.OrgCompanyData.OrgARTerms
OrgHeader.OrgCompanyData.OrgARTerms.Branch
OrgHeader.OrgCompanyData.OrgARTerms.Department
OrgHeader.OrgCompanyData.OrgARTerms.OrgARTermsCycle
OrgHeader.OrgCompanyData.OrgCollectionNote
OrgHeader.OrgCompanyData.OrgCollectionNote.CallingStaff
OrgHeader.OrgCompanyData.OrgCollectionNote.OrgContact
OrgHeader.OrgCompanyData.OrgCollectionNote.OrgContact.OrgHeader
OrgHeader.OrgCompanyData.OrgInvoiceRollupOrGroup
OrgHeader.OrgCompanyData.OrgInvoiceRollupOrGroup.InvoicePostingCurrency
OrgHeader.OrgCompanyData.OrgInvoiceType
OrgHeader.OrgCompanyData.OrgInvoiceType.OrgInvTypeDeferredCharges
OrgHeader.OrgCompanyData.OrgInvoiceType.OrgInvTypeDeferredCharges.AccChargeCode
OrgHeader.OrgCompanyData.OrgInvoiceType.OrgInvTypeDeferredCharges.AccChargeCode.GlbCompany
OrgHeader.OrgCompanyData.OrgInvoiceType.ServiceLevel
OrgHeader.OrgCompanyData.OrgWhsChgAttribGrpBy
OrgHeader.OrgCompetitor
OrgHeader.OrgCompetitor.Company
OrgHeader.OrgCompetitor.Competitor
OrgHeader.OrgContact
OrgHeader.OrgContact.AddressOverride
OrgHeader.OrgContact.GenRegCertAccredMaintList
OrgHeader.OrgContact.GenRegCertAccredMaintList.CountryOfIssuance
OrgHeader.OrgContact.GlbGroupOrgContactLink
OrgHeader.OrgContact.GlbGroupOrgContactLink.Group
OrgHeader.OrgContact.Nationality
OrgHeader.OrgContact.OrgAddress
OrgHeader.OrgContact.OrgAddress.OrgHeader
OrgHeader.OrgContact.OrgContactAttribute
OrgHeader.OrgContact.OrgDocument
OrgHeader.OrgContact.OrgDocument.FilterBranch
OrgHeader.OrgContact.OrgDocument.FilterCompany
OrgHeader.OrgContact.OrgDocument.FilterDepartment
OrgHeader.OrgContact.OrgDocument.MenuItem
OrgHeader.OrgContact.OrgDocument.MenuItem.StaffCodeExternal
OrgHeader.OrgContact.OrgDocument.OrgDocumentCopyRecipient
OrgHeader.OrgContact.OrgDocument.RelatedFilterByParty
OrgHeader.OrgContact.OrgDocument.Suppressed
OrgHeader.OrgContact.OrgSecurityContacts
OrgHeader.OrgContact.OrgSecurityContacts.OrgSecurity
OrgHeader.OrgContact.OrgSecurityContacts.OrgSecurity.OrgHeader
OrgHeader.OrgContact.OrgSecurityContacts.OrgSecurity.StmMenuItem
OrgHeader.OrgContact.OrgSecurityContacts.OrgSecurity.StmMenuItem.StaffCodeExternal
OrgHeader.OrgCountryData
OrgHeader.OrgCountryData.ApprovedLocation
OrgHeader.OrgCountryData.ApprovedLocation.OrgHeader
OrgHeader.OrgCountryData.ClientCountryRelation
OrgHeader.OrgCountryData.DefaultConsignee
OrgHeader.OrgCountryData.DefaultConsignee.OrgHeader
OrgHeader.OrgCountryData.IssuingAuthorityCountry
OrgHeader.OrgCountryData.NotifyParty
OrgHeader.OrgCountryData.ReviewedByUser
OrgHeader.OrgCountryData.WarehouseAddress
OrgHeader.OrgCountryData.WarehouseAddress.OrgHeader
OrgHeader.OrgCusAccount
OrgHeader.OrgCusAccount.CountryCode
OrgHeader.OrgCusCode
OrgHeader.OrgCusCode.CodeCountry
OrgHeader.OrgCusCode.PremisesAddress
OrgHeader.OrgCusCode.PremisesAddress.OrgHeader
OrgHeader.OrgCustomLabels
OrgHeader.OrgCustomLabels.StmEvent
OrgHeader.OrgLandedCostingPrefs
OrgHeader.OrgLandedCostingPrefs.OrgLandedCostingPrefCharges
OrgHeader.OrgLandedCostingPrefs.OrgLandedCostingPrefCharges.ChargeCode
OrgHeader.OrgLandedCostingPrefs.OrgLandedCostingPrefCharges.ChargeCode.GlbCompany
OrgHeader.OrgMiscServ
OrgHeader.OrgMiscServ.Airline
OrgHeader.OrgMiscServ.ARGlobalCreditCurrency
OrgHeader.OrgMiscServ.ARGlobalCreditGroup
OrgHeader.OrgMiscServ.CartonGroup
OrgHeader.OrgMiscServ.CMMainExportCmdty
OrgHeader.OrgMiscServ.CMMainImportCmdty
OrgHeader.OrgMiscServ.CMPreferredPaymentCompany
OrgHeader.OrgMiscServ.EXDefaultCntryOfOrigin
OrgHeader.OrgMiscServ.EXDefaultDGContact
OrgHeader.OrgMiscServ.EXDefaultDGContact.OrgHeader
OrgHeader.OrgMiscServ.EXDefaultServiceLevel
OrgHeader.OrgMiscServ.EXDefCurrency
OrgHeader.OrgMiscServ.FWDefCurrency
OrgHeader.OrgMiscServ.IMDefaultServiceLevel
OrgHeader.OrgMiscServ.OrgCarrierServiceLevel
OrgHeader.OrgMiscServ.OrgSecurityGroup
OrgHeader.OrgMiscServ.WhsDefaultWarehouse
OrgHeader.OrgMiscServ.WhsPackingSlip
OrgHeader.OrgPatternMatchOverride
OrgHeader.OrgRateTariffLevel
OrgHeader.OrgRateTariffLevel.GlbCompany
OrgHeader.OrgRelatedParty
OrgHeader.OrgRelatedParty.GlbCompany
OrgHeader.OrgRelatedParty.ImporterCountry
OrgHeader.OrgRelatedParty.OrgAddress
OrgHeader.OrgRelatedParty.OrgAddress.OrgHeader
OrgHeader.OrgRelatedParty.RelatedParty
OrgHeader.OrgSales_Buyer
OrgHeader.OrgSales_Buyer.Buyer
OrgHeader.OrgSales_Buyer.GlbCompany
OrgHeader.OrgSales_Buyer.OrgTradeDetail
OrgHeader.OrgSales_Buyer.OrgTradeDetail.OrgSupplierPart
OrgHeader.OrgSales_Buyer.Primary
OrgHeader.OrgSales_Buyer.Product
OrgHeader.OrgSales_Buyer.RevenueCurrency
OrgHeader.OrgSales_Buyer.WhsWarehouse
OrgHeader.OrgSales_Supplier
OrgHeader.OrgSales_Supplier.GlbCompany
OrgHeader.OrgSales_Supplier.OrgTradeDetail
OrgHeader.OrgSales_Supplier.OrgTradeDetail.OrgSupplierPart
OrgHeader.OrgSales_Supplier.Primary
OrgHeader.OrgSales_Supplier.Product
OrgHeader.OrgSales_Supplier.RevenueCurrency
OrgHeader.OrgSales_Supplier.Supplier
OrgHeader.OrgSales_Supplier.WhsWarehouse
OrgHeader.OrgSecurity
OrgHeader.OrgSecurity.StmMenuItem
OrgHeader.OrgSecurity.StmMenuItem.StaffCodeExternal
OrgHeader.OrgServiceLevel
OrgHeader.OrgServiceLevel.RefServiceLevel
OrgHeader.OrgServiceLevel.SrvLvl
OrgHeader.OrgStaffAssignments
OrgHeader.OrgStaffAssignments.GlbCompany
OrgHeader.OrgStaffAssignments.PersonResponsible
OrgHeader.OrgSupplierBuyerLink_Buyer
OrgHeader.OrgSupplierBuyerLink_Buyer.Buyer
OrgHeader.OrgSupplierBuyerLink_Buyer.ControllingCustomer
OrgHeader.OrgSupplierBuyerLink_Buyer.DefaultCurrency
OrgHeader.OrgSupplierBuyerLink_Buyer.ImportBroker
OrgHeader.OrgSupplierBuyerLink_Buyer.ImporterCountry
OrgHeader.OrgSupplierBuyerLink_Buyer.NotifyPartyContact
OrgHeader.OrgSupplierBuyerLink_Buyer.NotifyPartyContact.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.CarrierLine
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.ControllingCustomer
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.CustomsControlledArrivalLocation
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.CustomsControlledArrivalLocation.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.CustomsExamSite
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.CustomsExamSite.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.DefaultServiceLevel
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.DeliveryCartageContractor
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.DischargePort
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.ImportCustomsAgent
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.LoadPort
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideConsigneeContact
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideConsigneeContact.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideDeliveryAddress
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideDeliveryAddress.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideNotifyParty
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideNotifyParty.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideNotifyPartyAddress
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideNotifyPartyAddress.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverridePickupAddress
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverridePickupAddress.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideSupplierContact
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.OverrideSupplierContact.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.PickupCartageContractor
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.PlaceOfDeliveryPort
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.PlaceOfReceivalPort
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.ReceivingAgent
OrgHeader.OrgSupplierBuyerLink_Buyer.OrgSupBuyLinkTrnMode.SendingAgent
OrgHeader.OrgSupplierBuyerLink_Supplier
OrgHeader.OrgSupplierBuyerLink_Supplier.ControllingCustomer
OrgHeader.OrgSupplierBuyerLink_Supplier.DefaultCurrency
OrgHeader.OrgSupplierBuyerLink_Supplier.ImportBroker
OrgHeader.OrgSupplierBuyerLink_Supplier.ImporterCountry
OrgHeader.OrgSupplierBuyerLink_Supplier.NotifyPartyContact
OrgHeader.OrgSupplierBuyerLink_Supplier.NotifyPartyContact.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.CarrierLine
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.ControllingCustomer
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.CustomsControlledArrivalLocation
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.CustomsControlledArrivalLocation.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.CustomsExamSite
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.CustomsExamSite.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.DefaultServiceLevel
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.DeliveryCartageContractor
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.DischargePort
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.ImportCustomsAgent
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.LoadPort
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideConsigneeContact
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideConsigneeContact.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideDeliveryAddress
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideDeliveryAddress.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideNotifyParty
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideNotifyParty.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideNotifyPartyAddress
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideNotifyPartyAddress.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverridePickupAddress
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverridePickupAddress.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideSupplierContact
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.OverrideSupplierContact.OrgHeader
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.PickupCartageContractor
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.PlaceOfDeliveryPort
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.PlaceOfReceivalPort
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.ReceivingAgent
OrgHeader.OrgSupplierBuyerLink_Supplier.OrgSupBuyLinkTrnMode.SendingAgent
OrgHeader.OrgSupplierBuyerLink_Supplier.Supplier
OrgHeader.OrgWebURL
OrgHeader.OrgWhsClientAccountAssociation
OrgHeader.OrgWhsClientAccountAssociation.BillToCarrierAccount
OrgHeader.OrgWhsClientAccountAssociation.BillToCarrierAccount.Carrier
OrgHeader.OrgWhsClientAccountAssociation.CarrierAccount
OrgHeader.OrgWhsClientAccountAssociation.CarrierAccount.Carrier
OrgHeader.OrgWhsClientAccountAssociation.DutyBillToCarrierAccount
OrgHeader.OrgWhsClientAccountAssociation.DutyBillToCarrierAccount.Carrier
OrgHeader.OrgWhsClientAccountAssociation.SalesChannel
OrgHeader.OrgWhsClientAccountAssociation.Warehouse
OrgHeader.ShippingLine
OrgHeader.StmNote
OrgHeader.StmNote.RelatedCompany
".Trim()
				, string.Join("\r\n", entities.OrderBy(entity => entity.FullName).Select(entity => entity.FullName)));

			Assert("Should not have a parent", !entities.HasDefinition("Parent"));
			Assert(entities.FindDefinition("OrgHeader.OrgCusCode").PropertyIsExcluded("OK_UnSignedCustomsRegNo"));
		}

		public void TestEDICodeMappingDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("EDICodeMapping");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("OrgPatternMatchOverride", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in OrgSupplierPart EntitySet"
				, @"
OrgPatternMatchOverride
OrgPatternMatchOverride.OrgHeader
				".Trim()
				, string.Join("\r\n", entities.Select(entity => entity.FullName).OrderBy(s => s)));
		}

		public void TestCountryDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("Country");
			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("RefCountry", root.EntityName);
			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in Country EntitySet"
				, @"
RefCountry
RefCountry.AirWaybillCurrency
RefCountry.LocalCurrency
RefCountry.RefCountryRequiredDocument_Destination
RefCountry.RefCountryRequiredDocument_Destination.Origin
RefCountry.RefCountryRequiredDocument_Origin
RefCountry.RefCountryRequiredDocument_Origin.Destination
RefCountry.RefCountryRules_Destination
RefCountry.RefCountryRules_Destination.Origin
RefCountry.RefCountryRules_Destination.RefServiceLevel
RefCountry.RefCountryRules_Origin
RefCountry.RefCountryRules_Origin.Destination
RefCountry.RefCountryRules_Origin.RefServiceLevel
RefCountry.RefCountryStates
RefCountry.RefCountryStates.RefCityTown
RefCountry.RefCountryStates.RefCityTown.Country
RefCountry.RefCountryStates.RefCityTown.Country.CountryCodeExternal
RefCountry.RefCountryStates.RefCityTown.RefPostCode
RefCountry.RefCountryStates.RefCityTown.RefPostCode.Country
RefCountry.RefCountryStates.RefCityTown.TimeZone
RefCountry.RefCountryStates.RefCityTown.TimeZone.DaylightSavingZone
RefCountry.RefCountryStates.RefCityTown.TimeZone.StandardZone
RefCountry.RefCountryStates.RefDomesticCartageZone
RefCountry.RefCountryStates.RefDomesticCartageZone.Loco
RefCountry.RefCountryStates.RefLatLongPostcode
RefCountry.RefCountryStates.RefLatLongPostcode.Country
RefCountry.RefLocoMap
RefCountry.RefLocoMap.LocoPort
RefCountry.RefUNLOCO
RefCountry.RefUNLOCO.RefCountryStates
RefCountry.RefUNLOCO.RefCountryStates.CountryCodeExternal
RefCountry.RefUNLOCO.RefTimeZoneSet
RefCountry.RefUNLOCO.RefTimeZoneSet.DaylightSavingZone
RefCountry.RefUNLOCO.RefTimeZoneSet.StandardZone
RefCountry.RefZoneHeader
RefCountry.RefZoneHeader.RelatedParty
				".Trim()
				, string.Join("\r\n", entities.OrderBy(entity => entity.FullName).Select(entity => entity.FullName).ToArray()));
		}

		public void TestUnlocoDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("UNLOCO");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("RefUNLOCO", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in UNLOCO EntitySet"
				, @"
RefUNLOCO
RefUNLOCO.CountryCode
RefUNLOCO.RefCountryStates
RefUNLOCO.RefCountryStates.CountryCodeExternal
RefUNLOCO.RefTimeZoneSet
RefUNLOCO.RefTimeZoneSet.DaylightSavingZone
RefUNLOCO.RefTimeZoneSet.StandardZone
RefUNLOCO.RefZonePivot
RefUNLOCO.RefZonePivot.RefZoneHeader
				".Trim()
				, string.Join("\r\n", entities.OrderBy(entity => entity.FullName).Select(entity => entity.FullName).ToArray()));

			Assert("Should have entity with full name: RefUNLOCO", entities.HasDefinition("RefUNLOCO"));
			Assert("Should have entity with full name: RefUNLOCO.RefZonePivot", entities.HasDefinition("RefUNLOCO.RefZonePivot"));

			var unloco = entities.FindDefinition("RefUNLOCO");
			var zonePivot = entities.FindDefinition("RefUNLOCO.RefZonePivot");
			Assert(unloco.HasMany().Contains(zonePivot));
			AssertEquals(zonePivot.Parent, unloco);
		}

		public void TestOrderDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("Order");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("JobOrderHeader", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;

			AssertMultilineASCIIEquals("Entities in Order EntitySet"
				, @"
JobOrderHeader
JobOrderHeader.ArrivalVessel
JobOrderHeader.BuyerAddress
JobOrderHeader.BuyerAddress.CountryCode
JobOrderHeader.BuyerAddress.OrgHeader
JobOrderHeader.BuyerAddress.RelatedPortCode
JobOrderHeader.BuyerContact
JobOrderHeader.BuyerContact.AddressOverride
JobOrderHeader.BuyerContact.GlbPerson
JobOrderHeader.BuyerContact.Nationality
JobOrderHeader.BuyerContact.OrgAddress
JobOrderHeader.BuyerContact.OrgAddress.OrgHeader
JobOrderHeader.BuyerContact.OrgHeader
JobOrderHeader.Carrier
JobOrderHeader.CountryOfSupply
JobOrderHeader.DepartureVessel
JobOrderHeader.GoodsAvailableAt
JobOrderHeader.GoodsDeliveredTo
JobOrderHeader.IntermediateVessel
JobOrderHeader.JobDeclaration
JobOrderHeader.JobDeclaration.GlbCompany
JobOrderHeader.JobOrderContainer
JobOrderHeader.JobOrderContainer.RefContainer
JobOrderHeader.JobOrderLine
JobOrderHeader.JobOrderLine.CommodityCode
JobOrderHeader.JobOrderLine.CountryOfOrigin
JobOrderHeader.JobOrderLine.JobOrderLineDelivery
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.DeliveryAddr
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.DeliveryAddr.OrgHeader
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.DeliveryPoint
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.DeliveryPoint.OrgHeader
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.DestinationPort
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.JobOrderLineDeliverContainer
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.JobOrderLineDeliverContainer.ArrivalVessel
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.JobOrderLineDeliverContainer.ContainerType
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.JobOrderLineDeliverContainer.JobContainer
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.JobOrderLineDeliverContainer.LoadPort
JobOrderHeader.JobOrderLine.JobOrderLineDelivery.JobOrderLineDeliverContainer.PackType
JobOrderHeader.JobOrderLine.PackType
JobOrderHeader.JobOrderLine.Supplier
JobOrderHeader.JobShipment
JobOrderHeader.LandedCostHeader
JobOrderHeader.LandedCostHeader.GlbCompany
JobOrderHeader.LandedCostHeader.LandCostInput
JobOrderHeader.LandedCostHeader.LandCostInput.ChargeCode
JobOrderHeader.LandedCostHeader.LandCostInput.ChargeCode.GlbCompany
JobOrderHeader.LandedCostHeader.LandCostInput.CostCurrency
JobOrderHeader.LandedCostHeader.LandedCostHistory
JobOrderHeader.LandedCostHeader.LandedCostHistory.CountryOfEntry
JobOrderHeader.LandedCostHeader.LandedCostHistory.OrgSupplierPart
JobOrderHeader.OrderCurrency
JobOrderHeader.PackType
JobOrderHeader.PortOfDischarge
JobOrderHeader.PortOfLoading
JobOrderHeader.ReceivingAgent
JobOrderHeader.SendingAgent
JobOrderHeader.ServiceLevel_NI
JobOrderHeader.ShipmentPrePlanning
JobOrderHeader.SupplierAddress
JobOrderHeader.SupplierAddress.CountryCode
JobOrderHeader.SupplierAddress.OrgHeader
JobOrderHeader.SupplierAddress.RelatedPortCode
JobOrderHeader.SupplierContact
JobOrderHeader.SupplierContact.AddressOverride
JobOrderHeader.SupplierContact.GlbPerson
JobOrderHeader.SupplierContact.Nationality
JobOrderHeader.SupplierContact.OrgAddress
JobOrderHeader.SupplierContact.OrgAddress.OrgHeader
JobOrderHeader.SupplierContact.OrgHeader
				".Trim()
				, string.Join("\r\n", entities.OrderBy(entity => entity.FullName).Select(entity => entity.FullName)));

			var order = entities.FindDefinition("JobOrderHeader");

			var packType = entities.FindDefinition("JobOrderHeader.PackType");
			Assert(order.Parents.Contains(packType));

			var supplierAddress = entities.FindDefinition("JobOrderHeader.SupplierAddress");

			var buyerAddress = entities.FindDefinition("JobOrderHeader.BuyerAddress");
		}

		public void TestCurrencyDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("CurrencyExchangeRate");
			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("RefExchangeRate", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;

			AssertMultilineASCIIEquals("Entities in CurrencyExchangeRate EntitySet"
				, @"
RefExchangeRate
RefExchangeRate.Client
RefExchangeRate.GlbCompany
RefExchangeRate.RefCurrency
				".Trim()
				, string.Join("\r\n", entities.OrderBy(entity => entity.FullName).Select(entity => entity.FullName).ToArray()));
		}

		public void TestDeclarationDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("Declaration");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("JobDeclaration", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in Declaration EntitySet"
				, @"
JobDeclaration
JobDeclaration.AuditUser
JobDeclaration.Buyer
JobDeclaration.BuyingAgent
JobDeclaration.BuyingAgentAddress
JobDeclaration.BuyingAgentAddress.OrgHeader
JobDeclaration.Consignee
JobDeclaration.ConsigneeAddress
JobDeclaration.ConsigneeAddress.OrgHeader
JobDeclaration.ControllingAgent
JobDeclaration.ControllingCustomer
JobDeclaration.CusAgent
JobDeclaration.CusContainer
JobDeclaration.CusContainer.JobContainer
JobDeclaration.CusContainer.OwnerCountry
JobDeclaration.CusContainer.RefContainer
JobDeclaration.CusDecHouseBill
JobDeclaration.CusDecHouseBill.JobShipment
JobDeclaration.CusDecHouseBill.ParentBill
JobDeclaration.CusEntryHeader
JobDeclaration.CusEntryHeader.CusEntryHeaderCharges
JobDeclaration.CusEntryHeader.CusEntryLine
JobDeclaration.CusEntryHeader.CusEntryLine.CusEntryLineFee
JobDeclaration.CusEntryHeader.CusEntryLine.InvoiceAmountCurrency
JobDeclaration.CusEntryHeader.CusEntryNum
JobDeclaration.CusEntryHeader.CusEntryNum.CountryCode
JobDeclaration.CusEntryHeader.Instruction
JobDeclaration.CusEntryHeader.PrimeEntry
JobDeclaration.CusEntryNum
JobDeclaration.CusEntryNum.CountryCode
JobDeclaration.CustomsCommencedUser
JobDeclaration.DeclarantAddress
JobDeclaration.DeclarantAddress.OrgHeader
JobDeclaration.DistributorAddress
JobDeclaration.DistributorAddress.OrgHeader
JobDeclaration.DutyPayer
JobDeclaration.Exporter
JobDeclaration.ExternalBroker
JobDeclaration.FinalDestination
JobDeclaration.Forwarder
JobDeclaration.GlbBranch
JobDeclaration.GlbCompany
JobDeclaration.Importer
JobDeclaration.ImporterAddress
JobDeclaration.ImporterAddress.OrgHeader
JobDeclaration.InsuranceCurrency
JobDeclaration.JobComInvoiceHeader
JobDeclaration.JobComInvoiceHeader.Buyer
JobDeclaration.JobComInvoiceHeader.BuyerAddress
JobDeclaration.JobComInvoiceHeader.BuyerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.BuyerAgent
JobDeclaration.JobComInvoiceHeader.Consignee
JobDeclaration.JobComInvoiceHeader.ConsigneeAddress
JobDeclaration.JobComInvoiceHeader.ConsigneeAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.CountryOfExport
JobDeclaration.JobComInvoiceHeader.DefaultOrigin
JobDeclaration.JobComInvoiceHeader.DistributorAddress
JobDeclaration.JobComInvoiceHeader.DistributorAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.ExporterAddress
JobDeclaration.JobComInvoiceHeader.ExporterAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.GlbBranch
JobDeclaration.JobComInvoiceHeader.GroupInvoiceFK
JobDeclaration.JobComInvoiceHeader.IntermediateConsigneeAddress
JobDeclaration.JobComInvoiceHeader.IntermediateConsigneeAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.Invoice_Currency
JobDeclaration.JobComInvoiceHeader.InvoicerAddress
JobDeclaration.JobComInvoiceHeader.InvoicerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.Catalog
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ClassUsageCommentReviewer
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.Commodity_Code
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ConsigneeAddress
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ConsigneeAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.CountryOfExport
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.CusClassification
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.CusClassification.CountryCodeExternal
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.CusContainer
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.CusEntryInstruction
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.CusEntryLine
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ExporterAddress
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ExporterAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.GrowerAddress
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.GrowerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.JobOrderLine
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.JobOrderLine.JobOrderHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ManufacturerAddress
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ManufacturerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.OrgSupplierPart
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ProducerAddress
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ProducerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.Seller
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.Seller.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ShipToPartyAddress
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.ShipToPartyAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.SoldToPartyAddress
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.SoldToPartyAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.Supplier
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.TaxType
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.TaxType.DataGroupingExternal
JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine.TreatmentProvider
JobDeclaration.JobComInvoiceHeader.Manufacturer
JobDeclaration.JobComInvoiceHeader.ManufacturerAddress
JobDeclaration.JobComInvoiceHeader.ManufacturerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.OriginState
JobDeclaration.JobComInvoiceHeader.OriginState.CountryCodeExternal
JobDeclaration.JobComInvoiceHeader.PackagerAddress
JobDeclaration.JobComInvoiceHeader.PackagerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.RelatedHouseBill
JobDeclaration.JobComInvoiceHeader.SellerAddress
JobDeclaration.JobComInvoiceHeader.SellerAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.SellingAgent
JobDeclaration.JobComInvoiceHeader.ShipperAddress
JobDeclaration.JobComInvoiceHeader.ShipperAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.ShipToPartyAddress
JobDeclaration.JobComInvoiceHeader.ShipToPartyAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.SoldToPartyAddress
JobDeclaration.JobComInvoiceHeader.SoldToPartyAddress.OrgHeader
JobDeclaration.JobComInvoiceHeader.Supplier
JobDeclaration.JobComInvoiceHeader.SupplierAddress
JobDeclaration.JobComInvoiceHeader.SupplierAddress.OrgHeader
JobDeclaration.JobDocsAndCartage
JobDeclaration.JobDocsAndCartage.DeliveryCartageCoAddr
JobDeclaration.JobDocsAndCartage.DeliveryCartageCoAddr.OrgHeader
JobDeclaration.JobDocsAndCartage.JobOrderItem
JobDeclaration.JobDocsAndCartage.JobService
JobDeclaration.JobDocsAndCartage.JobService.Contractor
JobDeclaration.JobDocsAndCartage.JobService.GlbCompany
JobDeclaration.JobDocsAndCartage.JobService.Location
JobDeclaration.JobDocsAndCartage.JobService.Location.OrgHeader
JobDeclaration.JobDocsAndCartage.JobService.RefEquipment
JobDeclaration.JobDocsAndCartage.JobService.ServiceRateCurrency
JobDeclaration.JobDocsAndCartage.PickupCartageCoAddr
JobDeclaration.JobDocsAndCartage.PickupCartageCoAddr.OrgHeader
JobDeclaration.JobShipment
JobDeclaration.LandedCostHeader
JobDeclaration.LandedCostHeader.GlbCompany
JobDeclaration.LandedCostHeader.LandCostInput
JobDeclaration.LandedCostHeader.LandCostInput.ChargeCode
JobDeclaration.LandedCostHeader.LandCostInput.ChargeCode.GlbCompany
JobDeclaration.LandedCostHeader.LandCostInput.CostCurrency
JobDeclaration.LandedCostHeader.LandedCostHistory
JobDeclaration.LandedCostHeader.LandedCostHistory.CountryOfEntry
JobDeclaration.LandedCostHeader.LandedCostHistory.OrgSupplierPart
JobDeclaration.Manufacturer
JobDeclaration.ManufacturerAddress
JobDeclaration.ManufacturerAddress.OrgHeader
JobDeclaration.NotifyParty
JobDeclaration.Origin
JobDeclaration.OriginState
JobDeclaration.OriginState.CountryCodeExternal
JobDeclaration.PackagerAddress
JobDeclaration.PackagerAddress.OrgHeader
JobDeclaration.PortOfArrival
JobDeclaration.PortOfFirstArrival
JobDeclaration.PortOfLoading
JobDeclaration.Representative
JobDeclaration.Representative.OrgHeader
JobDeclaration.SellerAddress
JobDeclaration.SellerAddress.OrgHeader
JobDeclaration.SellingAgent
JobDeclaration.ServiceLevel
JobDeclaration.ShipperAddress
JobDeclaration.ShipperAddress.OrgHeader
JobDeclaration.ShippingLine
JobDeclaration.ShipToPartyAddress
JobDeclaration.ShipToPartyAddress.OrgHeader
JobDeclaration.SoldToPartyAddress
JobDeclaration.SoldToPartyAddress.OrgHeader
JobDeclaration.Supplier
JobDeclaration.SupplierAddress
JobDeclaration.SupplierAddress.OrgHeader
JobDeclaration.Trailer1Nationality
JobDeclaration.Trailer2Nationality
JobDeclaration.TransportNationality
JobDeclaration.TransportNationalityInland
JobDeclaration.Vessel
				".Trim()
				, string.Join("\r\n", entities.OrderBy(entity => entity.FullName).Select(entity => entity.FullName).ToArray()));

			AssertEquals("Should only have one Supplier for JobDeclaration", 1, root.BelongsTo().Count(parent => parent.FullName == "JobDeclaration.Supplier"));
			AssertEquals("Should only have one Importer for JobDeclaration", 1, root.BelongsTo().Count(parent => parent.FullName == "JobDeclaration.Importer"));
			AssertEquals("Should only have one ShippingLine for JobDeclaration", 1, root.BelongsTo().Count(parent => parent.FullName == "JobDeclaration.ShippingLine"));
			AssertEquals("Should only have one Forwarder for JobDeclaration", 1, root.BelongsTo().Count(parent => parent.FullName == "JobDeclaration.Forwarder"));
		}

		public void TestStaffDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("Staff");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("GlbStaff", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in GlbStaff EntitySet"
				, @"
GlbStaff
GlbStaff.CountryCode
GlbStaff.GenRegCertAccredMaintList
GlbStaff.GenRegCertAccredMaintList.CountryOfIssuance
GlbStaff.GlbGroupLink
GlbStaff.GlbGroupLink.GlbGroup
GlbStaff.GlbStaffHoliday
GlbStaff.GlbWorkTime
GlbStaff.HomeBranch
GlbStaff.HomeDepartment
GlbStaff.LastLogonBranch
GlbStaff.LastLogonDepartment
GlbStaff.NationalityCode
GlbStaff.PreferredPaymentCompany
				".Trim()
				, string.Join("\r\n", entities.Select(entity => entity.FullName).OrderBy(s => s)));

			Assert(entities.FindDefinition("GlbStaff").PropertyIsExcludedFromExport("GS_PasswordHash"));
			Assert(entities.FindDefinition("GlbStaff").PropertyIsExcludedFromExport("GS_PasswordSalt"));
			Assert(entities.FindDefinition("GlbStaff").PropertyIsExcludedFromExport("GS_PasswordHashIterations"));
			Assert(entities.FindDefinition("GlbStaff").PropertyIsExcluded("GS_LastPasswordChangeDate"));
			Assert(entities.FindDefinition("GlbStaff").PropertyIsExcluded("GS_IsSystemAccount"));
			Assert(entities.FindDefinition("GlbStaff").PropertyIsExcluded("GS_IsDeveloper"));
			Assert(entities.FindDefinition("GlbStaff").PropertyIsExcludedFromExport("GS_SqlLoginPasswordHash"));
		}

		// Change this test to check whether definition contains CusCALPCO structure.
		public void TestProductDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("Product");

			var root = info.Root;

			AssertNotNull(root);
			AssertEquals("OrgSupplierPart", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in OrgSupplierPart EntitySet"
				, @"
OrgSupplierPart
OrgSupplierPart.CommodityCode
OrgSupplierPart.CusClassPartPivot
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.AdditionalInformationGrandChild
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.AdditionalInformationGrandChild.AdditionalInformationGreatGrandChild
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.Applicant
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.Applicant.OrgHeader
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.AuthorizationCountry
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.Holder
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.Holder.OrgHeader
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.IssuanceCountryCode
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.OriginCountryCode
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCALPCO.SmeltAndPourCountryCode
OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild.CusCodeDataFdaAffirmation
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.AdditionalInformationGrandChild
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.AdditionalInformationGrandChild.AdditionalInformationGreatGrandChild
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.Applicant
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.Applicant.OrgHeader
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.AuthorizationCountry
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.Holder
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.Holder.OrgHeader
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.IssuanceCountryCode
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.OriginCountryCode
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCALPCO.SmeltAndPourCountryCode
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.AdditionalInformationChild.CusCodeDataFdaAffirmation
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.Country
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CountryOfExport
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CountryOfOrigin
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusClassification
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusClassification.CountryCodeExternal
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusCodeDataCensus
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.CastCountry
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.CertificateOrigin
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.CountryOfExport
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.CountryOfOrigin
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Exporter
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Exporter.OrgHeader
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Manufacturer
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode.CodeCountry
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Manufacturer.OrgHeader
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.MeltCountry
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Nine802ValuePerUnitCurr
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.PerUnitCostCurr
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.PrimaryCountry
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.SecondaryCountry
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.OrgHeader
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.OrgSupplierPart
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.OriginState
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.OriginState.CountryCodeExternal
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.TaxType
OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.TaxType.DataGroupingExternal
OrgSupplierPart.CusClassPartPivot.Country
OrgSupplierPart.CusClassPartPivot.CountryOfExport
OrgSupplierPart.CusClassPartPivot.CountryOfOrigin
OrgSupplierPart.CusClassPartPivot.CusAttributeFilter
OrgSupplierPart.CusClassPartPivot.CusCAClassification
OrgSupplierPart.CusClassPartPivot.CusCAClassification.CFIAOrigin
OrgSupplierPart.CusClassPartPivot.CusCAClassification.Manufacturer
OrgSupplierPart.CusClassPartPivot.CusCAClassification.Manufacturer.OrgCusCode
OrgSupplierPart.CusClassPartPivot.CusCAClassification.Manufacturer.OrgCusCode.CodeCountry
OrgSupplierPart.CusClassPartPivot.CusCAClassification.Manufacturer.OrgHeader
OrgSupplierPart.CusClassPartPivot.CusCAClassification.Origin
OrgSupplierPart.CusClassPartPivot.CusCAClassification.Source
OrgSupplierPart.CusClassPartPivot.CusClassification
OrgSupplierPart.CusClassPartPivot.CusClassification.CountryCodeExternal
OrgSupplierPart.CusClassPartPivot.CusCNClassification
OrgSupplierPart.CusClassPartPivot.CusCNClassification.Manufacturer
OrgSupplierPart.CusClassPartPivot.CusCNClassification.Manufacturer.OrgCusCode
OrgSupplierPart.CusClassPartPivot.CusCNClassification.Manufacturer.OrgCusCode.CodeCountry
OrgSupplierPart.CusClassPartPivot.CusCNClassification.Manufacturer.OrgHeader
OrgSupplierPart.CusClassPartPivot.CusCNClassification.TradeUnitPriceCurrency
OrgSupplierPart.CusClassPartPivot.CusCodeDataCensus
OrgSupplierPart.CusClassPartPivot.CusLineTariffDetail
OrgSupplierPart.CusClassPartPivot.CusSupportingInfo
OrgSupplierPart.CusClassPartPivot.CusSupportingInfo.CountryCode
OrgSupplierPart.CusClassPartPivot.CusSupportingInfo.Currency
OrgSupplierPart.CusClassPartPivot.CusSupportingInfo.SupportingInfo
OrgSupplierPart.CusClassPartPivot.CusUSClassification
OrgSupplierPart.CusClassPartPivot.CusUSClassification.CastCountry
OrgSupplierPart.CusClassPartPivot.CusUSClassification.CertificateOrigin
OrgSupplierPart.CusClassPartPivot.CusUSClassification.CountryOfExport
OrgSupplierPart.CusClassPartPivot.CusUSClassification.CountryOfOrigin
OrgSupplierPart.CusClassPartPivot.CusUSClassification.Exporter
OrgSupplierPart.CusClassPartPivot.CusUSClassification.Exporter.OrgHeader
OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer
OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode
OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode.CodeCountry
OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer.OrgHeader
OrgSupplierPart.CusClassPartPivot.CusUSClassification.MeltCountry
OrgSupplierPart.CusClassPartPivot.CusUSClassification.Nine802ValuePerUnitCurr
OrgSupplierPart.CusClassPartPivot.CusUSClassification.PerUnitCostCurr
OrgSupplierPart.CusClassPartPivot.CusUSClassification.PrimaryCountry
OrgSupplierPart.CusClassPartPivot.CusUSClassification.SecondaryCountry
OrgSupplierPart.CusClassPartPivot.OrgHeader
OrgSupplierPart.CusClassPartPivot.OrgHeader.OrgCusCode
OrgSupplierPart.CusClassPartPivot.OrgHeader.OrgCusCode.CodeCountry
OrgSupplierPart.CusClassPartPivot.OriginState
OrgSupplierPart.CusClassPartPivot.OriginState.CountryCodeExternal
OrgSupplierPart.CusClassPartPivot.StmNote
OrgSupplierPart.CusClassPartPivot.StmNote.RelatedCompany
OrgSupplierPart.CusClassPartPivot.TaxType
OrgSupplierPart.CusClassPartPivot.TaxType.DataGroupingExternal
OrgSupplierPart.GenCustomAddOnValue
OrgSupplierPart.GenCustomAddOnValue.Rule
OrgSupplierPart.LastWeightedCostCurr
OrgSupplierPart.OrgPartBOM
OrgSupplierPart.OrgPartBOM.Component
OrgSupplierPart.OrgPartBOM.Component.ComponentOrgPartRelation
OrgSupplierPart.OrgPartBOM.Component.ComponentOrgPartRelation.OrgHeader
OrgSupplierPart.OrgPartBOM.PackType
OrgSupplierPart.OrgPartLocation
OrgSupplierPart.OrgPartRelation
OrgSupplierPart.OrgPartRelation.CartonGroup
OrgSupplierPart.OrgPartRelation.Category
OrgSupplierPart.OrgPartRelation.DefaultInventoryHoldCode
OrgSupplierPart.OrgPartRelation.OrgHeader
OrgSupplierPart.OrgPartRelation.OrgHeader.OrgCusCode
OrgSupplierPart.OrgPartRelation.OrgHeader.OrgCusCode.CodeCountry
OrgSupplierPart.OrgPartRelation.RoyaltyCurrency
OrgSupplierPart.OrgPartRelation.UnitPriceCurrency
OrgSupplierPart.OrgPartUnit
OrgSupplierPart.OrgSecondaryPartBOM
OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot
OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM
OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.Component
OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.Component.ComponentOrgPartRelation
OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.Component.ComponentOrgPartRelation.OrgHeader
OrgSupplierPart.OrgSecondaryPartBOM.OrgSecondaryPartBOMPivot.ComponentOrgPartBOM.PackType
OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart
OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation
OrgSupplierPart.OrgSecondaryPartBOM.SecondaryPart.SecondaryPartOrgPartRelation.OrgHeader
OrgSupplierPart.OrgSupplierPartBarcode
OrgSupplierPart.OrgSupplierPartBarcode.PackType
OrgSupplierPart.PackType
OrgSupplierPart.ProductStyleClassification
OrgSupplierPart.ProductStyleClassification.ClassificationProductStyle
OrgSupplierPart.ProductStyleClassification.ClassificationProductStyle.Owner
OrgSupplierPart.ProductStyleColour
OrgSupplierPart.ProductStyleColour.ColourProductStyle
OrgSupplierPart.ProductStyleColour.ColourProductStyle.Owner
OrgSupplierPart.ProductStyleSize
OrgSupplierPart.ProductStyleSize.SizeProductStyle
OrgSupplierPart.ProductStyleSize.SizeProductStyle.Owner
OrgSupplierPart.StmNote
OrgSupplierPart.StmNote.RelatedCompany
OrgSupplierPart.UNDGDataItem
OrgSupplierPart.UNDGDataItem.DGContact
OrgSupplierPart.UNDGDataItem.DGContact.OrgHeader
OrgSupplierPart.UNDGDataItem.PackType
OrgSupplierPart.UNDGDataItem.UNDGSubstance
OrgSupplierPart.UNDGDataItem.UNDGSubstancePivot
OrgSupplierPart.WhsPickFace
OrgSupplierPart.WhsPickFace.Client
OrgSupplierPart.WhsPickFace.WhsLocation
OrgSupplierPart.WhsProductParamsByWhsAndClient
OrgSupplierPart.WhsProductParamsByWhsAndClient.DynamicPickFaceArea
OrgSupplierPart.WhsProductParamsByWhsAndClient.DynamicPickFaceArea.Whs
OrgSupplierPart.WhsProductParamsByWhsAndClient.InwardsProcessingStagingLocationBOM
OrgSupplierPart.WhsProductParamsByWhsAndClient.OrgHeader
OrgSupplierPart.WhsProductParamsByWhsAndClient.PutawayGroup
OrgSupplierPart.WhsProductParamsByWhsAndClient.ReceivedPackType
OrgSupplierPart.WhsProductParamsByWhsAndClient.ReleasedPackType
OrgSupplierPart.WhsProductParamsByWhsAndClient.StagingLocationBOM
OrgSupplierPart.WhsProductParamsByWhsAndClient.WhsWarehouse
				".Trim()
				, string.Join("\r\n", entities.Select(entity => entity.FullName).OrderBy(s => s)));

			Assert(entities.FindDefinition("OrgSupplierPart.CusClassPartPivot.ComponentCusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode").PropertyIsExcluded("OK_UnSignedCustomsRegNo"));
			Assert(entities.FindDefinition("OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode").PropertyIsExcluded("OK_UnSignedCustomsRegNo"));
			Assert(entities.FindDefinition("OrgSupplierPart.CusClassPartPivot.OrgHeader.OrgCusCode").PropertyIsExcluded("OK_UnSignedCustomsRegNo"));
			Assert(entities.FindDefinition("OrgSupplierPart.OrgPartRelation.OrgHeader.OrgCusCode").PropertyIsExcluded("OK_UnSignedCustomsRegNo"));
			Assert(entities.FindDefinition("OrgSupplierPart.UNDGDataItem.UNDGSubstancePivot").IncludeParentTableCode);
		}

		void AssertFullNamesShouldBeUnique(IEntitySetDefinition entitySetDefinition)
		{
			var fullNameList = new List<string>();
			foreach (var entityDefinition in entitySetDefinition.Entities)
			{
				var errorMsg = "Full Names should be unique for EntitySet:" + entitySetDefinition.Name;
				Assert(errorMsg, !fullNameList.Contains(entityDefinition.FullName));
				fullNameList.Add(entityDefinition.FullName);
			}
		}

		public void TestTransportZoneSetDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("TransportZone");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("RateTransportProvider", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in RateTransportProvider EntitySet"
				, @"
RateTransportProvider
RateTransportProvider.Country
RateTransportProvider.RateTransportZones
RateTransportProvider.RateTransportZones.RateTransportZoneItem
RateTransportProvider.RateTransportZones.RateTransportZoneItem.CityTown
RateTransportProvider.RateTransportZones.RateTransportZoneItem.CityTown.CountryExternal
RateTransportProvider.RateTransportZones.RateTransportZoneItem.CityTown.StateExternal
RateTransportProvider.RateTransportZones.RateTransportZoneItem.CityTown.StateExternal.CountryCodeExternal
RateTransportProvider.RateTransportZones.RateTransportZoneItem.Country
RateTransportProvider.RelatedParty
RateTransportProvider.ZoneHubLocation
RateTransportProvider.ZoneHubLocation.CountryExternal
RateTransportProvider.ZoneHubLocation.StateExternal
RateTransportProvider.ZoneHubLocation.StateExternal.CountryCodeExternal
						".Trim()
				, string.Join("\r\n", entities.Select(entity => entity.FullName).OrderBy(s => s)));
		}

		public void TestWarehouseClientAccountAssociationSetDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("WarehouseClientAccountAssociation");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("OrgWhsClientAccountAssociation", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in OrgWhsClientAccountAssociation EntitySet"
				, @"
OrgWhsClientAccountAssociation
OrgWhsClientAccountAssociation.BillToCarrierAccount
OrgWhsClientAccountAssociation.BillToCarrierAccount.Carrier
OrgWhsClientAccountAssociation.CarrierAccount
OrgWhsClientAccountAssociation.CarrierAccount.BillToParty
OrgWhsClientAccountAssociation.CarrierAccount.Carrier
OrgWhsClientAccountAssociation.Client
OrgWhsClientAccountAssociation.Client.ClosestPort
OrgWhsClientAccountAssociation.Client.ShippingLine
OrgWhsClientAccountAssociation.DutyBillToCarrierAccount
OrgWhsClientAccountAssociation.DutyBillToCarrierAccount.Carrier
OrgWhsClientAccountAssociation.SalesChannel
OrgWhsClientAccountAssociation.Warehouse
OrgWhsClientAccountAssociation.Warehouse.DefaultLocationType
OrgWhsClientAccountAssociation.Warehouse.DGContact
OrgWhsClientAccountAssociation.Warehouse.DGContact.OrgHeader
OrgWhsClientAccountAssociation.Warehouse.RelatedCompanyBranch
OrgWhsClientAccountAssociation.Warehouse.ReleaseGroup
OrgWhsClientAccountAssociation.Warehouse.WarehouseAddress
OrgWhsClientAccountAssociation.Warehouse.WarehouseAddress.OrgHeader
						".Trim()
				, string.Join("\r\n", entities.Select(entity => entity.FullName).OrderBy(s => s)));
		}

		public void TestCarrierAccountSetDefinition()
		{
			var info = TestUtil.GetEntitySetDefinition("CarrierAccount");

			var root = info.Root;
			AssertNotNull(root);
			AssertEquals("OrgCarrierAccount", root.EntityName);

			AssertFullNamesShouldBeUnique(info);

			var entities = info.Entities;
			AssertMultilineASCIIEquals("Entities in OrgCarrierAccount EntitySet"
				, @"
OrgCarrierAccount
OrgCarrierAccount.BillToParty
OrgCarrierAccount.BillToParty.ClosestPort
OrgCarrierAccount.BillToParty.ShippingLine
OrgCarrierAccount.Carrier
OrgCarrierAccount.Carrier.ClosestPort
OrgCarrierAccount.Carrier.ShippingLine
						".Trim()
				, string.Join("\r\n", entities.Select(entity => entity.FullName).OrderBy(s => s)));
		}
	}
}
