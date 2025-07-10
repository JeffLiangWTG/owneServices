using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocShipsCompartment : AutoQuarantineExDocShipsCompartment, IClusterKeyWorker
	{
		public QuarantineExDocShipsCompartment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public QuarantineExDocHeader QuarantineExDocHeader
		{
			get { return Factory.Load<QuarantineExDocHeader>(QC_QH); }
		}

		[RelatedBusinessObject(nameof(QuarantineExDocHeader))]
		public override ZGuid QC_QH
		{
			get { return base.QC_QH; }
			set { base.QC_QH = value; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region Amend Permisssion Matrix processing

		protected bool QC_Compartments_ReadOnly
		{
			get { return !QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.Compartments); }
		}

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)QC_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(QuarantineExDocHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)QC_QHInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
