using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7MessageWrapper))]
public sealed class H7MessageWrapperTest : DataProviderTestCase<H7MessageWrapper>
{
	public void TestHeader()
	{
		SetUpTests();

		var wrapper = new H7MessageWrapper(bill) as IH7Message;
		AssertNotNull(wrapper.Header);
		AssertType<H7HeaderWrapper>(wrapper.Header);
	}

	public void TestItems()
	{
		SetUpTests();

		var wrapper = new H7MessageWrapper(bill) as IH7Message;
		AssertType<ReadOnlyCollection<IH7Item>>(wrapper.Items);
		AssertNotNull(wrapper.Items);
		AssertEquals(1, wrapper.Items.Count);
		AssertType<H7ItemWrapper>(wrapper.Items.First());
	}

	void SetUpTests()
	{
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		bill.PackedItems.AddNew();
	}

	protected override H7MessageWrapper GetProvider()
	{
		var pHeader = Factory.New<AsycudaManifestHeader>();
		var pBill = pHeader.Bills.AddNew();
		var pItem = pBill.PackedItems.AddNew();
		return new H7MessageWrapper(pBill);
	}

	AsycudaManifestHeader header;
	AsycudaBill bill;
}
