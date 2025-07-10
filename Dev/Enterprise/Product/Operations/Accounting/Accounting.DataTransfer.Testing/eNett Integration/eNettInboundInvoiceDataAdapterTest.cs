using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	[TestedType(typeof(eNettInboundInvoiceDataAdapter))]
	sealed class eNettInboundInvoiceDataAdapterTest : FinancialInvoiceXmlDataAdapterTest
	{
		[TestDate(2006, 01, 05)]
		public void TestImportFromENett()
		{
			if (IsImportFromValueObjectSupported)
			{
				ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_HouseBill = "TESTHOUSE";
				JobHeader job1 = new JobHeader.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
				job1.JH_GE = GlbDepartment.CurrentDepartment.PK;

				ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment2.JS_HouseBill = "OTHERHOUSE";
				JobHeader job2 = new JobHeader.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
				job2.JH_GE = GlbDepartment.CurrentDepartment.PK;

				Factory.Save();

				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();

				//IsCreatedByENett not setted
				TxnHeader invoice = GetFullyPopulatedXmlInvoice_AR();
				invoice.TxnLines[0].ConsolOrJobTypeSpecified = true;
				invoice.TxnLines[0].ConsolOrJobType = TxnLineConsolOrJobType.SHP;
				invoice.TxnLines[0].ConsolOrJobNo = shipment2.JS_UniqueConsignRef;
				invoice.TxnLines[0].HouseBIllNo = "TESTHOUSE";
				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				OrgHeader creditor = ObjectCreator.ABIGAS;
				ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				Enterprise.Environment.Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = true;
				creditor.CompanyData.OB_IsCreditor = true;
				Factory.Save();

				UAInvoice newBizO = Factory.New<UAInvoice>();
				newBizO.AH_TransactionNum = "ABC1231111";
				ValueObjectImportContext context = new ValueObjectImportContext(newBizO.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(creditor, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(newBizO, invoice, context);

				Assert("IsCreatedByENett not setted: Invoice validation works: Invoice has been deleted due to validation errors.", newBizO.IsDeleted);

				//IsCreatedByENett setted
				invoice = GetFullyPopulatedXmlInvoice_AR();
				invoice.TxnLines[0].ConsolOrJobTypeSpecified = true;
				invoice.TxnLines[0].ConsolOrJobType = TxnLineConsolOrJobType.SHP;
				invoice.TxnLines[0].ConsolOrJobNo = shipment2.JS_UniqueConsignRef;
				invoice.TxnLines[0].HouseBIllNo = "TESTHOUSE";
				notify = new NotificationBuffer();
				Factory.Save();

				newBizO = Factory.New<UAInvoice>();
				newBizO.AH_TransactionNum = "ABC123";
				newBizO.IsCreatedByENett = true;
				context = new ValueObjectImportContext(newBizO.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(creditor, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(newBizO, invoice, context);

				Assert("IsCreatedByENett setted: Invoice validation don't works: Invoice has not been deleted due to validation errors.", !newBizO.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.UnapprovedPayableTransactions, newBizO.AH_Ledger);
				AssertEquals("Client", creditor.PK, newBizO.AH_OH);
				AssertEquals("Shipment 1", shipment1.PK, newBizO.Lines[0].Job.JH_ParentID);
			}
			else
			{
				Assert(true);
			}
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert(true);
		}

		#region Implementation

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		protected override InvoicingBase NewBusinessObject()
		{
			return Factory.New<ARInvoice>();
		}

		protected override ValueObjectDataAdapter<InvoicingBase, TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new eNettInboundInvoiceDataAdapter();
		}

		#endregion
	}
}
