using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

namespace Enterprise.Customs.IT.Messaging.SAD.Testing;

sealed class SadPositiveResponseMessageTest : TestCaseWithFactory
{
	public void TestFullRegistrationInfo()
	{
		var responseMessage = new SadPositiveResponseMessageForTest
		{
			RegisterCode = "4",
			RegisterSeries = "T",
			RegistrationNumber = "00061689",
			RegistrationNumberCin = "G",
			RegistrationDate = ZDate.Today,
		};

		AssertEquals("FullRegistrationInfo", $"4-T-00061689-G-{ZDate.Today.ToString("dd/MM/yyyy")}", responseMessage.FullRegistrationInfo);
	}

	public void TestRegistrationInfo()
	{
		var responseMessage = new SadPositiveResponseMessageForTest
		{
			RegisterCode = "4",
			RegisterSeries = "T",
			RegistrationNumber = "00061689",
			RegistrationNumberCin = "G",
		};

		AssertEquals("RegistrationInfo", "4 T-61689G", responseMessage.RegistrationInfo);
	}

	public void TestIsCleared()
	{
		var responseMessage = new SadPositiveResponseMessageForTest();

		responseMessage.ReleaseNotes = "";
		Assert("IsCleared", !responseMessage.IsCleared);

		responseMessage.ReleaseNotes = "SVINCOLATA";
		Assert("IsCleared", responseMessage.IsCleared);
	}

	public void TestIsUnderControl()
	{
		var responseMessage = new SadPositiveResponseMessageForTest();

		responseMessage.ReleaseNotes = "";
		Assert("IsUnderControl", !responseMessage.IsUnderControl);

		responseMessage.ReleaseNotes = "NON SVINCOLABILE";
		Assert("IsUnderControl", responseMessage.IsUnderControl);
	}

	public void TestIsAwaitingResponse()
	{
		var responseMessage = new SadPositiveResponseMessageForTest();

		responseMessage.ReleaseNotes = "";
		Assert("IsAwaitingResponse", !responseMessage.IsAwaitingResponse);

		responseMessage.ReleaseNotes = "IN ATTESA DI ESITO";
		Assert("IsAwaitingResponse", responseMessage.IsAwaitingResponse);
	}
	public void TestIsRegistered()
	{
		var responseMessage = new SadPositiveResponseMessageForTest();
		Assert("IsRegistered", responseMessage.IsRegistered);
	}

	public void TestHasA93FirstPayment()
	{
		var responseMessage = new SadPositiveResponseMessageForTest();

		responseMessage.A93Number = "12345";
		responseMessage.FirstPaymentMethod = "G";
		responseMessage.FirstPaymentDueDate = ZDate.Today;
		Assert("HasA93FirstPayment", responseMessage.HasA93FirstPayment);

		responseMessage.A93Number = "";
		Assert("HasA93FirstPayment", !responseMessage.HasA93FirstPayment);

		responseMessage.A93Number = "1234";
		responseMessage.FirstPaymentMethod = "";
		Assert("HasA93FirstPayment", !responseMessage.HasA93FirstPayment);

		responseMessage.FirstPaymentMethod = "G";
		responseMessage.FirstPaymentDueDate = ZDate.Empty;
		Assert("HasA93FirstPayment", !responseMessage.HasA93FirstPayment);
	}

	public void TestHasA93SecondPayment()
	{
		var responseMessage = new SadPositiveResponseMessageForTest();

		responseMessage.A93Number = "12345";
		responseMessage.SecondPaymentMethod = "G";
		responseMessage.SecondPaymentDueDate = ZDate.Today;
		Assert("HasA93SecondPayment", responseMessage.HasA93SecondPayment);

		responseMessage.A93Number = "";
		Assert("HasA93SecondPayment", !responseMessage.HasA93SecondPayment);

		responseMessage.A93Number = "1234";
		responseMessage.SecondPaymentMethod = "";
		Assert("HasA93SecondPayment", !responseMessage.HasA93SecondPayment);

		responseMessage.SecondPaymentMethod = "G";
		responseMessage.SecondPaymentDueDate = ZDate.Empty;
		Assert("HasA93SecondPayment", !responseMessage.HasA93SecondPayment);
	}

	public void TestHasA93ThirdPayment()
	{
		var responseMessage = new SadPositiveResponseMessageForTest();

		responseMessage.A93Number = "12345";
		responseMessage.ThirdPaymentMethod = "G";
		responseMessage.ThirdPaymentDueDate = ZDate.Today;
		Assert("HasA93ThirdPayment", responseMessage.HasA93ThirdPayment);

		responseMessage.A93Number = "";
		Assert("HasA93ThirdPayment", !responseMessage.HasA93ThirdPayment);

		responseMessage.A93Number = "1234";
		responseMessage.ThirdPaymentMethod = "";
		Assert("HasA93ThirdPayment", !responseMessage.HasA93ThirdPayment);

		responseMessage.ThirdPaymentMethod = "G";
		responseMessage.ThirdPaymentDueDate = ZDate.Empty;
		Assert("HasA93ThirdPayment", !responseMessage.HasA93ThirdPayment);
	}
}

internal class SadPositiveResponseMessageForTest : SadPositiveResponseMessage
{
	public new ZString RegisterCode
	{
		get => base.RegisterCode;
		set => base.RegisterCode = value;
	}

	public new ZString RegisterSeries
	{
		get => base.RegisterSeries;
		set => base.RegisterSeries = value;
	}

	public new ZString RegistrationNumber
	{
		get => base.RegistrationNumber;
		set => base.RegistrationNumber = value;
	}

	public new ZString RegistrationNumberCin
	{
		get => base.RegistrationNumberCin;
		set => base.RegistrationNumberCin = value;
	}

	public new ZDate RegistrationDate
	{
		get => base.RegistrationDate;
		set => base.RegistrationDate = value;
	}

	public new ZString ReleaseNotes
	{
		get => base.ReleaseNotes;
		set => base.ReleaseNotes = value;
	}

	public new ZString OperationResult
	{
		get => base.OperationResult;
		set => base.OperationResult = value;
	}

	public new ZString A93Number
	{
		get => base.A93Number;
		set => base.A93Number = value;
	}

	public new ZString FirstPaymentMethod
	{
		get => base.FirstPaymentMethod;
		set => base.FirstPaymentMethod = value;
	}

	public new ZDate FirstPaymentDueDate
	{
		get => base.FirstPaymentDueDate;
		set => base.FirstPaymentDueDate = value;
	}
	public new ZString SecondPaymentMethod
	{
		get => base.SecondPaymentMethod;
		set => base.SecondPaymentMethod = value;
	}
	public new ZDate SecondPaymentDueDate
	{
		get => base.SecondPaymentDueDate;
		set => base.SecondPaymentDueDate = value;
	}
	public new ZString ThirdPaymentMethod
	{
		get => base.ThirdPaymentMethod;
		set => base.ThirdPaymentMethod = value;
	}
	public new ZDate ThirdPaymentDueDate
	{
		get => base.ThirdPaymentDueDate;
		set => base.ThirdPaymentDueDate = value;
	}
}
