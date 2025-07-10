using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsDepartureHeaderContainer : NctsDepartureHeaderContainer, Integration.Customs.FR.IFRNctsDepartureHeaderContainer
	{
		public FRNctsDepartureHeaderContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		[ResourceStringData("FRNctsHeaderContainer.BC_RC", Caption = "Type")]
		public override ZGuid BC_RC { get => base.BC_RC; set => base.BC_RC = value; }

		[ResourceStringData("FRNctsHeaderContainer.BC_Mode", Caption = "Mode")]
		[List(nameof(Lookups) + "." + nameof(FRNctsDepartureHeaderContainerLookups.Modes))]
		public override ZString BC_Mode { get => base.BC_Mode; set => base.BC_Mode = value; }

		public new FRNctsDepartureHeaderContainerLookups Lookups => (FRNctsDepartureHeaderContainerLookups)base.Lookups;
		protected override CusInBondContainerLookups GetNewLookups() => new FRNctsDepartureHeaderContainerLookups(this);

		protected override CusInBondContainerValidation GetNewPhase5Validation() => new FRNctsDepartureHeaderContainerPhase5Validation(this);

		protected override CusInBondContainerValidation GetNewPhase4Validation() => new FRNctsDepartureHeaderContainerPhase4Validation(this);

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = new FRNctsDepartureHeaderContainerValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;

		protected override bool IsContainerisedCore => BC_Mode == Core.Constants.ContainerModes.FCL || BC_Mode == Core.Constants.ContainerModes.LCL || BC_Mode == Core.Constants.ContainerModes.Containerised;
	}
}
