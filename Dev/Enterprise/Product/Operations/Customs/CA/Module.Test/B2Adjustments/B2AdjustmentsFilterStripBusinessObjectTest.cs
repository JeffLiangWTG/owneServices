using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(B2AdjustmentsFilterStripBusinessObject))]
	sealed class B2AdjustmentsFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDefaultQuery()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var dec1 = Factory.New<JobDeclaration>();
				dec1.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				dec1.CA_B2Type = B2TypeList.Codes.Specific;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				var header = declaration.Invoices.AddNew();
				header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
				var line = header.AsAccountForFilteredInvoiceLines.AddNew();
				line.JI_CustomsQuantity = 5m;
				declaration.DoMerge();
				Factory.Save();
				var im2 = declaration.GetNewCopyToB2Declaration();
				im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				im2.ApportionmentDirty = false;
				im2.DoMerge();
				im2.CA_B2Type = B2TypeList.Codes.Specific;

				declaration.CA_CSAEntry = true;
				var b3x = declaration.GetNewCopyToB2Declaration();
				b3x.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				b3x.ApportionmentDirty = false;
				b3x.DoMerge();
				b3x.CA_B2Type = B2TypeList.Codes.Specific;

				Factory.Save();

				var filterObj = new B2AdjustmentsFilterStripBusinessObject();
				var filter = filterObj[DeclarationFilterConstants.B2Type] as ModuleTextFilter;
				filter.Property = B2TypeList.Codes.Specific;
				filter.IsActive = true;

				Assert(dec1.MatchesFilter(filterObj.Filter));
				Assert(im2.MatchesFilter(filterObj.Filter));
				Assert(b3x.MatchesFilter(filterObj.Filter));
				AssertEquals(CAAddInfoSchema.CA_B2Type.MaxLength, filter.MaxLength);

				var transactionFilter = filterObj[DeclarationFilterConstants.NumberFilterTypes.TransactionNumber] as ModuleTextFilter;
				AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength), transactionFilter.MaxLength);

				var portOfClearanceFilter = filterObj[DeclarationFilterConstants.PortFilterTypes.PortOfClearance] as ModuleNkFilter;
				AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, portOfClearanceFilter.ModuleId);
			}
		}

		public void TestB2TypeQuery()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			dec1.CA_B2Type = B2TypeList.Codes.Blanket;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			dec2.CA_B2Type = B2TypeList.Codes.Specific;

			Factory.Save();

			var filterObj = new B2AdjustmentsFilterStripBusinessObject();
			var filter = filterObj[DeclarationFilterConstants.B2Type] as ModuleTextFilter;
			filter.Property = B2TypeList.Codes.Specific;
			filter.IsActive = true;

			Assert(!dec1.MatchesFilter(filterObj.Filter));
			Assert(dec2.MatchesFilter(filterObj.Filter));
		}

		public void TestOriginalTransactionNo()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			dec1.CA_OriginalTransactionNo = "123456789";
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			dec2.CA_OriginalTransactionNo = "987654321";

			var filterObj = new B2AdjustmentsFilterStripBusinessObject();
			var filter = filterObj[DeclarationFilterConstants.NumberFilterTypes.OriginalTransactionNo] as ModuleTextFilter;
			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			Factory.Save();

			Assert(dec1.MatchesFilter(filterObj.Filter));
			Assert(!dec2.MatchesFilter(filterObj.Filter));
			AssertEquals(CAAddInfoSchema.CA_OriginalTransactionNo.MaxLength, filter.MaxLength);
		}

		public void TestDatesFilterQuery()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.CA_B2SubmissionDate = new ZDateTime(2016, 10, 12);

			var filterObj = new B2AdjustmentsFilterStripBusinessObject();
			var filter = filterObj[DeclarationFilterConstants.DateFilterTypes.SubmittedDate] as ModuleDateFilter;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 10, 12);
			filter.Property2 = new ZDateTime(2016, 10, 12);
			filter.IsActive = true;
			Factory.Save();
			Assert(declaration.MatchesFilter(filterObj.Filter));

			var filter2 = filterObj[DeclarationFilterConstants.DateFilterTypes.ConfirmedDate] as ModuleDateFilter;
			filter2.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter2.Property1 = new ZDateTime(2016, 10, 13);
			filter2.Property2 = new ZDateTime(2016, 10, 13);
			filter2.IsActive = true;

			Assert(!declaration.MatchesFilter(filterObj.Filter));
			declaration.CA_ConfirmedDate = new ZDateTime(2016, 10, 13);
			Factory.Save();
			Assert(declaration.MatchesFilter(filterObj.Filter));

			var filter3 = filterObj[DeclarationFilterConstants.DateFilterTypes.DecisionDate] as ModuleDateFilter;
			filter3.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter3.Property1 = new ZDateTime(2016, 10, 14);
			filter3.Property2 = new ZDateTime(2016, 10, 14);
			filter3.IsActive = true;

			Assert(!declaration.MatchesFilter(filterObj.Filter));
			declaration.CA_B2AcceptedDate = new ZDateTime(2016, 10, 14);
			Factory.Save();
			Assert(declaration.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new B2AdjustmentsFilterStripBusinessObject();
	}
}
