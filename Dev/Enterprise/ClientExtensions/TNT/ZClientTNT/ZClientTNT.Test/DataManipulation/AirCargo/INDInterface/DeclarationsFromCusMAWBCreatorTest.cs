using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(DeclarationsFromCusMAWBCreator))]
	public class DeclarationsFromCusMAWBCreatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "MAWB";
			Factory.Save();
			DeclarationsFromCusMAWBCreator creator = new DeclarationsFromCusMAWBCreator(masterBill);
			AssertNotNull("Creator should not be null", creator);
		}

		public void TestCreateDeclarations()
		{
			#region Setup Masterbill and House bill data
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "MAWB";
			CusHAWB normalHouseBill = masterBill.ChildBills.AddNew();
			normalHouseBill.CS_HAWB = "HAWB1";
			normalHouseBill.CS_RL_NKDestination = "AUSYD";
			CommonShipment newShipment = Factory.New<CommonShipment>();
			CusHAWB housBillLinkedToShipment = masterBill.ChildBills.AddNew();
			housBillLinkedToShipment.CS_HAWB = "HAWB2";
			housBillLinkedToShipment.CS_RL_NKDestination = "AUSYD";
			housBillLinkedToShipment.CS_JS = newShipment.PK;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "MAWB";
			declaration.JE_HouseBill = "DECLHAWB";
			CusHAWB housBillLinkedToDeclaration = masterBill.ChildBills.AddNew();
			housBillLinkedToDeclaration.CS_HAWB = "HAWB3";
			housBillLinkedToDeclaration.CS_RL_NKDestination = "AUSYD";
			housBillLinkedToDeclaration.CS_JE_CustomsFormalEntry = declaration.PK;
			CusHAWB housBillNotAustralianDestination = masterBill.ChildBills.AddNew();
			housBillNotAustralianDestination.CS_HAWB = "HAWB4";
			housBillNotAustralianDestination.CS_RL_NKDestination = "USLAX";
			housBillNotAustralianDestination.CS_GoodsValue = 1000m;
			housBillNotAustralianDestination.CS_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			CusHAWB housBillHavingSameInfoAsExistingDec = masterBill.ChildBills.AddNew();
			housBillHavingSameInfoAsExistingDec.CS_HAWB = "DECLHAWB";
			housBillHavingSameInfoAsExistingDec.CS_RL_NKDestination = "AUSYD";
			TaxOrFeeTestHelper.SetUp();
			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);
			housBillHavingSameInfoAsExistingDec.CS_GoodsValue = deminimus + 100m;
			housBillHavingSameInfoAsExistingDec.CS_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			CusHAWB sACHouseBill = masterBill.ChildBills.AddNew();
			sACHouseBill.CS_HAWB = "HAWB5";
			sACHouseBill.CS_RL_NKDestination = "AUSYD";
			sACHouseBill.CS_IsSelfAssessedClearance = true;
			Factory.Save();
			#endregion
			int declarationCount = Factory.GetDatabaseCount(typeof(JobDeclaration));
			DeclarationsFromCusMAWBCreator creator = new DeclarationsFromCusMAWBCreator(masterBill);
			NotificationBuffer buffer = new NotificationBuffer();
			try
			{
				creator.Progress += new TNTProgressEventHandler(Creator_Progress);
				Creator_ProgressCalled = false;
				creator.CreateDeclarations(buffer);
				AssertEquals("Buffer should not have error - Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
				AssertEquals("Creator_ProgressCalled should have been called", true, Creator_ProgressCalled);
				AssertEquals("1 new declaration should have beeen created", declarationCount + 1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
				AssertEquals("NormalHouseBill should be linked to a Declaration", true, normalHouseBill.CS_JE_CustomsFormalEntry != ZGuid.Empty);
				string expectedMessage = string.Format(" created for {0}", normalHouseBill.UnderbondHumanReadableName);
				AssertEquals(string.Format("Buffer should contain message '{0}'; Buffer contains:{1}{2}", expectedMessage, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(expectedMessage) >= 0);
				AssertEquals("HousBillLinkedToShipment should not be linked to a Declaration", ZGuid.Empty, housBillLinkedToShipment.CS_JE_CustomsFormalEntry);
				expectedMessage = string.Format("{0} is already linked to a Shipment", housBillLinkedToShipment.UnderbondHumanReadableName);
				AssertEquals(string.Format("Buffer should contain message '{0}'; Buffer contains:{1}{2}", expectedMessage, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(expectedMessage) >= 0);
				AssertEquals("HousBillLinkedToDeclaration should not have been relinked to another Declaration", declaration.PK, housBillLinkedToDeclaration.CS_JE_CustomsFormalEntry);
				expectedMessage = string.Format("{0} is already linked to a Declaration", housBillLinkedToDeclaration.UnderbondHumanReadableName);
				AssertEquals(string.Format("Buffer should contain message '{0}'; Buffer contains:{1}{2}", expectedMessage, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(expectedMessage) >= 0);
				AssertEquals("HousBillNotAustralianDestination should not be linked to a Declaration", ZGuid.Empty, housBillNotAustralianDestination.CS_JE_CustomsFormalEntry);
				expectedMessage = string.Format("{0} is a transhipment; Declaration will not be created for it", housBillNotAustralianDestination.UnderbondHumanReadableName);
				AssertEquals(string.Format("Buffer should contain message '{0}'; Buffer contains:{1}{2}", expectedMessage, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(expectedMessage) >= 0);
				AssertEquals("HousBillHavingSameInfoAsExistingDec should not be linked to a Declaration", ZGuid.Empty, housBillHavingSameInfoAsExistingDec.CS_JE_CustomsFormalEntry);
				expectedMessage = string.Format("A Declaration with the same Housebill number is already exists; No new Declaration will be created for {0}", housBillHavingSameInfoAsExistingDec.UnderbondHumanReadableName);
				AssertEquals(string.Format("Buffer should contain message '{0}'; Buffer contains:{1}{2}", expectedMessage, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(expectedMessage) >= 0);
				AssertEquals("SACHouseBill should not be linked to a Declaration", ZGuid.Empty, sACHouseBill.CS_JE_CustomsFormalEntry);
				expectedMessage = string.Format("No Declaration created for {0} as it is a SAC", sACHouseBill.UnderbondHumanReadableName);
				AssertEquals(string.Format("Buffer should contain message '{0}'; Buffer contains:{1}{2}", expectedMessage, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(expectedMessage) >= 0);
			}
			finally
			{
				creator.Progress -= new TNTProgressEventHandler(Creator_Progress);
			}
		}

		#region Creator_Progress
		void Creator_Progress(object sender, TNTProgressEventArgs e)
		{
			Creator_ProgressCalled = true;
		}

		bool Creator_ProgressCalled;
		#endregion
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeclarationsFromCusMAWBCreator(Factory.NewWithValidTestData<CusMAWB>());
		}
	}
}
