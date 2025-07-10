using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

class DangerousGoodsWrapperTest : DataProviderTestCase<DangerousGoodsWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DangerousGoodsWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestUndgid()
	{
		AssertEquals("UNDGID", undg.Substance.DG_UNNO, wrapper.Undgid);
	}

	protected override DangerousGoodsWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		invLine = Factory.New<JobComInvoiceLine>();
		substance = Factory.New<UNDGSubstance>();
		undg = invLine.UNDGs.AddNew();
		substance.DG_UNNO = "1001";
		undg.DI_DG = substance.PK;
		wrapper = new DangerousGoodsWrapper(undg, 1);
	}
	UNDGSubstance substance;
	JobComInvoiceLine invLine;
	UNDGDataItem undg;
	DangerousGoodsWrapper wrapper;
}
