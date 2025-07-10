using System;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	public class eNettTransactionLineBuilderTest : TransactionLineBuilderTest
	{
		protected override OrgHeader GetOrgForChargeCodeMappingTest()
		{
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", AuthenticationCode = "EJPx7yyuHu", OrganisationPK = ObjectCreator.AALSHI.PK });
			return ObjectCreator.AALSHI;
		}

		protected override void SetXMLInvoiceLineChargeCode(TxnLine txnLine, ZString chargeCodeThatDoesExist)
		{
			base.SetXMLInvoiceLineChargeCode(txnLine, chargeCodeThatDoesExist);
			if (PopulateEnettValue)
			{
				txnLine.eNettChargeCodeMapping = chargeCodeThatDoesExist;
			}
		}

		protected override void GetNewLineBuilder()
		{
			Builder = new eNettTransactionLineBuilder(new NotificationManager(Notify), new TransactionBuilderConfig());
		}

		bool PopulateEnettValue = true;

		public void TestFallBackToChargeCodeFieldWhenEnettChargeCodeFieldIsEmpty()
		{
			PopulateEnettValue = false;
			AssertChargeCodeMappingForOrganisation();
		}
	}
}
