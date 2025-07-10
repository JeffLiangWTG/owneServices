using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLinePivot : AutoCusTempStorageLinePivot
		, Integration.Customs.EU.ICusTempStorageLinePivot
	{
		public CusTempStorageLinePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region SLR_TSL_FromLine

		[RelatedBusinessObject("FromLine")]
		public override ZGuid SLR_TSL_FromLine
		{
			get { return base.SLR_TSL_FromLine; }
			set { base.SLR_TSL_FromLine = value; }
		}

		public virtual CusTempStorageLine FromLine
		{
			get { return Factory.Load<CusTempStorageLine>(SLR_TSL_FromLine); }
		}

		#endregion

		#region SLR_TSL_ToLine

		[RelatedBusinessObject("ToLine")]
		public override ZGuid SLR_TSL_ToLine
		{
			get { return base.SLR_TSL_ToLine; }
			set { base.SLR_TSL_ToLine = value; }
		}

		public virtual CusTempStorageLine ToLine
		{
			get { return Factory.Load<CusTempStorageLine>(SLR_TSL_ToLine); }
		}

		#endregion

		public override void Delete()
		{
			var toLine = ToLine;
			if (!IsDeleted
				&& toLine != null
				&& !toLine.IsDeleted
				&& toLine.ShouldDeleteReleatedToLinesWhenDeleting)
			{
				toLine.Delete();
			}

			base.Delete();
		}
	}
}
