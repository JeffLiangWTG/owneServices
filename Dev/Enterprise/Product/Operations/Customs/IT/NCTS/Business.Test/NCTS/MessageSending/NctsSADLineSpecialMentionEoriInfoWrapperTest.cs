using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADLineSpecialMentionEoriInfoWrapperTest : TestCaseWithFactory
{
	public void TestFirstEoriCode()
	{
		AssertEquals(nameof(wrapper.FirstEoriCode), ZString.Empty, wrapper.FirstEoriCode);
	}

	public void TestSecondEoriCode()
	{
		AssertEquals(nameof(wrapper.SecondEoriCode), ZString.Empty, wrapper.SecondEoriCode);
	}

	public void TestPreviousInvoiceAmount()
	{
		AssertNull(nameof(wrapper.PreviousInvoiceAmount), wrapper.PreviousInvoiceAmount);
	}

	protected override void SetUp()
	{
		base.SetUp();
		wrapper = new NctsSADLineSpecialMentionEoriInfoWrapper();
	}
	NctsSADLineSpecialMentionEoriInfoWrapper wrapper;
}
