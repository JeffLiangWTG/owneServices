using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(HolderOfTheTransitProcedureProvider))]
	class HolderOfTheTransitProcedureProviderTest : PartyProviderAbstractTest<HolderOfTheTransitProcedureProvider>
	{
		public void TestTirHolderIdentificationNumber()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_InBondEntryType = Constants.OrgCusCodeTypes.TransitOperationHolder;

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIRNumber", Core.Constants.CountryCodes.Belgium)
			};
			address.Organisation.CustomsCodes.AddRange(orgCusCodes);

			nctsHeader.DocAddresses.Add(address);

			AssertEquals("TIRNumber", Provider.TirHolderIdentificationNumber);
		}

		public void TestTirHolderIdentificationNumberNoTIRInBondEntryType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_InBondEntryType = "OTH";

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIRNumber", Core.Constants.CountryCodes.Belgium)
			};
			address.Organisation.CustomsCodes.AddRange(orgCusCodes);

			nctsHeader.DocAddresses.Add(address);

			AssertNull(Provider.TirHolderIdentificationNumber);
		}

		protected override HolderOfTheTransitProcedureProvider CreateProvider(JobDocAddress address) => (HolderOfTheTransitProcedureProvider)Activator.CreateInstance(typeof(HolderOfTheTransitProcedureProvider), address, ZBool.True, false);
	}
}
