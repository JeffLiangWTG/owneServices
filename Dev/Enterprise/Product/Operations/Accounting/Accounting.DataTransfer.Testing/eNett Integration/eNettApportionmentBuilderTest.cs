using System;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Testing.eNett_Integration
{
	public class eNettApportionmentBuilderTest : ApportionmentBuilderTest
	{
		protected override OrgHeader GetOrgForChargeCodeMappingTest()
		{
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", AuthenticationCode = "EJPx7yyuHu", OrganisationPK = ObjectCreator.AALSHI.PK });
			return ObjectCreator.ABIGAS;
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
			Builder = new eNettApportionmentBuilder(new NotificationManager(Notify), new TransactionBuilderConfig());
		}

		bool PopulateEnettValue = true;

		public void TestFallBackToChargeCodeFieldWhenEnettChargeCodeFieldIsEmpty()
		{
			PopulateEnettValue = false;
			AssertChargeCodeMappingForOrganisation();
		}
	}
}
