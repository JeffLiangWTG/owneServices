using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEUEntryHeaderLookups : EU.Business.Declaration.CusEUEntryHeaderLookups
	{
		public CusEUEntryHeaderLookups(CusEUEntryHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList EADPrintProcedureCodeList => Factory.GetCachedValue<EADPrintProcedureCodeList>();
	}
}
