using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using static System.FormattableString;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirOutturnDataObjectReaderHelper : UniversalCommonReaderHelper
	{
		public AirOutturnDataObjectReaderHelper(UniversalObjectFactory factory, string dataProviderForCodeMapping = null)
			: base(factory, Core.Constants.CountryCodes.Australia, dataProviderForCodeMapping)
		{
		}

		public void MarkUnprocessedExistingBillsFor(CusUnderbond underbond)
		{
			foreach (var outturn in underbond.Outturns.Cast<CusOutturn>())
			{
				if (!ExistingOutturnProcessingDictionary.ContainsKey(outturn))
				{
					ExistingOutturnProcessingDictionary.Add(outturn, false);
				}
			}
		}

		public void MarkProcessed(CusOutturn outturn)
		{
			if (ExistingOutturnProcessingDictionary.ContainsKey(outturn))
			{
				ExistingOutturnProcessingDictionary[outturn] = true;
			}
		}

		public void DeleteUnprocessedBillsFor(CusUnderbond underbond, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingOutturnProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.C5_C4_Underbond == underbond.PK)
				{
					var outturn = pair.Key;
					if (outturn.CanDelete)
					{
						ExistingOutturnProcessingDictionary.Remove(outturn);
						logger.Log(LogType.Information, Res.GetString("4820B9E9-33D4-4673-B477-2CDF9E8AAC30", "Deleted {0} from {1}.", outturn.HumanReadableName, "UniversalShipment"));
						underbond.Outturns.RemoveAndDelete(outturn);
					}
					else
					{
						var reason = (ZString)outturn.ReasonForNotAbleToDelete;
						var reasonToShow = reason.Left(1).ToLower() + reason.SubstringSafe(1);

						logger.Log(LogType.Warning, Invariant($"{outturn.HumanReadableName} does not appear in UniveralShipment, but {reasonToShow}"));
						MarkProcessed(outturn);
					}
				}
			}
		}

		Dictionary<CusOutturn, bool> ExistingOutturnProcessingDictionary => existingOutturnProcessingDictionary ?? (existingOutturnProcessingDictionary = new Dictionary<CusOutturn, bool>());
		Dictionary<CusOutturn, bool> existingOutturnProcessingDictionary;
	}
}
