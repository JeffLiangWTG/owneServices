using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class UNDG : IDataObject
	{
		public UNDG()
		{
		}

		public UNDG(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(6)]
		public ZString? UNDGCode { get; set; }
		[MaxLength(4)]
		public ZString? IMOClass { get; set; }
		[MaxLength(30)]
		public ZString? FlashPoint { get; set; }
		[MaxLength(200)]
		public ZString? ProperShippingName { get; set; }
		[MaxLength(100)]
		public ZString? TechicalName { get; set; }
		public UNDGMarinePollutant MarinePollutant { get; set; }
		[MaxLength(3)]
		public ZString? PackingGroup { get; set; }
		public ZBool? PackedInLimitedQuantity { get; set; }
		public OrganizationContact Contact { get; set; }
		public ZDecimal? Weight { get; set; }
		public UnitOfWeight WeightUQ { get; set; }
		public ZDecimal? Volume { get; set; }
		public UnitOfVolume VolumeUQ { get; set; }
		[MaxLength(20)]
		public ZString? SubLabel1 { get; set; }
		[MaxLength(20)]
		public ZString? SubLabel2 { get; set; }
		[MaxLength(2)]
		public ZString? PackingInstructionSection { get; set; }
		public ZInt? PackQty { get; set; }
		public PackageType PackType { get; set; }

		public List<Note> NoteCollection { get; private set; }
		[MaxLength(3)]
		public ZString? Standard { get; set; }
		public UNDGState? State { get; set; }
		public ZDecimal? NetExplosiveWeight { get; set; }
		public UnitOfWeight NetExplosiveWeightUQ { get; set; }
		public ZDecimal? Radioactivity { get; set; }
		public UnitOfRadioactivity RadioactivityUQ { get; set; }
		public ZDecimal? RadioactiveTransportIndex { get; set; }
		public ZDecimal? RadioactiveCriticalitySafetyIndex { get; set; }
		[MaxLength(3)]
		public ZString? RadioactiveLabelCategory { get; set; }
		[MaxLength(2)]
		public ZString? RadionuclideElement { get; set; }
		[MaxLength(50)]
		public ZString? RadionuclideElementSuffix { get; set; }
		public ZDecimal? RadioactiveMaximumActivity { get; set; }
		[MaxLength(3)]
		public ZString? RadioactiveMaximumActivityUnit { get; set; }
		public ZBool? PackedInExceptedQuantity { get; set; }
		public CodeDescriptionPair EmergencyScheduleFire { get; set; }
		public CodeDescriptionPair EmergencyScheduleSpillage { get; set; }
		public CodeDescriptionPair4Char MedicalFirstAidGuide { get; set; }
		public CodeDescriptionPair ExceptedQuantityCode { get; set; }
		public ZBool? HasOverpack { get; set; }
		[MaxLength(35)]
		public ZString? OverpackID { get; set; }
		public ZBool? IsCombustible { get; set; }
		public ZDate? SpecialPermitIssuedDate { get; set; }
		[MaxLength(30)]
		public ZString? SpecialPermitNumber { get; set; }
		[MaxLength(4)]
		public ZString? WasteCode { get; set; }
		public ZBool? SalvagePackaging { get; set; }
		public ZBool? ResidueLastContained { get; set; }
		[MaxLength(100)]
		public ZString? Description { get; set; }
		public ZBool? FissileExcepted {  get; set; }
		public ZBool? ExclusiveUse {  get; set; }
		public ZBool? HighwayRouteControlledQuantity { get; set; }
	}
}
