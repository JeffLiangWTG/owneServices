using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(JobRequiredDocumentAddInfoFilterBusinessObject))]
	sealed class JobRequiredDocumentAddInfoFilterBusinessObjectTest : MasterFiles.Module.Testing.JobRequiredDocumentAddInfoFilterBusinessObjectTest
	{
		public void TestURNFilter()
		{
			var addInfo1 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo1.EX_ReferenceNumber = "REF1";
			addInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;

			var addInfo2 = Factory.NewWithValidTestData<JobRequiredDocumentAddInfo>();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			Factory.Save();

			var filterBO = new JobRequiredDocumentAddInfoFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[JobRequiredDocumentAddInfoFilterBusinessObject.Constants.URN];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "REF1";
			filter.IsActive = true;

			Assert(addInfo1.MatchesFilter(filterBO.Filter));
			Assert(!addInfo2.MatchesFilter(filterBO.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobRequiredDocumentAddInfoFilterBusinessObject();
	}
}
