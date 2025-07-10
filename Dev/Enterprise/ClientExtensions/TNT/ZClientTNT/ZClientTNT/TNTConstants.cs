


namespace Enterprise.Client.TNT
{
	public abstract class TNTConstants
	{
		#region Service Task Codes

		//Service Tasks for TNT 
		public const string NADFileImportSrvTaskCode = "ZT1";
		public const string OutTurnFileImportSrvTaskCode = "ZT2";
		public const string AirCargoResponseExportSrvTaskCode = "ZT3";
		public const string QuantumFileImportSrvTaskCode = "ZT4";

		#endregion

		public const string DataImportNotificationGroupCode = "QAN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string ErrorNotificationEmailSubject = "CargoWise One Quantum Upload Notifications";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string NotificationEmailSubjectForOutTurnFiles = "CargoWise One Automated Import Result of OutTurn Files";
		public const string DateFormat = "ddMMyy";
		public const string OuturnFileExtension = "out";
	}
}
