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
	public class DeclarationConsignmentUNDGContactContactWrapper : IDeclarationConsignmentUNDangerousGoodsContactContact
	{
		public DeclarationConsignmentUNDGContactContactWrapper(OrgContact orgContact)
		{
			this.orgContact = orgContact;
		}

		internal static DeclarationConsignmentUNDGContactContactWrapper NewOrNull(OrgContact orgContact)
			=> orgContact != null ? new DeclarationConsignmentUNDGContactContactWrapper(orgContact) : null;

		public ICollection<IDeclarationConsignmentUNDangerousGoodsContactContactCommunication> Communication
		{
			get
				=> new Collection<IDeclarationConsignmentUNDangerousGoodsContactContactCommunication>()
				{
					DeclarationConsignmentUNDGContactContactCommunicationWrapper.NewOrNull(orgContact.OC_Phone, Constants.MessagesWrappers.CommunicationType.Phone),
					DeclarationConsignmentUNDGContactContactCommunicationWrapper.NewOrNull(orgContact.OC_Email, Constants.MessagesWrappers.CommunicationType.Email)
				}.WhereNotNull().ToArray();
		}

		public ITextType Name => TextTypeWrapper.NewOrNull(orgContact.OC_ContactName);

		readonly OrgContact orgContact;
	}
}
