using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ComplianceRisk.Business
{
	public class CompliancePartyRiskLog : NonPersistentBusinessObject
	{
		readonly ScreeningStatusesList screeningStatusesList;

		public CompliancePartyRiskLog(Party party, ScreeningStatusesList screeningStatusesList, IStmEntityScreeningLog entityScreeningLog)
		{
			Party = party;
			this.screeningStatusesList = screeningStatusesList;
			EntityScreeningLog = entityScreeningLog;
		}

		public ZString Status => Party.Status;

		[ResourceStringData("7feca50a-c7f6-46e1-9b11-6ee616e9db4c", Caption = "Risk Status")]
		public ZString StatusDescription => screeningStatusesList.GetDescriptionFromCode(Status);

		[ResourceStringData("f1c50b82-21e1-4ef7-a408-56b23b2a17fa", Caption = "Party Name")]
		public ZString Code => Party.Code;

		public Party Party { get; }

		[ResourceStringData("8a64ce98-9ae7-4a20-89ec-7cdae4a281e2", Caption = "Description")]
		public ZString Description => Party.Description;

		[ResourceStringData("c4e3fd48-ee28-40d8-a41a-483adccaa263", Caption = "Screened Date")]
		public ZDateTime LastScreenDate => EntityScreeningLog?.PJ_ScreenDate ?? ZDateTime.MinSmallDateTimeValue;

		[ResourceStringData("de8d3e4e-7ad9-4f67-bcf7-1238d807e73b", Caption = "High Confidence")]
		public ZString HighConfidence => EntityScreeningLog?.PJ_HighConfidenceResults ?? ZString.Empty;

		[ResourceStringData("0dbbc640-2b9a-437e-8525-3450dd61e0fe", Caption = "Medium Confidence")]
		public ZString MediumConfidence => EntityScreeningLog?.PJ_MediumConfidenceResults ?? ZString.Empty;

		[ResourceStringData("8df28f02-10a0-4f71-a318-4ddc3076e2ef", Caption = "Low Confidence")]
		public ZInt LowConfidence => EntityScreeningLog?.PJ_LowConfidenceResultsCount ?? 0;

		[ResourceStringData("265780d7-fce0-40cc-a10f-f90e340a0b50", Caption = "Included Lists")]
		public ZString IncludedLists => EntityScreeningLog?.PJ_IncludedLists ?? ZString.Empty;

		[ResourceStringData("0b34a8b3-896a-42f2-9c5e-d03776110b39", Caption = "Excluded Lists")]
		public ZString ExcludedLists => EntityScreeningLog?.PJ_ExcludedLists ?? ZString.Empty;

		public IStmEntityScreeningLog EntityScreeningLog { get; }
	}
}
