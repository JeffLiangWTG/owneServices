using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESCommonSendMessageWrapper : AESCommonSendMessageWrapper, IDeclarationAESCommonMessageDataProvider
{
	public DeclarationAESCommonSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificateData, ZString messageSubType) : base(entryHeader, certificateData)
	{
		Argument.NotNullOrEmpty(messageSubType, nameof(messageSubType));
		entryInstruction = entryHeader.EntryInstruction;
		IsComplementaryCWithMRN = messageSubType == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration
									&& entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.C
									&& !entryHeader.MovementReferenceNumber.IsEmpty;
	}
	readonly CusEntryInstruction entryInstruction;

	public IReadOnlyCollection<ICommonAuthorisation> Authorisations
	{
		get
		{
			if (authorisations == null)
			{
				var authorisationsList = new List<CommonAuthorisationWrapper>();

				var cusAuthorizations = entryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>();
				ZShort seqNum = 1;
				foreach (var authorization in cusAuthorizations)
				{
					authorisationsList.Add(new CommonAuthorisationWrapper(authorization, seqNum));
					seqNum++;
				}

				authorisations = authorisationsList.AsReadOnly();
			}
			return authorisations;
		}
	}
	IReadOnlyCollection<CommonAuthorisationWrapper> authorisations;

	public ZString CustomOfficeOfPresentation => GetOfficeOfPresentation(entryInstruction.IsSubStyleCOrYOrZ);

	public ZString CustomOfficeOfExport => OfficeOfExport;

	public ZString CustomOfficeOfExit
	{
		get
		{
			if (customOfficeOfExit == null)
			{
				customOfficeOfExit = new CachedProperty<ZString>(declaration.Factory, () => declaration.GetExitCustomsOffice());
			}
			return customOfficeOfExit.Value;
		}
	}
	CachedProperty<ZString> customOfficeOfExit;

	public IDeclarationAESExporter Exporter => exporter ?? (exporter = DeclarationAESExporterWrapper.New(declaration.SupplierDocumentaryAddress, (JobDeclaration)entryInstruction.JobDeclaration));
	DeclarationAESExporterWrapper exporter;

	public IPartyIdProviderWithContactPerson Declarant => declarant ?? (declarant = AESCommonDeclarantWrapperWithContactPerson.New(declaration));
	AESCommonDeclarantWrapperWithContactPerson declarant;

	public ICommonRepresentativeWithContactPerson Representative => representative ?? (representative = CommonRepresentativeWrapperWithContactPerson.New(declaration));
	CommonRepresentativeWrapperWithContactPerson representative;

	public IDeclarationAESGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = new DeclarationAESGoodsShipmentWrapper(entryHeader, IsComplementaryCWithMRN));
	DeclarationAESGoodsShipmentWrapper goodsShipment;

	public ZBool IsComplementaryCWithMRN { get; }
}
