using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocDisbursementJobsCloseBatch : DocBaseWrapper, IDocDsbJobCloseBatchProvider
	{
		DocDisbursementJobsCloseBatch(DsbJobCloseBatch objectToWrapper, BusinessObjectFactory factoryToWrap)
			: base(objectToWrapper, factoryToWrap)
		{
		}

		public static DocDisbursementJobsCloseBatch New(DsbJobCloseBatch objectToWrapper, BusinessObjectFactory factoryToWrap)
		{
			return new DocDisbursementJobsCloseBatch(objectToWrapper, factoryToWrap);
		}

		IBODocDataProvider IDocDsbJobCloseBatchProvider.CreateDocDsbJobCloseBatch(DsbJobCloseBatch batch, BusinessObjectFactory factory)
		{
			return New(batch, factory);
		}

		public DocDisbursementJobsCloseBatchLineCollection BatchDetailsLines
		{
			get
			{
				if (batchDetailsLines == null)
				{
					batchDetailsLines = new DocDisbursementJobsCloseBatchLineCollection((BusinessObject)WrappedObject, Factory);
					batchDetailsLines.Sort("JobNumber");
				}

				return batchDetailsLines;
			}
		}
		DocDisbursementJobsCloseBatchLineCollection batchDetailsLines;

		public ZString BatchPrintedBy
		{
			get
			{
				return GlbStaff.CurrentUser.GS_FullName;
			}
		}

		public ZString CompanyName
		{
			get { return GlbCompany.CurrentCompany.CompanyName; }
		}

		public ZString CompanyCode
		{
			get { return GlbCompany.CurrentCompany.GC_Code; }
		}
	}
}
