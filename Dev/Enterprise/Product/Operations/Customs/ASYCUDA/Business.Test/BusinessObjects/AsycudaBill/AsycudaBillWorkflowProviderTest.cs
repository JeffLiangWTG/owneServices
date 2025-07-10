using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillWorkflowProviderTest : WorkflowProviderTest<AsycudaBill, ProcessTaskCollection<AsycudaBillProcessTask, AsycudaBill>>
	{
		public void TestGetTemplateSelectionCriteria()
		{
			var header = Bill.Header;
			header.SuspendCheckBusinessObjectType();
			AssertGetTemplateFilterCriteria<ZString>(header.AMA_RN_NKCountryInfo, ProcessTaskTemplate.P0_SubType1Info, "SG", "ZA", ZString.Empty);
			AssertGetTemplateFilterCriteria<ZString>(header.AMA_ManifestTypeInfo, ProcessTaskTemplate.P0_SubType2Info, "MGI", "MGE", ZString.Empty);
			AssertGetTemplateFilterCriteria<ZString>(bill.ABL_ShipmentTypeInfo, ProcessTaskTemplate.P0_SubType3Info, "IMP", "EXP", ZString.Empty);

			var ranker = (ColumnValueRanker)((IWorkflowProvider)Bill).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { GlbBranch.CurrentBranch.PK, ZGuid.Empty }, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB));
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			Assert("Asycuda Bill doesn't support Tasks & Milestones.", true);
		}

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode;

		protected override AsycudaBill GetNewBusinessObject(BusinessObjectFactory factory) => Bill;

		AsycudaBill Bill
		{
			get
			{
				if (bill == null)
				{
					var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
					bill = header.Bills.AddNew();
				}
				return bill;
			}
		}
		AsycudaBill bill;
	}
}
