using Enterprise.MasterFiles.Business;
#if DEBUG

namespace Enterprise.Accounting.Business.GenericJob
{
	partial class GenericJob
	{
		public JobHeader Job_ForTestOnly => Job;
	}
}

#endif
