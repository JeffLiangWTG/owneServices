using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryLineProductQualification))]
	class EntryLineProductQualificationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_LineNumber = 2;
			var pq1 = Factory.New<CIQProductQualification>();
			pq1.CSI_Code = "408";
			pq1.CSI_ReferenceNumber = "001";
			pq1.CSI_LineNo = 1;
			pq1.CSI_Quantity = 1;
			pq1.CSI_UnitOfQuantity = "010";
			var pq2 = Factory.New<CIQProductQualification>();
			pq2.CSI_Code = "408";
			pq2.CSI_ReferenceNumber = "001";
			pq2.CSI_LineNo = 1;
			pq2.CSI_Quantity = 2;
			pq2.CSI_UnitOfQuantity = "010";
			var entryLineProductQualification = new EntryLineProductQualification(entryLine, new[] { pq1, pq2 }, 1);
			AssertEquals((ZInt)1, entryLineProductQualification.Sequence);
			AssertEquals((ZInt)2, entryLineProductQualification.EntryLineNo);
			Assert(entryLineProductQualification.SupportsVIN);
			AssertEquals("408", entryLineProductQualification.DocumentType);
			AssertEquals("001", entryLineProductQualification.DocumentNumber);
			AssertEquals((ZShort)1, entryLineProductQualification.LineNumber);
			AssertEquals(3m, entryLineProductQualification.Quantity);
			AssertEquals("010", entryLineProductQualification.UnitOfQuantity);
			AssertEquals("408:001/1/3 010", entryLineProductQualification.ToString());
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryLineProductQualification(Factory.New<CusEntryLine>(), new[] { Factory.New<CIQProductQualification>() }, 1);
	}
}
