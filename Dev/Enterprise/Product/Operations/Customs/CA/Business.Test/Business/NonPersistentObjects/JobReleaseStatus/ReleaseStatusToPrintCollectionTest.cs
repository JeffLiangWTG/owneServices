using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ReleaseStatusToPrintCollection))]
	sealed class ReleaseStatusToPrintCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReleaseStatusToPrintCollection>
	{
		public void TestCollection()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN3";
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN4";
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", "1"));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN2", "2"));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN3", "3"));

			AssertEquals("ReleaseStatuses.Count", 3, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatus can be printed from EDIReleaseMessage only", 1, declaration.ReleaseStatusesToPrint.Count);

			declaration.CargoControlNumbers.RemoveAndDeleteAll();
			entry.Messages.RemoveAndDeleteAll();
			declaration.ReleaseStatuses.Load();
			var day1 = new ZDateTime(2011, 06, 29, 01, 02, 00);
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage(string.Empty, day1, "1", day1));
			AssertEquals("ReleaseStatuses.Count", 0, declaration.ReleaseStatuses.Count);
			AssertEquals("Common release response (no CCN) can be printed even if no CCNs on declaration", 1, declaration.ReleaseStatusesToPrint.Count);
			AssertEquals("ReleaseStatusesToPrint 1, RL_ProcessingDate", day1, declaration.ReleaseStatusesToPrint[0].RL_ReleaseDate);
		}

		#region Implementation

		protected override ReleaseStatusToPrintCollection GetCollectionToTest()
		{
			return new ReleaseStatusToPrintCollection(Factory.New<JobDeclaration>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReleaseStatus(Factory.New<CargoControlNumber>());
		}

		#endregion
	}
}
