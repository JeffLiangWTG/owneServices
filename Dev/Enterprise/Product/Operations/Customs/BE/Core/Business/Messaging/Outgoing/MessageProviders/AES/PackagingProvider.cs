using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class PackagingProvider : IPackaging
{
	readonly InvoiceLinePackagePivot packagePivot;
	readonly int sequenceNumber;

	public PackagingProvider(InvoiceLinePackagePivot packagePivot, int sequenceNumber)
	{
		this.packagePivot = Argument.NotNull(packagePivot, nameof(packagePivot));
		this.sequenceNumber = sequenceNumber;
	}

	public int SequenceNumber => sequenceNumber;

	public string TypeOfPackages => packagePivot.Package.CW_PackType;

	public int? NumberOfPackages => packagePivot.CHC_NumberOfPacks;

	public string ShippingMarks => packagePivot.Package.CW_MarksAndNos;
}
