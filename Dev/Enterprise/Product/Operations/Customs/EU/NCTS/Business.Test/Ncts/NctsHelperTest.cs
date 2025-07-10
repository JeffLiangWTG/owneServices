using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHelperTest : TestCaseWithFactory
	{
		public void TestIsUnloadedStateAccepted()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Unloaded State DEC", true, NctsHelper.IsUnloadedStateAccepted("DEC"));
				AssertEquals("Unloaded State dec", true, NctsHelper.IsUnloadedStateAccepted("dec"));
				AssertEquals("Unloaded State DIF", true, NctsHelper.IsUnloadedStateAccepted("DIF"));
				AssertEquals("Unloaded State dif", true, NctsHelper.IsUnloadedStateAccepted("dif"));
				AssertEquals("Unloaded State MIS", true, NctsHelper.IsUnloadedStateAccepted("MIS"));
				AssertEquals("Unloaded State mis", true, NctsHelper.IsUnloadedStateAccepted("mis"));
				AssertEquals("Unloaded State NEW", false, NctsHelper.IsUnloadedStateAccepted("NEW"));
				AssertEquals("Unloaded State new", false, NctsHelper.IsUnloadedStateAccepted("new"));
				AssertEquals("Unloaded State ''", false, NctsHelper.IsUnloadedStateAccepted(string.Empty));
			});
		}

		public void TestIsUnloadedStateDiscrepancy() => CombineAssertions(() =>
		{
			AssertEquals($"Unloaded State {NctsUnloadedStateList.Codes.NEW}", true, NctsHelper.IsUnloadedStateDiscrepancy(NctsUnloadedStateList.Codes.NEW));
			AssertEquals($"Unloaded State {NctsUnloadedStateList.Codes.MIS}", true, NctsHelper.IsUnloadedStateDiscrepancy(NctsUnloadedStateList.Codes.MIS));
			AssertEquals($"Unloaded State {NctsUnloadedStateList.Codes.DIF}", true, NctsHelper.IsUnloadedStateDiscrepancy(NctsUnloadedStateList.Codes.DIF));
			AssertEquals($"Unloaded State {NctsUnloadedStateList.Codes.DEC}", false, NctsHelper.IsUnloadedStateDiscrepancy(NctsUnloadedStateList.Codes.DEC));
			AssertEquals($"Unloaded State empty", false, NctsHelper.IsUnloadedStateDiscrepancy(string.Empty));
		});

		public void TestUnloadedStateInitiallyNew()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Unloaded State empty", false, NctsHelper.UnloadedStateInitiallyNew(new UnloadedStateInitiallyNewTestObject(string.Empty).propertyInfo));
				AssertEquals($"Unloaded State {NctsUnloadedStateList.Codes.NEW}", true, NctsHelper.UnloadedStateInitiallyNew(new UnloadedStateInitiallyNewTestObject(NctsUnloadedStateList.Codes.NEW).propertyInfo));
				AssertEquals($"Unloaded State {NctsUnloadedStateList.Codes.DEC}", false, NctsHelper.UnloadedStateInitiallyNew(new UnloadedStateInitiallyNewTestObject(NctsUnloadedStateList.Codes.DEC).propertyInfo));
			});
		}

		public void TestIsValidCountry()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				AssertEquals("Valid Country Code", true, NctsHelper.IsValidCountry(Core.Constants.CountryCodes.Poland, factory));
				AssertEquals("Non 2-letter Country Code", false, NctsHelper.IsValidCountry("PLE", factory));
				AssertEquals("Invalid Country Code", false, NctsHelper.IsValidCountry("AA", factory));
			});
		}

		public void TestTypeOfSecurityIsEntOrExiOrBth()
		{
			var allCodes = new NctsTypeOfSecurityList().GetAllCodes();
			var codesOfEntOrExiOrBth = new ZString[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH };

			CombineAssertions(() =>
			{
				allCodes.ForEach(securityType =>
				{
					AssertEquals($"TypeOfSecurity is {securityType}.", codesOfEntOrExiOrBth.Contains(securityType), NctsHelper.IsSecurityTypeBTHOrEXIOrENT(securityType));
				});
			});
		}

		public void TestStandardCountryCodeLength()
		{
			AssertEquals("The constants should be equal to 2", 2, NctsHelper.StandardCountryCodeLength);
		}

		public void TestMovementReferenceNumberAlreadyExist()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalHeader.ArrivalMrnFromUser = "MRN 1";
			arrivalHeader.LocalReferenceNumber = "LRN 1";

			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.ArrivalMrnFromUser = "MRN 1";
			departureHeader.LocalReferenceNumber = "LRN 1";
			departureHeader.BH_IsActive = true;

			var secondArrivalHeader = Factory.New<NctsHeader>();
			secondArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			secondArrivalHeader.ArrivalMrnFromUser = "MRN 2";
			secondArrivalHeader.LocalReferenceNumber = "LRN 2";
			secondArrivalHeader.BH_IsActive = false;

			var thirdArrivalHeader = Factory.New<NctsHeader>();
			thirdArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			thirdArrivalHeader.BH_IsActive = true;
			thirdArrivalHeader.ArrivalMrnFromUser = "MRN 2";
			thirdArrivalHeader.LocalReferenceNumber = "LRN 2";
			Factory.Save();

			CombineAssertions(() =>
			{
				(var duplicate, var matchingLRN) = NctsHelper.RetrieveLRNOfDeclarationWithMatchingMRN(arrivalHeader.PK, arrivalHeader.MovementReferenceNumber, Factory, NctsMovementType.Codes.Arrival);
				AssertEquals("The first arrival header should not have a matching LRN because the other header with the same MRN is a departure.", ZString.Empty, matchingLRN);
				AssertEquals("The first arrival header should not have a duplicate because the other header with the same MRN is a departure.", false, duplicate);

				(duplicate, matchingLRN) = NctsHelper.RetrieveLRNOfDeclarationWithMatchingMRN(departureHeader.PK, departureHeader.MovementReferenceNumber, Factory, NctsMovementType.Codes.Arrival);
				AssertEquals("The departure header should have a matching LRN because the other header with the same MRN is an arrival.", "LRN 1", matchingLRN);
				AssertEquals("The departure header should have a duplicate because the other header with the same MRN is an arrival.", true, duplicate);

				(duplicate, matchingLRN) = NctsHelper.RetrieveLRNOfDeclarationWithMatchingMRN(secondArrivalHeader.PK, secondArrivalHeader.MovementReferenceNumber, Factory, NctsMovementType.Codes.Arrival);
				AssertEquals("The second arrival header should have a matching LRN because the other header with the same MRN is an activated arrival.", "LRN 2", matchingLRN);
				AssertEquals("The second arrival header should have a duplicate because the other header with the same MRN is an activated arrival.", true, duplicate);

				(duplicate, matchingLRN) = NctsHelper.RetrieveLRNOfDeclarationWithMatchingMRN(thirdArrivalHeader.PK, thirdArrivalHeader.MovementReferenceNumber, Factory, NctsMovementType.Codes.Arrival);
				AssertEquals("The third arrival header should not have a matching LRN because the other header with the same MRN is inactivated.", ZString.Empty, matchingLRN);
				AssertEquals("The third arrival header should not have a duplicate because the other header with the same MRN is inactivated.", false, duplicate);
			});
		}

		public void TestIsNCTSPreviousDocument() => CombineAssertions(() =>
		{
			AssertEquals("empty", false, NctsHelper.IsNCTSPreviousDocument(ZString.Empty));
			AssertEquals("N123", true, NctsHelper.IsNCTSPreviousDocument("N123"));
			AssertEquals("X123", false, NctsHelper.IsNCTSPreviousDocument("X123"));
		});

		public void TestGetCachedNctsBillAdditionalDocumentStatusList()
		{
			CombineAssertions(() =>
			{
				var statusList = NctsHelper.GetCachedNctsBillAdditionalDocumentStatusList(Factory, true);

				AssertEquals("StatusList CodesAsString", "DEC, MIS", statusList.CodesAsString);
				AssertEquals("DEC Description", "Declared Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.DEC));
				AssertEquals("MIS Description", "Missing Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.MIS));
				AssertSame("Cached", statusList, NctsHelper.GetCachedNctsBillAdditionalDocumentStatusList(Factory, true));

				statusList = NctsHelper.GetCachedNctsBillAdditionalDocumentStatusList(Factory, false);

				AssertEquals("StatusList CodesAsString", "DEC, MIS, NEW", statusList.CodesAsString);
				AssertEquals("NEW Description", "New Value", statusList.GetDescriptionFromCode(NctsBillAdditionalDocumentStatusList.Codes.NEW));
				AssertSame("Cached", statusList, NctsHelper.GetCachedNctsBillAdditionalDocumentStatusList(Factory, false));
			});
		}

		public void TestHas30600AdditionalInformation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When additionalDocument is null.",expected: false, bill.AdditionalDocuments.Has30600AdditionalInformation());

				var additionalDocument = bill.AdditionalDocuments.AddNew();
				additionalDocument.CSI_Code = "30601";
				additionalDocument.CSI_SubType = "INF";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type != 30600.", expected: false, bill.AdditionalDocuments.Has30600AdditionalInformation());

				additionalDocument.CSI_Code = "30600";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type = 30600.", expected: true, bill.AdditionalDocuments.Has30600AdditionalInformation());
			});
		}

		sealed class UnloadedStateInitiallyNewTestObject : NonPersistentBusinessObject
		{
			ZString UnloadedState { get; set; }

			public UnloadedStateInitiallyNewTestObject(string state)
			{
				UnloadedState = state;
			}

			public ZPropertyInfo propertyInfo => GetZPropertyInfo(nameof(UnloadedState));
		}
	}
}
