using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IStatusSupporter : IIdentified
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		ZString MessageStatus { set; }
		ZString CustomsStatus { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		ZString ManifestPermitNumber { set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		ZString CustomsEntryNumber { set; }
		Logs Logs { get; }
		ZBool SupportsPackLevelMessages { get; }

		void LogEventsOnParent(Event eventType, ZString reference);
	}
}
