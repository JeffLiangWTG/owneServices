using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.GUI.Testing
{
	[TestedType(typeof(DataImporterForm))]
	public class DataImporterFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (DataImporterForm form = NewDataImporterForm())
			{
				form.Importer = new DataImporterForTest();
				form.Show();
				Application.DoEvents();
				AssertEquals("IsReadOnlyProgressTextBox", true, form.IsReadOnlyProgressTextBox);
				AssertEquals("Form caption should be correct", ExpectedFormCaption, form.Text);
			}
		}

		public void TestTextBoxIsNotBound()
		{
			using (DataImporterForm form = NewDataImporterForm())
			{
				form.Importer = new DataImporterForTest();
				form.Show();
				Assert("The progress text box should NEVER be bound - updating the text is slow and consumes memory", form.ProgressTextBox.DataBindings.Count == 0);
			}
		}

		[RequiresSTA]
		public void TestFormCaptionConstructor()
		{
			using (DataImporterForm form = new DataImporterFormForTest("MyCaption"))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("FormCaption should be correct", form.Text, "MyCaption");
			}
		}

		[RequiresSTA]
		public void TestBusinessObjectAndFormCaptionConstructor()
		{
			using (DataImporterForm form = new DataImporterFormForTest(new DataImporterBusinessObject(new BusinessObjectFactory()), "MyCaption"))
			{
				form.Show();
				Application.DoEvents();
				AssertNotNull("Business object should be bound to the form", form.BusinessEntity);
				AssertEquals("FormCaption should be correct", form.Text, "MyCaption");
			}
		}

		[ExpectNoExceptions]
		public void TestQueryUser()
		{
			using (DataImporterForm form1 = new DataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), "MyCaption", BillingInterfaceName.Test))
			{
				var form = (INotificationSubscriberQueryUser)form1;
				form.QueryUser(null);
			}
		}

		public void TestImportFromFileAskedAboutUpdateEveryTime()
		{
			DataImporterBusinessObject importerBO = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (TempFile tempFile = TempFile.New())
			using (DataImporterForm form = new DataImporterForm(importerBO, "Test", BillingInterfaceName.Test))
			{
				ProductValueObjectDataAdapterForTest adapter = new ProductValueObjectDataAdapterForTest();
				form.Importer = new XmlDataImporter(adapter);

				form.Show();
				Application.DoEvents();

				File.WriteAllText(tempFile.Filename, testXMLBody);

				form.ImportFromFile(tempFile.Filename);
				AssertContains("Data Import completed", form.ProgressTextBox.Text);
				AssertEquals(false, adapter.IsConfirmUpdateOfExistingBusinessObjectCalled);

				form.ImportFromFile(tempFile.Filename);
				AssertEquals(true, adapter.IsConfirmUpdateOfExistingBusinessObjectCalled);

				form.ImportFromFile(tempFile.Filename);
				AssertEquals(true, adapter.IsConfirmUpdateOfExistingBusinessObjectCalled);
			}
		}

		#region Test XML

		const string testXMLBody = @"<Product xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <ProductCode>PART</ProductCode>
  <ProductDescription>descr</ProductDescription>
  <StockUnit>1</StockUnit>
  <BrandName>Brand</BrandName>
  <Model>mod</Model>
  <DecimalPlaces>1</DecimalPlaces>
  <RelatedOrganisations>
    <RelatedOrganisation>
      <Organisation EDICode=""YACZUM"" OwnerCode=""YACZUM"">
        <OrganisationDetails>
          <Name>YACHT ZUNO</Name>
          <Location Country=""Australia"" City=""Cairns"">AUCNS</Location>
          <Addresses>
            <Address AddressType=""MAIN"">
              <AddressLine1>C/- CAIRNS INTL AIRFREIGHT</AddressLine1>
              <AddressCode>Delivery Address</AddressCode>
              <Location>AUCNS</Location>
              <Sequence>1</Sequence>
              <AddressCapabilities>
                <AddressCapability AddressType=""MAIN"" />
                <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
              </AddressCapabilities>
            </Address>
          </Addresses>
        </OrganisationDetails>
      </Organisation>
      <RelationshipType>OWN</RelationshipType>
      <ClientUQ>LB</ClientUQ>
      <LocalProductNumber>local PartNumber</LocalProductNumber>
      <LocalProductDescription>local descr</LocalProductDescription>
      <UseAttribute1>true</UseAttribute1>
      <UseExpiryDate>true</UseExpiryDate>
			<ConsigneeMinShelfLifeAccepted>30</ConsigneeMinShelfLifeAccepted>
			<UsePackingDate>true</UsePackingDate>
      <RFAttributeConfirm>NON</RFAttributeConfirm>
      <LCMarkUpPercentage1>12</LCMarkUpPercentage1>
      <LCMarkUpPercentage2>13</LCMarkUpPercentage2>
      <LCMarkUpPercentage3>14</LCMarkUpPercentage3>
      <RoyaltyPercentage>36</RoyaltyPercentage>
      <Hi>4</Hi>
      <Ti>6</Ti>
    </RelatedOrganisation>
    <RelatedOrganisation>
      <Organisation EDICode=""KANCOR"" OwnerCode=""KANCOR"">
        <OrganisationDetails>
          <Name>KANEMATSU CORPORATION</Name>
          <Location Country=""Japan"" City=""Osaka"">JPOSA</Location>
          <Addresses>
            <Address AddressType=""MAIN"">
              <AddressLine1>2-15 AWAJI MACHI 4 CHOME CHUO-KU</AddressLine1>
              <AddressLine2>OSAKA, JAPAN</AddressLine2>
              <AddressCode>PST: 2-15 AWAJI MACHI 4 C</AddressCode>
              <Language>EN</Language>
              <Location>JPOSA</Location>
              <Sequence>1</Sequence>
              <AddressCapabilities>
                <AddressCapability AddressType=""MAIN"" />
                <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
              </AddressCapabilities>
            </Address>
          </Addresses>
        </OrganisationDetails>
      </Organisation>
      <RelationshipType>SUP</RelationshipType>
      <RFAttributeConfirm>NON</RFAttributeConfirm>
    </RelatedOrganisation>
  </RelatedOrganisations>
  <Barcodes>
    <Barcode>
      <PackageUQ>KG</PackageUQ>
      <BarcodeString>dfg</BarcodeString>
    </Barcode>
  </Barcodes>
  <ClientDefinedDetails>
    <OrderMultipleQty>2</OrderMultipleQty>
    <VendorPack>1</VendorPack>
    <Department>DEP</Department>
    <Division>Division</Division>
  </ClientDefinedDetails>
  <BasicStockControl>
    <WeightedCost>5</WeightedCost>
    <LastCost>20</LastCost>
    <QtyInStock>10</QtyInStock>
  </BasicStockControl>
