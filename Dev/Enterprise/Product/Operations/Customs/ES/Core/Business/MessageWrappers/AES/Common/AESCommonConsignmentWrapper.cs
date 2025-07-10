using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class AESCommonConsignmentWrapper : IAESCommonConsignment
{
	public AESCommonConsignmentWrapper(CusEntryHeader entryHeader, bool isComplementaryCWithMRN)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = entryHeader.EntryInstruction;
		this.isComplementaryCWithMRN = isComplementaryCWithMRN;
	}
	protected readonly CusEntryHeader entryHeader;
	protected readonly JobDeclaration declaration;
	protected readonly CusEntryInstruction entryInstruction;
	protected readonly bool isComplementaryCWithMRN;

	public ZBool IsContainerised => entryHeader.IsContainerised();

	public ZString InlandModeOfTransport => ShoudlSendEmptyWhenBorCWithoutMRNOrSameCustomsOffices
														? ZString.Empty
														: declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, false);

	protected IReadOnlyCollection<CommonDepartureTransportMeansWrapper> GetDepartureTransportMeans()
	{
		var departureTransportMeans = new List<CommonDepartureTransportMeansWrapper>();

		if (!ShoudlSendEmptyWhenBorCWithoutMRNOrSameCustomsOffices)
		{
			var mot = declaration.JE_TransportMeans;
			var id = declaration.IsRoadInland ? declaration.JE_TransportIDInland : declaration.ZG_Box18TransportID;
			var nationality = declaration.IsRoadInland ? declaration.JE_RN_NKTransportNationalityInland : declaration.ZG_Box18TransportNationality;

			if (!mot.IsEmpty || !id.IsEmpty || !nationality.IsEmpty)
			{
				departureTransportMeans.Add(new CommonDepartureTransportMeansWrapper(mot, id, nationality, 1, declaration));

				if (declaration.IsRoadInland || declaration.IsRailInland)
				{
					AddExtraDepartureTransportMeansForROAorRAI(departureTransportMeans);
				}
			}
		}

		return departureTransportMeans.AsReadOnly();
	}

	ZBool ShoudlSendEmptyWhenBorCWithoutMRNOrSameCustomsOffices => declaration.GetExportCustomsOffice() == declaration.GetExitCustomsOffice()
																				|| (entryInstruction.IsSubStyleBOrC && !isComplementaryCWithMRN);

	void AddExtraDepartureTransportMeansForROAorRAI(List<CommonDepartureTransportMeansWrapper> departureTransportMeans)
	{
		if (!declaration.JE_Trailer1RegNo.IsEmpty)
		{
			var mot = declaration.IsRoadInland
				? EU.Business.Declaration.ExportInlandTransportTypeList.Codes._31
				: EU.Business.Declaration.ExportInlandTransportTypeList.Codes._20;
			departureTransportMeans.Add(new CommonDepartureTransportMeansWrapper(mot, declaration.JE_Trailer1RegNo, declaration.JE_RN_NKTrailer1Nationality, 2, declaration));

			if (declaration.IsRoadInland && !declaration.JE_Trailer2RegNo.IsEmpty)
			{
				departureTransportMeans.Add(new CommonDepartureTransportMeansWrapper(mot, declaration.JE_Trailer2RegNo, declaration.JE_RN_NKTrailer2Nationality, 3, declaration));
			}
			else if (declaration.IsRailInland && declaration.InlandTransports.Count != 0)
			{
				ZShort sequenceNumber = 3;
				foreach (var inlandTransport in declaration.InlandTransports.Where(x => !x.CY_Data.IsEmpty))
				{
					departureTransportMeans.Add(new CommonDepartureTransportMeansWrapper(mot, inlandTransport.CY_Data, inlandTransport.Nationality, sequenceNumber, declaration));
					sequenceNumber++;
				}
			}
		}
	}
}
