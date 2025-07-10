using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using GW = Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit;

public class T2LDocLineWrapper : DocBaseWrapper
{
	protected T2LDocLineWrapper(CusEntryLine entryLine, BusinessObjectFactory factory, ZBool isForT2LF) : base(entryLine, factory)
	{
		this.isForT2LF = isForT2LF;
	}
	readonly ZBool isForT2LF;

	public static T2LDocLineWrapper New(CusEntryLine entryLine, BusinessObjectFactory factory, bool isForT2LF) => entryLine == null ? null : new T2LDocLineWrapper(entryLine, factory, isForT2LF);

	public CusEntryLine EntryLine => (CusEntryLine)base.WrappedObject;

	JobDeclaration Declaration => EntryLine.Declaration;

	CusEntryHeader EntryHeader => EntryLine.Header;

	public ZString ExpeditionExportationCustomsFallBackNumber => FRCustomsDataRegistry.DeltaGFallbackIsActive ? EntryHeader.FRCustomsFallbackNumber : ZString.Empty;

	public ZString ExpeditionExportationCustomsOfficePlusDescription
	{
		get
		{
			var officeCode = Declaration.JE_CustomsOffice;
			var officeDesc = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, officeCode, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.ZZD_Description ?? string.Empty;

			return $"{officeCode} {officeDesc}";
		}
	}

	public ZString Box1DocType => isForT2LF ? T2LF : T2L;
	public static string T2LF => "T2LF";
	public static string T2L => "T2L";

	public ZString Box2SupplierEoriOfMainOffice => EntryHeader.SupplierEoriOfMainOffice;

