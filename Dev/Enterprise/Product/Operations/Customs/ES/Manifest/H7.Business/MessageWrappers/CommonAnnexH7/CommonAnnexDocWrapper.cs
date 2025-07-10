using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CommonAnnexDocWrapper : ES.Business.MessageWrappers.AnnexDocCommonWrapper
	{
		public CommonAnnexDocWrapper(IeDoc document, ZString docDescription, AdditionalInfoSendingObject addInfoSendingObject) : base(document, docDescription)
		{
			this.addInfoSendingObject = Argument.NotNull(addInfoSendingObject, nameof(addInfoSendingObject));
		}

		readonly AdditionalInfoSendingObject addInfoSendingObject;

		protected override ZString ReferenceNumberCore => addInfoSendingObject.ReferenceNumber;
	}
}
