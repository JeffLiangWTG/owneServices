using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class LocalExportAmendmentTypeCodeList
	{
		public static ZString GetLocalExportAmendTypeForEntry(ZString amendType)
		{
			var result = ZString.Empty;
			switch (amendType)
			{
				case Codes.Delete:
					result = nameof(EntityAmendType.Delete);
					break;
				case Codes.Update:
					result = nameof(EntityAmendType.Update);
					break;
				default:
					result = nameof(EntityAmendType.Add);
					break;
			}
			return result;
		}
	}
}
