using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.ManifestBase
{
	public class EUMemberStateCommunicationTypeDecider : TypeDecider, IEUMemberStateCommunicationTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var parentPK = new ZGuid(row[EUMemberStateCommunication.Schema.EUS_ParentId]);
				var parentTableCode = new ZString(row[EUMemberStateCommunication.Schema.EUS_ParentTableCode]);
				switch (parentTableCode)
				{
					case AsycudaManifestHeaderSchema.Constants.Prefix:
						{
							var header = parentPK.IsValid ? factory.Load<AsycudaManifestHeader>(parentPK) : null;
							if (header != null)
							{
								result = header.GetEUMemberStateCommunicationType();
							}
							break;
						}
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}

