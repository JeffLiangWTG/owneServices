using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class ProcedureImport : ITProcedureImport
{
	public ProcedureImport(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine, CusEntryLine.Schema.TableName);
		this.entryLine = entryLine;
	}

	readonly CusEntryLine entryLine;

	public ZString[] NationalProcedureCode { get => new[] { entryLine.RandomLine.JI_Calc_Concession }; }
	public ZString[] ProcedureNat { get => Array.Empty<ZString>(); }
	public ZString ProcedurePart1 { get => entryLine.RandomLine.JI_Procedure.Left(2); }
	public ZString ProcedurePart2 { get => entryLine.RandomLine.JI_Calc_PreviousProcedure; }
	public ZString ProcedureType
	{
		get
		{
			return entryLine.Header.EntryInstruction.CEI_Style;
		}
	}
}
