using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public sealed class CAUDMInterchangeTypeDecider : TypeDecider, Integration.Customs.CA.IUDMInterchangeTypeDecider
	{
		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = row[EDIInterchangeSchema.EI_ApplicationCode.Name].ToString();

			if (applicationCode == EDIInterchange.ApplicationCodes.UniversalDataMessaging
				&& BatchProcessorUtilities.IsCBSAeHubID(row[EDIInterchangeSchema.Constants.EI_To]?.ToString()))
			{
				ZGuid pk = ZGuid.TryParse(row[EDIInterchangeSchema.PK.Name].ToString(), out pk) ? pk : ZGuid.Empty;

				if (pk.IsValid)
				{
					var message = factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, pk));

					if (message != null
						&& message.EM_ApplicationCode == EDIMessage.ApplicationCodes.CAIMP
						&& message.EM_MessageType == MessageTypeList.Codes.IntegratedImportDeclaration)
					{
						return typeof(CAUniversalXMLInterchange);
					}
				}
			}

			return null;
		}
	}
}
