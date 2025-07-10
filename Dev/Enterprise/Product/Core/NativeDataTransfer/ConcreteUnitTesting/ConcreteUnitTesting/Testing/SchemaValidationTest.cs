using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class SchemaValidationTest : TestCaseWithFactory
	{
		public void TestStaffAndCompanySchemasArePartOfSet()
		{
			var definition = TestUtil.GetEntitySetDefinition("Staff");
			AssertNotNull("Should be able to get definition for Staff Dataset.", definition);
			definition = TestUtil.GetEntitySetDefinition("Company");
			AssertNotNull("Should be able to get definition for Company Dataset.", definition);
		}

		public void TestPopulatedOrgHeaderExportedAsNativeXmlValidatesAgainstTheGeneratedSchema()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "NZAKL";

			var companyData = orgHeader.CompanyData;
			companyData.OB_IsCreditor = true;

			var apAccountDetails1 = companyData.AccountDetailsCollection.AddNew();
			apAccountDetails1.A1_AccountName = "JOHN SAMPLE";

			var apAccountDetails2 = companyData.AccountDetailsCollection.AddNew();
			apAccountDetails2.A1_AccountName = "TIM SIMPLE";

			var otherOrg = Factory.New<OrgHeader>();
			otherOrg.OH_FullName = "OTHER ORG";
			otherOrg.OH_RL_NKClosestPort = "NZAKL";

			var orgBuyerLink = orgHeader.BuyerLinks.AddNew();
			orgBuyerLink.OL_OH_Buyer = otherOrg.PK;

			var orgSupplierLink = orgHeader.SupplierLinks.AddNew();
			orgSupplierLink.OL_OH_Supplier = otherOrg.PK;

			CheckSchemaValidity(orgHeader, "Organization");
		}

		public static void CheckSchemaValidity(BusinessObject sourceBizO, string entityName, Func<XElement, XDocument> getBizODocumentDelegate = null)
		{
			ReferenceDataXMLForSerialize.ResetInstanceForTesting();
			try
			{
				var nativeRootSchema = GenerateNativeCommonSchemaForTesting();
				var commonSchema = GenerateUniversalCommonSchemaForTesting();

				var xsdGenerator = new NativeXsdGenerator();
				var definition = TestUtil.GetEntitySetDefinition(entityName);
				var bizoSchema = xsdGenerator.Generate(definition);

				sourceBizO.Factory.Save();

				var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
				var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
				var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

				using (var dataStream = xmlSerializer.SerializeToStream(sourceBizO))
				{
					var nativeDataElement = XElement.Load(dataStream);
					var ns = nativeDataElement.GetDefaultNamespace();
					var bodyElement = nativeDataElement.Element(ns + "Body");
					var bizoElement = bodyElement.Element(ns + entityName);

					using (var bizoReader = bizoElement.CreateReader())
					{
						var bizoDocument = getBizODocumentDelegate != null ? getBizODocumentDelegate(bizoElement) : XDocument.Load(bizoReader);

						using (var bizoSchemaReader = bizoSchema.CreateReader())
						using (var nativeRootSchemaReader = nativeRootSchema.CreateReader())
						using (var commonSchemaReader = commonSchema.CreateReader())
						{
							var schemaSet = new XmlSchemaSet();
							schemaSet.Add(NativeXmlInfo.Namespace_2011_11, bizoSchemaReader);
							schemaSet.Add(NativeXmlInfo.Namespace_2011_11, nativeRootSchemaReader);
							schemaSet.Add(UniversalXmlInfo.Namespace_2011_11, commonSchemaReader);

							var errors = new List<string>();
							bizoDocument.Validate(schemaSet, (sender, eventArgs) => { errors.Add(eventArgs.Severity.ToString() + ":- " + eventArgs.Message); });
							AssertMultilineASCIIEquals("There should be no errors... Errors shown below.", "", string.Join("\r\n", errors.ToArray()));
						}
					}
				}
			}
			finally
			{
				ReferenceDataXMLForSerialize.ResetInstanceForTesting();
			}
		}

		public static XDocument GenerateUniversalCommonSchemaForTesting()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var importService = new NativeXmlImportService();
				importService.WriteUniversalCommonSchemaForTesting(tempDirectory);

				var commonSchemaPath = Path.Combine(tempDirectory, UniversalXmlInfo.CommonSchemaName);
				return XDocument.Load(commonSchemaPath);
			}
		}

		public static XDocument GenerateNativeCommonSchemaForTesting()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var importService = new NativeXmlImportService();
				importService.GenerateAndSaveAllXSDs(tempDirectory);

				var nativeRootSchemaPath = Path.Combine(tempDirectory, "Native.xsd");
				return XDocument.Load(nativeRootSchemaPath);
			}
		}
	}
}
