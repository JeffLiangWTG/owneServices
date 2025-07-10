using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.ExportMessageConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportSendMessageWrapper : ExportSendMessageCommonWrapper, IDUAExportMessageDataProvider
	{
		public DUAExportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData) { }

		public ZString MessageType => MessageTypeCore;

		protected virtual ZString MessageTypeCore => ExportDeclarationWrapperMessageTypeCodeList.ExportDeclaration;

		protected override ZString LocalReferenceNumberCore => entryHeader.CH_BGMReference;

		public ZString CustomsProcedureCategory1 => declaration.JE_MessageSubType;

		public ZString CustomsProcedureCategory2 => CustomsProcedureCategory2Core;

		protected virtual ZString CustomsProcedureCategory2Core => entryHeader.EntryInstruction.CEI_SubStyle;

		public ZString CustomsProcedureCategory3 => declaration.ZG_CTStatusID;

		public ZString CustomsProcedureCategory4 => invoiceHeader.JZ_ValuationCode;

		public ZString CustomsProcedureCategory5 => CustomsProcedureCategory5Core;

		protected virtual ZString CustomsProcedureCategory5Core => declaration.JE_CustomsOffice.Right(4);

		public ZString CountryOfExport => declaration.JE_GoodsOrigin;

		public ZString CountryOfDestination => declaration.JE_GoodsDestination;

		public ZString CustomsOfficeofExitCountryCode => ExitCustomsOffice.Left(2);

		public ZString CustomsOfficeofExit => ExitCustomsOffice.SubstringSafe(2);

		public ZString LocationOfGoodsExamCustomsOffice => GetTrimmedJE_LocationOfGoods().Left(4);

		public ZString LocationOfGoodsExam => GetTrimmedJE_LocationOfGoods().SubstringSafe(4);

		ZString GetTrimmedJE_LocationOfGoods()
		{
			if (trimmedLocation == null)
			{
				trimmedLocation = new CachedProperty<ZString>(declaration.Factory, () =>
				{
					var location = declaration.JE_LocationOfGoods;
					return location.Length > 10 ? location.SubstringSafe(4) : location;
				});
			}
			return trimmedLocation.Value;
		}
		CachedProperty<ZString> trimmedLocation;

		public ZString Warehouse => entryHeader.EntryInstruction.FromWarehouseCode;

		public ZDateTime DateOfRecap => declaration.ZG_LCPDepart;

		public ZBool GoodsInContainerIndicator => entryHeader.IsContainerised();

		public ZBool RMTIndicator => declaration.ZG_IsRMTApplicable;

		public IReadOnlyCollection<ZString> CountryCodes
		{
			get
			{
				if (countryCodes == null)
				{
					countryCodes = entryHeader.EntryInstruction.IncludeRoutingSecurityData
									? entryHeader.CountriesOfRouting.Where(x => !x.IsEmpty).ToArray()
									: Array.Empty<ZString>();
				}
				return countryCodes;
			}
		}
		IReadOnlyCollection<ZString> countryCodes;

		public IReadOnlyCollection<ZString> SealCodes
		{
			get
			{
				if (sealCodes == null)
				{
					sealCodes = entryHeader.SealCodes.ToArray();
				}
				return sealCodes;
			}
		}
		IReadOnlyCollection<ZString> sealCodes;

		public ZString TextFunctionCode => entryHeader.TransportChargesMoP;

		public ZString ReferenceNumber => declaration.JE_OwnerRef;

		public ZString SpecificCircumstancesIndicator => declaration.ZG_SpecificCircumstanceIndicator;

		public ITransportMediumInfoCommon BorderTransportMode => borderTransportMode ?? (borderTransportMode = new DUAExportBorderTransportInfoWrapper(declaration));
		DUAExportBorderTransportInfoWrapper borderTransportMode;

		public ZString InternalTransportMode => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, false);

		public ZString TransportModeName => declaration.ZG_Box18TransportID;

		public IDUAExportPartyProvider Exporter => CachedValueHelper.GetValue(ref exporter, () => DUAExportPartyWrapper.New(declaration.Supplier));
		CachedValue<IDUAExportPartyProvider> exporter;

		public IDUAExportPartyProvider Receiver => CachedValueHelper.GetValue(ref receiver, () => DUAExportImporterWrapper.New(declaration.Importer));
		CachedValue<IDUAExportPartyProvider> receiver;

		public ZString LocationId => declaration.ZG_AgreedPlaceCode;

		public ZBool IsDeclarationInEuros => true;

		public IReadOnlyCollection<IDUAExportLine> Lines => lines ?? (lines = LinesCore.ToArray());
		IReadOnlyCollection<IDUAExportLine> lines;

		protected virtual IEnumerable<IDUAExportLine> LinesCore => entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new DUAExportLineWrapper(x));

		public ZInt TotalNumberOfPackageElements => entryHeader.PackagesCount;

		ZString ExitCustomsOffice
		{
			get
			{
				if (customsOffice == null)
				{
					customsOffice = new CachedProperty<ZString>(declaration.Factory, () =>
					{
						var office = declaration.JE_CustomsOffice;
						foreach (EU.Business.EuOfficeCode cus in declaration.CustomsOffices)
						{
							if ((cus.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit || cus.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport) && !cus.CY_Data.IsEmpty)
							{
								office = cus.CY_Data;
								break;
							}
						}
						return office;
					});
				}
				return customsOffice.Value;
			}
		}
		CachedProperty<ZString> customsOffice;
	}
}
