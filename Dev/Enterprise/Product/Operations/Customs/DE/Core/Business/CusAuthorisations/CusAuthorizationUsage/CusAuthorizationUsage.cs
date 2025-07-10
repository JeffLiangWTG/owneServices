using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage
	{
		public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new Declaration.CusEntryInstruction Instruction => (Declaration.CusEntryInstruction)base.Instruction;

		public new CusAuthorizationUsageLookups Lookups => (CusAuthorizationUsageLookups)base.Lookups;

		public new CusAuthorizationUsageValidation Validation => (CusAuthorizationUsageValidation)base.Validation;

		protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);

		protected override EU.Business.CusAuthorizationUsageValidation GetNewValidation() => new CusAuthorizationUsageValidation(this);
	}
}
