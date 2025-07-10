using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class AmendmentComplXExportLineWrapperTest : DUAExportLineWrapperTest
	{
		protected override void AssertGoodsCustomsProcedureCategory3()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = EntryLineData.Procedure;
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with only procedure's concession", EntryLineData.ProcedureConcessionPart, wrapper.GoodsCustomsProcedureCategory3);

				invoiceLine.JI_Procedure = EntryLineData.Procedure;
				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure1);
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with procedure's concession + first additional concession", EntryLineData.ProcedureConcessionPart + EntryLineData.Procedure1ConcessionPart, wrapper.GoodsCustomsProcedureCategory3);

				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure2);
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with procedure's concession + first additional concession + second additional concession", EntryLineData.ConcessionCodes, wrapper.GoodsCustomsProcedureCategory3);

				invoiceLine.JI_Procedure = EntryLineData.ProcedurePart1;
				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure1);
				invoiceLine.AdditionalProcedureCodes.AddNew(EntryLineData.AddProcedure2);
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with procedure < 4 => only first additional concession + second additional concession", EntryLineData.Procedure1ConcessionPart + EntryLineData.Procedure2ConcessionPart, wrapper.GoodsCustomsProcedureCategory3);

				invoiceLine.JI_Procedure = "1234005";
				invoiceLine.AdditionalProcedureCodes.AddNew("9VA");
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with only procedure's concession when additional procedure is 9VA", "005", wrapper.GoodsCustomsProcedureCategory3);

				invoiceLine.AdditionalProcedureCodes.AddNew("9PV");
				AssertEquals("Expected filled GoodsCustomsProcedureCategory3 with only procedure's concession when additional procedure is 9PV", "005", wrapper.GoodsCustomsProcedureCategory3);

				invoiceLine.JI_Procedure = "12349VA";
				AssertEquals("Expected empty GoodsCustomsProcedureCategory3 when procedure's concession is 9VA or 9PV", ZString.Empty, wrapper.GoodsCustomsProcedureCategory3);
			});
		}

		protected override DUAExportLineWrapper GetWrapper(CusEntryLine entryLine) => new AmendmentComplXExportLineWrapper(entryLine);
	}
}
