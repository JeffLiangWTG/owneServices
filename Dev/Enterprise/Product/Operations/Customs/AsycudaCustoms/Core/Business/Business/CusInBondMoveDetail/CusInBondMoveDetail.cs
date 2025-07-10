using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveDetail : BaseCusInBondMoveDetail, ICusInBondContainerTypeSupporter
	{
		public CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(false)]
		public CusInBondContainerCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = GetContainersCollection();
					RegisterEditableChildObject(containers);
				}
				return containers;
			}
		}
		CusInBondContainerCollection containers;

		protected virtual CusInBondContainerCollection GetContainersCollection()
		{
			return new CusInBondContainerCollection(this);
		}

		protected override Customs.Business.CusInBondMoveDetailValidation GetNewValidation()
		{
			return new CusInBondMoveDetailValidation(this);
		}

		public new CusInBondMoveHeader MoveHeader => (CusInBondMoveHeader)base.MoveHeader;

		protected override BaseCusInBondMoveHeader GetMoveHeader() => Factory.Load<CusInBondMoveHeader>(B9_BM);

		public override void Delete()
		{
			if (containers != null)
			{
				containers.DeleteAll();
			}
			base.Delete();
		}

		#region ICusInBondContainerTypeSupporter

		Type ICusInBondContainerTypeSupporter.ContainerType
		{
			get
			{
				return typeof(CusInBondContainer);
			}
		}

		#endregion
	}
}
