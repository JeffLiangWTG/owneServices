using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestCSI_NctsExportFromEC()
		{
			AssertEquals("Is EU Code", lineAddInfo.CSI_NctsExportFromECInfo.Description);
			AssertResourceStringData(lineAddInfo.CSI_NctsExportFromECInfo, lineAddInfo, "Is EU Code", "Is EU", "Is EU", "");
		}

		public void TestCSI_StatusDefaultValue()
		{
			AssertEquals("Status is empty by default", ZString.Empty, lineAddInfo.CSI_Status);
		}

		public void TestCSI_SubTypeCaptionWhenEmpty()
		{
			AssertResourceStringData(lineAddInfo.CSI_SubTypeInfo, lineAddInfo, "Kind", "Kind", "Kind", "");
		}

		public void TestCSI_SubTypeCaptionWhenINF()
		{
			lineAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertResourceStringData(lineAddInfo.CSI_SubTypeInfo, lineAddInfo, "Kind", "Kind", "Kind", "[12 02 000 000] Additional Information");
		}

		public void TestCSI_SubTypeCaptionWhenTRA()
		{
			lineAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertResourceStringData(lineAddInfo.CSI_SubTypeInfo, lineAddInfo, "Kind", "Kind", "Kind", "[12 05 000 000] Transport Document");
		}

		public void TestCSI_SubTypeCaptionWhenREF()
		{
			lineAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertResourceStringData(lineAddInfo.CSI_SubTypeInfo, lineAddInfo, "Kind", "Kind", "Kind", "[12 04 000 000] Additional Reference");
		}

		public void TestCSI_RN_NKCountryCode()
		{
			AssertResourceStringData(lineAddInfo.CSI_RN_NKCountryCodeInfo, lineAddInfo, "CCI Validate Country", "CCI Valid. Country", "CCI Country", "Only used in CCI. Identify the country who will validate the data");
		}

		public void TestLookups()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					var addInfo = invoiceLine.AdditionalInfos.AddNew();
					AssertType<AdditionalInfoLookups>("NoUCC6, EXP", addInfo.Lookups);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					addInfo = invoiceLine.AdditionalInfos.AddNew();
					AssertType<Ucc6AdditionalInfoLookup>("NoUCC6, IMP", addInfo.Lookups);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					var addInfo = invoiceLine.AdditionalInfos.AddNew();
					AssertType<Ucc6AdditionalInfoLookup>("UCC6, EXP", addInfo.Lookups);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					addInfo = invoiceLine.AdditionalInfos.AddNew();
					AssertType<Ucc6AdditionalInfoLookup>("UCC6, IMP", addInfo.Lookups);
				}
			});
		}

		public void TestCopyFrom()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = "BLT";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var addInf1 = Factory.New<AdditionalInfo>();
			addInf1.CSI_Code = "1234";
			addInf1.CSI_SubType = "TRA";
			addInf1.CSI_ReferenceNumber = "REFERENCE";
			addInf1.CSI_Description = "description";
			addInf1.CSI_ReferenceNumber2 = "REFERENCE2";
			addInf1.CSI_Value = 20;
			addInf1.CSI_RX_NKCurrency = "EUR";
			addInf1.CSI_DataModel = Core.Constants.CountryCodes.Spain;
			declaration.AdditionalInfos.Add(addInf1);
			var readOnlyAdditionalInfoCollection = new ReadOnlyAdditionalInfoCollection(entryLine);
			readOnlyAdditionalInfoCollection.LoadNew();
			var readOnlyAdditionalInfo = readOnlyAdditionalInfoCollection.Cast<ReadOnlyAdditionalInfo>().First();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null readonlyaddinf", () => AdditionalInfo.CopyFrom(null));

				var addInf = AdditionalInfo.CopyFrom(readOnlyAdditionalInfo);
				AssertEquals("CSI_Code", readOnlyAdditionalInfo.CSI_Code, addInf.CSI_Code);
				AssertEquals("CSI_Description", readOnlyAdditionalInfo.CSI_Description, addInf.CSI_Description);
				AssertEquals("CSI_SubType", readOnlyAdditionalInfo.CSI_SubType, addInf.CSI_SubType);
				AssertEquals("CSI_ReferenceNumber", readOnlyAdditionalInfo.CSI_ReferenceNumber, addInf.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2", readOnlyAdditionalInfo.CSI_ReferenceNumber2, addInf.CSI_ReferenceNumber2);
				AssertEquals("CSI_RX_NKCurrency", readOnlyAdditionalInfo.CSI_RX_NKCurrency, addInf.CSI_RX_NKCurrency);
				AssertEquals("CSI_Value", readOnlyAdditionalInfo.CSI_Value, addInf.CSI_Value);
				AssertEquals("CSI_DataModel", readOnlyAdditionalInfo.CSI_DataModel, addInf.CSI_DataModel);
			});
		}

		void AssertResourceStringData(ZPropertyInfo info, AdditionalInfo additionalInfo, string caption, string mediumCaption, string shortCaption, string fullDescription)
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info, new DataBoundBusinessObject(additionalInfo));
				AssertEquals("Caption", caption, captionResourceString.Caption);
				AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
				AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
			});
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			var line = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var add = line.AdditionalInfos.AddNew();
			yield return add;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var add = line.AdditionalInfos.AddNew();
			return add;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			lineAddInfo = invoiceLine.AdditionalInfos.AddNew();
		}

		JobDeclaration declaration;
		AdditionalInfo lineAddInfo;
		JobComInvoiceLine invoiceLine;
	}
}
