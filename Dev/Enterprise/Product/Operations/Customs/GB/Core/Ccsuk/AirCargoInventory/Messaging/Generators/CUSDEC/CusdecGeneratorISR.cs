using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Edifact.D00A.Messages.CUSDEC;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912
{
	class CusdecGeneratorISR : CusdecGeneratorBase
	{
		public CusdecGeneratorISR(ICcsukCusAwb awb, ErrorCollector ec, CusUnderbond isrUnderbond)
			: base(awb, ec, isrUnderbond)
		{
			this.isrUnderbond = (InterShedRemoval)isrUnderbond;
		}

		protected override string AssociationAssignedCode
		{
			get { return "109605"; }
		}

		protected override string BgmDocumentName
		{
			get { return "ISR"; }
		}

		protected override void MakeGroup1ReferenceDeclarantsReference(SegmentGroup1 grp1)
		{
		}

		protected override void MakesGISforLicenceRestricted()
		{
		}

		protected override void MakeTDT()
		{
		}

		protected override void AddShedToLOC85DestinationAirport(LocationAndRelationsAsASingleElement parentLocation)
		{
			parentLocation.SubLocationIdentification3439 = isrUnderbond.NewShedId;
			parentLocation.CodeListQualifier1131_2 = "129";
			parentLocation.CodeListResponsibleAgencyCoded3055_2 = "ZZZ";
		}

		protected override bool ThisProfileCanMakeThisMessage
		{
			get { return Awb != null && (LicenceAndPimaHelper.IsFullShed(Awb) || LicenceAndPimaHelper.IsFallbackShed(Awb)); }
		}

		readonly InterShedRemoval isrUnderbond;
	}
}
