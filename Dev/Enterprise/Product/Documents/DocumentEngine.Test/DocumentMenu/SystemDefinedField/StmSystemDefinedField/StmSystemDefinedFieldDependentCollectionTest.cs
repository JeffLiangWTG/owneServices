using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldDependentCollection))]
	sealed class StmSystemDefinedFieldDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionMembers()
		{
			StmSystemDefinedField field1 = Factory.New<StmSystemDefinedField>();
			StmSystemDefinedField field2 = Factory.New<StmSystemDefinedField>();

			field1.S1_BusinessContext = "Test";
			field2.S1_BusinessContext = "Consol";

			StmSystemDefinedFieldCountry fieldCountry1 = field1.FieldCountries.AddNew();
			StmSystemDefinedFieldCountry fieldCountry2 = field2.FieldCountries.AddNew();

			fieldCountry1.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.Australia;
			fieldCountry2.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.Australia;

			StmSystemDefinedFieldColumn fieldColumn1 = field1.FieldColumns.AddNew();
			StmSystemDefinedFieldColumn fieldColumn2 = field2.FieldColumns.AddNew();

			fieldColumn1.S1_OrderColumn = 1;
			fieldColumn2.S1_OrderColumn = 2;

			Collection.Load();

			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("Contains(Field1)", true, Collection.Contains(field1));
		}

		public void TestDefaultsForNewChild()
		{
			var mockDocumentSupporter = new Mock<DocumentSupporter>(Factory.New<DummyBusinessObject>());
			MockDocSupportBizO documentSupportable = new MockDocSupportBizO(mockDocumentSupporter.Object);
			StmSystemDefinedFieldDependentCollection collection = new StmSystemDefinedFieldDependentCollection(documentSupportable, Factory);

			mockDocumentSupporter.Setup(m => m.BusinessContext).Returns(BusinessContext.Shipment);
			mockDocumentSupporter.Verify(m => m.BusinessContext, Times.AtMost(3));
			StmSystemDefinedField field1 = collection.AddNew();
			StmSystemDefinedField field2 = collection.AddNew();
			StmSystemDefinedField field3 = collection.AddNew();
			mockDocumentSupporter.VerifyAll();

			AssertEquals("Field1.S1_Order", (short)1, field1.S1_Order);
			AssertEquals("Field2.S1_Order", (short)2, field2.S1_Order);
			AssertEquals("Field3.S1_Order", (short)3, field3.S1_Order);

			AssertEquals("Field1.S1_BusinessContext", "Shipment", field1.S1_BusinessContext);
			AssertEquals("Field2.S1_BusinessContext", "Shipment", field2.S1_BusinessContext);
			AssertEquals("Field3.S1_BusinessContext", "Shipment", field3.S1_BusinessContext);

			collection.Remove(field2);

			mockDocumentSupporter.Setup(m => m.BusinessContext).Returns(BusinessContext.Consol);
			mockDocumentSupporter.Verify(m => m.BusinessContext, Times.AtMost(3));
			StmSystemDefinedField field4 = collection.AddNew();
			StmSystemDefinedField field5 = collection.AddNew();
			StmSystemDefinedField field6 = collection.AddNew();
			mockDocumentSupporter.VerifyAll();

			AssertEquals("Field4.S1_Order", (short)4, field4.S1_Order);
			AssertEquals("Field5.S1_Order", (short)5, field5.S1_Order);
			AssertEquals("Field6.S1_Order", (short)6, field6.S1_Order);

			AssertEquals("Field4.S1_BusinessContext", "Consol", field4.S1_BusinessContext);
			AssertEquals("Field5.S1_BusinessContext", "Consol", field5.S1_BusinessContext);
			AssertEquals("Field6.S1_BusinessContext", "Consol", field6.S1_BusinessContext);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmSystemDefinedFieldDependentCollection(new MockDocSupportBizO(), Factory);
		}

		new StmSystemDefinedFieldDependentCollection Collection
		{
			get { return (StmSystemDefinedFieldDependentCollection)base.Collection; }
		}

		#endregion
	}
}
