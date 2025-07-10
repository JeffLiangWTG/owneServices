using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ECC = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseLoadingObjectParent : XmlLoadingObjectParent<respostaconsultali, ImportLicenseLoadingObjectCollection>
	{
		public ImportLicenseLoadingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public override ImportLicenseLoadingObjectCollection Collection
		{
			get
			{
				if (importLicenseLoadingObjectCollection == null)
				{
					importLicenseLoadingObjectCollection = new ImportLicenseLoadingObjectCollection(this);
					RegisterEditableChildObject(importLicenseLoadingObjectCollection);
				}
				return importLicenseLoadingObjectCollection;
			}
		}
		ImportLicenseLoadingObjectCollection importLicenseLoadingObjectCollection;
		respostaconsultali fileDataLoaded;

		protected override bool ValidateResponse(string fileName, respostaconsultali responseData)
		{
			if (responseData == null || responseData.Item == null || ((listalicompletatype)responseData.Item).licompleta.Length == 0)
			{
				AddLog?.Invoke(100, 100, Res.GetString("EF0307B6-1623-4EB5-AE7F-990C15FD2AFB", "Unable to read {0}", fileName));
				return false;
			}

			var returnList = ((listalicompletatype)responseData.Item).licompleta;
			var importerNumber = Declaration.Importer?.GetCNPJOrCPF() ?? ZString.Empty;
			var count = 0;
			var total = returnList.Length;
			foreach (var li in returnList)
			{
				count++;
				ZString importerIdentifier = li.GrupoDadosBasicos?.Importador?.importadoridentificador;
				if (importerNumber != importerIdentifier.KeepChars(ZString.NumericCharacters))
				{
					AddLog?.Invoke(count, total, Res.GetString("06A9EE56-9B06-44AF-AEE4-FBE110B4B593", "The Importer Registration Number contained in the XML file ({0}) does not match the Importer of this Job.", importerIdentifier));
					return false;
				}
			}
			return true;
		}

		protected override void LoadObjects(respostaconsultali responseData)
		{
			SkipAllUnknownSuppliers = false;
			SkipAllUnknownManufacturer = false;

			foreach (var li in ((listalicompletatype)responseData.Item).licompleta)
			{
				var loadingObject = Collection.AddNew();
				loadingObject.ImportLicenseNo = li.GrupoDadosBasicos?.numeroli;
				if (ZDateTime.TryParseExact((ZString)li.GrupoLIAnuencias?.InformacoesLI?.dataregistro, out var registerDate, Constants.DataFormat))
				{
					loadingObject.RegistrationDate = registerDate;
				}
				loadingObject.Incoterm = li.GrupoMercadoria?.DadosGerais?.incoterm;
				loadingObject.VMLE = ConvertStringToDecimal(li.GrupoMercadoria?.Totalizadores?.valortotallocalembarque);
				loadingObject.VMCV = ConvertStringToDecimal(li.GrupoMercadoria?.Totalizadores?.valortotalcondicaovenda);
				loadingObject.NetWeight = ConvertStringToDecimal(li.GrupoMercadoria?.Totalizadores?.pesoliquidototalkg);
				loadingObject.UQ = ECC.Weight.Kilograms;
				loadingObject.AdditionalTariffsType = BRRefCusCodeListTypes.GetTariffAgreementCodeByTypeAndAgreementCodeInImportEntry(Factory, Constants.TariffAgreementTypes.Aladi, li.GrupoNegociacao?.codigoacordoaladi);
				loadingObject.Tariff = li.GrupoMercadoria?.DadosGerais?.subitemncm.Replace(".", "");
				loadingObject.Currency = BRRefCusMapper.MapCustomsCodeCurrencyToCW1Code(Factory, li.GrupoMercadoria?.DadosGerais?.moeda);
				loadingObject.ExchangeHedging = li.GrupoNegociacao?.coberturacambial;
				loadingObject.NcmCode = li.GrupoMercadoria?.listadestaquencm?.destaquencm?.codigodestaquencm;
				loadingObject.SupplierAddressPK = FindSupplierAddressPK(li.GrupoFornecedor?.Fornecedor?.fornecedorestrangeironome);
				loadingObject.ManufacturerIndicator = li.GrupoFornecedor?.fornecedortipo;
				if (loadingObject.ManufacturerIndicator == ManufacturerIndicatorList.Codes._3)
				{
					loadingObject.GoodsOrigin = BRRefCusMapper.MapCustomsCodeCountryToCW1Code(Factory, li.GrupoFornecedor?.paisaquisicaomercadoria);
				}
				if (loadingObject.ManufacturerIndicator == ManufacturerIndicatorList.Codes._2)
				{
					loadingObject.ManufacturerAddressPK = FindManufacturerAddressPK(li.GrupoFornecedor?.Fabricante?.fabricantenome);
				}
				if (loadingObject.ManufacturerIndicator == ManufacturerIndicatorList.Codes._1)
				{
					loadingObject.ManufacturerAddressPK = loadingObject.SupplierAddressPK;
				}
				loadingObject.NaladiHs = li.GrupoMercadoria?.DadosGerais?.mercadorianaladi;
				loadingObject.GoodsCondition = li.GrupoMercadoria?.CondicaoMercadoria?.condicaomercadoria;
				loadingObject.DutyTaxRegime = li.GrupoNegociacao?.regimeacordotributario;
				loadingObject.DutyLegalBase = li.GrupoNegociacao?.fundamentolegalregime;
				loadingObject.ExchangeHedgeFinancialInstitution = li.GrupoNegociacao?.codigoorgaofinanceirointernacional;
				loadingObject.ExchangeHedgeReason = li.GrupoNegociacao?.codigomotivosemcobertura;
				loadingObject.ImportLicenseLoadingObjectNcmDetailsCollection.AddNewItems(li.GrupoMercadoria?.listadetalhencm);
				loadingObject.Declaration = Declaration;
			}
			AddLog?.Invoke(100, 100, Res.GetString("69FE7C23-B497-483A-92B6-58FF305D80EC", "File loading complete!"));
		}

		ZGuid FindSupplierAddressPK(ZString supplierName)
		{
			var addressPK = ZGuid.Empty;

			if (!supplierName.IsEmpty)
			{
				addressPK = GetAddressPKByName(supplierName);
				if (!addressPK.IsValid && !SkipAllUnknownSuppliers)
				{
					var args = new UnknownOrganisationCodeEventArgs() { Name = supplierName };
					FireUnknownSupplierCodeFound(args);
					if (!args.Code.IsEmpty)
					{
						addressPK = GetAddressPKByCode(args.Code);
						matchedAddresses.Add(supplierName, addressPK);
					}
				}
			}
			return addressPK;
		}

		ZGuid FindManufacturerAddressPK(ZString manufacturerName)
		{
			var addressPK = ZGuid.Empty;
			if (!manufacturerName.IsEmpty)
			{
				addressPK = GetAddressPKByName(manufacturerName);
				if (!addressPK.IsValid && !SkipAllUnknownManufacturer)
				{
					var args = new UnknownOrganisationCodeEventArgs() { Name = manufacturerName };
					FireUnknownManufacturerCodeFound(args);
					if (!args.Code.IsEmpty)
					{
						addressPK = GetAddressPKByCode(args.Code);
						matchedAddresses.Add(manufacturerName, addressPK);
					}
				}
			}
			return addressPK;
		}

		protected override respostaconsultali ReadFile(Stream stream)
		{
			fileDataLoaded = null;
			using (var reader = new StreamReader(stream))
			{
				try
				{
					var responseText = reader.ReadToEnd();
					fileDataLoaded = XmlObjectSerializer.Deserialize<respostaconsultali>(responseText);
				}
				catch
				{
					fileDataLoaded = null;
				}
			}

			return fileDataLoaded;
		}

		public override bool CreateDataFromXml()
		{
			var successLoaded = hasValidResponse;
			if (hasValidResponse)
			{
				var totalCount = importLicenseLoadingObjectCollection.Count + importLicenseLoadingObjectCollection.Cast<ImportLicenseLoadingObject>().Sum(x => x.ImportLicenseLoadingObjectNcmDetailsCollection.Count);
				var loadedCount = 0;

				var entryInstruction = LoadOrCreateEntryInstruction();
				foreach (ImportLicenseLoadingObject loadedObject in importLicenseLoadingObjectCollection)
				{
					var invoiceHeader = LoadOrCreateInvoiceHeader(loadedObject);
					invoiceHeader.JZ_OA_SupplierAddress = loadedObject.SupplierAddressPK;
					invoiceHeader.JZ_IncoTerm = loadedObject.Incoterm.Left(invoiceHeader.JZ_IncoTermInfo.MaxLength);
					invoiceHeader.JZ_RX_NKInvoice_Currency = loadedObject.Currency.Left(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.MaxLength);
					invoiceHeader.JZ_NetWeight = loadedObject.NetWeight;
					invoiceHeader.JZ_NetWeightUQ = loadedObject.UQ.Left(invoiceHeader.JZ_NetWeightUQInfo.MaxLength);
					invoiceHeader.JZ_InvoiceAmount = loadedObject.VMCV;
					invoiceHeader.ExchangeHedgeType = loadedObject.ExchangeHedging.Left(invoiceHeader.ExchangeHedgeTypeInfo.MaxLength);
					invoiceHeader.ExchangeHedgeFinancialInstitution = loadedObject.ExchangeHedgeFinancialInstitution.Left(invoiceHeader.ExchangeHedgeFinancialInstitutionInfo.MaxLength);
					invoiceHeader.ExchangeHedgeReason = loadedObject.ExchangeHedgeReason.Left(invoiceHeader.ExchangeHedgeReasonInfo.MaxLength);

					if (loadedObject.ImportLicenseLoadingObjectNcmDetailsCollection.Count > 0)
					{
						foreach (ImportLicenseLoadingObjectNcmDetails ncmDetails in loadedObject.ImportLicenseLoadingObjectNcmDetailsCollection)
						{
							CreateInvoiceLine(loadedObject, ncmDetails, entryInstruction, invoiceHeader);
							AddLog?.Invoke(++loadedCount, totalCount, null);
						}
						AddLog?.Invoke(++loadedCount, totalCount, Res.GetString("DDB03A8E-6560-43AB-BAAA-1E958B7ED795", "Invoice Lines for Invoice Header {0} data is loaded!", invoiceHeader.JZ_InvoiceNumber));
					}
					else
					{
						AddLog?.Invoke(++loadedCount, totalCount, Res.GetString("BEE4B74E-5C84-4D73-AADC-DA3AFD3C687E", "Invoice Lines for Invoice Header {0} were not created, because XML do not contain NCM details.", invoiceHeader.JZ_InvoiceNumber));
						successLoaded = false;
					}
				}
			}
			else
			{
				AddLog?.Invoke(100, 100, Res.GetString("542A92CC-2BDC-4D11-838A-DCF78706022B", "Valid response file has not been loaded."));
			}
			return successLoaded;
		}

		void CreateInvoiceLine(ImportLicenseLoadingObject loadedObject, ImportLicenseLoadingObjectNcmDetails ncmDetails, CusEntryInstruction entryInstruction, JobComInvoiceHeader invoiceHeader)
		{
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = loadedObject.Tariff.Left(invoiceLine.JI_TariffInfo.MaxLength);
			invoiceLine.JI_LineNo = ncmDetails.SequencialProductNumber;
			invoiceLine.JI_InvoiceUQ = GetPackCode(ncmDetails.ComercialMeasureUnitName).Left(invoiceLine.JI_InvoiceUQInfo.MaxLength);
			invoiceLine.JI_NetWeight = ncmDetails.NetWeight;
			invoiceLine.JI_InvoiceQuantity = ncmDetails.ComercialMerchandiseQuantity;
			invoiceLine.JI_CustomsQuantity = ncmDetails.StatisticMerchandiseQuantity;
			invoiceLine.JI_LinePrice = ncmDetails.ShipmentTotalValue;
			invoiceLine.ImportLicenseType = loadedObject.ImportLicenseType;
			invoiceLine.ImportLicenseAuthorizationDate = loadedObject.ImportLicenseAuthorizationDate;
			invoiceLine.ImportLicenseFeeType = loadedObject.ImportLicenseFeeType;
			invoiceLine.FullGoodsDescription = ncmDetails.ProductDescription.Left(invoiceLine.FullGoodsDescriptionInfo.MaxLength);
			invoiceLine.ImportLicenseNumber = loadedObject.ImportLicenseNo.KeepNumericCharacters().Left(invoiceLine.ImportLicenseNumberInfo.MaxLength);
			if (!loadedObject.NcmCode.IsEmpty)
			{
				var invoiceLineTariffDetach = invoiceLine.TariffDetachs.AddNew();
				invoiceLineTariffDetach.CY_Code = loadedObject.NcmCode;
			}
			invoiceLine.JI_ManufacturerIndicator = loadedObject.ManufacturerIndicator.Left(invoiceLine.JI_ManufacturerIndicatorInfo.MaxLength);
			if (!invoiceLine.JI_CountryOfOriginReadOnly)
			{
				invoiceLine.JI_CountryOfOrigin = loadedObject.GoodsOrigin;
			}
			if (!invoiceLine.ManufacturerAddress_ReadOnly)
			{
				invoiceLine.EffectiveManufacturerAddressPK = loadedObject.ManufacturerAddressPK;
			}
			if (loadedObject.GoodsCondition == "S")
			{
				invoiceLine.JI_GoodsCondition = GoodsConditionTypeList.Codes.UsedMaterial;
			}
			invoiceLine.NaladiHs = loadedObject.NaladiHs.Left(invoiceLine.NaladiHsInfo.MaxLength);
			invoiceLine.DutyTaxRegime = loadedObject.DutyTaxRegime.Left(invoiceLine.DutyTaxRegimeInfo.MaxLength);
			invoiceLine.DutyLegalBase = loadedObject.DutyLegalBase.Left(invoiceLine.DutyLegalBaseInfo.MaxLength);
			var additional = invoiceLine.AdditionalTariffs.AddNew(AdditionalTaxTypeList.Codes.TariffAgreement) as AdditionalTariff;
			additional.TariffType = loadedObject.AdditionalTariffsType;
		}

		CusEntryInstruction LoadOrCreateEntryInstruction()
		{
			var entryInstruction = Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault();
			if (entryInstruction == null)
			{
				entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Description = $"{Declaration.JE_DeclarationReference}-1";
			}
			return entryInstruction;
		}

		JobComInvoiceHeader LoadOrCreateInvoiceHeader(ImportLicenseLoadingObject loadedObject)
		{
			var invoiceHeader = loadedObject.InvoiceHeaderPK.IsValid ? Factory.Load<JobComInvoiceHeader>(loadedObject.InvoiceHeaderPK) : null;
			if (invoiceHeader == null)
			{
				invoiceHeader = Declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = loadedObject.ImportLicenseNo;
			}
			return invoiceHeader;
		}

		public static ZDecimal ConvertStringToDecimal(ZString value) => ZDecimal.ParseSafe(value.Replace(".", "").Replace(",", "."), ZDecimal.Zero);

		ZString GetPackCode(ZString value)
		{
			var refPackType = value.IsEmpty ? null
				: RefPackTypes.FirstOrDefault(x => x.F3_DescriptionMultilingual.ToString(Core.SharedConstants.Languages.PortugueseBrazil).Equals(value, StringComparison.CurrentCultureIgnoreCase));
			return refPackType?.F3_Code ?? ZString.Empty;
		}

		IEnumerable<RefPackType> RefPackTypes => fRefPackTypes ?? (fRefPackTypes = new RefPackTypeCollection(Factory).Cast<RefPackType>().ToList());
		IEnumerable<RefPackType> fRefPackTypes;

		public event UnknownOrganisationCodeEventHandler UnknownSupplierCodeFound;
		public event UnknownOrganisationCodeEventHandler UnknownManufacturerCodeFound;

		void FireUnknownSupplierCodeFound(UnknownOrganisationCodeEventArgs e) => UnknownSupplierCodeFound?.Invoke(this, e);
		void FireUnknownManufacturerCodeFound(UnknownOrganisationCodeEventArgs e) => UnknownManufacturerCodeFound?.Invoke(this, e);

		public ZBool SkipAllUnknownSuppliers = false;
		public ZBool SkipAllUnknownManufacturer = false;

		protected Dictionary<string, ZGuid> matchedAddresses = new Dictionary<string, ZGuid>();

		ZGuid GetAddressPKByName(ZString name) => matchedAddresses.TryGetValue(name, out var pk) ? pk : ZGuid.Empty;

		ZGuid GetAddressPKByCode(ZString code) => Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code)?.MainAddress?.PK ?? ZGuid.Empty;
	}
}
