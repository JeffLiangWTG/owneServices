using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class MergedPreviousDocument : IMergedPreviousDocument
{
	MergedPreviousDocument()
	{
	}

	public ZString CSI_Code { get; set; }
	public ZDateTime CSI_DateOfIssue { get; set; }
	public ZString CSI_ReferenceNumber { get; set; }
	public ZString ReferenceNumberCin { get; set; }
	public ZString ReferenceNumberWithoutCin { get; set; }
	public ZString CSI_SubType { get; set; }
	public ZString CSI_Status { get; set; }
	public ZString CSI_CustomsOffice { get; set; }
	public ZInt CSI_LineNo { get; set; }
	public ZString CSI_Procedure { get; set; }
	public ZString CSI_ReferenceNumber2 { get; set; }
	public ZString CSI_Tariff { get; set; }
	public ZString CSI_UnitOfQuantity2 { get; set; }
	public ZDecimal NetMass { get; set; }
	public ZDecimal SupplementaryQuantity { get; set; }
	public ZDecimal GrossMass { get; set; }
	public ZInt PackageQuantity { get; set; }
	public ZString Key => CSI_Code + CSI_DateOfIssue.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + CSI_ReferenceNumber + CSI_SubType + CSI_Status + CSI_CustomsOffice + CSI_LineNo + CSI_Procedure + CSI_ReferenceNumber2 + CSI_Tariff + CSI_UnitOfQuantity2;
	public ZBool IsSummaryDeclarationDocument => CustomsRulesProvider.IsSummaryDeclarationDocument(CSI_Procedure);
	public ZBool IsPreviousProcedureDocument => CustomsRulesProvider.IsPreviousProcedureDocument(CSI_Procedure);
	public ZDecimal CSI_Quantity;
	public ZDecimal CSI_Quantity2;
	public ZDecimal CSI_Quantity3;

	public static MergedPreviousDocument FromPreviousDocumentButQuantities(PreviousDocument previousDocument)
	{
		Argument.NotNull(previousDocument, nameof(previousDocument));
		var mergedPreviousDocument = new MergedPreviousDocument
		{
			CSI_Code = previousDocument.CSI_Code,
			CSI_DateOfIssue = previousDocument.CSI_DateOfIssue,
			CSI_ReferenceNumber = previousDocument.CSI_ReferenceNumber,
			ReferenceNumberCin = previousDocument.ReferenceNumberProvider.ReferenceNumberCin,
			ReferenceNumberWithoutCin = previousDocument.ReferenceNumberProvider.ReferenceNumberWithoutCin,
			CSI_SubType = previousDocument.CSI_SubType,
			CSI_Status = previousDocument.CSI_Status,
			CSI_CustomsOffice = previousDocument.CSI_CustomsOffice,
			CSI_LineNo = previousDocument.CSI_LineNo,
			CSI_Procedure = previousDocument.CSI_Procedure,
			CSI_ReferenceNumber2 = previousDocument.CSI_ReferenceNumber2,
			CSI_Tariff = previousDocument.CSI_Tariff,
			CSI_UnitOfQuantity2 = previousDocument.CSI_UnitOfQuantity2
		};
		return mergedPreviousDocument;
	}

	public static MergedPreviousDocument FromPreviousDocument(PreviousDocument previousDocument)
	{
		Argument.NotNull(previousDocument, nameof(previousDocument));
		var mergedPreviousDocument = FromPreviousDocumentButQuantities(previousDocument);
		mergedPreviousDocument.NetMass = previousDocument.EffectiveNetMass.InKilogramsSafe;
		mergedPreviousDocument.SupplementaryQuantity = previousDocument.CSI_Quantity2;
		mergedPreviousDocument.GrossMass = previousDocument.EffectiveGrossMass.InKilogramsSafe;
		mergedPreviousDocument.PackageQuantity = previousDocument.CSI_PackQty;
		return mergedPreviousDocument;
	}

	#region IMergedPreviousDocument Members

	ZString IMergedPreviousDocument.Type => CSI_SubType;

	ZString IMergedPreviousDocument.Category => CSI_Code;

	ZString IMergedPreviousDocument.Mrn => CSI_ReferenceNumber2;

	ZString IMergedPreviousDocument.Register => CSI_Procedure;

	ZString IMergedPreviousDocument.ReferenceNumber => ReferenceNumberWithoutCin;

	ZString IMergedPreviousDocument.ReferenceNumberCin => ReferenceNumberCin;

	ZDate IMergedPreviousDocument.Date => (ZDate)CSI_DateOfIssue;

	ZString IMergedPreviousDocument.Series => CSI_Status;

	ZString IMergedPreviousDocument.CustomsOffice => CSI_CustomsOffice;

	ZInt IMergedPreviousDocument.ItemNumber => CSI_LineNo;

	ZBool IMergedPreviousDocument.IsSummaryDeclarationDocument => IsSummaryDeclarationDocument;

	ZBool IMergedPreviousDocument.IsPreviousProcedureDocument => IsPreviousProcedureDocument;

	ZInt IMergedPreviousDocument.PackageQuantity => PackageQuantity;

	ZDecimal IMergedPreviousDocument.GrossMass => GrossMass;

	ZDecimal IMergedPreviousDocument.NetMass => NetMass;

	ZDecimal IMergedPreviousDocument.SupplementaryQuantity => SupplementaryQuantity;

	ZString IMergedPreviousDocument.Tariff => CSI_Tariff;

	#endregion
}
