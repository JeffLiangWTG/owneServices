using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.G3.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3RevokeHouseConsignmentWrapper : G3HouseConsignmentWrapper, IG3RevokeHouseConsignment
	{
		public G3RevokeHouseConsignmentWrapper(AsycudaBill bill, IDocumentsCommon additionalInfoWrapper) : base(bill)
		{
			this.additionalInfoWrapper = Argument.NotNull(additionalInfoWrapper, nameof(additionalInfoWrapper));
		}

		readonly IDocumentsCommon additionalInfoWrapper;

		public IDocumentsCommon AdditionalInformation => CachedValueHelper.GetValue(ref additionalInformation, () => additionalInfoWrapper);
		CachedValue<IDocumentsCommon> additionalInformation;
	}
}
