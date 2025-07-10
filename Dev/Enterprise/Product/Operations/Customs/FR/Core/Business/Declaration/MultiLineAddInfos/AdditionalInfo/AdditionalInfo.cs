using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public override ZString CSI_Code
		{
			get { return base.CSI_Code; }
			set
			{
				base.CSI_Code = value;
				Declaration?.MarkAsNeedingValidation();
			}
		}

		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				base.CSI_SubType = value;
				Declaration?.MarkAsNeedingValidation();
			}
		}

		protected override bool IsHeaderOnlyCore
		{
			get
			{
				if (Declaration != null && Declaration.IsUCC6)
				{
					return true;
				}
				else
				{
					return base.IsHeaderOnlyCore;
				}
			}
		}

		protected override bool IsLineCore
		{
			get
			{
				if (Declaration != null && Declaration.IsUCC6)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		protected override bool IsLineOnlyCore => true;

		[ResourceStringData("FRAdditionalInfo|CSI_DateOfIssue", Caption = "Date Of Issue")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		protected new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new AdditionalInfoLookups(this);
		}

		protected new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return Declaration?.ApplicationExtender.GetAdditionalInfoValidation(this) ?? new AdditionalInfoValidation(this);
		}
	}
}
