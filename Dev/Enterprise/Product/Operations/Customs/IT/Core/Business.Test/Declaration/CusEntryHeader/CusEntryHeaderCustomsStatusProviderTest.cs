using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryHeaderCustomsStatusProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryHeaderCustomsStatusProvider(entryHeader: null));
	}

	public void TestAwaitingMessageStatus()
	{
		AssertEquals(nameof(provider.AwaitingMessageStatus), ITMessageStatusList.Codes.AwaitingOriginal, provider.AwaitingMessageStatus);
	}

	public void TestAcknowledgedMessageStatus()
	{
		AssertEquals(nameof(provider.AcknowledgedMessageStatus), ITMessageStatusList.Codes.AcknowledgedOriginal, provider.AcknowledgedMessageStatus);
	}

	public void TestClearedMessageStatus()
	{
		AssertEquals(nameof(provider.ClearedMessageStatus), ITMessageStatusList.Codes.ClearOriginal, provider.ClearedMessageStatus);
	}

	public void TestErrorMessageStatus()
	{
		AssertEquals(nameof(provider.ErrorMessageStatus), ITMessageStatusList.Codes.ErrorOriginal, provider.ErrorMessageStatus);
	}

	public void TestRegisteredCustomsStatus()
	{
		AssertEquals(nameof(provider.RegisteredCustomsStatus), ITEntryStatusList.Codes.Registered, provider.RegisteredCustomsStatus);
	}

	public void TestUnderControlCustomsStatus()
	{
		AssertEquals(nameof(provider.UnderControlCustomsStatus), ITEntryStatusList.Codes.UnderControl, provider.UnderControlCustomsStatus);
	}

	public void TestClearedCustomsStatus()
	{
		entryHeader.Declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		provider = new CusEntryHeaderCustomsStatusProvider(entryHeader);
		AssertEquals($"When entryHeader.IsImport = true, {nameof(provider.ClearedCustomsStatus)}", ITEntryStatusList.Codes.ImportCleared, provider.ClearedCustomsStatus);

		entryHeader.Declaration.JE_MessageType = ZString.Empty;
		provider = new CusEntryHeaderCustomsStatusProvider(entryHeader);
		AssertEquals($"When entryHeader.IsImport  = false, {nameof(provider.ClearedCustomsStatus)}", ITEntryStatusList.Codes.ExportCleared, provider.ClearedCustomsStatus);
	}

	public void TestNbRejectedCustomsStatus()
	{
		AssertEquals(nameof(provider.NbRejectedCustomsStatus), ITEntryStatusList.Codes.NbRejected, provider.NbRejectedCustomsStatus);
	}

	public void TestArrivalCustomsStatus()
	{
		AssertEquals(nameof(provider.ArrivalCustomsStatus), ITEntryStatusList.Codes.Arrival, provider.ArrivalCustomsStatus);
	}

	public void TestStatusWithInformationOrderCollection()
	{
		CombineAssertions("When entryHeader.IsImport = true", () =>
		{
			entryHeader.Declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			provider = new CusEntryHeaderCustomsStatusProvider(entryHeader);
			var expectedValues = new CustomsStatusOrder[]
			{
				new CustomsStatusOrder(ZString.Empty, 0),
				new CustomsStatusOrder(ITEntryStatusList.Codes.Registered, 1),
				new CustomsStatusOrder(ITEntryStatusList.Codes.NbRejected, 2),
				new CustomsStatusOrder(ITEntryStatusList.Codes.UnderControl, 3),
				new CustomsStatusOrder(ITEntryStatusList.Codes.ImportCleared, 5)
			};
			CustomsStatusOrderTestHelper.AssertCollection(expectedValues, provider.StatusWithInformationOrderCollection.ToArray());
		});

		CombineAssertions("When entryHeader.IsImport = false", () =>
		{
			entryHeader.Declaration.JE_MessageType = ZString.Empty;
			provider = new CusEntryHeaderCustomsStatusProvider(entryHeader);
			var expectedValues = new CustomsStatusOrder[]
			{
				new CustomsStatusOrder(ZString.Empty, 0),
				new CustomsStatusOrder(ITEntryStatusList.Codes.Registered, 1),
				new CustomsStatusOrder(ITEntryStatusList.Codes.NbRejected, 2),
				new CustomsStatusOrder(ITEntryStatusList.Codes.UnderControl, 3),
				new CustomsStatusOrder(ITEntryStatusList.Codes.ExportCleared, 4),
				new CustomsStatusOrder(ITEntryStatusList.Codes.Exit, 5),
				new CustomsStatusOrder(ITEntryStatusList.Codes.Arrival, 6)
			};
			CustomsStatusOrderTestHelper.AssertCollection(expectedValues, provider.StatusWithInformationOrderCollection.ToArray());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		provider = new CusEntryHeaderCustomsStatusProvider(entryHeader);
	}

	CusEntryHeader entryHeader;
	ISadCustomsStatusProvider provider;
}
