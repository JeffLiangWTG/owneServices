using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(CusEUEntryHeader))]
	class CusEUEntryHeaderTest : EU.Business.Declaration.Testing.CusEUEntryHeaderAbstractTest<CusEUEntryHeader>
	{
		public void TestICusEUEntryHeaderIsCorrectlySetup()
		{
			var bizObj = (BusinessObject)Factory.New<Integration.Customs.ES.ICusEUEntryHeader>();
			AssertType<CusEUEntryHeader>(bizObj);
			AssertType<CusEUEntryHeader>(Factory.Load(bizObj.TablePrefix, bizObj.PK));
		}

		public void TestTypeSafe()
		{
			var entry = Factory.New<CusEntryHeader>();

			CombineAssertions("ES.CusEntryHeader AddInfo Types", () =>
			{
				AssertType<CusEUEntryHeader>("ES CusEUEntryHeader", entry.AddInfoChild);
				AssertType<CusEUEntryHeaderLookups>("ES CusEUEntryHeaderLookups", entry.AddInfoChildLookups);
				AssertType<EU.Business.Declaration.CusEUEntryHeaderValidation>("EU CusEUEntryHeaderValidation (currently no ES-specific)", entry.AddInfoChildValidation);
			});
		}

		public void TestEADPrintProcedureListAttribute()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEUEntryHeader), nameof(CusEUEntryHeader.EUH_EADPrintProcedure), false, a => a.ListDataSourceMember == "Lookups.EADPrintProcedureCodeList");
		}

		public void TestFormattedEADPrint()
		{
			var entry = Factory.New<CusEUEntryHeader>();

			CombineAssertions(() =>
			{
				AssertFormattedEADPrint(entry, "0", "[0] No EAD Print");
				AssertFormattedEADPrint(entry, "1", "[1] EAD printed by Customs authorities or through the Virtual Office");
				AssertFormattedEADPrint(entry, "2", "[2] EAD can be printed by declarant or through the Virtual Office");
			});
		}

		void AssertFormattedEADPrint(CusEUEntryHeader entry, string printProcedure, string expectedFormattedValue)
		{
			entry.EUH_EADPrintProcedure = printProcedure;
			AssertEquals($"FormattedEADPrint when code = {printProcedure}", expectedFormattedValue, entry.FormattedEADPrint);
		}
	}
}
