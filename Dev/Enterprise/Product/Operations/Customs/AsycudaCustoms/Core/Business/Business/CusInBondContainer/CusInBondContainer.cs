using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondContainer : Customs.Business.BaseCusInBondContainer
	{
		public CusInBondContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("665569EC-334A-475F-B93A-6C371EE1B6FB", Caption = "Container Number")]
		[CargoWise.ComponentModel.List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.CusContainers))]
		public override ZString BC_ContainerNum { get => base.BC_ContainerNum; set => base.BC_ContainerNum = value; }

		public new CusInBondContainerLookups Lookups
		{
			get { return (CusInBondContainerLookups)base.Lookups; }
		}

		protected override Customs.Business.CusInBondContainerLookups GetNewLookups()
		{
			return new CusInBondContainerLookups(this);
		}

		public CusInBondMoveDetail MoveDetail => Factory.Load<CusInBondMoveDetail>(BC_ParentID);

		protected override Customs.Business.CusInBondContainerValidation GetNewValidation()
		{
			return new CusInBondContainerValidation(this);
		}
	}
}
