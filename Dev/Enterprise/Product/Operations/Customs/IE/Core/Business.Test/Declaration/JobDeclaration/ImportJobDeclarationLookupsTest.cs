using System;
using System.Reflection;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportJobDeclarationLookupsTest : JobDeclarationLookupsTest<ImportJobDeclarationLookups>
	{
		public void TestCustomsOfficeDataGrouping()
		{
			var property = lookups.GetType().GetProperty("CustomsOfficeDataGrouping", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertEquals("CustomsOfficeDataGrouping", "IE", property.GetValue(lookups).ToString());
		}

		public void TestMessageSubTypeList()
		{
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "EU, CO, IM", lookups.MessageSubTypeList.CodesAsString);
				AssertSame("Cached", lookups.MessageTypeList, lookups.MessageTypeList);
			});
		}

		public void TestIncoTermList()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var invoice = declaration.Invoices.AddNew();
				AssertContains(Core.Constants.IncoTerms.Other, invoice.Lookups.JZ_IncoTerm_List.CodesAsString);
			}

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invoice = declaration.Invoices.AddNew();
				AssertContains(Core.Constants.IncoTerms.Other, invoice.Lookups.JZ_IncoTerm_List.CodesAsString);
			}
		}

		public void TestJE_ApplicationCodeList_BLT()
		{
			var customsInterface = new LocalCountryCustomsInterface { SubmissionType = DeclarationApplicationCodeList.Codes.Builtin };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
				var newDeclaration = Factory.New<JobDeclaration>();
				newDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
				IECustomsDataRegistry.Instance.IsUCC6EnabledForImport.SetValue(jobDeclaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
				CombineAssertions(() =>
				{
					var applicationCodeList = lookups.ApplicationCodeList;
					AssertEquals("Codes from list when disabled", "V1", applicationCodeList.CodesAsString);
					AssertSame("Cached", applicationCodeList, newDeclaration.Lookups.ApplicationCodeList);
				});

				IECustomsDataRegistry.Instance.IsUCC6EnabledForImport.SetValue(jobDeclaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
				CombineAssertions(() =>
				{
					var applicationCodeList = lookups.ApplicationCodeList;
					AssertEquals("Codes from list when enabled", "V1, V2", applicationCodeList.CodesAsString);
					AssertSame("Cached", applicationCodeList, newDeclaration.Lookups.ApplicationCodeList);
				});
			}
		}

		public void TestJE_ApplicationCodeList()
		{
			IECustomsDataRegistry.Instance.IsUCC6EnabledForImport.SetValue(jobDeclaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			var newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				var applicationCodeList = lookups.ApplicationCodeList;
				AssertEquals("Codes from list when disabled", "V1, ITF", applicationCodeList.CodesAsString);
				AssertSame("Cached", applicationCodeList, newDeclaration.Lookups.ApplicationCodeList);
			});

			IECustomsDataRegistry.Instance.IsUCC6EnabledForImport.SetValue(jobDeclaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CombineAssertions(() =>
			{
				var applicationCodeList = lookups.ApplicationCodeList;
				AssertEquals("Codes from list when enabled", "V1, V2, ITF", applicationCodeList.CodesAsString);
				AssertSame("Cached", applicationCodeList, newDeclaration.Lookups.ApplicationCodeList);
			});
		}

		public override void TestEntryStatusList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "ACC, AMR, AMA, SUP, CAR, CAN, CON, GEN, INS, INV, NOT, PRE, RAA, RAJ, RAR, REG, REJ, REL", lookups.EntryStatusList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<AISEntryStatusList>(), lookups.EntryStatusList);
			});
		}
	}
}
