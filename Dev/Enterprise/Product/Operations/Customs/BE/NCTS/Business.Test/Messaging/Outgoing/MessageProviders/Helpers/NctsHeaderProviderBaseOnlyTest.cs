using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureHeaderProvider))]
	sealed class NctsHeaderProviderBaseOnlyTest : NctsHeaderProviderAbstractTest<NctsDepartureHeaderProvider>
	{
		public void TestRepresentative() => CombineAssertions(() =>
		{
			AssertNull("not available", Provider.Representative);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", nctsHeader.MovementHeader.Representative, "1", traderTir: "GBR/022/1234567");
			var provider = new NctsDepartureHeaderProvider(nctsHeader);
			AssertNotNull("available", provider.Representative);
		});

		public void TestHolderOfTheTransitProcedure()
		{
			AssertNotNull(Provider.HolderOfTheTransitProcedure);
		}

		public void TestCustomsOfficeOfDeparture()
		{
			var movementHeader = nctsHeader.MovementHeader;
			var customsOfficeOfDeparture = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			customsOfficeOfDeparture.CY_Data = "DepID";

			AssertEquals("DepID", Provider.CustomsOfficeOfDeparture);
		}

		public void TestCustomsOfficeOfDestination()
		{
			var movementHeader = nctsHeader.MovementHeader;
			var customsOfficeOfDestination = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			customsOfficeOfDestination.CY_Data = "DesID";

			AssertEquals("DesID", Provider.CustomsOfficeOfDestination);
		}

		public void TestLRN()
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRNOfTheNCT";
			AssertEquals("LRNOfTheNCT", Provider.LRN);
		}

		public void TestHolderOfTheTransitProcedureIdentificationNumber()
		{
			CreatePrincipal();
			AssertEquals("BEHolderID", Provider.HolderOfTheTransitProcedureIdentificationNumber);
		}

		public void TestMRN()
		{
			nctsHeader.ArrivalMrnFromUser = "MRNOfTheNCTS";
			AssertEquals("MRNOfTheNCTS", Provider.MRN);
		}

		public void TestHolderOfTheTransitProcedureTIRNumber()
		{
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			CreatePrincipal();
			AssertEquals("TIRNumber", Provider.HolderOfTheTransitProcedureTIRNumber);
		}

		public void TestHolderOfTheTransitProcedureTIRNumberNoTIRInBondEntryType()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = "OTH";
			AssertNull(Provider.HolderOfTheTransitProcedureTIRNumber);
		}

		public void TestMessageSender()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection
			{
				new MessageVersionRegistry { DomainCode = Constants.MessageVersionRegistryDomainCodes.NCTSP5, TargetSystemName = "NCTS.BE" }
			};
			using (BECustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertEquals("NCTS.BE", Provider.MessageSender);
			}
		}

		public void TestMessageRecipient()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection
			{
				new MessageVersionRegistry { DomainCode = Constants.MessageVersionRegistryDomainCodes.NCTSP5, TargetSystemName = "NCTS.BE" }
			};
			using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertEquals("NCTS.BE", Provider.MessageRecipient);
			}
		}

		[TestDate(2020, 10, 28, 11, 38, 00)]
		public void TestPreparationDateTime()
		{
			AssertEquals(new DateTime(2020, 10, 28, 11, 38, 00), Provider.PreparationDateTime);
		}

		public void TestMessageIdentification()
		{
			AssertEquals("<<SENDERS REFERENCE PLACE HOLDER>>", Provider.MessageIdentification);
		}

		public void TestCorrelationIdentifier()
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRNOfTheNCT";
			AssertEquals("LRNOfTheNCT", Provider.CorrelationIdentifier);
		}

		public void TestCorrelationIdentifierWithCIDEntryNum()
		{
			nctsHeader.CorrelationIdentifierEntryNumber.CE_EntryLineReference = "XXXXX";
			AssertEquals("XXXXX", Provider.CorrelationIdentifier);
		}

		protected override string MessageType => ZString.Empty;

		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
