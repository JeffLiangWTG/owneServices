using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CusTempStorageDecs")]
	public class CHGTSTCusTempStorageDec : CusTempStorageDec
	{
		public CHGTSTCusTempStorageDec(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : CusTempStorageDec.Schema
		{
			public const string NewCustodianBranch = "NewCustodianBranch";
			public const int NewCustodianBranchMaxlength = 4;
		}

		#region Properties

		public override ZString ReferenceNumber => FormattedOwnerReferenceNumber;

		public override ZString STH_IdentificationIndicator
		{
			get => base.STH_IdentificationIndicator;
			set
			{
				var hasChanges = STH_IdentificationIndicator != value;
				if (hasChanges)
				{
					base.STH_IdentificationIndicator = value;
					if (IsREGDeclaration)
					{
						foreach (CHGTSTCusTempStorageLine line in CusTempStorageLines)
						{
							line.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
						}
					}
					else if (IsAWBDeclaration)
					{
						var regLines = CusTempStorageLines.Cast<CHGTSTCusTempStorageLine>().Where(
							x => x.TSL_OwnerReferenceType == OwnerReferenceTypeList.Codes.REG);
						foreach (var line in regLines)
						{
							line.TSL_OwnerReferenceType = ZString.Empty;
						}
					}
				}
			}
		}

		[MaxLength(Schema.NewCustodianBranchMaxlength)]
		[List(nameof(Lookups) + "." + nameof(CHGTSTCusTempStorageDecLookups.NewCustodianBranchList))]
		[ResourceStringData("44658ee2-a908-47ab-bbd9-1c8f4d66051d", Caption = "New Custodian Branch")]
		public ZString NewCustodianBranch
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.NewCustodianBranch); }
			set
			{
				var oldValue = NewCustodianBranch;
				CheckMaximumLength(NewCustodianBranchInfo, value);
				this.SetSystemDefinedValue(Schema.NewCustodianBranch, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNewCustodianBranch();
				}
				NewCustodianBranchInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo NewCustodianBranchInfo => GetZPropertyInfo(Schema.NewCustodianBranch);

		#endregion

		#region Lookups
		public new CHGTSTCusTempStorageDecLookups Lookups => (CHGTSTCusTempStorageDecLookups)base.Lookups;

		protected override EU.Business.CusTempStorage.CusTempStorageDecLookups GetNewLookups() => new CHGTSTCusTempStorageDecLookups(this);

		#endregion

		#region Validation

		public new CHGTSTCusTempStorageDecValidation Validation => (CHGTSTCusTempStorageDecValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new CHGTSTCusTempStorageDecValidation(this);

		#endregion

		#region CusTempStorageLines

		public new CHGTSTCusTempStorageLineCollection CusTempStorageLines => (CHGTSTCusTempStorageLineCollection)base.CusTempStorageLines;

		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CHGTSTCusTempStorageLineCollection(this);

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation;
		}

		#endregion
	}
}
