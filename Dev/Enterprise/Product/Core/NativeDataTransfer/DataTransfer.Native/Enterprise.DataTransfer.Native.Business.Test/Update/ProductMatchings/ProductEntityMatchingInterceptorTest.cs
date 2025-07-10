using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching.Test
{
	public class ProductEntityMatchingInterceptorTest : TestCaseWithFactory
	{
		// See eDocs of WI for explanation of scenarios

		public void TestScenario01PartNumOwnerMustBeUnique()
		{
			CreateExistingProduct(new string[] { craImpChi }, Array.Empty<string>());
			RunXml("1. ProductA OWN=CRAIMPCHI insert.xml");
			var logs = GetLogs();
			AssertContains("Cannot insert, product 'PRODUCTA' with related organisations CRAIMPCHI/OWN already exists", logs);

			ResetLogs();
			RunXml("1. ProductA OWN=CRAIMPCHI merge.xml");
			logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 1 updates", logs);

			ResetLogs();
			RunXml("1+30. ProductA OWN=CRAIMPCHI, OWN=BACCRI insert.xml");
			logs = GetLogs();
			AssertContains("Duplicate Product detected: Owner = CRAIMPCHI", logs);

			ResetLogs();
			RunXml("1. ProductA OWN=CRAIMPCHI, OWN=BACCRI merge.xml");
			logs = GetLogs();
			AssertContains("Duplicate Product detected: Owner = CRAIMPCHI", logs);
		}

		public void TestScenario02UpdateMustMatchExactly()
		{
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				CreateExistingProduct(new[] { craImpChi, baccri }, Array.Empty<string>());
				CreateExistingProduct(new[] { craImpChi }, new[] { abcExporters });
				CreateExistingProduct(new[] { craImpChi }, new[] { _4BELEV });
				CreateExistingProduct(new[] { craImpChi }, new[] { daecor });
				CreateExistingProduct(new[] { craImpChi }, new[] { baccri });
			}
			RunXml("2. ProductA Sup=BACCRI update.xml");
			var logs = GetLogs();
			Assert(logs, logs.Contains("Unable to find OrgSupplierPart to update using the details provided."));
			ResetLogs();
			RunXml("2. ProductA Sup=BACCRI merge.xml");
			logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts", logs);
			AssertProductNowExists(Array.Empty<string>(), new[] { baccri });
		}

		public void TestScenario03MergeNotMatchingExistingMustInsert_SUP()
		{
			CreateExistingProduct(new string[] { craImpChi }, new string[] { abcExporters });

			RunXml("3+32. ProductA Sup=ABCEXPBNE merge.xml");
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts", logs);
			AssertProductNowExists(Array.Empty<string>(), new string[] { abcExporters });
		}

		public void TestImportProductWithOutPGALines()
		{
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ACETESPHL";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "130GDKFSNSPC";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.UnitedStates;
			pivot[CusClassPartPivotSchema.CI_TariffNum] = "3978104000";
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";

			BusinessObjectCollection fdaLines = (BusinessObjectCollection)pivot["ACEFDAs"];
			var line1 = fdaLines.AddNew();
			line1["US_ProcessingCode"] = "DRU";
			line1["US_Description"] = "AABBBCC";

			var line2 = fdaLines.AddNew();
			line2["US_ProcessingCode"] = "DRU";
			line2["US_Description"] = "222222";

			Factory.Save();

			var xml = string.Format(CultureInfo.InvariantCulture, @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Header>
		<OwnerCode>USALGLSFO</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
	<Product>
		<OrgSupplierPart Action=""MERGE"">
				<PartNum>130GDKFSNSPC</PartNum>
				<Desc>WITHOUT PGALINES</Desc>
					<CusClassPartPivotCollection>
					<CusClassPartPivot Action=""MERGE"">
					<TariffNum>3924104000</TariffNum>
					<ChildType>HTI</ChildType>
					<CusUSClassificationCollection>
						<CusUSClassification Action=""MERGE"">
							<PSTIndicator>D</PSTIndicator>
							<PSTDisclaimReason></PSTDisclaimReason>
						</CusUSClassification>
					</CusUSClassificationCollection>
					<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
					</CusClassPartPivot>
					</CusClassPartPivotCollection>
				<OrgPartRelationCollection>
					<OrgPartRelation Action=""MERGE"">
						<Relationship>OWN</Relationship>
						<OrgHeader>
							<Code>ACETESPHL</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
		</OrgSupplierPart>
	</Product>
  </Body>
</Native>
");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();

			var qry = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			qry.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, "3924104000");
			var pivotAfterImport = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(qry).ToArray();

			AssertEquals(1, pivotAfterImport.Length);
			foreach (BusinessObject onePivot in pivotAfterImport)
			{
				var fdaAfterImport = (BusinessObjectCollection)onePivot["ACEFDAs"];
				AssertEquals("FDA Line should remain after import.", 2, fdaAfterImport.Count);
			}
		}

		public void TestImportProductWithCusCALPCO()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "YOUCAN_CA";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ACETESPHL";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "NODUPLICATE";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = org2.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.Canada;
			pivot[CusClassPartPivotSchema.CI_TariffNum] = "2202.90.90 90";
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";

			BusinessObject details = (BusinessObject)pivot["Details"];
			details["CCA_CFIAIndicator"] = "Y";

			BusinessObject cfia = (BusinessObject)pivot["CFIAPGAHeader"];
			cfia[CusAddInfoSchema.B7_Type] = "CCF";
			cfia[CusAddInfoSchema.B7_ParentTableCode] = "CI";
			cfia[CusAddInfoSchema.B7_ParentID] = pivot.PK;

			BusinessObjectCollection lpcoViews = (BusinessObjectCollection)cfia["LPCOViews"];
			var lpcoView = lpcoViews.AddNew();
			BusinessObject lpco = (BusinessObject)lpcoView["LPCO"];
			lpco["CLP_Type"] = "10";
			lpco["CLP_RefNo"] = "REFFNO3";

			Factory.Save();

			var xml = string.Format(CultureInfo.InvariantCulture, @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Header>
		<OwnerCode>YOUCAN_CA</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Product Version=""2.0"">
			<OrgSupplierPart Action=""MERGE"">
				<PartNum>NODUPLICATE</PartNum>
				<StockKeepingUnit>UNT</StockKeepingUnit>
				<IsActive>true</IsActive>
				<Desc>BMATE 1060N 20KG1A2 6M</Desc>
				<CusClassPartPivotCollection>
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>2202.90.90 90</TariffNum>
						<ChildType>HTI</ChildType>
						<ChildListOrder>0</ChildListOrder>
						<AdditionalInformationChildCollection TableName=""CusAddInfo"">
							<AdditionalInformationChild Action=""MERGE"">
								<Type>CHC</Type>
							</AdditionalInformationChild>
							<AdditionalInformationChild Action=""MERGE"">
								<Type>CCF</Type>
								<AddInfoCollection>
								  <AddInfo>
									<Key>AIRSEndUse</Key>
									<Value>104</Value>
								  </AddInfo>
								  <AddInfo>
									<Key>AllProgramInd</Key>
									<Value>Y</Value>
								  </AddInfo>
								</AddInfoCollection>
								<NAddInfoData></NAddInfoData>
								<CusCALPCOCollection TableName=""CusCALPCO"">
								  <CusCALPCO Action=""MERGE"">
									<ClusterKey>0</ClusterKey>
									<CommodityTypeCode></CommodityTypeCode>
									<DIFRefNumberOrLocation></DIFRefNumberOrLocation>
									<IsMixedCountryOfOrigin>false</IsMixedCountryOfOrigin>
									<ApplicantType></ApplicantType>
									<HolderType></HolderType>
									<StartDate></StartDate>
									<EndDate></EndDate>
									<IssueDate></IssueDate>
									<AlternativeQuotaQuantity>0.0</AlternativeQuotaQuantity>
									<AlternativeQuotaUQ></AlternativeQuotaUQ>
									<RefNo>REFNO2</RefNo>
									<SecondaryRefNo></SecondaryRefNo>
									<Type>11</Type>
									<AuthorizationCountry TableName=""RefCountry"" />
									<IssuanceCountryCode TableName=""RefCountry"" />
									<OriginCountryCode TableName=""RefCountry"" />
								  </CusCALPCO>
								</CusCALPCOCollection>
							  </AdditionalInformationChild>
						</AdditionalInformationChildCollection>

						<Country TableName=""RefCountry"">
							<Code>CA</Code>
						</Country>
					</CusClassPartPivot>
				</CusClassPartPivotCollection>
				<OrgPartRelationCollection>
					<OrgPartRelation Action=""MERGE"">
						<Relationship>OWN</Relationship>
						<OrgHeader>
							<Code>ACETESPHL</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
			</OrgSupplierPart>
		</Product>
	</Body>
</Native>
");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var qry = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			qry.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, "2202.90.90 90");
			var loadedPivots = newFactory.Load<Enterprise.Integration.Customs.CA.ICusClassPartPivot>(qry).ToArray();
			AssertEquals(1, loadedPivots.Length);

			BusinessObject loadedPivot = (BusinessObject)loadedPivots[0];
			BusinessObject loadedCFIA = (BusinessObject)loadedPivot["CFIAPGAHeader"];
			AssertNotNull(loadedCFIA);
			BusinessObjectCollection views = (BusinessObjectCollection)loadedCFIA["LPCOViews"];
			AssertEquals(1, views.Count);
			BusinessObject loadedLPCO = (BusinessObject)views.ToArray()[0]["LPCO"];
			ZString type = (ZString)loadedLPCO["CLP_Type"];
			ZString refNo = (ZString)loadedLPCO["CLP_RefNo"];
			AssertEquals("11", type);
			AssertEquals("REFNO2", refNo);
		}

		public void TestImportProductWithPGALines()
		{
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ACETESPHL";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "NODUPLICATE";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.UnitedStates;
			pivot[CusClassPartPivotSchema.CI_TariffNum] = "3214.10.00 10";
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";

			BusinessObjectCollection pstLines = (BusinessObjectCollection)pivot["PSTLines"];
			var pesticide = pstLines.AddNew();
			pesticide["US_ProductType"] = "PS1";
			pesticide["US_BrandName"] = "BRD";

			BusinessObjectCollection pesticideLines = (BusinessObjectCollection)pesticide["PesticideLines"];
			var pestline1 = pesticideLines.AddNew();
			pestline1["US_NameOfActiveIngredient"] = "AABBBCC";
			pestline1["US_ActiveIngredientPercentage"] = 45.0M;
			pestline1["US_LPCOType"] = "CAS";
			pestline1["US_LPCONumber"] = "112233";

			var pesticide2 = pstLines.AddNew();
			pesticide2["US_ProductType"] = "PS2";
			pesticide2["US_BrandName"] = "BR2";

			Factory.Save();

			var xml = string.Format(CultureInfo.InvariantCulture, @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Header>
		<OwnerCode>ACETESPHL</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Product Version=""2.0"">
			<OrgSupplierPart Action=""MERGE"">
				<PartNum>NODUPLICATE</PartNum>
				<StockKeepingUnit>UNT</StockKeepingUnit>
				<IsActive>true</IsActive>
				<Desc>BMATE 1060N 20KG1A2 6M</Desc>
				<CusClassPartPivotCollection>
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<ChildListOrder>0</ChildListOrder>
						<AdditionalInformationChildCollection>
							<AdditionalInformationChild Action=""MERGE"">
								<Type>PST</Type>
								<AddInfoCollection>
									<AddInfo>
										<Key>ProductType</Key>
										<Value>PS1</Value>
									</AddInfo>
									<AddInfo>
										<Key>BrandName</Key>
										<Value>MAR</Value>
									</AddInfo>
								</AddInfoCollection>
							</AdditionalInformationChild>
							<AdditionalInformationChild Action=""MERGE"">
								<Type>CCF</Type>
								<AddInfoCollection>
								  <AddInfo>
									<Key>AIRSEndUse</Key>
									<Value>104</Value>
								  </AddInfo>
								  <AddInfo>
									<Key>AllProgramInd</Key>
									<Value>Y</Value>
								  </AddInfo>
								</AddInfoCollection>
								<NAddInfoData></NAddInfoData>
								<LPCOCollection TableName=""CusCALPCO"">
								  <LPCO Action=""MERGE"">
									<ClusterKey>0</ClusterKey>
									<CommodityTypeCode></CommodityTypeCode>
									<DIFRefNumberOrLocation></DIFRefNumberOrLocation>
									<IsMixedCountryOfOrigin>false</IsMixedCountryOfOrigin>
									<ApplicantType></ApplicantType>
									<HolderType></HolderType>
									<StartDate></StartDate>
									<EndDate></EndDate>
									<IssueDate></IssueDate>
									<AlternativeQuotaQuantity>0.0</AlternativeQuotaQuantity>
									<AlternativeQuotaUQ></AlternativeQuotaUQ>
									<RefNo>REFNO2</RefNo>
									<SecondaryRefNo></SecondaryRefNo>
									<Type>10</Type>
									<AuthorizationCountry TableName=""RefCountry"" />
									<IssuanceCountryCode TableName=""RefCountry"" />
									<OriginCountryCode TableName=""RefCountry"" />
								  </LPCO>
								</LPCOCollection>
							  </AdditionalInformationChild>
						</AdditionalInformationChildCollection>

						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
					</CusClassPartPivot>
				</CusClassPartPivotCollection>
				<OrgPartRelationCollection>
					<OrgPartRelation Action=""MERGE"">
						<Relationship>OWN</Relationship>
						<OrgHeader>
							<Code>ACETESPHL</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
			</OrgSupplierPart>
		</Product>
	</Body>
</Native>
");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var qry = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			qry.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, "3214.10.00 10");
			var loadedPivot = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(qry).ToArray();
			AssertEquals(1, loadedPivot.Length);
			foreach (BusinessObject onePivot in loadedPivot)
			{
				var pstLine = (BusinessObjectCollection)onePivot["PSTLines"];
				AssertEquals("PST Line should remain after import.", 1, pstLine.Count);
			}
		}

		public void TestImportProductWithAdditionalTariffs()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ACETESPHL";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "130GDKFSNSPC";

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.UnitedStates;
			pivot[CusClassPartPivotSchema.CI_TariffNum] = "3924104000";
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";

			Factory.Save();

			var xml = string.Format(CultureInfo.InvariantCulture, @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
	<OwnerCode>USALGLSFO</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
	<Product version=""2.0"">
	  <OrgSupplierPart Action=""MERGE"">
		<PartNum>130GDKFSNSPC</PartNum>
		<Desc>PRODUCT WITH ADDITIONAL TARIFFS</Desc>
		<CusClassPartPivotCollection>
		  <CusClassPartPivot Action=""MERGE"">
			<ChildType>HTI</ChildType>
			<TariffNum>3924104000</TariffNum>
			<CusLineTariffDetailCollection>
			  <CusLineTariffDetail Action=""MERGE"">
				<Tariff>99031111</Tariff>
				<Type>AT1</Type>
				<DataModel>US</DataModel>
			  </CusLineTariffDetail>
			  <CusLineTariffDetail Action=""MERGE"">
				<Tariff>99032222</Tariff>
				<Type>AT2</Type>
				<DataModel>US</DataModel>
			  </CusLineTariffDetail>
			  <CusLineTariffDetail Action=""MERGE"">
				<Tariff>99033333</Tariff>
				<Type>AT3</Type>
				<DataModel>US</DataModel>
			  </CusLineTariffDetail>
			</CusLineTariffDetailCollection>
			<Country TableName=""RefCountry"">
				<Code>US</Code>
			</Country>
		  </CusClassPartPivot>
		</CusClassPartPivotCollection>
		<OrgPartRelationCollection>
		  <OrgPartRelation Action=""MERGE"">
			<Relationship>OWN</Relationship>
			<OrgHeader>
			  <Code>ACETESPHL</Code>
			</OrgHeader>
		  </OrgPartRelation>
		</OrgPartRelationCollection>
	  </OrgSupplierPart>
	</Product>
  </Body>
</Native>
");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();

			var qry = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			qry.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, "3924104000");
			var pivotAfterImport = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(qry).ToArray();

			AssertEquals(1, pivotAfterImport.Length);
			var loadedPivot = (BusinessObject)pivotAfterImport[0];
			var additionalTariffs = ((BusinessObjectCollection)loadedPivot["CusLineTariffDetails"]).ToArray();
			AssertEquals("3 Additional Tariffs added after import", 3, additionalTariffs.Length);

			var additionalTariff1 = additionalTariffs[0];
			AssertEquals("99031111", additionalTariff1["BZ_Tariff"]);
			AssertEquals("AT1", additionalTariff1["BZ_Type"]);
			var additionalTariff2 = additionalTariffs[1];
			AssertEquals("99032222", additionalTariff2["BZ_Tariff"]);
			AssertEquals("AT2", additionalTariff2["BZ_Type"]);
			var additionalTariff3 = additionalTariffs[2];
			AssertEquals("99033333", additionalTariff3["BZ_Tariff"]);
			AssertEquals("AT3", additionalTariff3["BZ_Type"]);
		}

		public void TestScenario04XMLMustBeValid()
		{
			RunXml("4. OWN=broken blank OH_Code.xml");
			var logs = GetLogs();
			AssertContains("Illegal XML; the Relationship requires a Code for the Organisation", logs);
			ResetLogs();
			RunXml("4. OWN=broken no OH_Code.xml");
			logs = GetLogs();
			AssertContains("Illegal XML; the Relationship requires a Code for the Organisation", logs);
			ResetLogs();
			RunXml("4. OWN=broken no relationships.xml");
			logs = GetLogs();
			AssertContains("Illegal XML: No relationships", logs);
			ResetLogs();
			RunXml("4. OWN=broken rel has no OH.xml");
			logs = GetLogs();
			AssertContains("Illegal XML; the Relationship requires an Organisation", logs);
			ResetLogs();
			RunXml("4. OWN=broken no relationship tag.xml");
			logs = GetLogs();
			AssertContains("Illegal XML: the OrgPartRelation requires a relationship type", logs);
			ResetLogs();
			RunXml("4. OWN=broken no PartNum.xml");
			logs = GetLogs();
			AssertContains("Illegal XML: the OrgPartRelation requires a part number", logs);
		}

		public void TestScenario05MergeNotMatchingExistingMustInsert_OWN()
		{
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				CreateExistingProduct(new string[] { craImpChi }, new string[] { abcExporters });
				CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			}
			RunXml("5. OWN=ABCEXPBNE merge.xml");
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts", logs);
			AssertProductNowExists(new string[] { abcExporters }, Array.Empty<string>());
		}

		public void TestScenario05NativeEngineShouldNotMatchOnNKFieldsWhenMerging()
		{
			// Same as above but with matching natural key fields such as desc - should not let the native engine do its own thing in finding a row, shoudl not allow it to look, thus will insert
			var product = CreateExistingProduct(new string[] { craImpChi }, new string[] { abcExporters });
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			}
			product.OP_IsActive = true;
			product.OP_Desc = "TRUCKS";
			product.OP_StockKeepingUnit = "UNT";
			product.OP_CountDecimalPlaces = 0;
			factory.Save();
			RunXml("5. OWN=ABCEXPBNE merge.xml");
			Assert("Product is still inserted even if all OP_XXXX properties mtch an extant row " + GetLogs(), GetLogs().Contains("OrgSupplierPart - 1 inserts"));
			AssertProductNowExists(new string[] { abcExporters }, Array.Empty<string>());
		}

		public void TestScenario06MergeShouldUpdateTheRightProduct()
		{
			CreateExistingProduct(new string[] { abcExporters }, Array.Empty<string>());
			CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			RunXml("6+8+9. OWN=CRAIMPCHI, SUP=BACCRI merge.xml");
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 1 updates", logs);
			AssertProductNowExists(new string[] { abcExporters }, Array.Empty<string>());
			var updated = AssertProductNowExists(new string[] { craImpChi }, new string[] { baccri });
			AssertEquals("Updated this one", updated.OP_Desc);
		}

		public void TestScenario07DuplicateExactMatch()
		{
			CreateExistingProduct(new string[] { abcExporters }, Array.Empty<string>());
			CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			RunXml("7. OWN=CRAIMPCHI, SUP=BACCRI insert.xml");
			var logs = GetLogs();
			AssertContains("Cannot insert, product 'PRODUCTA' with related organisations CRAIMPCHI/OWN, BACCRI/SUP already exists", logs);
		}

		public void TestScenario08MergeExactMatch()
		{
			CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			RunXml("6+8+9. OWN=CRAIMPCHI, SUP=BACCRI merge.xml");
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 1 updates", logs);
			var updated = AssertProductNowExists(new string[] { craImpChi }, new string[] { baccri });
			AssertEquals("Updated this one", updated.OP_Desc);
		}

		public void TestScenario09MergeDuplicate()
		{
			CreateExistingProduct(new string[] { craImpChi, abcExporters }, new string[] { baccri });
			RunXml("6+8+9. OWN=CRAIMPCHI, SUP=BACCRI merge.xml");
			var logs = GetLogs();
			AssertContains("Duplicate Product detected: Owner = CRAIMPCHI", logs);
			var notUpdated = AssertProductNowExists(new string[] { craImpChi, abcExporters }, new string[] { baccri });
			AssertEquals("Database Value", notUpdated.OP_Desc);
		}

		public void TestScenario10UpdateExistingByPK()
		{
			var product = CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			var xml = GetXmlToImport("10+11+13. Product B With PK placeholder.xml").ReadToEnd();
			xml = xml.Replace("{PK}", product.PK.ToString());
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 1 updates", logs);
			product.Reload();
			AssertEquals("Updated this one", product.OP_Desc);
			AssertEquals("PRODUCTB", product.OP_PartNum);
		}

		public void TestScenario11UpdateExistingByPartNumIfNoMatchOnPK()
		{
			var product = CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			var xml = GetXmlToImport("10+11+13. Product B With PK placeholder.xml").ReadToEnd();
			xml = xml.Replace("{PK}", ZGuid.NewZGuid().ToString());  // valid but points to no record
			xml = xml.Replace("PRODUCTB", "PRODUCTA");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 1 updates", logs);
			var updated = AssertProductNowExists(new string[] { craImpChi }, new string[] { baccri });
			AssertEquals("Updated this one", updated.OP_Desc);
		}

		// Scenario 12 seems to tbe the same as scenario 10

		public void TestScenario13DuplicateProductWhenUpdatePartNumByPK()
		{
			var productA = CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri }, "PRODUCTA");
			var productB = CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri }, "PRODUCTB");
			var xml = GetXmlToImport("10+11+13. Product B With PK placeholder.xml").ReadToEnd();
			xml = xml.Replace("{PK}", productA.PK.ToString());
			xml = xml.Replace("MERGE", "UPDATE");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("Duplicate Product detected", logs);
		}

		public void TestScenario14UpdateByPKEvenIfNoRelationsInXML()
		{
			var product = CreateExistingProduct(new string[] { craImpChi }, new string[] { baccri });
			var xml = GetXmlToImport("14. Product B With PK placeholder and no relationships.xml").ReadToEnd();
			xml = xml.Replace("{PK}", product.PK.ToString());
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 1 updates", logs); // OK to update by PK if a match even if no relations were in XML
			product.Reload();
			AssertEquals("Updated this one", product.OP_Desc);
			AssertEquals("PRODUCTB", product.OP_PartNum);

			product.OP_Desc = "Old";
			factory.Save();
			xml = GetXmlToImport("14. Product B With PK placeholder and no relationships.xml").ReadToEnd();
			xml = xml.Replace("{PK}", ZGuid.NewZGuid().ToString());
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			logs = GetLogs();
			AssertContains("Could not find match using primary key and", logs);
			product.Reload();
			AssertEquals("Old", product.OP_Desc);
		}

		public void TestScenario15IfThereAreOWNAndSUPThatAreTheSame()
		{
			RunXml("15. OWN and SUP are same.xml");
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var orgRelations = factory.Load<OrgPartRelation>(new ZQuery());
			AssertEquals(1, orgRelations.Length);
			AssertEquals("BTH", orgRelations.First().OU_Relationship);
		}

		public void TestScenario16IfThereAreOWNAndSUPThatMappingSameOrgHeader()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.New<OrgHeader>();
			org.OH_Code = "EDIDZAJNB";
			org.MainAddress.OA_Address1 = "ADD 1";

			var orgProxy = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var ov = orgProxy.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = "ORG";
			ov.OO_ForeignCode = "115161";
			ov.OO_LocalCode = org.OH_Code;
			ov.OO_LocalGuid = org.PK;

			ov = orgProxy.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = "ORG";
			ov.OO_ForeignCode = "USGENINC1SOU";
			ov.OO_LocalCode = org.OH_Code;
			ov.OO_LocalGuid = org.PK;
			factory.Save();

			RunXml("16. OWN and SUP mapping to same orgheader.xml");
			var logs = GetLogs();
			AssertContains(@"Importing Product: TICKETY
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
StmNote - 2 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "TICKETY"));
			var orgRelations = product.RelatedOrganisations;
			AssertEquals(1, orgRelations.Count);
			var orgRelation = orgRelations[0];
			AssertEquals("BTH", orgRelation.OU_Relationship);
			AssertEquals(org.PK, orgRelation.OU_OH);
		}

		public void TestScenario16ComponentPivotDoesNeedOrgSupplierPartNodeAndThatComponentsAreMatchedAndThatMissingCusUSClassificationsAreInjected()
		{
			TestScenario16("16. ChildPivots.xml");
		}

		public void TestScenario16WithoutPKs()
		{
			TestScenario16("16b. ChildPivots Without PKs.xml");
		}

		void TestScenario16(string testXmlFileName)
		{
			// Checks that we don't care if the component pivot lacks the <OrgSupplierPart> node.

			var xml = GetXmlToImport(testXmlFileName).ReadToEnd();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
ComponentCusClassPartPivot - 3 inserts, 0 updates, 0 deletes
CusUSClassification - 4 inserts, 0 updates, 0 deletes
CusCodeDataCensus - 2 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var orgRelation = factory.LoadTop1<OrgPartRelation>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(4, pivots.Length);
			var parentPivot = (from Enterprise.Integration.Customs.US.ICusClassPartPivot p in pivots where (ZGuid)((BusinessObject)p)[CusClassPartPivotSchema.CI_CI_Parent.Name] == ZGuid.Empty select p).First();
			var childPivot2 = pivots.Single(x => x.CI_TariffNum == "22222222");
			var childPivot3 = pivots.Single(x => x.CI_TariffNum == "3333333333");
			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, childPivot3.PK);
			query.AddToFilter(CusCodeDataSchema.CY_Type, "CWO");
			var cusCodeDataCensus = factory.Load<Enterprise.Integration.Customs.US.ICensusWarningOverride>(query).Single();

			((BusinessObject)childPivot2).Delete();
			((BusinessObject)cusCodeDataCensus).Delete();

			// Now add an extra component to the record and reload the file.
			// Check that when we merge for a pivot that has existing component pivots, the existing component pivots are matched; if not matched then it will be deleted.
			var newComponentPivot = factory.New<Enterprise.Integration.Customs.US.ICusClassPartPivot>();
			newComponentPivot.CI_CI_Parent = ((BusinessObject)parentPivot).PK;
			newComponentPivot.CI_OP = product.PK;
			newComponentPivot.CI_TariffNum = "12345";
			newComponentPivot.CI_ChildListOrder = 4;
			factory.Save();
			// need to create in different factory to ensure correct type decider
			var factory2 = new BusinessObjectFactory();
			var cusCodeDataCensus2 = factory2.New<Enterprise.Integration.Customs.US.ICensusWarningOverride>();
			cusCodeDataCensus2.CY_ParentID = newComponentPivot.PK;
			cusCodeDataCensus2.CY_ParentTableCode = CusClassPartPivotSchema.Constants.Prefix;
			cusCodeDataCensus2.CY_Code = "census";
			cusCodeDataCensus2.CY_Data = "C4";

			cusCodeDataCensus = factory2.New<Enterprise.Integration.Customs.US.ICensusWarningOverride>();
			cusCodeDataCensus.CY_ParentID = childPivot3.PK;
			cusCodeDataCensus.CY_ParentTableCode = CusClassPartPivotSchema.Constants.Prefix;
			cusCodeDataCensus.CY_Code = "census";
			cusCodeDataCensus.CY_Data = "C4";

			factory2.Save();
			cusCodeDataCensus2 = factory.Load<Enterprise.Integration.Customs.US.ICensusWarningOverride>(cusCodeDataCensus2.PK);

			SetupImporter();
			dummyLogger.Clear();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			logs = GetLogs();
			AssertContains("If is not working it might be it's (not) considering OU too. ",
				@"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
ComponentCusClassPartPivot - 1 inserts, 0 updates, 1 deletes
CusCodeDataCensus - 1 inserts, 0 updates, 2 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes", logs);
			pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals("The new pivots has been clobbered - we still have 1+3=4", 4, pivots.Length);
		}

		public void TestScenario17aMissingCountry()
		{
			// Checks that a parent pivot with a missing country borks
			RunXml("17a. PivotHasMissingCountry.xml");
			var logs = GetLogs();
			AssertContains("must have a country", logs);
		}

		public void TestScenario17bMissingCountry()
		{
			// Checks that component pivots with a missing country code (in three different ways of being missing) doesn't bork but uses the parent pivot's country
			RunXml("17b. ChildPivotsHaveMissingCountry.xml");
			var logs = GetLogs();
			AssertContains("ComponentCusClassPartPivot - 3 inserts", logs);
			var query = new ZQuery(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.IsNotBlank, string.Empty);
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals("All four (parent plus three kids) pivots are for US", 4, pivots.Length);
		}

		public void TestScenario17cBadStartDate()
		{
			// Checks that a parent pivot with a bad date format fails gracefully
			CreateExistingProduct(new string[] { craImpChi }, new string[] { abcExporters });
			RunXml("17c. PivotHasBadStartDate.xml");

			var logs = GetLogs();
			AssertContains("OrgSupplierPart.CusClassPartPivot.DateStart validation failed: '<value-of xmlns:sx=\"http://www.servingxml.com/core\" select=\"concat(substring(effectiveDate, 1, 4), '-', substring(effectiveDate, 5, 2), '-', substring(effectiveDate, 7, 2))\" />' could not be converted to type smalldatetime", logs);
		}

		public void TestScenario17dInvalidStartDate()
		{
			// Checks that a parent pivot with an empty date field fails gracefully
			CreateExistingProduct(new string[] { craImpChi }, new string[] { abcExporters });
			RunXml("17d. PivotHasInvalidStartDate.xml");

			var logs = GetLogs();
			AssertContains("OrgSupplierPart.CusClassPartPivot.DateStart validation failed: '1750-01-01T00:00:00' is not within the range for smalldatetime (1900-01-01T00:00:00 - 2079-06-06T23:59:29)", logs);
		}

		public void TestScenario18NoDuplicatePivots_a1()
		{
			// Ensure each pivot on a part is unique when compared using a limited number of columns - otherwise they'll overwrite each other.
			var xml = GetXmlToImport("18. Duplicate pivots when ignoring tariff.xml").ReadToEnd();
			xml = xml.Replace("{SECONDPIVOTCHILDTYPE}", "HTI"); // will give a dupe
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("duplicate pivots", logs);
		}

		public void TestScenario18NoDuplicatePivots_a2()
		{
			// Same file as above but with one field slightly different, therefore no dupe and no bork
			var xml = GetXmlToImport("18. Duplicate pivots when ignoring tariff.xml").ReadToEnd();
			xml = xml.Replace("{SECONDPIVOTCHILDTYPE}", "HTE");  // no dupe
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("CusClassPartPivot - 2 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario18NoDuplicatePivots_a3()
		{
			// Ensure each pivot on a part is unique when compared using a limited number of columns - otherwise they'll overwrite each other.
			var xml = GetXmlToImport("18c. Duplicate pivots when ignoring tariff.xml").ReadToEnd();
			xml = xml.Replace("{SECONDPIVOTCHILDTYPE}", "HTI"); // will give a dupe
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains(@"This Part has invalid HTI (Country='US', Party='') attributes. Attributes have been specified on other pivots and therefore must be specified on all pivots.", logs);
		}

		public void TestScenario18NoDuplicatePivots_b1()
		{
			// Ensure each pivot on a part is unique when compared using a limited number of columns - otherwise they'll overwrite each other.
			// Here, a node is present in one pivot and missing from the other - check that we still use this field for comparison
			RunXml("18b. Duplicate pivots when ignoring tariff.xml");
			var logs = GetLogs();
			AssertContains("CusClassPartPivot - 2 inserts", logs);
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery());
			var bearsBirthday = new ZDateTime(2015, 3, 12, 0, 0, 0).ToString();
			var startDate1 = ((BusinessObject)pivots[0])["CI_DateStart"].ToString();
			var startDate2 = ((BusinessObject)pivots[1])["CI_DateStart"].ToString();
			Assert(
				(startDate1 == bearsBirthday && string.IsNullOrEmpty(startDate2))
				||
				(startDate2 == bearsBirthday && string.IsNullOrEmpty(startDate1))
				);
		}

		public void TestScenario19aDuplicateChildren()
		{
			// Checks that if a pivot has component child whose child sequences are not unique, we bork
			var xml = GetXmlToImport("19. Duplicate child pivots.xml").ReadToEnd();
			xml = xml.Replace("{SECONDCHILDSEQUENCE}", "1");  //   dupe
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("Child pivots must have different sequences", logs);
		}

		public void TestScenario19bDuplicateChildren_1()
		{
			// Converse of above - don't bortk when all component child sequences are unique.
			// Also checks that you can have two parent pivots, each with their own child piots, and the requirement for uniqueness of child sequences is limited only to brothers on a parent (no cross-checking of cousins is needed)
			var xml = GetXmlToImport("19. Duplicate child pivots.xml").ReadToEnd();
			xml = xml.Replace("{SECONDCHILDSEQUENCE}", "2");  // no dupe
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("CusClassPartPivot - 2 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario20OrgPartRelationIsSetCorrectly()
		{
			// Scenarios 15 and below set OU on OP but don't set CI_OH too.  This checks that CI_OH is set on the parent pivot.
			var xml = GetXmlToImport("20. Children and OrgPartRelation.xml").ReadToEnd();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
ComponentCusClassPartPivot - 3 inserts, 0 updates, 0 deletes", logs);
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var craImpChiOrg = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, craImpChi);
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(4, pivots.Length);
			var parentPivot = (from Enterprise.Integration.Customs.US.ICusClassPartPivot p in pivots where (ZGuid)((BusinessObject)p)[CusClassPartPivotSchema.CI_CI_Parent.Name] == ZGuid.Empty select p).First();
			AssertEquals("CI_OH has been set correctly", craImpChiOrg.PK, ((BusinessObject)parentPivot)[CusClassPartPivotSchema.CI_OH.Name]);
		}

		public void TestScenario20OldOrgPartRelationCanBeImported()
		{
			var xml = GetXmlToImport("20b. Old Children and OrgPartRelation.xml").ReadToEnd();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
ComponentCusClassPartPivot - 3 inserts, 0 updates, 0 deletes", logs);
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var craImpChiOrg = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, craImpChi);
			var orgRelation = factory.LoadTop1<OrgPartRelation>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(4, pivots.Length);
			var parentPivot = (from Enterprise.Integration.Customs.US.ICusClassPartPivot p in pivots where (ZGuid)((BusinessObject)p)[CusClassPartPivotSchema.CI_CI_Parent.Name] == ZGuid.Empty select p).First();
			AssertEquals("CI_OH has been set correctly", craImpChiOrg.PK, ((BusinessObject)parentPivot)[CusClassPartPivotSchema.CI_OH.Name]);
		}

		public void TestScenario21DoNotFlushAllChildrenOfPivotIfMatched()
		{
			RunXml("21. Children of component pivots.xml");    // Insert
			var rowFactory = new RowFactory(Db.Connection, null);
			AssertEquals(14, rowFactory.GetDatabaseCount("CusAddInfo"));
			ResetLogs();
			RunXml("21. Children of component pivots.xml");  // Match using PKs....
			AssertEquals(14, rowFactory.GetDatabaseCount("CusAddInfo"));
			var logs = GetLogs();
			CombineAssertions(() =>
			{
				AssertContains(@"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusClassPartPivot - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusAttributeFilter - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusCodeDataCensus - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"AdditionalInformationChild - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"AdditionalInformationGrandChild - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"AdditionalInformationGreatGrandChild - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusCodeDataFdaAffirmation - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusUSClassification - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"ComponentCusClassPartPivot - 0 inserts, 0 updates, 0 deletes", logs);
			});
			ResetLogs();
			RunXml("21. Children of component pivots without PKs.xml");  // Match using other field PKs....
			AssertEquals(14, rowFactory.GetDatabaseCount("CusAddInfo"));
			logs = GetLogs();
			CombineAssertions(() =>
			{
				AssertContains(@"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusClassPartPivot - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusAttributeFilter - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusCodeDataCensus - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"AdditionalInformationChild - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"AdditionalInformationGrandChild - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"AdditionalInformationGreatGrandChild - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusCodeDataFdaAffirmation - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"CusUSClassification - 0 inserts, 0 updates, 0 deletes", logs);
				AssertContains(@"ComponentCusClassPartPivot - 0 inserts, 0 updates, 0 deletes", logs);
			});
		}

		public void TestMatchComponentCusClassPartPivot_Country()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			usPivot.CI_OP = product.PK;
			usPivot.CI_ChildType = "HTI";
			usPivot.CI_TariffNum = "3214.10.00 10";
			usPivot.CI_RN_NKCountry = "US";
			var componentPivot = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot.CI_OP = product.PK;
			componentPivot.CI_CI_Parent = usPivot.PK;
			componentPivot.CI_ChildType = "COM";
			componentPivot.CI_TariffNum = "1111.11";
			componentPivot.CI_RN_NKCountry = "US";
			componentPivot.CI_ChildListOrder = 1;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<PK>{Core.Constants.CountryGuids.UnitedStates}</PK>
								</Country>
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, usPivot.PK);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot.PK, pivots[0].PK);
			var cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot.PK));
			AssertEquals(1, cusAddInfos.Length);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestMatchComponentCusClassPartPivot_CountryOfOrigin()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			usPivot.CI_OP = product.PK;
			usPivot.CI_ChildType = "HTI";
			usPivot.CI_TariffNum = "3214.10.00 10";
			usPivot.CI_RN_NKCountry = "US";
			var componentPivot1 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot1.CI_OP = product.PK;
			componentPivot1.CI_CI_Parent = usPivot.PK;
			componentPivot1.CI_ChildType = "COM";
			componentPivot1.CI_TariffNum = "1111.11";
			componentPivot1.CI_RN_NKCountry = "US";
			componentPivot1.CI_RN_NKCountryOfOrigin = "NZ";
			componentPivot1.CI_ChildListOrder = 1;
			var componentPivot2 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot2.CI_OP = product.PK;
			componentPivot2.CI_CI_Parent = usPivot.PK;
			componentPivot2.CI_ChildType = "COM";
			componentPivot2.CI_TariffNum = "1111.11";
			componentPivot2.CI_RN_NKCountry = "US";
			componentPivot2.CI_RN_NKCountryOfOrigin = "AU";
			componentPivot2.CI_ChildListOrder = 1;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<CountryOfOrigin TableName=""RefCountry"">
									<PK>{Core.Constants.CountryGuids.Australia}</PK>
								</CountryOfOrigin>
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, usPivot.PK);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			var cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);

			pivots[0].CI_RN_NKCountryOfOrigin = ZString.Empty;
			((BusinessObject)cusAddInfos[0]).Delete();
			newFactory.Save();

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<CountryOfOrigin />
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestMatchComponentCusClassPartPivot_CountryOfExport()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			usPivot.CI_OP = product.PK;
			usPivot.CI_ChildType = "HTI";
			usPivot.CI_TariffNum = "3214.10.00 10";
			usPivot.CI_RN_NKCountry = "US";
			var componentPivot1 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot1.CI_OP = product.PK;
			componentPivot1.CI_CI_Parent = usPivot.PK;
			componentPivot1.CI_ChildType = "COM";
			componentPivot1.CI_TariffNum = "1111.11";
			componentPivot1.CI_RN_NKCountry = "US";
			componentPivot1.CI_RN_NKCountryOfExport = "NZ";
			componentPivot1.CI_ChildListOrder = 1;
			var componentPivot2 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot2.CI_OP = product.PK;
			componentPivot2.CI_CI_Parent = usPivot.PK;
			componentPivot2.CI_ChildType = "COM";
			componentPivot2.CI_TariffNum = "1111.11";
			componentPivot2.CI_RN_NKCountry = "US";
			componentPivot2.CI_RN_NKCountryOfExport = "AU";
			componentPivot2.CI_ChildListOrder = 1;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<CountryOfExport TableName=""RefCountry"">
									<PK>{Core.Constants.CountryGuids.Australia}</PK>
								</CountryOfExport>
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, usPivot.PK);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			var cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);

			pivots[0].CI_RN_NKCountryOfExport = ZString.Empty;
			((BusinessObject)cusAddInfos[0]).Delete();
			newFactory.Save();

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<CountryOfExport />
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestMatchComponentCusClassPartPivot_OriginState()
		{
			var qld = new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode("QLD", "AU");
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			usPivot.CI_OP = product.PK;
			usPivot.CI_ChildType = "HTI";
			usPivot.CI_TariffNum = "3214.10.00 10";
			usPivot.CI_RN_NKCountry = "US";
			var componentPivot1 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot1.CI_OP = product.PK;
			componentPivot1.CI_CI_Parent = usPivot.PK;
			componentPivot1.CI_ChildType = "COM";
			componentPivot1.CI_TariffNum = "1111.11";
			componentPivot1.CI_RN_NKCountry = "US";
			componentPivot1.CI_RW_NKOriginState = "NSW";
			componentPivot1.CI_ChildListOrder = 1;
			var componentPivot2 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot2.CI_OP = product.PK;
			componentPivot2.CI_CI_Parent = usPivot.PK;
			componentPivot2.CI_ChildType = "COM";
			componentPivot2.CI_TariffNum = "1111.11";
			componentPivot2.CI_RN_NKCountry = "US";
			componentPivot2.CI_RW_NKOriginState = "QLD";
			componentPivot2.CI_ChildListOrder = 1;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<OriginState TableName=""RefCountryStates"">
									<PK>{qld.PK}</PK>
								</OriginState>
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, usPivot.PK);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			var cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);

			pivots[0].CI_RW_NKOriginState = ZString.Empty;
			((BusinessObject)cusAddInfos[0]).Delete();
			newFactory.Save();

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<OriginState />
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestMatchComponentCusClassPartPivot_TaxType()
		{
			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			var taxOrFee = helper.CreateTaxOrFee("AU", ZDecimal.Zero, "US", new ZDateTime(2020, 1, 1), new ZDateTime(2021, 1, 1));

			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			usPivot.CI_OP = product.PK;
			usPivot.CI_ChildType = "HTI";
			usPivot.CI_TariffNum = "3214.10.00 10";
			usPivot.CI_RN_NKCountry = "US";
			var componentPivot1 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot1.CI_OP = product.PK;
			componentPivot1.CI_CI_Parent = usPivot.PK;
			componentPivot1.CI_ChildType = "COM";
			componentPivot1.CI_TariffNum = "1111.11";
			componentPivot1.CI_RN_NKCountry = "US";
			componentPivot1.CI_ZZF_NKTaxType = "NZ";
			componentPivot1.CI_ChildListOrder = 1;
			var componentPivot2 = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			componentPivot2.CI_OP = product.PK;
			componentPivot2.CI_CI_Parent = usPivot.PK;
			componentPivot2.CI_ChildType = "COM";
			componentPivot2.CI_TariffNum = "1111.11";
			componentPivot2.CI_RN_NKCountry = "US";
			componentPivot2.CI_ZZF_NKTaxType = "AU";
			componentPivot2.CI_ChildListOrder = 1;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<TaxType TableName=""RefDatabase_RefCusTaxOrFee"">
									<PK>{taxOrFee.PK}</PK>
								</TaxType>
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, usPivot.PK);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			var cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);

			pivots[0].CI_ZZF_NKTaxType = ZString.Empty;
			((BusinessObject)cusAddInfos[0]).Delete();
			newFactory.Save();

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<TaxType />
								<AdditionalInformationChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationChild Action=""MERGE"">
										<Type>AMS</Type>
										<AddInfoData>Key1</AddInfoData>
									</AdditionalInformationChild>
								</AdditionalInformationChildCollection>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot2.PK, pivots[0].PK);
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, componentPivot2.PK));
			AssertEquals(1, cusAddInfos.Length);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestMatchCusClassPartPivot_Country()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var auPivot = Factory.New<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>();
			auPivot.CI_OP = product.PK;
			auPivot.CI_ChildType = "HTI";
			auPivot.CI_TariffNum = "3214.10.00 10";
			auPivot.CI_RN_NKCountry = "ZZ";
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			auPivot = pivots.Single(x => x.PK == auPivot.PK);
			var usPivot = pivots.Single(x => x.CI_RN_NKCountry == "US");
			AssertCusClassPartPivot(auPivot, "HTI", "ZZ", "3214.10.00 10", "");
			AssertCusClassPartPivot(usPivot, "HTI", "US", "3214.10.00 10", "");

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<PK>{Core.Constants.CountryGuids.UnitedStates}</PK>
						</Country>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			auPivot = pivots.Single(x => x.PK == auPivot.PK);
			usPivot = pivots.Single(x => x.PK == usPivot.PK);
			AssertCusClassPartPivot(auPivot, "HTI", "ZZ", "3214.10.00 10", "");
			AssertCusClassPartPivot(usPivot, "HTI", "US", "3214.10.00 10", "");

			AssertEquals(0, newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, auPivot.PK)).Length);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot.PK));
			AssertEquals(1, pivots.Length);
			var componentPivot = pivots[0];
			AssertCusClassPartPivot(componentPivot, "COM", "US", "1111.11", "");
		}

		public void TestMatchCusClassPartPivot_OrgHeader()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";
			var orgCode2 = "USAMERPHL2";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = orgCode2;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot1 = Factory.New<Enterprise.Integration.Customs.US.ICusClassPartPivot>();
			usPivot1.CI_OP = product.PK;
			usPivot1.CI_ChildType = "HTI";
			usPivot1.CI_TariffNum = "3214.10.00 10";
			usPivot1.CI_RN_NKCountry = "US";
			usPivot1.CI_OH = supplier.PK;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<OrgHeader Action=""MERGE"">
							<Code>USAMERPHL</Code>
						</OrgHeader>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			AssertNotNull(pivots.Single(x => x.PK == usPivot1.PK && x.CI_OH == supplier.PK));
			var usPivot2 = pivots.Single(x => x.CI_OH == buyer.PK);

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<OrgHeader Action=""MERGE"">
							<Code>USAMERPHL</Code>
						</OrgHeader>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			AssertNotNull(pivots.Single(x => x.PK == usPivot1.PK && x.CI_OH == supplier.PK));
			usPivot2 = pivots.Single(x => x.PK == usPivot2.PK && x.CI_OH == buyer.PK);

			AssertEquals(0, newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot1.PK)).Length);
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot2.PK));
			AssertEquals(1, pivots.Length);

			usPivot2.CI_OH = ZGuid.Empty;
			((BusinessObject)pivots[0]).Delete();
			newFactory.Save();

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<OrgHeader />
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			AssertNotNull(pivots.Single(x => x.PK == usPivot1.PK && x.CI_OH == supplier.PK));
			usPivot2 = pivots.Single(x => x.PK == usPivot2.PK && x.CI_OH.IsEmpty);

			AssertEquals(0, newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot1.PK)).Length);
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot2.PK));
			AssertEquals(1, pivots.Length);
		}

		public void TestMatchCusClassPartPivot_DateStart()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";
			var dateStart1 = new ZDateTime(2020, 10, 1);
			var dateStart2 = new ZDateTime(2020, 11, 1);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot1 = Factory.New<Enterprise.Integration.Customs.US.ICusClassPartPivot>();
			usPivot1.CI_OP = product.PK;
			usPivot1.CI_ChildType = "HTI";
			usPivot1.CI_TariffNum = "3214.10.00 10";
			usPivot1.CI_RN_NKCountry = "US";
			usPivot1.CI_DateStart = dateStart1;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<DateStart>2020-11-01T00:00:00</DateStart>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			AssertNotNull(pivots.Single(x => x.PK == usPivot1.PK && x.CI_DateStart == dateStart1));
			var usPivot2 = pivots.Single(x => x.CI_DateStart == dateStart2);

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<DateStart>2020-11-01T00:00:00</DateStart>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			AssertNotNull(pivots.Single(x => x.PK == usPivot1.PK && x.CI_DateStart == dateStart1));
			AssertNotNull(pivots.Single(x => x.PK == usPivot2.PK && x.CI_DateStart == dateStart2));

			AssertEquals(0, newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot1.PK)).Length);
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot2.PK));
			AssertEquals(1, pivots.Length);
		}

		public void TestMatchCusClassPartPivot_DateEnd()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";
			var dateEnd1 = new ZDateTime(2020, 10, 1);
			var dateEnd2 = new ZDateTime(2020, 11, 1);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var usPivot1 = Factory.New<Enterprise.Integration.Customs.US.ICusClassPartPivot>();
			usPivot1.CI_OP = product.PK;
			usPivot1.CI_ChildType = "HTI";
			usPivot1.CI_TariffNum = "3214.10.00 10";
			usPivot1.CI_RN_NKCountry = "US";
			usPivot1.CI_DateEnd = dateEnd1;
			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<DateEnd>2020-11-01T00:00:00</DateEnd>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			AssertNotNull(pivots.Single(x => x.PK == usPivot1.PK && x.CI_DateEnd == dateEnd1));
			var usPivot2 = pivots.Single(x => x.CI_DateEnd == dateEnd2);

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<DateEnd>2020-11-01T00:00:00</DateEnd>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(query);
			AssertEquals(2, pivots.Length);
			AssertNotNull(pivots.Single(x => x.PK == usPivot1.PK && x.CI_DateEnd == dateEnd1));
			AssertNotNull(pivots.Single(x => x.PK == usPivot2.PK && x.CI_DateEnd == dateEnd2));

			AssertEquals(0, newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot1.PK)).Length);
			pivots = newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, usPivot2.PK));
			AssertEquals(1, pivots.Length);
		}

		public void TestMatchCusClassPartPivot_AddInfo()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<AddInfo>Key3=D*Key1=B*Key2=A*</AddInfo>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<AddInfo>*Key22=A*Key11=B*Key33=D</AddInfo>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			var pivot = pivots[0];
			AssertCusClassPartPivot(pivot, "HTI", "US", "3214.10.00 10", "Key1=B*Key2=A*Key3=D");
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, pivot.PK));
			AssertEquals(1, pivots.Length);
			var componentPivot = pivots[0];
			AssertCusClassPartPivot(componentPivot, "COM", "US", "1111.11", "Key11=B*Key22=A*Key33=D");

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<AddInfo>Key1=B*Key3=D*Key2=A*</AddInfo>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>{componentPivot.CI_TariffNum}</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<AddInfo>Key33=D*Key11=B*Key22=A*</AddInfo>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(pivot.PK, pivots[0].PK);
			pivot = pivots[0];
			AssertCusClassPartPivot(pivot, "HTI", "US", "3214.10.00 10", "Key1=B*Key2=A*Key3=D");
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, pivot.PK));
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot.PK, pivots[0].PK);
			componentPivot = pivots[0];
			AssertCusClassPartPivot(componentPivot, "COM", "US", "1111.11", "Key11=B*Key22=A*Key33=D");
		}

		public void TestMatchCusClassPartPivot_NAddInfo()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<NAddInfo>Key3=D*Key1=B*Key2=A*</NAddInfo>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>1111.11</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<NAddInfo>*Key22=A*Key11=B*Key33=D</NAddInfo>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			var pivot = pivots[0];
			AssertCusClassPartPivot(pivot, "HTI", "US", "3214.10.00 10", "", "Key1=B*Key2=A*Key3=D");
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, pivot.PK));
			AssertEquals(1, pivots.Length);
			var componentPivot = pivots[0];
			AssertCusClassPartPivot(componentPivot, "COM", "US", "1111.11", "", "Key11=B*Key22=A*Key33=D");

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<NAddInfo>Key1=B*Key3=D*Key2=A*</NAddInfo>
						<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
							<ComponentCusClassPartPivot Action=""MERGE"">
								<UsageComment></UsageComment>
								<TariffNum>{componentPivot.CI_TariffNum}</TariffNum>
								<ChildType>COM</ChildType>
								<ChildListOrder>1</ChildListOrder>
								<Country TableName=""RefCountry"">
									<Code>US</Code>
								</Country>
								<NAddInfo>Key33=D*Key11=B*Key22=A*</NAddInfo>
							</ComponentCusClassPartPivot>
						</ComponentCusClassPartPivotCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query);
			AssertEquals(1, pivots.Length);
			AssertEquals(pivot.PK, pivots[0].PK);
			pivot = pivots[0];
			AssertCusClassPartPivot(pivot, "HTI", "US", "3214.10.00 10", "", "Key1=B*Key2=A*Key3=D");
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, pivot.PK));
			AssertEquals(1, pivots.Length);
			AssertEquals(componentPivot.PK, pivots[0].PK);
			componentPivot = pivots[0];
			AssertCusClassPartPivot(componentPivot, "COM", "US", "1111.11", "", "Key11=B*Key22=A*Key33=D");
		}

		public void TestMatchAdditionalInformation_AddInfoData()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<AdditionalInformationChildCollection TableName=""CusAddInfo"">
							<AdditionalInformationChild Action=""MERGE"">
								<Type>PGA</Type>
								<AddInfoData>Key3=D*Key1=B*Key2=A*</AddInfoData>
								<AdditionalInformationGrandChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationGrandChild Action=""MERGE"">
										<Type>US7</Type>
										<AddInfoData>Key7=ValueC*Key6=ValueA*Key5=ValueB</AddInfoData>
										<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
											<AdditionalInformationGreatGrandChild Action=""MERGE"">
												<Type>VV1</Type>
												<AddInfoData>Key3=ValueC*Key2=ValueA*Key1=ValueB</AddInfoData>
											</AdditionalInformationGreatGrandChild>
										</AdditionalInformationGreatGrandChildCollection>
									</AdditionalInformationGrandChild>
								</AdditionalInformationGrandChildCollection>
							</AdditionalInformationChild>
						</AdditionalInformationChildCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(1, pivots.Length);
			var pivot = pivots[0];
			var cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, pivot.PK));
			AssertEquals(1, cusAddInfos.Length);
			var cusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Child", cusAddInfo, "PGA", "Key1=B*Key2=A*Key3=D");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, cusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			var grandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Grand Child", grandCusAddInfo, "US7", "Key5=ValueB*Key6=ValueA*Key7=ValueC");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, grandCusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			var greatGrandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Great Grand Child", greatGrandCusAddInfo, "VV1", "Key1=ValueB*Key2=ValueA*Key3=ValueC");

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<AdditionalInformationChildCollection TableName=""CusAddInfo"">
							<AdditionalInformationChild Action=""MERGE"">
								<Type>PGA</Type>
								<AddInfoData>Key2=A*Key3=D*Key1=B*</AddInfoData>
								<AdditionalInformationGrandChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationGrandChild Action=""MERGE"">
										<Type>US7</Type>
										<AddInfoData>Key6=ValueA*Key7=ValueC*Key5=ValueB</AddInfoData>
										<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
											<AdditionalInformationGreatGrandChild Action=""MERGE"">
												<Type>VV1</Type>
												<AddInfoData>Key2=ValueA*Key3=ValueC*Key1=ValueB</AddInfoData>
											</AdditionalInformationGreatGrandChild>
										</AdditionalInformationGreatGrandChildCollection>
									</AdditionalInformationGrandChild>
								</AdditionalInformationGrandChildCollection>
							</AdditionalInformationChild>
						</AdditionalInformationChildCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(1, pivots.Length);
			pivot = pivots[0];
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, pivot.PK));
			AssertEquals(1, cusAddInfos.Length);
			cusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Child", cusAddInfo, "PGA", "Key1=B*Key2=A*Key3=D");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, cusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			grandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Grand Child", grandCusAddInfo, "US7", "Key5=ValueB*Key6=ValueA*Key7=ValueC");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, grandCusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			greatGrandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Great Grand Child", greatGrandCusAddInfo, "VV1", "Key1=ValueB*Key2=ValueA*Key3=ValueC");
			AssertEquals("CusAddInfoTypeDecider.GetTypeForLoad|ParentBO:Enterprise.Customs.Business.MultiLineAddInfos.UnknownCusAddInfo|B7_Type:VV1", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestMatchAdditionalInformation_NAddInfoData()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<AdditionalInformationChildCollection TableName=""CusAddInfo"">
							<AdditionalInformationChild Action=""MERGE"">
								<Type>PGA</Type>
								<NAddInfoData>Key3=D*Key1=B*Key2=A*</NAddInfoData>
								<AdditionalInformationGrandChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationGrandChild Action=""MERGE"">
										<Type>US7</Type>
										<NAddInfoData>Key7=ValueC*Key6=ValueA*Key5=ValueB</NAddInfoData>
										<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
											<AdditionalInformationGreatGrandChild Action=""MERGE"">
												<Type>VV1</Type>
												<NAddInfoData>Key3=ValueC*Key2=ValueA*Key1=ValueB</NAddInfoData>
											</AdditionalInformationGreatGrandChild>
										</AdditionalInformationGreatGrandChildCollection>
									</AdditionalInformationGrandChild>
								</AdditionalInformationGrandChildCollection>
							</AdditionalInformationChild>
						</AdditionalInformationChildCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(1, pivots.Length);
			var pivot = pivots[0];
			var cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, pivot.PK));
			AssertEquals(1, cusAddInfos.Length);
			var cusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Child", cusAddInfo, "PGA", "", "Key1=B*Key2=A*Key3=D");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, cusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			var grandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Grand Child", grandCusAddInfo, "US7", "", "Key5=ValueB*Key6=ValueA*Key7=ValueC");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, grandCusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			var greatGrandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Great Grand Child", greatGrandCusAddInfo, "VV1", "", "Key1=ValueB*Key2=ValueA*Key3=ValueC");

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<AdditionalInformationChildCollection TableName=""CusAddInfo"">
							<AdditionalInformationChild Action=""MERGE"">
								<Type>PGA</Type>
								<NAddInfoData>Key2=A*Key3=D*Key1=B*</NAddInfoData>
								<AdditionalInformationGrandChildCollection TableName=""CusAddInfo"">
									<AdditionalInformationGrandChild Action=""MERGE"">
										<Type>US7</Type>
										<NAddInfoData>Key6=ValueA*Key7=ValueC*Key5=ValueB</NAddInfoData>
										<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
											<AdditionalInformationGreatGrandChild Action=""MERGE"">
												<Type>VV1</Type>
												<NAddInfoData>Key2=ValueA*Key3=ValueC*Key1=ValueB</NAddInfoData>
											</AdditionalInformationGreatGrandChild>
										</AdditionalInformationGreatGrandChildCollection>
									</AdditionalInformationGrandChild>
								</AdditionalInformationGrandChildCollection>
							</AdditionalInformationChild>
						</AdditionalInformationChildCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(1, pivots.Length);
			pivot = pivots[0];
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, pivot.PK));
			AssertEquals(1, cusAddInfos.Length);
			cusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Child", cusAddInfo, "PGA", "", "Key1=B*Key2=A*Key3=D");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, cusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			grandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Grand Child", grandCusAddInfo, "US7", "", "Key5=ValueB*Key6=ValueA*Key7=ValueC");
			cusAddInfos = newFactory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, grandCusAddInfo.PK));
			AssertEquals(1, cusAddInfos.Length);
			greatGrandCusAddInfo = cusAddInfos[0];
			AssertCusAddInfo("Great Grand Child", greatGrandCusAddInfo, "VV1", "", "Key1=ValueB*Key2=ValueA*Key3=ValueC");
			AssertEquals("CusAddInfoTypeDecider.GetTypeForLoad|ParentBO:Enterprise.Customs.Business.MultiLineAddInfos.UnknownCusAddInfo|B7_Type:VV1", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestNonMatchedChildrenOfPivotAreDeleted()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>AU</Code>
						</Country>
						<CusAttributeFilterCollection>
							<CusAttributeFilter Action=""MERGE"">
								<AttributeName>AT1</AttributeName>
								<AttributeValue1>ATT P1</AttributeValue1>
							</CusAttributeFilter>
						</CusAttributeFilterCollection>
					</CusClassPartPivot>
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>4214100010</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<CusAttributeFilterCollection>
							<CusAttributeFilter Action=""MERGE"">
								<AttributeName>AT1</AttributeName>
								<AttributeValue1>ATT P1</AttributeValue1>
							</CusAttributeFilter>
						</CusAttributeFilterCollection>
					</CusClassPartPivot>
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>5214100010</TariffNum>
						<ChildType>HTE</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<CusAttributeFilterCollection>
							<CusAttributeFilter Action=""MERGE"">
								<AttributeName>AT1</AttributeName>
								<AttributeValue1>ATT P1</AttributeValue1>
							</CusAttributeFilter>
						</CusAttributeFilterCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusCodeData>(), 1);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(3, pivots.Length);
			var pivot1 = pivots.Single(x => x.CI_TariffNum == "3214.10.00 10");
			var pivot2 = pivots.Single(x => x.CI_TariffNum == "4214100010");
			var pivot3 = pivots.Single(x => x.CI_TariffNum == "5214100010");
			AssertCusClassPartPivot(pivot1, "HTI", "AU", "3214.10.00 10", "");
			AssertCusClassPartPivot(pivot2, "HTI", "US", "4214100010", "");
			AssertCusClassPartPivot(pivot3, "HTE", "US", "5214100010", "");

			var assertCusAttributeFilter = new Action<int, DataRow>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAttributeFilter(i.ToString(), row, "AT1", "ATT P1", "");
						break;
				}
			});
			var pivot1Attribute = AssertCusAttributeFilters(pivot1, 1, assertCusAttributeFilter)[0];
			var pivot2Attribute = AssertCusAttributeFilters(pivot2, 1, assertCusAttributeFilter)[0];
			var pivot3Attribute = AssertCusAttributeFilters(pivot3, 1, assertCusAttributeFilter)[0];

			xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<PK>{pivot2.PK.ToString()}</PK><!--Should match using PK-->
						<TariffNum>4214100011</TariffNum>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<CusAttributeFilterCollection>
							<CusAttributeFilter Action=""MERGE""><!--should match using other fields-->
								<AttributeName>AT1</AttributeName>
								<AttributeValue1>ATT P1</AttributeValue1>
							</CusAttributeFilter>
						</CusAttributeFilterCollection>
					</CusClassPartPivot>
					<CusClassPartPivot Action=""MERGE""><!--should match using other fields-->
						<TariffNum>5214100010</TariffNum>
						<ChildType>HTE</ChildType>
						<Country TableName=""RefCountry"">
							<Code>US</Code>
						</Country>
						<CusAttributeFilterCollection>
							<CusAttributeFilter Action=""MERGE""><!--should not match as value1 is different-->
								<AttributeName>AT1</AttributeName>
								<AttributeValue1>ATT P2</AttributeValue1>
							</CusAttributeFilter>
							<CusAttributeFilter Action=""MERGE""><!--should create new-->
								<AttributeName>AT2</AttributeName>
								<AttributeValue1>ATT P2</AttributeValue1>
							</CusAttributeFilter>
						</CusAttributeFilterCollection>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusCodeData>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(3, pivots.Length);
			pivot1 = pivots.Single(x => x.PK == pivot1.PK);
			AssertCusClassPartPivot(pivot1, "HTI", "AU", "3214.10.00 10", "");
			var pivot1Attribute2 = AssertCusAttributeFilters(pivot1, 1, assertCusAttributeFilter)[0];
			AssertEquals(pivot1Attribute[CusAttributeFilterSchema.Constants.PK], pivot1Attribute2[CusAttributeFilterSchema.Constants.PK]);

			pivot2 = pivots.Single(x => x.PK == pivot2.PK);
			var pivot2Attribute2 = AssertCusAttributeFilters(pivot2, 1, assertCusAttributeFilter)[0];
			AssertEquals(pivot2Attribute[CusAttributeFilterSchema.Constants.PK], pivot2Attribute2[CusAttributeFilterSchema.Constants.PK]);

			pivot3 = pivots.Single(x => x.PK == pivot3.PK);
			assertCusAttributeFilter = (i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAttributeFilter(i.ToString(), row, "AT1", "ATT P2", "");
						break;
					case 1:
						AssertCusAttributeFilter(i.ToString(), row, "AT2", "ATT P2", "");
						break;
				}
			};
			AssertEquals("existing pivot3 attributes should have been deleted", false, newFactory.ExistsInDatabase(CusAttributeFilterSchema.Constants.TableName, new ZQuery(CusAttributeFilterSchema.PK, pivot3Attribute[CusAttributeFilterSchema.Constants.PK])));
			AssertCusAttributeFilters(pivot3, 2, assertCusAttributeFilter);
		}

		public void TestNonMatchedChildrenOfPivotAreDeleted_ComponetAndAdditionalInfo()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var xmlComponentCusClassPartPivot1 = CreateCusClassPartPivotXmlForFirstParse("ComponentCusClassPartPivot", "COM", "US", "2211", @"<AddInfoCollection>
	<AddInfo>
		<Key>GREETING</Key>
		<Value>HELLO</Value>
	</AddInfo>
	<AddInfo>
		<Key>KEY2</Key>
		<Value>ABC</Value>
	</AddInfo>
</AddInfoCollection>", 1);
			var xmlComponentCusClassPartPivot2 = CreateCusClassPartPivotXmlForFirstParse("ComponentCusClassPartPivot", "COM", "US", "2222", "<AddInfo>GREETING=HELLO*KEY2=ABC</AddInfo>", 2);
			var xmlPivot = CreateCusClassPartPivotXmlForFirstParse("CusClassPartPivot", "HTI", "US", "4214100010", @"<AddInfoCollection>
	<AddInfo>
		<Key>GREETING</Key>
		<Value>HELLO</Value>
	</AddInfo>
	<AddInfo>
		<Key>KEY2</Key>
		<Value>ABC</Value>
	</AddInfo>
</AddInfoCollection>", 0, $@"	<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
{xmlComponentCusClassPartPivot1}{xmlComponentCusClassPartPivot2}
	</ComponentCusClassPartPivotCollection>
");
			var xml = CreateProductXml(partNo, orgCode, xmlPivot);
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusCodeData>(), 1);
			var pivotQuery = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(pivotQuery);
			AssertEquals(1, pivots.Length);
			var pivot = pivots[0];
			AssertCusClassPartPivot(pivot, "HTI", "US", "4214100010", "GREETING=HELLO*KEY2=ABC");
			var data = AssertCusClassPartPivotFirstParse(newFactory, pivot.PK);
			var pivotCusCodeData1 = data.pivotCusCodeDatas[0];
			var pivotCusCodeData2 = data.pivotCusCodeDatas[1];
			var pivotCusUSClassification = data.pivotCusUSClassification;
			var pivotCusAddInfo1 = data.pivotCusAddInfos[0];
			var pivotCusAddInfo2 = data.pivotCusAddInfos[0];
			var pivotCusAddInfo1CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[0];
			var pivotCusAddInfo1CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[1];
			var pivotCusAddInfo2CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[2];
			var pivotCusAddInfo2CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[3];
			var pivotCusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[1];
			var pivotCusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[2];
			var pivotCusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[3];
			var pivotCusAddInfo1CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[1];
			var pivotCusAddInfo1CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[2];
			var pivotCusAddInfo1CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[3];
			var pivotCusAddInfo2CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[4];
			var pivotCusAddInfo2CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[5];
			var pivotCusAddInfo2CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[6];
			var pivotCusAddInfo2CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[7];

			var assertCusClassPartPivot = new Action<int, Enterprise.Integration.Customs.Shared.ICusClassPartPivot>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusClassPartPivot(row, "COM", "US", "2211", "GREETING=HELLO*KEY2=ABC");
						break;
					case 1:
						AssertCusClassPartPivot(row, "COM", "US", "2222", "GREETING=HELLO*KEY2=ABC");
						break;
				}
			});
			var pivotCusClassPartPivots = AssertCusClassPartPivots(pivot, 2, assertCusClassPartPivot);
			var pivotCusClassPartPivot1 = pivotCusClassPartPivots[0];
			var pivotCusClassPartPivot2 = pivotCusClassPartPivots[1];

			data = AssertCusClassPartPivotFirstParse(newFactory, pivotCusClassPartPivot1.PK);
			var pivotCusClassPartPivot1CusCodeData1 = data.pivotCusCodeDatas[0];
			var pivotCusClassPartPivot1CusCodeData2 = data.pivotCusCodeDatas[1];
			var pivotCusClassPartPivot1CusUSClassification = data.pivotCusUSClassification;
			var pivotCusClassPartPivot1CusAddInfo1 = data.pivotCusAddInfos[0];
			var pivotCusClassPartPivot1CusAddInfo2 = data.pivotCusAddInfos[0];
			var pivotCusClassPartPivot1CusAddInfo1CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[0];
			var pivotCusClassPartPivot1CusAddInfo1CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[1];
			var pivotCusClassPartPivot1CusAddInfo2CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[2];
			var pivotCusClassPartPivot1CusAddInfo2CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[3];
			var pivotCusClassPartPivot1CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[0];
			var pivotCusClassPartPivot1CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[1];
			var pivotCusClassPartPivot1CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[2];
			var pivotCusClassPartPivot1CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[3];
			var pivotCusClassPartPivot1CusAddInfo1CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[0];
			var pivotCusClassPartPivot1CusAddInfo1CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[1];
			var pivotCusClassPartPivot1CusAddInfo1CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[2];
			var pivotCusClassPartPivot1CusAddInfo1CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[3];
			var pivotCusClassPartPivot1CusAddInfo2CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[4];
			var pivotCusClassPartPivot1CusAddInfo2CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[5];
			var pivotCusClassPartPivot1CusAddInfo2CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[6];
			var pivotCusClassPartPivot1CusAddInfo2CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[7];

			data = AssertCusClassPartPivotFirstParse(newFactory, pivotCusClassPartPivot2.PK);
			var pivotCusClassPartPivot2CusCodeData1 = data.pivotCusCodeDatas[0];
			var pivotCusClassPartPivot2CusCodeData2 = data.pivotCusCodeDatas[1];
			var pivotCusClassPartPivot2CusUSClassification = data.pivotCusUSClassification;
			var pivotCusClassPartPivot2CusAddInfo1 = data.pivotCusAddInfos[0];
			var pivotCusClassPartPivot2CusAddInfo2 = data.pivotCusAddInfos[0];
			var pivotCusClassPartPivot2CusAddInfo1CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[0];
			var pivotCusClassPartPivot2CusAddInfo1CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[1];
			var pivotCusClassPartPivot2CusAddInfo2CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[2];
			var pivotCusClassPartPivot2CusAddInfo2CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[3];
			var pivotCusClassPartPivot2CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[0];
			var pivotCusClassPartPivot2CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[1];
			var pivotCusClassPartPivot2CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[2];
			var pivotCusClassPartPivot2CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[3];
			var pivotCusClassPartPivot2CusAddInfo1CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[0];
			var pivotCusClassPartPivot2CusAddInfo1CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[1];
			var pivotCusClassPartPivot2CusAddInfo1CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[2];
			var pivotCusClassPartPivot2CusAddInfo1CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[3];
			var pivotCusClassPartPivot2CusAddInfo2CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[4];
			var pivotCusClassPartPivot2CusAddInfo2CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[5];
			var pivotCusClassPartPivot2CusAddInfo2CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[6];
			var pivotCusClassPartPivot2CusAddInfo2CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[7];

			xmlComponentCusClassPartPivot2 = CreateCusClassPartPivotXmlForSecondParse("ComponentCusClassPartPivot", "COM", "US", "2222", "<AddInfo>KEY2=ABC*GREETING=HELLO</AddInfo>", 2, pivotCusClassPartPivot2CusCodeData1.PK, pivotCusClassPartPivot2CusAddInfo1CusCodeData1.PK);
			xmlPivot = CreateCusClassPartPivotXmlForSecondParse("CusClassPartPivot", "HTI", "US", "4214100020", "<AddInfo>GREETING=HELLO*KEY2=DEF</AddInfo>", 0, pivotCusCodeData1.PK, pivotCusAddInfo1CusCodeData1.PK, $@"	<PK>{pivot.PK}</PK>
	<ComponentCusClassPartPivotCollection TableName=""CusClassPartPivot"">
{xmlComponentCusClassPartPivot2}
	</ComponentCusClassPartPivotCollection>
");
			xml = CreateProductXml(partNo, orgCode, xmlPivot);
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			newFactory = new BusinessObjectFactory();
			enableUnknownInTypeDecider = newFactory.GetCachedValue("EnableUnknownInTypeDecider", () => new Dictionary<Type, int>());
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusAddInfo>(), 1);
			enableUnknownInTypeDecider.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusCodeData>(), 1);
			pivots = newFactory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(pivotQuery);
			AssertEquals(1, pivots.Length);
			var pivot2 = pivots[0];
			AssertEquals("Should have matched pivot using PK", pivot.PK, pivot2.PK);
			AssertCusClassPartPivot(pivot2, "HTI", "US", "4214100020", "GREETING=HELLO*KEY2=DEF");
			data = AssertCusClassPartPivotSecondParse(newFactory, pivot2.PK);
			var pivot2CusCodeData1 = data.pivotCusCodeDatas[0];
			AssertEquals("Should have matched CusCodeData using PK", pivotCusCodeData1.PK, pivot2CusCodeData1.PK);
			var pivot2CusCodeData2 = data.pivotCusCodeDatas[1];
			var pivot2CusUSClassification = data.pivotCusUSClassification;
			AssertEquals("Should have matched using ParentID and only one", pivotCusUSClassification.PK, pivot2CusUSClassification.PK);
			var pivot2CusAddInfo1 = data.pivotCusAddInfos[0];
			AssertEquals("Should have matched using ParentID and all fields", pivotCusAddInfo1.PK, pivot2CusAddInfo1.PK);
			var pivot2CusAddInfo1CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[0];
			AssertEquals("Should have matched CusCodeData using PK", pivotCusAddInfo1CusCodeData1.PK, pivot2CusAddInfo1CusCodeData1.PK);
			var pivot2CusAddInfo1CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[1];
			var pivot2CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[0];
			AssertEquals("Should have matched CusAddInfo using all fields", pivotCusAddInfo1CusAddInfo1.PK, pivot2CusAddInfo1CusAddInfo1.PK);
			var pivot2CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[1];
			var pivot2CusAddInfo1CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[0];
			var pivot2CusAddInfo1CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[1];
			AssertEquals("Should have matched CusAddInfo using all fields", pivotCusAddInfo1CusAddInfo1CusAddInfo2.PK, pivot2CusAddInfo1CusAddInfo1CusAddInfo2.PK);
			var pivot2CusAddInfo1CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[2];
			var pivot2CusAddInfo1CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[3];

			AssertDataWasDeleted(newFactory, (BusinessObject)pivotCusCodeData2,
				(BusinessObject)pivotCusAddInfo1CusCodeData2, (BusinessObject)pivotCusAddInfo2CusCodeData1, (BusinessObject)pivotCusAddInfo2CusCodeData2,
				(BusinessObject)pivotCusAddInfo1CusAddInfo2, (BusinessObject)pivotCusAddInfo2CusAddInfo1, (BusinessObject)pivotCusAddInfo2CusAddInfo2, (BusinessObject)pivotCusAddInfo1CusAddInfo2CusAddInfo1,
				(BusinessObject)pivotCusAddInfo1CusAddInfo2CusAddInfo2,
				(BusinessObject)pivotCusAddInfo2CusAddInfo1CusAddInfo1, (BusinessObject)pivotCusAddInfo2CusAddInfo1CusAddInfo2, (BusinessObject)pivotCusAddInfo2CusAddInfo2CusAddInfo1, (BusinessObject)pivotCusAddInfo2CusAddInfo2CusAddInfo2);

			assertCusClassPartPivot = new Action<int, Enterprise.Integration.Customs.Shared.ICusClassPartPivot>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusClassPartPivot(row, "COM", "US", "2222", "GREETING=HELLO*KEY2=ABC");
						break;
				}
			});

			AssertDataWasDeleted(newFactory, (BusinessObject)pivotCusClassPartPivot1,
				(BusinessObject)pivotCusClassPartPivot1CusCodeData1,
				(BusinessObject)pivotCusClassPartPivot1CusCodeData2,
				(BusinessObject)pivotCusClassPartPivot1CusUSClassification,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusCodeData1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusCodeData2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusCodeData1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusCodeData2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusAddInfo1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusAddInfo2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusAddInfo1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusAddInfo2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusAddInfo1CusAddInfo1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusAddInfo1CusAddInfo2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusAddInfo2CusAddInfo1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo1CusAddInfo2CusAddInfo2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusAddInfo1CusAddInfo1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusAddInfo1CusAddInfo2,
				(BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusAddInfo2CusAddInfo1, (BusinessObject)pivotCusClassPartPivot1CusAddInfo2CusAddInfo2CusAddInfo2);

			pivotCusClassPartPivots = AssertCusClassPartPivots(pivot2, 1, assertCusClassPartPivot);
			var pivot2CusClassPartPivot1 = pivotCusClassPartPivots[0];
			AssertEquals("Should have matched pivot using PK", pivotCusClassPartPivot2.PK, pivot2CusClassPartPivot1.PK);
			data = AssertCusClassPartPivotSecondParse(newFactory, pivot2CusClassPartPivot1.PK);
			var pivot2CusClassPartPivot1CusCodeData1 = data.pivotCusCodeDatas[0];
			AssertEquals("Should have matched CusCodeData using PK", pivotCusClassPartPivot2CusCodeData1.PK, pivot2CusClassPartPivot1CusCodeData1.PK);
			var pivot2CusClassPartPivot1CusCodeData2 = data.pivotCusCodeDatas[1];
			var pivot2CusClassPartPivot1CusUSClassification = data.pivotCusUSClassification;
			AssertEquals("Should have matched using ParentID and only one", pivotCusClassPartPivot2CusUSClassification.PK, pivot2CusClassPartPivot1CusUSClassification.PK);
			var pivot2CusClassPartPivot1CusAddInfo1 = data.pivotCusAddInfos[0];
			AssertEquals("Should have matched using ParentID and all fields", pivotCusClassPartPivot2CusAddInfo1.PK, pivot2CusClassPartPivot1CusAddInfo1.PK);
			var pivot2CusClassPartPivot1CusAddInfo1CusCodeData1 = data.pivotCusAddInfoCusCodeDatas[0];
			AssertEquals("Should have matched CusCodeData using PK", pivotCusClassPartPivot2CusAddInfo1CusCodeData1.PK, pivot2CusClassPartPivot1CusAddInfo1CusCodeData1.PK);
			var pivot2CusClassPartPivot1CusAddInfo1CusCodeData2 = data.pivotCusAddInfoCusCodeDatas[1];
			var pivot2CusClassPartPivot1CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfos[0];
			AssertEquals("Should have matched CusAddInfo using all fields", pivotCusClassPartPivot2CusAddInfo1CusAddInfo1.PK, pivot2CusClassPartPivot1CusAddInfo1CusAddInfo1.PK);
			var pivot2CusClassPartPivot1CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfos[1];
			var pivot2CusClassPartPivot1CusAddInfo1CusAddInfo1CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[0];
			var pivot2CusClassPartPivot1CusAddInfo1CusAddInfo1CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[1];
			AssertEquals("Should have matched CusAddInfo using all fields ", pivotCusClassPartPivot2CusAddInfo1CusAddInfo1CusAddInfo2.PK, pivot2CusClassPartPivot1CusAddInfo1CusAddInfo1CusAddInfo2.PK);
			var pivot2CusClassPartPivot1CusAddInfo1CusAddInfo2CusAddInfo1 = data.pivotCusAddInfoCusAddInfoCusAddInfos[2];
			var pivot2CusClassPartPivot1CusAddInfo1CusAddInfo2CusAddInfo2 = data.pivotCusAddInfoCusAddInfoCusAddInfos[3];

			AssertDataWasDeleted(newFactory, (BusinessObject)pivotCusClassPartPivot2CusCodeData2,
				(BusinessObject)pivotCusClassPartPivot2CusAddInfo1CusCodeData2, (BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusCodeData1, (BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusCodeData2,
				(BusinessObject)pivotCusClassPartPivot2CusAddInfo1CusAddInfo2, (BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusAddInfo1, (BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusAddInfo2, (BusinessObject)pivotCusClassPartPivot2CusAddInfo1CusAddInfo2CusAddInfo1,
				(BusinessObject)pivotCusClassPartPivot2CusAddInfo1CusAddInfo2CusAddInfo2,
				(BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusAddInfo1CusAddInfo1, (BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusAddInfo1CusAddInfo2, (BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusAddInfo2CusAddInfo1, (BusinessObject)pivotCusClassPartPivot2CusAddInfo2CusAddInfo2CusAddInfo2);

			Assert(ErrorReporter.HasBeenReported("Enterprise.Customs.US.Business.PGA does not support CY_Type 'AFM'"));
			ErrorReporter.Clear();
		}

		string CreateCusClassPartPivotXml(string elementName, string childType, string country, string tariffNum, string addInfo, int sequence, string additionalXml)
		{
			return $@"<{elementName} Action=""MERGE"">
	<TariffNum>{tariffNum}</TariffNum>
	<ChildType>{childType}</ChildType>
	<ChildListOrder>{sequence}</ChildListOrder>
	<Country TableName=""RefCountry"">
		<Code>{country}</Code>
	</Country>
	{addInfo}
{additionalXml}</{elementName}>
";
		}

		string CreateCusClassPartPivotXmlForFirstParse(string elementName, string childType, string country, string tariffNum, string addInfo, int sequence, string additionalXml = "")
		{
			return CreateCusClassPartPivotXml(elementName, childType, country, tariffNum, addInfo, sequence, $@"	<AdditionalInformationChildCollection TableName=""CusAddInfo"">
		<AdditionalInformationChild Action=""MERGE"">
			<Type>PGA</Type>
			<AddInfoCollection>
				<AddInfo>
					<Key>Key9</Key>
					<Value>ValueB</Value>
				</AddInfo>
				<AddInfo>
					<Key>Key10</Key>
					<Value>ValueA</Value>
				</AddInfo>
			</AddInfoCollection>
			<AdditionalInformationGrandChildCollection TableName=""CusAddInfo"">
				<AdditionalInformationGrandChild Action=""MERGE"">
					<Type>USP</Type>
					<AddInfoData>Key5=ValueB*Key6=ValueA</AddInfoData>
					<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>SCI</Type>
							<AddInfoData>Key1=ValueB*Key2=ValueA</AddInfoData>
						</AdditionalInformationGreatGrandChild>
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>RQD</Type>
							<AddInfoCollection>
								<AddInfo>
									<Key>Key1</Key>
									<Value>ValueA</Value>
								</AddInfo>
								<AddInfo>
									<Key>Key2</Key>
									<Value>ValueB</Value>
								</AddInfo>
							</AddInfoCollection>
						</AdditionalInformationGreatGrandChild>
					</AdditionalInformationGreatGrandChildCollection>
				</AdditionalInformationGrandChild>
				<AdditionalInformationGrandChild Action=""MERGE"">
					<Type>CCP</Type>
					<AddInfoData>Key7=ValueB*Key8=ValueA</AddInfoData>
					<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>SCI</Type>
							<AddInfoData>Key1=ValueB*Key2=ValueA</AddInfoData>
						</AdditionalInformationGreatGrandChild>
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>RQD</Type>
							<AddInfoData>Key1=ValueA*Key2=ValueB</AddInfoData>
						</AdditionalInformationGreatGrandChild>
					</AdditionalInformationGreatGrandChildCollection>
				</AdditionalInformationGrandChild>
			</AdditionalInformationGrandChildCollection>
			<CusCodeDataFdaAffirmationCollection TableName=""CusCodeData"">
				<CusCodeDataFdaAffirmation Action=""MERGE"">
					<Type>AFM</Type>
					<Data>AFF C2</Data>
				</CusCodeDataFdaAffirmation>
				<CusCodeDataFdaAffirmation Action=""MERGE"">
					<Type>AFM</Type>
					<Data>AFF C1</Data>
				</CusCodeDataFdaAffirmation>
			</CusCodeDataFdaAffirmationCollection>
		</AdditionalInformationChild>
		<AdditionalInformationChild Action=""MERGE"">
			<Type>AML</Type>
			<AddInfoData>Key3=ValueB*Key4=ValueA</AddInfoData>
			<AdditionalInformationGrandChildCollection TableName=""CusAddInfo"">
				<AdditionalInformationGrandChild Action=""MERGE"">
					<Type>USP</Type>
					<AddInfoData>Key5=ValueB*Key6=ValueA</AddInfoData>
					<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>SCI</Type>
							<AddInfoCollection>
								<AddInfo>
									<Key>Key1</Key>
									<Value>ValueB</Value>
								</AddInfo>
								<AddInfo>
									<Key>Key2</Key>
									<Value>ValueA</Value>
								</AddInfo>
							</AddInfoCollection>
						</AdditionalInformationGreatGrandChild>
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>RQD</Type>
							<AddInfoData>Key1=ValueA*Key2=ValueB</AddInfoData>
						</AdditionalInformationGreatGrandChild>
					</AdditionalInformationGreatGrandChildCollection>
				</AdditionalInformationGrandChild>
				<AdditionalInformationGrandChild Action=""MERGE"">
					<Type>CCP</Type>
					<AddInfoData>Key7=ValueB*Key8=ValueA</AddInfoData>
					<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>SCI</Type>
							<AddInfoData>Key1=ValueB*Key2=ValueA</AddInfoData>
						</AdditionalInformationGreatGrandChild>
						<AdditionalInformationGreatGrandChild Action=""MERGE"">
							<Type>RQD</Type>
							<AddInfoData>Key1=ValueA*Key2=ValueB</AddInfoData>
						</AdditionalInformationGreatGrandChild>
					</AdditionalInformationGreatGrandChildCollection>
				</AdditionalInformationGrandChild>
			</AdditionalInformationGrandChildCollection>
			<CusCodeDataFdaAffirmationCollection TableName=""CusCodeData"">
				<CusCodeDataFdaAffirmation Action=""MERGE"">
					<Type>AFM</Type>
					<Data>AFF C2</Data>
				</CusCodeDataFdaAffirmation>
				<CusCodeDataFdaAffirmation Action=""MERGE"">
					<Type>AFM</Type>
					<Data>AFF C1</Data>
				</CusCodeDataFdaAffirmation>
			</CusCodeDataFdaAffirmationCollection>
		</AdditionalInformationChild>
	</AdditionalInformationChildCollection>
	<CusUSClassificationCollection>
		<CusUSClassification Action=""MERGE"">
			<SPI>P</SPI>
		</CusUSClassification>
	</CusUSClassificationCollection>
	<CusCodeDataCensusCollection TableName=""CusCodeData"">
		<CusCodeDataCensus Action=""MERGE"">
			<Type>CWO</Type>
			<Data>C1</Data>
		</CusCodeDataCensus>
		<CusCodeDataCensus Action=""MERGE"">
			<Type>CWO</Type>
			<Data>C2</Data>
		</CusCodeDataCensus>
	</CusCodeDataCensusCollection>
{additionalXml}");
		}

		string CreateCusClassPartPivotXmlForSecondParse(string elementName, string childType, string country, string tariffNum, string addInfo, int sequence, ZGuid cusCodeData2PK, ZGuid cusAddInfo2CusCodeData1PK, string additionalXml = "")
		{
			return CreateCusClassPartPivotXml(elementName, childType, country, tariffNum, addInfo, sequence, $@"	<AdditionalInformationChildCollection TableName=""CusAddInfo"">
		<AdditionalInformationChild Action=""MERGE""><!--should match using fiels - addinfo has key in different order-->
			<Type>AML</Type>
			<AddInfoData>Key4=ValueA*Key3=ValueB</AddInfoData>
			<AdditionalInformationGrandChildCollection TableName=""CusAddInfo"">
				<AdditionalInformationGrandChild Action=""MERGE""><!--should create new-->
					<Type>USA</Type>
					<AddInfoData>Key5=ValueB*Key6=ValueA</AddInfoData>
					<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
						<AdditionalInformationGreatGrandChild Action=""MERGE""><!--should create new-->
							<Type>SCI</Type>
							<AddInfoData>Key1=ValueB*Key2=ValueA</AddInfoData>
						</AdditionalInformationGreatGrandChild>
						<AdditionalInformationGreatGrandChild Action=""MERGE""><!--should create new-->
							<Type>RQD</Type>
							<AddInfoData>Key1=ValueA*Key2=ValueB</AddInfoData>
						</AdditionalInformationGreatGrandChild>
					</AdditionalInformationGreatGrandChildCollection>
				</AdditionalInformationGrandChild>
				<AdditionalInformationGrandChild Action=""MERGE""><!--should match fields-->
					<Type>CCP</Type>
					<AddInfoData>Key7=ValueB*Key8=ValueA</AddInfoData>
					<AdditionalInformationGreatGrandChildCollection TableName=""CusAddInfo"">
						<AdditionalInformationGreatGrandChild Action=""MERGE""><!--should create new-->
							<Type>SCI</Type>
							<AddInfoData>Key1=ValueB*Key2=ValueA</AddInfoData>
						</AdditionalInformationGreatGrandChild>
						<AdditionalInformationGreatGrandChild Action=""MERGE""><!--should match using fields - AddInfo has data in different order-->
							<Type>RQD</Type>
							<AddInfoData>Key2=ValueB*Key1=ValueA</AddInfoData>
						</AdditionalInformationGreatGrandChild>
					</AdditionalInformationGreatGrandChildCollection>
				</AdditionalInformationGrandChild>
			</AdditionalInformationGrandChildCollection>
			<CusCodeDataFdaAffirmationCollection TableName=""CusCodeData"">
				<CusCodeDataFdaAffirmation Action=""MERGE""><!--should match using pk-->
					<PK>{cusAddInfo2CusCodeData1PK}</PK>
					<Type>AFM</Type>
					<Data>AFF C1</Data>
				</CusCodeDataFdaAffirmation>
				<CusCodeDataFdaAffirmation Action=""MERGE""><!--should create new-->
					<Type>AFM</Type>
					<Data>AFF C3</Data>
				</CusCodeDataFdaAffirmation>
			</CusCodeDataFdaAffirmationCollection>
		</AdditionalInformationChild>
	</AdditionalInformationChildCollection>
	<CusUSClassificationCollection>
		<CusUSClassification Action=""MERGE""><!--should match using parent reference-->
			<SPI>S</SPI>
		</CusUSClassification>
	</CusUSClassificationCollection>
	<CusCodeDataCensusCollection TableName=""CusCodeData"">
		<CusCodeDataCensus Action=""MERGE""><!--should create new-->
			<Type>CWO</Type>
			<Data>C3</Data>
		</CusCodeDataCensus>
		<CusCodeDataCensus Action=""MERGE""><!--should match using pk-->
			<PK>{cusCodeData2PK}</PK>
			<Type>CWO</Type>
			<Data>C2</Data>
		</CusCodeDataCensus>
	</CusCodeDataCensusCollection>
{additionalXml}");
		}

		(Enterprise.Integration.Customs.ICusCodeData[] pivotCusCodeDatas,
			Enterprise.Integration.Customs.US.ICusUSClassification pivotCusUSClassification,
			Enterprise.Integration.Customs.ICusAddInfo[] pivotCusAddInfos,
			Enterprise.Integration.Customs.ICusCodeData[] pivotCusAddInfoCusCodeDatas,
			Enterprise.Integration.Customs.ICusAddInfo[] pivotCusAddInfoCusAddInfos,
			Enterprise.Integration.Customs.ICusAddInfo[] pivotCusAddInfoCusAddInfoCusAddInfos) AssertCusClassPartPivotFirstParse(BusinessObjectFactory factory, ZGuid pivotPK)
		{
			var assertCusCodeData = new Action<int, Enterprise.Integration.Customs.ICusCodeData>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusCodeData(i.ToString(), row, "CWO", "C1");
						break;
					case 1:
						AssertCusCodeData(i.ToString(), row, "CWO", "C2");
						break;
				}
			});
			var pivotCusCodeDatas = AssertCusCodeDatas(factory, CusClassPartPivotSchema.Constants.Prefix, pivotPK, 2, assertCusCodeData);
			var pivotCusCodeData1 = pivotCusCodeDatas[0];
			var pivotCusCodeData2 = pivotCusCodeDatas[1];

			var assertCusUSClassification = new Action<int, Enterprise.Integration.Customs.US.ICusUSClassification>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusUSClassification(i.ToString(), row, "P");
						break;
				}
			});
			var pivotCusUSClassification = AssertCusUSClassifications(factory, CusClassPartPivotSchema.Constants.Prefix, pivotPK, 1, assertCusUSClassification)[0];

			var assertCusAddInfo = new Action<int, Enterprise.Integration.Customs.ICusAddInfo>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAddInfo(i.ToString(), row, "AML", "Key3=ValueB*Key4=ValueA");
						break;
					case 1:
						AssertCusAddInfo(i.ToString(), row, "PGA", "Key10=ValueA*Key9=ValueB");
						break;
				}
			});
			var cusAddInfos = AssertCusAddInfos(factory, CusClassPartPivotSchema.Constants.Prefix, pivotPK, 2, assertCusAddInfo);
			var pivotCusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo2 = cusAddInfos[1];

			assertCusCodeData = new Action<int, Enterprise.Integration.Customs.ICusCodeData>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusCodeData(i.ToString(), row, "AFM", "AFF C1");
						break;
					case 1:
						AssertCusCodeData(i.ToString(), row, "AFM", "AFF C2");
						break;
				}
			});
			var pivotCusAddInfoCusCodeDatas = AssertCusCodeDatas(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1.PK, 2, assertCusCodeData);
			var pivotCusAddInfo1CusCodeData1 = pivotCusAddInfoCusCodeDatas[0];
			var pivotCusAddInfo1CusCodeData2 = pivotCusAddInfoCusCodeDatas[1];

			pivotCusAddInfoCusCodeDatas = AssertCusCodeDatas(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo2.PK, 2, assertCusCodeData);
			var pivotCusAddInfo2CusCodeData1 = pivotCusAddInfoCusCodeDatas[0];
			var pivotCusAddInfo2CusCodeData2 = pivotCusAddInfoCusCodeDatas[1];

			assertCusAddInfo = new Action<int, Enterprise.Integration.Customs.ICusAddInfo>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAddInfo(i.ToString(), row, "CCP", "Key7=ValueB*Key8=ValueA");
						break;
					case 1:
						AssertCusAddInfo(i.ToString(), row, "USP", "Key5=ValueB*Key6=ValueA");
						break;
				}
			});
			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1.PK, 2, assertCusAddInfo);
			var pivotCusAddInfo1CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo2 = cusAddInfos[1];

			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo2.PK, 2, assertCusAddInfo);
			var pivotCusAddInfo2CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo2CusAddInfo2 = cusAddInfos[1];

			assertCusAddInfo = new Action<int, Enterprise.Integration.Customs.ICusAddInfo>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAddInfo(i.ToString(), row, "RQD", "Key1=ValueA*Key2=ValueB");
						break;
					case 1:
						AssertCusAddInfo(i.ToString(), row, "SCI", "Key1=ValueB*Key2=ValueA");
						break;
				}
			});
			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1CusAddInfo1.PK, 2, assertCusAddInfo);
			var pivotCusAddInfo1CusAddInfo1CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo1CusAddInfo2 = cusAddInfos[1];

			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1CusAddInfo2.PK, 2, assertCusAddInfo);
			var pivotCusAddInfo1CusAddInfo2CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo2CusAddInfo2 = cusAddInfos[1];

			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo2CusAddInfo1.PK, 2, assertCusAddInfo);
			var pivotCusAddInfo2CusAddInfo1CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo2CusAddInfo1CusAddInfo2 = cusAddInfos[1];

			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo2CusAddInfo2.PK, 2, assertCusAddInfo);
			var pivotCusAddInfo2CusAddInfo2CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo2CusAddInfo2CusAddInfo2 = cusAddInfos[1];
			return (new[] { pivotCusCodeData1, pivotCusCodeData2 }, pivotCusUSClassification,
				new[] { pivotCusAddInfo1, pivotCusAddInfo2 },
				new[]
				{
					pivotCusAddInfo1CusCodeData1, pivotCusAddInfo1CusCodeData2,
					pivotCusAddInfo2CusCodeData1, pivotCusAddInfo2CusCodeData2
				},
				new[]
				{
					pivotCusAddInfo1CusAddInfo1, pivotCusAddInfo1CusAddInfo2,
					pivotCusAddInfo2CusAddInfo1, pivotCusAddInfo2CusAddInfo2
				},
				new[]
				{
					pivotCusAddInfo1CusAddInfo1CusAddInfo1, pivotCusAddInfo1CusAddInfo1CusAddInfo2,
					pivotCusAddInfo1CusAddInfo2CusAddInfo1, pivotCusAddInfo1CusAddInfo2CusAddInfo2,
					pivotCusAddInfo2CusAddInfo1CusAddInfo1, pivotCusAddInfo2CusAddInfo1CusAddInfo2,
					pivotCusAddInfo2CusAddInfo2CusAddInfo1, pivotCusAddInfo2CusAddInfo2CusAddInfo2
				});
		}

		(Enterprise.Integration.Customs.ICusCodeData[] pivotCusCodeDatas,
			Enterprise.Integration.Customs.US.ICusUSClassification pivotCusUSClassification,
			Enterprise.Integration.Customs.ICusAddInfo[] pivotCusAddInfos,
			Enterprise.Integration.Customs.ICusCodeData[] pivotCusAddInfoCusCodeDatas,
			Enterprise.Integration.Customs.ICusAddInfo[] pivotCusAddInfoCusAddInfos,
			Enterprise.Integration.Customs.ICusAddInfo[] pivotCusAddInfoCusAddInfoCusAddInfos) AssertCusClassPartPivotSecondParse(BusinessObjectFactory factory, ZGuid pivotPK)
		{
			var assertCusCodeData = new Action<int, Enterprise.Integration.Customs.ICusCodeData>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusCodeData(i.ToString(), row, "CWO", "C2");
						break;
					case 1:
						AssertCusCodeData(i.ToString(), row, "CWO", "C3");
						break;
				}
			});
			var pivotCusCodeDatas = AssertCusCodeDatas(factory, CusClassPartPivotSchema.Constants.Prefix, pivotPK, 2, assertCusCodeData);
			var pivotCusCodeData1 = pivotCusCodeDatas[0];
			var pivotCusCodeData2 = pivotCusCodeDatas[1];

			var assertCusUSClassification = new Action<int, Enterprise.Integration.Customs.US.ICusUSClassification>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusUSClassification(i.ToString(), row, "S");
						break;
				}
			});
			var pivotCusUSClassification = AssertCusUSClassifications(factory, CusClassPartPivotSchema.Constants.Prefix, pivotPK, 1, assertCusUSClassification)[0];

			var assertCusAddInfo = new Action<int, Enterprise.Integration.Customs.ICusAddInfo>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAddInfo(i.ToString(), row, "AML", "Key3=ValueB*Key4=ValueA");
						break;
				}
			});
			var cusAddInfos = AssertCusAddInfos(factory, CusClassPartPivotSchema.Constants.Prefix, pivotPK, 1, assertCusAddInfo);
			var pivotCusAddInfo1 = cusAddInfos[0];

			var assertPivotCusAddInfoCusCodeData = new Action<int, Enterprise.Integration.Customs.ICusCodeData>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusCodeData(i.ToString(), row, "AFM", "AFF C1");
						break;
					case 1:
						AssertCusCodeData(i.ToString(), row, "AFM", "AFF C3");
						break;
				}
			});
			var pivotCusAddInfoCusCodeDatas = AssertCusCodeDatas(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1.PK, 2, assertPivotCusAddInfoCusCodeData);
			var pivotCusAddInfo1CusCodeData1 = pivotCusAddInfoCusCodeDatas[0];
			var pivotCusAddInfo1CusCodeData2 = pivotCusAddInfoCusCodeDatas[1];

			var assertPivotCusAddInfoCusAddInfo = new Action<int, Enterprise.Integration.Customs.ICusAddInfo>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAddInfo(i.ToString(), row, "CCP", "Key7=ValueB*Key8=ValueA");
						break;
					case 1:
						AssertCusAddInfo(i.ToString(), row, "USA", "Key5=ValueB*Key6=ValueA");
						break;
				}
			});
			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1.PK, 2, assertPivotCusAddInfoCusAddInfo);
			var pivotCusAddInfo1CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo2 = cusAddInfos[1];

			var assertPivotCusAddInfoCusAddInfoCusAddInfo = new Action<int, Enterprise.Integration.Customs.ICusAddInfo>((i, row) =>
			{
				switch (i)
				{
					case 0:
						AssertCusAddInfo(i.ToString(), row, "RQD", "Key1=ValueA*Key2=ValueB");
						break;
					case 1:
						AssertCusAddInfo(i.ToString(), row, "SCI", "Key1=ValueB*Key2=ValueA");
						break;
				}
			});
			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1CusAddInfo1.PK, 2, assertPivotCusAddInfoCusAddInfoCusAddInfo);
			var pivotCusAddInfo1CusAddInfo1CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo1CusAddInfo2 = cusAddInfos[1];

			cusAddInfos = AssertCusAddInfos(factory, CusAddInfoSchema.Constants.Prefix, pivotCusAddInfo1CusAddInfo2.PK, 2, assertPivotCusAddInfoCusAddInfoCusAddInfo);
			var pivotCusAddInfo1CusAddInfo2CusAddInfo1 = cusAddInfos[0];
			var pivotCusAddInfo1CusAddInfo2CusAddInfo2 = cusAddInfos[1];
			return (new[] { pivotCusCodeData1, pivotCusCodeData2 }, pivotCusUSClassification,
				new[] { pivotCusAddInfo1 },
				new[]
				{
					pivotCusAddInfo1CusCodeData1, pivotCusAddInfo1CusCodeData2
				},
				new[]
				{
					pivotCusAddInfo1CusAddInfo1, pivotCusAddInfo1CusAddInfo2
				},
				new[]
				{
					pivotCusAddInfo1CusAddInfo1CusAddInfo1, pivotCusAddInfo1CusAddInfo1CusAddInfo2,
					pivotCusAddInfo1CusAddInfo2CusAddInfo1, pivotCusAddInfo1CusAddInfo2CusAddInfo2
				});
		}

		void AssertDataWasDeleted(BusinessObjectFactory factory, params BusinessObject[] bizObjs)
		{
			CombineAssertions(() =>
			{
				var count = 0;
				foreach (var bizObj in bizObjs)
				{
					AssertNull(count++ + bizObj.ToString(), factory.Load(bizObj.TablePrefix, bizObj.PK));
				}
			});
		}

		Enterprise.Integration.Customs.ICusAddInfo[] AssertCusAddInfos(BusinessObjectFactory factory, ZString parentTableCode, ZGuid parentID, int count, Action<int, Enterprise.Integration.Customs.ICusAddInfo> assertCusAddInfo)
		{
			var query = new ZQuery(CusAddInfoSchema.B7_ParentID, parentID);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, parentTableCode);
			return AssertDatas(count, factory.Load<Enterprise.Integration.Customs.ICusAddInfo>(query).OrderBy(x => x.B7_Type).ToArray(), assertCusAddInfo);
		}

		void AssertCusAddInfo(string identifier, Enterprise.Integration.Customs.ICusAddInfo cusAddInfo, string type, string addInfo, string nAddInfo = null)
		{
			AssertEquals($"{identifier}-B7_Type", type, cusAddInfo.B7_Type);
			AssertEquals($"{identifier}-B7_AddInfoData", addInfo, cusAddInfo.B7_AddInfoData);
			if (nAddInfo != null)
			{
				AssertEquals($"{identifier}-B7_NAddInfoData", nAddInfo, cusAddInfo.B7_NAddInfoData);
			}
		}

		Enterprise.Integration.Customs.US.ICusUSClassification[] AssertCusUSClassifications(BusinessObjectFactory factory, ZString parentTableCode, ZGuid parentID, int count, Action<int, Enterprise.Integration.Customs.US.ICusUSClassification> assertCusUSClassification)
		{
			var query = new ZQuery(CusUSClassificationSchema.CD_ParentID, parentID);
			query.AddToFilter(CusUSClassificationSchema.CD_ParentTableCode, parentTableCode);
			return AssertDatas(count, factory.Load<Enterprise.Integration.Customs.US.ICusUSClassification>(query).OrderBy(x => x.CD_SPI).ToArray(), assertCusUSClassification);
		}

		void AssertCusUSClassification(string identifier, Enterprise.Integration.Customs.US.ICusUSClassification classification, string spi)
		{
			AssertEquals($"{identifier}-CD_SPI", spi, classification.CD_SPI);
		}

		Enterprise.Integration.Customs.ICusCodeData[] AssertCusCodeDatas(BusinessObjectFactory factory, ZString parentTableCode, ZGuid parentID, int count, Action<int, Enterprise.Integration.Customs.ICusCodeData> assertCusCodeData)
		{
			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, parentID);
			query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, parentTableCode);
			return AssertDatas(count, factory.Load<Enterprise.Integration.Customs.ICusCodeData>(query).OrderBy(x => x.CY_Type).ThenBy(x => x.CY_Data).ToArray(), assertCusCodeData);
		}

		void AssertCusCodeData(string identifier, Enterprise.Integration.Customs.ICusCodeData cusCodeData, string type, string data)
		{
			AssertEquals($"{identifier}-CY_Type", type, cusCodeData.CY_Type);
			AssertEquals($"{identifier}-CY_Data", data, cusCodeData.CY_Data);
		}

		Enterprise.Integration.Customs.Shared.ICusClassPartPivot[] AssertCusClassPartPivots(Enterprise.Integration.Customs.Shared.ICusClassPartPivot pivot, int count, Action<int, Enterprise.Integration.Customs.Shared.ICusClassPartPivot> assertCusClassPartPivot)
		{
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, pivot.CI_OP);
			query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, pivot.PK);
			var factory = (BusinessObjectFactory)(IBusinessObjectFactoryInternals)((BusinessObject)pivot).Factory;
			return AssertDatas(count, factory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query).OrderBy(x => x.CI_RN_NKCountry).ThenBy(x => x.CI_ChildType).ThenBy(x => x.CI_TariffNum).ToArray(), assertCusClassPartPivot);
		}

		void AssertCusClassPartPivot(Enterprise.Integration.Customs.Shared.ICusClassPartPivot pivot, string childType, string countryCode, string tariffNum, string addInfo, string nAddInfo = null)
		{
			AssertEquals("CI_ChildType", childType, pivot.CI_ChildType);
			AssertEquals("CI_RN_NKCountry", countryCode, pivot.CI_RN_NKCountry);
			AssertEquals("CI_TariffNum", tariffNum, pivot.CI_TariffNum);
			AssertEquals("CI_AddInfo", addInfo, pivot.CI_AddInfo);
			if (nAddInfo != null)
			{
				AssertEquals("CI_NAddInfo", nAddInfo, pivot.CI_NAddInfo);
			}
		}

		void AssertCusAttributeFilter(string identifier, DataRow row, string attributeName, string attributeValue1, string attributeValue2)
		{
			AssertEquals($"{identifier}-AttributeName", attributeName, row[CusAttributeFilterSchema.Constants.BG_AttributeName]);
			AssertEquals($"{identifier}-AttributeValue1", attributeValue1, row[CusAttributeFilterSchema.Constants.BG_AttributeValue1]);
			AssertEquals($"{identifier}-AttributeValue2", attributeValue2, row[CusAttributeFilterSchema.Constants.BG_AttributeValue2]);
		}

		DataRow[] AssertCusAttributeFilters(Enterprise.Integration.Customs.Shared.ICusClassPartPivot pivot, int count, Action<int, DataRow> assertCusAttributeFilter)
		{
			var rowFactory = ((IBusinessObjectFactoryInternals)((BusinessObject)pivot).Factory).RowFactory;
			return AssertDatas(count, rowFactory.Load(CusAttributeFilterSchema.Constants.TableName, new ZQuery(CusAttributeFilterSchema.BG_CI, pivot.PK)).OrderBy(x => x[CusAttributeFilterSchema.Constants.BG_AttributeName]).ToArray(), assertCusAttributeFilter);
		}

		T[] AssertDatas<T>(int count, T[] actualDatas, Action<int, T> assertData)
		{
			AssertEquals(count, actualDatas.Length);
			for (int i = 0; i < count; i++)
			{
				assertData(i, actualDatas[i]);
			}
			return actualDatas;
		}

		public void TestImportProductWithoutPst()
		{
			var partNo = "99115813";
			var orgCode = "USAMERPHL";

			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = orgCode;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNo;

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.US.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.UnitedStates;
			pivot[CusClassPartPivotSchema.CI_TariffNum] = "3214.10.00 10";
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";

			BusinessObjectCollection pstLines = (BusinessObjectCollection)pivot["PSTLines"];
			var pesticide = pstLines.AddNew();
			pesticide["US_ProductType"] = "PS1";
			pesticide["US_BrandName"] = "BIOBAN 045";

			BusinessObjectCollection pesticideLines = (BusinessObjectCollection)pesticide["PesticideLines"];
			var line = pesticideLines.AddNew();
			line["US_NameOfActiveIngredient"] = "2-N-OCRYL";
			line["US_ActiveIngredientPercentage"] = 45.0M;
			line["US_LPCOType"] = "CAS";
			line["US_LPCONumber"] = "26530-20-1";

			Factory.Save();

			var xml = CreateProductXml(partNo, orgCode, FormattableString.Invariant($@"
					<CusClassPartPivot Action=""MERGE"">
						<TariffNum>3214.10.00 10</TariffNum>
						<ChildType>HTI</ChildType>
						<Country TableName=""RefCountry"">
							<Code>AU</Code>
						</Country>
					</CusClassPartPivot>"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			var newFactory = new BusinessObjectFactory();
			var pivotAfterImport = (BusinessObject)newFactory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(pivot.PK);
			var pstLinesAfterImport = (BusinessObjectCollection)pivotAfterImport["PSTLines"];
			var pesticideLinesAfterImport = (BusinessObjectCollection)pesticide["PesticideLines"];

			AssertEquals("PST Line should remain after import.", 1, pstLinesAfterImport.Count);
			AssertEquals("Pesticide Line should remain after import.", 1, pesticideLinesAfterImport.Count);
		}

		string CreateProductXml(string partNo, string orgCode, string pivotXml)
		{
			var xml = FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Header>
		<OwnerCode>GWUS2HO</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Product Version=""2.0"">
			<OrgSupplierPart Action=""MERGE"">
				<PartNum>{partNo}</PartNum>
				<StockKeepingUnit>UNT</StockKeepingUnit>
				<IsActive>true</IsActive>
				<Desc>BMATE 1060N 20KG1A2 6M</Desc>
				<CusClassPartPivotCollection>{pivotXml}
				</CusClassPartPivotCollection>
				<OrgPartRelationCollection>
					<OrgPartRelation Action=""MERGE"">
						<Relationship>OWN</Relationship>
						<OrgHeader>
							<Code>{orgCode}</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
			</OrgSupplierPart>
		</Product>
	</Body>
</Native>
");
			return xml;
		}

		public void TestScenario22EnsureBlankNodesAreInjectedForMissingFieldsToEnsureRobustLookupsOfPivots()
		{
			RunXml("22. Three pivots maybe ambiguous if not considering empty fields.xml");
			var logs = GetLogs();
			AssertContains("CusClassPartPivot - 3 inserts, 0 updates, 0 deletes", logs);
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var orgRelation = factory.LoadTop1<OrgPartRelation>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK)).OrderBy(x => x.CI_TariffNum).ToArray();
			AssertEquals(3, pivots.Length);
			AssertEquals("111", pivots[0].CI_TariffNum);
			AssertEquals("222", pivots[1].CI_TariffNum);
			AssertEquals("3333", pivots[2].CI_TariffNum);

			ResetLogs();
			RunXml("22. Three pivots maybe ambiguous if not considering empty fields.xml");
			logs = GetLogs();
			AssertContains("CusClassPartPivot - 0 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario23MakeSureToConsiderCountryCodeWhenLookingForExistingPivots()
		{
			RunXml("23. Multiple countries.xml");
			var expectedLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";
			var logs = GetLogs();
			AssertContains(expectedLogs, logs);

			ResetLogs();
			RunXml("23. Multiple countries.xml");
			expectedLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes";
			logs = GetLogs();
			AssertContains(expectedLogs, logs);
		}

		public void TestScenario24InjectMissingOrgPartRelationOnPivot()
		{
			var xml = GetXmlToImport("24a. Inject missing OrgPartRelation.xml").ReadToEnd();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("CusClassPartPivot - 1 inserts, 0 updates, 0 deletes", logs);
			var startSnip = xml.IndexOf("STARTSNIP");
			var endSnip = xml.IndexOf("ENDSNIP");
			var xmlWithoutOU = xml.Substring(0, startSnip) + xml.Substring(endSnip);
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlWithoutOU)));
			logs = GetLogs();
			AssertContains("CusClassPartPivot - 1 inserts, 0 updates, 0 deletes", logs);  // Another insert, not updated... this checks that we're looking at the OU when seeking existing rows.  The one in hte DB cannot be a match the second time around, so insert new.
		}

		public void TestScenario25ForbidDuplicateOrgHeadersInPivots()
		{
			RunXml("25. Dupicate OHs in pivot.xml");
			Assert(GetLogs(), GetLogs().Contains(@"OrgHeader may be specified at most once on CusClassPartPivot."));
		}

		public void TestScenario26AllowAdditionOfNewOUsAndIgnoreOUsWithActionInsertWhenMatching()
		{
			var xml = GetXmlToImport("26. Ignore INSERT for matching.xml").ReadToEnd();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts, 0 updates, 0 deletes", logs);
			AssertContains("OrgPartRelation - 2 inserts, 0 updates, 0 deletes", logs);
			var xmlWithThirdInsertOU = xml.Replace("<!--", "").Replace("-->", ""); // Remvoe comment tags to make third OU node live
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlWithThirdInsertOU)));
			logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 0 updates, 0 deletes", logs);
			AssertContains("OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario27aInsertNewProductWithAnInsertOwner()
		{
			var xml = @"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""INSERT"">
    <ns0:PartNum>NXMLPRODUCT1</ns0:PartNum>
    <ns0:StockKeepingUnit>UNT</ns0:StockKeepingUnit>
    <ns0:Weight>1800</ns0:Weight>
    <ns0:WeightUQ>KG</ns0:WeightUQ>
    <ns0:NetWeight>1473</ns0:NetWeight>
    <ns0:Brand>Nissan</ns0:Brand>
    <ns0:Model>370Z</ns0:Model>
    <ns0:Desc>Nissan 370Z COUPE 3.7 AT NAV</ns0:Desc>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>CRAIMPCHI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>
";
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts, 0 updates, 0 deletes", logs);
			AssertContains("OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			Factory.Save();

			xml = @"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>NXMLPRODUCT1</ns0:PartNum>
    <ns0:StockKeepingUnit>UNT</ns0:StockKeepingUnit>
    <ns0:Weight>1800</ns0:Weight>
    <ns0:WeightUQ>KG</ns0:WeightUQ>
    <ns0:NetWeight>1473</ns0:NetWeight>
    <ns0:Brand>Nissan</ns0:Brand>
    <ns0:Model>370Z</ns0:Model>
    <ns0:Desc>Nissan 370Z COUPE 3.7 AT NAV</ns0:Desc>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>CRAIMPCHI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>
";
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			logs = GetLogs();
			AssertContains("Cannot process NXMLPRODUCT1. Related organisation CRAIMPCHI: Duplicate organization relationship - please remove this entry (or the duplicate entry)", logs);
			AssertContains("No insert/update action performed.", logs);
		}

		public void TestScenario27bInsertNewProductWithAnInsertOwner()
		{
			var xml = @"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""INSERT"">
    <ns0:PartNum>NXMLPRODUCT1</ns0:PartNum>
    <ns0:StockKeepingUnit>UNT</ns0:StockKeepingUnit>
    <ns0:Weight>1800</ns0:Weight>
    <ns0:WeightUQ>KG</ns0:WeightUQ>
    <ns0:NetWeight>1473</ns0:NetWeight>
    <ns0:Brand>Nissan</ns0:Brand>
    <ns0:Model>370Z</ns0:Model>
    <ns0:Desc>Nissan 370Z COUPE 3.7 AT NAV</ns0:Desc>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>CRAIMPCHI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>
";
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts, 0 updates, 0 deletes", logs);
			AssertContains("OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			xml = @"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>NXMLPRODUCT1</ns0:PartNum>
    <ns0:StockKeepingUnit>UNT</ns0:StockKeepingUnit>
    <ns0:Weight>1800</ns0:Weight>
    <ns0:WeightUQ>KG</ns0:WeightUQ>
    <ns0:NetWeight>1473</ns0:NetWeight>
    <ns0:Brand>Nissan</ns0:Brand>
    <ns0:Model>370Z</ns0:Model>
    <ns0:Desc>Nissan 370Z COUPE 3.7 AT NAV</ns0:Desc>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>CRAIMPCHI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>BACCRI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>
";
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 0 updates, 0 deletes", logs);
			AssertContains("OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			xml = @"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""INSERT"">
    <ns0:PartNum>NXMLPRODUCT1</ns0:PartNum>
    <ns0:StockKeepingUnit>UNT</ns0:StockKeepingUnit>
    <ns0:Weight>1800</ns0:Weight>
    <ns0:WeightUQ>KG</ns0:WeightUQ>
    <ns0:NetWeight>1473</ns0:NetWeight>
    <ns0:Brand>Nissan</ns0:Brand>
    <ns0:Model>370Z</ns0:Model>
    <ns0:Desc>Nissan 370Z COUPE 3.7 AT NAV</ns0:Desc>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>CRAIMPCHI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>BACCRI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>DAECOR</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>ABCEXPBNE</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>
";
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			logs = GetLogs();
			AssertContains(@"Test error: Cannot process NXMLPRODUCT1. Duplicate Product detected: Owner = CRAIMPCHI
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			xml = @"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>NXMLPRODUCT1</ns0:PartNum>
    <ns0:StockKeepingUnit>UNT</ns0:StockKeepingUnit>
    <ns0:Weight>1800</ns0:Weight>
    <ns0:WeightUQ>KG</ns0:WeightUQ>
    <ns0:NetWeight>1473</ns0:NetWeight>
    <ns0:Brand>Nissan</ns0:Brand>
    <ns0:Model>370Z</ns0:Model>
    <ns0:Desc>Nissan 370Z COUPE 3.7 AT NAV</ns0:Desc>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>CRAIMPCHI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>BACCRI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>DAECOR</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>ABCEXPBNE</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>
";
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			logs = GetLogs();
			AssertContains("OrgSupplierPart - 0 inserts, 0 updates, 0 deletes", logs);
			AssertContains("OrgPartRelation - 2 inserts, 0 updates, 0 deletes", logs);

			xml = @"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""INSERT"">
    <ns0:PartNum>NXMLPRODUCT2</ns0:PartNum>
    <ns0:StockKeepingUnit>UNT</ns0:StockKeepingUnit>
    <ns0:Weight>1800</ns0:Weight>
    <ns0:WeightUQ>KG</ns0:WeightUQ>
    <ns0:NetWeight>1473</ns0:NetWeight>
    <ns0:Brand>Nissan</ns0:Brand>
    <ns0:Model>370Z</ns0:Model>
    <ns0:Desc>Nissan 370Z COUPE 3.7 AT NAV</ns0:Desc>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>CRAIMPCHI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>BACCRI</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>DAECOR</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""INSERT"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>ABCEXPBNE</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>
";
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts, 0 updates, 0 deletes", logs);
			AssertContains("OrgPartRelation - 4 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario28OrgPartRelationIsSetCorrectly()
		{
			var xml = GetXmlToImport("28. Children and OrgPartRelation Without PK.xml").ReadToEnd();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes", logs);
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var orgRelation = factory.LoadTop1<OrgPartRelation>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			AssertEquals(4, pivots.Length);
			var parentPivot = (from Enterprise.Integration.Customs.US.ICusClassPartPivot p in pivots where (ZGuid)((BusinessObject)p)[CusClassPartPivotSchema.CI_CI_Parent.Name] == ZGuid.Empty select p).First();
			AssertEquals("CI_OH has been set correctly", orgRelation.OU_OH, ((BusinessObject)parentPivot)[CusClassPartPivotSchema.CI_OH.Name]);
		}

		public void TestScenario29Barcode()
		{
			CreateExistingProduct(owners: new string[] { craImpChi }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });

			RunXml("29. ProductB With Duplicate Barcode.xml");
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
Test error: The barcode(s) or Product Code '1313,PRODUCTB' has already been used on Product(s) 'PRODUCTA' by the same owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestScenario29Barcode_CanHaveSameBarcodeIfProductInactive()
		{
			CreateExistingProduct(owners: new string[] { craImpChi }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" }, isActive: false);

			RunXml("29. ProductB With Duplicate Barcode.xml");
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario29Barcode_DifferentOwnerCanHaveSameBarCode()
		{
			CreateExistingProduct(owners: new string[] { craImpChi }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append(GetOrgPartRelationSection("BACCRI"));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("1313", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario29Barcode_SupplierCanHaveDuplicateBarcode_Insert()
		{
			CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append(GetOrgPartRelationSection("", action: EntityAction.INSERT)); //abcExporters suppler
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("1313", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario29Barcode_SupplierCanHaveDuplicateBarcode_Merge()
		{
			CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });
			var productB = CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { abcExporters }, productCode: "PRODUCTB", barcodes: Array.Empty<string>());
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE", action: EntityAction.MERGE)); //abcExporters suppler
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("1313", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
Test error: The barcode(s) or Product Code '1313,PRODUCTB' has already been used on Product(s) 'PRODUCTA' by the same owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestScenario29Barcode_SupplierCanHaveDuplicateBarcode_Delete()
		{
			CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });
			var productB = CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { abcExporters }, productCode: "PRODUCTB", barcodes: Array.Empty<string>());
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("", action: EntityAction.DELETE)); //abcExporters suppler
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("1313", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 1 deletes
OrgSupplierPartBarcode - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario29Barcode_SupplierCanHaveDuplicateBarcode_Ignore()
		{
			CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });
			var productB = CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { abcExporters }, productCode: "PRODUCTB", barcodes: Array.Empty<string>());
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("", action: EntityAction.IGNORE)); //abcExporters suppler
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("1313", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario29Barcode_SupplierCannotHaveDuplicateBarcodeIfAlreadyHaveOwnerRelation()
		{
			CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });
			var productB = CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), productCode: "PRODUCTB", barcodes: new string[] { "4567" });

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("")); //abcExporters
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("1313", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
Test error: The barcode(s) or Product Code '1313,PRODUCTB' has already been used on Product(s) 'PRODUCTA' by the same owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestScenario29Barcode_SupplierCanHaveDuplicateBarcode_UpdateRelationshipToOwner()
		{
			CreateExistingProduct(owners: new string[] { craImpChi }, suppliers: Array.Empty<string>(), productCode: "PRODUCTB", barcodes: new string[] { "1313" });
			CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { craImpChi }, barcodes: new string[] { "1313" });
			RunXml("33. ProductA BTH=CRAIMPCHI merge.xml");

			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA
Test error: Cannot process PRODUCTA. Duplicate Product detected: Supplier = CRAIMPCHI
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestScenario29Barcode_SupplierCanHaveDuplicateBarcode_IfExistingBarcodeDoesNotHaveOwenerRelationshipInDB()
		{
			CreateExistingProduct(owners: new string[] { craImpChi }, suppliers: new string[] { abcExporters }, barcodes: new string[] { "1313" });
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE"));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("1313", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestScenario29Barcode_CannotImportProductWithProductNumSameAsOtherProductBarcodesForSameOwner()
		{
			CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), barcodes: new string[] { "PRODUCTB" });

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE"));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("8888", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
Test error: The barcode(s) or Product Code '8888,PRODUCTB' has already been used on Product(s) 'PRODUCTA' by the same owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestScenario29Barcode_CannotImportProductWithBarcodeSameAsOtherProductNumForSameOwner()
		{
			CreateExistingProduct(owners: new string[] { abcExporters }, suppliers: Array.Empty<string>(), barcodes: new string[] { "1313" });

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE"));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("PRODUCTA", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
Test error: The barcode(s) or Product Code 'PRODUCTA,PRODUCTB' has already been used on Product(s) 'PRODUCTA' by the same owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestScenario29Barcode_BarCodeSameAsProductCode()
		{
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE"));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("PRODUCTB", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTB
Test error: Barcode cannot be the same as the Product Code 'PRODUCTB'.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestScenario30InsertOwners()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				RunXml("1+30. ProductA OWN=CRAIMPCHI, OWN=BACCRI insert.xml");
				var logs = GetLogs();
				AssertContains("OrgSupplierPart - 1 inserts", logs);
				AssertProductNowExists(new string[] { craImpChi, baccri }, Array.Empty<string>());

				ResetLogs();
				RunXml("1+30. ProductA OWN=CRAIMPCHI, OWN=BACCRI insert.xml");
				logs = GetLogs();
				var result = logs.Contains(@"Cannot insert, product 'PRODUCTA' with related organisations CRAIMPCHI/OWN, BACCRI/OWN already exists.") || logs.Contains(@"Cannot insert, product 'PRODUCTA' with related organisations BACCRI/OWN, CRAIMPCHI/OWN already exists.");
				AssertEquals(true, result);
			}
		}

		public void TestScenario30InsertOwnersWhenBothOrgWithSameCodeExists()
		{
			CreateExistingProduct(new string[] { baccri }, new string[] { baccri });

			RunXml("1+30. ProductA OWN=CRAIMPCHI, OWN=BACCRI insert.xml");
			var logs = GetLogs();
			AssertContains("Cannot process PRODUCTA. Duplicate Product detected: Owner = BACCRI", logs);
		}

		public void TestScenario31SameOwnerAndBoth()
		{
			RunXml("31. ProductA OWN=CRAIMPCHI, BTH=CRAIMPCHI insert.xml");
			var logs = GetLogs();
			AssertContains(@"Cannot process PRODUCTA. Related organisation CRAIMPCHI: Duplicate organization relationship - please remove this entry (or the duplicate entry)", logs);
		}

		public void TestScenario32MergeProductMergeSuppliers()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				RunXml("3+32. ProductA Sup=ABCEXPBNE merge.xml");
				var logs = GetLogs();
				AssertContains("OrgSupplierPart - 1 inserts", logs);
				AssertProductNowExists(Array.Empty<string>(), new string[] { abcExporters });

				ResetLogs();
				RunXml("3+32. ProductA Sup=ABCEXPBNE merge.xml");
				logs = GetLogs();
				AssertContains("OrgSupplierPart - 0 inserts, 0 updates, 0 deletes", logs);
				AssertProductNowExists(Array.Empty<string>(), new string[] { abcExporters });
			}
		}

		public void TestScenario32MergeProductInsertSuppliers()
		{
			RunXml("3+32. ProductA Sup=ABCEXPBNE insert.xml");
			var logs = GetLogs();
			AssertContains("OrgSupplierPart - 1 inserts", logs);
			AssertProductNowExists(Array.Empty<string>(), new string[] { abcExporters });

			ResetLogs();
			RunXml("3+32. ProductA Sup=ABCEXPBNE insert.xml");
			logs = GetLogs();
			AssertContains(@"Cannot process PRODUCTA. Related organisation ABCEXPBNE: Duplicate organization relationship - please remove this entry (or the duplicate entry)", logs);
		}

		public void TestScenario33MergeOwnerAndSupplierWhenBothOrgWithSameCodeExists()
		{
			CreateExistingProduct(new string[] { baccri }, new string[] { baccri });
			CreateExistingProduct(new string[] { craImpChi }, new string[] { craImpChi });

			RunXml("1. ProductA OWN=CRAIMPCHI merge.xml");
			var logs = GetLogs();
			AssertContains(@"Cannot process PRODUCTA. Duplicate Product detected: Owner = CRAIMPCHI", logs);

			ResetLogs();
			RunXml("2. ProductA Sup=BACCRI merge.xml");
			logs = GetLogs();
			AssertContains(@"Cannot process PRODUCTA. Duplicate Product detected: Supplier = BACCRI", logs);
		}

		public void TestScenario33MergeBothOrgWhenOwnerWithSameCodeExists()
		{
			CreateExistingProduct(new string[] { craImpChi }, Array.Empty<string>());

			RunXml("33. ProductA BTH=CRAIMPCHI merge.xml");
			var logs = GetLogs();
			AssertContains(@"Cannot process PRODUCTA. Duplicate Product detected: Owner = CRAIMPCHI", logs);
		}

		public void TestScenario34UpdateProductRelationshipWithoutPK()
		{
			CreateExistingProduct(new string[] { craImpChi }, Array.Empty<string>());
			RunXml("34. ProductA Change OWN to BTH without PK.xml");
			var logs = GetLogs();

			AssertContains(@"OrgSupplierPart - 0 inserts, 1 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 1 deletes", logs);
		}

		public void TestScenario34UpdateProductRelationshipWithDifferentPK()
		{
			CreateExistingProduct(new string[] { craImpChi }, Array.Empty<string>());
			RunXml("34. ProductA Change OWN to BTH with different PK.xml");
			var logs = GetLogs();

			AssertContains(@"OrgSupplierPart - 0 inserts, 1 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 1 deletes", logs);
		}

		public void TestScenario34UpdateProductRelationshipWithPK()
		{
			var product = CreateExistingProduct(new string[] { craImpChi }, Array.Empty<string>());
			var orgRelationPK = product.RelatedOrganisations.GetPKs().First();
			var xml = GetXmlToImport("34. ProductA Change OWN to BTH with PK placeholder.xml").ReadToEnd();
			xml = xml.Replace("{PK}", orgRelationPK.ToString());
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();

			AssertContains(@"OrgSupplierPart - 0 inserts, 1 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 1 deletes", logs);
		}

		public void TestScenario34UpdateProductRelationshipWithPKandWrongRelation()
		{
			var product = CreateExistingProduct(new string[] { craImpChi }, Array.Empty<string>());
			var orgRelationPK = product.RelatedOrganisations.GetPKs().First();
			var xml = GetXmlToImport("34. ProductA Change OWN to BTH with PK and Wrong Relation.xml").ReadToEnd();
			xml = xml.Replace("{PK}", orgRelationPK.ToString());
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();

			AssertContains(@"Cannot process ProductA. Duplicate Product detected: Owner = CRAIMPCHI", logs);
		}

		#region Test Scenario35 Multiple HTI With Different Attributes

		public void TestScenario35MultipleHTIWithDifferentAttributesNormal()
		{
			var xml = GetXmlToImport("35. Multiple HTI With Different Attributes.xml").ReadToEnd();

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 8 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);
		}

		#region Test Scenario35 Multiple HTI With Different Attributes (Part 1: Improve Old Logics)

		string GetScenario35HTIPart1XMLToTest(bool keepChildTypePlaceholder = false,
			bool keepCountryPlaceholder = false,
			bool keepOrgPartRelationPlaceholder = false,
			bool keepRelOrgHeaderPlaceholder = false)
		{
			var result = GetXmlToImport("35.1. Multiple HTI With Different Attributes.xml").ReadToEnd();
			if (!keepChildTypePlaceholder)
			{
				result = result.Replace("{Part1ChildType1}", @"<ChildType>HTI</ChildType>")
				.Replace("{Part1ChildType2}", @"<ChildType>HTI</ChildType>");
			}
			if (!keepCountryPlaceholder)
			{
				result = result.Replace("{Part1Country1}", @"<Country TableName=""RefCountry"">
              <Code>US</Code>
			  </Country>")
				.Replace("{Part1Country2}", @"<Country TableName=""RefCountry"">
              <Code>US</Code>
			  </Country>");
			}
			if (!keepOrgPartRelationPlaceholder)
			{
				result = result.Replace("{Part1Rel1}", @"<Relationship>OWN</Relationship>")
				.Replace("{Part1Rel2}", @"<Relationship>OWN</Relationship>");
			}
			if (!keepRelOrgHeaderPlaceholder)
			{
				result = result.Replace("{Part1RelOrgHeader1}", @"
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
            </OrgHeader>")
				.Replace("{Part1RelOrgHeader2}", @"
            <OrgHeader>
              <Code>ABCEXPBNE</Code>
            </OrgHeader>");
			}
			return result;
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesEntityHasBothXAndYButNotEqual_Success()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepChildTypePlaceholder: true)
				.Replace("{Part1ChildType1}", @"<ChildType>HTE</ChildType>")
				.Replace("{Part1ChildType2}", @"<ChildType>HTI</ChildType>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 8 inserts, 0 updates, 0 deletes
OrgPartRelation - 2 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesEntityHasBothXEqualsY_Success()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepChildTypePlaceholder: true)
				.Replace("{Part1ChildType1}", @"<ChildType>HTI</ChildType>")
				.Replace("{Part1ChildType2}", @"<ChildType>HTI</ChildType>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 8 inserts, 0 updates, 0 deletes
OrgPartRelation - 2 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesEntityOnlyHasYButEmpty_Fail()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepChildTypePlaceholder: true)
				.Replace("{Part1ChildType1}", @"")
				.Replace("{Part1ChildType2}", @"<ChildType></ChildType>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has duplicate pivots when comparing using a limited number of fields. Pivots should be action INSERT or should be sufficiently different.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesEntityOnlyHasYNotEmpty_Success()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepChildTypePlaceholder: true)
				.Replace("{Part1ChildType1}", @"")
				.Replace("{Part1ChildType2}", @"<ChildType>HTI</ChildType>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 8 inserts, 0 updates, 0 deletes
OrgPartRelation - 2 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesCountryXNullAndYNull_Fail()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepCountryPlaceholder: true)
				.Replace("{Part1Country1}", @"")
				.Replace("{Part1Country2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"All CusClassPartPivot rows must have a country specified";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesCountryXNullAndYNotEmpty_FailBecauseInvalidXML()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepCountryPlaceholder: true)
				.Replace("{Part1Country1}", @"")
				.Replace("{Part1Country2}", @"<Country TableName=""RefCountry"">
              <Code>US</Code>
			  </Country>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"All CusClassPartPivot rows must have a country specified";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesCountryXNullAndYEmpty_Fail()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepCountryPlaceholder: true)
				.Replace("{Part1Country1}", @"")
				.Replace("{Part1Country2}", @"<Country TableName=""RefCountry"">
              <Code></Code>
			  </Country>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"All CusClassPartPivot rows must have a country specified";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestCusClassPartPivotAndCusClassificationCountriesMismatch()
		{
			var xml = GetScenario35HTIPart1XMLToTest()
				.Replace("<CusClassification />", @"<CusClassification><ClassificationType>IMP</ClassificationType><CountryCode>AU</CountryCode><LookupCode>9001.90.90 53</LookupCode><CountryCodeExternal TableName=""RefCountry""><Code>AU</Code><PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK></CountryCodeExternal></CusClassification>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"CusClassPartPivot and its CusClassification's countries mismatch.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestCusClassificationCountryMissed()
		{
			var xml = GetScenario35HTIPart1XMLToTest()
				.Replace("<CusClassification />", @"<CusClassification><ClassificationType>IMP</ClassificationType><CountryCode>AU</CountryCode><LookupCode>9001.90.90 53</LookupCode></CusClassification>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"CusClassPartPivot's CusClassification rows must have a country specified.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesCountryXEqualsY_Success()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepCountryPlaceholder: true)
				.Replace("{Part1Country1}", @"<Country TableName=""RefCountry"">
              <Code>US</Code>
			  </Country>")
				.Replace("{Part1Country2}", @"<Country TableName=""RefCountry"">
              <Code>US</Code>
			  </Country>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 8 inserts, 0 updates, 0 deletes
OrgPartRelation - 2 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesRelXNullAndYNull_Fail()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepOrgPartRelationPlaceholder: true)
				.Replace("{Part1Rel1}", @"")
				.Replace("{Part1Rel2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @" Illegal XML: the OrgPartRelation requires a relationship type";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesRelXNullAndYNotEmpty_FailBecauseInvalidXML()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepOrgPartRelationPlaceholder: true)
				.Replace("{Part1Rel1}", @"")
				.Replace("{Part1Rel2}", @"<Relationship>OWN</Relationship>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"Illegal XML: the OrgPartRelation requires a relationship type";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesRelXNullAndYEmpty_FailBecauseInvalidXML()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepOrgPartRelationPlaceholder: true)
				.Replace("{Part1Rel1}", @"")
				.Replace("{Part1Rel2}", @"<Relationship></Relationship>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"Illegal XML: the OrgPartRelation requires a relationship type";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesRelXEqualsY_Success()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepOrgPartRelationPlaceholder: true)
				.Replace("{Part1Rel1}", @"<Relationship>OWN</Relationship>")
				.Replace("{Part1Rel2}", @"<Relationship>OWN</Relationship>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 8 inserts, 0 updates, 0 deletes
OrgPartRelation - 2 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 4, 4);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOrgXNullAndYNull_FailBecauseInvalidXML()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepRelOrgHeaderPlaceholder: true)
				.Replace("{Part1RelOrgHeader1}", @"")
				.Replace("{Part1RelOrgHeader2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"Illegal XML; the Relationship requires an Organisation";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOrgXNullAndYNotEmpty_FailBecauseInvalidXML()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepRelOrgHeaderPlaceholder: true)
				.Replace("{Part1RelOrgHeader1}", @"")
				.Replace("{Part1RelOrgHeader2}", @"
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
              <PK>988dbe0b-d5c3-4d81-be4c-c3b19d97c52c</PK>
            </OrgHeader>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"Illegal XML; the Relationship requires an Organisation";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOrgXNullAndYEmpty_FailBecauseInvalidXML()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepRelOrgHeaderPlaceholder: true)
				.Replace("{Part1RelOrgHeader1}", @"")
				.Replace("{Part1RelOrgHeader2}", @"
            <OrgHeader>
              <Code></Code>
            </OrgHeader>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @" Illegal XML; the Relationship requires an Organisation";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOrgXEqualsY_FailBecauseInvalidXML()
		{
			var xml = GetScenario35HTIPart1XMLToTest(keepRelOrgHeaderPlaceholder: true)
				.Replace("{Part1RelOrgHeader1}", @"
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
              <PK>988dbe0b-d5c3-4d81-be4c-c3b19d97c52c</PK>
            </OrgHeader>")
				.Replace("{Part1RelOrgHeader2}", @"
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
              <PK>988dbe0b-d5c3-4d81-be4c-c3b19d97c52c</PK>
            </OrgHeader>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"Invalid XML for product IMPORT DEFAULT GL. The combination of relationship type and organisation code CRAIMPCHI is already specified";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		#endregion

		#region Test Scenario35 Multiple HTI With Different Attributes (Part 2: AT1 AT2 AT3 Match Comparer)

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1Same_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attribute type cannot be duplicated between pivots, type must be unique across pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1NotSame_Success()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 2 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 1, 1);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 1, 1);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2Same_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attribute type cannot be duplicated between pivots, type must be unique across pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2NotSame_Success()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 4 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 2, 2);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 2, 2);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2SameAT3Same_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attribute type cannot be duplicated between pivots, type must be unique across pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2SameAT3NotSame_Success()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 6 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1NotSameAT2SameAT3Same_FailAttributesDuplicated()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 6 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2NotSameAT3Same_Success()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 6 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2SameAT3SameWithDifferentOrder_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attribute type cannot be duplicated between pivots, type must be unique across pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2SameAT3NotSameWithDifferentOrder_Success()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>D</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>C</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 12 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 6, 6);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 6, 6);
		}

		#endregion

		#region Test Scenario35 Multiple HTI With Different Attributes (Part 3: AT1 AT2 AT3 FindRow)

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1NotSame_Insert()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 2 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 1, 1);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>C</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>D</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 2 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 4, 1, 1, 1, 1);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1Same_Update()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 2 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 1, 1);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 1, 1);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2NotSame_Insert()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 4 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 2, 2);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>C</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
CusAttributeFilter - 2 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 3, 2, 2, 2);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2Same_Update()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 4 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 2, 2);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>");
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 2, 2);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2SameAT3NotSame_Insert()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 6 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter>
			<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>C</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
CusAttributeFilter - 3 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 3, 3, 3, 3);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2SameAT3Same_Update()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 6 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
              </CusAttributeFilter><CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 3, 3);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesAT1SameAT2SameAT3NotSame_AllWithDiferentOrder_Update()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>A</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT1</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>B</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT1</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>A</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT2</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>B</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT2</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>A</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT3</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>B</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT3</AttributeName>
				</CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>B</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT1</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>A</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT1</AttributeName>
				</CusAttributeFilter>

				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>B</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT2</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>A</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT2</AttributeName>
				</CusAttributeFilter>

				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>C</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT3</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
					<AttributeValue1>D</AttributeValue1>
					<AttributeValue2></AttributeValue2>
					<AttributeOperator></AttributeOperator>
					<AttributeName>AT3</AttributeName>
				</CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 12 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 6, 6);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT1</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>B</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>A</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT2</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>C</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
				</CusAttributeFilter>
				<CusAttributeFilter Action=""MERGE"">
                <AttributeValue1>D</AttributeValue1>
                <AttributeValue2></AttributeValue2>
                <AttributeOperator></AttributeOperator>
                <AttributeName>AT3</AttributeName>
              </CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 2, 6, 6);
		}

		#endregion

		#region Test Scenario35 Multiple HTI With Different Attributes (Part 4: Fail with invalid attributes)

		public void TestScenario35MultipleHTIWithDifferentAttributesNoAttributes_FailAsDuplicatedPivots()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has duplicate pivots when comparing using a limited number of fields. Pivots should be action INSERT or should be sufficiently different";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveEmptyAT1OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1></AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Some attribute values are empty or duplicated on a single pivot.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveEmptyAT2OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1></AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Some attribute values are empty or duplicated on a single pivot.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveEmptyAT3OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1></AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT3</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Some attribute values are empty or duplicated on a single pivot.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveDuplicatedAT1OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Some attribute values are empty or duplicated on a single pivot.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveDuplicatedAT2OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Some attribute values are empty or duplicated on a single pivot.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveDuplicatedAT3OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT3</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT3</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Some attribute values are empty or duplicated on a single pivot.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveAT1OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attributes have been specified on other pivots and therefore must be specified on all pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveAT2OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attributes have been specified on other pivots and therefore must be specified on all pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithDifferentAttributesOneHaveAT3OneDont_Fail()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT3</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attributes have been specified on other pivots and therefore must be specified on all pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		public void TestScenario35MultipleHTIWithSameAttributes()
		{
			var xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>B</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"This Part has invalid HTI (Country='US', Party='CRAIMPCHI') attributes. Attribute type cannot be duplicated between pivots, type must be unique across pivots.";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);

			xml = GetXmlToImport("35.2. Multiple HTI With Different Attributes.xml").ReadToEnd()
				.Replace("{Attributes1}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT1</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT3</AttributeName>
</CusAttributeFilter>")
				.Replace("{Attributes2}", @"<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT3</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT2</AttributeName>
</CusAttributeFilter>
<CusAttributeFilter Action=""MERGE"">
	<AttributeValue1>A</AttributeValue1>
	<AttributeValue2></AttributeValue2>
	<AttributeOperator></AttributeOperator>
	<AttributeName>AT3</AttributeName>
</CusAttributeFilter>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 0);
		}

		#endregion

		public void TestScenario35MultipleHTIWithDifferentAttributesNormal_withTwoOrgs()
		{
			var xml = GetXmlToImport("35.3. Multiple HTI With Different Attributes.xml").ReadToEnd();

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var assertLogs = @"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 4 inserts, 0 updates, 0 deletes
CusUSClassification - 4 inserts, 0 updates, 0 deletes
CusAttributeFilter - 16 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 4, 4, 4, 4, 4);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			assertLogs = @"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes";

			AssertScenario35MultipleHTIWithDifferentAttributes(GetLogs(), assertLogs, 4, 4, 4, 4, 4);
		}

		void AssertScenario35MultipleHTIWithDifferentAttributes(string logs, string assertedlogs, int pivotsLength, params int[] attrsLength)
		{
			AssertContains(assertedlogs, logs);
			if (pivotsLength > 0)
			{
				var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
				var orgRelation = factory.LoadTop1<OrgPartRelation>(new ZQuery());
				var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
				AssertEquals(pivotsLength, pivots.Length);
				for (int i = 0; i < pivotsLength; i++)
				{
					var attrs = new BusinessObjectFactory().Load<Enterprise.Integration.Customs.US.ICusAttributeFilter>(new ZQuery(CusAttributeFilterSchema.BG_CI, pivots[i].PK));

					AssertEquals(attrsLength[i], attrs.Length);
				}
			}
		}

		#endregion

		public void TestScenario36CreatingAProductEntityWithoutAPKDuringUpdate()
		{
			CreateExistingProduct(new[] { craImpChi }, Array.Empty<string>());
			RunXml("36. ProductA Sup=CRAIMPCHI update.xml");
			var logs = GetLogs();
			Assert(logs, logs.Contains("Unable to find OrgSupplierPart to update using the details provided."));
		}

		public void TestScenario38WhiteSpaceInTheProductName()
		{
			CreateExistingProduct(new[] { craImpChi }, Array.Empty<string>());
			RunXml("38. WhitespaceInTheProductName.xml");
			var logs = GetLogs();
			Assert(logs, logs.Contains("Importing Product: PRODUCT         A"));
		}

		public void TestCreateMIDOrganization()
		{
			var org1 = factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			org1.OH_FullName = "organization1";
			org1.MainAddress.OA_Address1 = "A MESSAGE HAS BEEN SENT TO US CUSTOMS TO REQUEST";
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "USCODE1", "US");
			factory.Save();

			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			RunXml("37.1. CusUSClassification.Manufacturer.OrgCusCode.xml");

			var logs = GetLogs();
			AssertContains(@"A new MID Organization: AUTCRE was created because no matched Organization was found and 'Registry > Customs > United States of America > Import > Create MID Organization on unmatched import' is on.
Importing Product: PRODUCTA_XYL
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100050") as BusinessObject;
			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));

			AssertNotNull(org);
			var manufacturerAddressPK = (ZGuid)pivot[CusUSClassificationSchema.Constants.CD_OA_Manufacturer];
			AssertNotEquals("Should not matches on org1.MainAddress even if it has the same code", org1.MainAddress.PK, manufacturerAddressPK);
			AssertEquals("Matched to the correct address", org.MainAddress.PK, manufacturerAddressPK);

			var messages = factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, org.PK));
			AssertEquals(1, messages.Length);
			var message = messages[0];
			AssertEquals("MA", message.EM_MessageType);
		}

		public void TestMatchingMIDAddress()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var manufacturer = factory.New<OrgHeader>();
			manufacturer.OH_Code = "ORG";
			manufacturer.MainAddress.OA_Code = "101 Main Street";
			manufacturer.MainAddress.Address1 = "101 Main Street";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "USTEMPCOMPANY", Core.Constants.CountryCodes.UnitedStates);
			factory.Save();

			RunXml("37.1. CusUSClassification.Manufacturer.OrgCusCode.xml");

			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA_XYL
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100050") as BusinessObject;

			AssertEquals("Matching address by Address Code", manufacturer.MainAddress.PK, pivot[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertNull("No new manufacturer is created", org);
		}

		public void TestMatchingMIDAddress_NoCountryNoMatchNoCreateWithExistingOrg()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var manufacturer = factory.New<OrgHeader>();
			manufacturer.OH_Code = "ORG";
			manufacturer.MainAddress.OA_Code = "101 Main Street";
			manufacturer.MainAddress.Address1 = "101 Main Street";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "USTEMPCOMPANY", Core.Constants.CountryCodes.UnitedStates);
			factory.Save();

			RunXml("37.2. CusUSClassification.Manufacturer.OrgCusCode.NoCountry.xml");

			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA_XYL
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100050") as BusinessObject;
			AssertEquals("Empty Manufacturer", ZGuid.Empty, pivot[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertNull("No new manufacturer is created", org);
		}

		public void TestMatchingMIDAddress_NoCountryNoCreate()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			RunXml("37.2. CusUSClassification.Manufacturer.OrgCusCode.NoCountry.xml");

			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA_XYL
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100050") as BusinessObject;

			AssertEquals("Empty Manufacturer", ZGuid.Empty, pivot[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertNull("No new manufacturer is created", org);
		}

		public void TestMatchingMIDAddress_XXCountryNoCreate()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			RunXml("37.3. CusUSClassification.Manufacturer.OrgCusCode.XXCountry.xml");

			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA_XYL
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100050") as BusinessObject;

			AssertEquals("Empty Manufacturer", ZGuid.Empty, pivot[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertNull("No new manufacturer is created", org);
		}

		public void TestMatchingMIDOrg()
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.MainAddress.OA_Code = "101 Main Street";
			org.MainAddress.Address1 = "101 Main Street";
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABCDEFG1", Core.Constants.CountryCodes.UnitedStates);
			var address2 = org.Addresses.AddNew("102", "102");
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABCDEFG2", Core.Constants.CountryCodes.UnitedStates);
			factory.Save();

			RunXml("38. CusClassPartPivot.OrgHeader.OrgCusCode.xml");

			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot = pivots.FirstOrDefault() as BusinessObject;

			AssertEquals(org.PK, pivot[CusClassPartPivotSchema.Constants.CI_OH]);
		}

		public void TestMatchingMIDOrg_NoMatchAndError()
		{
			RunXml("38. CusClassPartPivot.OrgHeader.OrgCusCode.xml");

			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA_XYL
Test error: Could not insert/update the Organization (OrgHeader) as it had an invalid reference to a Business Registration Number (OrgCusCode). There is no Business Registration Number with the following values: [RN_NKCodeCountry:US][OH:00000000-0000-0000-0000-000000000000][CustomsRegNo:ABCDEFG2][CodeType:MID][CountryDefault:False].
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestMatchingMIDOrg_NoMatchAndCreate()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			RunXml("38. CusClassPartPivot.OrgHeader.OrgCusCode.xml");

			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot = pivots.FirstOrDefault() as BusinessObject;
			AssertEquals(org.PK, pivot[CusClassPartPivotSchema.Constants.CI_OH]);

			var messages = factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, org.PK));
			AssertEquals(1, messages.Length);
			var message = messages[0];
			AssertEquals("MA", message.EM_MessageType);
		}

		public void TestMatchingMIDOrg_MIDHasLeadingSpaces_NoMatchAndError()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			RunXml("38.1 OrgCusCode.MIDWithLeadingSpace.xml");

			var logs = GetLogs();
			AssertContains("Import Log", @"There are invalid characters in MID   ABCDEFG1, only alphanumeric characters are allowed.
Importing Product: PRODUCTA_XYL
Test error: Could not insert/update the Organization (OrgHeader) as it had an invalid reference to a Business Registration Number (OrgCusCode). There is no Business Registration Number with the following values: [RN_NKCodeCountry:US][OH:00000000-0000-0000-0000-000000000000][CustomsRegNo:  ABCDEFG1][CodeType:MID][CountryDefault:False].
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestMatchAddInfoOrgByMID_NoMatchNoCreate()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			RunXml("39. CusClassPartPivot.AdditionalInformationChild.AddInfoOrg.xml");

			var orgs = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName)).OrderBy(o => o.OH_Code).ToArray();
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var cusAddInfos = factory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, pivots.FirstOrDefault().PK));
			AssertEquals("Empty orgs along with asterisks are removed", "Description=DBD-GLOVE,PF,STRTCH VINYL,DISPSABLE GLV*ProductCode=80L--YZ", (cusAddInfos.FirstOrDefault() as BusinessObject)[CusAddInfoSchema.Constants.B7_AddInfoData]);

			AssertEquals("No org is created", 0, factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName)).Length);
		}

		public void TestMatchAddInfoOrgByMID_NoMatchAndCreate()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			RunXml("39. CusClassPartPivot.AdditionalInformationChild.AddInfoOrg.xml");

			var orgs = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName)).OrderBy(o => o.OH_Code).ToArray();
			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var cusAddInfos = factory.Load<Enterprise.Integration.Customs.ICusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, pivots.FirstOrDefault().PK));

			StringBuilder expected = new StringBuilder();
			expected.Append("Description=DBD-GLOVE,PF,STRTCH VINYL,DISPSABLE GLV*DummyAddress01=");
			expected.Append(orgs[0].MainAddress.PK.ToString());
			expected.Append("*DummyAddress02=");
			expected.Append(orgs[1].MainAddress.PK.ToString());
			expected.Append("*ManufacturerAddress=");
			expected.Append(orgs[2].MainAddress.PK.ToString());
			expected.Append("*ProductCode=80L--YZ");
			AssertEquals(expected.ToString(), (cusAddInfos.FirstOrDefault() as BusinessObject)[CusAddInfoSchema.Constants.B7_AddInfoData]);

			var messages = factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(3, messages.Length);
			foreach (var message in messages)
			{
				AssertEquals("MA", message.EM_MessageType);
			}
		}

		public void TestMatchAddInfoOrgByMID_NoMatchNoCreate_EmptyValue()
		{
			var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
			var createMIDOrganizationOnUnmatchedImport = (BooleanRegistryItem)uSCustomsRegistry.FindByName("CreateMIDOrganizationOnUnmatchedImport");
			createMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			RunXml("39.1 CusClassPartPivot.AdditionalInformationChild.AddInfoOrgEmpty.xml");

			var logs = GetLogs();
			AssertNotContains("Do not create empty organisation if MID is empty.", @"There was a problem saving the new MID Organization. This means that it cannot find out the matched manufacturer. You can try to reimport the data.", logs);
		}

		public void TestScenario37MatchingOrgAddressUsingRegistrationNumber()
		{
			var manufacturer = factory.New<OrgHeader>();
			manufacturer.OH_Code = "ORG";
			manufacturer.MainAddress.OA_Code = "101 Main Street";
			manufacturer.MainAddress.Address1 = "101 Main Street";
			var address = manufacturer.Addresses.AddNew();
			address.OA_Code = "184 Test Street";
			address.Address1 = "184 Test Street";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "003008787159", Core.Constants.CountryCodes.UnitedStates);
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "56668558", Core.Constants.CountryCodes.UnitedStates);
			factory.Save();

			RunXml("37. CusUSClassification.Manufacturer.OrgCusCode.xml");
			var logs = GetLogs();
			AssertContains(@"Importing Product: PRODUCTA_XYL
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 3 inserts, 0 updates, 0 deletes
ComponentCusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 5 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			var product = factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			var pivots = factory.Load<Enterprise.Integration.Customs.US.ICusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK));
			var pivot1 = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100010") as BusinessObject;
			var pivot2 = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100020") as BusinessObject;
			var pivot3 = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100030") as BusinessObject;
			var pivot4 = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100040") as BusinessObject;
			var pivot5 = pivots.FirstOrDefault(p => p.CI_TariffNum == "0101100050") as BusinessObject;

			AssertEquals("Matching address by Address Code", address.PK, pivot1[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);
			AssertEquals("Matching address by Address Code, ignores the OrgCusCode", address.PK, pivot2[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);
			AssertEquals("Matching address by the single OrgCusCode", manufacturer.MainAddress.PK, pivot3[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);
			AssertEquals("Does not matching address when multiple OrgCusCodes found", ZGuid.Empty, pivot4[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);
			AssertEquals("Does not matching address if OrgCusCode type is not those which we support to be unique", ZGuid.Empty, pivot5[CusUSClassificationSchema.Constants.CD_OA_Manufacturer]);
		}

		#region TestCannotDeleteOrgPartRelationWithUnfinalisedASNLines

		public void TestCannotDeleteOrgPartRelationWithUnfinalisedASNLines()
		{
			var client = WhsHelper.CreateClient("ORG2");
			var whs = WhsHelper.CreateWarehouse("1", "A");
			var part1 = WhsHelper.CreateProduct(client, "P1");
			var relation = part1.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			var receive = WhsHelper.CreateWhsReceive(client, whs, "R1", null);
			WhsHelper.CreateWhsReceiveInventoryLine(receive, part1.PK, 0m, "A");
			WhsHelper.CreateAsnLine(receive, part1, 0m);
			Factory.Save();
			var relationInDB = Factory.Load<OrgPartRelation>(relation.PK);
			AssertNotNull("Should have the part relation in DB.", relationInDB);
			AssertEquals(false, relationInDB.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relationInDB.HasAsnLineOnUnfinalisedReceive);

			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_CanBeDeleted(part1);

			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PK>{part1.PK}</ns0:PK>
    <ns0:PartNum>{part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""DELETE"">
        <ns0:PK>{relation.PK}</ns0:PK>
        <ns0:Relationship>BTH</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>{client.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs1 = GetLogs();

				var relationShouldNotBeNull = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
				AssertNotNull("Should not have deleted the part relation.", relationShouldNotBeNull);
				AssertContains($@"Cannot delete Part RelationShip(s) as there are current ASN line(s) in the warehouse. Client = {client.OH_Code}.", logs1);
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertEquals(true, receive.IsFinalised);
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			ResetLogs();

			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs2 = GetLogs();

				var relationShouldBeNull = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
				AssertNull("Should have deleted the part relation.", relationShouldBeNull);
				AssertContains($@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 1 deletes", logs2);
			}
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_CanBeDeleted(OrgSupplierPart part)
		{
			ResetLogs();
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = WhsHelper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient.PK;
			anotherRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PK>{part.PK}</ns0:PK>
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""DELETE"">
        <ns0:PK>{anotherRelation.PK}</ns0:PK>
        <ns0:Relationship>BTH</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>{anotherClient.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs = GetLogs();

				var relationShouldBeNull = new BusinessObjectFactory().Load<OrgPartRelation>(anotherRelation.PK);
				AssertNull("Should have deleted the part relation.", relationShouldBeNull);
				AssertContains($@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 1 deletes", logs);
			}

			ResetLogs();
		}

		#endregion

		#region TestCannotChangeOrgPartRelationWithUnfinalisedASNLines

		public void TestCannotChangeOrgPartRelationTypeWithUnfinalisedASNLines()
		{
			var client = WhsHelper.CreateClient("ORG2");
			var whs = WhsHelper.CreateWarehouse("1", "A");
			var part1 = WhsHelper.CreateProduct(client, "P1");
			var relation = part1.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			var receive = WhsHelper.CreateWhsReceive(client, whs, "R1", null);
			WhsHelper.CreateWhsReceiveInventoryLine(receive, part1.PK, 0m, "A");
			WhsHelper.CreateAsnLine(receive, part1, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLinesWhenChangeRelationType(part1);

			// change Relationship Type from Both to Supplier
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PK>{part1.PK}</ns0:PK>
    <ns0:PartNum>{part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>{OrgPartRelation.RelationshipTypes.Supplier}</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>{client.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs1 = GetLogs();

				var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
				AssertEquals("Should not have updated relation type.", OrgPartRelation.RelationshipTypes.Both, relationInDB.OU_Relationship);
				AssertContains($@"Cannot change Part RelationShip(s) as there are current ASN line(s) in the warehouse. Client = {client.OH_Code}.", logs1);
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertEquals(true, receive.IsFinalised);
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			ResetLogs();

			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs2 = GetLogs();

				var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
				AssertEquals("Should still not have updated relation type.", OrgPartRelation.RelationshipTypes.Both, relationInDB.OU_Relationship);
				AssertContains("Should failed not because of unfinalised ASN lines, but for other reasons.", string.Format("There is no OrgPartRelation with the following values: [OH:{0}][Relationship:{1}].",
					client.PK, OrgPartRelation.RelationshipTypes.Supplier), logs2);
			}
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLinesWhenChangeRelationType(OrgSupplierPart part)
		{
			ResetLogs();
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = WhsHelper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient.PK;
			anotherRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
			// change Relationship Type from Both to Supplier
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PK>{part.PK}</ns0:PK>
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>{OrgPartRelation.RelationshipTypes.Supplier}</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>{anotherClient.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs = GetLogs();
				var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(anotherRelation.PK);
				AssertEquals("Should still not have updated relation type.", OrgPartRelation.RelationshipTypes.Both, relationInDB.OU_Relationship);
				AssertContains("Should failed not because of unfinalised ASN lines, but for other reasons.", string.Format("There is no OrgPartRelation with the following values: [OH:{0}][Relationship:{1}].",
					anotherClient.PK, OrgPartRelation.RelationshipTypes.Supplier), logs);
			}
		}

		public void TestCannotChangeOrgPartRelationClientWithUnfinalisedASNLines()
		{
			var client = WhsHelper.CreateClient("ORG2");
			var whs = WhsHelper.CreateWarehouse("1", "A");
			var part1 = WhsHelper.CreateProduct(client, "P1");
			var relation = part1.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			var receive = WhsHelper.CreateWhsReceive(client, whs, "R1", null);
			WhsHelper.CreateWhsReceiveInventoryLine(receive, part1.PK, 0m, "A");
			WhsHelper.CreateAsnLine(receive, part1, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLinesWhenChangeClient(part1);

			var client2 = WhsHelper.CreateClient("ORG5");
			Factory.Save();
			// change client code
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PK>{part1.PK}</ns0:PK>
    <ns0:PartNum>{part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>{OrgPartRelation.RelationshipTypes.Both}</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>{client2.OH_Code}</ns0:Code>
          <ns0:PK>{client2.PK}</ns0:PK>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			ResetLogs();
			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);

				var logs1 = GetLogs();

				var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
				// can not find existing relation by the client2.PK in xml, so it will fail for other reason
				AssertEquals("Should not have updated client code.", client.PK, relationInDB.OU_OH);
				AssertContains("Should failed not because of unfinalised ASN lines, but for other reasons.", string.Format("There is no OrgPartRelation with the following values: [OH:{0}][Relationship:{1}].",
					client2.PK, OrgPartRelation.RelationshipTypes.Both), logs1);
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertEquals(true, receive.IsFinalised);
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			ResetLogs();

			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs2 = GetLogs();

				var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
				AssertEquals("Should still not have updated client code.", client.PK, relationInDB.OU_OH);
				AssertContains("Should failed not because of unfinalised ASN lines, but for other reasons.", string.Format("There is no OrgPartRelation with the following values: [OH:{0}][Relationship:{1}].",
					client2.PK, OrgPartRelation.RelationshipTypes.Both), logs2);
			}
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLinesWhenChangeClient(OrgSupplierPart part)
		{
			ResetLogs();
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = WhsHelper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient.PK;
			anotherRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			var client2 = WhsHelper.CreateClient("ORG7");
			Factory.Save();
			// change client code
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PK>{part.PK}</ns0:PK>
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>{OrgPartRelation.RelationshipTypes.Both}</ns0:Relationship>
        <ns0:OrgHeader>
          <ns0:Code>{client2.OH_Code}</ns0:Code>
          <ns0:PK>{client2.PK}</ns0:PK>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";

			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);

				var logs = GetLogs();
				var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(anotherRelation.PK);
				AssertEquals("Should not have updated client code.", anotherClient.PK, relationInDB.OU_OH);
				AssertContains("Should failed not because of unfinalised ASN lines, but for other reasons.", string.Format("There is no OrgPartRelation with the following values: [OH:{0}][Relationship:{1}].",
					client2.PK, OrgPartRelation.RelationshipTypes.Both), logs);
				ResetLogs();
			}
		}

		#endregion

		#region TestCannotChangeAttributeWithUnfinalisedASNLines

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_PartAttribute1()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1, isUsePartAttribute: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_PartAttribute2()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2, isUsePartAttribute: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_PartAttribute3()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3, isUsePartAttribute: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_SerialNumber()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Serial, OrgPartRelationSchema.OU_UseSerialNumber, isUsePartAttribute: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_PackingDate()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.PackingDate, OrgPartRelationSchema.OU_UsePackingDate, isUsePartAttribute: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_ExpiryDate()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.ExpiryDate, OrgPartRelationSchema.OU_UseExpiryDate, isUsePartAttribute: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_IsPartAttrib1ReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.One, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured, isUsePartAttribute: true, isReleaseCaptured: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_IsPartAttrib2ReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured, isUsePartAttribute: true, isReleaseCaptured: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_IsPartAttrib3ReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured, isUsePartAttribute: true, isReleaseCaptured: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromAbleToDisable_IsSerialNumberReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Serial, OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured, isUsePartAttribute: true, isReleaseCaptured: true, initialSettings: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_PartAttribute1()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_PartAttribute2()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_PartAttribute3()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_SerialNumber()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Serial, OrgPartRelationSchema.OU_UseSerialNumber);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_PackingDate()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.PackingDate, OrgPartRelationSchema.OU_UsePackingDate);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_ExpiryDate()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.ExpiryDate, OrgPartRelationSchema.OU_UseExpiryDate);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_IsPartAttrib1ReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.One, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured, isUsePartAttribute: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_IsPartAttrib2ReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured, isUsePartAttribute: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_IsPartAttrib3ReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured, isUsePartAttribute: true);
		}

		public void TestCannotChangeAttributeWithUnfinalisedASNLinesFromDisableToAble_IsSerialNumberReleaseCaptured()
		{
			TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber.Serial, OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured, isUsePartAttribute: true);
		}

		void TestCannotChangeAttributeWithUnfinalisedASNLines_Core(AttributeNumber attributeNo, SchemaColumn attributeField, bool isUsePartAttribute = false, bool isReleaseCaptured = false, bool initialSettings = false)
		{
			var whs1 = WhsHelper.CreateWarehouse("1", "A");
			var org1 = WhsHelper.CreateClient("ORG1");
			var part = WhsHelper.CreateProduct(org1, "P1");

			var relation = part.RelatedOrganisations[0];
			WhsHelper.SetClientAttributeType(org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(org1, part, attributeNo, isUsePartAttribute, isReleaseCaptured);

			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = WhsHelper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient.PK;
			anotherRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			WhsHelper.SetClientAttributeType(anotherClient, attributeNo, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(anotherClient, part, attributeNo, isUsePartAttribute, isReleaseCaptured, anotherRelation.OU_Relationship);
			Factory.Save();
			var receive = WhsHelper.CreateWhsReceive(org1, whs1, "R1", null);
			WhsHelper.CreateWhsReceiveInventoryLine(receive, part.PK, 0m, "A");
			WhsHelper.CreateAsnLine(receive, part, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			var partRelation = part.RelatedOrganisations.FindFirstByOrganisationPK(org1.PK);
			var anotherPartRelation = part.RelatedOrganisations.FindFirstByOrganisationPK(anotherClient.PK);

			var columnNameWithoutPrefix = attributeField.Name.Replace("OU_", "");

			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""MERGE"">
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:PK>{anotherRelation.PK}</ns0:PK>
        <ns0:Relationship>{anotherRelation.OU_Relationship}</ns0:Relationship>
<ns0:{columnNameWithoutPrefix}>{!initialSettings}</ns0:{columnNameWithoutPrefix}>
          <ns0:OrgHeader>
          <ns0:Code>{anotherClient.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:PK>{relation.PK}</ns0:PK>
        <ns0:Relationship>{relation.OU_Relationship}</ns0:Relationship>
<ns0:{columnNameWithoutPrefix}>{initialSettings}</ns0:{columnNameWithoutPrefix}>
        <ns0:OrgHeader>
          <ns0:Code>{org1.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>"; // No change for Relation with unfinalised ASN Lines

			using (var stream = new MemoryStream(Encoding.Default.GetBytes(xml)))
			{
				manager.Import(stream);
				var logs1 = GetLogs();
				partRelation.Reload();
				anotherPartRelation.Reload();

				AssertEquals("Should not have updated attribute flag.", initialSettings, partRelation[attributeField]);
				AssertEquals("Should have updated attribute flag.", !initialSettings, anotherPartRelation[attributeField]);
				AssertContains($@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 1 updates, 0 deletes", logs1);
			}

			ResetLogs();
			var newXml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""MERGE"">
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:PK>{anotherRelation.PK}</ns0:PK>
        <ns0:Relationship>{anotherRelation.OU_Relationship}</ns0:Relationship>
<ns0:{columnNameWithoutPrefix}>{!initialSettings}</ns0:{columnNameWithoutPrefix}>
          <ns0:OrgHeader>
          <ns0:Code>{anotherClient.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:PK>{relation.PK}</ns0:PK>
        <ns0:Relationship>{relation.OU_Relationship}</ns0:Relationship>
<ns0:{columnNameWithoutPrefix}>{!initialSettings}</ns0:{columnNameWithoutPrefix}>
        <ns0:OrgHeader>
          <ns0:Code>{org1.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>"; // change for Relation with unfinalised ASN Lines

			using (var stream = new MemoryStream(Encoding.Default.GetBytes(newXml)))
			{
				manager.Import(stream);

				var logs2 = GetLogs();
				partRelation.Reload();
				anotherPartRelation.Reload();

				AssertEquals("Should not have updated attribute flag.", initialSettings, partRelation[attributeField]);
				AssertEquals("AnotherRelation setting should not change.", !initialSettings, anotherPartRelation[attributeField]);
				AssertContains($@"Cannot change Attribute settings as there are current ASN line(s) in the warehouse. Client = {org1.OH_Code}.", logs2);
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertEquals(true, receive.IsFinalised);
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			ResetLogs();

			using (var stream = new MemoryStream(Encoding.Default.GetBytes(newXml)))
			{
				manager.Import(stream);
				var logs3 = GetLogs();
				partRelation.Reload();
				AssertEquals("Should have updated attribute flag.", !initialSettings, partRelation[attributeField]);
				AssertEquals("AnotherRelation setting should not change.", !initialSettings, anotherPartRelation[attributeField]);
				AssertContains($@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 1 updates, 0 deletes", logs3);
			}
		}

		#endregion

		#region TestCannotChangeIsReleaseCapturedWithExistingStock

		public void TestCannotChangeIsReleaseCapturedWithExistingStock_PartAttribute1()
		{
			TestCannotChangeIsReleaseCapturedWithExistingStock_Core(AttributeNumber.One, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured);
		}

		public void TestCannotChangeIsReleaseCapturedWithExistingStock_PartAttribute2()
		{
			TestCannotChangeIsReleaseCapturedWithExistingStock_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured);
		}

		public void TestCannotChangeIsReleaseCapturedWithExistingStock_PartAttribute3()
		{
			TestCannotChangeIsReleaseCapturedWithExistingStock_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured);
		}

		void TestCannotChangeIsReleaseCapturedWithExistingStock_Core(AttributeNumber attributeNo, SchemaColumn isReleaseCapturedField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, attributeNo, true, setReleaseCaptured: true);

			var receive = WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var columnNameWithoutPrefix = isReleaseCapturedField.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(columnNameWithoutPrefix, false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated ReleaseCaptured flag.", ZBool.True, partRelation[isReleaseCapturedField]);
			AssertContains($@"Cannot change ReleaseCaptured settings as there is current stock in the warehouse. Client = {data.Org1.OH_Code}.", logs1);

			// Adjust out stock on hand, re-import
			var adjustment = WhsHelper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			WhsHelper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, receive.Lines[0].WE_WL);
			adjustment.FinaliseDocket();
			AssertEquals("Precondition: Adjustment Finalised.", true, adjustment.IsFinalised);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs2 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated ReleaseCaptured flag.", ZBool.False, partRelation[isReleaseCapturedField]);
			AssertNotContains($@"Cannot change ReleaseCaptured settings as there is current stock in the warehouse. Client = {data.Org1.OH_Code}.", logs2);
		}

		public void TestCannotChangeIsReleaseCapturedWithExistingStock_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			var receive = WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var columnNameWithoutPrefix = OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(columnNameWithoutPrefix, false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated ReleaseCaptured flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured]);
			AssertContains($@"Cannot change ReleaseCaptured settings as there is current stock in the warehouse. Client = {data.Org1.OH_Code}.", logs1);

			// Adjust out stock on hand, re-import
			var adjustment = WhsHelper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			WhsHelper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, receive.Lines[0].WE_WL);
			adjustment.FinaliseDocket();
			AssertEquals("Precondition: Adjustment Finalised.", true, adjustment.IsFinalised);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs2 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated ReleaseCaptured flag.", ZBool.False, partRelation[OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured]);
			AssertNotContains($@"Cannot change ReleaseCaptured settings as there is current stock in the warehouse. Client = {data.Org1.OH_Code}.", logs2);
		}

		#endregion

		#region TestCannotDisableUsePartAttribIfReleaseCapturedEnabled

		public void TestCannotDisableUsePartAttribIfReleaseCapturedEnabled_PartAttribute1()
		{
			TestCannotDisableUsePartAttribIfReleaseCapturedEnabled_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured);
		}

		public void TestCannotDisableUsePartAttribIfReleaseCapturedEnabled_PartAttribute2()
		{
			TestCannotDisableUsePartAttribIfReleaseCapturedEnabled_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured);
		}

		public void TestCannotDisableUsePartAttribIfReleaseCapturedEnabled_PartAttribute3()
		{
			TestCannotDisableUsePartAttribIfReleaseCapturedEnabled_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured);
		}

		void TestCannotDisableUsePartAttribIfReleaseCapturedEnabled_Core(AttributeNumber attributeNo, SchemaColumn usePartAttributeField, SchemaColumn isReleaseCapturedField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, attributeNo, true, setReleaseCaptured: true);
			Factory.Save();

			var isReleaseCapturedColumnNameWithoutPrefix = isReleaseCapturedField.Name.Replace("OU_", "");
			var usePartAttribColumnNameWithoutPrefix = usePartAttributeField.Name.Replace("OU_", "");

			// Try importing with invalid release captured flag
			var xml1 = GetWhsProductXML(data.Part1, data.Org1,
				new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false),
				new KeyValuePair<string, bool>(isReleaseCapturedColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml1)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.True, partRelation[usePartAttributeField]);
			AssertContains($@"Cannot ReleaseCapture unused PartAttributes. Client = {data.Org1.OH_Code}.", logs1);

			// Try importing without specifying the release captured flag (can also cause inconsistent state)
			var xml2 = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false));
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml2)));
			var logs2 = GetLogs();

			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.True, partRelation[usePartAttributeField]);
			AssertContains($@"Cannot ReleaseCapture unused PartAttributes. Client = {data.Org1.OH_Code}.", logs2);

			// Try only importing the release captured flag
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, attributeNo, false, setReleaseCaptured: false);
			Factory.Save();

			var xml3 = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(isReleaseCapturedColumnNameWithoutPrefix, true));
			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml3)));
			var logs3 = GetLogs();

			partRelation.Reload();
			AssertEquals("Should *not* have updated ReleaseCaptured flag.", ZBool.False, partRelation[isReleaseCapturedField]);
			AssertContains($@"Cannot ReleaseCapture unused PartAttributes. Client = {data.Org1.OH_Code}.", logs3);

			var newPart = Factory.New<OrgSupplierPart>();
			newPart.OP_PartNum = "P3";
			var xmlForNewPart = GetWhsProductXML(newPart, data.Org1,
				new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false),
				new KeyValuePair<string, bool>(isReleaseCapturedColumnNameWithoutPrefix, true));
			newPart.Delete();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlForNewPart)));
			var logs4 = GetLogs();
			AssertContains($@"Cannot ReleaseCapture unused PartAttributes. Client = {data.Org1.OH_Code}.", logs4);
		}

		public void TestCannotDisableUseSerialNumberIfReleaseCapturedEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Factory.Save();

			var isReleaseCapturedColumnNameWithoutPrefix = OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured.Name.Replace("OU_", "");
			var usePartAttribColumnNameWithoutPrefix = OrgPartRelationSchema.OU_UseSerialNumber.Name.Replace("OU_", "");

			// Try importing with invalid release captured flag
			var xml1 = GetWhsProductXML(data.Part1, data.Org1,
				new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false),
				new KeyValuePair<string, bool>(isReleaseCapturedColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml1)));
			var logs1 = GetLogs();

			var expectedError = $@"Cannot ReleaseCapture unused PartAttributes. Client = {data.Org1.OH_Code}.";
			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UseSerialNumber flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertContains(expectedError, logs1);

			// Try importing without specifying the release captured flag (can also cause inconsistent state)
			var xml2 = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml2)));
			var logs2 = GetLogs();

			partRelation.Reload();
			AssertEquals("Should *not* have updated UseSerialNumber flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertContains(expectedError, logs2);

			// Try only importing the release captured flag
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, false, setReleaseCaptured: false);
			Factory.Save();

			var xml3 = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(isReleaseCapturedColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml3)));
			var logs3 = GetLogs();

			partRelation.Reload();
			AssertEquals("Should *not* have updated IsSerialNumberReleaseCaptured flag.", ZBool.False, partRelation[OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured]);
			AssertContains(expectedError, logs3);
		}

		#endregion

		#region TestEnablingUsePartAttribX_NotEnabledOnClient

		public void TestEnablingUsePartAttrib1_NotEnabledOnClient()
		{
			TestEnablingUsePartAttrib_NotEnabledOnClient_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestEnablingUsePartAttrib2_NotEnabledOnClient()
		{
			TestEnablingUsePartAttrib_NotEnabledOnClient_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestEnablingUsePartAttrib3_NotEnabledOnClient()
		{
			TestEnablingUsePartAttrib_NotEnabledOnClient_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		public void TestEnablingUsePackingDate_NotEnabledOnClient()
		{
			TestEnablingUsePartAttrib_NotEnabledOnClient_Core(AttributeNumber.PackingDate, OrgPartRelationSchema.OU_UsePackingDate);
		}

		public void TestEnablingUseExpiryDate_NotEnabledOnClient()
		{
			TestEnablingUsePartAttrib_NotEnabledOnClient_Core(AttributeNumber.ExpiryDate, OrgPartRelationSchema.OU_UseExpiryDate);
		}

		void TestEnablingUsePartAttrib_NotEnabledOnClient_Core(AttributeNumber attributeNo, SchemaColumn usePartAttributeField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = usePartAttributeField.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.False, partRelation[usePartAttributeField]);
			AssertContains($@"Attribute setup does not match Client = {data.Org1.OH_Code}.", logs1);

			var newPart = Factory.New<OrgSupplierPart>();
			newPart.OP_PartNum = "P3";
			var xmlForNewPart = GetWhsProductXML(newPart, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, true));
			newPart.Delete();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlForNewPart)));
			var logs2 = GetLogs();
			AssertContains($@"Attribute setup does not match Client = {data.Org1.OH_Code}.", logs2);

			// Enable attribute on client, re-import
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs3 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated UsePartAttrib flag.", ZBool.True, partRelation[usePartAttributeField]);
			AssertNotContains($@"Attribute setup does not match Client = {data.Org1.OH_Code}.", logs3);
		}

		public void TestEnableUsePartAttribWithEntityToDeleteShouldNotFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""DELETE"">
        <ns0:PK>{partRelation.PK}</ns0:PK>
				<ns0:Relationship>OWN</ns0:Relationship>
        <ns0:UsePartAttrib1>true</ns0:UsePartAttrib1>
				<ns0:OrgHeader>
          <ns0:Code>{data.Org1.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertNotContains($@"Attribute setup does not match Client = {data.Org1.OH_Code}.", logs);
			AssertNull("Part Relation should have been deleted.", new BusinessObjectFactory().Load<OrgPartRelation>(partRelation.PK));
		}

		public void TestReleaseCaptureMismatchWithEntityToDeleteShouldNotFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""DELETE"">
        <ns0:PK>{partRelation.PK}</ns0:PK>
				<ns0:Relationship>OWN</ns0:Relationship>
        <ns0:UsePartAttrib1>false</ns0:UsePartAttrib1>
				<ns0:IsPartAttrib1ReleaseCaptured>true</ns0:IsPartAttrib1ReleaseCaptured>
				<ns0:OrgHeader>
          <ns0:Code>{data.Org1.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertNotContains($@"Cannot ReleaseCapture unused PartAttributes. Client = {data.Org1.OH_Code}.", logs);
			AssertNull("Part Relation should have been deleted.", new BusinessObjectFactory().Load<OrgPartRelation>(partRelation.PK));
		}

		public void TestEnablingUseSerial_NotEnabledOnClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = OrgPartRelationSchema.OU_UseSerialNumber.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UseSerialNumber flag.", ZBool.False, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertContains($@"Attribute setup does not match Client = {data.Org1.OH_Code}.", logs1);

			// Enable attribute on client, re-import
			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, PartAttributeTypeList.Codes.Mandatory);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs2 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated UseSerialNumber flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
		}

		#endregion

		#region TestEnablingUsePartAttribX_Mandatory

		public void TestEnablingUsePartAttrib1_Mandatory()
		{
			TestEnablingUsePartAttrib_Mandatory_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestEnablingUsePartAttrib2_Mandatory()
		{
			TestEnablingUsePartAttrib_Mandatory_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestEnablingUsePartAttrib3_Mandatory()
		{
			TestEnablingUsePartAttrib_Mandatory_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		public void TestEnablingUsePackingDate_Mandatory()
		{
			TestEnablingUsePartAttrib_Mandatory_Core(AttributeNumber.PackingDate, OrgPartRelationSchema.OU_UsePackingDate);
		}

		public void TestEnablingUseExpiryDate_Mandatory()
		{
			TestEnablingUsePartAttrib_Mandatory_Core(AttributeNumber.ExpiryDate, OrgPartRelationSchema.OU_UseExpiryDate);
		}

		void TestEnablingUsePartAttrib_Mandatory_Core(AttributeNumber attributeNo, SchemaColumn usePartAttributeField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);

			var receive = WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = usePartAttributeField.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.False, partRelation[usePartAttributeField]);
			AssertContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs1);

			// Adjust out stock on hand, re-import
			var adjustment = WhsHelper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			WhsHelper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, receive.Lines[0].WE_WL);
			adjustment.FinaliseDocket();
			AssertEquals("Precondition: Adjustment Finalised.", true, adjustment.IsFinalised);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs2 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated UsePartAttrib flag.", ZBool.True, partRelation[usePartAttributeField]);
			AssertNotContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs2);
		}

		public void TestEnablingUseSerialNumber_Mandatory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, PartAttributeTypeList.Codes.Mandatory);

			var receive = WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = OrgPartRelationSchema.OU_UseSerialNumber.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.False, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs1);

			// Adjust out stock on hand, re-import
			var adjustment = WhsHelper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			WhsHelper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, receive.Lines[0].WE_WL);
			adjustment.FinaliseDocket();
			AssertEquals("Precondition: Adjustment Finalised.", true, adjustment.IsFinalised);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs2 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated UsePartAttrib flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertNotContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs2);
		}

		#endregion

		#region TestEnablingUsePartAttribX_NonMandatory

		public void TestEnablingUsePartAttrib1_NonMandatory()
		{
			TestEnablingUsePartAttrib_NonMandatory_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestEnablingUsePartAttrib2_NonMandatory()
		{
			TestEnablingUsePartAttrib_NonMandatory_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestEnablingUsePartAttrib3_NonMandatory()
		{
			TestEnablingUsePartAttrib_NonMandatory_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		void TestEnablingUsePartAttrib_NonMandatory_Core(AttributeNumber attributeNo, SchemaColumn usePartAttributeField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.NonMandatory);

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = usePartAttributeField.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, true));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should have updated UsePartAttrib flag.", ZBool.True, partRelation[usePartAttributeField]);
			AssertNotContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs1);
		}

		#endregion

		#region TestDisablingUsePartAttrib_Mandatory

		public void TestDisablingUsePartAttrib1_Mandatory()
		{
			TestDisablingUsePartAttrib_Mandatory_Core(AttributeNumber.One, WhsDocketLineSchema.WE_PartAttrib1, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestDisablingUsePartAttrib2_Mandatory()
		{
			TestDisablingUsePartAttrib_Mandatory_Core(AttributeNumber.Two, WhsDocketLineSchema.WE_PartAttrib2, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestDisablingUsePartAttrib3_Mandatory()
		{
			TestDisablingUsePartAttrib_Mandatory_Core(AttributeNumber.Three, WhsDocketLineSchema.WE_PartAttrib3, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		public void TestDisablingUsePackingDate_Mandatory()
		{
			TestDisablingUsePartAttrib_Mandatory_Core(AttributeNumber.PackingDate, WhsDocketLineSchema.WE_PackingDate, OrgPartRelationSchema.OU_UsePackingDate);
		}

		public void TestDisablingUseExpiryDate_Mandatory()
		{
			TestDisablingUsePartAttrib_Mandatory_Core(AttributeNumber.ExpiryDate, WhsDocketLineSchema.WE_ExpiryDate, OrgPartRelationSchema.OU_UseExpiryDate);
		}

		void TestDisablingUsePartAttrib_Mandatory_Core(AttributeNumber attributeNo, SchemaColumn inventoryColumn, SchemaColumn usePartAttributeField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, attributeNo, true);

			var receive = WhsHelper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = WhsHelper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receiveLine[inventoryColumn] = inventoryColumn is SchemaDateTimeColumn ? ZDateTime.Today : "AAA";
			receive.FinaliseDocket();
			AssertEquals("Precondition.", true, receive.IsFinalised);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = usePartAttributeField.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.True, partRelation[usePartAttributeField]);
			AssertContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs1);

			// Adjust out stock on hand, re-import
			var adjustment = WhsHelper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var line = adjustment.CreateDocketLineFromInventory(receiveLine.Inventory[0]);
			line.WE_ReasonCode = "SHR";
			adjustment.FinaliseDocket();
			AssertEquals("Precondition: Adjustment Finalised.", true, adjustment.IsFinalised);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs2 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated UsePartAttrib flag.", ZBool.False, partRelation[usePartAttributeField]);
			AssertNotContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs2);
		}

		public void TestDisablingUseSerialNumber_RegistryOn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = WhsHelper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = WhsHelper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine[WhsDocketLineSchema.WE_SerialNumber] = "AAA";
			receive.FinaliseDocket();
			AssertEquals("Precondition.", true, receive.IsFinalised);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = OrgPartRelationSchema.OU_UseSerialNumber.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UseSerialNumber flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs1);

			// Adjust out stock on hand, re-import
			var adjustment = WhsHelper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			var line = adjustment.CreateDocketLineFromInventory(receiveLine.Inventory[0]);
			line.WE_ReasonCode = "SHR";
			adjustment.FinaliseDocket();
			AssertEquals("Precondition: Adjustment Finalised.", true, adjustment.IsFinalised);
			Factory.Save();

			ResetLogs();
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs2 = GetLogs();
			partRelation.Reload();
			AssertEquals("Should have updated UseSerialNumber flag.", ZBool.False, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertNotContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs2);
		}

		#endregion

		#region TestDisablingUsePartAttrib_NonMandatory

		public void TestDisablingUsePartAttrib1_NonMandatory()
		{
			TestDisablingUsePartAttrib_NonMandatory_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestDisablingUsePartAttrib2_NonMandatory()
		{
			TestDisablingUsePartAttrib_NonMandatory_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestDisablingUsePartAttrib3_NonMandatory()
		{
			TestDisablingUsePartAttrib_NonMandatory_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		void TestDisablingUsePartAttrib_NonMandatory_Core(AttributeNumber attributeNo, SchemaColumn usePartAttributeField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.NonMandatory);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, attributeNo, true);

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var usePartAttribColumnNameWithoutPrefix = usePartAttributeField.Name.Replace("OU_", "");
			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs1 = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should have updated UsePartAttrib flag as no stock is using the part attribute.", ZBool.False, partRelation[usePartAttributeField]);
			AssertNotContains($@"Inventory exists that is incompatible with the PartAttribute setup. Client = {data.Org1.OH_Code}", logs1);
		}

		#endregion

		#region TestDisablingUsePartAttrib_ReleaseCaptured

		public void TestDisablingUsePartAttrib1_ReleaseCaptured()
		{
			TestDisablingUsePartAttrib_ReleaseCaptured_Core(AttributeNumber.One, OrgPartRelationSchema.OU_UsePartAttrib1, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured);
		}

		public void TestDisablingUsePartAttrib2_ReleaseCaptured()
		{
			TestDisablingUsePartAttrib_ReleaseCaptured_Core(AttributeNumber.Two, OrgPartRelationSchema.OU_UsePartAttrib2, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured);
		}

		public void TestDisablingUsePartAttrib3_ReleaseCaptured()
		{
			TestDisablingUsePartAttrib_ReleaseCaptured_Core(AttributeNumber.Three, OrgPartRelationSchema.OU_UsePartAttrib3, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured);
		}

		void TestDisablingUsePartAttrib_ReleaseCaptured_Core(AttributeNumber attributeNo, SchemaColumn usePartAttributeField, SchemaColumn isReleaseCapturedField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, attributeNo, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, attributeNo, true, setReleaseCaptured: true);
			Factory.Save();

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var isReleaseCapturedColumnNameWithoutPrefix = isReleaseCapturedField.Name.Replace("OU_", "");
			var usePartAttribColumnNameWithoutPrefix = usePartAttributeField.Name.Replace("OU_", "");

			var xml = GetWhsProductXML(data.Part1, data.Org1,
				new KeyValuePair<string, bool>(usePartAttribColumnNameWithoutPrefix, false),
				new KeyValuePair<string, bool>(isReleaseCapturedColumnNameWithoutPrefix, false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.True, partRelation[usePartAttributeField]);
			AssertEquals("Should *not* have updated IsReleaseCaptured flag.", ZBool.True, partRelation[isReleaseCapturedField]);
		}

		public void TestDisablingUseSerialNumber_ReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Factory.Save();

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var xml = GetWhsProductXML(data.Part1, data.Org1,
				new KeyValuePair<string, bool>("UseSerialNumber", false),
				new KeyValuePair<string, bool>("IsSerialNumberReleaseCaptured", false));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should *not* have updated UsePartAttrib flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_UseSerialNumber]);
			AssertEquals("Should *not* have updated IsReleaseCaptured flag.", ZBool.True, partRelation[OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured]);
		}

		#endregion

		#region TestRFAttributeConfirm_Serialnumber

		public void TestRFAttributeConfirm_Serialnumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, PartAttributeTypeList.Codes.Mandatory);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = WhsHelper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = WhsHelper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SER#";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition: receive is finalised", true, receive.IsFinalised);

			Factory.Save();

			var xml = GetWhsProductXML(data.Part1, data.Org1, new KeyValuePair<string, string>("RFAttributeConfirm", "SER"));
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.Reload();
			AssertEquals("Should have updated UsePartAttrib flag as registry is on.", RFAttributeConfirmCode.Codes.SerialNumber, partRelation[OrgPartRelationSchema.OU_RFAttributeConfirm]);
		}

		#endregion

		#region TestOrgPartBOM

		#region TestOrgPartBOM_Insert

		public void TestOrgPartBOM_Insert_BasicInformation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 1 inserts, 0 updates, 0 deletes", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNotNull(bomPart);
			AssertEquals(15m, bomPart.OE_ComponentQty);
			AssertEquals(data.Part1.PK, bomPart.OE_OP_MainProduct);
			AssertEquals(bomProduct.PK, bomPart.OE_OP_Component);
		}

		public void TestOrgPartBOM_Insert_BasicInformation_MatchComponentByPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PK>{bomProduct.PK}</ns0:PK>");

			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 1 inserts, 0 updates, 0 deletes", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNotNull(bomPart);
			AssertEquals(15m, bomPart.OE_ComponentQty);
			AssertEquals(data.Part1.PK, bomPart.OE_OP_MainProduct);
			AssertEquals(bomProduct.PK, bomPart.OE_OP_Component);
		}

		public void TestOrgPartBOM_Insert_Fail_MissingComponent()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgPartBOM without PK or Component definition.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_MissingComponentPKAndPartNum()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to match OrgSupplierPart without PK or Product Partnum/Owner pair defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_MultipleRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");

			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart with multiple owner OrgPartRelations defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_NoRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart without owner OrgPartRelation defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_RelationDoesNotSpecifyRelationship()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgPartRelation must specify Relationship.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_RelationIsNotOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>SUP</ns0:Relationship>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgPartRelation must be an Owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_MissingOrgHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgSupplierPart OrgPartRelation must include a valid OrgHeader.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_CannotMatchOrgHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>ABC</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgSupplierPart OrgPartRelation must include a valid OrgHeader.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_ComponentDoesNotShareOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var owner2 = WhsHelper.CreateClient("OWN2");
			var bomProduct = WhsHelper.CreateProduct("BOM", owner2);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{owner2.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot import product as Component owner is not an owner of P1.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		public void TestOrgPartBOM_Insert_Fail_ComponentDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>ABC</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot import product as OrgSupplierPart ABC cannot be matched with existing definitions.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var bomPart = Factory.Load<OrgPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNull(bomPart);
		}

		#endregion

		#region TestOrgPartBOM_Update

		public void TestOrgPartBOM_Update_BasicInformation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 0 inserts, 1 updates, 0 deletes", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(15m, bomPartInOtherFactory.OE_ComponentQty);
			AssertEquals(data.Part1.PK, bomPartInOtherFactory.OE_OP_MainProduct);
			AssertEquals(bomProduct.PK, bomPartInOtherFactory.OE_OP_Component);
		}

		public void TestOrgPartBOM_Update_BasicInformation_MatchComponentByPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PK>{bomProduct.PK}</ns0:PK>");

			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 0 inserts, 1 updates, 0 deletes", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(15m, bomPartInOtherFactory.OE_ComponentQty);
			AssertEquals(data.Part1.PK, bomPartInOtherFactory.OE_OP_MainProduct);
			AssertEquals(bomProduct.PK, bomPartInOtherFactory.OE_OP_Component);
		}

		public void TestOrgPartBOM_Update_MissingComponent()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 0 inserts, 1 updates, 0 deletes", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(15m, bomPartInOtherFactory.OE_ComponentQty);
			AssertEquals(data.Part1.PK, bomPartInOtherFactory.OE_OP_MainProduct);
			AssertEquals(bomProduct.PK, bomPartInOtherFactory.OE_OP_Component);
		}

		public void TestOrgPartBOM_Update_Fail_MissingComponentPKAndPartNum()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to match OrgSupplierPart without PK or Product Partnum/Owner pair defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(10m, bomPartInOtherFactory.OE_ComponentQty);
		}

		public void TestOrgPartBOM_Update_Fail_MultipleRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");

			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart with multiple owner OrgPartRelations defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(10m, bomPartInOtherFactory.OE_ComponentQty);
		}

		public void TestOrgPartBOM_Update_Fail_NoRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart without owner OrgPartRelation defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(10m, bomPartInOtherFactory.OE_ComponentQty);
		}

		public void TestOrgPartBOM_Update_Fail_RelationDoesNotSpecifyRelationship()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgPartRelation must specify Relationship.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(10m, bomPartInOtherFactory.OE_ComponentQty);
		}

		public void TestOrgPartBOM_Update_Fail_RelationIsNotOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>SUP</ns0:Relationship>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgPartRelation must be an Owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(10m, bomPartInOtherFactory.OE_ComponentQty);
		}

		public void TestOrgPartBOM_Update_Fail_MissingOrgHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgSupplierPart OrgPartRelation must include a valid OrgHeader.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(10m, bomPartInOtherFactory.OE_ComponentQty);
		}

		public void TestOrgPartBOM_Update_Fail_CannotMatchOrgHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "BAG");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>ABC</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgSupplierPart OrgPartRelation must include a valid OrgHeader.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);

			var newFactory = new BusinessObjectFactory();
			var bomPartInOtherFactory = newFactory.Load<OrgPartBOM>(orgPartBOM.PK);
			AssertNotNull(bomPartInOtherFactory);
			AssertEquals(10m, bomPartInOtherFactory.OE_ComponentQty);
		}

		#endregion

		#region TestOrgPartBOMProductPickedOnSalesOrder

		public void TestOrgPartBOMProductPickedOnSalesOrder_Insert_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_Insert_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_Insert_CanNotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_Insert_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_Insert_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "BAG");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct2.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(It.IsAny<BusinessObjectFactory>(), data.Part1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 1 inserts, 0 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: P1
Test error: Cannot change the BOM composition for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_Delete_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_Delete_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_Delete_CanNotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_Delete_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_Delete_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "BAG");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "DELETE"));
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct1.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(It.IsAny<BusinessObjectFactory>(), data.Part1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 0 inserts, 0 updates, 1 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: P1
Test error: Cannot change the BOM composition for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_MergeWithEmptyPK_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_MergeWithEmptyPK_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_MergeWithEmptyPK_CanNotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_MergeWithEmptyPK_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_MergeWithEmptyPK_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "BAG");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "MERGE"));
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct2.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(It.IsAny<BusinessObjectFactory>(), data.Part1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 1 inserts, 0 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: P1
Test error: Cannot change the BOM composition for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_InvalidPK()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_InvalidPK_Core();
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_InvalidPK_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "BAG");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "MERGE"));

			var obscureGuid = Guid.NewGuid();
			xmlBuilder.AppendLine($"<ns0:PK>{obscureGuid}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct2.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");
			xmlBuilder.AppendLine(CloseBOMProductXML);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(It.IsAny<BusinessObjectFactory>(), data.Part1.PK)).Returns(true);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			}
			var logs = GetLogs();

			AssertEquals($@"Importing Product: P1
Test error: Cannot finding matching OrgPartBOM with PK '{obscureGuid}'.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_ComponentQty_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_ComponentQty_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_ComponentQty_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_ComponentQty_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_ComponentQty_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "BAG");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "MERGE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>15.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct1.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(It.IsAny<BusinessObjectFactory>(), data.Part1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 0 inserts, 1 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: P1
Test error: Cannot change the BOM composition for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_PackType_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_PackType_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_PackType_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_PackType_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_PackType_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "BAG");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "MERGE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>10.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct1.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>PLT</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(It.IsAny<BusinessObjectFactory>(), data.Part1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 0 inserts, 1 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: P1
Test error: Cannot change the BOM composition for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_Component_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_Component_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_Component_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_Component_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_ModifyingCriticalField_Component_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "BAG");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "MERGE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQty>10.000</ns0:ComponentQty>");
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct2.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>PLT</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(It.IsAny<BusinessObjectFactory>(), data.Part1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 0 inserts, 1 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: P1
Test error: Cannot change the BOM composition for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		#endregion

		#region TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_MergeWithEmptyPK_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_MergeWithEmptyPK_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_MergeWithEmptyPK_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_MergeWithEmptyPK_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_MergeWithEmptyPK_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var xml = GetWhsProductUnitConversion_XML(bomProduct1.OP_PartNum, "UNT", "BOX", orgPartPK: bomProduct1.PK);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), bomProduct1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: BOM1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 1 inserts, 0 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: BOM1
Test error: Cannot change the unit conversions for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_InvalidPK_IsUsedToBuiltKitOnSalesOrder_True()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_InvalidPK_Core(isUsedToBuiltKitOnSalesOrder: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_InvalidPK_IsUsedToBuiltKitOnSalesOrder_False()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_InvalidPK_Core(isUsedToBuiltKitOnSalesOrder: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_InvalidPK_Core(bool isUsedToBuiltKitOnSalesOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartUnit = whsHelper.CreateProductUnit(bomProduct1, "BOX", 5m);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var obscureGuid = ZGuid.NewZGuid();
			var xml = GetWhsProductUnitConversion_XML(bomProduct1.OP_PartNum, "UNT", "BOX", orgPartPK: bomProduct1.PK, partUnitPK: obscureGuid);
			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), bomProduct1.PK)).Returns(isUsedToBuiltKitOnSalesOrder);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			}
			var logs = GetLogs();

			if (isUsedToBuiltKitOnSalesOrder)
			{
				AssertEquals(@$"Importing Product: BOM1
Test error: Cannot change the unit conversions for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
			else
			{
				AssertEquals(@"Importing Product: BOM1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 0 updates, 0 deletes", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_Delete_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_Delete_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_Delete_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_Delete_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_Delete_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartUnit = whsHelper.CreateProductUnit(bomProduct1, "BOX", 5);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var xml = GetWhsProductUnitConversionForDelete_XML(bomProduct1.OP_PartNum, bomProduct1.PK, orgPartUnit.PK);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), bomProduct1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: BOM1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 0 updates, 1 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: BOM1
Test error: Cannot change the unit conversions for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_ParentPackType_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_ParentPackType_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_ParentPackType_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_ParentPackType_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_ParentPackType_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartUnit = whsHelper.CreateProductUnit(bomProduct1, "BOX", 5);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var xml = GetWhsProductUnitConversion_XML(bomProduct1.OP_PartNum, "UNT", "CAS", orgPartPK: bomProduct1.PK, partUnitPK: orgPartUnit.PK);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), bomProduct1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: BOM1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 1 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: BOM1
Test error: Cannot change the unit conversions for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_PackType_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_PackType_Core(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_PackType_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_PackType_Core(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_PackType_Core(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartUnit = whsHelper.CreateProductUnit(bomProduct1, "BOX", 5);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var xml = GetWhsProductUnitConversion_XML(bomProduct1.OP_PartNum, "CAS", "BOX", orgPartPK: bomProduct1.PK, partUnitPK: orgPartUnit.PK);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), bomProduct1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: BOM1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 1 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: BOM1
Test error: Cannot change the unit conversions for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_QuantityInParent_CanUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_QuantityInParent(canUpdate: true);
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_QuantityInParent_CannotUpdate()
		{
			TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_QuantityInParent(canUpdate: false);
		}

		void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingCriticalField_QuantityInParent(bool canUpdate)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartUnit = whsHelper.CreateProductUnit(bomProduct1, "BOX", 5m);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var xml = GetWhsProductUnitConversion_XML(bomProduct1.OP_PartNum, "UNT", "BOX", orgPartPK: bomProduct1.PK, partUnitPK: orgPartUnit.PK, quantityInParent: 99m);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), bomProduct1.PK)).Returns(!canUpdate);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			}
			var logs = GetLogs();

			if (canUpdate)
			{
				AssertEquals(@"Importing Product: BOM1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 1 updates, 0 deletes", logs);
			}
			else
			{
				AssertEquals(@$"Importing Product: BOM1
Test error: Cannot change the unit conversions for this product.
This product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
			}
		}

		public void TestOrgPartBOMProductPickedOnSalesOrder_OrgPartUnitOnComponent_ModifyingNonCriticalFields()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartUnit = whsHelper.CreateProductUnit(bomProduct1, "BOX", 5m);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var orgPartUnitCollection = $@"<ns0:OrgPartUnitCollection>
