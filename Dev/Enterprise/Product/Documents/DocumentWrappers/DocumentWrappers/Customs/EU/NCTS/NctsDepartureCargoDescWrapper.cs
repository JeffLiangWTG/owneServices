using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class NctsDepartureCargoDescWrapper : DocBaseWrapper
	{
		protected NctsDepartureCargoDescWrapper(NctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
		{
			Line = Argument.NotNull(line, nameof(line));
		}

		public static NctsDepartureCargoDescWrapper New(NctsDepartureCargoDesc line, BusinessObjectFactory factory)
		{
			return new NctsDepartureCargoDescWrapper(line, factory);
		}

		public static NctsDepartureCargoDescWrapper New(NctsDepartureCargoDesc line)
		{
			return new NctsDepartureCargoDescWrapper(line, line.Factory);
		}

		public ZString BOX2CONSIGNOR => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.Consignor.GetWrappedAddressSummary());

		public ZString BOX2CONSIGNORSECURITY => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.ConsignorSecurity.GetWrappedAddressSummary());

		public ZString BOX7REFERENCE => ZString.Empty;

		public ZString BOX7UCR => DepartureGoodsItemWrapper.CommercialReferenceNumber;

		public ZString BOX8CONSIGNEE => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.Consignee.GetWrappedAddressSummary());

		public ZString BOX8CONSIGNEESECURITY => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.ConsigneeSecurity.GetWrappedAddressSummary());

		public ZString BOX1REGIME => Tools.ValueOrThreeDashes(GetBox1Regime());

		public ZString BOX15COUNTRYOFORIGIN => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.CountryOfDispatchExportCode);

		public ZString BOX17COUNTRYOFDESTINATION => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.CountryOfDestinationCode);

		public ZString BOX32ITEM => GetBox32ItemNumber();

		public ZString Box32ItemIndex { get; set; }

		protected virtual ZString GetBox32ItemNumber() => Box32ItemIndex.IsEmpty ? DepartureGoodsItemWrapper.DeclarationItemNumber.ToString() : Box32ItemIndex;

		public ZString BOX33COMMODITY => GetBox33Commodity();

		protected virtual ZString GetBox33Commodity() => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.CommodityCode);

		public ZString BOX35GROSSMASS => GetBox35GrossMass();

		public ZString BOX38NETTMASS => GetBox38NetMass();

		public ZString BOX40DOCUMENTS => GetPreviousDocuments();

		public ZString BOX44 => GetSupportingDocuments();

		public ZString BOX444UNDG => DepartureGoodsItemWrapper.UNDangerousGoodsCode;

		public ZString BOX441DOCSANDCERTS => GetBox44_1_DocsAndCerts();

		protected virtual ZString GetBox44_1_DocsAndCerts() => GetSupportingDocuments();

		public ZString BOX442SPECIALMENTIONS => GetSpecialMentions();

		public ZString BOX311MARKS => GetWrappedPackageMarks();

		public ZString BOX312NUMBERS => GetWrappedPackageNumbersAndKind();

		public ZString BOX313CONTAINERS => Containers;

		ZString Containers => CachedValueHelper.GetValue(ref containersCached, () => ZString.Join(", ", GetContainers().Where(x => !x.IsEmpty).ToArray()));
		CachedValue<ZString> containersCached;

		IReadOnlyCollection<ZString> GetContainers()
		{
			if (Line.IsPhase5)
			{
				return Line.Packages.Cast<NctsPackage>()
					.SelectMany(package => package.ContainersPivot.Containers.Where(x => x.BC_Mode != Core.Constants.ContainerModes.NonContainerised).Select(x => x.BC_ContainerNum))
					.ToHashSet();
			}

			return DepartureGoodsItemWrapper.Containers;
		}

		public ZString BOX314SENSITIVE => GetWrappedSgiCodes();

		public ZString BOX315SENSITIVE => GetWrappedSgiCodes();

		public ZString BOX315SENSITIVEQTY => GetWrappedSgiQuantities();

		public ZString BOX314DESCRIPTION => DepartureGoodsItemWrapper.GoodsDescription;

		public ZString BOX312DESCRIPTION => DepartureGoodsItemWrapper.GoodsDescription;

		public ZString BOX311MARKSANDNUMBERS => GetWrappedPackageMarksAndNumbers();

		public ZString BOXS29TRANSPORTCHARGESMOP => GetTransportChargesMethodOfPayment();

		public ZString BOXS28SEALS => Seals;

		ZString Seals => CachedValueHelper.GetValue(ref sealsCached, () => GetSeals());
		CachedValue<ZString> sealsCached;

		ZString GetSeals()
		{
			if (Line.MoveHeader?.Header is NctsHeader header)
			{
				var sealStringBuilder = new ZStringBuilder();
				var moveHeaderLinkedContainers = header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>()
				.Join(
					DepartureGoodsItemWrapper.Containers,
					c => c.BC_ContainerNum,
					goodsContainerNo => goodsContainerNo,
					(c, goodsContainerNo) => c);

				foreach (var container in moveHeaderLinkedContainers)
				{
					sealStringBuilder.AppendIfNotEmpty(container.BC_Seal1);
					sealStringBuilder.AppendIfNotEmpty(container.BC_Seal2);
					container.AdditionalSeals
						.OfType<CusSeal>()
						.Select(cusSeal => cusSeal.BK_SealNumber)
						.Where(sealNumber => !string.IsNullOrEmpty(sealNumber))
						.ToList()
						.ForEach(sealNumber => sealStringBuilder.AppendIfNotEmpty(sealNumber));
				}

				return sealStringBuilder.ToStringWithDelimiterBetweenAppends(", ");
			}

			return ZString.Empty;
		}

		void GatherSgiData()
		{
			if (!hasGatherSgiData)
			{
				hasGatherSgiData = true;
				var sgiQuantityDataBuilder = new ZStringBuilder();
				var sgiCodeDataBuilder = new ZStringBuilder();
				foreach (var sgi in Line.SgiCodes)
				{
					if (!sgi.Qty.IsEmpty)
					{
						sgiQuantityDataBuilder.Append(sgi.Qty.ToString());
					}
					sgiCodeDataBuilder.AppendIfNotEmpty(sgi.Code);
				}
				sgiQuantities = sgiQuantityDataBuilder.ToStringWithNewLineBetweenAppends();
				sgiCodes = sgiCodeDataBuilder.ToStringWithNewLineBetweenAppends();
			}
		}
		bool hasGatherSgiData;
		ZString GetWrappedSgiQuantities()
		{
			GatherSgiData();
			return sgiQuantities;
		}
		ZString sgiQuantities;

		ZString GetWrappedSgiCodes()
		{
			GatherSgiData();
			return sgiCodes;
		}
		ZString sgiCodes;

		void GatherPackageData()
		{
			if (!hasGatherPackageData)
			{
				hasGatherPackageData = true;
				var packageNumbersAndKindBuiler = new ZStringBuilder();
				var packageTypeList = GetPackageUnitTypeList;
				var packageMarksBuilder = new ZStringBuilder();
				foreach (var pack in DepartureGoodsItemWrapper.Packages)
				{
					if (!pack.NumberOfPackages.IsEmpty || !pack.KindOfPackages.IsEmpty)
					{
						packageNumbersAndKindBuiler.Append(pack.NumberOfPackages + " - " + packageTypeList.GetDescriptionFromCode(pack.KindOfPackages));
					}
					packageMarksBuilder.AppendIfNotEmpty(pack.MarksAndNumbersOfPackages);
				}
				packageNumbersAndKind = packageNumbersAndKindBuiler.ToStringWithNewLineBetweenAppends();
				packageMarks = packageMarksBuilder.ToStringWithNewLineBetweenAppends();
			}
		}
		bool hasGatherPackageData;

		protected virtual CodeDescriptionPairList GetPackageUnitTypeList => NctsPackageLookups.GetPackageUnitTypeList(Line.Factory);

		ZString GetWrappedPackageNumbersAndKind()
		{
			GatherPackageData();
			return packageNumbersAndKind;
		}
		ZString packageNumbersAndKind;

		ZString GetWrappedPackageMarks()
		{
			GatherPackageData();
			return packageMarks;
		}
		ZString packageMarks;

		ZString GetWrappedPackageMarksAndNumbers()
		{
			var isPhase5Departure = Line.IsPhase5Departure;

			if (!isPhase5Departure)
			{
				return BOX311MARKS + " " + BOX312NUMBERS;
			}

			var packageTypeList = GetPackageUnitTypeList;
			var packageMarksAndNumbersBuilder = new ZStringBuilder();

			foreach (var pack in DepartureGoodsItemWrapper.Packages.Select(x => (x.MarksAndNumbersOfPackages, x.NumberOfPackages, x.KindOfPackages)).Where(x => !x.MarksAndNumbersOfPackages.IsEmpty || !x.NumberOfPackages.IsEmpty || !x.KindOfPackages.IsEmpty))
			{
				packageMarksAndNumbersBuilder
					.Append(pack.MarksAndNumbersOfPackages)
					.Append(DocumentWrapperConstants.Delimiters.CommaAndSpace)
					.Append(pack.NumberOfPackages.ToString())
					.Append(DocumentWrapperConstants.Delimiters.Hyphen)
					.Append(packageTypeList.GetDescriptionFromCode(pack.KindOfPackages))
					.AppendLine();
			}

			return packageMarksAndNumbersBuilder.ToString();
		}

		ZString GetTransportChargesMethodOfPayment()
		{
			ZString methodOfPayment = DepartureGoodsItemWrapper.TransportChargesMethodOfPayment;

			if (methodOfPayment.IsEmpty)
			{
				methodOfPayment = Line.Bill?.B0_TransportPaymentMethod ?? ZString.Empty;
			}
			if (methodOfPayment.IsEmpty)
			{
				methodOfPayment = Line.MoveHeader?.BM_MethodOfPayment ?? ZString.Empty;
			}
			return Tools.ValueOrThreeDashes(methodOfPayment);
		}

		ZString GetSpecialMentions()
		{
			var sb = new ZStringBuilder();
			foreach (var sm in DepartureGoodsItemWrapper.SpecialMentions)
			{
				sb.Append(sm.Statement + (sm.StatementText.IsEmpty ? "" : "-" + sm.StatementText));
			}
			var isPhase5Departure = Line.IsPhase5Departure;

			if(isPhase5Departure)
			{
				var additionalSpecialMentions = Line.Bill?
				.AdditionalDocuments
				.Cast<NctsBillAdditionalDocument>()
				.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument)
				.ToList() ?? [];

				foreach (var additionalSplMention in additionalSpecialMentions)
				{
					sb.Append($"{additionalSplMention.CSI_Code}{(additionalSplMention.CSI_Description.IsEmpty ? "" : "-" + additionalSplMention.CSI_Description)}");
				}
			}

			return sb.ToStringWithDelimiterBetweenAppends("; ");
		}

		protected virtual ZString GetSupportingDocuments()
		{
			var supportingDocuments = GetAllSupportingDocuments();
			return new CompactProducedDocumentsCertificatesBuilder().GetProducedDocumentsCertificatesFormatted(supportingDocuments, Line.IsPhase5Departure);
		}

		protected IEnumerable<SupportingDocument> GetAllSupportingDocuments()
		{
			var isPhase5Departure = Line.IsPhase5Departure;
			var supportingDocuments = Line.SupportingDocuments.Cast<NctsSupportingDocument>();

			if (isPhase5Departure)
			{
				var headersSupportingDocuments = Line.Bill.SupportingDocuments.Cast<NctsSupportingDocument>();
				supportingDocuments = headersSupportingDocuments.Concat(supportingDocuments);
			}

			return supportingDocuments;
		}

		protected virtual ZString GetPreviousDocuments()
		{
			var previousDocuments = GetAllPreviousDocuments();
			return new CompactPreviousDocumentsBuilder().GetPreviousDocumentsFormatted(previousDocuments, Line.IsPhase5Departure);
		}

		protected IEnumerable<PreviousDocument> GetAllPreviousDocuments()
		{
			var isPhase5Departure = Line.IsPhase5Departure;
			var previousDocuments = Line.PreviousDocuments.Cast<PreviousDocument>();

			if (isPhase5Departure)
			{
				var billsPreviousDocuments = Line.Bill.PreviousDocuments.Cast<PreviousDocument>();
				previousDocuments = billsPreviousDocuments.Concat(previousDocuments);
			}
			return previousDocuments;
		}

		protected virtual ZString GetBox1Regime() => DepartureGoodsItemWrapper.TypeOfDeclaration;
		protected virtual ZString GetBox35GrossMass() => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.GrossMass);
		protected virtual ZString GetBox38NetMass() => Tools.ValueOrThreeDashes(DepartureGoodsItemWrapper.NetMass);

		protected ZString DataGroupingCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected NctsDepartureCargoDesc Line { get; }

		protected IDepartureGoodsItem DepartureGoodsItemWrapper => departureGoodsItemWrapper ??= GetDepartureGoodsItemWrapper(Line);
		protected virtual IDepartureGoodsItem GetDepartureGoodsItemWrapper(NctsDepartureCargoDesc line) => new DepartureGoodsItemWrapper(line);
		IDepartureGoodsItem departureGoodsItemWrapper;

		public ZInt DeclarationItemNumber => DepartureGoodsItemWrapper.DeclarationItemNumber;

		public ZInt LineNumber => DepartureGoodsItemWrapper.ItemNumber;

		public ZShort BillSequenceNumber => DepartureGoodsItemWrapper.BillSequenceNumber;
	}
}
