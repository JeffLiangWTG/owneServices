using System.Collections.Generic;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class ContainerWrapper : IContainer
	{
		public ContainerWrapper(AsycudaContainer container)
		{
			this.container = container;
		}
		readonly AsycudaContainer container;

		string IContainer.Number => container.ACN_ContainerNumber;

		string IContainer.Type
		{
			get
			{
				var query = new ZQuery(RefContainerCodeMapSchema.RCM_RC_Container, container.ACN_RC_ContainerType);
				query.AddToFilter(RefContainerCodeMapSchema.RCM_RN_NKCountry, Core.Constants.CountryCodes.Brazil);
				var containercodemap = container?.Factory.LoadTop1<RefContainerCodeMap>(query);
				return containercodemap?.RCM_Code ?? ZString.Empty;
			}
		}

		IReadOnlyCollection<string> IContainer.Seals => new List<string>() { container.ACN_Seal1, container.ACN_Seal2, container.ACN_Seal3 };
	}
}
