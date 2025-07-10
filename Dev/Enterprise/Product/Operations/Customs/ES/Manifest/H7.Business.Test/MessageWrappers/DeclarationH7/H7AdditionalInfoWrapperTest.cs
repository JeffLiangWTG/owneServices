using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class H7AdditionalInfoWrapperTest : DataProviderTestCase<H7AdditionalInfoWrapper>
{
	public void TestCode()
	{
		doc.CSI_Code = "Code";
		AssertEquals("Code", wrapper.Code);
	}

	public void TestDescription()
	{
		doc.CSI_Description = "Description";
		AssertEquals("Description", wrapper.Description);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		var bill = header.Bills.AddNew();
		doc = bill.AdditionalDocuments.AddNew();

		wrapper = new H7AdditionalInfoWrapper(doc);
	}

	protected override H7AdditionalInfoWrapper GetProvider()
	{
		return wrapper;
	}

	H7AdditionalInfoWrapper wrapper;
	EU.H7.Business.AdditionalDocument doc;
}
