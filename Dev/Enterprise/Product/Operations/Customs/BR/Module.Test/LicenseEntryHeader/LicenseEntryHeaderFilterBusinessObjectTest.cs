using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LicenseEntryHeaderFilterBusinessObject))]
	class LicenseEntryHeaderFilterBusinessObjectTest : Customs.Module.Testing.EntryHeaderFilterBusinessObjectAbstractTest
	{
		public void TestFilterForMessageType()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var entry1Dec1 = dec1.CustomsEntryHeaders.AddNew();
			entry1Dec1.CH_MessageType = BRJobMessageTypeList.Codes.LPCO;

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entry1Dec2 = dec2.CustomsEntryHeaders.AddNew();
			entry1Dec2.CH_MessageType = MessageTypeList.Codes.CDE;

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entry1Dec3 = dec3.CustomsEntryHeaders.AddNew();
			entry1Dec3.CH_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry1Dec4 = dec4.CustomsEntryHeaders.AddNew();
			entry1Dec4.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			Assert(!entry1Dec1.MatchesFilter(filterBO.Filter));
			Assert(!entry1Dec2.MatchesFilter(filterBO.Filter));
			Assert(!entry1Dec3.MatchesFilter(filterBO.Filter));
			Assert(entry1Dec4.MatchesFilter(filterBO.Filter));
		}

		public void TestEntryNumberFiltersDescription()
		{
			AssertEquals(LicenseFilterStripBusinessObject.LicenseNumberText, filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.EntryNumber].MultilingualDescription);
		}

		public override void TestEntryStatusFilterDescription()
		{
			AssertEquals(LicenseFilterStripBusinessObject.LicenseStatusText, filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.EntryStatus].MultilingualDescription);
		}

		public override void TestReleaseDateFilterDescription()
		{
			AssertNull(filterBO[EntryHeaderFilterBusinessObject.Constants.ReleaseDate]);
		}

		public void TestAvailableFilters()
		{
			CombineAssertions(() =>
			{
				AssertNull(filterBO[Customs.GUI.InvoiceLineFilterConstants.ContainerNumber]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.OriginDestination]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.LoadingDischarge]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.Loading]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.Discharge]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.ExportDate]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.ReleaseDate]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.FinalDestinationETA]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.OriginETD]);
				AssertNull(filterBO[Customs.Module.DeclarationFilterConstants.NumberFilterTypes.HouseBill]);
				AssertNull(filterBO[Customs.Module.DeclarationFilterConstants.NumberFilterTypes.MasterBill]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.ReferenceNumber]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.WarehouseTransactionStatus]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.CustomsAgent]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.Declarant]);
				AssertNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.DeclarationType]);
				AssertNull(filterBO[Customs.Module.DeclarationFilterConstants.FlightVoyageVessel]);
				AssertNotNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.EntryNumber]);
				AssertNotNull(filterBO[Customs.Module.EntryHeaderFilterBusinessObject.Constants.EntryStatus]);
			});
		}

		public override void TestHouseBillFilterDescription()
		{
			AssertNull(filterBO[Customs.Module.DeclarationFilterConstants.NumberFilterTypes.HouseBill]);
		}

		public override void TestMasterBillFilterDescription()
		{
			AssertNull(filterBO[Customs.Module.DeclarationFilterConstants.NumberFilterTypes.MasterBill]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new LicenseEntryHeaderFilterBusinessObject();

		LicenseEntryHeaderFilterBusinessObject filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (LicenseEntryHeaderFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
