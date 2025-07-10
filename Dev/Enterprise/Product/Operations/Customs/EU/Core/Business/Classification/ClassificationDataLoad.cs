using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Business.Classification
{
	public class ClassificationDataLoad : ClassificationDataLoad<CusClassification>
	{
		public const string Column_CPC = "CPC";
		public const string Column_EcSupplement1 = "ECSUPPLEMENT1";
		public const string Column_EcSupplement2 = "ECSUPPLEMENT2";

		protected override void ProcessCountrySpecificData(ClassificationDataToLoad dataToLoad, CusClassification classification)
		{
			if (dataToLoad is ClassificationDataToLoadForEU data)
			{
				SetValue(classification.CC_ProcedureCodeInfo, data.CPC);
				SetValue(classification.CC_EcSupplement1Info, data.ECSUPPLEMENT1);
				SetValue(classification.CC_EcSupplement2Info, data.ECSUPPLEMENT2);
			}
		}

		protected override IEnumerable<ZPropertyInfo> GetCountrySpecificClassificationFieldsUponWhichToRunFriendlyValidation(CusClassification classification)
		{
			return new ZPropertyInfo[] { classification.CC_ProcedureCodeInfo, classification.CC_EcSupplement1Info, classification.CC_EcSupplement2Info };
		}

		protected override IEnumerable<string> GetCountrySpecificFieldNames()
		{
			return new string[] { Column_CPC, Column_EcSupplement1, Column_EcSupplement2 };
		}

		protected override ZString GetTariffDescription(ZString tariff, ZString type)
		{
			return string.Empty; // never called. 
		}

		protected override CusClassification LoadClassificationIfItExists(ZString lookupCode, ZString type)
		{
			return new CusClassification.Loader(Factory).Load(lookupCode, null);
		}

		protected override ClassificationDataToLoad GetClassificationDataToLoad()
		{
			return new ClassificationDataToLoadForEU();
		}
	}
}