<ns0:OrgPartUnit Action=""MERGE"">
	<ns0:PK>{orgPartUnit.PK}</ns0:PK>
	<ns0:PackType>UNT</ns0:PackType>
	<ns0:Weight>1.000</ns0:Weight>
	<ns0:Height>2.000</ns0:Height>
	<ns0:Width>3.000</ns0:Width>
	<ns0:Depth>4.000</ns0:Depth>
	<ns0:Cubic>5.000</ns0:Cubic>
	<ns0:ParentPackType>BOX</ns0:ParentPackType>
	<ns0:QuantityInParent>5.0000</ns0:QuantityInParent>
	<ns0:NoOfSKUsInThisPack>6.000</ns0:NoOfSKUsInThisPack>
</ns0:OrgPartUnit>
</ns0:OrgPartUnitCollection>";

			var xml = GetOpeningProductSection(bomProduct1.OP_PartNum, bomProduct1.PK) + GetOrgPartRelationSection("") + orgPartUnitCollection + GetClosingProductSection();

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(It.IsAny<BusinessObjectFactory>(), bomProduct1.PK)).Returns(true);
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			}
			var logs = GetLogs();

			AssertEquals(@"Importing Product: BOM1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 1 updates, 0 deletes", logs);
		}

		#endregion

		#region TestOrgPartBOMProduct_InvalidInsertion

		public void TestOrgPartBOMProduct_InvalidInsertion_MainProductIsSetToCanPickWithoutWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);

			bomProduct1.OP_IsComponentPickedOnSalesOrder = true;
			WhsHelper.CreateProductBOM(bomProduct1, data.Part1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct2.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));

			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Component can not be added to this product, because its main product is set to 'Can Pick without Work Order'
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgPartBOMProduct_InvalidInsertion_ComponentHasChildComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);

			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			WhsHelper.CreateProductBOM(bomProduct1, bomProduct2);

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:Component TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{bomProduct1.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:Component>");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>BAG</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine(CloseBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));

			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Component can not be added to this product, because it has child components.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		#endregion

		string StartBOMProductXML(OrgSupplierPart part, OrgHeader org, string bomAction) =>
$@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""MERGE"">
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
		<ns0:OrgHeader>
          <ns0:Code>{org.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
	<ns0:OrgPartBOMCollection>
		<ns0:OrgPartBOM Action=""{bomAction}"">";

		string CloseBOMProductXML => @"
		</ns0:OrgPartBOM>
	</ns0:OrgPartBOMCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";

		#endregion

		#region TestOrgSecondaryPartBOM

		#region TestOrgSecondaryPartBOM_Insert

		public void TestOrgSecondaryPartBOM_Insert_BasicInformation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 3m, "UNT", "BOM", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 1 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOMPivot - 1 inserts, 0 updates, 0 deletes", logs);

			var secondaryBomPart = Factory.Load<OrgSecondaryPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNotNull(secondaryBomPart);
			AssertEquals(5m, secondaryBomPart.OSB_ProductQuantity);
			AssertEquals(data.Part1.PK, secondaryBomPart.OSB_OP_MainProduct);
			AssertEquals(secondaryProduct.PK, secondaryBomPart.OSB_OP_SecondaryProduct);

			var secondaryBomPartPivot = Factory.Load<OrgSecondaryPartBOMPivot>(new ZQuery()).SingleOrDefault();
			AssertNotNull(secondaryBomPartPivot);
			AssertEquals(3m, secondaryBomPartPivot.OPP_ComponentQuantity);
			AssertEquals(secondaryBomPart.PK, secondaryBomPartPivot.OPP_OSB_SecondaryPart);
			AssertEquals(orgPartBOM.PK, secondaryBomPartPivot.OPP_OE_Component);
		}

		public void TestOrgSecondaryPartBOM_Insert_BasicInformation_MatchComponentByPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");
			xmlBuilder.AppendLine($"<ns0:PK>{secondaryProduct.PK}</ns0:PK>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivot Action=\"INSERT\">");

			xmlBuilder.AppendLine("<ns0:ComponentQuantity>3.000</ns0:ComponentQuantity>");
			xmlBuilder.AppendLine("<ns0:ComponentOrgPartBOM TableName=\"OrgPartBOM\">");
			xmlBuilder.AppendLine($"<ns0:PK>{orgPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("</ns0:ComponentOrgPartBOM>");

			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 1 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOMPivot - 1 inserts, 0 updates, 0 deletes", logs);

			var secondaryBomPart = Factory.Load<OrgSecondaryPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNotNull(secondaryBomPart);
			AssertEquals(5m, secondaryBomPart.OSB_ProductQuantity);
			AssertEquals(data.Part1.PK, secondaryBomPart.OSB_OP_MainProduct);
			AssertEquals(secondaryProduct.PK, secondaryBomPart.OSB_OP_SecondaryProduct);

			var secondaryBomPartPivot = Factory.Load<OrgSecondaryPartBOMPivot>(new ZQuery()).SingleOrDefault();
			AssertNotNull(secondaryBomPartPivot);
			AssertEquals(3m, secondaryBomPartPivot.OPP_ComponentQuantity);
			AssertEquals(secondaryBomPart.PK, secondaryBomPartPivot.OPP_OSB_SecondaryPart);
			AssertEquals(orgPartBOM.PK, secondaryBomPartPivot.OPP_OE_Component);
		}

		public void TestOrgSecondaryPartBOM_Insert_MainProductHasPickWithoutWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Secondary part can not be added to this product, because the main product is set to 'Can Pick without Work Order'.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryProductHasPickWithoutWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			secondaryProduct.OP_IsComponentPickedOnSalesOrder = true;

			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Secondary part can not be added to this product if it is set to 'Can Pick without Work Order'.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryProductHasPickWithoutWorkOrder_OnAnotherParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var otherproduct = WhsHelper.CreateProduct("Other", data.Org1);
			otherproduct.OP_IsComponentPickedOnSalesOrder = true;

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			var otherOrgPartBOM = WhsHelper.CreateProductBOM(otherproduct, secondaryProduct, 10m, "UNT");

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Secondary part can not be added to this product if it is set to 'Can Pick without Work Order'.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryProductIsMainProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", data.Part1.OP_PartNum, data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot Select the Main Product as a Secondary Product for BOM.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_MainProductMustHaveBillOfMaterials()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: You cannot add Secondary Parts if Main Product has no Bill of Materials.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryProductHasABillOfMaterials()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct1 = WhsHelper.CreateProduct("BOM1", data.Org1);
			var bomProduct2 = WhsHelper.CreateProduct("BOM2", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM1 = WhsHelper.CreateProductBOM(data.Part1, bomProduct1, 10m, "UNT");
			var orgPartBOM2 = WhsHelper.CreateProductBOM(secondaryProduct, bomProduct2, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: You cannot add Secondary Parts if it has Bill of Materials.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_PivotsTotalMatchesComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var secondaryProduct2 = WhsHelper.CreateProduct("SEC2", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC2", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 10m, "UNT", "BOM", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 1 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOMPivot - 1 inserts, 0 updates, 0 deletes", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_PivotsTotalExceedsComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var secondaryProduct2 = WhsHelper.CreateProduct("SEC2", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC2", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 20m, "UNT", "BOM", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot have Total Component Quantity used (20) greater than the Total Component Stock Quantity (10) on the BOM Component.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryPartMissing()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");
			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot create new OrgSecondaryPartBOM without Secondary Part definition.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryPartCannotBeMatched_PK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");
			var randomGuid = Guid.NewGuid();
			xmlBuilder.AppendLine($"<ns0:PK>{randomGuid}</ns0:PK>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals($@"Importing Product: P1
Test error: Cannot import product as OrgSupplierPart with PK {randomGuid} cannot be matched with existing definitions.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryPartCannotBeMatched_MissingProductPartNum()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine($"<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to match OrgSupplierPart without PK or Product Partnum/Owner pair defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryPartCannotBeMatched_MissingProductRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");
			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart without owner OrgPartRelation defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryPartCannotBeMatched_UnableToMatchRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine("<ns0:Code>ABC</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgSupplierPart OrgPartRelation must include a valid OrgHeader.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryPartCannotBeMatched_IsNotOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>SUP</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgPartRelation must be an Owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryPartCannotBeMatched_TooManyRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherClient = WhsHelper.CreateClient("C2");
			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>SUP</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{otherClient.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart with multiple owner OrgPartRelations defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryProductUsedMultipleTimes()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 10m);
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot Select the same Secondary Product twice.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_SecondaryProductUsedMultipleTimesInXML()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");
			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOM>");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOM Action=\"INSERT\">");
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");
			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot Select the same Secondary Product twice.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMMissing()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivot Action=\"INSERT\">");
			xmlBuilder.AppendLine("<ns0:ComponentQuantity>2.000</ns0:ComponentQuantity>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to import an OrgSecondaryPartBOM without a PK or ComponentOrgPartBOM definition.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMCannotBeMatched_Product()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			var otherBomProduct = WhsHelper.CreateProduct("BOM2", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "BAG", "BOM2", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot import product as OrgSupplierPart SEC cannot be matched with existing definitions.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMCannotBeMatched_PackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "BAG", "BOM1", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to match OrgPartBom with provided Component 'BOM1' and Pack Type 'BAG'.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMCannotBeMatched_NotAnOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "BAG", "BOM1", data.Org1.OH_Code, "SUP");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgPartRelation must be an Owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMCannotBeMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			WhsHelper.CreateProductUnit(bomProduct, "BAG", 3m);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "BAG", "BOM1", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to match OrgPartBom with provided Component 'BOM1' and Pack Type 'BAG'.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMCannotBeusedMoreThanOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");
			var orgSecondaryPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);

			var pivot = orgSecondaryPartBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 1m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgSecondaryPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "UNT", "BOM1", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot Select the same BOM Component twice.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMCannotBeusedMoreThanOnceInXML()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "UNT", "BOM1", data.Org1.OH_Code, "OWN");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "UNT", "BOM1", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot Select the same BOM Component twice.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Insert_ComponentOrgPartBOMWithSecondaryPartBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "INSERT"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "INSERT", null, 2m, "UNT", "BOM", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOM>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMCollection>");

			// Insert a BOM
			xmlBuilder.AppendLine("<ns0:OrgPartBOMCollection>");
			xmlBuilder.AppendLine("<ns0:OrgPartBOM Action=\"INSERT\">");
			xmlBuilder.AppendLine("<ns0:ComponentQty>10.000</ns0:ComponentQty>");
			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "Component", "BOM", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine("<ns0:Code>UNT</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine("</ns0:OrgPartBOM>");
			xmlBuilder.AppendLine("</ns0:OrgPartBOMCollection>");

			xmlBuilder.AppendLine("</ns0:OrgSupplierPart>");
			xmlBuilder.AppendLine("</ns0:Product>");

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartBOM - 1 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 1 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOMPivot - 1 inserts, 0 updates, 0 deletes", logs);
		}

		#endregion

		#region TestOrgSecondaryPartBOM_Update

		public void TestOrgSecondaryPartBOM_Update_BasicInformation_MatchComponentByPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>10.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");
			xmlBuilder.AppendLine($"<ns0:PK>{secondaryProduct.PK}</ns0:PK>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivot Action=\"UPDATE\">");

			xmlBuilder.AppendLine($"<ns0:PK>{pivot.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQuantity>5.000</ns0:ComponentQuantity>");

			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 0 inserts, 1 updates, 0 deletes
OrgSecondaryPartBOMPivot - 0 inserts, 1 updates, 0 deletes", logs);

			var otherFactory = new BusinessObjectFactory();
			var secondaryBomPart = otherFactory.Load<OrgSecondaryPartBOM>(new ZQuery()).SingleOrDefault();
			AssertNotNull(secondaryBomPart);
			AssertEquals(10m, secondaryBomPart.OSB_ProductQuantity);
			AssertEquals(data.Part1.PK, secondaryBomPart.OSB_OP_MainProduct);
			AssertEquals(secondaryProduct.PK, secondaryBomPart.OSB_OP_SecondaryProduct);

			var secondaryBomPartPivot = otherFactory.Load<OrgSecondaryPartBOMPivot>(new ZQuery()).SingleOrDefault();
			AssertNotNull(secondaryBomPartPivot);
			AssertEquals(5m, secondaryBomPartPivot.OPP_ComponentQuantity);
			AssertEquals(secondaryBomPart.PK, secondaryBomPartPivot.OPP_OSB_SecondaryPart);
			AssertEquals(orgPartBOM.PK, secondaryBomPartPivot.OPP_OE_Component);
		}

		public void TestOrgSecondaryPartBOM_Update_PivotsTotalMatchesComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC1", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivot Action=\"UPDATE\">");
			xmlBuilder.AppendLine($"<ns0:PK>{pivot.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQuantity>10</ns0:ComponentQuantity>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOMPivot - 0 inserts, 1 updates, 0 deletes", logs);
		}

		public void TestOrgSecondaryPartBOM_Update_PivotsTotalExceedsComponentStockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);

			var secondaryProduct1 = WhsHelper.CreateProduct("SEC1", data.Org1);
			var secondaryProduct2 = WhsHelper.CreateProduct("SEC2", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProductBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct1, 5m);
			var pivot = secondaryProductBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 5m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>4.000</ns0:ProductQuantity>");

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC2", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivot Action=\"UPDATE\">");
			xmlBuilder.AppendLine($"<ns0:PK>{pivot.PK}</ns0:PK>");
			xmlBuilder.AppendLine("<ns0:ComponentQuantity>20</ns0:ComponentQuantity>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot have Total Component Quantity used (20) greater than the Total Component Stock Quantity (10) on the BOM Component.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Update_SecondaryPartCannotBeMatched_PK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>10.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");
			var randomGuid = Guid.NewGuid();
			xmlBuilder.AppendLine($"<ns0:PK>{randomGuid}</ns0:PK>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "UPDATE", pivot.PK.ToGuid(), 5m, "UNT", "BOM", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals($@"Importing Product: P1
Test error: Cannot import product as OrgSupplierPart with PK {randomGuid} cannot be matched with existing definitions.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Update_SecondaryPartCannotBeMatched_MissingProductPartNum()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine($"<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to match OrgSupplierPart without PK or Product Partnum/Owner pair defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Update_SecondaryPartCannotBeMatched_MissingProductRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");
			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart without owner OrgPartRelation defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Update_SecondaryPartCannotBeMatched_UnableToMatchRelation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine("<ns0:Code>ABC</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgSupplierPart OrgPartRelation must include a valid OrgHeader.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Update_SecondaryPartCannotBeMatched_IsNotOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>SUP</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: OrgPartRelation must be an Owner.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Update_SecondaryPartCannotBeMatched_TooManyRelations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherClient = WhsHelper.CreateClient("C2");

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine("<ns0:ProductQuantity>5.000</ns0:ProductQuantity>");

			xmlBuilder.AppendLine("<ns0:SecondaryPart TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>");

			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>OWN</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{data.Org1.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("<ns0:SecondaryPartOrgPartRelation>");

			xmlBuilder.AppendLine("<ns0:Relationship>SUP</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{otherClient.OH_Code}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelation>");
			xmlBuilder.AppendLine("</ns0:SecondaryPartOrgPartRelationCollection>");
			xmlBuilder.AppendLine("</ns0:SecondaryPart>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Cannot match OrgSupplierPart with multiple owner OrgPartRelations defined.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOMPivot_Update_RequiresPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM", data.Org1);
			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryOrgPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);
			var pivot = secondaryOrgPartBOM.ComponentUsages.AddNew();
			pivot.OPP_OE_Component = orgPartBOM.PK;
			pivot.OPP_ComponentQuantity = 2m;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));

			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "SecondaryPart", "SEC", data.Org1.OH_Code, "OWN");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			AppendSecondaryPartBOMPivotDetailsToStringBuilder(xmlBuilder, "UPDATE", null, 5m, "UNT", "BOM", data.Org1.OH_Code, "OWN");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: Unable to update dbo.OrgSecondaryPartBOMPivot without PK.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		#endregion

		#region TestOrgSecondaryPartBOM_Delete

		public void TestOrgSecondaryPartBOM_Delete_DeleteOrgSecondaryPartBOMWithoutDeletingPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgSecondaryPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);

			var pivot = orgSecondaryPartBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 1m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "DELETE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgSecondaryPartBOM.PK}</ns0:PK>");
			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
