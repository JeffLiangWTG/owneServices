using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public abstract class T2LPOUSCommonSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IT2LPOUSCommonDataProvider
	{
		public T2LPOUSCommonSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
		{
			contactEmail = declaration.DeclEmailAddr;
		}
		protected readonly ZString contactEmail;
		protected virtual ZBool isRepresentativeDeclared => false;
		protected const string SendEmailFixed = "S";

		public IT2LPOUSCommonPersonReqPresWithAddress PersonReqPres => personReqPres ?? (personReqPres = T2LPOUSCommonPersonReqPresWithAddressWrapper.New(OrgAddressForPersonReqPresCommon, contactEmail, isRepresentativeDeclared));
		T2LPOUSCommonPersonReqPresWithAddressWrapper personReqPres;

		protected abstract OrgAddress OrgAddressForPersonReqPresCommon { get; }

		public ZString CustomsOffice => declaration.JE_CustomsOffice;

		public ZString SendEmailL => SendEmailFixed;

		public virtual ZString SendEmailU => ZString.Empty;

		public virtual ZString SendEmailExp => ZString.Empty;
	}
}
