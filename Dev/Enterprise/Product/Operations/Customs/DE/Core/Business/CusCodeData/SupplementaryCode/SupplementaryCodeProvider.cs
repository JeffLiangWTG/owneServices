using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.DE.Business
{
	sealed class SupplementaryCodeProvider : EU.Business.SupplementaryCodeProvider
	{
		public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
		{
		}

		public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
		{
			if (master is ICanBeImportOrExport importExportParent)
			{
				IsForExport = importExportParent.IsExport;
			}
		}

		bool IsForExport { get; }

		public override ZShort NumberOfCodes => IsForExport ? new ZShort(97) : base.NumberOfCodes;
	}
}
