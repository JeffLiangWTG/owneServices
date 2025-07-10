using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class GroupedPreviousDocument : NonPersistentBusinessObject
{
	public GroupedPreviousDocument(IMergedPreviousDocumentsProvider mergedPreviousDocumentsProvider, IMergedPreviousDocument summaryDeclarationDocument, IMergedPreviousDocument previousProcedureDocument, BusinessObjectFactory businessObjectFactory)
		: base(businessObjectFactory)
	{
		Argument.NotNull(businessObjectFactory, nameof(businessObjectFactory));
		if (summaryDeclarationDocument == null && previousProcedureDocument == null)
		{
			throw new ArgumentNullException(Res.GetString("C3DC6713-6A41-4056-A412-BB0BDE5FA92A", "Both merged previous document arguments cannot be null"));
		}
		this.mergedPreviousDocumentsProvider = Argument.NotNull(mergedPreviousDocumentsProvider, nameof(mergedPreviousDocumentsProvider));
		this.summaryDeclarationDocument = summaryDeclarationDocument;
		this.previousProcedureDocument = previousProcedureDocument;
	}

	readonly IMergedPreviousDocumentsProvider mergedPreviousDocumentsProvider;
	readonly IMergedPreviousDocument summaryDeclarationDocument;
	readonly IMergedPreviousDocument previousProcedureDocument;

	public static class Schema
	{
		public const string EntryLineNumber = "EntryLineNumber";
		public const string EntryLineCustomsStatusDescription = "EntryLineCustomsStatusDescription";

		public const string SummaryDeclarationDocumentRegister = "SummaryDeclarationDocumentRegister";
		public const string SummaryDeclarationDocumentReferenceNumber = "SummaryDeclarationDocumentReferenceNumber";
		public const string SummaryDeclarationDocumentReferenceCIN = "SummaryDeclarationDocumentReferenceCIN";
		public const string SummaryDeclarationDocumentDate = "SummaryDeclarationDocumentDate";
		public const string SummaryDeclarationDocumentSeries = "SummaryDeclarationDocumentSeries";
		public const string SummaryDeclarationDocumentCustomsOffice = "SummaryDeclarationDocumentCustomsOffice";
		public const string SummaryDeclarationDocumentItemNumber = "SummaryDeclarationDocumentItemNumber";
		public const string SummaryDeclarationDocumentMRN = "SummaryDeclarationDocumentMRN";

		public const string PreviousProcedureDocumentRegister = "PreviousProcedureDocumentRegister";
		public const string PreviousProcedureDocumentReferenceNumber = "PreviousProcedureDocumentReferenceNumber";
		public const string PreviousProcedureDocumentReferenceCIN = "PreviousProcedureDocumentReferenceCIN";
		public const string PreviousProcedureDocumentDate = "PreviousProcedureDocumentDate";
		public const string PreviousProcedureDocumentSeries = "PreviousProcedureDocumentSeries";
		public const string PreviousProcedureDocumentCustomsOffice = "PreviousProcedureDocumentCustomsOffice";
		public const string PreviousProcedureDocumentItemNumber = "PreviousProcedureDocumentItemNumber";

		public const string PackageQuantity = "PackageQuantity";
		public const string GrossMass = "GrossMass";
		public const string NetMass = "NetMass";
		public const string SupplementaryQuantity = "SupplementaryQuantity";
		public const string Tariff = "Tariff";
	}

	#region Properties

	public ZInt EntryLineNumber => mergedPreviousDocumentsProvider.LineNumber;
	public ZPropertyInfo EntryLineNumberInfo => GetZPropertyInfo(Schema.EntryLineNumber);

	public ZString EntryLineCustomsStatusDescription => new EntryLineCustomsStatusList().GetDescriptionFromCode(mergedPreviousDocumentsProvider.NBStatus);
	public ZPropertyInfo EntryLineCustomsStatusDescriptionInfo => GetZPropertyInfo(Schema.EntryLineCustomsStatusDescription);

	#region Previous Summary Declaration

	public ZString SummaryDeclarationDocumentRegister => summaryDeclarationDocument?.Register ?? ZString.Empty;
	public ZPropertyInfo SummaryDeclarationDocumentRegisterInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentRegister);

	public ZString SummaryDeclarationDocumentReferenceNumber => summaryDeclarationDocument?.ReferenceNumber ?? ZString.Empty;
	public ZPropertyInfo SummaryDeclarationDocumentReferenceNumberInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentReferenceNumber);

	public ZString SummaryDeclarationDocumentReferenceCIN => summaryDeclarationDocument?.ReferenceNumberCin ?? ZString.Empty;
	public ZPropertyInfo SummaryDeclarationDocumentReferenceCINInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentReferenceCIN);

	public ZDateTime SummaryDeclarationDocumentDate => summaryDeclarationDocument?.Date ?? ZDateTime.Empty;
	public ZPropertyInfo SummaryDeclarationDocumentDateInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentDate);

	public ZString SummaryDeclarationDocumentSeries => summaryDeclarationDocument?.Series ?? ZString.Empty;
	public ZPropertyInfo SummaryDeclarationDocumentSeriesInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentSeries);

	public ZString SummaryDeclarationDocumentCustomsOffice => summaryDeclarationDocument?.CustomsOffice ?? ZString.Empty;
	public ZPropertyInfo SummaryDeclarationDocumentCustomsOfficeInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentCustomsOffice);

	public ZInt SummaryDeclarationDocumentItemNumber => summaryDeclarationDocument?.ItemNumber ?? ZInt.Zero;
	public ZPropertyInfo SummaryDeclarationDocumentItemNumberInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentItemNumber);

	public ZString SummaryDeclarationDocumentMRN => summaryDeclarationDocument?.Mrn ?? ZString.Empty;
	public ZPropertyInfo SummaryDeclarationDocumentMRNInfo => GetZPropertyInfo(Schema.SummaryDeclarationDocumentMRN);

	#endregion

	#region Previous Procedure

	public ZString PreviousProcedureDocumentRegister => previousProcedureDocument?.Register ?? ZString.Empty;
	public ZPropertyInfo PreviousProcedureDocumentRegisterInfo => GetZPropertyInfo(Schema.PreviousProcedureDocumentRegister);

	public ZString PreviousProcedureDocumentReferenceNumber => previousProcedureDocument?.ReferenceNumber ?? ZString.Empty;
	public ZPropertyInfo PreviousProcedureDocumentReferenceNumberInfo => GetZPropertyInfo(Schema.PreviousProcedureDocumentReferenceNumber);

	public ZString PreviousProcedureDocumentReferenceCIN => previousProcedureDocument?.ReferenceNumberCin ?? ZString.Empty;
	public ZPropertyInfo PreviousProcedureDocumentReferenceCINInfo => GetZPropertyInfo(Schema.PreviousProcedureDocumentReferenceCIN);

	public ZDateTime PreviousProcedureDocumentDate => previousProcedureDocument?.Date ?? ZDateTime.Empty;
	public ZPropertyInfo PreviousProcedureDocumentDateInfo => GetZPropertyInfo(Schema.PreviousProcedureDocumentDate);

	public ZString PreviousProcedureDocumentSeries => previousProcedureDocument?.Series ?? ZString.Empty;
	public ZPropertyInfo PreviousProcedureDocumentSeriesInfo => GetZPropertyInfo(Schema.PreviousProcedureDocumentSeries);

	public ZString PreviousProcedureDocumentCustomsOffice => previousProcedureDocument?.CustomsOffice ?? ZString.Empty;
	public ZPropertyInfo PreviousProcedureDocumentCustomsOfficeInfo => GetZPropertyInfo(Schema.PreviousProcedureDocumentCustomsOffice);

	public ZInt PreviousProcedureDocumentItemNumber => previousProcedureDocument?.ItemNumber ?? ZInt.Zero;
	public ZPropertyInfo PreviousProcedureDocumentItemNumberInfo => GetZPropertyInfo(Schema.PreviousProcedureDocumentItemNumber);

	#endregion

	public ZInt PackageQuantity => summaryDeclarationDocument?.PackageQuantity ?? previousProcedureDocument.PackageQuantity;
	public ZPropertyInfo PackageQuantityInfo => GetZPropertyInfo(Schema.PackageQuantity);
	public ZDecimal GrossMass => summaryDeclarationDocument?.GrossMass ?? previousProcedureDocument.GrossMass;
	public ZPropertyInfo GrossMassInfo => GetZPropertyInfo(Schema.GrossMass);
	public ZDecimal NetMass => previousProcedureDocument?.NetMass ?? summaryDeclarationDocument.NetMass;
	public ZPropertyInfo NetMassInfo => GetZPropertyInfo(Schema.NetMass);
	public ZDecimal SupplementaryQuantity => previousProcedureDocument?.SupplementaryQuantity ?? summaryDeclarationDocument.SupplementaryQuantity;
	public ZPropertyInfo SupplementaryQuantityInfo => GetZPropertyInfo(Schema.SupplementaryQuantity);
	public ZString Tariff => previousProcedureDocument?.Tariff ?? summaryDeclarationDocument.Tariff;
	public ZPropertyInfo TariffInfo => GetZPropertyInfo(Schema.Tariff);

	#endregion
}
