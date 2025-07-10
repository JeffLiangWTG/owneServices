using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Outgoing;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class T2LCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
		where TProvider : IESEDIMessageCollectionProvider
	{
		protected T2LCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected T GetPopulatedSegmentosDeServicio<T>() where T : IT2LServiceSegment, new()
		{
			return new T
			{
				TransactionId = TransactionId
			};
		}

		protected void PopulateT2LCommon<S>(IT2LCommon declaration, IT2LHeaderCommon commonHeaderProvider)
			where S : IT2LServiceSegment, new()
		{
			declaration.ServiceSegment = GetPopulatedSegmentosDeServicio<S>();
			declaration.ExpeditionCountry = commonHeaderProvider.ExpeditionCountry;
			declaration.TotalLinesNum = commonHeaderProvider.TotalLinesNum;
			declaration.TotalPackagesQty = commonHeaderProvider.TotalPackagesQty;
			declaration.ContainersIndicator = commonHeaderProvider.ContainersIndicator ? IndicadorContenedoresSiNoTipo.Item1 : IndicadorContenedoresSiNoTipo.Item0;
			declaration.Declarant = GetPopulatedAddressInformationDeclarantCommon<DeclaranteTipo>(commonHeaderProvider.Declarant);
		}

		protected T GetPopulatedT2LExpeditionCommon<T, S, E, D, L>(IExpeditionHeader commonHeaderProvider, IEnumerable<IExpeditionLine> lines)
			where T : IT2LExpeditionCommon, new()
			where S : IT2LServiceSegment, new()
			where E : IOrgAddressInfoCommon, new()
			where D : IOrgAddressInfoCommon, new()
			where L : IT2LExpeditionCommonLine, new()
		{
			var declaration = new T
			{
				ExpeditionCustomsOffice = commonHeaderProvider.ExpeditionCustomsOffice,
				DestinationCountry = commonHeaderProvider.DestinationCountry,
				ConveyanceId = commonHeaderProvider.ConveyanceId,
				Sender = GetPopulatedAddressInformationCommon<E>(commonHeaderProvider.Sender),
				Consignee = GetPopulatedAddressInformationCommon<D>(commonHeaderProvider.Consignee),
				Communications = GetPopulatedCommunications(commonHeaderProvider.Communications),
				Lines = new Collection<IT2LExpeditionCommonLine>(lines.ConvertToCollection(GetPopulatedLineExpedition)?.Cast<IT2LExpeditionCommonLine>().ToArray() ?? System.Array.Empty<IT2LExpeditionCommonLine>())
			};

			PopulateT2LCommon<S>(declaration, commonHeaderProvider);

			return declaration;

			L GetPopulatedLineExpedition(IExpeditionLine line)
			{
				var declarationLine = GetPopulatedLineCommon<L>(line);
				declarationLine.DocumentsSubmitted = line.DocumentsSubmitted.ConvertToCollection(GetPopulatedDocument);

				return declarationLine;

				DocumentosTipo GetPopulatedDocument(IExpeditionDocumentSubmitted doc)
				{
					return doc == null ? null : new DocumentosTipo
					{
						ClaseDocumento = doc.Code,
						IdentificacionDocumento = doc.Number,
						FechaDelDocumento = doc.Date.ToCustomsFormatDateString()
					};
				}
			}
		}

		protected T GetPopulatedT2LReceptionCommon<T, S, L>(IReceptionHeader commonHeaderProvider, IEnumerable<IT2LLineCommon> lines)
			where T : IT2LReceptionCommon, new()
			where S : IT2LServiceSegment, new()
			where L : IT2LCommonLine, new()
		{
			var declaration = new T
			{
				ReceptionCustomsOffice = commonHeaderProvider.ReceptionCustomsOffice,
				ReceptionT2LReference = commonHeaderProvider.ReceptionT2LReference,
				ExpeditionDate = commonHeaderProvider.ExpeditionDate.ToCustomsFormatDateString(),
				Lines = new Collection<IT2LCommonLine>(lines.ConvertToCollection(GetPopulatedLineCommon<L>)?.Cast<IT2LCommonLine>().ToArray() ?? System.Array.Empty<IT2LCommonLine>())
			};

			PopulateT2LCommon<S>(declaration, commonHeaderProvider);

			return declaration;
		}

		protected ComunicacionesCorreoElectronicoTipo GetPopulatedCommunications(IT2LCommunicationsCommon communications)
		{
			return communications == null ? null : new ComunicacionesCorreoElectronicoTipo
			{
				DirComunicaDespacho = communications.DeclarationEmail,
				DirComunicaOtros = communications.OtherEmail,
				ComunicarDespachoVerdes = communications.GreenCircuitIndicator ? IndicadorSiNoTipo.S : IndicadorSiNoTipo.N,
			};
		}

		T GetPopulatedLineCommon<T>(IT2LLineCommon line) where T : IT2LCommonLine, new()
		{
			return new T()
			{
				LineNumber = line.LineNumber,
				GoodsCode = line.GoodsCode,
				GoodsDescription = line.GoodsDescription.Left(MessageSchema.JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l),
				GrossWeightInKG = line.GrossWeightInKG,
				NetWeightInKG = line.NetWeightInKG,
				Packages = line.Packages.ConvertToCollection(GetPopulatedPackageNumbers<BultosTipo>),
				Containers = line.Containers.ConvertToStringCollection(),
				Vehicles = line.Vehicles.ConvertToCollection(GetPopulatedCommonVehicle<VehiculosTipo>)
			};
		}
	}
}
