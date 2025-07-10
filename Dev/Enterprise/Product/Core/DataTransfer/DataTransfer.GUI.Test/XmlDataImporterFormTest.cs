using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DataTransfer.GUI.Testing
{
	[TestedType(typeof(XmlDataImporterForm))]
	sealed class XmlDataImporterFormTest : DataImporterFormTest
	{
		protected override DataImporterForm NewDataImporterForm()
		{
			return XmlDataImporterForm.Create(BillingInterfaceName.Test);
		}

		protected override Form GetFormToBashCore()
		{
			return NewDataImporterForm();
		}

		protected override string ExpectedFormCaption
		{
			get { return "XML Data Importer"; }
		}

		public void TestPromptUserAndImportWithOnlySaveDataWhenNoRecordsHaveErrorsCheckBox()
		{
			new XmlDataTransferDirector(new OrganisationValueObjectDataAdapter(), false).PromptUserAndImport("", BillingInterfaceName.Test);
			Assert(!((XmlDataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);

			new XmlDataTransferDirector(new TestOrganisationValueObjectDataAdapter(), false).PromptUserAndImport("", BillingInterfaceName.Test);
			Assert(((XmlDataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);

			new XmlDataTransferDirector(new OrganisationValueObjectDataAdapter(), false).PromptUserAndImport(BillingInterfaceName.Test);
			Assert(!((XmlDataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);

			new XmlDataTransferDirector(new TestOrganisationValueObjectDataAdapter(), false).PromptUserAndImport(BillingInterfaceName.Test);
			Assert(((XmlDataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);
		}

		public void TestNoExceptionForOpeningYesNoAllFormDuringTransaction()
		{
			var importerBO = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (var tempFile = TempFile.New())
			using (var form = new XmlDataImporterForm(importerBO, "Test", BillingInterfaceName.Test))
			{
				form.Show();
				Application.DoEvents();

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "1234";
				orgHeader.OH_FullName = "Imported Name";
				orgHeader.OH_RL_NKClosestPort = "AUSYD";
				orgHeader.MainAddress.OA_Address1 = "splaty";
				Factory.Save();

				const string xml = @"<Organisations>
	<Organisation EDICode='1234' OwnerCode=''>
		<OrganisationDetails>
			<Name>Imported Name</Name>
			<Location>AUSYD</Location>
			<Addresses>
				<Address AddressType='MAIN'>
					<AddressLine1>splaty</AddressLine1>
					<TelephoneNumbers />
					<Sequence>1</Sequence>
				</Address>
			</Addresses>
		</OrganisationDetails>
	</Organisation>
</Organisations>";

				System.IO.File.WriteAllText(tempFile.Filename, xml);

				var adapter = new TestOrganisationValueObjectDataAdapter();
				var director = new XmlDataTransferDirector(adapter, false);
				AssertNoExceptionThrown(() => director.PromptUserAndImport(tempFile.Filename, BillingInterfaceName.Test));
			}
		}

		public class TestOrganisationValueObjectDataAdapter : OrganisationValueObjectDataAdapter
		{
			public override bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked
			{
				get { return true; }
			}

			public override bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible
			{
				get { return true; }
			}
		}
	}
}
