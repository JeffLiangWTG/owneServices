using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiPersonMergeQueue : AutoEdiPersonMergeQueue
	{
		public EdiPersonMergeQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
