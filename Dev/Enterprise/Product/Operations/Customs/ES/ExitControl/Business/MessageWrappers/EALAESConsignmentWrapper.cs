using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using TransportModeTranslator = Enterprise.Customs.EU.Business.TransportModeTranslator;

namespace Enterprise.Customs.ES.ExitControl.Business;

public class EALAESConsignmentWrapper : IEALAESConsignment
{
	public EALAESConsignmentWrapper(CusExitReport exitReport)
	{
		this.exitReport = Argument.NotNull(exitReport, nameof(exitReport));
		exitConsignment = Argument.NotNull(exitReport.Consignment, nameof(exitReport.Consignment));
	}
	readonly CusExitReport exitReport;
	readonly CusExitConsignment exitConsignment;

	public ZString ModeOfTransportAtTheBorder => exitReport.CER_Calc_Discrepancies ? TransportModeTranslator.TranslateToWCOCode(exitReport.CER_TransportMode, false) : ZString.Empty;

	public ZString ReferenceNumberUCR => exitReport.CER_Calc_Discrepancies ? exitConsignment.CXC_UniqueConsignmentReference : ZString.Empty;

	public IPartyIdProviderWithContactPerson ExitCarrier => exitCarrier ?? (exitCarrier = EALAESDeclarantWrapperWithContactPerson.New(exitReport.Header));
	EALAESDeclarantWrapperWithContactPerson exitCarrier;

	public IReadOnlyCollection<IAESCommonTransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = EALAESTransportEquipmentWrapper.GetTransportEquipmentList(exitReport));
	IReadOnlyCollection<EALAESTransportEquipmentWrapper> transportEquipment;

	public IEALAESLocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new EALAESLocationOfGoodsWrapper(exitReport));
	EALAESLocationOfGoodsWrapper locationOfGoods;

	public ITransportMediumInfoCommon ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = GetActiveBorderTransportMeans(exitReport));
	TransportMediumInfoCommonWrapper activeBorderTransportMeans;

	public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument
	{
		get
		{
			if (transportDocument == null)
			{
				var transportDocsList = new List<CommonDocumentSequenceNumberWrapper>();

				if (exitReport.CER_Calc_Discrepancies)
				{
					var addDocs = exitReport.AdditionalInfos.Cast<AdditionalInfo>().Where(doc => doc.IsATransportDocument
																					&& (doc.StatusIsDifferencesToDeclared
																						|| doc.StatusIsMissing))
															.ToList();

					foreach (var doc in addDocs)
					{
						var isMissing = doc.StatusIsMissing;
						transportDocsList.Add(new CommonDocumentSequenceNumberWrapper(isMissing ? ZString.Empty : doc.CSI_Code, isMissing ? ZString.Empty : doc.CSI_ReferenceNumber, doc.CSI_ItemNumber));
					}
				}

				transportDocument = transportDocsList.AsReadOnly();
			}
			return transportDocument;
		}
	}
	IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> transportDocument;

	TransportModeTranslator TransportModeTranslator => transportModeTranslator ?? (transportModeTranslator = new TransportModeTranslator());
	TransportModeTranslator transportModeTranslator;

	TransportMediumInfoCommonWrapper GetActiveBorderTransportMeans(CusExitReport exitReport)
	{
		var mot = exitReport.CER_TransportType;
		var id = exitReport.CER_TransportID;
		var nationality = exitReport.CER_RN_NKTransportNationality;

		return exitReport.CER_Calc_Discrepancies && (!mot.IsEmpty || !id.IsEmpty || !nationality.IsEmpty)
			? new TransportMediumInfoCommonWrapper(mot, id, nationality) : null;
	}
}
