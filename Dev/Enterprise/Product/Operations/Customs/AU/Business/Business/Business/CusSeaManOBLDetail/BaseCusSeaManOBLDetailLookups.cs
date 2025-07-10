using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BaseCusSeaManOBLDetailLookups : Customs.Business.CusSeaManOBLDetailLookups
	{
		public BaseCusSeaManOBLDetailLookups(Customs.Business.AutoCusSeaManOBLDetail parent)
			: base(parent)
		{
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewPackageTypes()
		{
			return new CMRPackageTypes();
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewGrossWeightCodes()
		{
			return new CMRGrossWeightCodes();
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewQuantityUnits()
		{
			return new CMRQuantityUnits();
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewCargoTypes()
		{
			return new CMRImportCargoTypes();
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewContainerSizes()
		{
			return new CMRContainerSizes();
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewTypesOfContainers()
		{
			return new CMRContainerTypes();
		}
	}
}
