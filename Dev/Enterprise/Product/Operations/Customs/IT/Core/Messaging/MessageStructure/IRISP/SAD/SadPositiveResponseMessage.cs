using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public interface ISadPositiveResponseMessageA93EntryPayments
{
	ZString A93Number { get; }

	ZBool HasA93FirstPayment { get; }
	ZString FirstPaymentMethod { get; }
	ZDate FirstPaymentDueDate { get; }

	ZBool HasA93SecondPayment { get; }
	ZString SecondPaymentMethod { get; }
	ZDate SecondPaymentDueDate { get; }

	ZBool HasA93ThirdPayment { get; }
	ZString ThirdPaymentMethod { get; }
	ZDate ThirdPaymentDueDate { get; }
}

public class SadPositiveResponseMessage : UnifiedDeclarationPositiveResponseMessage, ISadPositiveResponseMessageA93EntryPayments
{
	public ZString RegisterCode { get; protected set; }
	public ZString RegisterSeries { get; protected set; }
	public ZString RegistrationNumber { get; protected set; }
	public ZString RegistrationNumberCin { get; protected set; }
	public ZDate RegistrationDate { get; protected set; }
	public ZString DebitAccountNumber { get; protected set; }
	public ZString DebitAccountNumberCin { get; protected set; }
	public ZString A93Number { get; protected set; }
	public ZString FirstPaymentMethod { get; protected set; }
	public ZDate FirstPaymentDueDate { get; protected set; }
	public ZString SecondPaymentMethod { get; protected set; }
	public ZDate SecondPaymentDueDate { get; protected set; }
	public ZString ThirdPaymentMethod { get; protected set; }
	public ZDate ThirdPaymentDueDate { get; protected set; }
	public ZString ReleaseCode { get; protected set; }
	public ZString ReleaseNotes { get; protected set; }
	public ZString MrnCode { get; protected set; }
	public ZDecimal? GuaranteeAmount { get; protected set; }
	public ZString Notes { get; protected set; }

	public ZString FullRegistrationInfo => FormattableString.Invariant($"{RegisterCode}-{RegisterSeries}-{RegistrationNumber}-{RegistrationNumberCin}-{RegistrationDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}");
	public ZString RegistrationInfo => FormattableString.Invariant($"{RegisterCode.PadRight(2, ' ')}{RegisterSeries}-{RegistrationNumber.TrimStart('0')}{RegistrationNumberCin}");
	public ZBool IsCleared => ReleaseNotes == ClereanceReleaseNote;
	public ZBool IsUnderControl => ReleaseNotes == UnderControlReleaseNote;
	public ZBool IsAwaitingResponse => ReleaseNotes == AwaitReponseReleaseNote;
	public ZBool IsRegistered => ZBool.True;
	public ZBool HasA93FirstPayment => !A93Number.IsEmpty && !FirstPaymentMethod.IsEmpty && !FirstPaymentDueDate.IsEmpty;
	public ZBool HasA93SecondPayment => !A93Number.IsEmpty && !SecondPaymentMethod.IsEmpty && !SecondPaymentDueDate.IsEmpty;
	public ZBool HasA93ThirdPayment => !A93Number.IsEmpty && !ThirdPaymentMethod.IsEmpty && !ThirdPaymentDueDate.IsEmpty;

	internal override void Load(ZString content)
	{
		const int recordTotalLength = 196;
		base.Load(content);
		var line = Lines.ElementAt(1).PadRight(recordTotalLength, ' ');
		RegisterCode = line.SubStringAndTrim(28, 2);
		RegisterSeries = line.SubStringAndTrim(30, 2);
		RegistrationNumber = line.SubStringAndTrim(32, 8);
		RegistrationNumberCin = line.SubStringAndTrim(40, 1);
		RegistrationDate = GetDateOrDefaultEmpty(line.SubStringAndTrim(41, 6));
		DebitAccountNumber = GetStringOrDefaultEmpty(line.SubStringAndTrim(47, 6));
		DebitAccountNumberCin = line.SubStringAndTrim(53, 1);
		A93Number = GetStringOrDefaultEmpty(line.SubStringAndTrim(54, 6));
		FirstPaymentMethod = GetStringOrDefaultEmpty(line.SubStringAndTrim(60, 1));
		FirstPaymentDueDate = GetDateOrDefaultEmpty(line.SubStringAndTrim(61, 6));
		SecondPaymentMethod = GetStringOrDefaultEmpty(line.SubStringAndTrim(67, 1));
		SecondPaymentDueDate = GetDateOrDefaultEmpty(line.SubStringAndTrim(68, 6));
		ThirdPaymentMethod = GetStringOrDefaultEmpty(line.SubStringAndTrim(74, 1));
		ThirdPaymentDueDate = GetDateOrDefaultEmpty(line.SubStringAndTrim(75, 6));
		ReleaseCode = line.SubStringAndTrim(81, 6);
		ReleaseNotes = line.SubStringAndTrim(87, 25);
		MrnCode = line.SubStringAndTrim(112, 18);
		var guaranteeAmountString = line.SubStringAndTrim(130, 15);
		GuaranteeAmount = !guaranteeAmountString.IsEmpty ? ZDecimal.Parse(guaranteeAmountString, System.Globalization.NumberFormatInfo.InvariantInfo) : null;
		Notes = line.SubStringAndTrim(145, 50);
	}

	const string _000000 = "000000";

	ZDate GetDateOrDefaultEmpty(ZString dateString) => dateString != _000000 ? (ZDate)dateString.ParseToDateTimeWithFormat("ddMMyy") : ZDate.Empty;

	ZString GetStringOrDefaultEmpty(ZString value) => value != _000000 ? value : ZString.Empty;

	#region SuppressResourceStringsCheckRegion

	public const string ClereanceReleaseNote = "SVINCOLATA";
	public const string UnderControlReleaseNote = "NON SVINCOLABILE";
	public const string AwaitReponseReleaseNote = "IN ATTESA DI ESITO";

	#endregion
}
