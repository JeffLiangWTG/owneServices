using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC170CProvider))]
	sealed class CC170CProviderTest : NctsHeaderProviderAbstractTest<CC170CProvider>
	{
		public void TestReducedDatasetIndicator()
		{
			nctsHeader.BH_FTZMove = true;
			nctsHeader.MovementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals(true, Provider.ReducedDatasetIndicator);

			nctsHeader.BH_FTZMove = false;
			nctsHeader.MovementHeader.BM_TypeOfSecurity = "ZZZ";
			AssertEquals(false, Provider.ReducedDatasetIndicator);
		}

		public void TestLimitDate()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_ExportDate = new ZDateTime(1994, 2, 1);
				AssertEquals(new DateTime(1994, 2, 1), Provider.LimitDate);

				var header2 = Factory.New<NctsHeader>();
				header2.SetMovementType(MovementType);
				header2.MovementHeader.BM_ExportDate = ZDateTime.Empty;
				var provider2 = new CC170CProvider(header2);
				AssertNull("LimitDate should be null from Empty", provider2.LimitDate);

				var header3 = Factory.New<NctsHeader>();
				header3.SetMovementType(MovementType);
				header3.MovementHeader.BM_ExportDate = new ZDateTime(DateTime.MinValue);
				var provider3 = new CC170CProvider(header3);
				AssertNull("LimitDate should be null from MinValue", provider3.LimitDate);
			});
		}

		public void TestLimitDate_Empty()
		{
			AssertNull(Provider.LimitDate);
		}

		public void TestLimitDate_HasACRAuthorization()
		{
			nctsHeader.MovementHeader.BM_ExportDate = new ZDateTime(1994, 2, 1);
			var authorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = Constants.CusPermitHeaderTypes.ACR;
			AssertNull(Provider.LimitDate);
		}

		public void TestLimitDate_HasACRAuthorization_WhenNotPhase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.MovementHeader.BM_ExportDate = new ZDateTime(1994, 2, 1);
			var authorizationUsage = nctsHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = Constants.CusPermitHeaderTypes.ACR;
			AssertNull(Provider.LimitDate);
		}

		public void TestHolderOfTheTransitProcedureIdentificationNumber()
		{
			AssertNull(Provider.HolderOfTheTransitProcedureIdentificationNumber);
		}

		public void TestHolderOfTheTransitProcedureIdentificationNumberNoTIRInBondEntryType()
		{
			CreatePrincipal();
			nctsHeader.MovementHeader.BM_InBondEntryType = "OTH";
			AssertEquals("BEHolderID", Provider.HolderOfTheTransitProcedureIdentificationNumber);
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
		}

		protected override string MessageType => Constants.MessageTypes.CC170C;

		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
