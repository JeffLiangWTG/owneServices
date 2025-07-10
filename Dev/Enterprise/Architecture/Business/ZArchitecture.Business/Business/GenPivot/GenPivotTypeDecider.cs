using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class GenPivotTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var application = row[GenPivotSchema.XX_RelationType.Name].ToString();
			if (application.EqualIgnoringOrder(Types.CusNctsContainer))
			{
				return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.EU.NCTS.ICusNctsContainerGenPivotTypeDecider>()).GetTypeForLoad(row, factory);
			}
			return GetType(application);
		}

		public static Type GetType(string application)
		{
			switch (application)
			{
				case Types.FDARelatedBillsGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IFDARelatedBillsGenPivot>();

				case Types.FDARelatedContainersGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IFDARelatedContainersGenPivot>();

				case Types.PGARelatedContainersGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IPGARelatedContainersGenPivot>();

				case Types.InvoiceRelatedDeclarationGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IInvoiceRelatedDeclarationGenPivot>();

				case Types.GroupRelatedDeclarationGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IGroupRelatedDeclarationGenPivot>();

				case Types.ABCEntryNumRelatedPacksGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.IABLEntryNumRelatedPacksGenPivot>();

				case Types.InvoiceLineRelatedControllingMessageHeaderPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IInvoiceLineRelatedCAHeadersGenPivot>();

				case Types.JobDecRelatedImportLicenseEntryGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IDeclarationRelatedImportLicenseEntryGenPivot>();

				case Types.RelatedEntryInstructionGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IRelatedEntryInstructionGenPivot>();

				case Types.NctsRelatedArrivalGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CH.INctsRelatedArrivalGenPivot>();

				case Types.NctsRelatedExportGenPivot:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CH.INctsRelatedExportEntryHeaderGenPivot>();

				case Types.AttachmentInvoiceLineLink:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IAttachmentInvoiceLineGenPivot>();
			}
			return typeof(GenPivot);
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public static class Types
		{
			public const string FDARelatedBillsGenPivot = "U1";
			public const string FDARelatedContainersGenPivot = "FD";
			public const string PGARelatedContainersGenPivot = "LA";
			public const string InvoiceRelatedDeclarationGenPivot = "ZE";
			public const string GroupRelatedDeclarationGenPivot = "GE";
			public const string ABCEntryNumRelatedPacksGenPivot = "AP";
			public const string CusBondDetailRelatedEntryInstructionPivot = "BEI";
			public const string FiscalReferenceRelatedEntryInstructionPivot = "FEI";
			public const string InvoiceLineRelatedControllingMessageHeaderPivot = "ICH";
			public const string InvoiceLineRelatedTrademarkImagePivot = "ITM";
			public const string JobDecRelatedImportLicenseEntryGenPivot = "LIC";
			public const string RelatedEntryInstructionGenPivot = "CEI";
			public const string NctsRelatedArrivalGenPivot = "AR";
			public const string CusNctsContainer = "NCT";
			public const string NctsRelatedExportGenPivot = "EXP";
			public const string AttachmentInvoiceLineLink = "ATH";
		}
	}
}
