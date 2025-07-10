namespace Enterprise.Customs.CA.Business
{
	using CargoWise.Types;

	interface IEDIReleaseMessageAttachee
	{
		ZDateTime ReleaseDate { get; set; }
		ZString ReleaseOffice { get; set; }
		bool SettingReleaseDateWithStatusUpdate { get; set; }
		ZString CargoControlNumbersReleaseStatus { get; }
		ZBool CargoControlNumbersAwaitingReply { get; }
	}
}
