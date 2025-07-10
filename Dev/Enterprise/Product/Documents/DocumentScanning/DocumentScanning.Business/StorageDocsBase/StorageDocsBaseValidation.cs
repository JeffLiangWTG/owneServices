using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsBaseValidation : StorageDocsValidation
	{
		public StorageDocsBaseValidation(StorageDocsBase parent)
			: base(parent)
		{
		}

		new StorageDocsBase Parent => (StorageDocsBase)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCompanyCode();
			ValidateBranchCode();
			ValidateIndex();
		}

		protected bool IsNewOrChangedRecord => !Parent.IsInDatabase || Parent.HasChanges;

		protected override void CheckSC_ImageDataIsValidZBlobSize()
		{
			var maximumFileSizeInMB = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value;

			if (Parent.HasSC_ImageDataBeenUpdated && Parent.SC_ImageDataFromDb.Length > maximumFileSizeInMB * 1024 * 1024)
			{
				Parent.SC_ImageDataInfo.AddError(Res.GetString("78647b37-4e56-46d5-ae33-4235682f5dd3", "The file '{0}' is larger than the maximum file size specified by the system ({1} MB)", Parent.SC_FileNameWithExtension, maximumFileSizeInMB.ToString(CultureInfo.CurrentCulture)));
			}
		}

		#region Index

		protected void CheckIndex()
		{
			if (Parent.IncludedInPrint)
			{
				if (Parent.EDocsToBeDelivered?.Cast<StorageDocsBase>().Any(s => s.IncludedInPrint && s.Index == Parent.Index && s.PK != Parent.PK) ?? false)
				{
					Parent.IndexInfo.AddWarning(Res.GetString("75a73037-c853-480e-90ec-4699ddb3c0c9", "The index value should not be the same as others."));
				}
			}
		}

		RunValidationInvoker GetIndexValidationInvoker()
		{
			return CheckIndex;
		}

		public void ValidateIndex()
		{
			((IValidationInternals)this).Validate(Parent.IndexInfo, GetIndexValidationInvoker());
		}

		#endregion

		#region CompanyCode

		public void ValidateCompanyCode()
		{
			((IValidationInternals)this).Validate(Parent.CompanyCodeInfo, CheckCompanyCode);
		}

		protected virtual void CheckCompanyCode()
		{
			if (!Parent.IsInDatabase || Parent.SC_GC_CompanyInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CompanyCodeInfo);
			}
		}

		#endregion

		#region BranchCode

		public void ValidateBranchCode()
		{
			((IValidationInternals)this).Validate(Parent.BranchCodeInfo, CheckBranchCode);
		}

		protected virtual void CheckBranchCode()
		{
			if (!Parent.IsInDatabase || Parent.SC_GB_BranchInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Parent.BranchCodeInfo);
			}
		}

		#endregion

		#region DepartmentCode

		public void ValidateDepartmentCode()
		{
			((IValidationInternals)this).Validate(Parent.DepartmentCodeInfo, CheckDepartmentCode);
		}

		protected virtual void CheckDepartmentCode()
		{
			if (!Parent.IsInDatabase || Parent.SC_GE_DepartmentInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DepartmentCodeInfo);
			}
		}

		#endregion

		#region IncludedInPrint

		public void ValidateIncludedInPrint()
		{
			((IValidationInternals)this).Validate(Parent.IncludedInPrintInfo, CheckIncludedInPrint);
		}

		protected virtual void CheckIncludedInPrint()
		{
			if (Parent.IncludedInPrint && !Parent.ShouldPrintByDefault)
			{
				if (!Parent.SC_IsPublished)
				{
					Parent.IncludedInPrintInfo.AddWarning(Res.GetString("5D846708-B4C5-46E5-9D7F-7ECBA48D220C", "The eDoc file {0} was not published.", Parent.SC_FileName));
				}
				else if (Parent.SC_IsDeleted)
				{
					Parent.IncludedInPrintInfo.AddError(Res.GetString("06A6BA74-FCAA-4C67-AAA5-225C645EDF46", "The eDoc file {0} has been deleted.", Parent.SC_FileName));
				}
			}
		}

		#endregion

		#region IsParsingEnabled

		public void ValidateIsParsingEnabled()
		{
			((IValidationInternals)this).Validate(Parent.IsParsingEnabledInfo, CheckIsParsingEnabled);
		}

		void CheckIsParsingEnabled()
		{
			if (Parent.IsParsingEnabledValue && Parent.IsParsingDeniedByDocumentOwner)
			{
				Parent.IsParsingEnabledInfo.AddError(Res.GetString("7CB24B62-F5D7-4D09-A079-7FE405ACAFB8", "The document is not eligible for parsing."));
			}

			if (!Parent.IsDataTypeValidForParsing && !Parent.ParseType.IsEmpty)
			{
				Parent.IsParsingEnabledInfo.AddWarning(Res.GetString("1B728CBA-B0E0-4F49-BD7F-C6E01BA4FFA2", "The document’s file type is not supported for parsing."));
			}
		}

		#endregion

		public override void ValidateSC_FileNameForRenaming()
		{
			ValidateCalculatedProperty(Parent.SC_FileNameForRenamingInfo);
		}
	}
}
