using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ESCusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using GW = Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class ESDocSADHImport : ESDocSADH
	{
		ESDocSADHImport(ESCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{ }

		public static ESDocSADHImport New(ESCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new ESDocSADHImport(entryHeader, factoryToWrap);

		protected override DocSADHLineCollection GetLinesCore() => new ESDocSADHLineCollectionImport(EntryHeader.MergedLines, Factory);

		protected override DocSADHPageCollection GetPagesCore() => new ESDocSADHPageCollectionImport(EntryHeader.MergedLines, Factory);

		const string DocNumeration = "8";
		const string DecimalsFormat = "N2";
		const string MultipleSuppliersCode = "00200";

		protected override GW.OrganisationWrapper GetOrganisationWrapperForSupplier()
		{
			var suppliers = EntryHeader.InvoiceHeaders?.Select(x => x.Supplier).Distinct();
			var orgHeader = suppliers?.FirstOrDefault();
			isMultipleSuppliers = suppliers?.Skip(1)?.Any() ?? false;

			if (isMultipleSuppliers)
			{
				return new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignor, MultipleSuppliersCode, Factory);
			}
			else
			{
				var supplierAddress = EntryHeader.InvoiceHeaders?.FirstOrDefault()?.JZ_OA_SupplierAddress ?? ZGuid.Empty;
				var mainAddress = orgHeader?.MainAddress ?? Factory.GetNull<OrgAddress>();
				var addressToSend = supplierAddress.IsEmpty ? mainAddress : EntryHeader.Factory.Load<OrgAddress>(supplierAddress);
				return new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignor, addressToSend, Factory.GetNull<OrgContact>(), Factory);
			}
		}
		bool isMultipleSuppliers { get; set; }

		protected override ZString EPUCore => ZString.Empty;

		protected override ZString ENOCore => ZString.Empty;

		protected override ZDateTime DOECore => ZDateTime.Empty;

		protected override ZString LayoutStyle2Core => DocNumeration;

		protected override ZString AgentsReferenceCore => EntryHeader.ReferenceNumber;

		protected override ZString Box29ExitOfficeCore
		{
			get { return Declaration.GetCustomsOfficeFromList(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent); }
		}

		protected override ZString BoxDControlResultCore => IsEntryAcceptedByCustoms && !EntryHeader.CH_EntryReleaseDate.IsEmpty ? string.Format((NoResString)"Levante: {0}", EntryHeader.CH_EntryReleaseDate.ToString("dd-MM-yyyy")) : string.Empty;

		protected override ZString BoxDContainerSealsAffixedCore
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				result.Append(IsEntryAcceptedByCustoms && !EntryHeader.MovementReferenceNumberIssueDate.IsEmpty ? string.Format((NoResString)"Admitido: {0}", EntryHeader.MovementReferenceNumberIssueDate.ToString("dd-MM-yyyy")) : string.Empty);
				result.AppendLine();
				result.Append(IsEntryAcceptedByCustoms && !EntryHeader.CSVClearance.IsEmpty ? string.Format("C.S.V.: {0}", EntryHeader.CSVClearance) : string.Empty);
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		protected override ZBool ShowEpuEnoDoeLabelsTextCore => false;

		protected override ZString Box14DetailsFooterCore => Declaration.ZG_AuthPerDeclaration ? Box14AuthorizationCaption : ZString.Empty;

		protected override ZBool ShowDeclarantRepresentativeAddressCore => EntryHeader.ImporterEoriOfMainOffice != EntryHeader.RepresentativeOrDeclarantEoriOfMainOffice || Declaration.JE_DeclarantType != ESRepresentationTypeList.Codes._1Auto;

		protected override ZString Box14AlternativeTextCore => Box14SelfRepTypeCaption;

		protected override ZBool ShowBox18LabelTextCore => true;

		protected override ZString Box17CountryOfDestinationCodeCore => ZString.Empty;
		protected override ZString Box17ImporterStateCore => Declaration.ZG_DestinationState;

		protected override ZString Box20AgreedPlaceCode2Core => Declaration.ZG_AgreedPlaceCode;
		protected override ZString Box20AgreedPlaceCodeCore => ZString.Empty;

		protected override ZString Box27PortOfLoadingCore => ZString.Empty;

		protected override ZString Box28FinancialAndBankingDataLine2Core
		{
			get
			{
				var result = ZString.Empty;
				var statisticalValue = EntryHeader.MergedLines.Cast<CusEntryLine>().Sum(x => x.CL_StatisticalValue);
				if (statisticalValue != ZDecimal.Zero)
				{
					result = statisticalValue.ToString(DecimalsFormat);
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		const string canaryMethodOfPaymentText = "MP A.T.C.:";
		protected override ZString BoxBAccountingDetailsCore
		{
			get
			{
				var result = new ZStringBuilder();
				if (Declaration.DestinationStateIsCanaryIsland)
				{
					result.Append(string.Format("{0} {1}", canaryMethodOfPaymentText, ((JobComInvoiceLine)EntryHeader.RandomEntryLine.RandomLine).ZG_MethodOfPayment2));
				}
				foreach (ESGuarantee guarantee in Declaration.Guarantees.Where(guarantee => ((ESGuarantee)guarantee).PW_BondAmount > 0 && EntryHeader.EntryInstruction.PK == ((ESGuarantee)guarantee).EntryInstruction.PK))
				{
					result.Append(guarantee.PW_BondNumber);
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		protected override ZBool ShowBoxDLabelsTextCore => false;

		#region Captions
		protected override ZString DispatchOfficeTitleCaptionCore => Res.GetString("40174C34-96CB-4951-A3B8-BC7B4C5637F7", "A OFFICE OF DESTINATION");
		protected override ZString DispatchOfficeTitleBisPageCaptionCore => Res.GetString("9E9A67E5-C85D-4EA1-8812-8D79DC8CB7B5", "OFFICE OF DESTINATION");

		protected override ZString LayoutStyle2DescriptionCore => Res.GetString("B602A6F2-0E38-4E11-9327-18936DE307BE", "Copy for consignee");

		protected override ZString Box10LabelPart1CaptionCore => Res.GetString("7A8F4A2E-BFD9-46D6-80E7-85C237297A34", "Last Country/Region");
		protected override ZString Box10LabelPart2CaptionCore => Res.GetString("142E7258-087A-46C4-A606-CE4A65F46AFA", "Proc.");

		protected override ZString Box11LabelPart1CaptionCore => Res.GetString("B171D1BA-DD6D-442A-953F-84035F83AA80", "Trad. Country/Region/");
		protected override ZString Box11LabelPart2CaptionCore => Res.GetString("9B7E9A9B-48F0-41B3-8608-071A1A197FAE", "Prod.");

		protected override ZString Box18LabelCaptionCore => Res.GetString("E737B07E-3FD5-43C2-93CF-ECB5A45F16E8", "18 Identity and nationality of means of transport on arrival");

		protected override ZString Box27LabelCaptionCore => Res.GetString("A5F152F3-761C-473A-B059-F32F7332A35D", "27 Place of unloading");

		protected override ZString Box29ExitOfficeLabelCore => Res.GetString("D9836159-2D14-457D-BE05-6B686A325A00", "29 Office of entry");

		protected override ZString BoxDLabelCaptionCore => Res.GetString("F37D0F59-389F-4284-8E13-207DDA1DD020", "CONTROL BY OFFICE OF DESTINATION");

		protected override ZString BoxDLetterLabelCaptionCore => "J";

		protected override ZString Box31PackagesAndDescriptionOfGoodsCaptionCore => Res.GetString("0D1CE6F4-2F7C-43D6-8AD6-312CBBBED930", "Marks and numeration - Container(s) number(s) – Number and class");

		protected override ZString Box43LabelCaptionCore => Res.GetString("C9AB097F-3DD0-4467-8934-55860958FCF5", "Cod M.E.");

		ZString Box14AuthorizationCaption => Res.GetString("D529F919-44E0-4279-97B3-CB5323E073A4", "Authorization: O");
		ZString Box14SelfRepTypeCaption => Res.GetString("5E3C2BF4-570C-4C7E-A696-2026F970B067", "CONSIGNEE");

		protected override OrgHeader GetBox54OrgHeader => Declaration.Importer;

		#endregion
	}
}
