using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPManifestDataObjectReaderHelper : UniversalCommonReaderHelper
	{
		public JPManifestDataObjectReaderHelper(UniversalObjectFactory factory, string dataProviderForCodeMapping = null)
			: base(factory, Core.Constants.CountryCodes.Japan, dataProviderForCodeMapping)
		{
		}

		public void MarkUnprocessedExistingBillsFor(JPAFRHeader header)
		{
			foreach (var bill in header.Bills.OfType<JPAFRBills>())
			{
				if (!ExistingBillProcessingDictionary.ContainsKey(bill))
				{
					ExistingBillProcessingDictionary.Add(bill, false);
				}
			}
		}

		public void MarkProcessed(JPAFRBills bill)
		{
			if (ExistingBillProcessingDictionary.ContainsKey(bill))
			{
				ExistingBillProcessingDictionary[bill] = true;
			}
		}

		public void DeleteUnprocessedBillsFor(JPAFRHeader header, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingBillProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.JPB_JPH_Header == header.PK)
				{
					var bill = pair.Key;
					ExistingBillProcessingDictionary.Remove(bill);
					logger.Log(LogType.Information, Res.GetString("C827BDA3-CAC7-4F48-93A4-C11590FF57F6", "Deleted {0} from {1}.", bill.HumanReadableName, "UniversalShipment"));
					bill.Delete();
				}
			}
		}

		Dictionary<JPAFRBills, bool> ExistingBillProcessingDictionary
		{
			get { return existingBillProcessingDictionary ?? (existingBillProcessingDictionary = new Dictionary<JPAFRBills, bool>()); }
		}
		Dictionary<JPAFRBills, bool> existingBillProcessingDictionary;
	}
}