Test error: The OrgSecondaryPartBOM cannot be deleted, because there is at least one OrgSecondaryPartBOMPivot referencing it.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.", logs);
		}

		public void TestOrgSecondaryPartBOM_Delete_WithDeletePivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgSecondaryPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);

			var pivot = orgSecondaryPartBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 1m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "DELETE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgSecondaryPartBOM.PK}</ns0:PK>");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivot Action=\"DELETE\">");
			xmlBuilder.AppendLine($"<ns0:PK>{pivot.PK}</ns0:PK>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 0 inserts, 0 updates, 1 deletes
OrgSecondaryPartBOMPivot - 0 inserts, 0 updates, 1 deletes", logs);
		}

		public void TestOrgSecondaryPartBOMPivot_Delete()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var bomProduct = WhsHelper.CreateProduct("BOM1", data.Org1);
			var orgPartBOM = WhsHelper.CreateProductBOM(data.Part1, bomProduct, 10m, "UNT");

			var secondaryProduct = WhsHelper.CreateProduct("SEC", data.Org1);
			var orgSecondaryPartBOM = WhsHelper.CreateSecondaryProduct(data.Part1, secondaryProduct, 5m);

			var pivot = orgSecondaryPartBOM.ComponentUsages.AddNew();
			pivot.OPP_ComponentQuantity = 1m;
			pivot.OPP_OE_Component = orgPartBOM.PK;

			Factory.Save();

			var xmlBuilder = new StringBuilder();
			xmlBuilder.AppendLine(StartSecondaryBOMProductXML(data.Part1, data.Org1, "UPDATE"));
			xmlBuilder.AppendLine($"<ns0:PK>{orgSecondaryPartBOM.PK}</ns0:PK>");

			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivotCollection>");
			xmlBuilder.AppendLine("<ns0:OrgSecondaryPartBOMPivot Action=\"DELETE\">");
			xmlBuilder.AppendLine($"<ns0:PK>{pivot.PK}</ns0:PK>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivotCollection>");

			xmlBuilder.AppendLine(CloseSecondaryBOMProductXML);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlBuilder.ToString())));
			var logs = GetLogs();

			AssertEquals(@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOM - 0 inserts, 0 updates, 0 deletes
OrgSecondaryPartBOMPivot - 0 inserts, 0 updates, 1 deletes", logs);
		}

		#endregion

		void AppendProductMatchingDetailsInsertionToStringBuilder(StringBuilder xmlBuilder, string elementName, string partNum, string client, string relationship)
		{
			xmlBuilder.AppendLine($"<ns0:{elementName} TableName=\"OrgSupplierPart\">");

			xmlBuilder.AppendLine($"<ns0:PartNum>{partNum}</ns0:PartNum>");

			xmlBuilder.AppendLine($"<ns0:{elementName}OrgPartRelationCollection TableName=\"OrgPartRelation\">");
			xmlBuilder.AppendLine($"<ns0:{elementName}OrgPartRelation>");

			xmlBuilder.AppendLine($"<ns0:Relationship>{relationship}</ns0:Relationship>");
			xmlBuilder.AppendLine("<ns0:OrgHeader>");
			xmlBuilder.AppendLine($"<ns0:Code>{client}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:OrgHeader>");

			xmlBuilder.AppendLine($"</ns0:{elementName}OrgPartRelation>");
			xmlBuilder.AppendLine($"</ns0:{elementName}OrgPartRelationCollection>");
			xmlBuilder.AppendLine($"</ns0:{elementName}>");
		}

		void AppendSecondaryPartBOMPivotDetailsToStringBuilder(StringBuilder xmlBuilder, string action, Guid? pivotPK, decimal componentQty, string packType, string bomPartNum, string client, string relationship)
		{
			xmlBuilder.AppendLine($"<ns0:OrgSecondaryPartBOMPivot Action=\"{action}\">");

			if (pivotPK != null)
			{
				xmlBuilder.AppendLine($"<ns0:PK>{pivotPK}</ns0:PK>");
			}

			xmlBuilder.AppendLine($"<ns0:ComponentQuantity>{componentQty}</ns0:ComponentQuantity>");

			xmlBuilder.AppendLine("<ns0:ComponentOrgPartBOM TableName=\"OrgPartBOM\">");
			AppendProductMatchingDetailsInsertionToStringBuilder(xmlBuilder, "Component", bomPartNum, client, relationship);

			xmlBuilder.AppendLine("<ns0:PackType TableName=\"RefPackType\">");
			xmlBuilder.AppendLine($"<ns0:Code>{packType}</ns0:Code>");
			xmlBuilder.AppendLine("</ns0:PackType>");

			xmlBuilder.AppendLine("</ns0:ComponentOrgPartBOM>");

			xmlBuilder.AppendLine("</ns0:OrgSecondaryPartBOMPivot>");
		}

		string StartSecondaryBOMProductXML(OrgSupplierPart part, OrgHeader org, string bomAction) =>
