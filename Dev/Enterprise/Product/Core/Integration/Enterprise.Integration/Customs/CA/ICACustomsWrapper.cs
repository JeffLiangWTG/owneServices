using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICACustomsWrapper
			{
				IJobDeclaration DeclarationExposed { get; }

				void RemoveUnrelatedReleaseStatuses(IReleaseStatus releaseStatus);

				BusinessObjectCollection GetCAReleaseStatus();

				ZString GetCAPreviousCCN();

				ZString GetCATransactionNo();

				ZString GetCargoControlNumberForCanada();

				ZString GetPreviousCargoControlNumberForCanada();

				ZString GetCACarrierName();

				ZString GetCAUSPortOfExit();

				ZDateTime GetDateOfFirstArrival();

				ZDateTime GetWarehouseReleaseDate();
			}
		}
	}
}
