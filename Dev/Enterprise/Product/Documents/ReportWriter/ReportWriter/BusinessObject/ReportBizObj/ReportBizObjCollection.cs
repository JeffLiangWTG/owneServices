using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ReportWriter
{
	public class ReportBizObjCollection : NonPersistentBusinessObjectCollection<ReportBizObj>
	{
		public ReportBizObjCollection(MainBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly MainBizObj parent;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public ReportBizObj this[ZString sheetName]
		{
			get
			{
				return this.Cast<ReportBizObj>().FirstOrDefault(x => x.WorkSheetName == sheetName);
			}
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReportBizObj(Factory);
		}

		#endregion
	}
}