$@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""MERGE"">
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
		<ns0:OrgHeader>
          <ns0:Code>{org.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
	<ns0:OrgSecondaryPartBOMCollection>
		<ns0:OrgSecondaryPartBOM Action=""{bomAction}"">";

		string CloseSecondaryBOMProductXML => @"
		</ns0:OrgSecondaryPartBOM>
	</ns0:OrgSecondaryPartBOMCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";

		#endregion

		#region TestProductStyleImport

		public void TestProductStyleImport()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertNotNull("New product is created.", Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")));

			var productStyle = Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST1")).Single();
			AssertNotNull("New product style is created.", productStyle);

			var productStyleColour = Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Single();
			AssertNotNull("New product style colour is created.", productStyleColour);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleColour.WSC_WST_ProductStyle);

			var productStyleSize = Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Single();
			AssertNotNull("New product style size is created.", productStyleSize);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleSize.WSZ_WST_ProductStyle);
			AssertEquals("Sequence is correct.", new ZByte(1), productStyleSize.WSZ_Sequence);
		}

		public void TestProductStyleImport_Classification()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleClassificationSection("M", "Male", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertNotNull("New product is created.", Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")));

			var productStyle = Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST1")).Single();
			AssertNotNull("New product style is created.", productStyle);

			var productStyleColour = Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Single();
			AssertNotNull("New product style colour is created.", productStyleColour);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleColour.WSC_WST_ProductStyle);

			var productStyleClassification = Factory.Load<IWhsProductStyleClassification>(new ZQuery(WhsProductStyleClassificationSchema.WSS_Code, "M")).Single();
			AssertNotNull("New product style classification is created.", productStyleClassification);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleClassification.WSS_WST_ProductStyle);

			var productStyleSize = Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Single();
			AssertNotNull("New product style size is created.", productStyleSize);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleSize.WSZ_WST_ProductStyle);
			AssertEquals("Sequence is correct.", new ZByte(1), productStyleSize.WSZ_Sequence);
		}

		public void TestProductStyleImport_ExistingProductWithNoProductStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleClassificationSection("M", "Male", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			AssertEquals("Product's product style colour is empty.", ZGuid.Empty, data.Part1.OP_WSC_WhsProductStyleColour);
			AssertEquals("Product's product style classification is empty.", ZGuid.Empty, data.Part1.OP_WSS_WhsProductStyleClassification);
			AssertEquals("Product's product style size is empty.", ZGuid.Empty, data.Part1.OP_WSZ_WhsProductStyleSize);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			var productStyle = Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST1")).Single();
			AssertNotNull("New product style is created.", productStyle);

			var productStyleColour = Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Single();
			AssertNotNull("New product style colour is created.", productStyleColour);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleColour.WSC_WST_ProductStyle);

			var productStyleClassification = Factory.Load<IWhsProductStyleClassification>(new ZQuery(WhsProductStyleClassificationSchema.WSS_Code, "M")).Single();
			AssertNotNull("New product style classification is created.", productStyleClassification);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleClassification.WSS_WST_ProductStyle);

			var productStyleSize = Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Single();
			AssertNotNull("New product style size is created.", productStyleSize);
			AssertEquals("Product style is new product style.", productStyle.PK, productStyleSize.WSZ_WST_ProductStyle);
			AssertEquals("Sequence is correct.", new ZByte(1), productStyleSize.WSZ_Sequence);

			data.Part1.Reload();
			AssertEquals("Product is updated with product style colour.", productStyleColour.PK, data.Part1.OP_WSC_WhsProductStyleColour);
			AssertEquals("Product is updated with product style size.", productStyleSize.PK, data.Part1.OP_WSZ_WhsProductStyleSize);
		}

		public void TestProductStyleImport_ProductStyleExists_ProductStyleSizeSequenceIncremented()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("TEST1", "Testing Product Style", data.Org1.PK);
			WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");
			WhsHelper.CreateProductStyleSize(productStyle, 2, "MEDIU");
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertNotNull("New product is created.", Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")));

			var productStyleSizeNew = Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Single();
			AssertNotNull("New product style size is created.", productStyleSizeNew);
			AssertEquals("Sequence is correct.", new ZByte(3), productStyleSizeNew.WSZ_Sequence);
		}

		public void TestProductStyleImport_MultipleProductStyleColourSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("BL1", "Color Blue", "TEST1", "Testing Product Style", "ABCEXPBNE"));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", "ABCEXPBNE"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain errors.", true, logs.Contains("Only 1 ProductStyleColour can be specified."));
			AssertEquals("Logs should contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "BL1")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
		}

		public void TestProductStyleImport_MultipleProductStyleClassificationSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleClassificationSection("M", "Male", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleClassificationSection("F", "Female", "TEST1", "Testing Product Style", "ABCEXPBNE"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain errors.", true, logs.Contains("Only 1 ProductStyleClassification can be specified."));
			AssertEquals("Logs should contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
			AssertEquals("New product style classification is not imported.", false, Factory.Load<IWhsProductStyleClassification>(new ZQuery(WhsProductStyleClassificationSchema.WSS_Code, "M")).Any());
			AssertEquals("New product style classification is not imported.", false, Factory.Load<IWhsProductStyleClassification>(new ZQuery(WhsProductStyleClassificationSchema.WSS_Code, "F")).Any());
		}

		public void TestProductStyleImport_MultipleProductStyleSizeSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("LARGE", "TEST1", "Testing Product Style", "ABCEXPBNE"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain errors.", true, logs.Contains("Only 1 ProductStyleSize can be specified."));
			AssertEquals("Logs should contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "LARGE")).Any());
		}

		public void TestProductStyleImport_ProductStyleNotSpecifiedOnProductStyleColour()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Red color", "", "", ""));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style colour/size should have a product style."));
			AssertEquals("Logs should contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
		}

		public void TestProductStyleImport_ProductStyleNotSpecifiedOnProductStyleSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Red color", "TEST", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "", "", ""));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style colour/size should have a product style."));
			AssertEquals("Logs should contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
		}

		public void TestProductStyleImport_ProductStyleSizeNotSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Red color", "TEST", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style colour and product style size must be both specified or both unspecified."));
			AssertEquals("Logs should contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
		}

		public void TestProductStyleImport_ProductStyleColourNotSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style colour and product style size must be both specified or both unspecified."));
			AssertEquals("Logs should contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
		}

		public void TestProductStyleImport_DifferentProductStyles()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST2", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style for both style colour, style classification and style size should be the same."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
		}

		public void TestProductStyleImport_DifferentProductStyleOwners()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", "ABCEXPBNE"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style owner for style colour, style classification and style size should be the same."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
		}

		public void TestProductStyleImport_NoProductOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(""));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style should not be specified if the product has no owners."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style from size is not imported.", false, Factory.Load<IWhsProductStyle>(new ZQuery(WhsProductStyleSchema.WST_Code, "TEST")).Any());
			AssertEquals("New product style size is not imported.", false, Factory.Load<IWhsProductStyleSize>(new ZQuery(WhsProductStyleSizeSchema.WSZ_Size, "SMALL")).Any());
		}

		public void TestProductStyleImport_MultipleProductOwners()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code, "ABCEXPBNE"));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import errors.", true, logs.Contains("Product style should not be specified if the product has multiple owners."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
		}

		public void TestProductStyleImport_NoProductStyleOwnerSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", ""));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", ""));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs should contain product style import error.", true, logs.Contains("Product style should have an owner."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
		}

		public void TestProductStyleImport_UpdatingExistingProductWithProductStyleFails_DifferentProductStyleColour()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("ORIG", "Testing product style", data.Org1.PK);
			var productStyleColour = WhsHelper.CreateProductStyleColour(productStyle, "BL1", "Color Blue");
			var productStyleSize = WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");

			data.Part1.OP_WSC_WhsProductStyleColour = productStyleColour.PK;
			data.Part1.OP_WSZ_WhsProductStyleSize = productStyleSize.PK;
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "ORIG", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("LARGE", "ORIG", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs contain errors.", true, logs.Contains("A product style colour has already been assigned to the product."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
		}

		public void TestProductStyleImport_UpdatingExistingProductWithProductStyleFails_DifferentProductStyleSize()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productStyle = WhsHelper.CreateProductStyle("ORIG", "Testing product style", data.Org1.PK);
			var productStyleColour = WhsHelper.CreateProductStyleColour(productStyle, "BL1", "Color Blue");
			var productStyleSize = WhsHelper.CreateProductStyleSize(productStyle, 1, "LARGE");

			data.Part1.OP_WSC_WhsProductStyleColour = productStyleColour.PK;
			data.Part1.OP_WSZ_WhsProductStyleSize = productStyleSize.PK;
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("BL1", "Color Blue", "ORIG", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetProductStyleSizeSection("SMALL", "ORIG", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();

			AssertEquals("Logs contain errors.", true, logs.Contains("A product style size has already been assigned to the product."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
			AssertEquals("New product style colour is not imported.", false, Factory.Load<IWhsProductStyleColour>(new ZQuery(WhsProductStyleColourSchema.WSC_Code, "RED")).Any());
		}

		public void TestProductStyleImport_EmptyProductStyleColourSection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetEmptyProductStyleSection("ProductStyleColour", "WhsProductStyleColour"));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs contain errors.", true, logs.Contains("Product style colour and product style size must be both specified or both unspecified."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
		}

		public void TestProductStyleImport_EmptyProductStyleClassificationSection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetEmptyProductStyleSection("ProductStyleClassification", "WhsProductStyleClassification"));
			xml.Append(GetProductStyleSizeSection("SMALL", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does not contain error.", false, logs.Contains("Product style colour and product style size must be both specified or both unspecified."));
			AssertNotNull("New product is imported.", Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
		}

		public void TestProductStyleImport_EmptyProductStyleSizeSection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetProductStyleColourSection("RED", "Color Red", "TEST1", "Testing Product Style", data.Org1.OH_Code));
			xml.Append(GetEmptyProductStyleSection("ProductStyleSize", "WhsProductStyleSize"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs contain errors.", true, logs.Contains("Product style colour and product style size must be both specified or both unspecified."));
			AssertEquals("New product is not imported.", false, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
		}

		public void TestProductStyleImport_EmptyProductStyleColourAndSizeSection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetEmptyProductStyleSection("ProductStyleColour", "WhsProductStyleColour"));
			xml.Append(GetEmptyProductStyleSection("ProductStyleSize", "WhsProductStyleSize"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertNotNull("New product is not imported.", Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Single());
		}

		#endregion

		#region TestProductUnitPriceAndCurrencyImport

		public void TestImportProductWithUnitPriceImport_NoCurrencyProperties_RelationPopulated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			partRelation.OU_UnitPrice = 4.5m;
			partRelation.OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
        <ns0:UnitPriceCurrency>
        </ns0:UnitPriceCurrency>
        <ns0:OrgHeader>
          <ns0:Code>{data.Org1.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();

			partRelation.Reload();
			AssertContains($@"Attempted to create a situation for Client: {data.Org1.OH_Code}, Part: {data.Part1.OP_PartNum} where the Unit Price is without a Unit Price Currency.", logs);
			CombineAssertions(() =>
			{
				AssertEquals("Should *not* have updated OU_UnitPrice.", 4.5m, partRelation[OrgPartRelationSchema.OU_UnitPrice]);
				AssertEquals("Should *not* have updated OU_RX_NKUnitPriceCurrency.", "AUD", partRelation[OrgPartRelationSchema.OU_RX_NKUnitPriceCurrency]);
			});
		}

		public void TestImportProductWithUnitPriceDeleteImport_WithBlankCurrency()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""DELETE"">
        <ns0:PK>{partRelation.PK}</ns0:PK>
        <ns0:UnitPrice>1</ns0:UnitPrice>
				<ns0:UnitPriceCurrency>
					<ns0:Code></ns0:Code>
				</ns0:UnitPriceCurrency>
				<ns0:OrgHeader>
          <ns0:Code>{data.Org1.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();
			AssertNotContains($@"Attempted to create a situation for Client: {data.Org1.OH_Code}, Part: {data.Part1.OP_PartNum} where the Unit Price is without a Unit Price Currency.", logs);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyAndPriceAreNull_BizoUnpopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: null,
				xmlUnitPrice: null,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: false,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyIsNullAndPriceHasValue_BizoUnpopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: null,
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: false,
				isErrorExpected: true,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyIsNullAndPriceEmpty_BizoUnpopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: false,
				isErrorExpected: true,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyHasValueAndPriceNull_BizoUnpopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "AUD",
				xmlUnitPrice: null,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: false,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyEmptyAndPriceNull_BizoUnpopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: null,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: false,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyNullAndPriceZero_BizoUnpopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: null,
				xmlUnitPrice: 0m,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: false,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyHasValueAndPriceHasValue_BizoUnpopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "AUD",
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: false,
				isErrorExpected: false,
				isPriceExpected: true,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyNullAndPriceNull_BizoCurrencyPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: null,
				xmlUnitPrice: null,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyHasValueAndPriceNull_BizoCurrencyPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "AUD",
				xmlUnitPrice: null,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyEmptyAndPriceNull_BizoCurrencyPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: null,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyEmptyAndPriceHasValue_BizoCurrencyPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: true,
				isErrorExpected: true,
				isPriceExpected: false,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyHasValueAndPriceHasValue_BizoCurrencyPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "AUD",
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: true,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyEmptyAndPriceZero_BizoCurrencyPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: 0m,
				isBizoPricePopulated: false,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyNullAndPriceNull_BizoPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: null,
				xmlUnitPrice: null,
				isBizoPricePopulated: true,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: true,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyEmptyAndPriceNull_BizoPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: null,
				isBizoPricePopulated: true,
				isBizoCurrencyPopulated: true,
				isErrorExpected: true,
				isPriceExpected: true,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyNullAndPriceEmpty_BizoPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: null,
				xmlUnitPrice: 0m,
				isBizoPricePopulated: true,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyEmptyAndPriceZero_BizoPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: 0m,
				isBizoPricePopulated: true,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: false,
				isCurrencyExpected: false);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyEmptyAndPriceHasValue_BizoPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "",
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: true,
				isBizoCurrencyPopulated: true,
				isErrorExpected: true,
				isPriceExpected: true,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyNullAndPriceHasValue_BizoPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: null,
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: true,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: true,
				isCurrencyExpected: true);
		}

		public void TestImportProductWithUnitPriceImport_ImportedCurrencyHasValueAndPriceHasValue_BizoPopulated()
		{
			TestImportProductWithUnitPriceImportCore(
				xmlUnitPriceCurrency: "AUD",
				xmlUnitPrice: 4.5m,
				isBizoPricePopulated: true,
				isBizoCurrencyPopulated: true,
				isErrorExpected: false,
				isPriceExpected: true,
				isCurrencyExpected: true);
		}

		void TestImportProductWithUnitPriceImportCore(
			string xmlUnitPriceCurrency, decimal? xmlUnitPrice,
			bool isBizoPricePopulated, bool isBizoCurrencyPopulated,
			bool isErrorExpected, bool isPriceExpected, bool isCurrencyExpected)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var partRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			if (isBizoPricePopulated)
			{
				partRelation.OU_UnitPrice = 4.5m;
			}
			if (isBizoCurrencyPopulated)
			{
				partRelation.OU_RX_NKUnitPriceCurrency = "AUD";
			}
			Factory.Save();

			var currency = string.Empty;
			if (xmlUnitPriceCurrency != null)
			{
				currency = $@"
				<ns0:UnitPriceCurrency>
          <ns0:Code>{xmlUnitPriceCurrency}</ns0:Code>
        </ns0:UnitPriceCurrency>";
			}

			var unitPrice = string.Empty;
			if (xmlUnitPrice != null)
			{
				unitPrice = $@"
				<ns0:UnitPrice>{xmlUnitPrice}</ns0:UnitPrice>";
			}

			var xml = $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>{data.Part1.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>
" + currency + @"
" + unitPrice + $@"
        <ns0:OrgHeader>
          <ns0:Code>{data.Org1.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";
			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));
			var logs = GetLogs();

			partRelation.Reload();

			CombineAssertions(() =>
			{
				if (isErrorExpected)
				{
					AssertContains($@"Attempted to create a situation for Client: {data.Org1.OH_Code}, Part: {data.Part1.OP_PartNum} where the Unit Price is without a Unit Price Currency.", logs);
				}
				else
				{
					AssertNotContains($@"Attempted to create a situation for Client: {data.Org1.OH_Code}, Part: {data.Part1.OP_PartNum} where the Unit Price is without a Unit Price Currency.", logs);
				}

				var expectedUnitPrice = isPriceExpected ? 4.5m : 0m;
				var expectedCurrency = isCurrencyExpected ? "AUD" : string.Empty;
				AssertEquals("Should have correct OU_UnitPrice.", expectedUnitPrice, partRelation[OrgPartRelationSchema.OU_UnitPrice]);
				AssertEquals("Should have correct OU_RX_NKUnitPriceCurrency.", expectedCurrency, partRelation[OrgPartRelationSchema.OU_RX_NKUnitPriceCurrency]);
			});
		}

		#endregion

		#region TestImportProductWithInvalidUnitConversionUnits

		public void TestImportProduct_UnitConversionPackTypeInvalid_ErrorRegistryEnabled()
		{
			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var xml = GetWhsProductUnitConversion_XML("testPartNum", "XXX", "PLT");

				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

				var logs = GetLogs();
				AssertEquals("Logs should contain error due to invalid unit conversion package type.", true, logs.Contains("Error during import: Invalid package types in unit conversion: XXX"));
				AssertEquals("New product is not imported.", true, logs.Contains("No insert/update action performed."));
			}
		}

		public void TestImportProduct_UnitConversionPackTypesValidLowerCaseAndWhitespace_ErrorRegistryEnabled()
		{
			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var xml = GetWhsProductUnitConversion_XML("testPartNum", " unt ", " l ");

				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

				var logs = GetLogs();
				AssertEquals("Logs should not contain error due to invalid unit conversion package type.", false, logs.Contains("Error during import: Invalid package types in unit conversion:"));
				AssertEquals("New product is imported.", true, logs.Contains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 1 inserts, 0 updates, 0 deletes"));
			}
		}

		public void TestImportProduct_UnitConversionParentPackTypeInvalid_ErrorRegistryEnabled()
		{
			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var xml = GetWhsProductUnitConversion_XML("testPartNum", "UNT", "YYY");

				manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml)));

				var logs = GetLogs();
				AssertEquals("Logs should contain error due to invalid unit conversion package type.", true, logs.Contains("Error during import: Invalid package types in unit conversion: YYY"));
				AssertEquals("New product is not imported.", true, logs.Contains("No insert/update action performed."));
			}
		}

		string GetWhsProductUnitConversion_XML(string partNum, string unitConversionPackType, string unitConversionParentPackType, ZGuid? orgPartPK = null, ZGuid? partUnitPK = null, decimal quantityInParent = 5)
		{
			var pkSection = partUnitPK == null ? "" : $"<ns0:PK>{partUnitPK.Value}</ns0:PK>";
			var orgPartUnitCollection = $@"<ns0:OrgPartUnitCollection>
<ns0:OrgPartUnit Action=""MERGE"">
	{pkSection}
	<ns0:PackType>{unitConversionPackType}</ns0:PackType>
	<ns0:Weight>0.000</ns0:Weight>
	<ns0:Height>0.000</ns0:Height>
	<ns0:Width>0.000</ns0:Width>
	<ns0:Depth>0.000</ns0:Depth>
	<ns0:Cubic>0.000</ns0:Cubic>
	<ns0:ParentPackType>{unitConversionParentPackType}</ns0:ParentPackType>
	<ns0:QuantityInParent>{quantityInParent}</ns0:QuantityInParent>
	<ns0:NoOfSKUsInThisPack>0.000</ns0:NoOfSKUsInThisPack>
</ns0:OrgPartUnit>
</ns0:OrgPartUnitCollection>";

			return GetOpeningProductSection(partNum, orgPartPK) + GetOrgPartRelationSection("") + orgPartUnitCollection + GetClosingProductSection();
		}

		string GetWhsProductUnitConversionForDelete_XML(string partNum, ZGuid orgPartPK, ZGuid orgPartUnitPKToDelete)
		{
			var orgPartUnitCollection = $@"<ns0:OrgPartUnitCollection>
<ns0:OrgPartUnit Action=""DELETE"">
	<ns0:PK>{orgPartUnitPKToDelete}</ns0:PK>
</ns0:OrgPartUnit>
</ns0:OrgPartUnitCollection>";

			return GetOpeningProductSection(partNum, orgPartPK) + GetOrgPartRelationSection("") + orgPartUnitCollection + GetClosingProductSection();
		}

		#endregion

		#region TestProductPartBarcodeImport

		#region TestProductPartBarcodeImport_EmptyBarcode

		public void TestProductPartBarcodeImport_EmptyBarcode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("", "PKG")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		#endregion

		#region TestProductPartBarcodeImport_EmptyPackType

		public void TestProductPartBarcodeImport_EmptyPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("Barcode1", ""), new KeyValuePair<string, string>("Barcode2", "UNT")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		#endregion

		#region TestProductPartBarcodeImport_EmptyBarcodeAndPackType

		public void TestProductPartBarcodeImport_EmptyBarcodeAndPackType_MERGE()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("", "")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		public void TestProductPartBarcodeImport_EmptyBarcodeAndPackType_INSERT()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.INSERT, new KeyValuePair<string, string>("", "")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		public void TestProductPartBarcodeImport_EmptyBarcodeAndPackType_UPDATE()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.UPDATE, new KeyValuePair<string, string>("", "")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		public void TestProductPartBarcodeImport_EmptyBarcodeAndPackType_EMPTY()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.EMPTY, new KeyValuePair<string, string>("", "")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does not contain error.", false, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is imported.", true, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
		}

		public void TestProductPartBarcodeImport_EmptyBarcodeAndPackType_IGNORE()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.IGNORE, new KeyValuePair<string, string>("", "")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does not contain error.", false, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is imported.", true, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Any());
		}

		#endregion

		#region TestProductPartBarcodeImport_WhitespaceBarcode

		public void TestProductPartBarcodeImport_WhitespaceBarcode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("	 	   ", "UNT")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		#endregion

		#region TestProductPartBarcodeImport_WhitespacePackType

		public void TestProductPartBarcodeImport_WhitespacePackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeCollectionXML(EntityAction.MERGE, new KeyValuePair<string, string>("Barcode1", " 	 ")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));
			var logs = GetLogs();
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		#endregion

		#region TestProductPartBarcodeImport_NoPackType

		public void TestProductPartBarcodeImport_NoPackType_DeleteAction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var productBarcode = data.Part1.PartBarcodes.AddNew();
			productBarcode.PH_Barcode = "1313";
			productBarcode.PH_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetDeleteOrgSupplierPartBarcodeSection(productBarcode.PK.ToString()));
			xml.Append(GetClosingProductSection());

			AssertEquals("Product has barcode.", true, data.Part1.PartBarcodes.Any());
			AssertNoExceptionThrown(() => manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends()))));

			var logs = GetLogs();
			AssertEquals("Logs does not contain 'Sequence contains no matching element' error.", false, logs.Contains("Sequence contains no matching element"));
			AssertEquals("Logs does not contain error.", false, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does not contain error.", false, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));

			var newFactory = new BusinessObjectFactory();
			var productInNewFactory = newFactory.Load<OrgSupplierPart>(data.Part1.PK);
			AssertEquals("Product barcode is deleted.", false, productInNewFactory.PartBarcodes.Any());
		}

		string GetDeleteOrgSupplierPartBarcodeSection(string barcodePK)
		{
			return $@"<ns0:OrgSupplierPartBarcodeCollection>
	<ns0:OrgSupplierPartBarcode Action=""DELETE"">
		<ns0:PK>{barcodePK}</ns0:PK>
	</ns0:OrgSupplierPartBarcode>
</ns0:OrgSupplierPartBarcodeCollection>";
		}

		public void TestProductPartBarcodeImport_NoPackType_MissingPackTypeEntity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("ABC001"));
			xml.Append(GetOrgPartRelationSection(data.Org1.OH_Code));
			xml.Append(GetOrgSupplierPartBarcodeSectionWithNoPackTypeEntity("Barcode1"));
			xml.Append(GetClosingProductSection());

			AssertNoExceptionThrown(() => manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends()))));

			var logs = GetLogs();
			AssertEquals("Logs does not contain 'Sequence contains no matching element' error.", false, logs.Contains("Sequence contains no matching element"));
			AssertEquals("Logs does contain error.", true, logs.Contains("Error occurred trying to import file. Please fix the error and try importing the file again."));
			AssertEquals("Logs does contain error.", true, logs.Contains("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType."));
			AssertEquals("New product is not imported.", 0, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "ABC001")).Length);
		}

		string GetOrgSupplierPartBarcodeSectionWithNoPackTypeEntity(string barcode)
		{
			return $@"<ns0:OrgSupplierPartBarcodeCollection>
	<ns0:OrgSupplierPartBarcode Action=""MERGE"">
		<ns0:Barcode>{barcode}</ns0:Barcode>
	</ns0:OrgSupplierPartBarcode>
</ns0:OrgSupplierPartBarcodeCollection>";
		}

		#endregion

		#endregion

		#region TestProductCusCNClassification

		public void TestScenario39CusCNClassification()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.New<OrgHeader>();
			org.OH_Code = "ORGTEST01";
			org.MainAddress.OA_Address1 = "ADD 1";
			var buyer = factory.New<OrgHeader>();
			buyer.OH_Code = "ACETESPHL";
			var owner = factory.New<OrgHeader>();
			owner.OH_Code = "NANINTSHA";
			var org2 = factory.New<OrgHeader>();
			org2.OH_Code = "EXPOSESYD";
			factory.Save();

			RunXml("41.CusCNClassification_Insert.xml");
			var logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusCNClassification - 1 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 2 inserts, 0 updates, 0 deletes", logs);

			ResetLogs();
			RunXml("41.CusCNClassification_Merge.xml");
			logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusCNClassification - 0 inserts, 0 updates, 0 deletes
