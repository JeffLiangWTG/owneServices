using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CusReferenceCollection<CusReference>))]
	sealed class CusReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestEmptyType()
		{
			AssertExceptionThrown<ArgumentException>(() => new CusReferenceCollection<CusReference>(null, ZString.Empty));
		}

		public override void TestAddNew()
		{
			base.TestAddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var collection = new CusReferenceCollection<CusReference>(entryInstruction, "XXX");
			var reference = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CFR_Type", "XXX", reference.CFR_Type);
				AssertEquals("CFR_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, reference.CFR_ParentTableCode);
				AssertEquals("CFR_ParentID", entryInstruction.PK, reference.CFR_ParentID);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new CusReferenceCollection<CusReference>(instruction, "XXX");
		}
	}
}
