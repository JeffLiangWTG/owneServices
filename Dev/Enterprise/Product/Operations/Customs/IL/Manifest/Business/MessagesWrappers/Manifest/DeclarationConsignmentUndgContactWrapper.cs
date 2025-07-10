using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentUndgContactWrapper : IDeclarationConsignmentUNDangerousGoodsContact
	{
		DeclarationConsignmentUndgContactWrapper(OrgContact orgContact)
		{
			this.orgContact = orgContact;
		}

		internal static DeclarationConsignmentUndgContactWrapper NewOrNull(OrgContact orgContact)
			=> orgContact != null ? new DeclarationConsignmentUndgContactWrapper(orgContact) : null;

		public ICollection<IDeclarationConsignmentUNDangerousGoodsCommunication> Communication
		{
			get => new Collection<IDeclarationConsignmentUNDangerousGoodsCommunication>()
			{
				DeclarationConsignmentUndgContactCommunicationWrapper.NewOrNull(orgContact.Header.MainAddress.OA_Phone, Constants.MessagesWrappers.CommunicationType.Phone),
				DeclarationConsignmentUndgContactCommunicationWrapper.NewOrNull(orgContact.Header.MainAddress.OA_Email, Constants.MessagesWrappers.CommunicationType.Email)
			}.WhereNotNull().ToArray();
		}

		public ICollection<IDeclarationConsignmentUNDangerousGoodsContactContact> Contact => new List<IDeclarationConsignmentUNDangerousGoodsContactContact>() { DeclarationConsignmentUNDGContactContactWrapper.NewOrNull(orgContact) };

		public ITextType Name => TextTypeWrapper.NewOrNull(orgContact.Header.OH_FullName);

		readonly OrgContact orgContact;
	}
}