StmNote - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes", logs);

			ResetLogs();
			RunXml("41.CusCNClassification_Delete.xml");
			logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 0 inserts, 0 updates, 1 deletes
CusClassPartPivot - 0 inserts, 0 updates, 1 deletes
CusCNClassification - 0 inserts, 0 updates, 1 deletes
StmNote - 0 inserts, 0 updates, 1 deletes
OrgPartRelation - 0 inserts, 0 updates, 2 deletes", logs);
		}

		#endregion

		#region TestProductCusCAClassification

		public void TestScenario39CusCAClassification()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.New<OrgHeader>();
			org.OH_Code = "ORGTEST01";
			org.MainAddress.OA_Address1 = "ADD 1";
			var buyer = factory.New<OrgHeader>();
			buyer.OH_Code = "ACETESPHL";
			factory.Save();

			RunXml("40.CusCAClassification_Insert.xml");
			var logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusCAClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			ResetLogs();
			RunXml("40.CusCAClassification_Merge.xml");
			logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusCAClassification - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes", logs);

			ResetLogs();
			RunXml("40.CusCAClassification_Delete.xml");
			logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 0 inserts, 0 updates, 1 deletes
CusClassPartPivot - 0 inserts, 0 updates, 1 deletes
CusCAClassification - 0 inserts, 0 updates, 1 deletes
OrgPartRelation - 0 inserts, 0 updates, 1 deletes", logs);
		}

		#endregion

		#region GetOrgSupplierPartBarcodeCollectionXML

		string GetOrgSupplierPartBarcodeCollectionXML(EntityAction action, params KeyValuePair<string, string>[] partBarcodes)
		{
			var xml = $@"<ns0:OrgSupplierPartBarcodeCollection>" +
				string.Join(System.Environment.NewLine, partBarcodes.Select(kvp => $@"
	<ns0:OrgSupplierPartBarcode Action=""{GetEntityActionString(action)}"">
		<ns0:Barcode>{kvp.Key}</ns0:Barcode>
		<ns0:PackType TableName=""RefPackType"">
			<ns0:Code>{kvp.Value}</ns0:Code>
		</ns0:PackType>
	</ns0:OrgSupplierPartBarcode>")) + $@"
</ns0:OrgSupplierPartBarcodeCollection>";

			return xml;
		}

		string GetEntityActionString(EntityAction action)
		{
			switch (action)
			{
				case EntityAction.EMPTY:
					return "";
				case EntityAction.INSERT:
					return "INSERT";
				case EntityAction.UPDATE:
					return "UPDATE";
				case EntityAction.MERGE:
					return "MERGE";
				case EntityAction.DELETE:
					return "DELETE";
				case EntityAction.IGNORE:
					return "IGNORE";
				default:
					return string.Empty;
			}
		}

		#endregion

		#region GetWhsProductXML

		string GetWhsProductXML(OrgSupplierPart part, OrgHeader org, params KeyValuePair<string, bool>[] relationElements) =>
$@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>" +
		string.Join(System.Environment.NewLine, relationElements.Select(kvp => $"<ns0:{kvp.Key}>{kvp.Value.ToString()}</ns0:{kvp.Key}>")) +
	 $@"<ns0:OrgHeader>
          <ns0:Code>{org.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";

		string GetWhsProductXML(OrgSupplierPart part, OrgHeader org, params KeyValuePair<string, string>[] relationElements) =>
$@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <ns0:OrgSupplierPart Action=""UPDATE"">
    <ns0:PartNum>{part.OP_PartNum}</ns0:PartNum>
    <ns0:OrgPartRelationCollection>
      <ns0:OrgPartRelation Action=""UPDATE"">
        <ns0:Relationship>OWN</ns0:Relationship>" +
		string.Join(System.Environment.NewLine, relationElements.Select(kvp => $"<ns0:{kvp.Key}>{kvp.Value}</ns0:{kvp.Key}>")) +
	 $@"<ns0:OrgHeader>
          <ns0:Code>{org.OH_Code}</ns0:Code>
        </ns0:OrgHeader>
      </ns0:OrgPartRelation>
    </ns0:OrgPartRelationCollection>
  </ns0:OrgSupplierPart>
</ns0:Product>";

		#endregion

		#region GetWhsProductWithProductStyleXML

		string GetOpeningProductSection(string partNum, ZGuid? orgPartPK = null)
		{
			var pkSection = orgPartPK == null ? "" : $"<ns0:PK>{orgPartPK.Value}</ns0:PK>";
			return $@"<ns0:Product version=""2.0"" xmlns:ns0=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:ns1=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<ns0:OrgSupplierPart Action=""MERGE"">
		{pkSection}
		<ns0:PartNum>{partNum}</ns0:PartNum>";
		}

		string GetOrgPartRelationSection(
			string firstOrgCode,
			string secondOwnerOrgCode = "",
			string orgRelationship = "OWN",
			EntityAction action = EntityAction.MERGE,
			params KeyValuePair<string, string>[] relationElements)
		{
			var relationship = string.IsNullOrEmpty(firstOrgCode) ? "SUP" : orgRelationship;
			firstOrgCode = string.IsNullOrEmpty(firstOrgCode) ? "ABCEXPBNE" : firstOrgCode;
			var secondOrgSection = string.IsNullOrEmpty(secondOwnerOrgCode)
				? ""
				: $@"<ns0:OrgPartRelation Action=""MERGE"">
	<ns0:Relationship>OWN</ns0:Relationship>
				<ns0:OrgHeader>
		<ns0:Code>{secondOwnerOrgCode}</ns0:Code>
	</ns0:OrgHeader>
</ns0:OrgPartRelation>";

			return $@"<ns0:OrgPartRelationCollection>
	<ns0:OrgPartRelation Action=""{Enum.GetName(typeof(EntityAction), action)}"">
		<ns0:Relationship>{relationship}</ns0:Relationship>
{string.Join(System.Environment.NewLine, relationElements.Select(kvp => $"		<ns0:{kvp.Key}>{kvp.Value}</ns0:{kvp.Key}> "))}
		<ns0:OrgHeader>
			<ns0:Code>{firstOrgCode}</ns0:Code>
		</ns0:OrgHeader>
	</ns0:OrgPartRelation>
{secondOrgSection}
</ns0:OrgPartRelationCollection>";
		}

		string GetProductParamsByWhsAndClientSection(ZGuid pk, string ownerOrgCode, string warehouseCode, EntityAction action, params KeyValuePair<string, string>[] elements)
		{
			return $@"
				<ns0:WhsProductParamsByWhsAndClientCollection>
          <ns0:WhsProductParamsByWhsAndClient Action=""{Enum.GetName(typeof(EntityAction), action)}"">
{(pk.IsEmpty ? "" : $"<PK>{pk}</PK>")}
{string.Join(System.Environment.NewLine, elements.Select(kvp => $"          <ns0:{kvp.Key}>{kvp.Value}</ns0:{kvp.Key}> "))}
						{(
						warehouseCode == null
						? ""
						: $@"<ns0:WhsWarehouse>
              <ns0:WarehouseCode>{warehouseCode}</ns0:WarehouseCode>
            </ns0:WhsWarehouse>"
						)}
						{(ownerOrgCode == null ? "" : $@"
			<ns0:OrgHeader>
				<ns0:Code>{ownerOrgCode}</ns0:Code>
            </ns0:OrgHeader>"
						)}
          </ns0:WhsProductParamsByWhsAndClient>
        </ns0:WhsProductParamsByWhsAndClientCollection>";
		}

		string GetProductStyleColourSection(string colourCode, string description, string productStyleCode, string productStyleDescription, string productStyleOwner)
		{
			return $@"<ns0:ProductStyleColour Action=""MERGE"" TableName=""WhsProductStyleColour"">
	<ns0:Description>{description}</ns0:Description>
	<ns0:Code>{colourCode}</ns0:Code>
	{GetParentProductStyleSection(productStyleCode, productStyleDescription, "Colour", productStyleOwner)}
</ns0:ProductStyleColour>";
		}

		string GetProductStyleClassificationSection(string classificationCode, string description, string productStyleCode, string productStyleDescription, string productStyleOwner)
		{
			return $@"<ns0:ProductStyleClassification Action=""MERGE"" TableName=""WhsProductStyleClassification"">
	<ns0:Description>{description}</ns0:Description>
	<ns0:Code>{classificationCode}</ns0:Code>
	{GetParentProductStyleSection(productStyleCode, productStyleDescription, "Classification", productStyleOwner)}
</ns0:ProductStyleClassification>";
		}

		string GetProductStyleSizeSection(string sizeCode, string productStyleCode, string productStyleDescription, string productStyleOwner)
		{
			return $@"<ns0:ProductStyleSize Action=""MERGE"" TableName=""WhsProductStyleSize"">
	<ns0:Size>{sizeCode}</ns0:Size>
	{GetParentProductStyleSection(productStyleCode, productStyleDescription, "Size", productStyleOwner)}
</ns0:ProductStyleSize>";
		}

		string GetEmptyProductStyleSection(string entityName, string tableName)
		{
			return $@"<ns0:{entityName} Action=""MERGE"" TableName=""{tableName}"" />";
		}

		string GetParentProductStyleSection(string code, string description, string colourOrSize, string owner)
		{
			var productStyleSection = string.IsNullOrEmpty(code)
				? ""
				: $@"<ns0:{colourOrSize}ProductStyle Action=""MERGE"" TableName=""WhsProductStyle"">
	<ns0:Code>{code}</ns0:Code>
	<ns0:Description>{description}</ns0:Description>
	{GetProductStyleOwnerSection(owner)}
</ns0:{colourOrSize}ProductStyle>";

			return productStyleSection;
		}

		string GetProductStyleOwnerSection(string owner)
		{
			var productStyleOwnerSection = string.IsNullOrEmpty(owner)
				? ""
				: $@"<ns0:Owner TableName=""OrgHeader"">
	<ns0:Code>{owner}</ns0:Code>
</ns0:Owner>";

			return productStyleOwnerSection;
		}

		string GetClosingProductSection()
		{
			return @"</ns0:OrgSupplierPart>
</ns0:Product>";
		}

		#endregion

		#region Match Active Product First

		public void TestMatchActiveProductFirst()
		{
			OrgSupplierPart partA1 = CreateExistingProduct(new[] { craImpChi }, Array.Empty<string>(), isActive: false);
			OrgSupplierPart partA2 = CreateExistingProduct(new[] { craImpChi }, Array.Empty<string>());
			OrgSupplierPart partB1 = CreateExistingProduct(new[] { craImpChi }, Array.Empty<string>(), productCode: "PRODUCTB");
			OrgSupplierPart partB2 = CreateExistingProduct(new[] { craImpChi }, Array.Empty<string>(), productCode: "PRODUCTB", isActive: false);

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>CW1DUSCHI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Product version=""2.0"">
      <OrgSupplierPart Action=""MERGE"">
        <IsActive>true</IsActive>
        <PartNum>PRODUCTA</PartNum>
        <Desc>UPDATED DESC</Desc>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <CountDecimalPlaces>0</CountDecimalPlaces>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <CommodityCode TableName=""RefCommodityCode"" />
        <LastWeightedCostCurr TableName=""RefCurrency"" />
        <PackType TableName=""RefPackType"" />
      </OrgSupplierPart>
    </Product>
    <Product version=""2.0"">
      <OrgSupplierPart Action=""MERGE"">
        <IsActive>true</IsActive>
        <PartNum>PRODUCTB</PartNum>
        <Desc>UPDATED DESC</Desc>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <CountDecimalPlaces>0</CountDecimalPlaces>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>CRAIMPCHI</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <CommodityCode TableName=""RefCommodityCode"" />
        <LastWeightedCostCurr TableName=""RefCurrency"" />
        <PackType TableName=""RefPackType"" />
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>")));

			CombineAssertions(() =>
			{
				AssertContains("logs", "OrgSupplierPart - 0 inserts, 2 updates, 0 deletes", GetLogs());
				AssertPart("A1", partA1, ZBool.False, "Database Value");
				AssertPart("A2", partA2, ZBool.True, "UPDATED DESC");
				AssertPart("B1", partB1, ZBool.True, "UPDATED DESC");
				AssertPart("B2", partB2, ZBool.False, "Database Value");
			});

			void AssertPart(string partFriendlyName, OrgSupplierPart part, ZBool isActive, ZString desc)
			{
				part.Reload();
				AssertEquals(partFriendlyName + " part.OP_IsActive", isActive, part.OP_IsActive);
				AssertEquals(partFriendlyName + " part.OP_Desc", desc, part.OP_Desc);
			}
		}

		#endregion

		#region TestConsigneeMinShelfLifeAcceptedValidation

		public void TestConsigneeMinShelfLifeAcceptedValidation_NegativeLarge()
		{
			TestConsigneeMinShelfLifeAcceptedValidationCore(consigneeMinShelfLifeAccepted: "-100000",
				expectedLog: $@"Importing Product: PRODUCTB
Test error: Consignee Minimum Shelf Life Accepted '-100000' is not valid.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.");
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Negative()
		{
			TestConsigneeMinShelfLifeAcceptedValidationCore(consigneeMinShelfLifeAccepted: "-1",
				expectedLog: $@"Importing Product: PRODUCTB
Test error: Consignee Minimum Shelf Life Accepted '-1' is not valid.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.");
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Zero()
		{
			TestConsigneeMinShelfLifeAcceptedValidationCore(consigneeMinShelfLifeAccepted: "0",
				expectedLog: $@"Importing Product: PRODUCTB
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes");
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Valid()
		{
			TestConsigneeMinShelfLifeAcceptedValidationCore(consigneeMinShelfLifeAccepted: "30",
				expectedLog: $@"Importing Product: PRODUCTB
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes");
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_NotFitInShortNumber()
		{
			TestConsigneeMinShelfLifeAcceptedValidationCore(consigneeMinShelfLifeAccepted: "123456789",
				expectedLog: $@"Importing Product: PRODUCTB
Test error: Consignee Minimum Shelf Life Accepted '123456789' is not valid.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.");
		}

		void TestConsigneeMinShelfLifeAcceptedValidationCore(string consigneeMinShelfLifeAccepted, string expectedLog)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productB = CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { abcExporters }, productCode: "PRODUCTB", barcodes: Array.Empty<string>());
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE", "", "OWN", EntityAction.INSERT, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", consigneeMinShelfLifeAccepted)));
			//
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, "ABCEXPBNE", data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "60")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_New_NoMaximumShelf()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productB = CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { abcExporters }, productCode: "PRODUCTB", barcodes: Array.Empty<string>());
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE", "", "OWN", EntityAction.INSERT, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, "ABCEXPBNE", data.Whs1.WW_WarehouseCode, EntityAction.MERGE));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: PRODUCTB
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_New_MaximumShelfInXMLIsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productB = CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { abcExporters }, productCode: "PRODUCTB", barcodes: Array.Empty<string>());
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE", "", "OWN", EntityAction.INSERT, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, "ABCEXPBNE", data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "0")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: PRODUCTB
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_New_GreaterThanMaximumShelfInXml()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productB = CreateExistingProduct(owners: Array.Empty<string>(), suppliers: new string[] { abcExporters }, productCode: "PRODUCTB", barcodes: Array.Empty<string>());
			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection("PRODUCTB"));
			xml.Append($"<PK>{productB.PK}</PK>");
			xml.Append(GetOrgPartRelationSection("ABCEXPBNE", "", "OWN", EntityAction.INSERT, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, "ABCEXPBNE", data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "5")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: PRODUCTB
Test error: Minimum shelf life 30 cannot be greater than Maximum Shelf Life 5.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_NoMaximumShelfInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetOrgPartRelationSection(clientCode, "", "OWN", EntityAction.MERGE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "5")));
			xml.Append(GetClosingProductSection());

			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 5);
			Factory.Save();

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Minimum shelf life 30 cannot be greater than Maximum Shelf Life 5.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasMaximumShelfInXmlAndDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 60);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetOrgPartRelationSection(clientCode, "", "OWN", EntityAction.MERGE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "5")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Minimum shelf life 30 cannot be greater than Maximum Shelf Life 5.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_NoMaximumShelfInXml()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 5);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetOrgPartRelationSection(clientCode, "", "OWN", EntityAction.MERGE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.MERGE));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Minimum shelf life 30 cannot be greater than Maximum Shelf Life 5.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_OnlyHasMaximumShelfInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 5);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetOrgPartRelationSection(clientCode, "", "OWN", EntityAction.MERGE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Minimum shelf life 30 cannot be greater than Maximum Shelf Life 5.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Delete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 30);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetOrgPartRelationSection(clientCode, "", "OWN", EntityAction.DELETE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "60")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.DELETE, new KeyValuePair<string, string>("MaximumShelfLife", "15")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 1 deletes
WhsProductParamsByWhsAndClient - 0 inserts, 0 updates, 1 deletes";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_OnlyProductParamsIsDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 90);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetOrgPartRelationSection(clientCode, "", "OWN", EntityAction.MERGE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "60")));
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.DELETE, new KeyValuePair<string, string>("MaximumShelfLife", "15")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 1 updates, 0 deletes
WhsProductParamsByWhsAndClient - 0 inserts, 0 updates, 1 deletes";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasInProgressPick_Owner()
		{
			TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasInProgressPickCore(OrgPartRelation.RelationshipTypes.Owner);
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasInProgressPick_Both()
		{
			TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasInProgressPickCore(OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasInProgressPick_WarehouseConsignee()
		{
			TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasInProgressPickCore(OrgPartRelation.RelationshipTypes.WarehouseConsignee);
		}

		public void TestConsigneeMinShelfLifeAcceptedValidation_Edit_HasInProgressPickCore(string relationship)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;

			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			var ownerRelation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			ownerRelation.OU_UseExpiryDate = true;

			var consignee = WhsHelper.CreateClient("CONSIGNEE");
			Factory.Save();

			var relation = ownerRelation;
			var isOwner = relationship == OrgPartRelation.RelationshipTypes.Owner || relationship == OrgPartRelation.RelationshipTypes.Both;
			if (!isOwner)
			{
				relation = WhsHelper.CreateProductClientRelationShip(consignee, data.Part1, relationship);
			}
			relation.OU_Relationship = relationship;
			Factory.Save();

			var expiryDate = ZDate.Today.AddDays(90);
			var receive = WhsHelper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = WhsHelper.CreateWhsReceiveLine(receive, data.Part1, 10m, expiryDate, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = WhsHelper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			var orderLine = WhsHelper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			WhsHelper.CreatePickNew(order);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			if (isOwner)
			{
				xml.Append(GetOrgPartRelationSection(clientCode, "", relationship, EntityAction.MERGE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			}
			else
			{
				xml.Append(GetOrgPartRelationSection(consignee.OH_Code, clientCode, relationship, EntityAction.MERGE, new KeyValuePair<string, string>("ConsigneeMinShelfLifeAccepted", "30")));
			}
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		#endregion

		#region TestMaximumShelfLifeValidation

		public void TestMaximumShelfLifeValidation_New_LessThanConsigneeMinShelfLifeAccepted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			var relation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == data.Org1.PK);
			relation.OU_ConsigneeMinShelfLifeAccepted = 30;
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "5")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Maximum shelf life 5 cannot be less than Minimum Shelf Life 30.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestMaximumShelfLifeValidation_Edit_LessThanConsigneeMinShelfLifeAccepted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			var relation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == data.Org1.PK);
			relation.OU_ConsigneeMinShelfLifeAccepted = 30;
			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 60);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "5")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Maximum shelf life 5 cannot be less than Minimum Shelf Life 30.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestMaximumShelfLifeValidation_Edit_InvalidValueIfJulianBatchNumberUsedAndHasStock_NoPKInXml()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;

			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 60);
			Factory.Save();

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "0")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Maximum Shelf Life cannot be 0 when there are stock for this product, client and warehouse currently in use.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestMaximumShelfLifeValidation_Edit_InvalidValueIfJulianBatchNumberUsedAndHasStock_HasPKInXml()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;

			WhsHelper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			WhsHelper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var productParams = WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 60);
			Factory.Save();

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			Factory.Save();

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append($"<PK>{data.Part1.PK}</PK>");
			xml.Append(GetProductParamsByWhsAndClientSection(productParams.PK, clientCode, data.Whs1.WW_WarehouseCode, EntityAction.MERGE, new KeyValuePair<string, string>("MaximumShelfLife", "0")));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Maximum Shelf Life cannot be 0 when there are stock for this product, client and warehouse currently in use.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		#endregion

		#region TestProductParamsByWhsAndClientSection_WarehouseNotSpecified

		public void TestProductParamsByWhsAndClientSection_WarehouseNotSpecified_Insert()
		{
			TestProductParamsByWhsAndClientSection_WarehouseNotSpecified_Core(EntityAction.INSERT);
		}

		public void TestProductParamsByWhsAndClientSection_WarehouseNotSpecified_Merge()
		{
			TestProductParamsByWhsAndClientSection_WarehouseNotSpecified_Core(EntityAction.MERGE);
		}

		public void TestProductParamsByWhsAndClientSection_WarehouseNotSpecified_Delete()
		{
			TestProductParamsByWhsAndClientSection_WarehouseNotSpecified_Core(EntityAction.DELETE);
		}

		void TestProductParamsByWhsAndClientSection_WarehouseNotSpecified_Core(EntityAction action)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			if (action == EntityAction.DELETE)
			{
				WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 60);
			}
			Factory.Save();

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			Factory.Save();

			var xmlInvalid = new ZStringBuilder();
			xmlInvalid.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xmlInvalid.Append($"<PK>{data.Part1.PK}</PK>");

			xmlInvalid.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, data.Org1.OH_Code, null, action));
			xmlInvalid.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlInvalid.ToStringWithNewLineBetweenAppends())));

			var expectedLogError = $@"Importing Product: P1
