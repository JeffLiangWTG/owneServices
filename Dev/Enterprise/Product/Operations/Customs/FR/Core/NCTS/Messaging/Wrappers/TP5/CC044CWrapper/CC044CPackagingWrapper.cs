using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5;

public class CC044CPackagingWrapper : PackagingWrapper
{
	protected CC044CPackagingWrapper(NctsPackage package) : base(package)
	{
	}

	public new static CC044CPackagingWrapper New(NctsPackage package) => package == null ? null : new CC044CPackagingWrapper(package);

	bool StatusIsNewOrDif => package.B5_TypeOfDifference.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF });
	bool IsPackDifference => package.IsPackDifference;

	public override string SequenceNumber => sequenceNumber ?? (sequenceNumber = package.B5_SequenceNumber.ToString());
	string sequenceNumber;

	public override string TypeOfPackages => typeOfPackages ?? (typeOfPackages = StatusIsNewOrDif || IsPackDifference ? package.B5_UnitType : null);
	string typeOfPackages;

	public override string NumberOfPackages => numberOfPackages ?? (numberOfPackages = StatusIsNewOrDif || IsPackDifference ? package.B5_UnitCount.ToString() : null);
	string numberOfPackages;

	public override string ShippingMarks => shippingMarks ?? (shippingMarks = StatusIsNewOrDif || IsPackDifference ? package.B5_MarksAndNumbers : null);
	string shippingMarks;
}
