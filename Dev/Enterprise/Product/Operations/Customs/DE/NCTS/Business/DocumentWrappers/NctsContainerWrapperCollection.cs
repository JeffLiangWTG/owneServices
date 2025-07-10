using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.DE.NCTS.Business.DocumentWrappers
{
	public class NctsContainerWrapperCollection : DocBaseWrapperCollection<NctsContainerWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public NctsContainerWrapperCollection(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));

			foreach (NctsDepartureHeaderContainer container in nctsHeader.DepartureHeaderContainers)
			{
				Add(NctsContainerWrapper.New(container));
			}
		}
	}
}
