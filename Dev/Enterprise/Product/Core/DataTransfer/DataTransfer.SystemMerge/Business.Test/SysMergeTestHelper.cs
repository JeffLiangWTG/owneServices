using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Cus = Enterprise.Customs.Business;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.Business.Testing
{
	internal class SysMergeTestHelper
	{
		public SysMergeTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public OrgSupplierPart GetNewProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			UNDGSubstance dgSubstance = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceStandardTypes.IMO));

			product.OP_AutoPrintAssemblyInstructions = true;
			product.OP_Brand = "";
			product.OP_CountDecimalPlaces = 3;
			product.OP_Cubic = 1;
			product.OP_CubicUQ = "M3";
			product.OP_CustomAttrib4 = "";
			product.OP_CustomAttrib5 = "CA5";
			product.OP_CustomDate1 = ZDateTime.MinSmallDateTimeValue;
			product.OP_CustomDate2 = new ZDateTime(2008, 12, 19, 14, 23, 41);
			product.OP_CustomDecimal2 = 3;
			product.OP_CustomDecimal3 = 0;
			product.OP_CustomFlag3 = true;
			product.OP_Depth = 2;
			product.OP_Desc = "Test Part";
			product.UNDGs.AddNew().DI_DG = dgSubstance.PK;
			product.OP_Height = .5;
			product.OP_IsActive = true;
			product.OP_MeasureUQ = "M";
			product.OP_PartNum = "TP1";
			product.OP_RH_NKCommodityCode = "ALUM";
			product.OP_StockKeepingUnit = "BOX";
			product.OP_Weight = 5;
			product.OP_WeightUQ = "KG";
			product.OP_Width = 1;
			product.OP_KitIsAutoReplenished = true;
			return product;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public OrgSupplierPart GetNewProductWithClassifications()
		{
			var product = GetNewProduct();

			#region OrgPartRelation

			OrgHeader orgA = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgPartRelation relation1 = product.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation1.OU_OH = orgA.PK;
			relation1.OU_LocalPartNumber = "LPN1";
			relation1.OU_Hi = 1;
			relation1.OU_Ti = 11;
			relation1.OU_LocalPartDescription = "Desc 1";
			relation1.OU_LandedCostMarginPercent1 = 1.1;
			relation1.OU_FormLayoutController = true;
			relation1.OU_ClientUQ = "UQ1";
			relation1.OU_RoyaltyFlatAmount = 1.11;
			relation1.OU_RX_NKRoyaltyCurrency = "AUD";
			relation1.OU_UsePartAttrib1 = true;
			relation1.OU_UsePartAttrib2 = true;
			relation1.OU_UsePartAttrib3 = true;
			relation1.OU_UseExpiryDate = true;
			relation1.OU_ConsigneeMinShelfLifeAccepted = 31;
			relation1.OU_UsePackingDate = true;
			relation1.OU_PickMode = "ANE";
			relation1.OU_CompletePalletPicking = true;
			relation1.OU_RollUpAttributesOnDocuments = true;
			relation1.OU_ExpiryDateFormatString = "dd-MMM-yyyy";
			relation1.OU_PackingDateFormatString = "dd/mm/yy";

			OrgHeader orgB = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			OrgPartRelation relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = orgB.PK;
			relation2.OU_LocalPartNumber = "LPN2";
			relation2.OU_Ti = 2;
			relation2.OU_LandedCostMarginPercent2 = 2.2;
			relation2.OU_UsePartAttrib1 = false;
			relation2.OU_RoyaltyPercent = 2.222;

			OrgPartRelation relation3 = product.RelatedOrganisations.AddNew();
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			relation3.OU_OH = orgA.PK;
			relation3.OU_LocalPartNumber = "LPN3";
			relation3.OU_Hi = 3;
			relation3.OU_LocalPartDescription = "Desc 3";
			relation3.OU_LandedCostMarginPercent3 = 3;
			relation3.OU_UseExpiryDate = true;
			relation1.OU_ConsigneeMinShelfLifeAccepted = 33;
			relation3.OU_RX_NKRoyaltyCurrency = "NZD";

			OrgHeader orgC = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			OrgPartRelation relation4 = product.RelatedOrganisations.AddNew();
			relation4.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation4.OU_OH = orgC.PK;
			relation4.OU_LocalPartNumber = "LPN4";
			relation4.OU_UsePartAttrib2 = false;

			OrgHeader orgD = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D"));
			OrgPartRelation relation5 = product.RelatedOrganisations.AddNew();
			relation5.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation5.OU_OH = orgD.PK;
			relation5.OU_LocalPartNumber = "LPN5";

			#endregion

			#region OrgPartLocation

			OrgPartLocation location1 = product.Locations.AddNew();
			location1.OR_Warehouse = "WH1";
			location1.OR_Hi = 3;
			location1.OR_StockTakeCount = 1.1;

			OrgPartLocation location2 = product.Locations.AddNew();
			location2.OR_Warehouse = "WH2";
			location2.OR_BinLocation = "BL2";
			location2.OR_Ti = 22;
			location2.OR_InStock = 2.2;

			#endregion

			#region OrgPartUnit

			OrgPartUnit partUnit1 = product.PartUnits.AddNew();
			partUnit1.OF_PackType = "PK1";
			partUnit1.OF_ParentPackType = "PP1";
			partUnit1.OF_Weight = 1.1;
			partUnit1.OF_NoOfSKUsInThisPack = 11;

			OrgPartUnit partUnit2 = product.PartUnits.AddNew();
			partUnit2.OF_PackType = "PK2";
			partUnit2.OF_QuantityInParent = 22.2;
			partUnit2.OF_Cubic = 2.2;

			OrgPartUnit partUnit3 = product.PartUnits.AddNew();
			partUnit3.OF_PackType = "PK3";
			partUnit3.OF_Height = 3.333;
			partUnit3.OF_Depth = 33.3;
			partUnit3.OF_Width = 333;

			#endregion

			#region OrgSupplierPartBarcode

			OrgSupplierPartBarcode partBarcode1 = product.PartBarcodes.AddNew();
			partBarcode1.PH_Barcode = "BC1";
			partBarcode1.PH_F3_NKPackType = "PK1";

			#endregion

			#region CusClassification

			var oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AR";

			var classif1 = product.Factory.New<Cus.BaseCusClassification>();
			classif1.CC_AddInfo = "AI1";
			classif1.CC_ClassificationType = "IMP";
			classif1.CC_LastAuditedDate = new ZDateTime(2008, 12, 29, 13, 26, 21);
			classif1.CC_LastAuditedUser = "U1";
			classif1.CC_LookupCode = "LC1";
			classif1.CC_RN_NKCountryCode = "AR";
			classif1.CC_TariffNum = "1";
			classif1.CC_Description = "Desc 1";
			classif1.CC_IsActive = false;
			classif1.CC_IsUnpublished = true;

			var pivot1 = product.Factory.New<Cus.BaseCusClassPartPivot>();
			pivot1.CI_OP = product.PK;
			pivot1.CI_CC = classif1.PK;
			pivot1.CI_TariffNum = "PTN1";
			pivot1.CI_AddInfo = "PAI1";
			pivot1.CI_LastAuditedDate = new ZDateTime(2008, 12, 28, 14, 33, 23);
			pivot1.CI_LastAuditedUser = "PU1";
			pivot1.CI_RN_NKCountry = "AR";
			pivot1.CI_TariffChangePending = false;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";

			var classif2 = product.Factory.New<Cus.BaseCusClassification>();
			classif2.CC_ClassificationType = "IMP";
			classif2.CC_Description = "Desc 2";
			classif2.CC_IsActive = false;
			classif2.CC_LastAuditedUser = "U2";
			classif2.CC_TariffChangePending = true;

			var pivot2 = product.Factory.New<Cus.BaseCusClassPartPivot>();
			pivot2.CI_OP = product.PK;
			pivot2.CI_CC = classif2.PK;
			pivot2.CI_TariffNum = "PTN2";
			pivot2.CI_AddInfo = "PAI2";
			pivot2.CI_LastAuditedDate = new ZDateTime(2008, 12, 28, 14, 33, 23);
			pivot2.CI_RN_NKCountry = "US";

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";

			var classif3 = product.Factory.New<Cus.BaseCusClassification>();
			classif3.CC_ClassificationType = "IMP";
			classif3.CC_AddInfo = "AI3";
			classif3.CC_IsUnpublished = true;
			classif3.CC_LastAuditedDate = new ZDateTime(2008, 11, 15, 11, 02, 00);
			classif3.CC_LookupCode = "LC3";
			classif3.CC_TariffChangePending = false;

			var pivot3 = product.Factory.New<Cus.BaseCusClassPartPivot>();
			pivot3.CI_OP = product.PK;
			pivot3.CI_CC = classif3.PK;
			pivot3.CI_TariffNum = "PTN3";
			pivot3.CI_LastAuditedUser = "PU3";

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "PT";

			var classif4 = product.Factory.New<Cus.BaseCusClassification>();
			classif4.CC_ClassificationType = "IMP";
			classif4.CC_Description = "Desc 4";
			classif4.CC_RN_NKCountryCode = "PT";

			var pivot4 = product.Factory.New<Cus.BaseCusClassPartPivot>();
			pivot4.CI_OP = product.PK;
			pivot4.CI_CC = classif4.PK;
			pivot4.CI_TariffNum = "PTN4";
			pivot4.CI_TariffChangePending = true;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = oldCountryCode;

			#endregion

			#region CPDecAnswers

			Cus.BaseCusEntryCPDec answer = product.Factory.New<Cus.BaseCusEntryCPDec>();
			answer.ON_ParentID = product.PK;
			answer.ON_ParentTableCode = OrgSupplierPartSchema.Constants.Prefix;
			answer.ON_CPDecNum = 155;
			answer.ON_AnswerCode = "Y";
			answer.ON_Permit = ZString.Empty;

			answer = product.Factory.New<Cus.BaseCusEntryCPDec>();
			answer.ON_ParentID = product.PK;
			answer.ON_ParentTableCode = OrgSupplierPartSchema.Constants.Prefix;
			answer.ON_CPDecNum = 177;
			answer.ON_AnswerCode = "N";
			answer.ON_Permit = ZString.Empty;

			answer = product.Factory.New<Cus.BaseCusEntryCPDec>();
			answer.ON_ParentID = product.PK;
			answer.ON_ParentTableCode = OrgSupplierPartSchema.Constants.Prefix;
			answer.ON_CPDecNum = 199;
			answer.ON_AnswerCode = "Y";
			answer.ON_Permit = "12345";

			answer = product.Factory.New<Cus.BaseCusEntryCPDec>();
			answer.ON_ParentID = ZGuid.NewZGuid(); // Should be skipped on export
			answer.ON_ParentTableCode = OrgSupplierPartSchema.Constants.Prefix;
			answer.ON_CPDecNum = 211;
			answer.ON_AnswerCode = "Y";
			answer.ON_Permit = ZString.Empty;

			answer = product.Factory.New<Cus.BaseCusEntryCPDec>();
			answer.ON_ParentID = product.PK;
			answer.ON_ParentTableCode = "XX"; // Should be skipped on export
			answer.ON_CPDecNum = 233;
			answer.ON_AnswerCode = "Y";
			answer.ON_Permit = ZString.Empty;

			#endregion

			return product;
		}

		public Cus.BaseCusClassification ActiveClassificationWithCPDecAnswers
		{
			get
			{
				if (activeClassificationWithCPDecAnswers == null)
				{
					activeClassificationWithCPDecAnswers = Factory.New<Cus.BaseCusClassification>();
					activeClassificationWithCPDecAnswers.CC_AddInfo = "AI3";
					activeClassificationWithCPDecAnswers.CC_IsUnpublished = true;
					activeClassificationWithCPDecAnswers.CC_LastAuditedDate = new ZDateTime(2008, 11, 15, 11, 02, 00);
					activeClassificationWithCPDecAnswers.CC_LastAuditedUser = "UR";
					activeClassificationWithCPDecAnswers.CC_LookupCode = "LC3";
					activeClassificationWithCPDecAnswers.CC_TariffNum = "123.234";
					activeClassificationWithCPDecAnswers.CC_TariffChangePending = false;
					activeClassificationWithCPDecAnswers.CC_RN_NKCountryCode = "AR";
					activeClassificationWithCPDecAnswers.CC_ClassificationType = "IMP";

					Cus.BaseCusEntryCPDec answer = Factory.New<Cus.BaseCusEntryCPDec>();
					answer.ON_ParentID = activeClassificationWithCPDecAnswers.PK;
					answer.ON_ParentTableCode = CusClassificationSchema.Constants.Prefix;
					answer.ON_CPDecNum = 155;
					answer.ON_AnswerCode = "Y";
					answer.ON_Permit = "31";

					Cus.BaseCusEntryCPDec answer2 = Factory.New<Cus.BaseCusEntryCPDec>();
					answer2.ON_ParentID = activeClassificationWithCPDecAnswers.PK;
					answer2.ON_ParentTableCode = CusClassificationSchema.Constants.Prefix;
					answer2.ON_CPDecNum = 20;
					answer2.ON_AnswerCode = "N";
					answer2.ON_Permit = "";
				}
				return activeClassificationWithCPDecAnswers;
			}
		}
		Cus.BaseCusClassification activeClassificationWithCPDecAnswers;

		public Cus.BaseCusClassification InactiveClassification
		{
			get
			{
				if (inactiveClassification == null)
				{
					inactiveClassification = Factory.New<Cus.BaseCusClassification>();
					inactiveClassification.CC_ClassificationType = "CT2";
					inactiveClassification.CC_Description = "Desc 2";
					inactiveClassification.CC_IsActive = false;
					inactiveClassification.CC_LastAuditedUser = "U2";
					inactiveClassification.CC_TariffChangePending = true;
				}
				return inactiveClassification;
			}
		}
		Cus.BaseCusClassification inactiveClassification;

		public void AssertCusClassificationXSD(Xsd.CusClassification xsdClassification, Cus.BaseCusClassification classification, bool isActiveIsTrue)
		{
			Assertion.AssertEquals("PK", classification.PK.ToString(), xsdClassification.PK);
			Assertion.AssertEquals("AddInfo", classification.CC_AddInfo, xsdClassification.AddInfo);
			Assertion.AssertEquals("ClassificationType", classification.CC_ClassificationType, xsdClassification.ClassificationType);
			Assertion.AssertEquals("Description", classification.CC_Description, xsdClassification.Description);
			if (isActiveIsTrue)
			{
				Assertion.AssertEquals("IsActive", true, classification.CC_IsActive);
			}
			else
			{
				Assertion.AssertEquals("IsActive", classification.CC_IsActive, xsdClassification.IsActive);
			}
			Assertion.AssertEquals("IsUnpublished", classification.CC_IsUnpublished, xsdClassification.IsUnpublished);
			Assertion.AssertEquals("LastAuditedDate", classification.CC_LastAuditedDate, xsdClassification.LastAuditedDate);
			Assertion.AssertEquals("LastAuditedUser", classification.CC_LastAuditedUser, xsdClassification.LastAuditedUser);
			Assertion.AssertEquals("LookupCode", classification.CC_LookupCode, xsdClassification.LookupCode);
			Assertion.AssertEquals("TariffChangePending", classification.CC_TariffChangePending, xsdClassification.TariffChangePending);
			Assertion.AssertEquals("TariffNum", classification.CC_TariffNum, xsdClassification.TariffNum);

			if (xsdClassification.RN_Code.IsEmpty)
			{
				Assertion.AssertEquals("Country", ZString.Empty, classification.CC_RN_NKCountryCode);
			}
			else
			{
				Assertion.AssertEquals("Country", classification.CC_RN_NKCountryCode, xsdClassification.RN_Code);
			}
		}

		public void AssertCPDecAnswerXSD(Xsd.CPDecAnswer xsdAnswer, Cus.BaseCusEntryCPDec cpDecAnswer)
		{
			Assertion.AssertNotNull("CpDecAnswer should be fond", cpDecAnswer);
			Assertion.AssertEquals("PK", cpDecAnswer.PK.ToString(), xsdAnswer.PK);
			Assertion.AssertEquals("Answer Code", cpDecAnswer.ON_AnswerCode, xsdAnswer.AnswerCode);
			Assertion.AssertEquals("CPDec Num", cpDecAnswer.ON_CPDecNum, xsdAnswer.CPDecNum);
			Assertion.AssertEquals("Permit", cpDecAnswer.ON_Permit, xsdAnswer.Permit);
		}

		#region Write XML

		public string WriteValueObjectToXml(IValueObject valueObj, XmlValueObjectSerializer serializer, IValueObjectDataAdapter adapter)
		{
			StringWriter writer = new UTF8StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.Indented;

			serializer.WriteToXml(xmlWriter, adapter, valueObj, new NotificationBuffer());
			xmlWriter.Flush();
			writer.Flush();
			string result = writer.GetStringBuilder().ToString();

			return result;
		}

		public class UTF8StringWriter : StringWriter
		{
			public override Encoding Encoding
			{
				get { return Encoding.UTF8; }
			}
		}

		#endregion
	}
}
