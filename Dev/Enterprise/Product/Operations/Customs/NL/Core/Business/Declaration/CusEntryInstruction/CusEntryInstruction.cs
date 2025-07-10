using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryInstruction : AutoCusEntryInstruction, Integration.Customs.NL.ICusEntryInstruction
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.CusEntryInstruction.Schema
	{
		public const string ZG_TransNature = "ZG_TransNature";
	}

	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> CusAuthorizationUsages => (EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>)base.CusAuthorizationUsages;

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.CusEntryInstruction> GetCusAuthorizationUsages() => new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(this, Factory);

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);
	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

	public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(CEI_AddInfoInfo);

	internal new AddInfoCusEntryInstruction AddInfo => (AddInfoCusEntryInstruction)base.AddInfo;

	public new AddInfoCusEntryInstructionLookups AddInfoLookups => new(AddInfo);

	public new AddInfoCusEntryInstructionValidation AddInfoValidation => new(AddInfo);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
	{
		var result = base.GetCusSupportingInfoTypesCore();
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	public IEnumerable<CusEntryLineFee> AllEntryLineFees()
	{
		if (EntryHeader != null)
		{
			foreach (CusEntryLine line in EntryHeader.AllEntryLines)
			{
				foreach (CusEntryLineFee fee in line.Fees)
				{
					yield return fee;
				}
			}
		}
	}

	public IEnumerable<CusAuthorizationUsage> AllAuthorizationUsages()
	{
		foreach (var authorizationUsage in CusAuthorizationUsages)
		{
			yield return authorizationUsage;
		}

		foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>())
		{
			foreach (var authorizationUsage in invoiceLine.CusAuthorizationUsages)
			{
				yield return authorizationUsage;
			}
		}
	}

	public bool HasProcedureStartingWithAny(params ZString[] procedureCodes) => InvoiceLines.Any(invoiceLine => procedureCodes.Any(procedureCode => invoiceLine.JI_Procedure.StartsWith(procedureCode)));

	[ResourceStringData("060C06F4-ACE2-455D-91EE-4308D80644C9", Caption = "[37] CPC")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CPCList))]
	public override ZString CEI_Procedure
	{
		get => base.CEI_Procedure;
		set
		{
			base.CEI_Procedure = value;
			JobDeclaration?.MarkAsNeedingValidation();
			JobDeclaration?.InvoiceLines?.RefreshBinding();
		}
	}

	[ResourceStringData("3F6FBF18-130D-409C-BAE1-66EF5AAEF9F3", Caption = "Sub Style", FullDescription = "[UCC 1/2] Sub Style")]
	[ResourceStringData("3F6FBF18-130D-409C-BAE1-66EF5AAEF9F7", Caption = "Sub Style", FullDescription = "[UCC 1/2] Sub Style", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString CEI_SubStyle { get => base.CEI_SubStyle; set => base.CEI_SubStyle = value; }

	[ResourceStringData("FEFFFC45-96A1-465F-8C93-FDBA80B5CE5C", Caption = "Location of Goods", FullDescription = "[UCC 5/23] Location of Goods")]
	public override ZString GoodsLocationDescription { get => base.GoodsLocationDescription; }

	public override ZString CEI_Style
	{
		get => base.CEI_Style;
		set
		{
			base.CEI_Style = value;
			if (IsFiscalReferencesReadOnly)
			{
				FiscalReferences.RemoveAndDeleteAll();
			}
			else
			{
				FiscalReferences.RefreshBindingIncludingChildren();
			}

			if (!CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5 }))
			{
				ZG_IsHighValueOvrd = false;
			}
		}
	}

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);
	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	protected override EU.Business.CusGoodsLocation GetGoodsLocation() => (CusGoodsLocation)base.GetGoodsLocation();

	public new ICusFiscalReferenceCollection<CusFiscalReference> FiscalReferences
	{
		get
		{
			var fiscalReference = base.FiscalReferences;
			fiscalReference.SetCountedReadOnlyIncludingChildren(IsFiscalReferencesReadOnly);
			return fiscalReference;
		}
	}

	bool IsFiscalReferencesReadOnly => new ZString[] { NLConstants.EntryStyles.DeclarationForCustWarehouse, NLConstants.EntryStyles.ImportSpecialFiscalTerritoriesDeclaration }.Contains(CEI_Style);

	[ResourceStringData("183F67FE-2CCB-4444-82A5-511AB405C8C7", Caption = "Valuation Indicators?", ShortCaption = "V.I.?",
		FullDescription = "[14 07 000 000] Valuation Indicators formerly known as D.V.1 relationship information")]
	public override ZBool ZG_IsHighValueOvrd { get => base.ZG_IsHighValueOvrd; set => base.ZG_IsHighValueOvrd = value; }

	public bool HasEmptyTransactionNatureForMessage => AddInfo.ZG_TransNature.IsEmpty && CEI_Style.In(validDeclarationTypesForMessage);

	public bool HasNonEmptyTransactionNatureForMessage => !AddInfo.ZG_TransNature.IsEmpty && CEI_Style.In(validDeclarationTypesForMessage);

	public bool IsValidForMessage => CEI_Style.In(validDeclarationTypesForMessage);

	readonly ZString[] validDeclarationTypesForMessage = new ZString[] { DeclarationTypeList.Codes.B1, DeclarationTypeList.Codes.B2, DeclarationTypeList.Codes.C1, DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H3, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5, DeclarationTypeList.Codes.I1 };
}