Test error: Invalid XML for WhsProductParamsByWhsAndClient under product P1. The warehouse is not specified.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLogError, GetLogs());

			dummyLogger.Clear();

			var xmlValid = new ZStringBuilder();
			xmlValid.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xmlValid.Append($"<PK>{data.Part1.PK}</PK>");

			xmlValid.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, action));
			xmlValid.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlValid.ToStringWithNewLineBetweenAppends())));

			string expectedLog;
			if (action == EntityAction.DELETE)
			{
				expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 0 inserts, 0 updates, 1 deletes";
			}
			else
			{
				expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes";
			}
			AssertContains(expectedLog, GetLogs());
		}

		#endregion

		#region TestProductParamsByWhsAndClientSection_OrgNotSpecified

		public void TestProductParamsByWhsAndClientSection_OrgNotSpecified_Insert()
		{
			TestProductParamsByWhsAndClientSection_OrgNotSpecified_Core(EntityAction.INSERT);
		}

		public void TestProductParamsByWhsAndClientSection_OrgNotSpecified_Merge()
		{
			TestProductParamsByWhsAndClientSection_OrgNotSpecified_Core(EntityAction.MERGE);
		}

		public void TestProductParamsByWhsAndClientSection_OrgNotSpecified_Delete()
		{
			TestProductParamsByWhsAndClientSection_OrgNotSpecified_Core(EntityAction.DELETE);
		}

		void TestProductParamsByWhsAndClientSection_OrgNotSpecified_Core(EntityAction action)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			if (action == EntityAction.DELETE)
			{
				WhsHelper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 60);
			}
			Factory.Save();

			WhsHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			Factory.Save();

			var xmlInvalid = new ZStringBuilder();
			xmlInvalid.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xmlInvalid.Append($"<PK>{data.Part1.PK}</PK>");

			xmlInvalid.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, null, data.Whs1.WW_WarehouseCode, action));
			xmlInvalid.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlInvalid.ToStringWithNewLineBetweenAppends())));

			var expectedLogError = $@"Importing Product: P1
