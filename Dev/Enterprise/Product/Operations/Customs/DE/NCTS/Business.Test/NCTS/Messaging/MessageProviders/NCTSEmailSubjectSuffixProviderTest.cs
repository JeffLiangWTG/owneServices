using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSEmailSubjectSuffixProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSEmailSubjectSuffixProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("MovementHeader missing", () => new NCTSEmailSubjectSuffixProvider(null));
		}

		public void TestSetEmailSubjectSuffix_Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader = nctsHeader.MovementHeader;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "DE1234567";
			dataProvider = GetProvider();

			var email = new EmailDef();
			email.Subject = "Test Subject";
			dataProvider.SetEmailSubjectSuffix(email);

			AssertEquals("Departure", "Test Subject LRN: DE1234567", email.Subject);
		}

		public void TestSetEmailSubjectSuffix_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader = nctsHeader.ArrivalMovementHeader;
			nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "DE1234567";
			dataProvider = GetProvider();

			var email = new EmailDef();
			email.Subject = "Test Subject";
			dataProvider.SetEmailSubjectSuffix(email);

			AssertEquals("Arrival", "Test Subject Ref.: DE1234567", email.Subject);
		}

		protected override NCTSEmailSubjectSuffixProvider GetProvider() => new NCTSEmailSubjectSuffixProvider(movementHeader);

		NCTSEmailSubjectSuffixProvider dataProvider;
		NctsCommonMovementHeader movementHeader;
	}
}
