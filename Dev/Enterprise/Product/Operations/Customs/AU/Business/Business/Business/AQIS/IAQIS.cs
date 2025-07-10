using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAQIS : IBusiness
	{
		AQISDocumentCollection AQISDocuments { get; }
		AQISPremisesIdAndProcessingTypeCollection AQISPremisesIdAndProcessingTypes { get; }
		AQISCommodityCodeCollection AQISCommodityCodes { get; }
		AQISEntityIdCollection AQISEntityIds { get; }
		AQISPermitIdCollection AQISPermitIds { get; }
		AQISProducerCodeCollection AQISProducerCodes { get; }
		AUAddInfo AddInfo { get; }
	}

	public interface IAQISUniqueCodeForSort
	{
		ZString[] CodesToSortBy { get; }
#if DEBUG
		ZPropertyInfo[] CodeInfosToSortByForTestingOnly { get; }
#endif
	}

	public class AQISUniqueCodeCaseInsensitiveComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			IAQISUniqueCodeForSort x1 = x as IAQISUniqueCodeForSort;
			IAQISUniqueCodeForSort y1 = y as IAQISUniqueCodeForSort;

			int result = 0;
			if (x1 != null && y1 != null)
			{
				ZString[] xCodes = x1.CodesToSortBy;
				ZString[] yCodes = y1.CodesToSortBy;

				if (xCodes.Length != yCodes.Length)
				{
					ErrorReporter.ReportOnce(x1.GetType().FullName + " " + y1.GetType().FullName + " are being compared which have the different number of 'CodesToSortBy'", x1.GetType().FullName + " " + y1.GetType().FullName + " are being compared which have the different number of 'CodesToSortBy'");
				}

				for (int index = 0; index < xCodes.Length && index < yCodes.Length; index++)
				{
					ZString xCode = xCodes[index];
					ZString yCode = yCodes[index];
					result = new CaseInsensitiveComparer().Compare(xCode, yCode);
					if (result != 0)
					{
						break;
					}
				}
			}
			return result;
		}
	}
}
