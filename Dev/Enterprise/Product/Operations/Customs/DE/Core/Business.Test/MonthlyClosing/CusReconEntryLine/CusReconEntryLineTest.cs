using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusReconEntryLine))]
	class CusReconEntryLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCRL_LineNumber_ReadOnly()
		{
			AssertPropertyIsReadOnly(CusReconEntryLine.Schema.CRL_LineNumber);
		}

		public void TestCRL_OriginalEntryLineNumber_ReadOnly()
		{
			AssertPropertyIsReadOnly(CusReconEntryLine.Schema.CRL_OriginalEntryLineNumber);
		}

		public void TestCRL_Description_ReadOnly()
		{
			AssertPropertyIsReadOnly(CusReconEntryLine.Schema.CRL_Description);
		}

		public void TestCRL_CustomsStatus_ReadOnly()
		{
			AssertPropertyIsReadOnly(CusReconEntryLine.Schema.CRL_CustomsStatus);
		}

		public void TestCustomsStatusDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "RL1", "RL1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				reconEntryLine.CRL_CustomsStatus = "RL1";
				AssertEquals("CRL_CustomsStatus is valid", "RL1 DESC", reconEntryLine.CustomsStatusDescription);
				reconEntryLine.CRL_CustomsStatus = "XX";
				AssertEquals("CRL_CustomsStatus is invalid", ZString.Empty, reconEntryLine.CustomsStatusDescription);
			});
		}

		public void TestCustomsStatusDescription_Caption()
		{
			var propertyInfo = reconEntryLine.GetType().GetProperty(nameof(reconEntryLine.CustomsStatusDescription));
			AssertEquals("Status Description", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestCustomsStatusDescription_ReadOnly()
		{
			AssertPropertyIsReadOnly(nameof(CusReconEntryLine.CustomsStatusDescription));
		}

		public void TestCurrentSnapshot()
		{
			CombineAssertions(() =>
			{
				var snapshot = reconEntryLine.CusReconSnapshots.AddNew();
				snapshot.CRS_Type = CusReconConstants.Lodged;
				AssertNull("No CUR snapshots", reconEntryLine.CurrentSnapshot);
				snapshot.CRS_Type = CusReconConstants.Current;
				AssertSame("Has CUR snapshot", snapshot, reconEntryLine.CurrentSnapshot);
			});
		}

		public void TestLodgedSnapshot()
		{
			CombineAssertions(() =>
			{
				var snapshot = reconEntryLine.CusReconSnapshots.AddNew();
				snapshot.CRS_Type = CusReconConstants.Current;
				AssertNull("No LDG snapshots", reconEntryLine.LodgedSnapshot);
				snapshot.CRS_Type = CusReconConstants.Lodged;
				AssertSame("Has LDG snapshot", snapshot, reconEntryLine.LodgedSnapshot);
			});
		}

		public void TestReconEntry()
		{
			AssertSame(reconEntry, reconEntryLine.ReconEntry);
		}

		public void TestEntryLine()
		{
			AssertSame(entryLine, reconEntryLine.EntryLine);
			entryLine.CL_LineNumber = 1;
			AssertNull(reconEntryLine.EntryLine);
		}

		public void TestEntryLineHasChanges()
		{
			var snapshot = reconEntryLine.CusReconSnapshots.AddNew();
			snapshot.CRS_Type = CusReconConstants.Lodged;

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, reconEntryLine.EntryLineHasChanges);
				snapshot.CRS_Type = CusReconConstants.Current;
				AssertEquals("Yes", reconEntryLine.EntryLineHasChanges);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = factory.NewWithValidTestData<OrgAddress>().PK;
			var reconEntryLine = factory.New<CusReconEntryLine>();
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_CustomsStatus = "AA";
			reconEntryLine.CRL_Description = "AA";
			reconEntryLine.CRL_OriginalEntryLineNumber = 2;
			reconEntryLine.CRL_CRE = reconEntry.PK;
			return reconEntryLine;
		}

		protected override void SetUp()
		{
			base.SetUp();

			reconEntryLine = (CusReconEntryLine)GetNewBusinessObjectForDeleteTest(Factory);
		}
		CusReconEntry reconEntry;
		CusReconEntryLine reconEntryLine;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;

		void AssertPropertyIsReadOnly(string propertyName)
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(CusReconEntryLine), propertyName, false, a => a.IsReadOnly);
		}
	}
}
