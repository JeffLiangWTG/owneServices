using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Testing
{
	public class CorrelationIDGeneratorTest : TestCaseWithFactory
	{
		public void TestInitCorrelationID()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			AssertEquals(ZString.Empty, statement.CorrelationID);
			Factory.Save();
			AssertNotNullOrEmpty(statement.CorrelationID);
			AssertEquals(statement.CorrelationID, statement.ChargesDetail.B3_BrokerReference);

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(ZString.Empty, entry.CorrelationID);
			Factory.Save();
			AssertNotNullOrEmpty(entry.CorrelationID);
			AssertEquals(true, Factory.ExistsInDatabase(CusEntryNumber.Schema.TableName, new ZQuery(CusEntryNumSchema.CE_EntryNum, entry.CorrelationID)));
			AssertNotEquals(statement.CorrelationID, entry.CorrelationID);
		}

		public void TestCorrelationIDUsingCustomisation()
		{
			var customisation = new CorrelationIDCustomisation();
			var keyDirection = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction];
			keyDirection.Include = true;
			var keySequence = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber];
			keySequence.Detail = "9"; // max length of sequence reduced to 9 so that Direction + Sequence fit in 10 chars so that max length validation passes

			AssertEquals("pre-req", true, customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction].Include);

			using (FRCustomsDataRegistry.Instance.CorrelationIDCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customisation))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.CustomsEntryHeaders.AddNew();
				AssertEquals(ZString.Empty, entry.CorrelationID);
				Factory.Save();
				AssertNotNullOrEmpty(entry.CorrelationID);
				AssertEquals("correlationID contains Direction and Sequence", "O000000001", entry.CorrelationID);
			}
		}

		public void TestEntryHeaderCorrelationIDPrefix()
		{
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "WTL"))
			{
				Env.Registry.PhysicalServerID = "FRM";
				GlbCompany.CurrentCompany.GC_Code = "DFR";

				var deltaGDeclaration = Factory.New<JobDeclaration>();
				deltaGDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				var deltaGEntry = deltaGDeclaration.CustomsEntryHeaders.AddNew();
				AssertEquals("Prerequisite: entry CorrelationIDPrefix is empty.", ZString.Empty, deltaGEntry.CorrelationIDPrefix);
				Factory.Save();
				AssertEquals("Entry's correlationID should not be prefixed if business object has CorrelationIDPrefix empty.", "0000000001", deltaGEntry.CorrelationID);

				var nonDeltaGDeclaration = Factory.New<JobDeclaration>();
				nonDeltaGDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				var nonDeltaGEntry = nonDeltaGDeclaration.CustomsEntryHeaders.AddNew();
				AssertEquals("Prerequisite: entry CorrelationIDPrefix is not empty.", "WTLDFRFRM", nonDeltaGEntry.CorrelationIDPrefix);
				Factory.Save();
				AssertEquals("Entry's correlationID should be prefixed  if business object has CorrelationIDPrefix not empty.", $"WTLDFRFRM0000000002", nonDeltaGEntry.CorrelationID);
			}
		}
	}
}
