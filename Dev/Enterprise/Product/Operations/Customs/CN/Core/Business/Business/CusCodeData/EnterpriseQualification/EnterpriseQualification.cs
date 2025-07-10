using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class EnterpriseQualification : CusCodeData, ICIQEnterpriseQualification
	{
		public EnterpriseQualification(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Override

		public override bool SupportsNotes => false;

		[ResourceStringData("Enterprise.Customs.CN.Business.EnterpriseQualification|CY_CodeDescription", Caption = "Qualification Type Description", MediumCaption = "Type Description", ShortCaption = "Type Desc.")]
		public override ZString Description => base.Description;

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.CN.Business.EnterpriseQualification|CY_Code", Caption = "Qualification Type", ShortCaption = "Type")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[MaxLength(40)]
		[ResourceStringData("Enterprise.Customs.CN.Business.EnterpriseQualification|CY_Data", Caption = "Qualification Number", ShortCaption = "Number")]
		[List(nameof(Lookups) + "." + nameof(EnterpriseQualificationLookups.CY_DataList))]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				base.CY_Data = value;

				if (!IsCopying && CY_Code.IsEmpty)
				{
					var des = Lookups.CY_DataList.GetDescriptionFromCode(CY_Data);
					if (!string.IsNullOrEmpty(des))
					{
						var subdes = des.Substring(des.IndexOf(")", comparisonType: StringComparison.OrdinalIgnoreCase) + 1);
						CY_Code = Lookups.CY_CodeList.GetCodeFromDescription(subdes);
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = Constants.CusCodeDataTypes.Codes.EnterpriseQualification;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new EnterpriseQualificationValidation(this);
		}

		public new EnterpriseQualificationValidation Validation => (EnterpriseQualificationValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new EnterpriseQualificationLookups(this);
		}

		public new EnterpriseQualificationLookups Lookups => (EnterpriseQualificationLookups)base.Lookups;

		public override string ToString()
		{
			return FormattableString.Invariant($"{CY_Code}:{CY_Data}");
		}

		#endregion

		ZString ICIQEnterpriseQualification.Type => CY_Code;

		ZString ICIQEnterpriseQualification.Number => CY_Data;
	}
}
