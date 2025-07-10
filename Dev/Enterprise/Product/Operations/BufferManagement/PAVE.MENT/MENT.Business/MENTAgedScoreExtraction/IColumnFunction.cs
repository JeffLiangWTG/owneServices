using CargoWise.Types;

namespace Enterprise.PAVE.MENT.Business
{
	interface IColumnFunction
	{
		ZString GetFunctionAsStringWithFormat();

		ZString FunctionType { get; set; }

		ZDecimal Parameter1 { get; set; }
	}
}
