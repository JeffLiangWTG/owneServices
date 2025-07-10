using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.CAU;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDH2V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class DeclarationDVDMessageBuilder : DVDCommonMessageBuilder<IDeclarationDVDMessageDataProvider, Dvdh2V1Ent>
	{
		public DeclarationDVDMessageBuilder(IDeclarationDVDMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		readonly string representativeTypeDIR = "2";
		readonly string representativeTypeIND = "3";
		readonly string isContainerised = "1";
		readonly string isNotContainerised = "0";

		protected override Dvdh2V1Ent GenerateXMLMessage()
		{
			return new Dvdh2V1Ent()
			{
				Mensaje = GetPopulatedCommonMessage<TdMensaje>(),
				Cabecera = GetPopulatedHeader(),
				Partida = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}

		TdCabecera GetPopulatedHeader()
		{
			var header = provider.Header;
			return header == null ? null : new TdCabecera
			{
				EdAduanaDeclaracion = header.CustomsOffice,
				Ed25Nrl = header.LRN,
				Ed11TipoDeclaracion = header.DeclarationType,
				Ed12TipoDeclaracionAdicional = header.DeclarationSubType,
				Ed65MasaBruta = header.TotalGrossMass,
				Ed32NumIdentifExportador = header.ExporterId,
				Ed316NumIdentifDepositante = header.ConsigneeId,
				Ed317Declarante = new Td22InformacionAdicional { CodigoUe = header.DeclarantUECode },
				Ed318NumIdentifDeclarante = header.DeclarantId,
				Ed319Representante = new TdEmailNotificacion { EmailNotificaDespacho = header.DeclarationEmail, EmailOtrasNotificaciones = header.DeclarationOtherEmail },
				Ed320NumIdentifRepresentante = header.RepresentativeId,
				Ed321CodEstatutoRepresentante = header.RepresentativeType.Equals(representativeTypeDIR) ? Td321CodEstatutoRepresentante.Item2 : Td321CodEstatutoRepresentante.Item3,
				Ed321CodEstatutoRepresentanteValueSpecified = header.RepresentativeType.Equals(representativeTypeDIR) || header.RepresentativeType.Equals(representativeTypeIND),
				Ed339TitularAutorizacion = header.Authorisations.ConvertToCollection(GetPopulatedAuthorisation),
				Ed74ModoTransporteFrontera = header.TransportCode,
				Ed337AgentesCadenaSuministro = header.AdditionalSupplyActors.ConvertToCollection(GetPopulatedSupplyChainActor),
				Ed23DocCertifAutoriz = header.SupportingDocuments.ConvertToCollection(GetPopulatedSupportingDocument),
				Ed72Contenedor = header.IsContainerised ? isContainerised : isNotContainerised,
				Ed523LocalizacionMercancias = GetPopulatedLocationOfGoods(header.LocationOfGoods),
				Ed58PaisDestino = header.CountryOfDestination,
				Ed514PaisExportacion = header.CountryOfExport,
				Ed24NumeroReferenciaRue = header.UCRReferenceNumber,
				Ed27Deposito = GetPopulatedWarehouse(header.Warehouse),
				Ed21DocumentoPrecedente = header.PreviousDocuments.ConvertToCollection(GetPopulatedPreviousDocument),
				Ed83Garantia = header.Guarantees.ConvertToCollection(GetPopulatedGuarantee),
				Ed526AduanaPresentacion = header.CustomOfficeOfPresentation
			};
		}

		TdPartida GetPopulatedLine(IDeclarationDVDLine line)
		{
			return new TdPartida
			{
				Ed16NumeroPartida = line.LineNumber,
				Ed23DocCertifAutoriz = line.SupportingDocuments.ConvertToCollection(GetPopulatedSupportingDocumentForLine),
				Ed22InformacionAdicional = line.AdditionalInfos.ConvertToCollection(GetPopulatedAdditionalInfos),
				Ed337AgentesCadenaSuministro = line.AdditionalSupplyActors.ConvertToCollection(GetPopulatedSupplyChainActor),
				Ed68DescripcionMercancias = line.Description,
				Ed613CodigoCus = line.CusCode,
				Ed61415CodigoMercancia = new Td61415CodigoMercancia { CodigoMercancia = line.TariffCode, CodigoTaricMercancia = line.TariffCodeCombined },
				Ed616CodigoAdicionalMercancia = line.TariffAdditionalCodes.ConvertToStringCollection(),
				Ed617CodigoAdicionalNacionalMercancia = line.NationalAdditionalCodes.ConvertToStringCollection(),
				Ed417Preferencia = GetPopulatedPreference(line.PreferenceCode, line.ReductionCode),
				Ed44BaseImponible = line.Taxes.ConvertToCollection(GetPopulatedTax),
				Ed61MasaNeta = line.NetMass,
				Ed65MasaBruta = line.GrossMass,
				Ed62UnidadesSuplementarias = line.SupplementaryUnitsQty,
				Ed710Contenedores = line.Containers.ConvertToStringCollection(),
				Ed58PaisDestino = line.CountryOfDestination,
				Ed514PaisExportacion = line.CountryOfExport,
				Ed110Regimen = new Td110Regimen { RegimenSolicitado = line.RequestedCPC, RegimenPrevio = line.PreviousCPC },
				Ed111RegimenAdicional = line.AdditionalProcedures.ConvertToCollection(GetPopulatedAdditionalProcedure),
				Ed515PaisOrigen = line.CountryOfOrigin,
				Ed691011Bultos = line.Packages.ConvertToCollection(GetPopulatedCommonPackage<Td691011Bultos>),
				Ed691011Vehiculos = line.Vehicles.ConvertToCollection(GetPopulatedVehicle),
				Ed21DocumentoPrecedente = line.PreviousDocuments.ConvertToCollection(GetPopulatedPreviousDocument),
				Ed24NumeroReferenciaRue = line.UCRReferenceNumber
			};
		}

		Td339TitularAutorizacion GetPopulatedAuthorisation(IDeclarationDVDAuthorisation authorisation)
		{
			return new Td339TitularAutorizacion
			{
				CodigoTipoAutorizacion = GetAuthorisationType(),
				IdentificadorTitAutorizacion = authorisation.OwnerId
			};

			TdTipoAutorizacionDecision GetAuthorisationType()
			{
				switch (authorisation.Type)
				{
					case AuthorisationType.ACE:
						return TdTipoAutorizacionDecision.Ace;
					case AuthorisationType.ACP:
						return TdTipoAutorizacionDecision.Acp;
					case AuthorisationType.ACR:
						return TdTipoAutorizacionDecision.Acr;
					case AuthorisationType.ACT:
						return TdTipoAutorizacionDecision.Act;
					case AuthorisationType.AEOC:
						return TdTipoAutorizacionDecision.Aeoc;
					case AuthorisationType.AEOF:
						return TdTipoAutorizacionDecision.Aeof;
					case AuthorisationType.AEOS:
						return TdTipoAutorizacionDecision.Aeos;
					case AuthorisationType.AWB:
						return TdTipoAutorizacionDecision.Awb;
					case AuthorisationType.BOI:
						return TdTipoAutorizacionDecision.Boi;
					case AuthorisationType.BTI:
						return TdTipoAutorizacionDecision.Bti;
					case AuthorisationType.CCL:
						return TdTipoAutorizacionDecision.Ccl;
					case AuthorisationType.CGU:
						return TdTipoAutorizacionDecision.Cgu;
					case AuthorisationType.CVA:
						return TdTipoAutorizacionDecision.Cva;
					case AuthorisationType.CW1:
						return TdTipoAutorizacionDecision.Cw1;
					case AuthorisationType.CW2:
						return TdTipoAutorizacionDecision.Cw2;
					case AuthorisationType.CWP:
						return TdTipoAutorizacionDecision.Cwp;
					case AuthorisationType.DDA1:
						return TdTipoAutorizacionDecision.Dda1;
					case AuthorisationType.DDA2:
						return TdTipoAutorizacionDecision.Dda2;
					case AuthorisationType.DDAP:
						return TdTipoAutorizacionDecision.Ddap;
					case AuthorisationType.DPO:
						return TdTipoAutorizacionDecision.Dpo;
					case AuthorisationType.DREF:
						return TdTipoAutorizacionDecision.Dref;
					case AuthorisationType.EIR:
						return TdTipoAutorizacionDecision.Eir;
					case AuthorisationType.ETd:
						return TdTipoAutorizacionDecision.Etd;
					case AuthorisationType.EUS:
						return TdTipoAutorizacionDecision.Eus;
					case AuthorisationType.IPO:
						return TdTipoAutorizacionDecision.Ipo;
					case AuthorisationType.OPO:
						return TdTipoAutorizacionDecision.Opo;
					case AuthorisationType.REM:
						return TdTipoAutorizacionDecision.Rem;
					case AuthorisationType.REP:
						return TdTipoAutorizacionDecision.Rep;
					case AuthorisationType.RSS:
						return TdTipoAutorizacionDecision.Rss;
					case AuthorisationType.SAS:
						return TdTipoAutorizacionDecision.Sas;
					case AuthorisationType.SDE:
						return TdTipoAutorizacionDecision.Sde;
					case AuthorisationType.SSE:
						return TdTipoAutorizacionDecision.Sse;
					case AuthorisationType.TEA:
						return TdTipoAutorizacionDecision.Tea;
					case AuthorisationType.TRD:
						return TdTipoAutorizacionDecision.Trd;
					case AuthorisationType.TST:
						return TdTipoAutorizacionDecision.Tst;
					default:
						return default(TdTipoAutorizacionDecision);
				}
			}
		}

		Td337AgenteCadenaSuministro GetPopulatedSupplyChainActor(IAdditionalSupplyChainActorCommon chainActor)
		{
			return new Td337AgenteCadenaSuministro
			{
				CodigoFuncion = GetChainActorRole(),
				IdentificadorAgente = chainActor.Id
			};

			TdCodigoFuncionAgenteCadenaSuministro GetChainActorRole()
			{
				switch (chainActor.Role)
				{
					case SupplyChainActorRole.CS:
						return TdCodigoFuncionAgenteCadenaSuministro.Cs;
					case SupplyChainActorRole.MF:
						return TdCodigoFuncionAgenteCadenaSuministro.Mf;
					case SupplyChainActorRole.FW:
						return TdCodigoFuncionAgenteCadenaSuministro.Fw;
					case SupplyChainActorRole.WH:
						return TdCodigoFuncionAgenteCadenaSuministro.Wh;
					default:
						return default(TdCodigoFuncionAgenteCadenaSuministro);
				}
			}
		}

		Td23V4DocCertifAutoriz GetPopulatedSupportingDocument(IDeclarationDVDSupportingDocument document)
		{
			return new Td23V4DocCertifAutoriz
			{
				TipoDocumentoUe = document.EUCode,
				TipoDocumentoNacional = document.NationalCode,
				IdentificadorDocumento = document.Number,
				FechaValidez = document.DocumentDate.ToCustomsFormatDateString()
			};
		}

		Td523LocalizacionMercancias GetPopulatedLocationOfGoods(IDeclarationDVDLocationOfGoods location)
		{
			return location == null ? null : new Td523LocalizacionMercancias
			{
				Pais = location.LocationCountry,
				TipoUbicacion = GetLocationType(),
				Cualificador = GetLocationQualifier(),
				LugarDeUbicacionCodificado = new TdUbicacionMercanciasLugarDeUbicacionCodificado
				{
					IdentificadorUbicacion = location.LocationId,
					IdentificadorAdicional = location.LocationAdditionalId
				},
				LugarDeUbicacionDescripcion = new TdUbicacionMercanciasLugarDeUbicacionDescripcion
				{
					CalleYNumero = location.LocationAddress,
					CodigoPostal = location.LocationPostCode,
					Localidad = location.LocationCity
				}
			};

			TdUbicacionMercanciasTipoUbicacion GetLocationType()
			{
				switch (location.LocationType)
				{
					case LocationOfGoodsType.A:
						return TdUbicacionMercanciasTipoUbicacion.A;
					case LocationOfGoodsType.B:
						return TdUbicacionMercanciasTipoUbicacion.B;
					case LocationOfGoodsType.C:
						return TdUbicacionMercanciasTipoUbicacion.C;
					case LocationOfGoodsType.D:
						return TdUbicacionMercanciasTipoUbicacion.D;
					default:
						return default(TdUbicacionMercanciasTipoUbicacion);
				}
			}

			TdUbicacionMercanciasCualificador GetLocationQualifier()
			{
				switch (location.LocationQualifier)
				{
					case LocationOfGoodsQualifier.T:
						return TdUbicacionMercanciasCualificador.T;
					case LocationOfGoodsQualifier.U:
						return TdUbicacionMercanciasCualificador.U;
					case LocationOfGoodsQualifier.V:
						return TdUbicacionMercanciasCualificador.V;
					case LocationOfGoodsQualifier.W:
						return TdUbicacionMercanciasCualificador.W;
					case LocationOfGoodsQualifier.X:
						return TdUbicacionMercanciasCualificador.X;
					case LocationOfGoodsQualifier.Y:
						return TdUbicacionMercanciasCualificador.Y;
					case LocationOfGoodsQualifier.Z:
						return TdUbicacionMercanciasCualificador.Z;
					default:
						return default(TdUbicacionMercanciasCualificador);
				}
			}
		}

		Td27Deposito GetPopulatedWarehouse(IWarehouseCommon warehouse)
		{
			return warehouse == null ? null : new Td27Deposito
			{
				TipoDeposito = GetWarehouseType(),
				IdentificacionDeposito = warehouse.Identifier
			};

			Td27DepositoTipoDeposito GetWarehouseType()
			{
				switch (warehouse.Type)
				{
					case WarehouseType.R:
						return Td27DepositoTipoDeposito.R;
					case WarehouseType.S:
						return Td27DepositoTipoDeposito.S;
					case WarehouseType.T:
						return Td27DepositoTipoDeposito.T;
					case WarehouseType.U:
						return Td27DepositoTipoDeposito.U;
					case WarehouseType.V:
						return Td27DepositoTipoDeposito.V;
					case WarehouseType.Y:
						return Td27DepositoTipoDeposito.Y;
					case WarehouseType.Z:
						return Td27DepositoTipoDeposito.Z;
					default:
						return default(Td27DepositoTipoDeposito);
				}
			}
		}

		Td21V4DocPrecedenteDecSimplificada GetPopulatedPreviousDocument(IDeclarationDVDPreviousDocument document)
		{
			return new Td21V4DocPrecedenteDecSimplificada
			{
				Tipo = document.Name,
				Referencia = document.Number,
				Partida = document.LineNumber,
				CodigoNumeroUnidades = document.UnitOfMeasure.IsEmpty ? null : new TdV1CodigoNumeroUnidades
				{
					UnidadMedidaTaric = document.UnitOfMeasure,
					NumeroUnidades = document.Quantity
				}
			};
		}

		Td83Garantia GetPopulatedGuarantee(IDeclarationDVDGuarantee guarantee)
		{
			return new Td83Garantia
			{
				ReferenciaGrn = guarantee.GRNReference,
				ReferenciaNoGrn = guarantee.NoGRNReference,
				CodigoAcceso = guarantee.AccessCode,
				Divisa = guarantee.Currency,
				Importe = guarantee.Amount,
				ImporteValueSpecified = !guarantee.Amount.IsEmpty,
				Aduana = guarantee.Office
			};
		}

		Td23V4DocCertifAutoriz GetPopulatedSupportingDocumentForLine(IDeclarationDVDSupportingDocumentForLine document)
		{
			var supDoc = GetPopulatedSupportingDocument(document);
			if (supDoc != null)
			{
				var uom = document.UnitOfMeasure;
				var quantity = document.Quantity;
				supDoc.CodigoNumeroUnidades = uom.IsEmpty && quantity.IsEmpty ? null : new TdV1CodigoNumeroUnidades
				{
					UnidadMedidaTaric = uom,
					NumeroUnidades = quantity
				};

				var currency = document.Currency;
				var amount = document.Amount;
				supDoc.DivisaImporte = currency.IsEmpty && amount.IsEmpty ? null : new TdDivisaImporte
				{
					Divisa = currency,
					Importe = amount
				};
			}
			return supDoc;
		}

		Td22InformacionAdicional GetPopulatedAdditionalInfos(IDeclarationDVDAdditionalInfo additionalInfo)
		{
			return new Td22InformacionAdicional
			{
				CodigoUe = additionalInfo.EUCode,
				CodigoNacional = additionalInfo.NationalCode,
				Descripcion = additionalInfo.Description
			};
		}

		Td417Preferencia GetPopulatedPreference(ZString preferenceCode, ZString reductionCode)
		{
			return preferenceCode.IsEmpty && reductionCode.IsEmpty ? null : new Td417Preferencia
			{
				Preferencia = GetPreferenceCode(),
				Reduccion = GetReductionCode()
			};

			Td417PreferenciaPreferencia GetPreferenceCode()
			{
				switch (preferenceCode)
				{
					case PreferenceCode.Item1:
						return Td417PreferenciaPreferencia.Item1;
					case PreferenceCode.Item2:
						return Td417PreferenciaPreferencia.Item2;
					case PreferenceCode.Item3:
						return Td417PreferenciaPreferencia.Item3;
					case PreferenceCode.Item4:
						return Td417PreferenciaPreferencia.Item4;
					case PreferenceCode.Item5:
						return Td417PreferenciaPreferencia.Item5;
					default:
						return default(Td417PreferenciaPreferencia);
				}
			}

			Td417PreferenciaReduccion GetReductionCode()
			{
				switch (preferenceCode)
				{
					case ReductionCode.Item00:
						return Td417PreferenciaReduccion.Item00;
					case ReductionCode.Item10:
						return Td417PreferenciaReduccion.Item10;
					case ReductionCode.Item18:
						return Td417PreferenciaReduccion.Item18;
					case ReductionCode.Item19:
						return Td417PreferenciaReduccion.Item19;
					case ReductionCode.Item20:
						return Td417PreferenciaReduccion.Item20;
					case ReductionCode.Item25:
						return Td417PreferenciaReduccion.Item25;
					case ReductionCode.Item28:
						return Td417PreferenciaReduccion.Item28;
					case ReductionCode.Item50:
						return Td417PreferenciaReduccion.Item50;
					default:
						return default(Td417PreferenciaReduccion);
				}
			}
		}

		Td44BaseImponible GetPopulatedTax(IDeclarationDVDTax tax)
		{
			var baseUnit = tax.BaseUnit;
			var baseQuantity = tax.BaseQuantity;
			var baseAmount = tax.BaseAmount;

			return new Td44BaseImponible
			{
				CodigoNumeroUnidades = baseUnit.IsEmpty && baseQuantity.IsEmpty ? null : new Td44BaseImponibleCodigoNumeroUnidades
				{
					UnidadMedidaTaric = baseUnit,
					NumeroUnidades = baseQuantity
				},
				Importe = baseAmount,
				ImporteValueSpecified = !baseAmount.IsEmpty
			};
		}

		Td111InformacionAdicional GetPopulatedAdditionalProcedure(IDeclarationDVDEUAndNationalCodes procedure)
		{
			return new Td111InformacionAdicional
			{
				CodigoUe = procedure.EUCode,
				CodigoNacional = procedure.NationalCode
			};
		}

		Td691011Vehiculos GetPopulatedVehicle(IDeclarationDVDVehicle vehicle)
		{
			return new Td691011Vehiculos
			{
				TipoBultoVehiculo = vehicle.Type,
				IdentificacionVehiculo = GetPopulatedCommonVehicle<TdIdentificacionVehiculo>(vehicle)
			};
		}

		struct AuthorisationType
		{
			internal const string ACE = "ACE";
			internal const string ACP = "ACP";
			internal const string ACR = "ACR";
			internal const string ACT = "ACT";
			internal const string AEOC = "AEOC";
			internal const string AEOF = "AEOF";
			internal const string AEOS = "AEOS";
			internal const string AWB = "AWB";
			internal const string BOI = "BOI";
			internal const string BTI = "BTI";
			internal const string CCL = "CCL";
			internal const string CGU = "CGU";
			internal const string CVA = "CVA";
			internal const string CW1 = "CW1";
			internal const string CW2 = "CW2";
			internal const string CWP = "CWP";
			internal const string DDA1 = "DDA1";
			internal const string DDA2 = "DDA2";
			internal const string DDAP = "DDAP";
			internal const string DPO = "DPO";
			internal const string DREF = "DREF";
			internal const string EIR = "EIR";
			internal const string ETd = "ETd";
			internal const string EUS = "EUS";
			internal const string IPO = "IPO";
			internal const string OPO = "OPO";
			internal const string REM = "REM";
			internal const string REP = "REP";
			internal const string RSS = "RSS";
			internal const string SAS = "SAS";
			internal const string SDE = "SDE";
			internal const string SSE = "SSE";
			internal const string TEA = "TEA";
			internal const string TRD = "TRD";
			internal const string TST = "TST";
		}
		struct SupplyChainActorRole
		{
			internal const string CS = "CS";
			internal const string MF = "MF";
			internal const string FW = "FW";
			internal const string WH = "WH";
		}
		struct LocationOfGoodsType
		{
			internal const string A = "A";
			internal const string B = "B";
			internal const string C = "C";
			internal const string D = "D";
		}
		struct LocationOfGoodsQualifier
		{
			internal const string T = "T";
			internal const string U = "U";
			internal const string V = "V";
			internal const string W = "W";
			internal const string X = "X";
			internal const string Y = "Y";
			internal const string Z = "Z";
		}
		struct WarehouseType
		{
			internal const string R = "R";
			internal const string S = "S";
			internal const string T = "T";
			internal const string U = "U";
			internal const string V = "V";
			internal const string Y = "Y";
			internal const string Z = "Z";
		}
		struct PreferenceCode
		{
			internal const string Item1 = "1";
			internal const string Item2 = "2";
			internal const string Item3 = "3";
			internal const string Item4 = "4";
			internal const string Item5 = "5";
		}
		struct ReductionCode
		{
			internal const string Item00 = "00";
			internal const string Item10 = "10";
			internal const string Item18 = "18";
			internal const string Item19 = "19";
			internal const string Item20 = "20";
			internal const string Item25 = "25";
			internal const string Item28 = "28";
			internal const string Item50 = "50";
		}
	}
}
