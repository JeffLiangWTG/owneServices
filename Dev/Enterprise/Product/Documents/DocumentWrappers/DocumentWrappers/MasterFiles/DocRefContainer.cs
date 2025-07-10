using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocRefContainer : DocBaseWrapper
	{
		DocRefContainer(RefContainer refContainer, BusinessObjectFactory factoryToWrap)
			: base(refContainer, factoryToWrap)
		{
		}

		public static DocRefContainer New(RefContainer refContainer, BusinessObjectFactory factoryToWrap)
		{
			if (refContainer == null)
			{
				return null;
			}
			else
			{
				return new DocRefContainer(refContainer, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return Code;
		}

		RefContainer RefContainer
		{
			get { return (RefContainer)WrappedObject; }
		}
		public ZString Code
		{
			get { return RefContainer.RC_Code; }
		}
		public ZString Description
		{
			get { return RefContainer.RC_DescriptionMultilingual; }
		}

		public ZString Size
		{
			get { return Code.Length > 2 ? new ZString(Code.SubstringSafe(0, 2)) : Code; }
		}

		public ZString Type
		{
			get { return Code.Length > 2 ? Code.SubstringSafe(2) : ZString.Empty; }
		}

		public ZBool HasTynes
		{
			get { return RefContainer.RC_HasTynes; }
		}
		public ZDecimal Height
		{
			get { return RefContainer.RC_Height; }
		}
		public ZBool IsActive
		{
			get { return RefContainer.RC_IsActive; }
		}
		public ZBool IsBolster
		{
			get { return RefContainer.RC_ContainerType == Constants.ContainerTypes.Bolster; }
		}
		public ZBool IsDryStorage
		{
			get { return RefContainer.RC_ContainerType == Constants.ContainerTypes.DryStorage; }
		}
		public ZBool IsFlatRack
		{
			get { return RefContainer.RC_ContainerType == Constants.ContainerTypes.FlatRack; }
		}
		public ZBool IsIso
		{
			get { return RefContainer.RC_IsIso; }
		}
		public ZBool IsOpenTop
		{
			get { return RefContainer.RC_ContainerType == Constants.ContainerTypes.OpenTop; }
		}
		public ZBool IsOther
		{
			get { return RefContainer.RC_ContainerType == Constants.ContainerTypes.Other; }
		}
		public ZString ISOType
		{
			get { return RefContainer.RC_ISOType; }
		}
		public ZBool IsRefrigerated
		{
			get { return RefContainer.RC_ContainerType == Constants.ContainerTypes.Refrigerated; }
		}

		public ZBool IsHighCube
		{
			get { return RefContainer.RC_IsHighCube; }
		}

		public ZDecimal Length
		{
			get { return RefContainer.RC_Length; }
		}
		public ZString StorageClass
		{
			get { return RefContainer.RC_StorageClass; }
		}
		public ZDecimal TareWeight
		{
			get { return RefContainer.RC_TareWeight; }
		}
		public ZDecimal TEU
		{
			get { return RefContainer.RC_TEU; }
		}
		public ZDecimal Width
		{
			get { return RefContainer.RC_Width; }
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}
	}
}
