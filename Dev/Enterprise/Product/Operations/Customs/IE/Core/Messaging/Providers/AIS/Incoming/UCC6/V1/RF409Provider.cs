using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409;
using CargoWise.EntityFramework;
using CargoWise.Types;
using PartiesType = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409.PartiesType;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class RF409Provider
	{
		public RF409Provider(Rf409 xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly Rf409 xmlObject;

		public ZString ApplicationReferenceId => Header?.ApplicationReferenceId;
		public ZString ApplicationDecisionCodeType => Header?.ApplicationDecisionCodeType;
		public ZBool RefundApplicationAccepted => Extensions.IsTrueOrFalse(Header?.RefundApplicationAccepted);
		public ZString Signature => Header?.Signature;
		public ZString DecisionTakingCustomsAuthority => Header?.DecisionTakingCustomsAuthority;
		public ZString TotalNumberOfDocuments => Header?.TotalNumberOfDocuments;
		public ZString Applicant => Parties?.Applicant;
		public ZString RepresentativeIdentification => Parties?.RepresentativeIdentification;
		public ZString Date => DatesPlaces?.Date;
		public ZString OfficeOfDept => DatesPlaces?.OfficeOfDept;
		public ZString OfficeOfResponsibility => DatesPlaces?.OfficeOfResponsibility;
		public ZString MRN => xmlObject.Mrn;
		public ZString LegalBasisCode => LegalBasisCodes?.LegalBasis;

		public ZString CustomsProcedureCode => CustomsProcedure?.ProcedureCode;

		public ZDecimal AmountToBeRepaid => AmountOfDutiesToBeRepaid.Amount ?? ZDecimal.Zero;
		public ZString AmountToBeRepaidCurrency => AmountOfDutiesToBeRepaid.Currency ?? ZString.Empty;
		public ZString AmountOfDutiesToBeRemitted => $"{AmountToBeRepaidCurrency} {AmountToBeRepaid}";

		public ZString DestinationOfGoods => xmlObject.DestinationOfGoods;
		public ZString TimeLimit => xmlObject.TimeLimit;
		public ZString StatementOfTheDecision => xmlObject.StatementOfTheDecision;
		public ZString DescriptionOfGrounds => xmlObject.DescriptionOfGrounds;
		public IReadOnlyCollection<GoodsInformationProvider> GoodsInformations => goodsInformations ??= GetGoodsInformations();
		IReadOnlyCollection<GoodsInformationProvider> goodsInformations;
		public IReadOnlyCollection<TypeOfDutyProvider> TypeOfDuties => typeOfDuties ??= GetTypeOfDuties();
		IReadOnlyCollection<TypeOfDutyProvider> typeOfDuties;
		public IReadOnlyCollection<AttachedDocumentProvider> AttachedDocuments => attachedDocuments ??= GetAttachedDocuments();
		IReadOnlyCollection<AttachedDocumentProvider> attachedDocuments;
		public IReadOnlyCollection<GeneralRemarkProvider> GeneralRemarks => generalRemarks ??= GetGeneralRemarks();
		IReadOnlyCollection<GeneralRemarkProvider> generalRemarks;

		HeaderType Header => xmlObject.Header;
		PartiesType Parties => xmlObject.Parties;
		DatePlacesType DatesPlaces => xmlObject.DatesPlaces;
		LegalBasisCodeType LegalBasisCodes => xmlObject.LegalBasisCodes;
		CustomsProcedureType CustomsProcedure => xmlObject.CustomsProcedure;
		AmountOfDutiesToBeRepaidRf409Type AmountOfDutiesToBeRepaid => xmlObject.DutiesToBeRemitted.AmountOfDutiesToBeRepaid;
		IReadOnlyCollection<GoodsInformationProvider> GetGoodsInformations() => xmlObject.GoodsInformation?.Select(e => new GoodsInformationProvider(e.CustomsValue)).ToArray() ?? Array.Empty<GoodsInformationProvider>();
		IReadOnlyCollection<TypeOfDutyProvider> GetTypeOfDuties() => xmlObject.TypeOfDuty?.Select(e => new TypeOfDutyProvider(e.TypeOfDuty)).ToArray() ?? Array.Empty<TypeOfDutyProvider>();
		IReadOnlyCollection<AttachedDocumentProvider> GetAttachedDocuments() => xmlObject.AttachedDocuments.Select(e => new AttachedDocumentProvider(e)).ToArray() ?? Array.Empty<AttachedDocumentProvider>();
		IReadOnlyCollection<GeneralRemarkProvider> GetGeneralRemarks() => xmlObject.GeneralRemarks.Select(e => new GeneralRemarkProvider(e.GeneralRemarks)).ToArray() ?? Array.Empty<GeneralRemarkProvider>();
	}
}
