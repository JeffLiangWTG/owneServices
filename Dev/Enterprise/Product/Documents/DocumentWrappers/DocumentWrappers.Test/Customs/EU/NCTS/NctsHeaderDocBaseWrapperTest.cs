using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class NctsHeaderDocBaseWrapperTest : TestCaseWithFactory
	{
		public void TestWrapper()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when nctsHeader parameter is null", () => new NctsHeaderDocBaseWrapperForTesting(null, Factory));
			AssertExceptionThrown<ArgumentNullException>("Exception expected when factory parameter is null", () => new NctsHeaderDocBaseWrapperForTesting(Factory.New<NctsHeader>(), null));
		}

		public void TestIsPhase5()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("NCTS5", true, wrapper.IsPhase5);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("NCTS4", false, wrapper.IsPhase5);
		}

		public void TestIsAddressExtended()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("IsAddressExtended", true, wrapper.IsAddressExtended);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("IsAddressExtended", false, wrapper.IsAddressExtended);
			}
		}

		public void TestIsFallBackActive()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			nctsHeader.IsFallBackActiveForTest = true;
			AssertEquals("fallback is active", true, wrapper.IsFallBackActive);

			nctsHeader.IsFallBackActiveForTest = false;
			AssertEquals("fallback is not active", false, wrapper.IsFallBackActive);
		}

		public void TestMrn()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			AssertEquals("When MovementReferenceNumber is empty", ZString.Empty, wrapper.Mrn);

			var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
			entryNum.CE_EntryNum = "21FR00007411BBC885";
			AssertEquals("When MovementReferenceNumber has value", "21FR00007411BBC885", wrapper.Mrn);
		}

		public void TestPending_MRN()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			AssertEquals("Declaration pending MRN", wrapper.Pending_MRN);
		}

		public void TestMOVEMENTREFERENCENUMBER()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			nctsHeader.IsFallBackActiveForTest = true;
			AssertEquals("When IsFallBackActive is true", ZString.Empty, wrapper.MOVEMENTREFERENCENUMBER);

			nctsHeader.IsFallBackActiveForTest = false;
			AssertEquals("When Mrn is empty", wrapper.Pending_MRN, wrapper.MOVEMENTREFERENCENUMBER);

			var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
			entryNum.CE_EntryNum = "21FR00007411BBC885";
			AssertEquals("When Mrn has value", "21FR00007411BBC885", wrapper.MOVEMENTREFERENCENUMBER);
		}

		public void TestEMAILSUBJECT()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			nctsHeader.IsFallBackActiveForTest = true;
			AssertEquals("When IsFallBackActive is true", "Fallback", wrapper.EMAILSUBJECT);

			nctsHeader.IsFallBackActiveForTest = false;
			var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
			entryNum.CE_EntryNum = "21FR00007411BBC885";
			AssertEquals("When Mrn has value", "21FR00007411BBC885", wrapper.EMAILSUBJECT);
		}

		public void TestLOCALREFERENCENUMBER()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			var wrapper = new NctsHeaderDocBaseWrapperForTesting(nctsHeader, Factory);
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.LocalReferenceNumber = "LRN001";
			AssertEquals("LRN001", wrapper.LOCALREFERENCENUMBER);
		}
	}

	sealed class NctsHeaderDocBaseWrapperForTesting : NctsHeaderDocBaseWrapper
	{
		public NctsHeaderDocBaseWrapperForTesting(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public new ZBool IsAddressExtended => base.IsAddressExtended;
	}
}
