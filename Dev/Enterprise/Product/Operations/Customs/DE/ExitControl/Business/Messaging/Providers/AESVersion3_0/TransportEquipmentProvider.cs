using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class TransportEquipmentProvider : ITransportEquipment
	{
		public TransportEquipmentProvider(CusExitContainer container, CusExitConsignment consignment, bool mapNumberOfSeals)
		{
			this.container = Argument.NotNull(container, nameof(container));
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
			this.mapNumberOfSeals = mapNumberOfSeals;
		}

		public TransportEquipmentProvider(CusExitConsignment consignment, bool mapNumberOfSeals)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
			this.mapNumberOfSeals = mapNumberOfSeals;
		}

		readonly CusExitContainer container;
		readonly CusExitConsignment consignment;
		readonly bool mapNumberOfSeals;

		public string ContainerIdentificationNumber => container != null && !container.CXN_IsEquipment ? container.CXN_ContainerNumber : null;

		public int? NumberOfSeals
		{
			get
			{
				int? numberOfSeals = null;
				if (mapNumberOfSeals)
				{
					if (container != null)
					{
						numberOfSeals = container.AllSealNumbers.Count;
					}
					else
					{
						numberOfSeals = 0;
					}
				}
				return numberOfSeals;
			}
		}

		public IReadOnlyCollection<string> SealIdentifiers
		{
			get
			{
				if (sealIdentifiers == null)
				{
					if (NumberOfSeals > 0)
					{
						var result = new List<string>();
						result.AddRange(container.AllSealNumbers.Select(x => x.BK_SealNumber.ToString()));
						sealIdentifiers = result.ToArray();
					}
					else
					{
						sealIdentifiers = Array.Empty<string>();
					}
				}
				return sealIdentifiers;
			}
		}
		IReadOnlyCollection<string> sealIdentifiers;

		public IReadOnlyCollection<int> DeclarationGoodsItemNumbers
		{
			get
			{
				if (declarationGoodsItemNumbers == null)
				{
					var consignmentItems = consignment.CusExitConsignmentItems;
					if (container != null)
					{
						declarationGoodsItemNumbers = container.CusExitConsignmentPivots
							.Where(p => consignmentItems.Contains(p.CNP_CCI_ConsignmentItem))
							.Select(p => (int)p.ConsignmentItem.CCI_LineNumber).ToArray();
					}
					else
					{
						declarationGoodsItemNumbers = consignmentItems.Select(i => (int)i.CCI_LineNumber).ToArray();
					}
				}
				return declarationGoodsItemNumbers;
			}
		}
		IReadOnlyCollection<int> declarationGoodsItemNumbers;
	}
}
