using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ImportSADNumber : CusSupportingInfo
	{
		public ImportSADNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusSupportingInfo.Schema
		{
			public new const int CSI_DescriptionMaxLength = 21;
		}

		[MaxLength(Schema.CSI_DescriptionMaxLength)]
		[ReadOnlyMember(nameof(IsLinkingToReadOnlyEMCSParent))]
		[ResourceStringData("FD36B88B-C7AC-4233-87FB-DA5376E54A42", Caption = "Entry Number")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set
			{
				if (base.CSI_Description != value)
				{
					base.CSI_Description = value;
				}
			}
		}

		public EMCSJobDeclaration Declaration => (EMCSJobDeclaration)Parent;

		public override bool CanDelete => base.CanDelete && !IsLinkingToReadOnlyEMCSParent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ImportSad;
			CSI_Code = CusSupportingInfoTypeList.Codes.ImportSad;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new ImportSADNumberValidation(this);

		public ZBool IsLinkingToReadOnlyEMCSParent => Factory.GetValue(ref isLinkingToReadOnlyEMCSParent, () => CSI_ParentID.IsValid && Declaration.IsMessageStatusSentOrAcknowledged);
		CachedProperty<ZBool> isLinkingToReadOnlyEMCSParent;
	}
}
