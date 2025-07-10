using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	[DependentBusinessObject(typeof(AccTaxReturn), "Lines")]
	public class AccTaxReturnLineCollection : DependentBusinessObjectCollection<AccTaxReturnLine, AccTaxReturn>
	{
		public AccTaxReturnLineCollection(AccTaxReturn master) : base(master)
		{
			master.IsTopLevel = true;
		}

		protected override string FkColumnName => AccTaxReturnLineSchema.ARL_ATR_AccTaxReturn.Name;
	}
}
