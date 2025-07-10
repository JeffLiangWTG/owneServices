using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondInvoicingSupporter))]
	public class CusUnderbondInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestCusEntryNumCorrectCountryCode()
		{
			CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("9914N", "67094168242");
			FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var underbond = Factory.New<CusUnderbond>();
				underbond.C4_DestinationPremiseID = "9914N";
				Factory.Save();
				var entryNumbers = underbond.CusEntryNumbers;
				AssertEquals(2, entryNumbers.Length);
				AssertEquals(true, entryNumbers.All(x => x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Australia));
			}
		}

		public void TestAU_OutturnResponsiblePartyID()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "A12";
			oceanBill.CB_LloydsIMO = "12345";
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "67094168242";
			AssertEquals("67094168242", underbond.AU_OutturnResponsiblePartyID);

			underbond.AU_OutturnResponsiblePartyID = "123456";
			AssertEquals("123456", underbond.AU_OutturnResponsiblePartyID);
		}

		public void TestUniversalDataContext()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "A12";
			oceanBill.CB_LloydsIMO = "12345";
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "192K";
			IDataContextManager manager = null;
			AssertNoExceptionThrown(() => { manager = underbond.GetUniversalDataContextManager(); });
			AssertNotNull("CusUnderbond should have [UniversalDataContext(DataContextType.UnderBond)] attribute", manager);
			AssertEquals(DataContextType.UnderBond, manager.DataContextType);
			AssertEquals("", manager.DataContextKey);
		}

		public void TestGetContactOrganisationWithCusUnderbondDocumentSupporter()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsLocalDocument = true;
			DocumentPack pack = new DocumentPack(menuItem);
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "A12";
			oceanBill.CB_LloydsIMO = "12345";
			var container = oceanBill.Containers.AddNew();
			AssertNoExceptionThrown(() => pack.DocumentSupporter = new CusUnderbondDocumentSupporter(container.Underbonds.AddNew()));
		}

		public void TestSeaOutturnHeader()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "A12";
			oceanBill.CB_LloydsIMO = "12345";
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "192K";
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "A12";
			outturnHeader.C6_OutturningPremiseID = "192K";
			AssertEquals(outturnHeader, underbond.OutturnHeader);
		}

		public void TestIsSeaOutturned()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "A12";
			oceanBill.CB_LloydsIMO = "12345";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN123";
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "192K";
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "A12";
			outturnHeader.C6_OutturningPremiseID = "192K";
			var outturnLine = outturnHeader.Outturns.AddNew();
			outturnLine.C5_ContainerNumber = "CN123";
			outturnLine.C5_HouseBill = "HB123";

			Assert(!underbond.IsSeaOutturned("HB123"));
			outturnLine.C5_CargoUnpackDate = ZDateTime.Today;
			Assert(underbond.IsSeaOutturned("HB123"));
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			CusUnderbond cusUnderbond = Factory.NewWithValidTestData<CusUnderbond>();
			return cusUnderbond;
		}

		public void TestIssue00649838()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.CustomsCodes.AddNew("CCP", "9914N", "AU");

			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "111";
			hAWB.CS_GoodsDescription = "description";
			hAWB.CS_PiecesManifested = 3;
			var underbond = mAWB.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_OriginPremiseID = "9920A";
			Assert("PreCondition", underbond.CanDoOutturn);

			((ICusUnderbondNilUnderbondPerformer)mAWB).PerformNilUnderbond(underbond);
			AssertEquals(1, underbond.Outturns.Count);
			Factory.Save();

			underbond.C4_DestinationPremiseID = "9914Y";
			Assert("PreCondition", !underbond.CanDoOutturn);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			underbond.C4_DestinationPremiseID = "9914N";
			Assert("PreCondition", underbond.CanDoOutturn);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}
	}
}
