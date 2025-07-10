using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class AdditionalProcedureCodeLookups : CusCodeDataLookups
	{
		public AdditionalProcedureCodeLookups(AdditionalProcedureCode officeCode) : base(officeCode)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Parent.Parent?.AdditionalProcedureCodeList ?? new CodeDescriptionPairList();

		protected new AdditionalProcedureCode Parent => (AdditionalProcedureCode)base.Parent;
	}
}
