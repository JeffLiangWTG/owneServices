using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class IE590ConsignmentProvider : IIE590Consignment
	{
		public IE590ConsignmentProvider(CusExitReport exitReport, CusExitHeader exitHeader)
		{
			this.exitReport = exitReport;
			this.exitHeader = exitHeader;
		}
		readonly CusExitReport exitReport;
		readonly CusExitHeader exitHeader;

		#region IIE590Consignment Members

		public IRegisteredEntity ExitCarrier => CachedValueHelper.GetValue(ref exitCarrierCached, GetExitCarrier);
		CachedValue<IRegisteredEntity> exitCarrierCached;
		IRegisteredEntity IIE590Consignment.ExitCarrier => ExitCarrier;

		IRegisteredEntity GetExitCarrier()
		{
			if (exitHeader.Carrier is OrgAddress carrier)
			{
				var eori = EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(carrier.Header, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				if (!eori.IsEmpty)
				{
					return PartyProvider.New(exitHeader.Carrier);
				}
			}
			return null;
		}

		public IReadOnlyCollection<string> ContainerNumbers => containerNumbers ?? (containerNumbers = GetContainerNumbers());
		IReadOnlyCollection<string> containerNumbers;
		IReadOnlyCollection<string> IIE590Consignment.ContainerNumbers => ContainerNumbers;

		IReadOnlyCollection<string> GetContainerNumbers()
		{
			var result = new List<string>();
			if (exitReport.CER_Calc_Discrepancies)
			{
				exitReport.GetContainersOrEquipments().ForEach(x =>
				{
					var container = x.container;
					if (!container.CXN_IsEquipment)
					{
						result.Add(container.CXN_ContainerNumber);
					}
				});
			}

			return result.ToArray();
		}

		#endregion
	}
}
