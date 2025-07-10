using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(RemarksInfoCollection))]
sealed class RemarksInfoCollectionTest : CusSupportingInfoCollectionTest<CusSupportingInfo>
{
	public void TestCheckDefaultCSI_Type()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		var remarksInfoCollection = new RemarksInfoCollection(jobComInvoiceLine);
		var remarks = remarksInfoCollection.AddNew();
		AssertEquals("Default CSI_Type", "REM", remarks.CSI_Type);
	}

	public void TestCheckCSI_DescriptionMaxLength()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		var remarksInfoCollection = new RemarksInfoCollection(jobComInvoiceLine);
		var remarks = remarksInfoCollection.AddNew();
		AssertEquals("CSI_Description max length", 512, remarks.CSI_DescriptionInfo.MaxLength);
	}

	protected override CusSupportingInfoCollection<CusSupportingInfo> GetCusSupportingInfoCollection()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		return new RemarksInfoCollection(jobComInvoiceLine);
	}
}
