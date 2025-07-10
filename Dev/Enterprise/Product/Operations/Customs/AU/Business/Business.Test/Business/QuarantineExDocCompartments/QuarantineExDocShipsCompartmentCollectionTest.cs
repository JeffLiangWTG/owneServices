using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocShipsCompartmentCollection))]
	sealed class QuarantineExDocShipsCompartmentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestQuarantineExDocShipsCompartmentCollectionMaxCount()
		{
			QuarantineExDocShipsCompartmentCollection collection = (QuarantineExDocShipsCompartmentCollection)GetCollectionToTest();
			AssertEquals("Max number of rows value is correct", 4, collection.MaxCount);
		}

		public void TestFindByCompartmentNumber()
		{
			QuarantineExDocShipsCompartmentCollection collection = (QuarantineExDocShipsCompartmentCollection)GetCollectionToTest();
			QuarantineExDocShipsCompartment compartment = collection.AddNew();
			compartment.QC_Compartments = "12345";
			AssertNull("Find Invalid comaprtment number", collection.FindByCompartmentNumber("34534"));
			AssertEquals("Find valid comaprtment number", compartment, collection.FindByCompartmentNumber("12345"));
		}

		public void TestClone()
		{
			QuarantineExDocShipsCompartmentCollection collection = (QuarantineExDocShipsCompartmentCollection)GetCollectionToTest();
			QuarantineExDocShipsCompartment exdocCompartment = collection.AddNew();
			exdocCompartment.QC_Compartments = "~~~";
			exdocCompartment.QC_InspectionDate = new ZDateTime(2006, 12, 12);
			exdocCompartment.QC_RL_NKInspectionPort = "AUSYD";

			QuarantineExDocHeader clonedHeader = Factory.New<QuarantineExDocHeader>();
			QuarantineExDocShipsCompartmentCollection result = new QuarantineExDocShipsCompartmentCollection(clonedHeader);
			result.Clone(collection);

			QuarantineExDocShipsCompartment clonedExdocCompartment = result[0];
			AssertEquals("Parent ID set", clonedHeader.PK, clonedExdocCompartment.QC_QH);
			AssertEquals("Cloned Compartments", "~~~", clonedExdocCompartment.QC_Compartments);
			AssertEquals("Cloned Inspection Date", new ZDateTime(2006, 12, 12), clonedExdocCompartment.QC_InspectionDate);
			AssertEquals("Cloned Inspection Port", "AUSYD", clonedExdocCompartment.QC_RL_NKInspectionPort);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<QuarantineExDocHeader>();
			return new QuarantineExDocShipsCompartmentCollection(header);
		}
	}
}
