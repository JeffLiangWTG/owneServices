using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationDtbBookingParentTest : BaseJobDeclarationDtbBookingParentTest
	{
		public override void TestSupportedDirections()
		{
			var declaration = GetNewParent();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var bookingParent = declaration as TransportBookings.Shared.IDtbBookingParent;
			AssertEquals(0, bookingParent.GetSupportedDirections().Length);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			bookingParent = declaration;
			AssertEquals(0, bookingParent.GetSupportedDirections().Length);

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			bookingParent = declaration;
			AssertEquals(0, bookingParent.GetSupportedDirections().Length);
		}

		protected override BaseJobDeclaration GetNewParent()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
