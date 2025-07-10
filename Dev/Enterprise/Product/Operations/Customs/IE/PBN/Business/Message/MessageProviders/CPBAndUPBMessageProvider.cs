using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces.PBN;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class CPBAndUPBMessageProvider : ICPBAndUPBHeader
	{
		public CPBAndUPBMessageProvider(PBNMessageSendingObject sendingObject)
		{
			Argument.NotNull(sendingObject, nameof(sendingObject));
			header = sendingObject.Header;
		}
		readonly AsycudaManifestHeader header;

		public string Direction
		{
			get
			{
				if (header.AMA_Nature.EqualsIgnoringCase(ShipmentTypeList.Codes.Import23))
				{
					return DirectionImport;
				}
				else if (header.AMA_Nature.EqualsIgnoringCase(ShipmentTypeList.Codes.Export22))
				{
					return DirectionExport;
				}
				else
				{
					return null;
				}
			}
		}

		public bool EmptyVehicle => header.IsEmptyVehicle;

		public IReadOnlyCollection<IPBNDeclaration> Declarations => declarationsCached ??= header.CustomsReferenceCollection.Where(x => !x.CSI_ReferenceNumber.IsEmpty && x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBA).Select(PBNDeclarationProvider.New)
			.Union(header.TransitDeclarationCollection.Where(x => !x.CSI_ReferenceNumber.IsEmpty).Select(PBNDeclarationProvider.New)).ToArray();
		IReadOnlyCollection<IPBNDeclaration> declarationsCached;

		public IPBNContactDetails ContactDetails => CachedValueHelper.GetValue(ref contactDetailsCached, () => PBNContactDetailsProvider.New(header));
		CachedValue<IPBNContactDetails> contactDetailsCached;

		const string DirectionImport = "IN_IRELAND";
		const string DirectionExport = "OUT_IRELAND";
	}
}
