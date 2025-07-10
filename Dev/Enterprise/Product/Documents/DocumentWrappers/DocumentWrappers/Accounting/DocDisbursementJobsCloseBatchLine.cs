using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocDisbursementJobsCloseBatchLine : DocBaseWrapper
	{
		DocDisbursementJobsCloseBatchLine(DisbursementJobsCloseBatchLine objectToWrapper, BusinessObjectFactory factoryToWrap)
			: base(objectToWrapper, factoryToWrap)
		{
		}

		public static DocDisbursementJobsCloseBatchLine New(DisbursementJobsCloseBatchLine objectToWrapper, BusinessObjectFactory factoryToWrap)
		{
			return new DocDisbursementJobsCloseBatchLine(objectToWrapper, factoryToWrap);
		}

		public ZString JobNumber
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).JobNumber;
			}
		}

		public ZDateTime JobOpenedDate
		{
			get
			{
				return new ZDateTime(((DisbursementJobsCloseBatchLine)WrappedObject).JobOpenedDate);
			}
		}

		public ZString LineBranch
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).LineBranch;
			}
		}

		public ZString LineDepartment
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).LineDepartment;
			}
		}

		public ZString ChargeCode
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).ChargeCode;
			}
		}

		public ZString LineGLAccount
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).LineGLAccount;
			}
		}

		public ZString DSBSurplusGLAccount
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).DSBSurplusGLAccount;
			}
		}

		public ZString DSBShortFallGLAccount
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).DSBShortFallGLAccount;
			}
		}

		public ZString LineType
		{
			get
			{
				return ((DisbursementJobsCloseBatchLine)WrappedObject).LineType;
			}
		}

		public ZDecimal LineLocalAmount
		{
			get
			{
				return new ZDecimal(((DisbursementJobsCloseBatchLine)WrappedObject).LineLocalAmount);
			}
		}
	}

	public class DisbursementJobsCloseBatchLine : NonPersistentBusinessObject
	{
		public DisbursementJobsCloseBatchLine(DynamicBusinessObject dynamicBusinessObject)
		{
			JobNumber = dynamicBusinessObject[nameof(JobNumber)].ToString();
			JobOpenedDate = new ZDateTime(dynamicBusinessObject[nameof(JobOpenedDate)]);
			LineBranch = dynamicBusinessObject[nameof(LineBranch)].ToString();
			LineDepartment = dynamicBusinessObject[nameof(LineDepartment)].ToString();
			ChargeCode = dynamicBusinessObject[nameof(ChargeCode)].ToString();
			LineGLAccount = dynamicBusinessObject[nameof(LineGLAccount)].ToString();
			DSBSurplusGLAccount = dynamicBusinessObject[nameof(DSBSurplusGLAccount)].ToString();
			DSBShortFallGLAccount = dynamicBusinessObject[nameof(DSBShortFallGLAccount)].ToString();
			LineType = dynamicBusinessObject[nameof(LineType)].ToString();
			LineLocalAmount = new ZDecimal(dynamicBusinessObject[nameof(LineLocalAmount)]);
		}

		public ZString JobNumber { get; set; }

		public ZDateTime JobOpenedDate { get; set; }

		public ZString LineBranch { get; set; }

		public ZString LineDepartment { get; set; }

		public ZString ChargeCode { get; set; }

		public ZString LineGLAccount { get; set; }

		public ZString DSBSurplusGLAccount { get; set; }

		public ZString DSBShortFallGLAccount { get; set; }

		public ZString LineType { get; set; }

		public ZDecimal LineLocalAmount { get; set; }
	}
}
