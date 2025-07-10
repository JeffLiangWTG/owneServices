namespace CargoWise.IO
{
	/// <summary>
	/// Contains the validity status of a counter value.
	/// </summary>
	public enum PdhCounterStatus : uint
	{
		CStatusNoMachine = 0x800007d0,
		CStatusNoObject = 0xc0000bb8,
		CStatusNoInstance = 0x800007d1,
		CStatusNoCounter = 0xc0000bb9,
		CStatusInvalidData = 0xc0000bba,
		CStatusValidData = 0x00000000,
		CStatusNewData = 0x00000001,
		MoreData = 0x800007d2,
		CStatusItemNotValidated = 0x800007d3,
		CStatusNoCountername = 0xc0000bbf,
		CalcNegativeDenominator = 0x800007d6,
		CalcNegativeTimebase = 0x800007d7,
		CalcNegativeValue = 0x800007d8,
		CStatusBadCountername = 0xc0000bc0,
	}
}
