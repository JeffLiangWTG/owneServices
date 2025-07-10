using System;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class StaffFact : IStaffFact
	{
		public StaffFact(GlbStaff staff, IDepartmentFact homeDeparmentFact)
		{
			Argument.NotNull(staff, nameof(staff));

			PK = staff.PK.ToGuid();
			Code = staff.GS_Code.ToString();
			HomeDepartment = new FactLeftJoin<IDepartmentFact>(homeDeparmentFact);
		}

		public Guid PK { get; }

		public string Code { get; }

		public FactLeftJoin<IDepartmentFact> HomeDepartment { get; }
	}
}
