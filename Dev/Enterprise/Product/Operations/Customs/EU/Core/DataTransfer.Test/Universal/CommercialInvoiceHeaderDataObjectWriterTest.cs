using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class CommercialInvoiceHeaderDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestExportSupplyChainActors()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, declaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(declaration));
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var actor1 = invoiceLine.CusSupplyChainActorReferences.AddNew();
				actor1.CFR_Code = SupplyChainActorRoleList.Codes.CS;
				actor1.CFR_Reference = "REF123";
				var address1 = Factory.NewWithValidTestData<OrgAddress>();
				address1.CompanyName = "Test Company 1";
				address1.Address1 = "123 Test Street";
				address1.Address2 = "Town";
				address1.Postcode = "A12B3C4";
				actor1.CFR_OA_Owner = address1.PK;
				actor1.CFR_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				var actor2 = invoiceLine.CusSupplyChainActorReferences.AddNew();
				actor2.CFR_Code = SupplyChainActorRoleList.Codes.MF;
				actor2.CFR_Reference = "REF456";
				var address2 = Factory.NewWithValidTestData<OrgAddress>();
				address2.CompanyName = "Test Company 2";
				address2.Address1 = "456 Test Street";
				address2.Address2 = "City";
				address2.Postcode = "X98Y7Z6";
				actor2.CFR_OA_Owner = address2.PK;
				actor2.CFR_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var dataObject = GetDataObject(invoice);
				AssertNotNull(dataObject.CommercialInvoiceLineCollection);
				AssertEquals("Invoice Line Count", 1, dataObject.CommercialInvoiceLineCollection.Count);
				var commercialInvoiceLine = dataObject.CommercialInvoiceLineCollection[0];
				AssertNotNull("CommercialInvoiceLine Customs Reference Collection", commercialInvoiceLine.CustomsReferenceCollection);
				var actorDataObjects = commercialInvoiceLine.CustomsReferenceCollection.Where(c => c.Type.Code.Value == Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor).ToArray();
				AssertEquals("CommercialInvoiceLine Supply Chain Actor count", 2, actorDataObjects.Length);

				AssertSupplyChainActor("Supply Chain Actor 1", actorDataObjects[0], new CodeDescriptionPair { Code = SupplyChainActorRoleList.Codes.MF, Description = SupplyChainActorRoleList.Descriptions.MF }, "REF456", address2);
				AssertSupplyChainActor("Supply Chain Actor 2", actorDataObjects[1], new CodeDescriptionPair { Code = SupplyChainActorRoleList.Codes.CS, Description = SupplyChainActorRoleList.Descriptions.CS }, "REF123", address1);
			}
		}

		public void TestExportSupplyChainActors_Empty()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, declaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(declaration));
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var dataObject = GetDataObject(invoice);
				AssertNotNull(dataObject.CommercialInvoiceLineCollection);
				AssertEquals("Invoice Line Count", 1, dataObject.CommercialInvoiceLineCollection.Count);
				var commercialInvoiceLine = dataObject.CommercialInvoiceLineCollection[0];
				AssertNotNull("CommercialInvoiceLine Customs Reference Collection", commercialInvoiceLine.CustomsReferenceCollection);
				var actorDataObjects = commercialInvoiceLine.CustomsReferenceCollection.Where(c => c.Type.Code.Value == Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor).ToArray();
				AssertEquals("CommercialInvoiceLine Supply Chain Actor count", 1, actorDataObjects.Length);
				AssertSupplyChainActor("Supply Chain Actor - Empty", actorDataObjects[0], null, null, null);
			}
		}

		public void TestDoNotExportSupplyChainActorsWhenNotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				AssertEquals("OrganizationsSupport", false, declaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(declaration));
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var actor1 = invoiceLine.CusSupplyChainActorReferences.AddNew();
				actor1.CFR_Code = SupplyChainActorRoleList.Codes.CS;
				actor1.CFR_Reference = "REF123";
				var address1 = Factory.NewWithValidTestData<OrgAddress>();
				address1.CompanyName = "Test Company 1";
				address1.Address1 = "123 Test Street";
				address1.Address2 = "Town";
				address1.Postcode = "A12B3C4";
				actor1.CFR_OA_Owner = address1.PK;
				var dataObject = GetDataObject(invoice);
				AssertNotNull(dataObject.CommercialInvoiceLineCollection);
				AssertEquals("Invoice Line Count", 1, dataObject.CommercialInvoiceLineCollection.Count);
				var commercialInvoiceLine = dataObject.CommercialInvoiceLineCollection[0];
				AssertNull("CommercialInvoiceLine Customs Reference Collection", commercialInvoiceLine.CustomsReferenceCollection);
			}
		}

		public static void AssertSupplyChainActor(string message, CustomsReference customsReference, ICodeDescriptionDataObject role = null, string reference = null, OrgAddress owner = null)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Type.Code", Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, customsReference.Type.Code);
				AssertEquals("Type.Description", Customs.Business.CusReferenceTypeList.Descriptions.SupplyChainActor, customsReference.Type.Description);
				if (role == null)
				{
					AssertNull("SubType", customsReference.SubType);
				}
				else
				{
					AssertEquals("SubType.Code", role.Code, customsReference.SubType.Code);
					AssertEquals("SubType.Description", role.Description, customsReference.SubType.Description);
				}
				AssertEquals("Reference", reference, customsReference.Reference);
				if (owner == null)
				{
					AssertNull("Owner", customsReference.Owner);
				}
				else
				{
					AssertEquals("Owner.CompanyName", owner.CompanyName, customsReference.Owner.CompanyName);
				}
				AssertNull("ReferencedEntityDescription", customsReference.ReferencedEntityDescription);
				AssertNull("IsOverridden", customsReference.IsOverridden);
				AssertNull("Order", customsReference.Order);
				AssertNull("DateCollection", customsReference.DateCollection);
			});
		}

		CommercialInvoiceHeader GetDataObject(JobComInvoiceHeader invoice)
		{
			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, invoice)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.Ireland));
			return writer.GetDataObject(invoice);
		}
	}
}