	public GW.OrganisationWrapper Box2Supplier
	{
		get
		{
			GW.OrganisationWrapper result = null;
			if (Declaration.SupplierDocumentaryAddress != null)
			{
				result = new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignor, Declaration.SupplierDocumentaryAddress, Factory);
			}
			return result;
		}
	}

	public ZString Box5Articles => EntryHeader.MergedLinesCount.ToString();

	public ZString Box6PackageCount => EntryHeader.PackagesCount.ToString();

	public GW.OrganisationWrapper Box8Importer => Declaration.ImporterDocumentaryAddress != null ? new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignee, Declaration.ImporterDocumentaryAddress, Factory) : null;

	public ZString Box14DeclarantRepresentativeEori => EntryHeader.RepresentativeOrDeclarantEoriOfMainOffice;

	public DocAddress Box14DeclarantRepresentative => GetDeclarantRepresentative();

	public ZString Box14FooterText => $"N° agrément : {Declaration.JE_CustomsProfile}, Mode de représentation : {new RepresentationTypeList().GetDescriptionFromCode(Declaration.JE_DeclarantType)}";

	public ZString Box17FinalDestinationCountry => Declaration.FinalDestination?.Country?.Description ?? ZString.Empty;

	public ZString Box19HasContainer => Declaration.IsContainerised && EntryHeader.Containers.Any(x => !x.CO_ContainerNumber.IsEmpty) ? "1" : "0";

	public ZString Box21TransportNationality => Declaration.JE_RN_NKTransportNationality;

	public ZString Box25ModeOfTransportAtTheBorder => Declaration.ModeOfTransportAtTheBorder;

	public ZString Box31PacksMarksAndNumbersBuilder
	{
		get
		{
			var box31MisMash = new ZStringBuilder();

			box31MisMash.AppendIfNotEmpty(EntryLine.SadBox31PackagesPremable);

			box31MisMash.AppendIfNotEmpty(Box31_2DescriptionOfGoods);

			var conts = Box31_3ContainerNumbers;
			if (!conts.IsEmpty)
			{
				box31MisMash.AppendIfNotEmpty((NoResString)Box31_1NumberOfPackagesPiecesMarksAndNumbers + (NoResString)DocumentWrapperConstants.Delimiters.CarriageReturn + (NoResString)"Numéro conteneurs : " + conts);
			}
			else
			{
				box31MisMash.AppendIfNotEmpty((NoResString)Box31_1NumberOfPackagesPiecesMarksAndNumbers);
			}

			return box31MisMash.ToStringWithNewLineBetweenAppends();
		}
	}

	ZString Box31_1NumberOfPackagesPiecesMarksAndNumbers => new T2LEntryLinePacksMarksAndNumbersBuilder(EntryLine).Build();

	ZString Box31_2DescriptionOfGoods
	{
		get
		{
			return EntryLine.CL_Description.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Left(280);
		}
	}

	ZString Box31_3ContainerNumbers => string.Join(DocumentWrapperConstants.Delimiters.CommaAndSpace, ContainerList);

	IEnumerable<ZString> ContainerList => containerList ?? (containerList = EntryLine.Containers.ToArray());
	ZString[] containerList;

	public ZString Box32EntryLineNumber => EntryLine.CL_LineNumber.ToString();

	public ZString Box33Tariff => EntryLine.Tariff;

	public ZString Box35TotalInvoiceLinesGrossWeight => EntryLine.TotalInvoiceLinesGrossWeightInKG.Round(3).ToString(3);

	public ZString Box38CustomsQuantity => EntryLine.CustomsQuantity.Round(3).ToString(3);

	public ZString Box44AddInfoAndDocuments => Box44AddInfoAndDocumentsHelper.GetBox44AddInfoAndDocuments(EntryLine, false);

	Box44AddInfoAndDocumentsHelper Box44AddInfoAndDocumentsHelper => box44AddInfoAndDocumentsHelper ?? (box44AddInfoAndDocumentsHelper = new Box44AddInfoAndDocumentsHelper());
	Box44AddInfoAndDocumentsHelper box44AddInfoAndDocumentsHelper;

	public DocAddress Box50DeclarantRepresentative => GetDeclarantRepresentative();

	DocAddress GetDeclarantRepresentative() => ShowDeclarantRepresentativeAddress ? DocAddress.New(Declaration.Declarant, Factory) : null;

	public ZBool ShowDeclarantRepresentativeAddress => Declaration.JE_DeclarantType != EU.Business.RepresentationTypeList.Codes._1Self;

	public ZString Box50RepresentativeCusAgentFullName => Declaration.CusAgent?.GS_FullName ?? ZString.Empty;

	public ZString Box50RepresentativeCity => Declaration.DeclarantOrgAddress?.City ?? ZString.Empty;

	public ZString Box50BAEDate => EntryHeader.CH_EntryReleaseDate.ToString("dd/MM/yyyy");

	public ZString Box52GuaranteeSubType => Declaration.CustomsGuarantee?.CPH_SubType ?? ZString.Empty;

	public ZString Box54DeclarationHomePortDescriptionAndTodayDate
	{
		get
		{
			var homeport = Declaration.Branch.HomePort;
			var date = ZDateTime.Today.ToString("dd/MM/yyyy");
			var sb = new StringBuilder();
			if (homeport != null)
			{
				sb.AppendLine(homeport.Description);
			}
			sb.Append(date);
			return sb.ToString();
		}
	}

	public ZString Box54SignatoryNameAndPosition => staffForBox54Details == null ? string.Empty
														: staffForBox54Details.GS_FullName + (staffForBox54Details.GS_Title.IsEmpty ? string.Empty : " (" + staffForBox54Details.GS_Title + ")");

	GlbStaff staffForBox54Details => _staffForBox54Details ?? (_staffForBox54Details = GetStaffForBox54Details());
	GlbStaff _staffForBox54Details;

	GlbStaff GetStaffForBox54Details()
	{
		var lastOutgoingMessage = EntryHeader.Messages.LastOutgoingNonSystemAndNonNullUserMessage;
		var result = Declaration.CusAgent;
		if (lastOutgoingMessage != null && lastOutgoingMessage.UserWhoQueuedThisRecord != null)
		{
			result = lastOutgoingMessage.UserWhoQueuedThisRecord;
		}
		return result;
	}
}
