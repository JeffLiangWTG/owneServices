using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface ICIQDataHeader
	{
		ZString CIQOfficeOfEntryOrExitCode { get; }
		ZString OfficeOfDestination { get; }

		ZString BillNumber { get; }
		ZString IsOriginalContainerLoading { get; }

		ZString CIQRelatedNumber { get; }
		ZString CIQRelatedReason { get; }

		ZString ConsumerContactName { get; }
		ZString ConsumerContactPhone { get; }

		ZDateTime DateOfUnloadComplete { get; }

		IEnumerable<ZString> OtherPackageCodes { get; }
		IEnumerable<ICIQEnterpriseQualification> EnterpriseQualifications { get; }
		IEnumerable<ICIQRequiredDocument> RequiredDocuments { get; }
	}

	public interface ICIQEnterpriseQualification
	{
		ZString Type { get; }
		ZString Number { get; }
	}

	public interface ICIQRequiredDocument
	{
		ZString DocumentType { get; }
		ZInt NumberOfOriginals { get; }
		ZInt NumberOfCopies { get; }
	}
}
