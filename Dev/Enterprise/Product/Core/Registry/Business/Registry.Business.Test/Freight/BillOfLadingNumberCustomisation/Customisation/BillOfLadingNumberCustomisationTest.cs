using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisation))]
	sealed class BillOfLadingNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateCheckDigit()
		{
			BizObj.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;
			BizObj.UnFilteredElements.RemoveAll();
			var element1 = new BillOfLadingNumberCustomisationElement(BizObj, new CommonElementStrategy("key1", "name1", string.Empty,NumberCustomisationElementCategories.Standard, 1, null));
			var element2 = new BillOfLadingNumberCustomisationElement(BizObj, new CommonElementStrategy("key2", "name2", string.Empty, NumberCustomisationElementCategories.Standard, 2, null));
			BizObj.UnFilteredElements.Add(element1);
			BizObj.UnFilteredElements.Add(element2);

			element1.Include = true;
			element1.CheckDigit = false;
			element2.Include = true;
			element2.CheckDigit = false;
			AssertNoErrors(BizObj.CheckDigitAlgorithmInfo);

			BizObj.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.MAWB;
			BizObj.ValidateCheckDigitAlgorithm();
			element2.Validation.ValidateCheckDigit();
			AssertHasError(BizObj.CheckDigitAlgorithmInfo, string.Format("The Check Digit Algorithm is {0}. At least one element needs to be included in the check digit algorithm.", BizObj.CheckDigitAlgorithm));

			element2.CheckDigit = true;
			BizObj.ValidateCheckDigitAlgorithm();
			AssertNoErrors(BizObj.CheckDigitAlgorithmInfo);
		}

		public void TestCopyValuesFrom()
		{
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);

			BillOfLadingNumberCustomisation copy = new BillOfLadingNumberCustomisation();
			copy.CopyValuesFrom(BizObj);
			Assert(copy.UnFilteredElements.Count > 0);
			AssertNull(copy.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			AssertNull(copy.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			AssertNull(copy.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			AssertNull(copy.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);
		}

		public void TestCloned()
		{
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			BizObj.UnFilteredElements.Remove(BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);

			BillOfLadingNumberCustomisation clonedBizObj = (BillOfLadingNumberCustomisation)BizObj.Clone(null, null);
			AssertNull(clonedBizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			AssertNull(clonedBizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			AssertNull(clonedBizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			AssertNull(clonedBizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);
		}

		public void TestCategories()
		{
			BillOfLadingNumberCustomisationElement linerAgencyElement = null;
			BillOfLadingNumberCustomisationElement standardElement = null;

			foreach (BillOfLadingNumberCustomisationElement element in BizObj.UnFilteredElements)
			{
				if (linerAgencyElement == null && element.Matches(NumberCustomisationElementCategories.LinerAgency))
				{
					linerAgencyElement = element;
				}
				else if (standardElement == null && element.Matches(NumberCustomisationElementCategories.Standard))
				{
					standardElement = element;
				}

				if (linerAgencyElement != null && standardElement != null)
				{
					break;
				}
			}

			AssertNotNull("precondition: linerAgencyElement", linerAgencyElement);
			AssertNotNull("precondition: standardElement", standardElement);

			BizObj.Categories = NumberCustomisationElementCategories.Standard;
			AssertEquals("Should contain standardElement", true, BizObj.Elements.Contains(standardElement));
			AssertEquals("Should not contain linerAgencyElement", false, BizObj.Elements.Contains(linerAgencyElement));

			BizObj.Categories = NumberCustomisationElementCategories.LinerAgency;
			AssertEquals("Should not contain standardElement", false, BizObj.Elements.Contains(standardElement));
			AssertEquals("Should contain linerAgencyElement", true, BizObj.Elements.Contains(linerAgencyElement));

			BizObj.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;
			AssertEquals("Should contain standardElement", true, BizObj.Elements.Contains(standardElement));
			AssertEquals("Should contain linerAgencyElement", true, BizObj.Elements.Contains(linerAgencyElement));
		}

		public void TestCustomisedElement_LinerAgencyElements()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.LinerAgency;

			var keys = new[]
			{
								BillOfLadingNumberCustomisationElement.Keys.CarrierPrincipalCode,
								BillOfLadingNumberCustomisationElement.Keys.ContainerTranshipmentIndicator,
								BillOfLadingNumberCustomisationElement.Keys.LoadIATA,
								BillOfLadingNumberCustomisationElement.Keys.LoadUNLOCO,
								BillOfLadingNumberCustomisationElement.Keys.DischargeIATA,
								BillOfLadingNumberCustomisationElement.Keys.DischargeUNLOCO,
						};

			AssertContainsExactElementsInAnyOrder(
					keys,
					Array.ConvertAll(customisation.Elements.ToArray(), (BusinessObject x) => (string)new ZString(x["Key"]))
					);
		}

		public void TestCustomisedElement_FreightStandardElements()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.FreightStandard;

			var keys = new[]
			{
								BillOfLadingNumberCustomisationElement.Keys.DestinationIATA,
								BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO,
								BillOfLadingNumberCustomisationElement.Keys.Direction,
								BillOfLadingNumberCustomisationElement.Keys.OriginIATA,
								BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO,
								BillOfLadingNumberCustomisationElement.Keys.TransportMode,
								BillOfLadingNumberCustomisationElement.Keys.ServiceLevel,
						};

			AssertContainsExactElementsInAnyOrder(
					keys,
					Array.ConvertAll(customisation.Elements.ToArray(), (BusinessObject x) => (string)new ZString(x["Key"]))
					);
		}

		public void TestCustomisedElement_ConsolElements()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.Consol;

			var keys = new[]
			{
								BillOfLadingNumberCustomisationElement.Keys.Direction,
								BillOfLadingNumberCustomisationElement.Keys.TransportMode,
								BillOfLadingNumberCustomisationElement.Keys.FirstLoadIATA,
								BillOfLadingNumberCustomisationElement.Keys.FirstLoadUNLOCO,
								BillOfLadingNumberCustomisationElement.Keys.LastDischargeIATA,
								BillOfLadingNumberCustomisationElement.Keys.LastDischargeUNLOCO,
								BillOfLadingNumberCustomisationElement.Keys.ServiceLevel,
						};

			AssertContainsExactElementsInAnyOrder(
					keys,
					Array.ConvertAll(customisation.Elements.ToArray(), (BusinessObject x) => (string)new ZString(x["Key"]))
					);
		}

		public void TestCustomisedElement_SundryChargesElements()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.SundryCharges;

			var keys = new[]
			{
								BillOfLadingNumberCustomisationElement.Keys.SundryChargesActivity,
								BillOfLadingNumberCustomisationElement.Keys.SundryChargesMode,
								BillOfLadingNumberCustomisationElement.Keys.SundryChargesType,
						};

			AssertContainsExactElementsInAnyOrder(
					keys,
					Array.ConvertAll(customisation.Elements.ToArray(), (BusinessObject x) => (string)new ZString(x["Key"]))
					);
		}

		public void TestCustomisedElement_DomesticElements()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.Domestic;

			var keys = new[]
			{
								BillOfLadingNumberCustomisationElement.Keys.ServiceLevel
						};

			AssertContainsExactElementsInAnyOrder(
					keys,
					Array.ConvertAll(customisation.Elements.ToArray(), (BusinessObject x) => (string)new ZString(x["Key"]))
					);
		}

		public void TestCustomisedElement_StandardElements()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.Standard;

			var keys = new[]
			{
								BillOfLadingNumberCustomisationElement.Keys.BranchCode,
								BillOfLadingNumberCustomisationElement.Keys.ClientCoded1,
								BillOfLadingNumberCustomisationElement.Keys.ClientCoded2,
								BillOfLadingNumberCustomisationElement.Keys.ClientCoded3,
								BillOfLadingNumberCustomisationElement.Keys.CompanyCode,
								BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode,
								BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits,
								BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter,
								BillOfLadingNumberCustomisationElement.Keys.Quarter,
								BillOfLadingNumberCustomisationElement.Keys.SequenceNumber,
								BillOfLadingNumberCustomisationElement.Keys.ServerCode,
								BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode,
								BillOfLadingNumberCustomisationElement.Keys.YearAsDigit,
								BillOfLadingNumberCustomisationElement.Keys.YearAsLetter,
						};

			AssertContainsExactElementsInAnyOrder(
					keys,
					Array.ConvertAll(customisation.Elements.ToArray(), (BusinessObject x) => (string)new ZString(x["Key"]))
					);
		}

		public void TestMaxGeneratedLength()
		{
			BizObj.RemoveFountainPrefix = true;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "3";
			AssertEquals("Minimum MaxGeneratedLength", 3, BizObj.MaxGeneratedLength);

			for (int i = 0; i < BizObj.UnFilteredElements.Count; i++)
			{
				BillOfLadingNumberCustomisationElement element = BizObj.UnFilteredElements[i];
				element.Include = true;
				element.Order = (byte)(i + 1);
			}

			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Detail = "AAAAA";
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "8";
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Detail = "4";

			BizObj.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			BizObj.RemoveFountainPrefix = false;
			AssertEquals("MaxGeneratedLength", 172, BizObj.MaxGeneratedLength);

			BizObj.PrefixLength = 4;
			AssertEquals("MaxGeneratedLength with adjusted prefix length", 175, BizObj.MaxGeneratedLength);
		}

		public void TestMaxGeneratedLength_UniversalOfficeCode_OnCompany()
		{
			var company = Factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, DBNull.Value));
			var companyOrg = Factory.Load<IOrgHeader>(company.GC_OH_OrgProxy);

			var companyCodes = ((BusinessObject)companyOrg)["CustomsCodes"] as IDependentBusinessObjectCollection;
			var companyCode = companyCodes.AddNew();
			companyCode["OK_RN_NKCodeCountry"] = "AU";
			companyCode["OK_CodeType"] = "UOC";
			companyCode["OK_CustomsRegNo"] = "ABCDE";

			var companyCode2 = companyCodes.AddNew();
			companyCode2["OK_RN_NKCodeCountry"] = "IT";
			companyCode2["OK_CodeType"] = "UOC";
			companyCode2["OK_CustomsRegNo"] = "ABCDEFGHIJK";

			var companyCode3 = companyCodes.AddNew();
			companyCode3["OK_RN_NKCodeCountry"] = "AU";
			companyCode3["OK_CodeType"] = "CRN";
			companyCode3["OK_CustomsRegNo"] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Factory.Save();

			BizObj.RemoveFountainPrefix = true;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "3";

			AssertEquals("MaxGeneratedLength without UOC included", 3, BizObj.MaxGeneratedLength);

			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode].Include = true;

			AssertEquals("MaxGeneratedLength with additional 11 char UOC at company level", 14, BizObj.MaxGeneratedLength);
		}

		public void TestMaxGeneratedLength_UniversalOfficeCode_OnBranch()
		{
			var branch = Factory.LoadTop1<IGlbBranch>(new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, SQLComparisonOperator.NotEqual, DBNull.Value));
			var branchOrg = Factory.Load<IOrgHeader>(branch.GB_OH_OrgProxy);

			var branchCodes = ((BusinessObject)branchOrg)["CustomsCodes"] as IDependentBusinessObjectCollection;
			var branchCode = branchCodes.AddNew();
			branchCode["OK_RN_NKCodeCountry"] = "AU";
			branchCode["OK_CodeType"] = "UOC";
			branchCode["OK_CustomsRegNo"] = "ABCDEFGH";

			var branchCode2 = branchCodes.AddNew();
			branchCode2["OK_RN_NKCodeCountry"] = "IT";
			branchCode2["OK_CodeType"] = "UOC";
			branchCode2["OK_CustomsRegNo"] = "ABCDEFGHIJKLM";

			var branchCode3 = branchCodes.AddNew();
			branchCode3["OK_RN_NKCodeCountry"] = "AU";
			branchCode3["OK_CodeType"] = "CRN";
			branchCode3["OK_CustomsRegNo"] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			Factory.Save();

			BizObj.RemoveFountainPrefix = true;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "3";

			AssertEquals("MaxGeneratedLength without UOC included", 3, BizObj.MaxGeneratedLength);

			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode].Include = true;

			AssertEquals("MaxGeneratedLength with 13 char UOC at branch level", 16, BizObj.MaxGeneratedLength);
		}

		public void TestSerialisation()
		{
			BizObj.UseShipmentSequenceNumber = false;
			BizObj.RemoveFountainPrefix = false;
			BizObj.AutoAllocateMasterBillNumbersToConsols = false;
			BizObj.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;

			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Order = 50;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "3";

			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Include = true;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Order = 1;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Detail = "XXX";
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Fountain = false;

			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(BillOfLadingNumberCustomisation));
			string xml;

			using (System.IO.StringWriter stream = new System.IO.StringWriter())
			{
				serialiser.Serialize(stream, BizObj);
				xml = stream.ToString();
			}

			const string expectedXml =
					"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" +
					"<BillOfLadingNumberCustomisation>\n" +
					"  <ServiceLevel />\n" +
					"  <RemoveFountainPrefix>N</RemoveFountainPrefix>\n" +
					"  <AutoAllocateMasterBillNumbersToConsols>N</AutoAllocateMasterBillNumbersToConsols>\n" +
					"  <CheckDigitAlgorithm>NON</CheckDigitAlgorithm>\n" +
					"  <UseShipmentSequenceNumber>N</UseShipmentSequenceNumber>\n" +
					"  <Elements>\n" +
					"    <Element key=\"ClientCoded\">\n" +
					"      <Order>1</Order>\n" +
					"      <CheckDigit>Y</CheckDigit>\n" +
					"      <Detail>XXX</Detail>\n" +
					"    </Element>\n" +
					"    <Element key=\"SequenceNumber\">\n" +
					"      <Order>50</Order>\n" +
					"      <CheckDigit>Y</CheckDigit>\n" +
					"      <Detail>3</Detail>\n" +
					"    </Element>\n" +
					"  </Elements>\n" +
					"</BillOfLadingNumberCustomisation>" +
					"";

			AssertMultilineASCIIEquals("", expectedXml, xml);

			BillOfLadingNumberCustomisation obj;

			using (System.IO.StringReader stream = new System.IO.StringReader(xml))
			{
				obj = (BillOfLadingNumberCustomisation)serialiser.Deserialize(stream);
			}
		}

		public void TestEqual()
		{
			var customisationBill1 = new BillOfLadingNumberCustomisation
			{
				ServiceLevel = "ALL",
				AllowNonAlphanumericCharacters = true,
				EnableMacroInsertion = true,
				Categories = NumberCustomisationElementCategories.Default,
				PrefixLength = 1,
				CheckDigitAlgorithm = "AL1",
				UseShipmentSequenceNumber = false,
				RemoveFountainPrefix = false,
				Elements = { }
			};

			var customisationBill2 = (BillOfLadingNumberCustomisation)customisationBill1.Clone(null, null);
			AssertEquals("Precondition", true, customisationBill1.Equals(customisationBill2));

			customisationBill1.UseShipmentSequenceNumber = true;
			AssertEquals(false, customisationBill1.Equals(customisationBill2));

			customisationBill1.UseShipmentSequenceNumber = false;
			AssertEquals(true, customisationBill1.Equals(customisationBill2));

			customisationBill1.RemoveFountainPrefix = true;
			AssertEquals(false, customisationBill1.Equals(customisationBill2));
			customisationBill1.RemoveFountainPrefix = customisationBill2.RemoveFountainPrefix;

			customisationBill1.ServiceLevel = "";
			AssertEquals(false, customisationBill1.Equals(customisationBill2));

			customisationBill1.EnableMacroInsertion = false;
			AssertEquals(false, customisationBill1.Equals(customisationBill2));
		}

		public void TestCheckDigitAlgorithmComparison()
		{
			var customisationBill1 = new BillOfLadingNumberCustomisation();
			var customisationBill2 = (BillOfLadingNumberCustomisation)customisationBill1.Clone(null, null);

			customisationBill1.AllowNonAlphanumericCharacters = true;
			customisationBill2.AllowNonAlphanumericCharacters = true;

			customisationBill1.Categories = NumberCustomisationElementCategories.Default;
			customisationBill2.Categories = NumberCustomisationElementCategories.Default;

			customisationBill1.PrefixLength = 1;
			customisationBill2.PrefixLength = 1;

			customisationBill1.Elements.AddNew();
			customisationBill2.Elements.AddNew();

			customisationBill1.CheckDigitAlgorithm = "Al1";
			customisationBill2.CheckDigitAlgorithm = "Al2";

			AssertEquals("Bill of Loading Customisation should not be equal with different CheckDigitAlgorithms", false, customisationBill1.Equals(customisationBill2));

			customisationBill2.CheckDigitAlgorithm = "Al1";

			AssertEquals("Bill of Loading Customisation should be equal with the same CheckDigitAlgorithms", true, customisationBill1.Equals(customisationBill2));
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.RemoveFountainPrefix = true;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Include = true;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Order = 1;
			BizObj.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Detail = "ABCDE";

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new BillOfLadingNumberCustomisation BizObj
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLadingNumberCustomisation)base.BizObj; }
		}

		#endregion
	}
}
