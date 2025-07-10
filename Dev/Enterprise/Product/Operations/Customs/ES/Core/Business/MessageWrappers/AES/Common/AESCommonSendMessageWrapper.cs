using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IAESCommonDataProvider
	{
		public AESCommonSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
		{
		}

		protected const string CeutaOffice = "ES0055";
		protected const string MelillaOffice = "ES0056";

		public IAESCommonMessage Message => message ?? (message = new AESCommonMessageWrapper(entryHeader));
		AESCommonMessageWrapper message;

		public ZBool IsFinalPeriod => IsFinalPeriodAndTestDeclaration;

		public ZBool PhaseIDSpecified => IsFinalPeriodAndTestDeclaration;

		ZBool IsFinalPeriodAndTestDeclaration => IsTest && MessageVersionRegistryProvider.IsExportVersionAes11();

		protected ZString GetOfficeOfPresentation(ZBool wrongEntryInstruction) => wrongEntryInstruction || OfficeOfExport.StartsWith(CeutaOffice) || OfficeOfExport.StartsWith(MelillaOffice)
													? ZString.Empty
													: declaration.GetCustomsOfficeFromList(EuOfficeCodesTypes.Codes.OfficeOfPresentation);

		protected ZString OfficeOfExport
		{
			get
			{
				if (officeOfExport == null)
				{
					officeOfExport = new CachedProperty<ZString>(declaration.Factory, () => declaration.GetExportCustomsOffice());
				}
				return officeOfExport.Value;
			}
		}

		CachedProperty<ZString> officeOfExport;
	}
}
