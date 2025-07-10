using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(DepartureCusTransportMeans))]
	sealed class DepartureCusTransportMeansTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Departure Customs Office Transport", departureCusTransportMeans.HumanReadableName);
		}

		public void TestCustomsOfficeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType("ROLE", "ROLE");

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOFF1", "DESCRIPTION1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "DES");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOFF2", "DESCRIPTION2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "TRA");
			Factory.Save();

			CombineAssertions(() =>
			{
				departureCusTransportMeans.TPM_CustomsOffice = "CUSOFF1";
				AssertEquals("CUSOFF1", "DESCRIPTION1", departureCusTransportMeans.CustomsOfficeDescription);

				departureCusTransportMeans.TPM_CustomsOffice = "CUSOFF2";
				AssertEquals("CUSOFF2", "DESCRIPTION2", departureCusTransportMeans.CustomsOfficeDescription);
			});
		}

		public void TestParent_Getter()
		{
			var movementHeader = nctsHeader.MovementHeader;
			var cusTransportMeans = Factory.New<DepartureCusTransportMeans>();
			CombineAssertions(() =>
			{
				AssertNull("TPM_ParentID and TPM_ParentTableCode not set", cusTransportMeans.Parent);

				cusTransportMeans.TPM_ParentID = movementHeader.PK;
				cusTransportMeans.TPM_ParentTableCode = movementHeader.TablePrefix;
				AssertSame("Child CusTransportMeans of NctsMovementHeader", movementHeader, cusTransportMeans.Parent);

				var bill = nctsHeader.Bills.AddNew();
				cusTransportMeans.TPM_ParentID = bill.PK;
				cusTransportMeans.TPM_ParentTableCode = bill.TablePrefix;
				AssertSame("Child CusTransportMeans of NctsBill", bill, cusTransportMeans.Parent);
			});
		}

		public void TestHeader()
		{
			var movementHeader = nctsHeader.MovementHeader;
			var cusTransportMeans = Factory.New<DepartureCusTransportMeans>();
			CombineAssertions(() =>
			{
				AssertNull("TPM_ParentID and TPM_ParentTableCode not set", cusTransportMeans.Header);

				cusTransportMeans.TPM_ParentID = movementHeader.PK;
				cusTransportMeans.TPM_ParentTableCode = movementHeader.TablePrefix;
				AssertSame("Child CusTransportMeans of NctsMovementHeader", nctsHeader, cusTransportMeans.Header);

				var bill = nctsHeader.Bills.AddNew();
				cusTransportMeans.TPM_ParentID = bill.PK;
				cusTransportMeans.TPM_ParentTableCode = bill.TablePrefix;
				AssertSame("Child CusTransportMeans of NctsBill", nctsHeader, cusTransportMeans.Header);
			});
		}

		public void TestMovementHeaderParent()
		{
			CombineAssertions(() =>
			{
				AssertSame("Child CusTransportMeans of NctsMovementHeader", nctsHeader.MovementHeader, departureCusTransportMeans.MovementHeaderParent);

				var cusTransportMeans2 = nctsHeader.Bills.AddNew().DepartureTransportInfos.AddNew();
				AssertNull("Child CusTransportMeans of NctsBill", cusTransportMeans2.MovementHeaderParent);
			});
		}

		public void TestTPM_CustomsOffice_Caption()
		{
			NCTSTestHelper.AssertCaptions(departureCusTransportMeans.TPM_CustomsOfficeInfo, NctsHeader.Phase5CaptionKey, "Customs Office", string.Empty, "Office");
		}

		public void TestTPM_ReferenceNumber()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(departureCusTransportMeans.TPM_ReferenceNumberInfo, NctsHeader.Phase5CaptionKey, "Conveyance Number", "Conveyance No.", "Conv. No.", "Conveyance Reference Number");
			AssertEquals("MaxLength", 17, departureCusTransportMeans.TPM_ReferenceNumberInfo.MaxLength);
		}

		public void TestTPM_TypeOfIdentification_Caption()
		{
			NCTSTestHelper.AssertCaptions(departureCusTransportMeans.TPM_TypeOfIdentificationInfo, NctsHeader.Phase5CaptionKey, "Type of Identification", "Type of ID", "Type");
		}

		public void TestTPM_IdentificationNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(departureCusTransportMeans.TPM_IdentificationNumberInfo, NctsHeader.Phase5CaptionKey, "Transport Identification", "Transport ID", "Transp. ID");
		}

		public void TestTPM_RN_NKTransportNationality_Caption()
		{
			NCTSTestHelper.AssertCaptions(departureCusTransportMeans.TPM_RN_NKTransportNationalityInfo, NctsHeader.Phase5CaptionKey, "Nationality", string.Empty, "Nat.");
		}
		public void TestIsTransportBorder()
		{
			var bill = nctsHeader.Bills.AddNew();
			var departureTransportMeans = bill.DepartureTransportInfos.AddNew();
			var borderTransportMeans = nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();
			AssertEquals("Departure", false, departureTransportMeans.IsTransportBorder);
			AssertEquals("Border", true, borderTransportMeans.IsTransportBorder);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => departureCusTransportMeans;

		protected override BusinessObject GetNewBusinessObject() => departureCusTransportMeans;

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureCusTransportMeans = nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();
		}
		DepartureCusTransportMeans departureCusTransportMeans;
		NctsHeader nctsHeader;
	}
}
