using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	partial class CombinedDataObjectReaderTest
	{
		public void TestCusSupplyChainActorReferencesDataObjectReader_Read()
		{
			var existingDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(existingDeclaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, existingDeclaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(existingDeclaration));
				existingDeclaration.JE_MasterBill = "MB123";
				var existingInvoice = existingDeclaration.Invoices.AddNew();
				existingInvoice.JZ_InvoiceNumber = "INV123ABC";
				var existingInvoiceline = existingInvoice.InvoiceLines.AddNew();
				existingInvoiceline.JI_MatchingKey = "INVLINE123ABC";
				var cusSupplyChainActorReferences = existingInvoiceline.CusSupplyChainActorReferences;
				var existingActor1 = cusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				var existingActor2 = cusSupplyChainActorReferences.AddNew();
				existingActor2.CFR_Code = "MF";
				existingActor2.CFR_Reference = "REF657";
				existingActor2.CFR_OA_Owner = Org2.MainAddress.PK;

				var existingActor3 = cusSupplyChainActorReferences.AddNew();
				existingActor3.CFR_Code = "WH";
				existingActor3.CFR_Reference = "ZZZ999";
				existingActor3.CFR_OA_Owner = Org1.MainAddress.PK;

				var customsReferenceCollection = CreateSupplyChainActors();

				CombineAssertions(() =>
				{
					new CusSupplyChainActorReferencesDataObjectReader(new TestErrorLogger(), Factory).Read(cusSupplyChainActorReferences, customsReferenceCollection.GroupBy(x => x.Type.Code.Value).ToDictionary(x => x.Key, y => y.ToList()));
					AssertEquals("Supply chain actor count", 2, cusSupplyChainActorReferences.Count);
					var actor1 = cusSupplyChainActorReferences[0];
					var actor2 = cusSupplyChainActorReferences[1];
					if (existingActor1.CFR_Reference != "REF123")
					{
						actor1 = cusSupplyChainActorReferences[1];
						actor2 = cusSupplyChainActorReferences[0];
					}
					AssertSame(existingActor1, actor1);
					AssertSame(existingActor2, actor2);
					AssertEquals("existingActor3.IsDeleted - not matched based on CFR_Code", true, existingActor3.IsDeleted);

					AssertEquals("Actor 1 Role", "CS", actor1.CFR_Code);
					AssertEquals("Actor 1 Reference", "REF123", actor1.CFR_Reference);
					AssertEquals("Actor 1 Owner", Org1.MainAddress.PK, actor1.CFR_OA_Owner);

					AssertEquals("Actor 2 Role", "MF", actor2.CFR_Code);
					AssertEquals("Actor 2 Reference", "REF987", actor2.CFR_Reference);
					AssertEquals("Actor 2 Owner", Org2.MainAddress.PK, actor2.CFR_OA_Owner);
				});
			}
		}

		public void TestCusSupplyChainActorReferencesDataObjectReader_Read_CustomsReferenceCollectionWithEmptyData()
		{
			var existingDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(existingDeclaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, existingDeclaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(existingDeclaration));
				existingDeclaration.JE_MasterBill = "MB123";
				var existingInvoice = existingDeclaration.Invoices.AddNew();
				existingInvoice.JZ_InvoiceNumber = "INV123ABC";
				var existingInvoiceline = existingInvoice.InvoiceLines.AddNew();
				existingInvoiceline.JI_MatchingKey = "INVLINE123ABC";
				var cusSupplyChainActorReferences = existingInvoiceline.CusSupplyChainActorReferences;
				var existingActor1 = cusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				var dictionary = new Dictionary<ZString, List<CustomsReference>>()
				{
					{ (ZString)CusReferenceTypeList.Codes.SupplyChainActor, new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.SupplyChainActor } } }) }
				};

				CombineAssertions(() =>
				{
					new CusSupplyChainActorReferencesDataObjectReader(new TestErrorLogger(), Factory).Read(cusSupplyChainActorReferences, dictionary);
					AssertEquals("Supply chain actor count", 0, cusSupplyChainActorReferences.Count);
					AssertEquals("existingActor1.IsDeleted - not matched based on CFR_Code", true, existingActor1.IsDeleted);
				});
			}
		}

		public void TestCusSupplyChainActorReferencesDataObjectReader_Read_CustomsReferenceCollectionWithNoSupplyChainActor()
		{
			var existingDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(existingDeclaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, existingDeclaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(existingDeclaration));
				existingDeclaration.JE_MasterBill = "MB123";
				var existingInvoice = existingDeclaration.Invoices.AddNew();
				existingInvoice.JZ_InvoiceNumber = "INV123ABC";
				var existingInvoiceline = existingInvoice.InvoiceLines.AddNew();
				existingInvoiceline.JI_MatchingKey = "INVLINE123ABC";
				var cusSupplyChainActorReferences = existingInvoiceline.CusSupplyChainActorReferences;
				var existingActor1 = cusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				var dictionary = new Dictionary<ZString, List<CustomsReference>>()
				{
					{ (ZString)CusReferenceTypeList.Codes.FiscalReference, new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.FiscalReference }, Reference = "REF12" } }) }
				};

				CombineAssertions(() =>
				{
					new CusSupplyChainActorReferencesDataObjectReader(new TestErrorLogger(), Factory).Read(cusSupplyChainActorReferences, dictionary);
					AssertEquals("Supply chain actor count", 1, cusSupplyChainActorReferences.Count);
					AssertSame(existingActor1, cusSupplyChainActorReferences[0]);
					AssertEquals("existingActor1.IsDeleted - should not touch since xml doesn't contain data", false, existingActor1.IsDeleted);

					AssertEquals("Actor 1 Role", "CS", existingActor1.CFR_Code);
					AssertEquals("Actor 1 Reference", "REF123", existingActor1.CFR_Reference);
					AssertEquals("Actor 1 Owner", Org3.MainAddress.PK, existingActor1.CFR_OA_Owner);
				});
			}
		}
	}
}
