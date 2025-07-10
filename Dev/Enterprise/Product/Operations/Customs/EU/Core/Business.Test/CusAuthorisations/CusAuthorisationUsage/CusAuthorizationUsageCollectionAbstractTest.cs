using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	public abstract class CusAuthorizationUsageCollectionAbstractTest<TCollection, TCusAuthorizationUsage, TCollectionItem> : BusinessObjectCollectionTestCase
		where TCollection : ICusAuthorizationUsageCollection<TCusAuthorizationUsage, TCollectionItem>
		where TCollectionItem : BusinessObject, ICusAuthorizationUsageMaster, ILinkable
		where TCusAuthorizationUsage : CusAuthorizationUsage
	{
		[ExpectNoExceptions]
		public void TestFKSchemaColumnInDependent()
		{
			var item = Factory.New<TCollectionItem>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(item.FKSchemaColumnInDependent, NUnit.Framework.Is.Not.EqualTo(default(SchemaGuidColumn)), "Foreign Key Schema Column Independent Item - should not be [null]");
				NUnit.Framework.Assert.That(authorizationUsageCollection.Master.FKSchemaColumnInDependent, NUnit.Framework.Is.Not.EqualTo(default(SchemaGuidColumn)), "Foreign Key Schema Column Independent Master - should not be [null]");
				NUnit.Framework.Assert.That(authorizationUsageCollection.Master.FKSchemaColumnInDependent, NUnit.Framework.Is.EqualTo(item.FKSchemaColumnInDependent), "Foreign Key Schema Column Independent");
			});
		}

		[ExpectNoExceptions]
		public void TestFKSchemaColumnInDependentExpected()
		{
			NUnit.Framework.Assert.That(authorizationUsageCollection.Master.FKSchemaColumnInDependent, NUnit.Framework.Is.EqualTo(ExpectedFKSchemaColumnInDependent));
		}

		[ExpectNoExceptions]
		public void TestFKItemObject()
		{
			authorizationUsageCollection.AddNew();
			var fKSchemaColumnInDependent = authorizationUsageCollection.Master.FKSchemaColumnInDependent.Name;
			object itemObject = null;
			switch (fKSchemaColumnInDependent)
			{
				case "AGC_ParentID":
					if (authorizationUsageCollection[0].AGC_ParentTableCode.Equals(CusEntryInstructionSchema.Constants.Prefix))
					{
						itemObject = authorizationUsageCollection[0].Instruction;
					}
					if (authorizationUsageCollection[0].AGC_ParentTableCode.Equals(JobComInvoiceLineSchema.Constants.Prefix))
					{
						itemObject = authorizationUsageCollection[0].InvoiceLine;
					}
					break;
				case "AGC_OH_Owner":
					itemObject = authorizationUsageCollection[0].Owner;
					break;
			}
			NUnit.Framework.Assert.That(itemObject, NUnit.Framework.Is.Not.EqualTo(default(object)), "Object for foreign key expected with:  + fKSchemaColumnInDependent - should not be [null]");
		}

		[ExpectNoExceptions]
		public void TestCusAuthorizationUsageCollectionMaxCount()
		{
			NUnit.Framework.Assert.That(authorizationUsageCollection.MaxCount, NUnit.Framework.Is.EqualTo(MaxCount), "Max number of rows value is correct");
		}

		[ExpectNoExceptions]
		public void TestAddNewReturnedType()
		{
			NUnit.Framework.Assert.That(authorizationUsageCollection.AddNew(), NUnit.Framework.Is.TypeOf<TCusAuthorizationUsage>());
		}

		[ExpectNoExceptions]
		public void TestIndexerReturnedType()
		{
			authorizationUsageCollection.AddNew();
			NUnit.Framework.Assert.That(authorizationUsageCollection[0], NUnit.Framework.Is.TypeOf<TCusAuthorizationUsage>());
		}

		[ExpectNoExceptions]
		public void TestGetEnumerator()
		{
			NUnit.Framework.Assert.That(authorizationUsageCollection.GetEnumerator(), NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerator<TCusAuthorizationUsage>)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorizationUsageCollection = (TCollection)(ICusAuthorizationUsageCollection<TCusAuthorizationUsage, TCollectionItem>)GetCollectionToTest();
		}

		TCollection authorizationUsageCollection;

		protected override Type GetExpectedCollectionType() => typeof(TCollection);

		protected virtual SchemaGuidColumn ExpectedFKSchemaColumnInDependent => null;

		protected virtual int MaxCount => 9;
	}
}
