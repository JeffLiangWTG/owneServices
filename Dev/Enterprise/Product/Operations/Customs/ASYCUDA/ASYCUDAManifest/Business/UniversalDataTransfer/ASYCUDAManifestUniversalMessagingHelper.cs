using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer
{
	public class ASYCUDAManifestUniversalMessagingHelper : AsycudaManifestUniversalMessagingHelper
	{
		public ASYCUDAManifestUniversalMessagingHelper(INotifications notifications)
			: base(notifications)
		{
		}

		protected override ICodeDescriptionDataObject GetActionPurpose(string countryCode, string messageType, string messageSubType)
		{
			return new CodeDescriptionPair() { Code = "Z" + countryCode, Description = "ASYCUDA Manifest" };
		}

		protected override IEnumerable<RecipientRoleDetail> GetRecipientRoles()
		{
			return new[] { new RecipientRoleDetail() { Type = RecipientRoleType.ASY } };
		}

		protected override ZString GetRecipientID(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return "ASYCUDA"; // Don't want to have to create a whole new eHub structure for each country. Use the existing one, and detect the country from the ActionPurpose.
		}

		protected override string CalculateMessageStatus(IMessageParent messageParent) => MessageStatusCodeList.Codes.Sent;
	}
}
