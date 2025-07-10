using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Outgoing;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class ImportCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
		where TProvider : IImportCommonDataProvider
	{
		protected ImportCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		public readonly string IsTestTrue = "S";
		public readonly string IsCeutaOrMelillaCode = "E";
		public readonly string IsImporterIndividual = "P";
		public readonly string IsDeclarantAuthorized = "O";
		public readonly int MaxDecimals2 = 2;
		public readonly int MaxDecimals3 = 3;
		public readonly int MaxDecimals6 = 6;

		public T GetPopulatedServiceSegment<T>()
			where T : IImportServiceSegmentCommon, new()
		{
			return new T()
			{
				Id = TransactionId,
				Date = DateOfCET,
				Time = TimeOfCET,
				Test = null
			};
		}

		protected void PopulateDUACommon(IDUAImportCommon declaration, IDUAImportDataProvider commonProvider)
		{
			declaration.MRN = commonProvider.MRN;
			declaration.IsCeutaOrMelilla = commonProvider.IsCeutaOrMelilla ? IsCeutaOrMelillaCode : null;
		}

		protected void PopulateImportHeaderCommon<I, D>(IImportHeaderCommon declaration, IImportCommonHeader commonHeader)
			where I : IImportImporter, new()
			where D : IImportDeclarant, new()
		{
			declaration.ShipmentType = commonHeader.ShipmentType;
			declaration.TotalLinesNum = commonHeader.TotalLinesNum;
			declaration.Importer = GetPopulatedImporter<I>(commonHeader.Importer);
			declaration.Declarant = GetPopulatedDeclarant<D>(commonHeader.Declarant);
			declaration.DeclarationEmail = commonHeader.DeclarationEmail;
			declaration.OtherEmail = commonHeader.OtherEmail;
			declaration.OriginCountry = commonHeader.OriginCountry;
			declaration.GoodsLocation = commonHeader.GoodsLocation;
		}

		protected void PopulateDUAHeaderCommon(IDUAImportHeaderCommon declaration, IDUAImportCommonHeader commonHeader, ZString mrn)
		{
			PopulateImportHeaderCommon<C08ImportadorTd, C14DeclaranteTd>(declaration, commonHeader);

			declaration.CustomsOfficeOfDestination = mrn.IsEmpty ? commonHeader.CustomsOfficeOfDestination : null;
			declaration.Procedure = commonHeader.Procedure;
			declaration.TotalPackagesNum = commonHeader.TotalPackagesNum;
			declaration.CommercialReference = commonHeader.CommercialReference;
			declaration.IsContainerised = commonHeader.IsContainerised ? Cas19Td.Item1 : Cas19Td.Item0;
			declaration.CurrencyCode = commonHeader.CurrencyCode;
			declaration.TotalTributesAmount = commonHeader.TotalTributesAmount.Truncate(MaxDecimals2);
			declaration.PaymentMode = commonHeader.PaymentMode;
			declaration.ClearanceGuarantee = commonHeader.ClearanceGuarantee;
			declaration.PendenciesGuarantee = commonHeader.PendenciesGuarantee;
			declaration.GRNGuarantees = commonHeader.GRNGuarantees.ConvertToStringCollection();
			declaration.PaymentModeCan = commonHeader.PaymentModeCan;
			declaration.ClearanceGuaranteeCan = commonHeader.ClearanceGuaranteeCan;
			declaration.PendenciesGuaranteeCan = commonHeader.PendenciesGuaranteeCan;
			declaration.GRNGuaranteesCan = commonHeader.GRNGuaranteesCan.ConvertToStringCollection();
		}

		T GetPopulatedImporter<T>(IImportImporterProvider commonImporter) where T : IImportImporter, new()
		{
			var importer = GetPopulatedAddressInformationCommon<T>(commonImporter, true);
			if (importer != null)
			{
				importer.Particular = commonImporter.IsIndividual ? IsImporterIndividual : null;
			}
			return importer;
		}

		T GetPopulatedDeclarant<T>(IImportDeclarantPartyIdProvider commonDeclarant) where T : IImportDeclarant, new()
		{
			var declarant = GetPopulatedAddressInformationDeclarantCommon<T>(commonDeclarant, true);
			if (declarant != null)
			{
				declarant.Type = commonDeclarant.Type;
				declarant.Authorized = commonDeclarant.IsAuthorized ? IsDeclarantAuthorized : null;
			}
			return declarant;
		}

		protected void PopulateImportLineCommon(IImportLineCommon declarationLine, IImportCommonLine commonLine)
		{
			declarationLine.LineNumber = commonLine.LineNumber;
			declarationLine.Containers = new Collection<string>(commonLine.Containers?.ConvertToArray() ?? System.Array.Empty<string>());
			declarationLine.GoodsDescription = commonLine.GoodsDescription.Left(MessageSchema.JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l);
			declarationLine.TariffCode = commonLine.TariffCode;
			declarationLine.OriginCountry = commonLine.OriginCountry;
		}

		protected void PopulateDUALineCommon(IDUAImportLineCommon declarationLine, IDUAImportCommonLine commonLine)
		{
			PopulateImportLineCommon(declarationLine, commonLine);

			declarationLine.ExternalPackagingType = commonLine.ExternalPackagingType;
			declarationLine.InternalPackages = commonLine.InternalPackages.ConvertToCollection(GetPopulatedPackageNumbers<C31EmpaquetamientoInternoTd>);
			declarationLine.Vehicles = commonLine.Vehicles.ConvertToCollection(GetPopulatedCommonVehicle<C31VehiculosTd>);
			declarationLine.OtherMeasurementUnits = GetPopulatedMeasurementUnits<C31OtrasUnidadesDeMedidaTd>(commonLine.OtherMeasurementUnitsNumber, commonLine.OtherMeasurementUnitsCode);
			declarationLine.TariffSupplementaryCodes = commonLine.TariffSupplementaryCodes.ConvertToStringCollection();
			declarationLine.ProductTitleForSpecialTaxes = commonLine.ProductTitleForSpecialTaxes;
			declarationLine.SpecialTaxesIndicator = commonLine.SpecialTaxesIndicator;
			declarationLine.GrossWeightInKG = commonLine.GrossWeightInKG;
			declarationLine.PreferenceCode = commonLine.PreferenceCode;
			declarationLine.ReductionCode = commonLine.ReductionCode;
			declarationLine.CustomsCPC = GetPopulatedRegimen();
			declarationLine.NetWeightInKG = commonLine.NetWeightInKG;
			declarationLine.Contingency = commonLine.Contingency;
			declarationLine.PrecedentDocument = GetPopulatedDocumentoCargoPrecedente();
			declarationLine.SupplementaryUnits = GetPopulatedMeasurementUnits<Cas41Td>(commonLine.SupplementaryUnitsNumber, commonLine.SupplementaryUnitsCode);
			declarationLine.InvoiceValue = commonLine.InvoiceValue.Truncate(MaxDecimals3);
			declarationLine.DocumentsAndCertificates = commonLine.DocumentsAndCertificates.ConvertToCollection(GetPopulatedDocAndCert);
			declarationLine.SpecialInstructions = commonLine.SpecialInstructions.ConvertToStringCollection();
			declarationLine.DeclaredTaxes = commonLine.DeclaredTaxes.ConvertToCollection(GetPopulatedDeclaredTax);
			declarationLine.TotalValue = commonLine.TotalValue.Truncate(MaxDecimals2);

			M GetPopulatedMeasurementUnits<M>(ZDecimal unitsNumber, ZString unitsCode)
				where M : IMeasurementUnits, new()
			{
				var measurements = default(M);
				if (!unitsCode.IsEmpty || !unitsNumber.IsEmpty)
				{
					measurements = new M
					{
						UnitsNumber = unitsNumber.Truncate(MaxDecimals3),
						UnitsCode = unitsCode
					};
				}

				return measurements;
			}

			Cas371RegTd GetPopulatedRegimen()
			{
				return new Cas371RegTd
				{
					C371RegimenSolicitado = commonLine.RequestedCPC,
					C371RegimenPrecedente = commonLine.PreviousCPC,
					C372CodigoAdicional = commonLine.ConcessionsCPC.ConvertToStringCollection()
				};
			}

			Cas40Td GetPopulatedDocumentoCargoPrecedente()
			{
				var docType = commonLine.PrecedentDocumentType;
				var docClass = commonLine.PrecedentDocumentClass;
				var docRef = commonLine.PrecedentDocumentReference;
				return docType.IsEmpty && docClass.IsEmpty && docRef.IsEmpty ? null : new Cas40Td
				{
					C40TipoDocumento = docType,
					C40ClaseDocumento = docClass,
					C40ReferenciaDocumento = docRef
				};
			}

			Cas47TributoDeclaradoTd GetPopulatedDeclaredTax(IDUAImportDeclaredTax declaredTax)
			{
				return new Cas47TributoDeclaradoTd
				{
					C47TributoClase = declaredTax.TaxClass,
					C47TributoBaseImponible = declaredTax.TaxableIncome.Truncate(MaxDecimals3),
					C47TributoTipoImpositivo = declaredTax.TaxRate.Truncate(MaxDecimals6),
					C47TributoIndicadorMaxMinNor = declaredTax.MaxMinIndicator,
					C47TributoUnidadFiscal = declaredTax.FiscalUnit,
					C47TributoCuota = declaredTax.Fee.Truncate(MaxDecimals2)
				};
			}
		}

		protected Cas44Td GetPopulatedDocAndCert(IImportCommonC44CertificateDocument docAndCert)
		{
			var declarationDoc = GetPopulatedDocumentCommon<Cas44Td>(docAndCert);

			declarationDoc.C44Unidad = docAndCert.CertQuantityUnit;
			declarationDoc.CertQuantityAmount = docAndCert.CertQuantityAmount.Truncate(MaxDecimals3);
			declarationDoc.C44Fecha = docAndCert.CertDate.IsEmpty ? null : (string)docAndCert.CertDate.ToCustomsFormatDateStringddMMyyyy();
			declarationDoc.C44ModoDeRegistro = null;

			return declarationDoc;
		}
	}
}
