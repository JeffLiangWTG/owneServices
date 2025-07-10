using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class GuaranteeReferenceForQueryWrapper : IGuaranteeReferenceForQuery
	{
		public GuaranteeReferenceForQueryWrapper(TP5MessageSendingObject sendingObject, NctsGuarantee guarantee)
		{
			this.sendingObject = sendingObject;
			this.nctsHeader = Argument.NotNull(sendingObject.NctsHeader, nameof(sendingObject));
			this.guarantee = guarantee;
		}

		readonly TP5MessageSendingObject sendingObject;
		readonly NctsHeader nctsHeader;
		readonly NctsGuarantee guarantee;

		public static GuaranteeReferenceForQueryWrapper New(TP5MessageSendingObject sendingObject, NctsGuarantee guarantee) => sendingObject == null || guarantee == null ? null : new GuaranteeReferenceForQueryWrapper(sendingObject, guarantee);

		public string Grn => grn ??= guarantee.CusGuarantee?.GetApplicationSpecificReference(OrgCusAccountDeltaTTypeList.Codes.TR) ?? guarantee.PW_BondNumber;
		string grn;

		public IGuaranteeQuery GuaranteeQuery => guaranteeQuery ??= GuaranteeQueryWrapper.New(sendingObject);
		IGuaranteeQuery guaranteeQuery;

		public IOrganization Owner => owner ??= sendingObject.RequesterRole == "1" ? null : OrganizationWrapper.New(nctsHeader.Principal.Organisation);
		IOrganization owner;

		public IAccessCode AccessCode => accessCode ??= new AccessCodeWrapper(guarantee.PW_Password);
		IAccessCode accessCode;
	}
}
