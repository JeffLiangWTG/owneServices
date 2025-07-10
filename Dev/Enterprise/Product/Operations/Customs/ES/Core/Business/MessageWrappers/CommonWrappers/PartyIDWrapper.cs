using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PartyIdWrapper : IPartyIdProvider
	{
		public static PartyIdWrapper New(OrgHeader orgHeader) => orgHeader == null ? null : new PartyIdWrapper(orgHeader);

		public static PartyIdWrapper New(OrgAddress address) => PartyIdWrapper.New(address?.Header);

		public static PartyIdWrapper New(JobDocAddress jobDocAddress) => PartyIdWrapper.New(jobDocAddress?.Address?.Header);

		protected PartyIdWrapper(OrgHeader orgH)
		{
			orgHeader = orgH;
		}
		protected readonly OrgHeader orgHeader;

		public ZString Id => IdCore;
		protected virtual ZString IdCore
		{
			get
			{
				if (orgCode == null)
				{
					orgCode = new CachedProperty<ZString>(orgHeader.Factory, () =>
					{
						return OrgHeaderExtension.GetIDCode(orgHeader);
					});
				}
				return orgCode.Value;
			}
		}
		CachedProperty<ZString> orgCode;

		protected (ZString, bool) GetIdForNaturalPerson()
		{
			var isNaturalPersonIndividual = orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual;
			if (isNaturalPersonIndividual)
			{
				var nif = OrgHeaderExtension.GetNIFCode(orgHeader);
				return (nif.IsEmpty ? OrgHeaderExtension.GetPASCode(orgHeader) : nif, isNaturalPersonIndividual);
			}
			else
			{
				return (ZString.Empty, isNaturalPersonIndividual);
			}
		}

		protected AddressType GetAddressForNaturalPerson<AddressType>(OrgAddress orgAddress, AddressType getAddress)
			where AddressType : PartyAddressWrapper
		{
			return orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual
																		&& !Id.IsEmpty
																		&& Id == OrgHeaderExtension.GetPASCode(orgHeader)
																? getAddress
																: null;
		}

		protected ZString GetPhoneFromAddress(OrgAddress orgAddress) => !orgAddress.OA_Phone.IsEmpty ? orgAddress.OA_Phone : orgAddress.Header?.MainAddress.OA_Phone ?? ZString.Empty;

		protected ZString GetEmailFromAddress(OrgAddress orgAddress, JobDeclaration jobDeclaration) => !orgAddress.OA_Email.IsEmpty
			? orgAddress.OA_Email :
			!(orgAddress.Header?.MainAddress.OA_Email ?? ZString.Empty).IsEmpty ? orgAddress.Header.MainAddress.OA_Email : jobDeclaration.DeclEmailAddr;

		protected ZString GetContactEmailForImportH1(JobDeclaration declaration) => declaration.DeclEmailAddr + (!declaration.ZG_OtherEmailAddr.IsEmpty ? " : " + declaration.ZG_OtherEmailAddr : ZString.Empty);

		protected ZBool DeclarantTypeIsNot2Or5(JobDeclaration declaration) => declaration.JE_DeclarantType != ESRepresentationTypeList.Codes._2Direct && declaration.JE_DeclarantType != ESRepresentationTypeList.Codes._5IndirectATC;

		protected static OrgAddress GetDeclarantOrgAddress(JobDeclaration jobDeclaration, OrgAddress declarantOrOperationOrgAddress, OrgAddress operatorAddress)
		{
			if (jobDeclaration.JE_DeclarantType == (ZString)ESRepresentationTypeList.Codes._2Direct || jobDeclaration.JE_DeclarantType == (ZString)ESRepresentationTypeList.Codes._5IndirectATC)
			{
				return jobDeclaration?.Representative?.Header == null
				? operatorAddress
					: declarantOrOperationOrgAddress;
			}
			else
			{
				return jobDeclaration?.Representative?.Header != null
					? jobDeclaration.Representative
					: declarantOrOperationOrgAddress;
			}
		}
	}
}
