using System.ComponentModel;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public enum ReasonForArchiving
	{
		[Description("Status1 and final CAC date both older than 7 days.")]
		Status1DateAndCacDatesBothOlderThanOneWeek,

		[Description("Status1 date older than 7 days and SDC=C/E.")]
		Status1DateOlderThanAWeekAndEuropeanSDC,

		[Description("NPX exceeds NPR and final CAC older than 180 days.")]
		NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days,

		[Description("Not archived because NPR exceeds NPX. (Add your custom event to exclude this from the report)")]
		DoNotArchiveBecauseNprGreaterThanNpx,

		[Description("NPR > NPX, but it has the custom event to exclude it, so can be archived")]
		ArchiveBecauseOfCustomEventExclusion,

		[Description("Pre-Arrival record on network older than 4 days.")]
		PreArrivalOlderThanFourDays,

		[Description("Not archived because Status 2 is not set on ETSF record.")]
		DoNotArchiveBecauseStatus2NotSetForEtsf,

		[Description("Archived because this consignment no longer belongs to any of our badges.")]
		NoLongOurConsignment,

		[Description("All children now archived.")]
		LastChildRecordWasArchived,

		[Description("Not archived because all other conditions not met.")]
		None,  //Called None for the benefit of the code anal-yser

#if DEBUG
		[Description("Reason for archiving for testing only.")]
		TestingOnly
#endif
	}
}
