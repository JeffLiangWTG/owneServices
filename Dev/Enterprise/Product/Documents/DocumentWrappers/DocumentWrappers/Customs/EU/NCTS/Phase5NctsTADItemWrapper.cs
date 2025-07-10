using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class Phase5NctsTADItemWrapper : DocBaseWrapper
	{
		public static Phase5NctsTADItemWrapper New(NctsDepartureCargoDesc line, BusinessObjectFactory factory)
		{
			return new Phase5NctsTADItemWrapper(line, factory);
		}

		protected Phase5NctsTADItemWrapper(NctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
		{
			GoodsItem = Argument.NotNull(line, nameof(line));
			Argument.NotNull(factory, nameof(factory));
			Bill = line.Bill;
		}

		NctsDepartureCargoDesc GoodsItem { get; }
		NctsBill Bill { get; }

		public ZString GoodsItemNumber => GoodsItem.BY_LineNo.ToString();
		public ZString DeclarationGoodsItemNumber => GoodsItem.BY_DeclarationGoodsItemNumber.ToString();
		public ZString ConsignorAddress => GetConsignorAddress();
		public ZString ConsignorId => GetConsignorId();
		public ZString ConsigneeAddress => GetConsigneeAddress();
		public ZString ConsigneeId => GetConsigneeId();
		public ZString AdditionalSupplyChainActor => GetAdditionalSupplyChainActor();
		public ZString DescriptionOfGoods => GoodsItem.BY_Description;
		public ZString PreviousDocuments => GetPreviousDocuments();
		public ZString SupportingDocuments => GetSupportingDocuments();
		public ZString AdditionalReference => GetAdditionalRefernce();
		public ZString AdditionalInformation => GetAdditionalInformation();
		public ZString TransportDocuments => GetTransportDocuments();
		public ZString ReferenceNumberUCR => GoodsItem.BY_CommercialReferenceNumber;
		public ZString CommodityCode => $"{GoodsItem.BY_HarmonisedTariff.SubstringSafe(0, 6)} {GoodsItem.BY_HarmonisedTariff.SubstringSafe(6, 2)}";
		public ZString DeclarationType => GoodsItem.BY_Type;
		public ZString CountryOfDispatch => GoodsItem.BY_RN_NKCountryOfDispatch;
		public ZString CountryOfDestination => GoodsItem.BY_RN_NKCountryOfDestination;
		public ZString GrossMass => Tools.ConvertNumberToString(new ZDecimal(Utilities.Round(GoodsItem.GrossMassInKilograms, 6)));
		public ZString NetMass => Tools.ConvertNumberToString(new ZDecimal(Utilities.Round(GoodsItem.NetMassInKilograms, 6)));
		public ZString SupplementaryUnits => Tools.ConvertNumberToString(GoodsItem.BY_CustomsSecondQuantity);
		public ZString DepartureTransportMeans => GetDepartureTransportMeans();
		public ZString DangerousGoods => GetDangerousGoods();
		public ZString CusCode => GoodsItem.BY_CusC4Number;
		public ZString TransportCharges => !GoodsItem.BY_TransportChargesMethodOfPayment.IsEmpty ? GoodsItem.BY_TransportChargesMethodOfPayment : Bill.B0_TransportPaymentMethod;
		public ZString Packages => GetPackagesInformation();

		ZString GetConsignorId()
		{
			var result = GoodsItem.Consignor?.GetEuIdentificationNumber() ?? ZString.Empty;
			if(result.IsEmpty)
			{
				result = Bill.Consignor?.GetEuIdentificationNumber() ?? ZString.Empty;
			}
			return result.Trim();
		}

		public ZString GetConsignorAddress()
		{
			var result = GoodsItem.Consignor?.FormatOnThreeLines(addContact: true) ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = Bill.Consignor?.FormatOnThreeLines(addContact: true) ?? ZString.Empty;
			}
			return result.Trim();
		}

		ZString GetConsigneeId()
		{
			var result = GoodsItem.Consignee?.GetEuIdentificationNumber() ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = Bill.Consignee?.GetEuIdentificationNumber() ?? ZString.Empty;
			}
			return result.Trim();
		}

		public ZString GetConsigneeAddress()
		{
			var result = GoodsItem.Consignee?.FormatOnThreeLines(addContact: true) ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = Bill.Consignee?.FormatOnThreeLines(addContact: true) ?? ZString.Empty;
			}
			return result.Trim();
		}

		ZString GetAdditionalSupplyChainActor()
		{
			var stringsList = new List<string>();
			var cusSupplyChainActors = GoodsItem.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().ToArray();
			var sequenceNumber = 1;
			foreach (var chain in cusSupplyChainActors)
			{
				stringsList.Add(($"{sequenceNumber++},{chain.CFR_Code},{chain.CFR_Reference}"));
			}
			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetPreviousDocuments()
		{
			var stringsList = new List<string>();
			var previousDocuments = Bill.PreviousDocuments.OrderBy(x => x.CSI_LineNo).ToList<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>();
			previousDocuments.AddRange(GoodsItem.PreviousDocuments.OrderBy(x => x.CSI_LineNo).ToArray<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>());

			foreach (var document in previousDocuments)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append(document.CSI_Code);
				sb.Append(document.CSI_ReferenceNumber);
				sb.Append(document.CSI_ItemNumber.NullIfZero()?.ToString() ?? ZString.Empty);
				sb.Append(document.CSI_UnitOfQuantity2);
				sb.Append(document.CSI_Quantity2.NullIfZero()?.ToString() ?? ZString.Empty);
				sb.Append(document.CSI_UnitOfQuantity);
				sb.Append(document.CSI_Quantity.NullIfZero()?.ToString() ?? ZString.Empty);
				sb.Append(document.CSI_ReferenceNumber2);
				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma));
			}
			stringsList = stringsList.Distinct().ToList();

			for (var index = 0; index < stringsList.Count; ++index)
			{
				stringsList[index] = $"{index + 1},{stringsList[index]}";
			}

			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetSupportingDocuments()
		{
			var stringsList = new List<string>();
			var supportingDocuments = Bill.SupportingDocuments.OrderBy(x => x.CSI_LineNo).ToList();
			supportingDocuments.AddRange(GoodsItem.SupportingDocuments.OrderBy(x => x.CSI_LineNo));
			
			foreach (var document in supportingDocuments)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append(document.CSI_Code);
				sb.Append(document.CSI_ReferenceNumber);
				sb.Append(document.CSI_ItemNumber.NullIfZero()?.ToString() ?? ZString.Empty);
				sb.Append(document.CSI_ReferenceNumber2);
				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma));
			}
			stringsList = stringsList.Distinct().ToList();

			for (var index = 0; index < stringsList.Count; ++index)
			{
				stringsList[index] = $"{index + 1},{stringsList[index]}";
			}

			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetAdditionalRefernce()
		{
			var stringsList = new List<string>();
			var additionalDocuments = Bill.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).OrderBy(x => x.CSI_LineNo).ToList<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			additionalDocuments.AddRange(GoodsItem.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).OrderBy(x => x.CSI_LineNo).ToArray<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>());

			foreach (var document in additionalDocuments)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append(document.CSI_Code);
				sb.Append(document.CSI_ReferenceNumber);
				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma));
			}
			stringsList = stringsList.Distinct().ToList();

			for (var index = 0; index < stringsList.Count; ++index)
			{
				stringsList[index] = $"{index + 1},{stringsList[index]}";
			}
			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetAdditionalInformation()
		{
			var stringsList = new List<string>();
			var additionalInfos = Bill.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).OrderBy(x => x.CSI_LineNo).ToList<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			additionalInfos.AddRange(GoodsItem.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).OrderBy(x => x.CSI_LineNo).ToArray<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>());

			foreach (var document in additionalInfos)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append(document.CSI_Code);
				sb.Append(document.CSI_Description);
				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma));
			}
			stringsList = stringsList.Distinct().ToList();

			for (var index = 0; index < stringsList.Count; ++index)
			{
				stringsList[index] = $"{index + 1},{stringsList[index]}";
			}
			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetTransportDocuments()
		{
			var stringsList = new List<string>();
			var transportDocuments = Bill.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).OrderBy(x => x.CSI_LineNo).ToList<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			transportDocuments.AddRange(GoodsItem.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).OrderBy(x => x.CSI_LineNo).ToArray<Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>());

			foreach (var document in transportDocuments)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append(document.CSI_Code);
				sb.AppendIfNotEmpty(document.CSI_ReferenceNumber);
				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma));
			}
			stringsList = stringsList.Distinct().ToList();

			for (var index = 0; index < stringsList.Count; ++index)
			{
				stringsList[index] = $"{index + 1},{stringsList[index]}";
			}
			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetDepartureTransportMeans()
		{
			var stringsList = new List<string>();
			var sequenceNumber = 1;
			foreach (var dt in Bill.DepartureTransportInfos)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append((sequenceNumber++).ToString());
				sb.AppendIfNotEmpty(dt.TPM_TypeOfIdentification);
				sb.AppendIfNotEmpty(dt.TPM_IdentificationNumber);
				sb.AppendIfNotEmpty(dt.TPM_RN_NKTransportNationality);
				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma));
			}
			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetDangerousGoods()
		{
			var stringsList = new List<string>();
			var sequenceNumber = 1;
			foreach (var code in GoodsItem.UNDGs?.Where(d => !d.DI_DG.IsEmpty).Select(d => d.UNDGSubstance.DG_Code))
			{
				stringsList.Add($"{sequenceNumber++},{code}");
			}
			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}

		ZString GetPackagesInformation()
		{
			var stringsList = new List<string>();
			var sequenceNumber = 1;
			foreach (var package in GoodsItem.Packages)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append((sequenceNumber++).ToString());
				sb.Append(package.B5_UnitType);
				sb.Append(package.B5_UnitCount.ToString());
				sb.AppendIfNotEmpty(package.B5_MarksAndNumbers);
				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma));
			}
			return string.Join(DocumentWrapperConstants.Delimiters.SemiColonAndspace, stringsList);
		}
	}
}