</Product>";

		#endregion

		[RequiresSTA]
		public void TestFormCannotBeClosedDuringImport()
		{
			using (DataImporterFormForTest form = new DataImporterFormForTest(new DataImporterBusinessObject(new BusinessObjectFactory()), "MyCaption"))
			{
				form.Show();
				Application.DoEvents();
				form.BeforeImport += delegate(object sender, CancelEventArgs e)
				{
					AssertEquals("Close button is not enabled during import", false, form.CloseButton.Enabled);
					AssertEquals("ImportFromFile button is not enabled during import", false, form.ImportFromFileButton.Enabled);
					AssertEquals("OnlySaveDataWhenNoRecordsHaveErrorsCheckBox check box is not enabled during import", false, form.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Enabled);
					form.Close();
					AssertEquals("Form is not able to be closed during import", false, form.IsDisposed);
					e.Cancel = true;
				};
				form.ImportFromFile("File.dat");
			}
		}

		[RequiresSTA]
		public void TestSetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox()
		{
			using (DataImporterFormForTest form = new DataImporterFormForTest(new DataImporterBusinessObject(new BusinessObjectFactory()), "MyCaption"))
			{
				DataImporterForTest importer = new DataImporterForTest();
				form.Importer = importer;
				Assert(!importer.OnlySaveDataWhenNoRecordsHaveErrors);
				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(true, true);
				Assert(importer.OnlySaveDataWhenNoRecordsHaveErrors);
			}
		}

		public void TestImporterThrowsException()
		{
			Exception kaboom = null;

			DataImporterForTest importer = new DataImporterForTest();
			importer.DoOnImport = delegate
			{
				throw (kaboom = new Exception("Ka-BOOM!"));
			};

			DataImporterBusinessObject importerBO = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (TempFile tempFile = TempFile.New())
			using (DataImporterFormForTest form = new DataImporterFormForTest(importerBO, "Blaticus"))
			{
				form.BeforeImport += delegate(object sender, CancelEventArgs e)
				{ e.Cancel = false; };
				form.Importer = importer;
				form.Show();
				Application.DoEvents();

				File.WriteAllText(tempFile.Filename, "Blaticus", System.Text.Encoding.UTF8);

				AssertEquals("precondition:", 0, ExceptionReporterTestListener.Instance.Count);
				form.ImportFromFile(tempFile.Filename);
				AssertEquals("should have reported exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("should have reported exception", kaboom, ExceptionReporterTestListener.Instance[0]);
				ErrorReporter.Clear();

				bool allowNumericCharactersInCodeGeneration = Registry.Business.OrganisationsDataRegistry.Instance.AllowNumericCharactersInCodeGeneration.Value;
				bool useUnmatchedOrganisationForMatching = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;
				string threshold = Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value;
				string orgProxy = GlbCompany.CurrentCompany.OrgProxy.OH_Code;
				string companyCode = GlbCompany.CurrentCompany.GC_Code;
				string branchCode = GlbBranch.CurrentBranch.GB_Code;

				string expected =
					"Importing data from file [{0}]...\r\n" +
					"\r\n" +
					"Registry value for Organizations -> Allow Numeric Characters In Code Generation is currently " + (allowNumericCharactersInCodeGeneration ? "Enabled" : "Disabled") + ".\r\n" +
					"Registry value for Organizations -> Use Default Organization for Matching is currently " + (useUnmatchedOrganisationForMatching ? "Enabled" : "Disabled") + ".\r\n" +
					"Current Organization Match Threshold - " + threshold + ".\r\n" +
					"Current Organization Proxy - " + orgProxy + ".\r\n" +
					"Current Company - " + companyCode + ".\r\n" +
					"Current Branch - " + branchCode + ".\r\n" +
					"\r\n" +
					"Error: Ka-BOOM!\r\n" +
					"\r\n" +
					"\r\n" +
					"No changes were made due to the above errors. Please fix the errors and try again.\r\n" +
					"";

				AssertMultilineASCIIEquals("Log output", string.Format(expected, tempFile.Filename).Trim(), form.ProgressTextBox.Text.Trim());
			}
		}

		[RequiresSTA]
		public void TestImporterNotThrowsException()
		{
			var importer = new DataImporterForTest();
			importer.DoOnImport = (t) => throw new OperationCanceledException("Operataion is canceled!");
			var importerBO = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (var tempFile = TempFile.New())
			using (var form = new DataImporterFormForTest(importerBO, "Blaticus"))
			{
				form.BeforeImport += (sender, e) => e.Cancel = false;
				form.Importer = importer;
				form.Show();
				Application.DoEvents();

				File.WriteAllText(tempFile.Filename, "Blaticus", System.Text.Encoding.UTF8);
				form.ImportFromFile(tempFile.Filename);
				AssertEquals("should have no reported exception", "None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());

				var allowNumericCharactersInCodeGeneration = Registry.Business.OrganisationsDataRegistry.Instance.AllowNumericCharactersInCodeGeneration.Value;
				var useUnmatchedOrganisationForMatching = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;
				var threshold = Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value;
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy.OH_Code;
				var companyCode = GlbCompany.CurrentCompany.GC_Code;
				var branchCode = GlbBranch.CurrentBranch.GB_Code;

				var expected =
					"Importing data from file [{0}]...\r\n" +
					"\r\n" +
					"Registry value for Organizations -> Allow Numeric Characters In Code Generation is currently " + (allowNumericCharactersInCodeGeneration ? "Enabled" : "Disabled") + ".\r\n" +
					"Registry value for Organizations -> Use Default Organization for Matching is currently " + (useUnmatchedOrganisationForMatching ? "Enabled" : "Disabled") + ".\r\n" +
					"Current Organization Match Threshold - " + threshold + ".\r\n" +
					"Current Organization Proxy - " + orgProxy + ".\r\n" +
					"Current Company - " + companyCode + ".\r\n" +
					"Current Branch - " + branchCode + ".\r\n" +
					"\r\n" +
					"Error: Operataion is canceled!\r\n" +
					"\r\n" +
					"\r\n" +
					"No changes were made due to the above errors. Please fix the errors and try again.\r\n" +
					"";

				AssertMultilineASCIIEquals("Log output", string.Format(expected, tempFile.Filename).Trim(), form.ProgressTextBox.Text.Trim());
			}
		}

		protected virtual string ExpectedFormCaption
		{
			get { return "Data Importer"; }
		}

		#region Implementation

		protected virtual DataImporterForm NewDataImporterForm()
		{
			return (DataImporterForm)Activator.CreateInstance(FormToBashType, new DataImporterBusinessObject(new BusinessObjectFactory()), null, null);
		}

		protected override Form GetFormToBashCore()
		{
			DataImporterForm result = NewDataImporterForm();
			result.Importer = new DataImporterForTest();
			return result;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "ProgressTextBox";
		}

		#endregion
	}
}
