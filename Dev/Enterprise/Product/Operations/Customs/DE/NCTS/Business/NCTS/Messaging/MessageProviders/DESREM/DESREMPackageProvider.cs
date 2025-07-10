using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DESREMPackageProvider : NCTSPackageProvider, IDESREMPackage
	{
		public DESREMPackageProvider(NctsPackage package) : base(package)
		{
		}

		public int SequenceNumber => package.SequenceNumber;

		string INCTSPackage.Kind => TypeOfDifferenceIsNEW ? Kind : null;

		long? INCTSPackage.Quantity => TypeOfDifferenceIsNEW ? Quantity : null;

		string INCTSPackage.MarksNumber => TypeOfDifferenceIsNEW ? MarksNumber : null;

		bool TypeOfDifferenceIsNEW => CachedValueHelper.GetValue(ref typeOfDifferenceIsNEW, () => package.B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW);
		CachedValue<bool> typeOfDifferenceIsNEW;
	}
}
