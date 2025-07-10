using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using static Enterprise.Customs.AE.Business.AEConstants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AE.Business;

public class CusEntryInstruction : AutoAECusEntryInstruction, Integration.Customs.AE.ICusEntryInstruction, ICusSupportingInfoTypeSupporter
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region DocumentAvailability
	[ChildEditable(true)]
	public DocumentAvailabilityCollection DocumentAvailability
	{
		get
		{
			if (documentAvailability == null)
			{
				documentAvailability = CreateNewDocumentAvailabilityCollection();
				documentAvailability.Load();
				RegisterEditableChildObject(documentAvailability);
			}
			return documentAvailability;
		}
	}
	DocumentAvailabilityCollection documentAvailability;

	protected DocumentAvailabilityCollection CreateNewDocumentAvailabilityCollection() => new DocumentAvailabilityCollection(this);
	#endregion

	[ResourceStringData("Enterprise.Customs.AE.Business.CusEntryInstruction|CEI_Style", Caption = "CPC")]
	public override ZString CEI_Style { get => base.CEI_Style; set => base.CEI_Style = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusEntryInstruction|CEI_DateForDuty", Caption = "Assessment Date")]
	public override ZDateTime CEI_DateForDuty { get => base.CEI_DateForDuty; set => base.CEI_DateForDuty = value; }

	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.DeclarationPurposeList))]
	[ResourceStringData("Enterprise.Customs.AE.Business.CusEntryInstruction|CEI_DeclarationPurpose", Caption = "Declaration Purpose", MediumCaption = "Dec. Purpose", ShortCaption = "Dec. Purp.")]
	public override ZString CEI_DeclarationPurpose
	{
		get => base.CEI_DeclarationPurpose;
		set
		{
			var oldValue = CEI_DeclarationPurpose;
			base.CEI_DeclarationPurpose = value;
			if (!IsCopying && oldValue != CEI_DeclarationPurpose)
			{
				CEI_DeclarationPurposeDetails = ZString.Empty;
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.AE.Business.CusEntryInstruction|CEI_DeclarationPurposeDetails", Caption = "Declaration Purpose Details", MediumCaption = "Dec. Purp. Details", ShortCaption = "Purp. Det.")]
	[ReadOnlyMember(nameof(CEI_DeclarationPurposeDetails_ReadOnly))]
	[MaxLength(Schema.CEI_DeclarationPurposeDetailsMaxLength)]
	public override ZString CEI_DeclarationPurposeDetails { get => base.CEI_DeclarationPurposeDetails; set => base.CEI_DeclarationPurposeDetails = value; }

	public ZBool CEI_DeclarationPurposeDetails_ReadOnly => !CEI_DeclarationPurpose.Equals(RefCusCodeList.Codes.DeclarationPurpose.Others);

	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.TradeTypeList))]
	[ResourceStringData("Enterprise.Customs.AE.Business.CusEntryInstruction|CEI_TradeType", Caption = "Trade Type")]
	public override ZString CEI_TradeType { get => base.CEI_TradeType; set => base.CEI_TradeType = value; }

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

	IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
	{
		var result = new Dictionary<ZString, Type>
		{
			{ AEConstants.CusSupportingInfoTypes.Codes.DocumentAvailability, typeof(DocumentAvailability) }
		};
		return result;
	}

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}
}
