using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusSupplyChainActorReference))]
	class CusSupplyChainActorReferenceTest : CusSupplyChainActorReferenceAbstractTest<CusSupplyChainActorReference>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Supply Chain Actor Reference", cusSupplyChainActorReference.HumanReadableName);
		}

		public void TestProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			cusSupplyChainActorReference.CFR_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			cusSupplyChainActorReference.CFR_ParentID = invoiceLine.PK;
			CombineAssertions(() =>
			{
				var provider = cusSupplyChainActorReference.Provider;
				AssertNotNull("Isn't null", provider);

				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
				declaration.JE_GC = company.PK;
				AssertEquals("Provider is changed when DataGroupingCode changes", false, ReferenceEquals(provider, cusSupplyChainActorReference.Provider));
			});

			var tempHeader = Factory.New<TemporaryStorageHeader>();
			var bill = Factory.New<TemporaryStorageBill>();
			bill.ABL_AMA = tempHeader.PK;
			var supplyChainActor = bill.SupplyChainActors.AddNew();

			CombineAssertions(() =>
			{
				var provider1 = supplyChainActor.Provider;
				AssertNotNull("Isn't null", provider1);
				AssertType<CusTempSupplyChainActorReferenceProvider>(provider1);
			});
		}

		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var bill = header.Bills.AddNew();
			var billSupplyChainActor = bill.SupplyChainActors.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var packedItemSupplyChainActor = packedItem.SupplyChainActors.AddNew();

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = billSupplyChainActor.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					if (propertyInfo.Name == nameof(billSupplyChainActor.CFR_ParentID) || propertyInfo.Name == nameof(billSupplyChainActor.CFR_ParentTableCode))
					{
						continue;
					}
					Assert(propertyInfo.ReadOnly);
				}

				propertyInfos = packedItemSupplyChainActor.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					if (propertyInfo.Name == nameof(packedItemSupplyChainActor.CFR_ParentID) || propertyInfo.Name == nameof(packedItemSupplyChainActor.CFR_ParentTableCode))
					{
						continue;
					}
					Assert(propertyInfo.ReadOnly);
				}
			}
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusReferenceTypeList.Codes.SupplyChainActor, cusSupplyChainActorReference.CFR_Type);
		}

		public void TestLookups()
		{
			AssertType<CusSupplyChainActorReferenceLookups>(cusSupplyChainActorReference.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusSupplyChainActorReferenceValidation>(cusSupplyChainActorReference.Validation);
		}

		public void TestCFR_Code_Caption()
		{
			AssertEquals("Role", DataBoundResourceStrings.GetDataForProperty(cusSupplyChainActorReference.CFR_CodeInfo).Caption);
		}

		public void TestCFR_Code_Caption_ImportUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusSupplyChainActorReference.CFR_CodeInfo, declaration.MultipleKeysToUse, "Role", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[13 14 031 000] Additional Supply Chain Actor < Role");
			}
		}

		public void TestCFR_Reference_Caption_ImportUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusSupplyChainActorReference.CFR_ReferenceInfo, declaration.MultipleKeysToUse, "Identification Number", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[13 14 017 000] Additional Supply Chain Actor < Identification Number");
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<CusSupplyChainActorReference>();

		protected override void SetUp()
		{
			base.SetUp();
			cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
		}
		CusSupplyChainActorReference cusSupplyChainActorReference;
	}

	public abstract class CusSupplyChainActorReferenceAbstractTest<T> : CusReferenceAbstractTest<T> where T : CusSupplyChainActorReference
	{
		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			if (FillWithValidData(declaration.CustomsEntryInstructions.AddNew().CusSupplyChainActorReferences.AddNew()) is T supplyChainActor1)
			{
				yield return supplyChainActor1;
			}
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			if (FillWithValidData(invoiceLine.CusSupplyChainActorReferences.AddNew()) is T supplyChainActor2)
			{
				yield return supplyChainActor2;
			}
			var tempHeader = factory.New<TemporaryStorageHeader>();
			tempHeader.AMA_Calc_HasHouseConsignment = true;
			var tempBill = tempHeader.Bills.AddNew();
			if (FillWithValidData(tempBill.SupplyChainActors.AddNew()) is T supplyChainActor3)
			{
				yield return supplyChainActor3;
			}
			var packedItem = tempBill.PackedItems.AddNew();
			if (FillWithValidData(packedItem.SupplyChainActors.AddNew()) is T supplyChainActor4)
			{
				yield return supplyChainActor4;
			}
		}

		protected CusSupplyChainActorReference FillWithValidData(CusSupplyChainActorReference reference)
		{
			reference.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			reference.CFR_Reference = "111";
			return reference;
		}
	}
}
