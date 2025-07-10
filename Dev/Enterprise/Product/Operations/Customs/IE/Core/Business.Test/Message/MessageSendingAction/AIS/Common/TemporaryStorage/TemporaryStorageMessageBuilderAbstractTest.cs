using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	abstract class TemporaryStorageMessageBuilderAbstractTest<T> : EU.Business.CusTempStorage.Testing.TemporaryStorageMessageBuilderTest<T> where T : TemporaryStorageMessageBuilder
	{
		protected override string MessageSubType => string.Empty;

		protected override ZString ExpectedEM_MessageType => function.MessageType;

		protected override ZGuid ExpectedEM_GP => Credential.PK;

		protected override EU.Business.CusTempStorage.TemporaryStorageHeader GetNewHeader()
		{
			_ = Credential;
			var port1 = Factory.New<RefUNLOCO>();
			port1.RL_Code = "AAA";
			port1.RL_RN_NKCountryCode = "FR";
			var port2 = Factory.New<RefUNLOCO>();
			port2.RL_Code = "BBB";
			port2.RL_RN_NKCountryCode = "IE";
			var header = Factory.New<TemporaryStorageHeader>();
			header.MasterBill.ABL_RL_NKPortOfLoading = "AAA";
			header.MasterBill.ABL_RL_NKPortOfDischarge = "BBB";
			header.AMA_CustomsOffice = "OFFICE1";
			header.CustomsOfficeOfLodgement = "OFFICE2";
			header.PresentationCustomsOffice = "OFFICE3";
			return header;
		}

		GlbCompanyCredential Credential => credential ?? (credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company).GlbExternalPassword);
		GlbCompanyCredential credential;

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
	}
}
