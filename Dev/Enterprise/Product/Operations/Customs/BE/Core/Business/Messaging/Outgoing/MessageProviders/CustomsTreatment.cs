using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CustomsTreatment : ITCustomsTreatmentImport
{
	public CustomsTreatment(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine, CusEntryLine.Schema.TableName);
		this.entryLine = entryLine;
		Preference = new Preference(entryLine);
		Procedure = new ProcedureImport(entryLine);
	}

	readonly CusEntryLine entryLine;

	public ITCalculationUnits[] CalculationUnits { get => CalculationUnit.ExtractCalculationUnits(entryLine).ToArray(); }
	public ITChargesImport ChargesImport { get; }
	public ITPreference Preference { get; }
	public ITProcedureImport Procedure { get; }
	public ZString Quota { get => entryLine.RandomLine.JI_ConcessionOrder; }
	public ITTaxCalculation[] TaxCalculation { get => Array.Empty<ITTaxCalculation>(); }
	public ZString ValuationMethod { get => entryLine.RandomLine.JI_ValuationCode; }
	public ITWarehouse Warehouse { get; }
}
