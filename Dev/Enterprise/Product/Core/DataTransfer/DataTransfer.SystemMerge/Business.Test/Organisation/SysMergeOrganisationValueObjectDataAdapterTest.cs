using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.Xml.Testing;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeOrganisationValueObjectDataAdapterTest : TestCaseWithFactory
	{
		#region TestBrandNamesAreExportedProperly

		public void TestBrandNamesAreExportedProperly()
		{
			OrgHeaderForDataTransfer org = Factory.New<OrgHeaderForDataTransfer>();
			org.OH_Code = "TEMPORG";
			OrgBrandOrRelatedName brand = Factory.New<OrgBrandOrRelatedName>();
			brand.P1_OH = org.PK;
			brand.P1_RelatedName = "Company Brand Name";

			Xsd.SysMergeOrganisation xmlOrganisation = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(1, xmlOrganisation.OrganisationDetail.BrandNames.Count);
			AssertEquals("Brand Names", "Company Brand Name", xmlOrganisation.OrganisationDetail.BrandNames[0].Value);
		}

		#endregion

		#region TestExportMultipleCustomsCodes

		public void TestExportMultipleCustomsCodes()
		{
			OrgHeaderForDataTransfer org = Factory.New<OrgHeaderForDataTransfer>();
			org.OH_Code = "TEMPORG";

			OrgCusCode customsCode1 = Factory.New<OrgCusCode>();
			customsCode1.OK_OH = org.PK;
			customsCode1.OK_CodeType = "CSC";
			customsCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			customsCode1.OK_CustomsRegNo = "12345678A";

			OrgCusCode customsCode2 = Factory.New<OrgCusCode>();
			customsCode2.OK_OH = org.PK;
			customsCode2.OK_CodeType = "CSC";
			customsCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			customsCode2.OK_CustomsRegNo = "87654321B";

			Xsd.SysMergeOrganisation xmlOrganisation = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));

			AssertNotNull("XmlOrganisation should be loaded from temporary OrgHeader", xmlOrganisation);
			AssertEquals("XmlOrganisation should contain 2 registration numbers", 2, xmlOrganisation.OrganisationDetail.OrgCusCodes.Count);

			AssertEquals("First registration number type should be CSC", "CSC", xmlOrganisation.OrganisationDetail.OrgCusCodes[0].CodeType);
			AssertEquals("First registration number should be 12345678A", "12345678A", xmlOrganisation.OrganisationDetail.OrgCusCodes[0].CustomsRegNo);
			AssertEquals("First CountryOfRegistration should be Australia", "AU", xmlOrganisation.OrganisationDetail.OrgCusCodes[0].RefCountryNk);

			AssertEquals("Second registration number type should be CSC", "CSC", xmlOrganisation.OrganisationDetail.OrgCusCodes[1].CodeType);
			AssertEquals("First registration number should be 87654321B", "87654321B", xmlOrganisation.OrganisationDetail.OrgCusCodes[1].CustomsRegNo);
			AssertEquals("First CountryOfRegistration should be NewZealand", "NZ", xmlOrganisation.OrganisationDetail.OrgCusCodes[1].RefCountryNk);
		}

		#endregion

		#region TestExportImportSysMergeOrg

		public void TestExportImportSysMergeOrg()
		{
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			// Initiallise BusinessObject
			var org = GetNewTestOrg();

			// Export BusinessObject To ValueObject
			//org.AllRelatedParties.Sort("PR_PartyType");
			var xsdOrganisation1 = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));

			// Write ValueObject to XML for Compare
			var exportedValueObjectXml1 = WriteValueObjectToXml(xsdOrganisation1);

			// Import from XMLValueObject to BusinessObject
			var otherFactory = NewFactory();
			otherFactory.NewWithValidTestData<RefAirline>();

			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, new NotificationBuffer());
			var importFromXsdOrg = otherFactory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(new Guid(xsdOrganisation1.OrganisationDetail.OrgHeader.PK));

			((DataTransfer.Xml.XsdVersion1.XmlInterchange)contextInOtherFactory.Interchange).ImportEDICode = true;
			DataAdapter.ImportFromValueObject(importFromXsdOrg, xsdOrganisation1, contextInOtherFactory);

			// Export BusinessObject To ValueObject
			//ImportFromXsdOrg.AllRelatedParties.Sort("PR_PartyType");
			var xsdOrganisation2 = DataAdapter.ExportToValueObject(importFromXsdOrg, new ValueObjectExportContext(new NotificationBuffer()));

			// Write ValueObject to XML for Compare
			var exportedValueObjectXml2 = WriteValueObjectToXml(xsdOrganisation2);
			AssertMultilineASCIIEquals("Comparing 2 ValueObject in XML format", exportedValueObjectXml1, exportedValueObjectXml2);
		}

		public void TestExportImportSysMergeOrgScreenStatus()
		{
			var org = GetNewTestOrg();
			var xsdOrganisation1 = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));

			var exportedValueObjectXml1 = WriteValueObjectToXml(xsdOrganisation1);
			var otherFactory = NewFactory();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, new NotificationBuffer());
			var importFromXsdOrg = otherFactory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(new Guid(xsdOrganisation1.OrganisationDetail.OrgHeader.PK));
			((DataTransfer.Xml.XsdVersion1.XmlInterchange)contextInOtherFactory.Interchange).ImportEDICode = true;
			DataAdapter.ImportFromValueObject(importFromXsdOrg, xsdOrganisation1, contextInOtherFactory);

			AssertEquals("MAT", importFromXsdOrg.OH_ScreeningStatus);
		}

		public void TestRelatedPartiesWithSameUniqueIndex()
		{
			var org = Factory.New<OrgHeader>();

			var someRelatedOrg1 = Factory.New<OrgHeaderForDataTransfer>();
			var someRelatedOrg2 = Factory.New<OrgHeaderForDataTransfer>();

			var orgRelatedParty1 = org.AllRelatedParties.AddNew();
			orgRelatedParty1.PR_FreightDirection = "AAA";
			orgRelatedParty1.PR_FreightTransportMode = "AAA";
			orgRelatedParty1.PR_FreightContainerMode = "AAA";
			orgRelatedParty1.PR_GC = ZGuid.Empty;
			orgRelatedParty1.PR_OH_RelatedParty = someRelatedOrg1.PK;
			orgRelatedParty1.PR_PartyType = "LTT";
			orgRelatedParty1.PR_Service = "AAA";
			orgRelatedParty1.PR_Location = "AAAAA";

			var orgRelatedParty2 = org.AllRelatedParties.AddNew();
			orgRelatedParty2.PR_FreightDirection = "AAA";
			orgRelatedParty2.PR_FreightTransportMode = "AAA";
			orgRelatedParty2.PR_FreightContainerMode = "AAA";
			orgRelatedParty2.PR_GC = ZGuid.Empty;
			orgRelatedParty2.PR_OH_RelatedParty = someRelatedOrg2.PK;
			orgRelatedParty2.PR_PartyType = "LTT";
			orgRelatedParty2.PR_Service = "AAA";
			orgRelatedParty2.PR_Location = "AAAAA";

			var orgForDataTransfer = Factory.Load<OrgHeaderForDataTransfer>(org.PK);

			var xsdOrganisation = DataAdapter.ExportToValueObject(orgForDataTransfer, new ValueObjectExportContext(new NotificationBuffer()));
			var xsdRelatedOrganisation1 = DataAdapter.ExportToValueObject(someRelatedOrg1, new ValueObjectExportContext(new NotificationBuffer()));
			var xsdRelatedOrganisation2 = DataAdapter.ExportToValueObject(someRelatedOrg2, new ValueObjectExportContext(new NotificationBuffer()));

			// Import from XMLValueObject to BusinessObject
			var otherFactory = NewFactory();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, new NotificationBuffer());
			var importFromXsdOrg = otherFactory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(new Guid(xsdOrganisation.OrganisationDetail.OrgHeader.PK));
			var importFromXsdRelatedOrg1 = otherFactory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(new Guid(xsdRelatedOrganisation1.OrganisationDetail.OrgHeader.PK));
			var importFromXsdRelatedOrg2 = otherFactory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(new Guid(xsdRelatedOrganisation2.OrganisationDetail.OrgHeader.PK));

			((DataTransfer.Xml.XsdVersion1.XmlInterchange)contextInOtherFactory.Interchange).ImportEDICode = true;
			DataAdapter.ImportFromValueObject(importFromXsdRelatedOrg1, xsdRelatedOrganisation1, contextInOtherFactory);
			DataAdapter.ImportFromValueObject(importFromXsdRelatedOrg2, xsdRelatedOrganisation2, contextInOtherFactory);
			DataAdapter.ImportFromValueObject(importFromXsdOrg, xsdOrganisation, contextInOtherFactory);

			AssertEquals("Only one related party should be imported.", 1, otherFactory.Load<OrgHeader>(org.PK).AllRelatedParties.Count);
			Assert("Should notice which organisation is being imported", contextInOtherFactory.LastNotificationMessage.Contains(string.Format("Error: Cannot create related party info for organization [({0}) - {1} - {2}].", org.PK, org.OH_Code, org.OH_FullName)));
		}

		#endregion

		#region TestExport_CustomLabels

		public void TestExport_CustomLabels()
		{
			var organisation = GetNewTestOrg();
			var customLabel1 = CreateCustomLabel(organisation, OrgConstants.CustomLabelType.Form, Constants.CustomLabels.WhsDocket.CustomAttribute1, "Custom Label");
			var customLabel2 = CreateCustomLabel(organisation, OrgConstants.CustomLabelType.Form, Constants.CustomLabels.WhsDocketLine.CustomAttribute1, "Custom Line Label");
			customLabel2.OT_ColumnSize = 100;
			customLabel2.OT_Hint = "Hint for Custom Label";
			customLabel2.OT_IsMandatory = true;
			customLabel2.OT_Position = 1;
			customLabel2.OT_Rule = "ZZZ";

			var xsdOrganisation = DataAdapter.ExportToValueObject(organisation, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Should export 2 custom labels.", 2, xsdOrganisation.OrganisationDetail.CustomLabels.Count);
			AssertContainsCustomLabel(xsdOrganisation.OrganisationDetail.CustomLabels, customLabel1);
			AssertContainsCustomLabel(xsdOrganisation.OrganisationDetail.CustomLabels, customLabel2);
		}

		OrgCustomLabels CreateCustomLabel(OrgHeaderForDataTransfer org, ZString type, ZString fieldName, ZString caption)
		{
			var customLabel = Factory.New<OrgCustomLabels>();
			customLabel.OT_OH = org.PK;
			customLabel.OT_Type = type;
			customLabel.OT_FieldName = fieldName;
			customLabel.OT_Caption = caption;

			return customLabel;
		}

		void AssertContainsCustomLabel(Xsd.SysMergeCustomLabelCollection xsdCustomLabelCollection, OrgCustomLabels expectedCustomLabel)
		{
			var xsdCustomLabel = xsdCustomLabelCollection.Cast<Xsd.SysMergeCustomLabel>().Single(l => l.PK == expectedCustomLabel.PK.ToString());
			AssertEquals("CustomLabel.Type", expectedCustomLabel.OT_Type, xsdCustomLabel.Type, xsdCustomLabel.TypeSpecified);
			AssertEquals("CustomLabel.FieldName", expectedCustomLabel.OT_FieldName, xsdCustomLabel.FieldName, xsdCustomLabel.FieldNameSpecified);
			AssertEquals("CustomLabel.Caption", expectedCustomLabel.OT_Caption, xsdCustomLabel.Caption, xsdCustomLabel.CaptionSpecified);
			AssertEquals("CustomLabel.Hint", expectedCustomLabel.OT_Hint, xsdCustomLabel.Hint, xsdCustomLabel.HintSpecified);
			AssertEquals("CustomLabel.Rule", expectedCustomLabel.OT_Rule, xsdCustomLabel.Rule, xsdCustomLabel.RuleSpecified);
			AssertEquals("CustomLabel.Position", expectedCustomLabel.OT_Position, xsdCustomLabel.Position, xsdCustomLabel.PositionSpecified);
			AssertEquals("CustomLabel.ColumnSize", expectedCustomLabel.OT_ColumnSize, xsdCustomLabel.ColumnSize, xsdCustomLabel.ColumnSizeSpecified);
			AssertEquals("CustomLabel.IsMandatory", expectedCustomLabel.OT_IsMandatory, xsdCustomLabel.IsMandatory, xsdCustomLabel.IsMandatorySpecified);
		}

		#endregion

		#region TestExport_WhsClientParametersByWarehouse

		public void TestExport_WhsClientParametersByWarehouse()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs1 = helper.CreateWarehouse("WHS1", "A");
			var whs2 = helper.CreateWarehouse("WHS2", "B");

			var whs1Area1 = helper.CreateWhsArea(whs1.PK, "Area 1");
			var whs1Area2 = helper.CreateWhsArea(whs1.PK, "Area 2");
			var whs2Area1 = helper.CreateWhsArea(whs2.PK, "Area 1");

			var orgWithNoWhsClientParamsPK = helper.CreateClient("CLIENT1");
			var xsdOrgWithNoWhsClientParams = DataAdapter.ExportToValueObject(Factory.Load<OrgHeaderForDataTransfer>(orgWithNoWhsClientParamsPK), new ValueObjectExportContext(new NotificationBuffer()));
			AssertNotNull("XsdOrganisation should be loaded from temporary OrgHeader", xsdOrgWithNoWhsClientParams);
			AssertEquals("This org had no WhsClientParameters so none should be exported for it.", 0, xsdOrgWithNoWhsClientParams.OrganisationDetail.WhsClientParametersByWarehouse.Count);

			var orgWithThreeWhsClientParamsPK = helper.CreateClient("CLIENT2");
			var clientParameter1 = helper.CreateWhsClientParameterByWarehouse(orgWithThreeWhsClientParamsPK, whs1.PK);
			var clientParameter2 = helper.CreateWhsClientParameterByWarehouse(orgWithThreeWhsClientParamsPK, whs1.PK);
			var clientParameter3 = helper.CreateWhsClientParameterByWarehouse(orgWithThreeWhsClientParamsPK, whs2.PK);
			var xsdOrgWithThreeWhsClientParams = DataAdapter.ExportToValueObject(Factory.Load<OrgHeaderForDataTransfer>(orgWithThreeWhsClientParamsPK), new ValueObjectExportContext(new NotificationBuffer()));
			AssertNotNull("XmlOrganisation should be loaded from temporary OrgHeader", xsdOrgWithThreeWhsClientParams);
			AssertEquals("This org have 3 WhsClientParameters and so 3 should be exported.", 3, xsdOrgWithThreeWhsClientParams.OrganisationDetail.WhsClientParametersByWarehouse.Count);
			AssertXsdWhsClientParameterByWarehouseExist(xsdOrgWithThreeWhsClientParams.OrganisationDetail.WhsClientParametersByWarehouse, clientParameter1);
			AssertXsdWhsClientParameterByWarehouseExist(xsdOrgWithThreeWhsClientParams.OrganisationDetail.WhsClientParametersByWarehouse, clientParameter2);
			AssertXsdWhsClientParameterByWarehouseExist(xsdOrgWithThreeWhsClientParams.OrganisationDetail.WhsClientParametersByWarehouse, clientParameter3);
		}

		void AssertXsdWhsClientParameterByWarehouseExist(Xsd.SysMergeWhsClientParameterByWarehouseCollection sysMergeWhsClientParameterByWarehouseCollection, IWhsClientParameterByWarehouse clientParameter)
		{
			foreach (Xsd.SysMergeWhsClientParameterByWarehouse xsdSysMergeWhsClientParam in sysMergeWhsClientParameterByWarehouseCollection)
			{
				if (xsdSysMergeWhsClientParam.PK == clientParameter.PK.ToString())
				{
					AssertEquals("SysMegerWhsClientParameterByWarehouse.Warehouse", clientParameter.WY_WW_Whs.ToString(), xsdSysMergeWhsClientParam.WarehousePK);
					return;
				}
			}
			Fail(string.Format("SysMegerWhsClientParameterByWarehouse for WhsClientParameterByWarehouse with PK = '{0}',  warehousePK = '{1}' was not found.", clientParameter.PK, clientParameter.WY_WW_Whs));
		}

		#endregion

		#region TestExport_MiscServSerialNumberIsKey

		public void TestExport_MiscServSerialNumberIsKey()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_IMSerialNumberIsKey = true;
			var orgForDataTransfer = Factory.Load<OrgHeaderForDataTransfer>(org.PK);
			var xsdOrg = DataAdapter.ExportToValueObject(orgForDataTransfer, new ValueObjectExportContext(new NotificationBuffer()));
			AssertNotNull("XsdOrganisation should be loaded from temporary OrgHeader", xsdOrg);
			AssertEquals(true, xsdOrg.OrganisationDetail.OrgMiscServ.SerialNumberIsKey);
		}

		#endregion

		#region TestImport_CustomLabels 

		public void TestImport_CustomLabels()
		{
			var organisation = GetNewTestOrg();
			var customLabel1 = CreateCustomLabel(organisation, OrgConstants.CustomLabelType.Form, Constants.CustomLabels.WhsDocket.CustomAttribute1, "Custom Label");
			var customLabel2 = CreateCustomLabel(organisation, OrgConstants.CustomLabelType.Form, Constants.CustomLabels.WhsDocketLine.CustomAttribute1, "Custom Line Label");
			customLabel2.OT_ColumnSize = 100;
			customLabel2.OT_Hint = "Hint for Custom Label";
			customLabel2.OT_IsMandatory = true;
			customLabel2.OT_Position = 1;
			customLabel2.OT_Rule = "ZZZ";
			AssertEquals("Precondition: Organisation should not be saved into DB, so that import can create labels", false, organisation.IsInDatabase);

			var xsdOrganisation = DataAdapter.ExportToValueObject(organisation, new ValueObjectExportContext(new NotificationBuffer()));

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			DataAdapter.CreateOrUpdateFromValueObject(xsdOrganisation, contextInOtherFactory);
			var importedOrgInOtherFactory = otherFactory.Load<OrgHeader>(organisation.PK);
			importedOrgInOtherFactory.CustomLabels.Load();
			AssertEquals("2 Custom Labels were exported, so 2 Custom Labels should be imported.", 2, importedOrgInOtherFactory.CustomLabels.Count);
			AssertContainsCustomLabel(importedOrgInOtherFactory.CustomLabels, customLabel1);
			AssertContainsCustomLabel(importedOrgInOtherFactory.CustomLabels, customLabel1);
		}

		void AssertContainsCustomLabel(OrgCustomLabelsCollection customLabelsCollection, OrgCustomLabels expectedCustomLabel)
		{
			var customLabel = customLabelsCollection.Cast<OrgCustomLabels>().Single(l => l.PK == expectedCustomLabel.PK);
			AssertEquals("CustomLabel.OT_Type", expectedCustomLabel.OT_Type, customLabel.OT_Type);
			AssertEquals("CustomLabel.OT_FieldName", expectedCustomLabel.OT_FieldName, customLabel.OT_FieldName);
			AssertEquals("CustomLabel.OT_Caption", expectedCustomLabel.OT_Caption, customLabel.OT_Caption);
			AssertEquals("CustomLabel.OT_Hint", expectedCustomLabel.OT_Hint, customLabel.OT_Hint);
			AssertEquals("CustomLabel.OT_Rule", expectedCustomLabel.OT_Rule, customLabel.OT_Rule);
			AssertEquals("CustomLabel.OT_Position", expectedCustomLabel.OT_Position, customLabel.OT_Position);
			AssertEquals("CustomLabel.OT_ColumnSize", expectedCustomLabel.OT_ColumnSize, customLabel.OT_ColumnSize);
			AssertEquals("CustomLabel.OT_IsMandatory", expectedCustomLabel.OT_IsMandatory, customLabel.OT_IsMandatory);
		}

		#endregion

		#region TestImport_WhsClientParametersByWarehouse

		public void TestImport_WhsClientParametersByWarehouse()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs1 = helper.CreateWarehouse("WHS1", "A");
			var whs2 = helper.CreateWarehouse("WHS2", "B");
			var whs3 = helper.CreateWarehouse("WHS2", "B");

			var whs1Area1 = helper.CreateWhsArea(whs1.PK, "Area 11");
			var whs2Area1 = helper.CreateWhsArea(whs2.PK, "Area 21");
			var whs3Area1 = helper.CreateWhsArea(whs3.PK, "Area 31");
			var whs3Area2 = helper.CreateWhsArea(whs3.PK, "Area 32");

			var orgPK = helper.CreateClient("CLIENT");
			Factory.Load<OrgHeader>(orgPK).MainAddress.FillWithValidTestData();

			var clientParameter1 = helper.CreateWhsClientParameterByWarehouse(orgPK, whs1.PK); // Imported with no Warehouse and no Area in otherFactory.
			var clientParameter2 = helper.CreateWhsClientParameterByWarehouse(orgPK, whs2.PK); // Imported with Warehouse, but no Area in otherFactory.
			var clientParameter3 = helper.CreateWhsClientParameterByWarehouse(orgPK, whs3.PK); // Imported with both Warehouse and Area in otherFactory.
			var clientParameter4 = helper.CreateWhsClientParameterByWarehouse(orgPK, whs3.PK);
			var xsdOrg = DataAdapter.ExportToValueObject(Factory.Load<OrgHeaderForDataTransfer>(orgPK), new ValueObjectExportContext(new NotificationBuffer()));

			var query = new ZQuery(WhsClientParameterByWarehouseSchema.WY_OH_Client, orgPK);

			// Import the created Xsd into new Factory.

			var otherFactory = new BusinessObjectFactory();
			// Not importing whs1
			otherFactory.ImportFromAnotherFactory(whs2);
			// Not importing area for whs2
			otherFactory.ImportFromAnotherFactory(whs3);
			otherFactory.ImportFromAnotherFactory(whs3Area1);
			otherFactory.ImportFromAnotherFactory(whs3Area2);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var importedOrgInOtherFactory = DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, contextInOtherFactory);
			var whsClientParamsInOtherFactory = otherFactory.Load<IWhsClientParameterByWarehouse>(query);
			AssertEquals("otherFactory have necessary data for 3 WhsClientParametersByWarehouse so they should be imported.", 3, whsClientParamsInOtherFactory.Length);
			AssertContainsWhsClientParameterByWarehouse(whsClientParamsInOtherFactory, clientParameter3);
			AssertContainsWhsClientParameterByWarehouse(whsClientParamsInOtherFactory, clientParameter4);

			AssertEquals("1 WhsClientParametersByWarehouse should not be imported.", 1, notificationBuffer.Events.Length);
			notificationBuffer.Events.Single(n => n.Message == string.Format("Organization with code '{0}': warehouse parameter skipped for warehouse [{1}].", importedOrgInOtherFactory.OH_Code, whs1.PK));
		}

		void AssertContainsWhsClientParameterByWarehouse(IWhsClientParameterByWarehouse[] allImportedWhsClientParametersByWarehouse, IWhsClientParameterByWarehouse expectedWhsClientParam)
		{
			foreach (var whsClientParam in allImportedWhsClientParametersByWarehouse)
			{
				if (whsClientParam.PK == expectedWhsClientParam.PK)
				{
					AssertEquals("Warehouse", expectedWhsClientParam.WY_WW_Whs, whsClientParam.WY_WW_Whs);
					return;
				}
			}
			Fail(string.Format("WhsClientParameterByWarehouse with PK = '{0}',  warehousePK = '{1}' was not found.",
				expectedWhsClientParam.PK,
				expectedWhsClientParam.WY_WW_Whs));
		}

		#endregion

		#region TestImport_MiscServSerialNumberIsKey

		public void TestImport_MiscServSerialNumberIsKey()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_IMSerialNumberIsKey = true;
			var orgForDataTransfer = Factory.Load<OrgHeaderForDataTransfer>(org.PK);
			var xsdOrg = DataAdapter.ExportToValueObject(orgForDataTransfer, new ValueObjectExportContext(new NotificationBuffer()));

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, contextInOtherFactory);
			var importedOrgInOtherFactory = otherFactory.Load<OrgHeader>(org.PK);
			AssertNotNull(importedOrgInOtherFactory);
			AssertEquals(true, importedOrgInOtherFactory.MiscServ.OM_IMSerialNumberIsKey);
		}

		#endregion

		#region TestImport_WhsDefaultExpiryNotificationPeriodInDaysIsNotEmpty

		public void TestImport_WhsDefaultExpiryNotificationPeriodInDaysIsNotEmpty()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 5;
			var orgForDataTransfer = Factory.Load<OrgHeaderForDataTransfer>(org.PK);
			var xsdOrg = DataAdapter.ExportToValueObject(orgForDataTransfer, new ValueObjectExportContext(new NotificationBuffer()));

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, contextInOtherFactory);
			var importedOrgInOtherFactory = otherFactory.Load<OrgHeader>(org.PK);
			AssertNotNull(importedOrgInOtherFactory);
			AssertEquals((short)5, importedOrgInOtherFactory.MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays);
		}

		#endregion

		#region AssertEquals

		void AssertEquals(string message, ZString expectedValue, ZString actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", !expectedValue.IsEmpty, actualValueSpecified);
		}

		void AssertEquals(string message, ZBool expectedValue, ZBool actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", expectedValue, actualValueSpecified);
		}

		void AssertEquals(string message, ZShort expectedValue, ZShort actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", true, actualValueSpecified);
		}

		#endregion

		OrgHeaderForDataTransfer GetNewTestOrg()
		{
			#region OrgHeader Initialised

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsActive = true;
			org.OH_FullName = "Simon LTD";
			org.OH_ScreeningStatus = "MAT";
			org.OH_IsConsignee = true;
			org.MainWebURL.PU_URL = "www.sauhdilasjdsajld.com.au";
			org.OH_Language = Core.SharedConstants.Languages.English;
			org.OH_SystemLastEditTimeUtc = ZDateTime.Now.ToDateTime();
			org.OH_SystemLastEditUser = "ANS";
			org.OH_SystemCreateTimeUtc = ZDateTime.Now.ToDateTime();
			org.OH_SystemCreateUser = "sds";

			#endregion

			#region OrgContact Initialised

			var contact = Factory.New<OrgContact>();
			contact.OC_IsActive = true;
			contact.OC_ContactName = "Simon";
			contact.OC_Salutation = "XXX";
			contact.OC_Language = Core.SharedConstants.Languages.English;
			contact.OC_NotifyMode = "EML";
			contact.OC_Title = "MR";
			contact.OC_JobCategory = "Engineer";
			contact.OC_Email = "a@b.com";
			contact.OC_Fax = "123343";
			contact.OC_HomePhone = "34242";
			contact.OC_Mobile = "3423424";
			contact.OC_OtherPhone = "3242342";
			contact.OC_Pager = "3242354234";
			contact.OC_AttachmentType = "PDF";

			// Need Manual test 
			// org.SecurityProvider.HasModifyDetailsWebSecurity = true;
			// contact.OC_WebAccessEnabled = true;

			contact.OC_WebContractSignedDate = ZDate.Today;
			contact.SetHashedPassword("23432423");
			contact.OC_Birthday = ZDate.Today;
			contact.OC_YearJoinedIndustry = ZDate.Today;
			contact.OC_YearJoinedCompany = ZDate.Today;
			contact.OC_ContactSource = "InPerson";
			contact.OC_DetailsVerified = ZDate.Today;
			contact.OC_PersonalInfo = "Fishing";

			//Set orgDocument
			contact.Documents.AddNew();
			contact.Documents[0].OD_AttachmentType = "PDF";
			contact.Documents[0].OD_DefaultContact = true;
			contact.Documents[0].OD_DeliverBy = "Tue";

			contact.Documents.AddNew();
			contact.Documents[1].OD_AttachmentType = "TIF";
			contact.Documents[1].OD_DeliverBy = "Wed";

			org.Contacts.Add(contact);

			#endregion

			#region OrgMiscServ Initialised

			var airline = Factory.NewWithValidTestData<RefAirline>();
			org.MiscServ.OM_RM_Airline = airline.PK;
			org.MiscServ.OM_IMEftCustomsFromImport = true;
			org.MiscServ.OM_IMEftQuarantineFromImport = true;
			org.MiscServ.OM_IMEftHoldUntilPayAuthorised = true;
			org.MiscServ.OM_IMEstDaysDeliveryAir = 23;
			org.MiscServ.OM_IMEstDaysDeliveryFCL = 24;
			org.MiscServ.OM_IMEstDaysDeliveryLCL = 25;
			org.MiscServ.OM_IMMaxEFTAmount = 200;
			org.MiscServ.OM_IMMinEFTAmount = 100;
			org.MiscServ.OM_IMEFTBankAccount = "3242424324";
			org.MiscServ.OM_IMEFTBankBSB = "324234";
			org.MiscServ.OM_IMIsGSTDeferred = true;
			org.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "ABC";
			org.MiscServ.OM_IMOrderLineAttrib1 = "attribute1";
			org.MiscServ.OM_IMOrderLineAttrib2 = "attribute2";
			org.MiscServ.OM_IMOrderLineAttrib3 = "attribute3";
			org.MiscServ.OM_IMOriginalSeaBills = 24;
			org.MiscServ.OM_IMCopySeaBills = 13;
			org.MiscServ.OM_IMSendImportDocsTo = "AUS";
			org.MiscServ.OM_IMSendSeaImportDocsTo = "USA";
			org.MiscServ.OM_IMImporterCategory = "DOC";
			org.MiscServ.OM_IMAirDepotFreeDays = 24;
			org.MiscServ.OM_IMSeaDepotFreeDays = 45;
			org.MiscServ.OM_IMImporterOwnsPartNumbers = true;
			org.MiscServ.OM_IMLastOrderReference = "Ord234";
			org.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum = true;
			org.MiscServ.OM_IMFCLEquipmentNeeded = "FCL";
			org.MiscServ.OM_IMLCLEquipmentNeeded = "LCL";
			org.MiscServ.OM_IMAirEquipmentNeeded = "Air";
			org.MiscServ.OM_RS_NKIMDefaultServiceLevel = "ABC";
			org.MiscServ.OM_IMDefaultINCOTerm = "ABC";
			org.MiscServ.OM_IMAutoImpJobRefered = true;
			org.MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = "XYZ";
			org.MiscServ.OM_IMPartAttrib1Type = "at1";
			org.MiscServ.OM_IMPartAttrib1Name = "at1";
			org.MiscServ.OM_IMAttrib1IsKey = true;
			org.MiscServ.OM_IMPartAttrib2Type = "at2";
			org.MiscServ.OM_IMPartAttrib2Name = "at2";
			org.MiscServ.OM_IMAttrib2IsKey = true;
			org.MiscServ.OM_IMPartAttrib3Type = "at3";
			org.MiscServ.OM_IMPartAttrib3Name = "at3";
			org.MiscServ.OM_IMAttrib3IsKey = true;
			org.MiscServ.OM_IMSerialNumberIsKey = true;
			org.MiscServ.OM_IMUseExpiryDate = true;
			org.MiscServ.OM_IMUsePackingDate = true;
			org.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			org.MiscServ.OM_IMJobRequireOrderTrackLink = true;
			org.MiscServ.OM_IMDocumentAddressPreference = "jkl";
			org.MiscServ.OM_IMDefaultWarehousePickOption = "asd";
			org.MiscServ.OM_IMInvoiceDetailReportSort = "asd";
			org.MiscServ.OM_IMInvoiceDetailReportSort2 = "sad";
			org.MiscServ.OM_IMInvoiceDetailReportSort3 = "sad";
			org.MiscServ.OM_LandedCostMarginPercent1 = 23.4;
			org.MiscServ.OM_LandedCostMarginPercent2 = 23.4;
			org.MiscServ.OM_LandedCostMarginPercent3 = 12.6;
			org.MiscServ.OM_LastArchiveDate = ZDateTime.Now.ToDateTime();
			org.MiscServ.OM_EXJobRequireOrderTrackLink = true;
			org.MiscServ.OM_EXExporterRequiresOrderNumbersOnDocs = true;
			org.MiscServ.OM_RS_NKEXDefaultServiceLevel = "dad";
			org.MiscServ.OM_EXGoodsDescription = "SDfsdfsdfS";
			org.MiscServ.OM_EXExporterCategory = "asd";
			org.MiscServ.OM_EXFCLEquipmentNeeded = "asd";
			org.MiscServ.OM_EXLCLEquipmentNeeded = "asd";
			org.MiscServ.OM_EXAirEquipmentNeeded = "Dad";
			org.MiscServ.OM_EXAllowedToPrintOriginalBL = true;
			org.MiscServ.OM_EXDocumentAddressPreference = "DSf";
			org.MiscServ.OM_EXDefaultDGContactPhoneUsed = "asd";
			org.MiscServ.OM_EXDefaultInvoicePriceFromProductLastCost = "Ada";
			org.MiscServ.OM_EXHandlingInstuctions = "dcasdadas";
			org.MiscServ.OM_EXDefaultIncoTerm = "fad";
			org.MiscServ.OM_EXMergeCustomsInvoiceLinesBy = "SAS";
			org.MiscServ.OM_EXPreAllocPrefix = "asd";
			org.MiscServ.OM_FWAgentCategory = "SAd";
			org.MiscServ.OM_FWAgentBelongsToGroup = true;
			org.MiscServ.OM_FWHandlesAir = true;
			org.MiscServ.OM_FWHandlesSea = true;
			org.MiscServ.OM_FWHandlesSeaForPortOrCountry = "AUSYD";
			org.MiscServ.OM_FWHandlesAirForPortOrCountry = "USANY";
			org.MiscServ.OM_FWRequestForCreditAllowed = true;
			org.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = true;
			org.MiscServ.OM_FWDealDirectlyWithUltimates = true;
			org.MiscServ.OM_FWIATACode = "AUSYD";
			org.MiscServ.OM_FWIATAAccountNumber = "3242346";
			org.MiscServ.OM_FWDirectAMSReporter = true;
			org.MiscServ.OM_CRCarrierCategory = "dfd";
			org.MiscServ.OM_SVServicesCategory = "adf";
			org.MiscServ.OM_CMSalesCategory = "sfd";
			org.MiscServ.OM_CMCompetitorActivity = "dfa";
			org.MiscServ.OM_CMLastCallDate = ZDateTime.Now.ToDateTime();
			org.MiscServ.OM_CMClientSize = "MED";
			org.MiscServ.OM_CMNoOfEmployees = 23;
			org.MiscServ.OM_CMEstimatedDateToClose = ZDateTime.Now.ToDateTime();
			org.MiscServ.OM_CMGrowthOutlook = "FDs";
			org.MiscServ.OM_CMDoesExports = true;
			org.MiscServ.OM_CMDoesImports = true;
			org.MiscServ.OM_RH_NKCMMainImportCmdty = "fds";
			org.MiscServ.OM_RH_NKCMMainExportCmdty = "SDa";
			org.MiscServ.OM_CMUseTradeLaneFigures = true;
			org.MiscServ.OM_CMTotalClientRevenue = 21321.23;
			org.MiscServ.OM_CMPercentage = 4543.23;
			org.MiscServ.OM_CMAcheivableClientRevenue = 4234.12;
			org.MiscServ.OM_CMEstimatedProfit = 42.34;
			org.MiscServ.OM_CMWarehouseRevenue = 23.21;
			org.MiscServ.OM_CMConsultingRevenue = 34.55;
			org.MiscServ.OM_CMOverallClientRelation = 23;
			org.MiscServ.OM_CMClientsDesireToRemain = 123;
			org.MiscServ.OM_CMEaseClientCanBePoached = 23;
			org.MiscServ.OM_CMAmountOfElectronicIntegration = 234;
			org.MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts = "fss";
			org.MiscServ.OM_CMOverallEffectOfClientOnLCLCosts = "dsf";
			org.MiscServ.OM_CMOverallEffectOfClientOnTEUCosts = "sdf";
			org.MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts = "fds";
			org.MiscServ.OM_CMOverallEffectOfClientOnOtherCosts = "DSf";
			org.MiscServ.OM_CMAmountOfBusinessWon = ZByte.Zero;
			org.MiscServ.OM_CMSalesTerritory = "dsf";
			org.MiscServ.OM_CMClientCommenced = ZDateTime.Now.ToDateTime();
			org.MiscServ.OM_CMClientPortalHomePage = "fsdfsdf";
			org.MiscServ.OM_CICompetitorCategory = "fgd";
			org.MiscServ.OM_CIEstimatedStaffThisLocation = 24;
			org.MiscServ.OM_CITypeOfService = "Dfs";
			org.MiscServ.OM_CIEstimatedStaffThisCountry = 234;
			org.MiscServ.OM_CISellingStyle = "fss";
			org.MiscServ.OM_CITurnover = 324.5;
			org.MiscServ.OM_CIProfit = 2323.45;
			org.MiscServ.OM_CICapitalEmployed = 423.56;
			org.MiscServ.OM_CICompetitiveRanking = 2;
			org.MiscServ.OM_CIStrength = "dsf";
			org.MiscServ.OM_CIWeaknesses = "ewr";
			org.MiscServ.OM_CIOpportunities = "ads";
			org.MiscServ.OM_CIThreats = "dsf";
			org.MiscServ.OM_WhsExpiryNotificationFromDefaults = true;
			org.MiscServ.OM_WhsExpiryNotificationPeriod = 23;
			org.MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 12;
			org.MiscServ.OM_WhsClientInvoiceFormat = "rgg";
			org.MiscServ.OM_WhsOrderNumberUniquenessStrategy = "Dfd";
			org.MiscServ.OM_CustomAttrib1 = "sdf";
			org.MiscServ.OM_CustomAttrib2 = "fdg";
			org.MiscServ.OM_CustomAttrib3 = "dsf";
			org.MiscServ.OM_CustomDate1 = ZDateTime.Now.ToDateTime();
			org.MiscServ.OM_CustomDate2 = ZDateTime.Now.ToDateTime();
			org.MiscServ.OM_CustomDate3 = ZDateTime.Now.ToDateTime();
			org.MiscServ.OM_CustomDecimal1 = 4535.34;
			org.MiscServ.OM_CustomDecimal2 = 3423.23;
			org.MiscServ.OM_CustomDecimal3 = 4545.23;
			org.MiscServ.OM_CustomFlag1 = true;
			org.MiscServ.OM_CustomFlag2 = true;
			org.MiscServ.OM_CustomFlag3 = true;
			org.MiscServ.OM_CustomFlag4 = true;
			org.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			org.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls = true;
			org.MiscServ.OM_WhsPackingSlipOrderBy = "LNO";
			org.MiscServ.OM_WhsOrderFulfillmentRule = "DFS";
			org.MiscServ.OM_WhsDefaultWarehousePickMode = "AUG";

			#endregion

			#region OrgAddress Initialised

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_IsActive = true;
			address.OA_Code = "sdadas";
			address.OA_Language = Core.SharedConstants.Languages.English;
			address.OA_CompanyNameOverride = "Test CompanyNameOverride";
			address.OA_Address1 = "23 Address 1";
			address.OA_Address2 = "24 ADdress 2";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "2000";
			address.OA_Phone = "4352342423423";
			address.OA_Fax = "324234233423";
			address.OA_Mobile = "0404555666";
			address.OA_Email = "asda@ewfw.com";
			address.OA_PickupFromTimeOnly = ZDate.Today;
			address.OA_PickupToTimeOnly = ZDate.Today;
			address.OA_DeliverFromTimeOnly = ZDate.Today;
			address.OA_DeliverToTimeOnly = ZDate.Today;
			address.OA_DoNotAttendFrom = ZDate.Today;
			address.OA_DoNotAttendTo = ZDate.Today;
			address.OA_DockLeveler = true;
			address.OA_ForkLift = true;
			address.OA_PalletJack = true;
			address.OA_ContainerHandling = "MAN";
			address.OA_AccessPoint = "AP1";
			address.OA_LabourRequired = "234";
			address.OA_CommunicationRequired = "FAX";
			address.OA_Dock_Height = "5";
			address.OA_OtherWarehouseFacilities = "other facility";
			address.OA_LoadingUnloadingConstraints = "LoadingUnloadingConstraints";
			address.OA_FCLEquipmentNeeded = "WUP";
			address.OA_LCLEquipmentNeeded = "PSL";
			address.OA_AIREquipmentNeeded = "PSL";
			address.OA_GeoLocation = ZGeography.CreatePoint(24.3, -55.1);

			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address.AddressCapability.SetIsNotMainAddress(OrgAddressType.Delivery.Code);

			org.Addresses.Add(address);

			#endregion

			#region OrgCompanyData Initialised

			var companyData = org.CompanyData;

			companyData.OB_IsDebtor = true;
			companyData.OB_IsCreditor = true;
			companyData.OB_APCategory = "ABC";
			companyData.OB_APCreditLimit = 123;
			companyData.OB_APPayInvoiceAfterPostingDefault = true;
			companyData.OB_APPaymentTermDays = 3;
			companyData.OB_APPaymentTerms = Constants.InvoiceTerms.CashOnDelivery;
			companyData.OB_APVATConfig = Constants.OrganisationTaxConfiguartionTypes.CashBasis;
			companyData.OB_APWHTApplicable = true;
			companyData.OB_RX_NKAPDefltCurrency = Constants.CurrencyCodes.Australia;
			companyData.OB_OG_APCreditorGroup = Factory.LoadTop1<OrgCreditorGroup>(new ZQuery()).PK;
			companyData.OB_AB_APDefaultBankAccount = ZGuid.NewZGuid();
			companyData.OB_AC_APDefaultChargeCode = ZGuid.NewZGuid();
			companyData.OB_APAirlineAccountNumber = "ABC";
			companyData.OB_ARAutoUpdateRates = true;
			companyData.OB_ARCategory = "ABC";
			companyData.OB_ARCombinedStatementInvoice = true;
			companyData.OB_ARConsolidatedAccountingCategory = "ABC";
			companyData.OB_ARCreditLimit = 123.4;
			companyData.OB_ARCreditRating = "ABC";
			companyData.OB_ARUseSettlementGroupCreditLimit = true;
			companyData.OB_ARCreditApproved = true;
			companyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			companyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 3;
			companyData.OB_ARTreatDisbursementsAsStandardValue = 123.3;
			companyData.OB_ARDontShowTaxOnDocs = true;
			companyData.OB_ARBuyersConsolInvoicingStyle = "ABC";
			companyData.OB_AROnCreditHold = true;
			companyData.OB_ARAccountAndCreditReviewDue = ZDateTime.Now;
			companyData.OB_AB_ARPayToAccount = ZGuid.NewZGuid();
			companyData.OB_ARPreviousChequeDrawer = "ABC";
			companyData.OB_ARPreviousChequeDrawerBank = "ABC";
			companyData.OB_ARPreviousChequeDrawerBankBranch = "ABC";
			companyData.OB_ARReceiptInvoiceAfterPostingDefault = true;
			companyData.OB_ARVATConfig = Constants.OrganisationTaxConfiguartionTypes.AccrualBasis;
			companyData.OB_AREftCustomsPaymentMethod = "ABC";
			companyData.OB_ARWHTApplicable = true;
			companyData.OB_RX_NKARDDefltCurrency = Constants.CurrencyCodes.Australia;
			companyData.OB_OJ_ARDebtorGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery()).PK;
			companyData.OB_ARIncludeInwardsWhsConsolidatedInvoice = true;
			companyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice = true;
			companyData.OB_ARWhsStorageCalcMethod = "ABC";
			companyData.OB_CRIsShipsAgencyPrincipal = true;
			companyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 123.2, 123.2); //should be =< 100
			companyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 123.2, 123.2); //should be =< 100
			companyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 123.2, 123.2); //should be =< 100
			companyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 123.2, 123.2); //should be =< 100
			companyData.OB_IMUsedBondedWhs = true;
			companyData.OB_GB_ControllingBranch = ZGuid.NewZGuid();
			companyData.OB_APQualityAssured = true;
			companyData.OB_APQualityAssuredCheckedDate = ZDateTime.Now;
			companyData.OB_ARQualityAssured = true;
			companyData.OB_ARQualityAssuredCheckedDate = ZDateTime.Now;
			companyData.OB_ARWarehouseRatingPeriod = "ABC";
			companyData.OB_WhsClientFreeStorageDays = 3;
			companyData.OB_WhsOverrideFreeStorage = true;

			companyData.OB_OJ_ARDebtorGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery()).PK;
			companyData.OB_OG_APCreditorGroup = Factory.LoadTop1<OrgCreditorGroup>(new ZQuery()).PK;

			#region OrgInvoiceType

			OrgInvoiceType orgInvoiceType = companyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = "ABC";
			orgInvoiceType.PI_Type = "ABC";
			orgInvoiceType.PI_Interval = "ABC";
			orgInvoiceType.PI_StartDay = "ABC";

			//add additional InvoiceTypes for test
			companyData.InvoiceTypes.AddNew();

			#endregion

			#region OrgInvoiceRollupOrGroup

			org.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup orgInvoiceRollupOrGroup = companyData.InvoiceRollupOrGroups.AddNew();
			orgInvoiceRollupOrGroup.PG_JobType = "ABC";
			orgInvoiceRollupOrGroup.PG_TransportMode = "ABC";
			orgInvoiceRollupOrGroup.PG_ServiceDirection = "ABC";
			orgInvoiceRollupOrGroup.PG_GroupOrSubTotal = "ABC";
			orgInvoiceRollupOrGroup.PG_GroupOrSubtotalStyle = "ABC";
			orgInvoiceRollupOrGroup.PG_InvoiceLineDisplayOption = "ABC";
			orgInvoiceRollupOrGroup.PG_InvoicePostingStyle = "ABC";

			//add additional InvoiceRollupOrGroups for test
			companyData.InvoiceRollupOrGroups.AddNew();

			#endregion

			//add additional companydata for test
			OrgCompanyData newCompanyData = Factory.New<OrgCompanyData>();
			ZQuery nonCurrentCompanyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, companyData.OB_GC);
			GlbCompany nonCurrentCompany = Factory.LoadTop1<GlbCompany>(nonCurrentCompanyQuery);
			newCompanyData.OB_GC = nonCurrentCompany.PK;
			newCompanyData.OB_OH = org.PK;

			#endregion

			#region OrgCountryData Initialised

			OrgCountryData countryData = org.CountryData;

			countryData.OV_RN_NKClientCountryRelation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryData.OV_OA_ApprovedLocation = ZGuid.NewZGuid();

			countryData.OV_ImportEntryPaymentPreference = "ABC";
			countryData.OV_ImportQuarantinePaymentPreference = "DEF";
			countryData.OV_EXApprovedOrMajorExporter = "GHI";
			countryData.OV_EXApprovalMethod = "JKL";
			countryData.OV_EXApprovalNumber = "123456789012345";
			countryData.OV_EXPermitNumber = "12345678901234567890123456789012345678901234567890";
			countryData.OV_EXExportPermissionDetails = "VC250";
			countryData.OV_CustomsEconomicGroupAddInfo = "VC1024";
			countryData.OV_ImportCustomsDefaultAddInfo = "VC1024";
			countryData.OV_GS_NKReviewedByUser = GlbStaff.CurrentUser.GS_Code;
			countryData.OV_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			countryData.OV_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
			countryData.OV_EXE3Signed = ZBool.True;
			countryData.OV_MakePartsBothImportAndExport = ZBool.True;

			ZDateTime now = ZDateTime.Now;
			countryData.OV_EXSiteInspectionDate = now;
			countryData.OV_LastReviewedOn = now;
			countryData.OV_SystemCreateTimeUtc = now;
			countryData.OV_SystemLastEditTimeUtc = now;

			//add additional countrydata for test
			org.CountryDataCollectionForThisCompany.AddNew();

			#endregion

			#region OrgEDICommunication Initialised

			org.EDICommunicationsModes.RemoveAndDeleteAll();
			var ediCommunicationMode = org.EDICommunicationsModes.AddNew();
			ediCommunicationMode.EK_CommsDirection = "TRX";
			ediCommunicationMode.EK_CommunicationsTransport = "EMA";
			ediCommunicationMode.EK_Destination = "Destination1";
			ediCommunicationMode.EK_DestinationFolder = "DestinationFolder1";
			ediCommunicationMode.EK_FileFormat = "XML";
			ediCommunicationMode.EK_Filename = "FileName1";
			ediCommunicationMode.EK_FtpLockingMethod = "FLM";
			ediCommunicationMode.EK_LocalPartyVanID = "AAA";
			ediCommunicationMode.EK_LoginName = "BBB";
			ediCommunicationMode.EK_MessagePurpose = "EVT";
			ediCommunicationMode.EK_Module = "ACA";
			ediCommunicationMode.EK_ParentID = org.PK;
			ediCommunicationMode.EK_ParentTableCode = "OH";
			ediCommunicationMode.EK_Password = "Test";
			ediCommunicationMode.EK_PortNumber = 123;
			ediCommunicationMode.EK_PublishInternalMilestones = ZBool.True;
			ediCommunicationMode.EK_RelatedPartyVanID = "CCC";
			ediCommunicationMode.EK_ServerAddressSubject = "SAS1";
			ediCommunicationMode.EK_SourceFolder = "SourceFolder1";
			ediCommunicationMode.EK_LastFailed = now;
			#endregion

			#region OrgRelatedParty

			OrgHeaderForDataTransfer someRelatedOrg = Factory.LoadTop1<OrgHeaderForDataTransfer>(new ZQuery());

			OrgRelatedParty orgRelatedParty = org.AllRelatedParties.AddNew();
			orgRelatedParty.PR_FreightDirection = "AAA";
			orgRelatedParty.PR_FreightTransportMode = "AAA";
			orgRelatedParty.PR_FreightContainerMode = "AAA";
			orgRelatedParty.PR_GC = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, org.CompanyData.Company.GC_Code).PK;
			orgRelatedParty.PR_OH_RelatedParty = someRelatedOrg.PK;
			orgRelatedParty.PR_PartyType = "AAA";
			orgRelatedParty.PR_Service = "AAA";
			orgRelatedParty.PR_Location = "AAAAA";

			OrgRelatedParty orgRelatedParty2 = org.AllRelatedParties.AddNew();
			orgRelatedParty2.PR_FreightDirection = "BBB";
			orgRelatedParty2.PR_FreightTransportMode = "BBB";
			orgRelatedParty2.PR_FreightContainerMode = "BBB";
			orgRelatedParty2.PR_GC = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, org.CompanyData.Company.GC_Code).PK;
			orgRelatedParty2.PR_OH_RelatedParty = someRelatedOrg.PK;
			orgRelatedParty2.PR_PartyType = "BBB";
			orgRelatedParty2.PR_Service = "BBB";
			orgRelatedParty2.PR_Location = "BBBBB";

			#endregion

			#region OrgCusCode

			org.CustomsCodes.AddNew("GST", "23848982094223");
			org.CustomsCodes[0].OK_CountryDefault = true;

			#endregion

			#region Brand Names

			org.BrandsOrRelatedNames.AddNew("IBM");
			org.BrandsOrRelatedNames.AddNew("Microsoft");

			#endregion

			return Factory.Load<OrgHeaderForDataTransfer>(org.PK);
		}

		protected string WriteValueObjectToXml(IValueObject valueObj)
		{
			StringWriter writer = new UTF8StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.Indented;
			XmlValueObjectSerializer serializer = new SysMergeOrganisationXmlValueObjectSerializerForTesting(DataAdapter);

			serializer.WriteToXml(xmlWriter, DataAdapter, valueObj, new NotificationBuffer());
			xmlWriter.Flush();
			writer.Flush();
			string result = writer.GetStringBuilder().ToString();

			return result;
		}

		public class UTF8StringWriter : StringWriter
		{
			public override System.Text.Encoding Encoding
			{
				get { return System.Text.Encoding.UTF8; }
			}
		}

		protected virtual StringWriter GetNewStringWriter()
		{
			return new UTF8StringWriter();
		}

		public void TestDataImportEventIsAdded()
		{
			Xsd.SysMergeOrganisation xsdOrg = new Xsd.SysMergeOrganisation();
			DataTransfer.Xml.XsdVersion1.XmlInterchange interchange = new DataTransfer.Xml.XsdVersion1.XmlInterchange();
			xsdOrg.OrganisationDetail.OrgHeader.FullName = "testorg";
			xsdOrg.OrganisationDetail.OrgHeader.PK = Guid.NewGuid().ToString();
			xsdOrg.OrganisationDetail.OrgHeader.RL_NKClosestPort = "AUSYD";

			Xsd.SysMergeOrgAddress address = xsdOrg.OrganisationDetail.OrgAddresses.AddNew();
			address.AddressLine1 = "1 Address";
			address.PK = Guid.NewGuid().ToString();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			BusinessObject bizO = DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, context);
			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
			AssertEquals("BizO should have a DataImport event", 1, bizO.GetLogs().GetAllLogs().Find(query).Length);
		}

		readonly SysMergeOrganisationValueObjectDataAdapterTestClass DataAdapter = new SysMergeOrganisationValueObjectDataAdapterTestClass();

		class SysMergeOrganisationValueObjectDataAdapterTestClass : SysMergeOrganisationValueObjectDataAdapter
		{
			public SysMergeOrganisationValueObjectDataAdapterTestClass()
				: base()
			{
			}
		}
	}
}
