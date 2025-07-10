//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmReportRunLookups
//
//    This class should be used for overriding collections in AutoStmReportRunLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.Business
{
	public class StmReportRunLookups : AutoStmReportRunLookups
	{
		public StmReportRunLookups(AutoStmReportRun parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StmReportRunStatus
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.ReportStatisticsStatus);
			}
		}

		public GlbStaffCollection AllStaff
		{
			get { return new GlbStaffCollection(Factory); }
		}
	}
}
