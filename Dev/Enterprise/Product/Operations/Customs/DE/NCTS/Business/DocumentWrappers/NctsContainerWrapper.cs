using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.DE.NCTS.Business.DocumentWrappers
{
	public class NctsContainerWrapper : DocBaseWrapper
	{
		public NctsContainerWrapper(NctsDepartureHeaderContainer container, BusinessObjectFactory factory) : base(container, factory)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}
		readonly NctsDepartureHeaderContainer container;

		public static NctsContainerWrapper New(NctsDepartureHeaderContainer container) => new NctsContainerWrapper(container, container.Factory);

		public ZString ContainerNumber => container.BC_ContainerNum;

		public ZString Seals
		{
			get
			{
				var sealStringBuilder = new ZStringBuilder();
				sealStringBuilder.AppendIfNotEmpty(container.BC_Seal1);
				sealStringBuilder.AppendIfNotEmpty(container.BC_Seal2);
				sealStringBuilder.AppendIfNotEmpty(string.Join(", ", container.AdditionalSeals.Select(s => s.BK_SealNumber)));

				return sealStringBuilder.ToStringWithDelimiterBetweenAppends(", ");
			}
		}
	}
}
