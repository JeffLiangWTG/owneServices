using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	partial class AmendTypeCodeList
	{
		public static ZString GetAmendTypeDescription(ZString code)
		{
			var result = ZString.Empty;
			switch (code)
			{
				case AmendTypeCodeList.Codes._01:
					result = nameof(EntityAmendType.Add);
					break;
				case AmendTypeCodeList.Codes._02:
					result = nameof(EntityAmendType.Delete);
					break;
				case AmendTypeCodeList.Codes._03:
					result = nameof(EntityAmendType.Update);
					break;
			}
			return result;
		}

		public static ZString AmendTypeForMessage(EntityAmendType amendType)
		{
			var result = ZString.Empty;
			switch (amendType)
			{
				case EntityAmendType.Add:
					result = AmendTypeCodeList.Codes._01;
					break;
				case EntityAmendType.Delete:
					result = AmendTypeCodeList.Codes._02;
					break;
				case EntityAmendType.Update:
					result = AmendTypeCodeList.Codes._03;
					break;
			}
			return result;
		}

		public static ZString LocalExportAmendTypeForMessage(EntityAmendType amendType)
		{
			var result = ZString.Empty;
			switch (amendType)
			{
				case EntityAmendType.Delete:
					result = "2";
					break;
				case EntityAmendType.Update:
					result = "3";
					break;
			}
			return result;
		}

		public static ZString ImportFTAAmendTypeForItem(EntityAmendType amendType)
		{
			var result = ZString.Empty;
			switch (amendType)
			{
				case EntityAmendType.Add:
					result = ImportFTAItemAmendTypeCodeList.Codes._99I;
					break;
				case EntityAmendType.Delete:
					result = ImportFTAItemAmendTypeCodeList.Codes._99D;
					break;
			}
			return result;
		}
	}
}