Test error: Invalid XML for WhsProductParamsByWhsAndClient under product P1. The Organisation is not specified.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLogError, GetLogs());

			dummyLogger.Clear();

			var xmlValid = new ZStringBuilder();
			xmlValid.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xmlValid.Append($"<PK>{data.Part1.PK}</PK>");

			xmlValid.Append(GetProductParamsByWhsAndClientSection(ZGuid.Empty, data.Org1.OH_Code, data.Whs1.WW_WarehouseCode, action));
			xmlValid.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xmlValid.ToStringWithNewLineBetweenAppends())));

			string expectedLog;
			if (action == EntityAction.DELETE)
			{
				expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 0 inserts, 0 updates, 1 deletes";
			}
			else
			{
				expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes";
			}
			AssertContains(expectedLog, GetLogs());
		}

		#endregion

		#region TestDefaultInventoryHoldCodeAssignment

		public void TestDefaultInventoryHoldCodeAssignment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			Factory.Save();

			Assert("Precondition: DAM is a valid inventory hold code.", Factory.Load<WhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Any());

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSectionWithDefaultInventoryHoldCode(clientCode, "DAM"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 1 updates, 0 deletes";
			AssertContains(expectedLog, GetLogs());
		}

		public void TestDefaultInventoryHoldCodeAssignment_InvalidHoldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			Factory.Save();

			AssertEquals("Precondition: ZZZ is not a valid inventory hold code.", false, Factory.Load<WhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "ZZZ")).Any());

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSectionWithDefaultInventoryHoldCode(clientCode, "ZZZ"));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Could not insert/update the OrgPartRelation (OrgPartRelation) as it had an invalid reference to a DefaultInventoryHoldCode (DefaultInventoryHoldCode). There is no DefaultInventoryHoldCode with the following values: [Code:ZZZ].
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		string GetOrgPartRelationSectionWithDefaultInventoryHoldCode(string ownerOrgCode, string defaultInventoryHoldCode, string relationship = "OWN", EntityAction action = EntityAction.MERGE)
		{
			return $@"<ns0:OrgPartRelationCollection>
	<ns0:OrgPartRelation Action=""{Enum.GetName(typeof(EntityAction), action)}"">
		<ns0:Relationship>{relationship}</ns0:Relationship>
		<ns0:OrgHeader>
			<ns0:Code>{ownerOrgCode}</ns0:Code>
		</ns0:OrgHeader>
		<ns0:DefaultInventoryHoldCode>
			<ns0:Code>{defaultInventoryHoldCode}</ns0:Code>
		</ns0:DefaultInventoryHoldCode>
	</ns0:OrgPartRelation>
</ns0:OrgPartRelationCollection>";
		}

		#endregion

		#region TestPutawayGroupAssignment

		public void TestPutawayGroupAssignment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			var putawayGroup = Factory.New<IWhsPutawayGroup>();
			putawayGroup.WPG_Code = "ABC";
			putawayGroup.WPG_Description = "ABC Putaway Group";
			Factory.Save();

			var productParamQuery = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, data.Part1.PK);

			var newFactory1 = new BusinessObjectFactory();
			Assert("Precondition: ABC is a valid putaway group code.", newFactory1.Load<IWhsPutawayGroup>(new ZQuery(WhsPutawayGroupSchema.WPG_Code, "ABC")).Any());
			AssertEquals("Precondition: product has no WhsProductParamsByWhsAndClient.", false, newFactory1.Load<WhsProductParamsByWhsAndClient>(productParamQuery).Any());

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(clientCode));
			xml.Append(GetWhsProductParamsByWhsAndClientWithPutawayGroup(clientCode, "ABC", data.Whs1.WW_WarehouseCode));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes";
			AssertContains(expectedLog, GetLogs());

			var newFactory2 = new BusinessObjectFactory();
			var productParamByWhsAndClientInNewFactory = newFactory2.Load<WhsProductParamsByWhsAndClient>(productParamQuery).Single();
			AssertEquals("Assigned putaway group code is correct.", "ABC", productParamByWhsAndClientInNewFactory.PutawayGroup.WPG_Code);
			AssertEquals("Assigned putaway group description is correct.", "ABC Putaway Group", productParamByWhsAndClientInNewFactory.PutawayGroup.WPG_Description);
		}

		public void TestPutawayGroupAssignment_InvalidPutawayGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientCode = data.Org1.OH_Code;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals("Precondition: ABC is not a valid putaway group code.", false, newFactory.Load<IWhsPutawayGroup>(new ZQuery(WhsPutawayGroupSchema.WPG_Code, "ABC")).Any());

			var xml = new ZStringBuilder();
			xml.Append(GetOpeningProductSection(data.Part1.OP_PartNum));
			xml.Append(GetOrgPartRelationSection(clientCode));
			xml.Append(GetWhsProductParamsByWhsAndClientWithPutawayGroup(clientCode, "ABC", data.Whs1.WW_WarehouseCode));
			xml.Append(GetClosingProductSection());

			manager.Import(new MemoryStream(Encoding.Default.GetBytes(xml.ToStringWithNewLineBetweenAppends())));

			var expectedLog = $@"Importing Product: P1
