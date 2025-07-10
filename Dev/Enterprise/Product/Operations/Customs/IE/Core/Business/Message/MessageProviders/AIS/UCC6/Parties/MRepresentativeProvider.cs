using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class MRepresentativeProvider : IMRepresentative
	{
		readonly JobDeclaration declaration;

		public MRepresentativeProvider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public string Id => CachedValueHelper.GetValue(ref idCached, () =>
		{
			var id = declaration.RepresentativeOrgAddress.GetEORI();
			return id.IsEmpty ? null : id;
		});
		CachedValue<string> idCached;

		public string Status => MapStatusToCustomsValue(declaration.JE_DeclarantType);

		public CargoWise.Customs.IE.MessageContracts.Interfaces.IContact ContactPerson
		{
			get
			{
				if (contactPersonCached == null)
				{
					var contact = declaration.Representative?.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
					if (contact != null)
					{
						contactPersonCached = new ContactProvider(contact.OC_ContactName, contact.OC_Phone, contact.Email);
					}
				}
				return contactPersonCached;
			}
		}
		CargoWise.Customs.IE.MessageContracts.Interfaces.IContact contactPersonCached;

		string MapStatusToCustomsValue(string cw1Code)
		{
			switch (cw1Code)
			{
				case RepresentationTypeList.Codes._2Direct:
					return Constants.RepresentationTypeList.Direct;
				case RepresentationTypeList.Codes._3Indirect:
					return Constants.RepresentationTypeList.Indirect;
				default:
					return null;
			}
		}
	}
}
