using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	[DependentBusinessObject(typeof(AccTaxReturn), "Columns")]
	public class AccTaxReturnColumnCollection : DependentBusinessObjectCollection<AccTaxReturnColumn, AccTaxReturn>
	{
		public AccTaxReturnColumnCollection(AccTaxReturn master) : base(master)
		{
			master.IsTopLevel = true;
		}

		protected override string FkColumnName => AccTaxReturnColumnSchema.ATC_ATR_AccTaxReturn.Name;
	}
}
