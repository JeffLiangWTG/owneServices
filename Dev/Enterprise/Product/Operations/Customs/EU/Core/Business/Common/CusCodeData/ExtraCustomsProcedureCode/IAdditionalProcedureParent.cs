using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public interface IAdditionalProcedureParent
	{
		BusinessObject BusinessObject { get; }
		CodeDescriptionPairList AdditionalProcedureCodeList { get; }
		ZString MainProcedure { get; }
		ZString MainProcedurePrefix { get; }
		AdditionalProcedureCodeCollection AdditionalProcedureCodes { get; }
		ZPropertyInfo AdditionalProcedureCodesAsStringInfo { get; }
		int MaxNumberOfAdditionalProcedureCode { get; }
	}
}
