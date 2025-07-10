using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ITCusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using ITCusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.IT.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class ITDocSADH : DocSADH
{
	protected ITDocSADH(ITCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
		: base(entryHeader, factoryToWrap)
	{ }

	public static ITDocSADH New(ITCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new ITDocSADH(entryHeader, factoryToWrap);

	public new ITCusEntryHeader EntryHeader => (ITCusEntryHeader)base.EntryHeader;

	protected override DocSADHLineCollection GetLinesCore() => new ITDocSADHLineCollection(EntryHeader.MergedLines, Factory);

	protected override DocSADHPageCollection GetPagesCore() => new ITDocSADHPageCollection(EntryHeader.MergedLines, Factory);

	public ZString RegistrationInfo
	{
		get
		{
			var registrationInfoEntryNumber = EntryHeader.EntryNumbersProvider.RegistrationInfo;
			if (registrationInfoEntryNumber is null)
			{
				return ZString.Empty;
			}

			return new ZStringBuilder()
				.Append(registrationInfoEntryNumber.CE_EntryNum)
				.Append((NoResString)" del ")
				.Append(registrationInfoEntryNumber.CE_IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture))
				.ToString();
		}
	}

	protected override ZString Box12ValueDetailsCore => EntryHeader.CH_FreightAdjustment.ToString();

	protected override IBox16CountryOfOriginEvaluator GetBox16CountryOfOriginEvaluator()
	{
		var entryHeader = EntryHeader;
		if (entryHeader.Declaration.IsExport)
		{
			return new EmptyBox16CountryOfOriginEvaluator();
		}
		return new SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(entryHeader);
	}

	protected override ZString Box17ImporterStateCore => new DeclarationCountryStatesRWCodeBox17bEvaluator(Declaration).Evaluate();

	protected override ZString Box28FinancialAndBankingDataLine1Core => EntryHeader?.EntryInstruction?.FinancialAndBankingDataLine1 ?? ZString.Empty;

	protected override ZString Box28FinancialAndBankingDataLine2Core => EntryHeader?.EntryInstruction?.FinancialAndBankingDataLine2 ?? ZString.Empty;

	protected override ZString AgentsReferenceCore => Declaration.JE_AgentsReference;

	protected override ZString Box30LocationOfGoodsCore => GetBox30LocationOfGoods();

	ZString GetBox30LocationOfGoods()
	{
		if (Declaration.IsUCC6AndIsExport)
		{
			var goodsLocation = Declaration.GoodsLocation;
			switch (goodsLocation.CGL_Qualifier)
			{
				case CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier:
					return $"{CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier}-{goodsLocation.CGL_Type}-{goodsLocation.CGL_CustomsOffice}";

				case CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
					return $"{CusGoodsLocationQualifierList.Codes.AuthorizationNumber}-{goodsLocation.CGL_Type}-{goodsLocation.CGL_AdditionalIdentifier}";

				case CusGoodsLocationQualifierList.Codes.Address:
					return $"{CusGoodsLocationQualifierList.Codes.Address}-{goodsLocation.CGL_Type}-{goodsLocation.Address.E2_City}";
			}
		}
		return EntryHeader.LocationOfGoods;
	}

	protected override ZString Box35GrossWeightInKGCore => IsDeclarationExport ? Box35GrossWeightInKGForEXPDeclaration : base.Box35GrossWeightInKGCore;

	ZString Box35GrossWeightInKGForEXPDeclaration => new IT5DecimalsWeightFormatter(EntryHeader.TotalGrossWeightInKG).GetFormattedValue();

	protected override ZString BoxABarcodeCore => EntryHeader.MovementReferenceNumber;

	protected override string IssuingDateFormat => "dd/MM/yyyy";

	protected override ZString BoxDControlResultCore
	{
		get
		{
			var releaseCode = EntryHeader.EntryNumbersProvider.ReleaseInfo?.CE_EntryNum ?? ZString.Empty;
			return releaseCode.IsEmpty ? releaseCode : (ZString)FormattableString.Invariant($"Dichiarazione considerata conforme - Codice di svincolo: {releaseCode}");
		}
	}

	protected override ZString BoxDSignatureCore => (NoResString)"Trasmissione telematica ai sensi dell'art.6 p.1 del Reg.UE 952/13";

	public ITDocSADHLineCollection LinesWithAttachment => linesWithAttachment ?? (linesWithAttachment = LoadLinesWithAttachment());
	ITDocSADHLineCollection linesWithAttachment;

	ITDocSADHLineCollection LoadLinesWithAttachment() => new ITDocSADHLineCollection(EntryHeader.MergedLines.Cast<ITCusEntryLine>().Where(x => x.AttachmentPrintingSupporter.RequiresAttachment), Factory);

	protected override ZString LayoutStyle2Core
	{
		get
		{
			var copyNumber = SadDocumentSupporter?.LayoutStyle ?? ZString.Empty;
			return copyNumber.IsEmpty ? base.LayoutStyle2Core : copyNumber;
		}
	}

	protected override ZString LayoutStyle2DescriptionCore
	{
		get
		{
			if (SadDocumentSupporter != null)
			{
				var copyNumber = SadDocumentSupporter.LayoutStyle;
				if (!copyNumber.IsEmpty)
				{
					return SadDocumentSupporter.Lookups.LayoutStyleList.GetDescriptionFromCode(copyNumber);
				}
			}
			return base.LayoutStyle2DescriptionCore;
		}
	}

	protected override ZString BoxBAccountingDetailsCore => boxBAccountingDetails ?? (boxBAccountingDetails = GetBoxBAccountingDetails());
	string boxBAccountingDetails;

	protected override ZString BoxCOfficeOfDepartureCore => boxCOfficeOfDeparture ?? (boxCOfficeOfDeparture = GetBoxCOfficeOfDeparture());
	string boxCOfficeOfDeparture;

	protected override ZString BoxCCore => IsDeclarationExport ? Declaration.JE_CustomsOffice : base.BoxCCore;

	JobDeclarationSadDocumentSupporter SadDocumentSupporter => EntryHeader?.Declaration?.DocumentSupporter?.SadDocumentSupporter;

	ZString GetBoxBAccountingDetails() => new ITDocSADHBoxBAccountingDetailsBuilder(EntryHeader).GetAccountingDetails();

	protected override ZString Box18IdentityOfTransportAtDepartureCore => new ITBox18IdentityOfTransportAtDepartureBuilder(Declaration).GetBox18IdentityOfTransportAtDepartureFormatted();

	protected override ZString Box18TransportNationalityAtDepartureCore => Declaration.IsExport ? base.Box18TransportNationalityAtDepartureCore : Declaration.ZG_Box18TransportNationality;

	ZString GetBoxCOfficeOfDeparture()
	{
		if (IsDeclarationExport)
		{
			return new ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder(EntryHeader).GetOfficeOfDepartureAndRegistrationData();
		}
		return ZString.Empty;
	}

	ZBool IsDeclarationExport => Declaration.IsExport;

	protected override ZBool ShouldShowNotInEcsCaptionIfNeededCore => ZBool.False;

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override ZString BoxS29TransportChargesMoPCore => ZString.Empty;
	protected override IBisPageTraderBox GetBisPageTraderBox() => Declaration.IsExport ? new SupplierBisPageTraderBox(this) : base.GetBisPageTraderBox();

	protected override GlbStaff GetStaffForBox54Details() => null;

	protected override ZString Box54SignatoryContactDetailsCore => ZString.Empty;

	protected override ZBool ShowEpuEnoDoeLabelsTextCore => false;

	protected override ZBool ShowEpuEnoDoeLabelsTextOnBISCore => false;

	protected override ZString Box54DateCore => FormatDateDefault(EntryHeader.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty);

	public override ZString Box17CountryOfDestination => GetBox17CountryOfDestination();

	protected override DocAddress Box14DeclarantRepresentativeCore => GetBox14DeclarantRepresentative();

	DocAddress GetBox14DeclarantRepresentative()
	{
		var factory = Factory;
		var declaration = Declaration;

		if (!declaration.IsUCC6)
		{
			return base.Box14DeclarantRepresentativeCore;
		}

		if (declaration.Representative is OrgAddress representative
			&& !representative.Header.GetEoriDetails().IsEmpty)
		{
			return DocAddress.New(representative, factory);
		}

		if (declaration.Declarant is OrgAddress declarant
			&& !declarant.Header.GetEoriDetails().IsEmpty)
		{
			return DocAddress.New(declarant, factory);
		}

		return null;
	}

	ZString GetBox17CountryOfDestination()
	{
		var declaration = Declaration;
		if (declaration.Lookups?.GoodsDestination is CodeDescriptionPairList countryOfDestinationCollection)
		{
			return countryOfDestinationCollection.GetDescriptionFromCode(declaration.JE_GoodsDestination);
		}

		return ZString.Empty;
	}
}
