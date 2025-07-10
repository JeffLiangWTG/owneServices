using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class OfficeCode : EuOfficeCode
	{
		public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobDeclaration Parent => (JobDeclaration)base.Parent;

		[List(nameof(Lookups) + "." + nameof(OfficeCodeLookups.OfficeCodeList))]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		public new OfficeCodeValidation Validation => (OfficeCodeValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups()
		{
			var result = new OfficeCodeLookups(this);
			if (Parent.IsExitSummary)
			{
				result = new ExitSummaryOfficeCodeLookups(this);
			}
			else if (Parent.IsExport)
			{
				result = new OfficeCodeLookups(this);
			}
			else if (Parent.IsImport)
			{
				if (Parent.IsUCC5)
				{
					result = new ImportUCC5OfficeCodeLookups(this);
				}
				else
				{
					result = new ImportOfficeCodeLookups(this);
				}
			}
			return result;
		}

		protected override bool IsLookupsCachedInBase => false;

		protected override CusCodeDataValidation GetNewValidation()
		{
			switch (Parent?.JE_MessageType.ToUpperInvariant() ?? ZString.Empty)
			{
				case IEJobMessageTypeList.Codes.ExitSummary:
					return new ExitSummaryOfficeCodeValidation(this);
				case IEJobMessageTypeList.Codes.ReExport:
					return new ReExportOfficeCodeValidation(this);
				case IEJobMessageTypeList.Codes.Export:
					return new ExportOfficeCodeValidation(this);
				default:
					return new OfficeCodeValidation(this);
			}
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override ZString HumanReadableNameCore => Res.GetString("B34ECF4B-5581-4AA6-BA6A-3C8830C41DEE", "Customs Office {0}({1})", CY_Code, Lookups.CY_CodeList.GetDescriptionFromCode(CY_Code));

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			var result = base.GetWarningBeforeBeingDeleted();

			if (result.IsEmpty && IsInDatabase && Parent is JobDeclaration declaration && declaration.CustomsEntryHeaders.HasAnEntryWithEntryStatus)
			{
				switch (CY_Code.ToUpperInvariant())
				{
					case EuOfficeCodesTypes.Codes.OfficeOfExit:
						if (!declaration.OriginalExitOffice.IsEmpty)
						{
							result = CommonResStrings.ShouldNotDeleteDeclaredDataString;
						}
						break;
					case EuOfficeCodesTypes.Codes.OfficeOfPresentation:
						if (!declaration.OriginalPresentationOffice.IsEmpty)
						{
							result = CommonResStrings.ShouldNotDeleteDeclaredDataString;
						}
						break;
					case EuOfficeCodesTypes.Codes.SupervisingOffice:
						if (!declaration.OriginalSupervisingOffice.IsEmpty)
						{
							result = CommonResStrings.ShouldNotDeleteDeclaredDataString;
						}
						break;
				}
			}
			return result;
		}
	}
}
