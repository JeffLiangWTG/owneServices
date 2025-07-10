using CargoWise.Integration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class CusClassificationLookups : Customs.Business.CusClassificationLookups
	{
		public CusClassificationLookups(CusClassification parent)
			: base(parent)
		{
		}

		public CusClassification Classification
		{
			get { return Parent; }
		}

		protected new CusClassification Parent
		{
			get { return (CusClassification)base.Parent; }
		}

		public ICodeDescriptionPairList CPCs
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.EU.AllCPCs_" + Parent.CC_RN_NKCountryCode, delegate
				{
					var importAndExportCpcs = new RefCusProcedure.Loader(Factory).LoadForZzzDataGrouping(Parent.CC_RN_NKCountryCode);
					var result = new CodeDescriptionPairList();
					result.AddRange(importAndExportCpcs);
					result.Sort();
					return result;
				});
			}
		}
	}
}