Test error: Could not insert/update the WhsProductParamsByWhsAndClient (WhsProductParamsByWhsAndClient) as it had an invalid reference to a PutawayGroup (PutawayGroup). There is no PutawayGroup with the following values: [Code:ABC].
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
			AssertContains(expectedLog, GetLogs());
		}

		string GetWhsProductParamsByWhsAndClientWithPutawayGroup(string ownerOrgCode, string putawayGroupCode, string whsCode, EntityAction action = EntityAction.MERGE)
		{
			return $@"<ns0:WhsProductParamsByWhsAndClientCollection>
	<ns0:WhsProductParamsByWhsAndClient Action=""{Enum.GetName(typeof(EntityAction), action)}"">
		<ns0:OrgHeader>
			<ns0:Code>{ownerOrgCode}</ns0:Code>
		</ns0:OrgHeader>
		<ns0:WhsWarehouse>
			<ns0:WarehouseCode>{whsCode}</ns0:WarehouseCode>
		</ns0:WhsWarehouse>
		<ns0:PutawayGroup>
			<ns0:Code>{putawayGroupCode}</ns0:Code>
		</ns0:PutawayGroup>
	</ns0:WhsProductParamsByWhsAndClient>
</ns0:WhsProductParamsByWhsAndClientCollection>";
		}

		#endregion

		#region TestRelationshipCLS

		public void TestRelationshipCLS()
		{
			var factory = new BusinessObjectFactory();
			var classifier = factory.New<OrgHeader>();
			classifier.OH_Code = "TESTCLS01";
			factory.Save();

			RunXml("43.Relationship CLS Insert.xml");
			var logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);

			ResetLogs();
			RunXml("43.Relationship CLS Merge.xml");
			logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 0 inserts, 1 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes", logs);

			ResetLogs();
			RunXml("43.Relationship CLS Delete.xml");
			logs = GetLogs();
			AssertContains(@"OrgSupplierPart - 0 inserts, 0 updates, 1 deletes
OrgPartRelation - 0 inserts, 0 updates, 1 deletes", logs);
		}

		#endregion

		void ResetLogs()
		{
			dummyLogger.Clear();
		}

		void RunXml(string testFileName)
		{
			var data = GetXmlToImport(testFileName);
			manager.Import(data.BaseStream);
		}

		StreamReader GetXmlToImport(string testFileName)
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.ProductMatchings.TestFiles." + testFileName));
			return data;
		}

		OrgSupplierPart AssertProductNowExists(string[] owners, string[] suppliers, string productCode = "PRODUCTA")
		{
			var both = owners.Intersect(suppliers).ToArray();
			return AssertProductNowExists(owners.Where(x => !both.Contains(x)).ToArray(), suppliers.Where(x => !both.Contains(x)).ToArray(), both, productCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Testing")]
		OrgSupplierPart AssertProductNowExists(string[] owners, string[] suppliers, string[] both, string productCode = "PRODUCTA")
		{
			var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			partQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCode);
			var relatedOrganisationSubQueryOwn = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			var relatedOrganisationSubQuerySup = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			var relatedOrganisationSubQueryBth = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			foreach (var own in owners)
			{
				relatedOrganisationSubQueryOwn.AddToFilter(GetAdditionalRelationshipQuery(own, "OWN"), JoinCondition.Or);
			}
			foreach (var sup in suppliers)
			{
				relatedOrganisationSubQuerySup.AddToFilter(GetAdditionalRelationshipQuery(sup, "SUP"), JoinCondition.Or);
			}
			foreach (var bth in both)
			{
				relatedOrganisationSubQueryBth.AddToFilter(GetAdditionalRelationshipQuery(bth, "BTH"), JoinCondition.Or);
			}
			partQuery.AddSubQuery(relatedOrganisationSubQueryOwn, JoinCondition.And);
			partQuery.AddSubQuery(relatedOrganisationSubQuerySup, JoinCondition.And);
			partQuery.AddSubQuery(relatedOrganisationSubQueryBth, JoinCondition.And);
			var products = new BusinessObjectFactory().Load<OrgSupplierPart>(partQuery);
			var targetProduct = (from OrgSupplierPart p in products
								 where
								 p.RelatedOrganisations.OfType<OrgPartRelation>().Count(r => r.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner) == owners.Length
								 && p.RelatedOrganisations.OfType<OrgPartRelation>().Count(r => r.OU_Relationship == OrgPartRelation.RelationshipTypes.Supplier) == suppliers.Length
								 && p.RelatedOrganisations.OfType<OrgPartRelation>().Count(r => r.OU_Relationship == OrgPartRelation.RelationshipTypes.Both) == both.Length
								 select p).FirstOrDefault();
			AssertNotNull(targetProduct);
			return targetProduct;
		}

		ZQuery GetAdditionalRelationshipQuery(ZString orgCode, ZString relationshipType)
		{
			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCode));
			var filter = new ZQuery(OrgPartRelationSchema.OU_Relationship, relationshipType);
			filter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, org.PK);
			return filter;
		}

		OrgSupplierPart CreateExistingProduct(string[] owners, string[] suppliers, string productCode = "PRODUCTA", string[] barcodes = null, bool isActive = true)
		{
			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = productCode;
			part.OP_Desc = "Database Value";
			part.OP_IsActive = isActive;
			foreach (var owner in owners)
			{
				var ou = part.RelatedOrganisations.AddNew();
				ou.OU_Relationship = suppliers.Contains(owner) ? OrgPartRelation.RelationshipTypes.Both : OrgPartRelation.RelationshipTypes.Owner;
				ou.OU_OH = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, owner)).PK;
			}
			foreach (var supplier in suppliers.Except(owners))
			{
				var ou = part.RelatedOrganisations.AddNew();
				ou.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				ou.OU_OH = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, supplier)).PK;
			}

			if (barcodes != null)
			{
				foreach (var barcode in barcodes)
				{
					var bc = part.PartBarcodes.AddNew();
					bc.PH_Barcode = barcode;
					bc.PH_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
				}
			}
			factory.Save();
			return part;
		}

		string GetLogs()
		{
			return string.Join("\r\n", dummyLogger.Buffer.Logs().Select(log => log.Message).ToArray());
		}

		void ErrorOccur(XElement source, Exception ex)
		{
			dummyLogger.Error("Test error: " + ex.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupImporter();
			var craig = factory.New<OrgHeader>();
			craig.OH_Code = craImpChi;
			var abc = factory.New<OrgHeader>();
			abc.OH_Code = abcExporters;
			factory.Save();

			ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		}

		void SetupImporter()
		{
			sessionServices = new AncillaryImportServices();
			dummyLogger = sessionServices.Logger as MemoryLogger;
			manager = new ImportHandler(sessionServices)
			{
				ErrorOccur = ErrorOccur
			};
			factory = new BusinessObjectFactory();
			setting = new ProductEntityMatchingSetting
			{
				Enable = true
			};
			context = new UpdateContext(sessionServices, new FactoryProvider());
			setting.Context = context;
			context.InterceptorSettings.Add(setting);
			interceptor = new ProductEntityMatchingInterceptor(setting, sessionServices);
			setting.Interceptor = interceptor;
		}

		const string craImpChi = "CRAIMPCHI";
		const string baccri = "BACCRI";
		const string daecor = "DAECOR";
		const string abcExporters = "ABCEXPBNE";
		const string _4BELEV = "4BELEV";

		WhsTestHelperFunctions WhsHelper
		{
			get { return whsHelper ?? (whsHelper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions whsHelper;

		AncillaryImportServices sessionServices;
		MemoryLogger dummyLogger;
		ImportHandler manager;
		ProductEntityMatchingSetting setting;
		EntityContext context;
		BusinessObjectFactory factory;
		ProductEntityMatchingInterceptor interceptor;
	}
}
