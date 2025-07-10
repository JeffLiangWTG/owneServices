using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillScreeningDocWrapper : DocumentWrapper
	{
		public AsycudaBillScreeningDocWrapper(AsycudaBillScreening billScreening)
			: base(billScreening, billScreening.Factory)
		{
			this.billScreening = billScreening;
		}
		readonly AsycudaBillScreening billScreening;

		public ZString Result => billScreening.Lookups.ScreeningResultList.GetDescriptionFromCode(billScreening.ASR_Result);

		public ZString AuthorizedPerson => Factory.Load<GlbPerson>(billScreening.ASR_PER_AuthorizedPerson)?.PER_FullName ?? null;

		public ZString PersonType => billScreening.Lookups.ScreeningAuthorizedPersonTypes.GetDescriptionFromCode(billScreening.ASR_AuthorizedPersonType);

		public ZString PersonName => billScreening.ASR_AuthorizedPersonName;

		public ZString PersonIdentifier => billScreening.ASR_AuthorizedPersonIdentifier;

		public ZGuid FacilityPlacePK => billScreening.FacilityPlace.E2_OA_Address;

		public ZString POBox => billScreening.FacilityPlace.POBox;

		public ZString SubDivision => billScreening.FacilityPlace.SubDivision;

		public ZString Number => billScreening.FacilityPlace.Number;
	}
}
