using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AltaH7V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.Outgoing;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class DeclarationH7MessageBuilder : H7CommonMessageBuilder<IDeclarationH7MessageDataProvider, AltaH7V1Ent>
	{
		public DeclarationH7MessageBuilder(IDeclarationH7MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override AltaH7V1Ent GenerateXMLMessage()
		{
			return new AltaH7V1Ent
			{
				Message = GetPopulatedMessage(),

				Declaration = new DeclarationTd()
				{
					SupervisingCustomsOffice = provider.Header.SupervisingCustomsOffice,
					GrossMass = provider.Header.GrossWeight,
					Representative = GetPopulatedRepresentative(),
					Declarant = GetPopulatedDeclarant(),
					Exporter = GetPopulatedExporter(),
					Importer = GetPopulatedImporter(),
					SupportingDocument = provider.Header.SupportingDocuments.ConvertToCollection(GetPopulatedDocument<SupportingDocumentTd>),
					AdditionalReference = provider.Header.AdditionalReferences.ConvertToCollection(GetPopulatedDocument<AdditionalReferenceTd>),
					TransportDocument = provider.Header.TransportDocuments.ConvertToCollection(GetPopulatedDocument<TransportDocumentTd>),
					AdditionalInformation = provider.Header.AdditionalInformation.ConvertToCollection(GetPopulatedAdditionalInformation),
					TranspCostToDest = GetPopulatedValueCost(provider.Header.TransportCostToDestination),
					PreviousDocument = provider.Header.PreviousDocuments.ConvertToCollection(GetPopulatedDocument<PreviousDocumentTd>),
					ReferenceNumberUcr = provider.Header.ReferenceNumberUCR,
					AdditionalFiscalRef = provider.Header.AdditionalFiscalRef.ConvertToCollection(GetPopulatedAdditionalFiscalRef),
					AdditionalProcedure = provider.Header.AdditionalProcedures.ConvertToCollection(GetPopulatedAdditionalProcedure),
					LocationOfGoods = provider.Header.GoodsLocation,
					GoodsItem = provider.Lines.ConvertToCollection(PopulatedGoodsItem)
				}
			};
		}

		RepresentativeTd GetPopulatedRepresentative()
		{
			var representative = provider.Header.Representative;
			return representative == null ? null : new RepresentativeTd
			{
				ContactPerson = GetPopulatedContactPerson(representative.ContactInfo),
				IdentificationNumber = representative.Id,
				Status = representative.Status
			};
		}

		DeclarantTd GetPopulatedDeclarant()
		{
			var declarant = provider.Header.Declarant;
			return declarant == null ? null : new DeclarantTd
			{
				ContactPerson = GetPopulatedContactPerson(declarant.ContactInfo),
				Name = !declarant.IsImporter ? declarant.Name : null,
				IdentificationNumber = declarant.Id,
				Address = !declarant.IsImporter ? GetPopulatedAddressInfo(declarant) : null
			};
		}

		ExporterTd GetPopulatedExporter()
		{
			var exporter = provider.Header.Exporter;
			return exporter == null ? null : new ExporterTd
			{
				Name = exporter.Name,
				IdentificationNumber = exporter.Id,
				Address = GetPopulatedAddressInfo(exporter)
			};
		}

		ImporterTd GetPopulatedImporter()
		{
			var importer = provider.Header.Importer;
			return importer == null ? null : new ImporterTd
			{
				Name = importer.Name,
				EMailAddress = importer.ContactInfo.Email,
				PhoneNumber = importer.ContactInfo.PhoneNumber,
				IdentificationNumber = importer.Id,
				Address = GetPopulatedAddressInfo(importer),
				NaturalPerson = importer.IsParticular ? IsValueTrue : IsValueFalse
			};
		}

		T GetPopulatedDocument<T>(IDocumentsCommon doc) where T : ICommonDocuments, new()
		{
			return new T()
			{
				Type = doc.Name,
				RefNum = doc.Number
			};
		}

		AdditionalInformationTd GetPopulatedAdditionalInformation(IH7AdditionalInfo addInfo)
		{
			return new AdditionalInformationTd
			{
				Code = addInfo.Code,
				AdditionalInformationDescription = addInfo.Description
			};
		}

		ValueTd GetPopulatedValueCost(IH7Value value)
		{
			return value == null ? null : new ValueTd
			{
				Amount = value.Amount,
				CurrencyCode = value.CurrencyCode
			};
		}

		AdditionalFiscalRefTd GetPopulatedAdditionalFiscalRef(IH7AdditionalFiscalRef addFiscalRef)
		{
			return new AdditionalFiscalRefTd
			{
				AdditionalFiscalRefRole = addFiscalRef.Role,
				AdditionalFiscalRefVatId = addFiscalRef.Id
			};
		}

		AdditionalProcedureTd GetPopulatedAdditionalProcedure(ZString procedure)
		{
			return new AdditionalProcedureTd
			{
				AdditionalProcedureCode = procedure
			};
		}

		GoodsItemTd PopulatedGoodsItem(IDeclarationH7Line line)
		{
			return new GoodsItemTd
			{
				DeclarationGoodsItemNumber = line.LineNumber,
				Value = GetPopulatedValueCost(line.Value),
				Commodity = GetPopulatedCommodity(),
				GoodsMeasure = GetPopulatedMeasures(),
				NumberOfPackages = line.NumberOfPackages
			};

			CommodityTd GetPopulatedCommodity()
			{
				return new CommodityTd
				{
					CommodityCode = line.CommodityCode,
					DescriptionOfGoods = line.GoodsDescription
				};
			}

			GoodsMeasureTd GetPopulatedMeasures()
			{
				return new GoodsMeasureTd
				{
					GrossMass = line.GrossWeight,
					SupplementaryUnitsQty = line.ComplementaryUnitsQty
				};
			}
		}
	}
}
