using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPEJobDeclarationRatingAdapterTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		#region IAutoRating / IAutoRatingFreightInfo
		public void TestChargeCodeGroups()
		{
			var jobDec = Factory.New<UPEJobDeclaration>();
			var chargeCodeGroups = jobDec.RatingAdapter.ChargeCodeGroups;
			Assert(chargeCodeGroups.Contains(ChargeCodeGroupList.Codes.Freight));
		}

		public void TestServiceLevel()
		{
			var upeCusHAWB = Factory.New<UPECusHAWB>();
			var jobDec = Factory.New<UPEJobDeclaration>();
			upeCusHAWB.CS_JE_CustomsFormalEntry = jobDec.PK;
			jobDec.JE_RS_NKServiceLevel = "1";
			upeCusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Letter;
			AssertEquals(UPERatingConstants.ServiceLevels.Envelopes, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			jobDec.JE_RS_NKServiceLevel = "1";
			upeCusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			AssertEquals(UPERatingConstants.ServiceLevels.Documents, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			jobDec.JE_RS_NKServiceLevel = "1";
			upeCusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			AssertEquals(UPERatingConstants.ServiceLevels.ExpressPackages, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			jobDec.JE_RS_NKServiceLevel = "5";
			AssertEquals(UPERatingConstants.ServiceLevels.ExpeditedPackages, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			jobDec.JE_RS_NKServiceLevel = "21";
			upeCusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Letter;
			AssertEquals(UPERatingConstants.ServiceLevels.Envelopes, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			jobDec.JE_RS_NKServiceLevel = "21";
			upeCusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			AssertEquals(UPERatingConstants.ServiceLevels.Documents, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			jobDec.JE_RS_NKServiceLevel = "21";
			upeCusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			AssertEquals(UPERatingConstants.ServiceLevels.ExpressPackages, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			jobDec.JE_RS_NKServiceLevel = "28";
			AssertEquals(UPERatingConstants.ServiceLevels.ExpressSaver, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
		}

		public void TestServiceLevelWithoutCusHAWB_NoNullReferenceExcpetion()
		{
			var jobDec = Factory.New<UPEJobDeclaration>();
			jobDec.JE_RS_NKServiceLevel = "1";
			AssertEquals(UPERatingConstants.ServiceLevels.ExpressPackages, jobDec.RatingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
		}
		#endregion
	}
}
