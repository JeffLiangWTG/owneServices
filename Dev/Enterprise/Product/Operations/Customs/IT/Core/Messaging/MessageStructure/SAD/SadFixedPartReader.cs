using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class SadFixedPartReader
{
	SadFixedPartReader(string recordType)
	{
		this.recordType = recordType;
	}
	readonly string recordType;

	public IdocRecordType RecordType
	{
		get
		{
			switch (recordType)
			{
				case IdocRecordTypeList.Codes.SadImportEntryHeader:
					return IdocRecordType.SadImportEntryHeader;
				case IdocRecordTypeList.Codes.SadImportEntryLine:
					return IdocRecordType.SadImportEntryLine;
				case IdocRecordTypeList.Codes.SadExportEntryHeader:
					return IdocRecordType.SadExportEntryHeader;
				case IdocRecordTypeList.Codes.SadExportEntryLine:
					return IdocRecordType.SadExportEntryLine;
				case IdocRecordTypeList.Codes.SadNbHeader:
					return IdocRecordType.SadNbHeader;
				case IdocRecordTypeList.Codes.SadNbLine:
					return IdocRecordType.SadNbLine;
				case IdocRecordTypeList.Codes.SadNeHeader:
					return IdocRecordType.SadNeHeader;
				case IdocRecordTypeList.Codes.SadNeLine:
					return IdocRecordType.SadNeLine;
				default:
					return IdocRecordType.Unkown;
			}
		}
	}

	public ZString AnnualProgressiveNumber { get; private set; }

	public ZInt ProgressiveNumber { get; private set; }

	public static SadFixedPartReader LoadFromRow(ZString row)
	{
		if (row.Length < 22 || !ZInt.TryParse(row.SubstringSafe(20, 2), out var progressiveNumber))
		{
			throw new UnableToInterpretCustomsMessageException(FormattableString.Invariant($"Row does not contain a valid Sad Fixed Part"));
		}

		return new SadFixedPartReader(recordType: row.SubstringSafe(0, 4).Trim())
		{
			AnnualProgressiveNumber = row.SubstringSafe(14, 6).Trim(),
			ProgressiveNumber = progressiveNumber,
		};
	}
}

public enum IdocRecordType
{
	IdocHeader,
	SadImportEntryHeader,
	SadImportEntryLine,
	SadExportEntryHeader,
	SadExportEntryLine,
	SadNbHeader,
	SadNbLine,
	SadNeHeader,
	SadNeLine,
	Unkown,
}
