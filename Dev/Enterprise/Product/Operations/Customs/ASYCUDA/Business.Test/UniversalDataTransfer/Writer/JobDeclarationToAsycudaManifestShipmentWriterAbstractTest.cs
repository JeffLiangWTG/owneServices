using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using NUnit.Framework;
using CustomsCharges = Enterprise.Core.Constants.Customs.CustomsCharges;
using DataType = Enterprise.UniversalDataBuss.DataObjects.DataType;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationToAsycudaManifestShipmentWriter))]
	public abstract class JobDeclarationToAsycudaManifestShipmentWriterAbstractTest : TestCaseWithFactory
	{
		protected JobDeclarationToAsycudaManifestShipmentWriterAbstractTest()
		{ }

		public void TestCreateXmlDuty_AddExciseValueToDutyAmount()
		{
			AssertXmlData(true, false, CreateXmlDuty_AddExciseValueToDutyAmountResource);
		}

		public void TestCreateXmlDuty_DoNotAddExciseValueToDutyAmount()
		{
			AssertXmlData(false, false, CreateXmlDuty_DoNotAddExciseValueToDutyAmountResource);
		}

		public void TestPopulateCustomFeilds()
		{
			var importer = CreateOrg("IMPORG");
			var supplier = CreateOrg("SUPORG");
			var notify = CreateOrg("NTFORG");

			var declaration = Factory.New<BaseJobDeclaration>();
			AddDeclarationData(declaration, supplier, importer, notify, false);

			declaration.SetUserDefinedValue("CustomField1", (ZString)"Hello World");

			var invoiceHeader1 = CreateInvoiceHeader(declaration, 1);

			CreateInvoiceLine(invoiceHeader1.JobComInvoiceLines.AddNew(), 1, 1000);
			CreateInvoiceLine(invoiceHeader1.JobComInvoiceLines.AddNew(), 2, 2000);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AddEntryDetails(declaration);

			Factory.Save();

			IDataWritingManager writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
			var writer = CreateWriter(writingManager);
			var shipment = writer.GetDataObject(declaration);

			AssertNull(shipment.CustomizedFieldCollection);
			shipment.SubShipmentCollection[0].CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "CustomField1", "Hello World");
		}

		protected virtual string CreateXmlDuty_AddExciseValueToDutyAmountResource => TestFiles.GetTestFilePath("DeclarationToManifestEvent.xml");

		protected virtual string CreateXmlDuty_DoNotAddExciseValueToDutyAmountResource => TestFiles.GetTestFilePath("DeclarationToManifestEvent_NoExcise.xml");

		protected void AssertXmlData(bool addExciseValueToDutyAmount, bool excludeImporterAndSupplier, string expectedResource, Action<BaseJobDeclaration> setupAdditionalData = null)
		{
			using (ManifestCustomsDataRegistry.Instance.AddExciseValueToDutyAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, addExciseValueToDutyAmount))
			{
				string actualText = CreateXml(excludeImporterAndSupplier, setupAdditionalData);
				string expectedText = AsycudaManifestUniversalMessagingHelperTest.LoadSampleUxml(CurrentAssembly, expectedResource);
				AssertMultilineASCIIEquals(expectedText, actualText);
			}
		}

		protected virtual Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

		protected string CreateXml(bool excludeImporterAndSupplier, Action<BaseJobDeclaration> setupAdditionalData)
		{
			var importer = CreateOrg("IMPORG");
			var supplier = CreateOrg("SUPORG");
			var notify = CreateOrg("NTFORG");

			var declaration = Factory.New<BaseJobDeclaration>();
			AddDeclarationData(declaration, supplier, importer, notify, excludeImporterAndSupplier);

			var invoiceHeader1 = CreateInvoiceHeader(declaration, 1);

			CreateInvoiceLine(invoiceHeader1.JobComInvoiceLines.AddNew(), 1, 1000);
			CreateInvoiceLine(invoiceHeader1.JobComInvoiceLines.AddNew(), 2, 2000);
			setupAdditionalData?.Invoke(declaration);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AddEntryDetails(declaration);

			IDataWritingManager writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
			var writer = CreateWriter(writingManager);
			var shipment = writer.GetDataObject(declaration);

			using (var mem = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(shipment, mem);
				using (var streamReader = new StreamReader(mem))
				{
					return streamReader.ReadToEnd();
				}
			}
		}

		protected virtual void AddDeclarationData(BaseJobDeclaration declaration, OrgHeader supplier, OrgHeader importer, OrgHeader notify, bool excludeImporterAndSupplier)
		{
			declaration.JE_MasterBill = "MB12345";
			declaration.JE_HouseBill = "HB12345";
			if (!excludeImporterAndSupplier)
			{
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_OH_Supplier = supplier.PK;
			}
			declaration.JE_OH_NotifyParty = notify.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			AddCharge(declaration.TopGroupInvoice.Charges, CustomsCharges.Codes.OverseasFreight, 500);
			AddCharge(declaration.TopGroupInvoice.Charges, CustomsCharges.Codes.OverseasInsurance, 600);
		}

		protected virtual JobDeclarationToAsycudaManifestShipmentWriter CreateWriter(IDataWritingManager writingManager) => new JobDeclarationToAsycudaManifestShipmentWriter(writingManager);

		protected virtual void AddEntryDetails(BaseJobDeclaration declaration)
		{
		}

		protected OrgHeader CreateOrg(ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = $"{code} company";
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "1234";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_Address1 = $"Some address 1 ({code})";
			org.MainAddress.OA_Address2 = $"Some address 2 ({code})";
			AddRegistrationNumbers(org);

			var companyData = org.CompanyDataCollection.Cast<OrgCompanyData>().FirstOrDefault(x => x.OB_GC == GlbCompany.CurrentCompany.PK);
			if (companyData == null)
			{
				companyData = org.CompanyDataCollection.AddNew();
				companyData.OB_GC = GlbCompany.CurrentCompany.PK;
			}
			companyData.OB_AREftCustomsPaymentMethod = SGPayeeIndicatorList.Codes.Q;

			return org;
		}

		protected virtual void AddRegistrationNumbers(OrgHeader org)
		{
		}

		protected virtual BaseJobComInvoiceHeader CreateInvoiceHeader(BaseJobDeclaration declaration, int num)
		{
			var invoiceHeader = declaration.Invoices.AddNew();

			invoiceHeader.JZ_InvoiceNumber = $"Inv00{num}";
			var charges = invoiceHeader.Charges;
			AddCharge(charges, CustomsCharges.Codes.OverseasInsurance, num * 5);
			AddCharge(charges, CustomsCharges.Codes.OverseasFreight, num * 10);
			AddCharge(charges, CustomsCharges.Codes.Discount, num * 20);
			AddCharge(charges, CustomsCharges.Codes.OtherCharges, num * 30);
			return invoiceHeader;
		}

		protected void AddCharge(IJobComInvChargeCollection<JobComInvCharge> charges, ZString chargeType, ZDecimal value)
		{
			charges.AddNew(chargeType, value);
		}

		protected virtual BaseJobComInvoiceLine CreateInvoiceLine(BaseJobComInvoiceLine invoiceLine, int num, ZDecimal linePrice)
		{
			invoiceLine.JI_PartNo = $"Part 0{num}";
			invoiceLine.JI_Description = $"Line Description 0{num}";
			invoiceLine.JI_LinePrice = linePrice;
			invoiceLine.JI_InvoiceQuantity = num * 11;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsUnitQty = "G";
			invoiceLine.JI_CountryOfOrigin = "AU";

			return invoiceLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode);
		}
		IDisposable countrySetter;

		protected virtual string CountryCode => Core.Constants.CountryCodes.Fiji;

		protected override void TearDown()
		{
			if (countrySetter != null)
			{
				countrySetter.Dispose();
				countrySetter = null;
			}
			base.TearDown();
		}
	}
}
