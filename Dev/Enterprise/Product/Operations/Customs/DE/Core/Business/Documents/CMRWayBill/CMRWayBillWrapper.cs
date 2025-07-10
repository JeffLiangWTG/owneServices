using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Documents.CMR
{
	sealed class CMRWayBillWrapper : EU.Business.Documents.CMR.CMRWayBillWrapper
	{
		public CMRWayBillWrapper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString Box14PaymentCarriageCore => Box14PaymentCarriageForDE;
		protected override ZString Box15CashOnDeliveryCore => Box15CashOnDeliveryForDE;
		protected override ZString Box19SpecialAgreementsCore => Box19SpecialAgreementsForDE;
		protected override ZString Box20ToBePaydByCore => Box20ToBePaydByForDE;
		protected override ZString Box23TransportAndTrailerIDCore => Box23TransportAndTrailerIDForDE;

		#region Implementation

		const string Box14PaymentCarriageForDE = "15";
		const string Box19SpecialAgreementsForDE = "20";
		const string Box20ToBePaydByForDE = "19";
		const string Box15CashOnDeliveryForDE = "14";
		const string Box23TransportAndTrailerIDForDE = "23";

		#endregion
	}
}
