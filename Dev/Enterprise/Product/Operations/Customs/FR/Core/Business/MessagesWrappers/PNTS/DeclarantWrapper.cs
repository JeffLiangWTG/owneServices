using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class DeclarantWrapper : IDeclarant
	{
		DeclarantWrapper(TemporaryStorageHeader header, OrgHeader declarantHeader)
		{
			this.declarantHeader = declarantHeader;
			this.customsAgent = header.CustomsAgent;
			this.header = header;
			this.declarant = header.Declarant;
		}

		readonly TemporaryStorageHeader header;
		readonly OrgHeader declarantHeader;
		readonly OrgAddress declarant;
		readonly GlbStaff customsAgent;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = declarant.GetEuIdentificationNumber(header.AMA_RN_NKCountry, true));
		string identificationNumber;

		public string Name => declarantHeader.OH_FullName;

		public ICollection<ICommunication> Communication => communication ?? (communication = GetCommunication());

		ICollection<ICommunication> communication;

		ICollection<ICommunication> GetCommunication()
		{
			var result = new Collection<ICommunication>();
			var broker = BrokerWrapper.New(customsAgent);
			if (broker != null)
			{
				result.Add(broker);
			}
			return result;
		}

		public static DeclarantWrapper New(TemporaryStorageHeader header) => header?.Declarant?.Header is OrgHeader declarantHeader ? new DeclarantWrapper(header, declarantHeader) : null;
	}
}
