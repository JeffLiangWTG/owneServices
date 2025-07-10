using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business.Declaration;

public class ReadOnlySupportingDocument : EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument
{
	public ReadOnlySupportingDocument(SupportingDocument supportingDocument)
		: base(supportingDocument)
	{
		this.supportingDoc = supportingDocument;
	}

	public ReadOnlySupportingDocument(SupportingDocument supportingDocument, CusEntryHeader header)
	: base(supportingDocument)
	{
		this.supportingDoc = supportingDocument;
		this.entryHeader = header;
	}
	readonly SupportingDocument supportingDoc;
	readonly CusEntryHeader entryHeader;

	[ResourceStringData("E5FD144D-A5E8-4DD5-9071-F5C035488EA3", Caption = "Status")]
	public override ZString CSI_Status => base.CSI_Status;

	[ResourceStringData("86495166-22F1-4383-B698-30EF87D52CAC", Caption = "Procedure (DJP)")]
	public override ZString CSI_Procedure => base.CSI_Procedure;

	[ResourceStringData("6F4995D4-90FE-47C8-A0B1-9907AE605A02", Caption = "Issuing Authority", ShortCaption = "Issuing Authority")]
	public override ZString CSI_AdditionalDescription => base.CSI_AdditionalDescription;

	[ResourceStringData("EF424A5A-7AA0-4262-A65F-165B74DD1A01", Caption = "Line No.", ShortCaption = "Line No.")]
	public override ZInt CSI_ItemNumber => base.CSI_ItemNumber;

	[ResourceStringData("FA81F75F-787E-437E-BAED-AB1321DE4D83", Caption = "Header")]
	public override ZBool IsDocumentHeader => ParentTableIsHeaderType(entryHeader, supportingDoc.CSI_ParentTableCode);

	static ZBool ParentTableIsHeaderType(CusEntryHeader entryHeader, ZString parentTab)
	{
		var isDocumentHeader = false;
		if (entryHeader.IsT2LorT2C && entryHeader.IsStyleEmpty)
		{
			isDocumentHeader = TypeParentTableCode(parentTab);
		}
		else if (entryHeader.IsImport)
		{
			isDocumentHeader = ParentTableIsHeaderTypeImport(entryHeader, parentTab);
		}
		else
		{
			isDocumentHeader = ParentTableIsHeaderTypeExport(entryHeader, parentTab);
		}
		return isDocumentHeader;
	}

	static ZBool ParentTableIsHeaderTypeImport(CusEntryHeader entryHeader, ZString parentTab)
	{
		var isDocumentHeader = false;
		if (entryHeader.IsH2Style || entryHeader.Declaration.IsUCC6)
		{
			isDocumentHeader = TypeParentTableCode(parentTab);
		}
		return isDocumentHeader;
	}

	static ZBool ParentTableIsHeaderTypeExport(CusEntryHeader entryHeader, ZString parentTab)
	{
		var isDocumentHeader = false;
		if (!entryHeader.IsT2LorT2CorEXS && !entryHeader.Declaration.IsTransitionPeriodAES30)
		{
			isDocumentHeader = TypeParentTableCode(parentTab);
		}
		return isDocumentHeader;
	}

	static ZBool TypeParentTableCode(ZString parentTab)
	{
		var isDocumentHeader = (string)parentTab switch
		{
			ParentTableCodeCusEntryInstruction or ParentTableCodeJobDeclaration or ParentTableCodeCH => true,
			_ => false,
		};
		return isDocumentHeader;
	}

	const string ParentTableCodeCusEntryInstruction = "CEI";
	const string ParentTableCodeJobDeclaration = "JE";
	const string ParentTableCodeCH = "CH";
}
