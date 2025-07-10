using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED819HeaderProvider))]
	class ED819HeaderProviderTest : HeaderProviderAbstractTest<ED819HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED819HeaderProvider(emcsDeclaration, null));
		}

		public void TestSequenceNumber()
		{
			var cusEntryNumber = CusEntryNumber.New(emcsDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryLineReference = "1";

			AssertEquals(1, HeaderProvider.SequenceNumber);
		}

		public void TestDestinationOfficeReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No offices entered", string.Empty, HeaderProvider.DestinationOfficeReferenceNumber);
				var deliveryCustomsOffice = emcsDeclaration.CustomsOffices.AddNew();
				deliveryCustomsOffice.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival;
				deliveryCustomsOffice.CY_Data = "DE00876";

				var dispatchCustomsOffice = emcsDeclaration.CustomsOffices.AddNew();
				dispatchCustomsOffice.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;
				dispatchCustomsOffice.CY_Data = "DE00934";
				AssertEquals("Delivery office code", "DE00876", HeaderProvider.DestinationOfficeReferenceNumber);
			});
		}

		public void TestRejectedFlag()
		{
			AssertEquals("Not rejected", false, HeaderProvider.RejectedFlag);

			alertOrReject.RejectedFlag = ZBool.True;
			AssertEquals("Rejected", true, HeaderProvider.RejectedFlag);
		}

		public void TestDateOfAlertOrRejection()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Null by default", null, HeaderProvider.DateOfAlertOrRejection);

				alertOrReject.DateOfAlertOrRejection = new ZDate(2020, 01, 02);
				AssertEquals("Returns Correct value", new DateTime(2020, 01, 02, 0, 0, 0), HeaderProvider.DateOfAlertOrRejection);
			});
		}

		public void TestConsigneeTraderNull()
		{
			AssertNull("No Consignee Trader entered", HeaderProvider.ConsigneeTrader);
		}

		public void TestConsigneeTrader()
		{
			var orgHeader = GetPartyTraderExciseNumberOrg("TEN821");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "43762894", Core.Constants.CountryCodes.Greece);
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var consigneeTrader = HeaderProvider.ConsigneeTrader;
			CombineAssertions(() =>
			{
				AssertEquals("Trader ID", "TEN821", consigneeTrader.TraderId);
				AssertEquals("EORI Number", "GR43762894", consigneeTrader.EoriNumber);
			});
		}

		public void TestAlertOrRejectionReasons_Null()
		{
			AssertEquals("Collection doesn't return null but is empty", false, HeaderProvider.AlertOrRejectionReasons.Any());
		}

		public void TestAlertOrRejectionReasons()
		{
			for (var i = 1; i < 10; i++)
			{
				alertOrReject.AlertOrRejectionReasons.AddNew();
			}
			var reasons = HeaderProvider.AlertOrRejectionReasons;
			AssertEquals("9 records, contents is tested in the provider", 9, reasons.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			alertOrReject = new AlertOrRejectSendingAction(emcsDeclaration);
		}
		AlertOrRejectSendingAction alertOrReject;

		protected new IED819Header HeaderProvider => base.HeaderProvider;

		protected override ED819HeaderProvider GetHeaderProvider() => new ED819HeaderProvider(emcsDeclaration, alertOrReject);

		protected override ED819HeaderProvider GetProvider()
		{
			var orgHeader = GetPartyTraderExciseNumberOrg("TEN821");
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			return new ED819HeaderProvider(emcsDeclaration, alertOrReject);
		}

		protected override IEnumerable<Expression<Func<ED819HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ConsigneeTrader;
		}
	}
}
