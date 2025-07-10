using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class PreviousDocumentMaster : NonPersistentBusinessObject
	{
		public PreviousDocumentMaster(BusinessObjectFactory factory, IPreviousDocumentParentProvider parent)
			: base(factory)
		{
			Parent = parent;
		}

		public readonly IPreviousDocumentParentProvider Parent;
		PreviousDocumentCollection PreviousDocuments => Parent.PreviousDocuments;

		#region Properties

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentMasterLookups.AuthorizationNumberList))]
		[ReadOnlyMember(nameof(AuthorizationNumber_ReadOnly))]
		[MaxLength(PreviousDocument.Schema.AuthorizationNumberMaxLength)]
		[ResourceStringData("899fa748-26a0-49ef-8dc8-55b96e12cf3a", Caption = "Authorization Number")]
		public ZString AuthorizationNumber
		{
			get => HasPreviousDocuments ? PreviousDocuments[0].AuthorizationNumber : ZString.Empty;
			set
			{
				if (AuthorizationNumber != value)
				{
					CheckMaximumLength(AuthorizationNumberInfo, value);
					foreach (PreviousDocument previousDocument in PreviousDocuments)
					{
						previousDocument.AuthorizationNumber = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateAuthorizationNumber();
					}
					AuthorizationNumberInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo AuthorizationNumberInfo => GetZPropertyInfo(PreviousDocument.Schema.AuthorizationNumber);

		bool AuthorizationNumber_ReadOnly => CSI_Procedure == PreviousProcedureList.Codes._ATAV && SimplifiedGrantAuthorizationFlag;

		void ClearAuthorizationNumberIfReadOnly()
		{
			if (AuthorizationNumber_ReadOnly)
			{
				AuthorizationNumber = ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(PreviousDocumentMasterLookups.ProcedureList))]
		[MaxLength(PreviousDocument.Schema.CSI_ProcedureMaxLength)]
		[ResourceStringData("76CB00B1-C0CC-4375-A4DF-C00C2990F78B", Caption = "Previous Procedure")]
		public ZString CSI_Procedure
		{
			get => HasPreviousDocuments ? PreviousDocuments[0].CSI_Procedure : ZString.Empty;
			set
			{
				if (CSI_Procedure != value)
				{
					var args = new CancelEventArgs(false);
					Parent.JobDeclaration?.PreviousDocumentMasterCSI_ProcedureAboutToChange(this, args);
					if (!args.Cancel)
					{
						CheckMaximumLength(CSI_ProcedureInfo, value);
						PreviousDocuments.RemoveAndDeleteAll();
						if (!value.IsEmpty)
						{
							var newPreviousDocument = PreviousDocuments.AddNew();
							newPreviousDocument.CSI_Procedure = value;
							if (IsImport || IsExport)
							{
								UpdateAuthorizationNumberIfOnlyOneExists();
								if (PreviousDocumentHelper.PreviousProceduresRequiringReference.Contains(value))
								{
									PreviousDocuments.RefreshBinding();
								}
							}
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateCSI_Procedure();
						}
						CSI_ProcedureInfo.RefreshBinding();
						CSI_Procedure_ValueChanged?.Invoke(this, null);
					}
				}
			}
		}
		public ZPropertyInfo CSI_ProcedureInfo => GetZPropertyInfo(PreviousDocument.Schema.CSI_Procedure);
		public EventHandler CSI_Procedure_ValueChanged;

		public void UpdateAuthorizationNumberIfOnlyOneExists()
		{
			var singleAuthorizationNumber = this.GetAuthorizationNumberIfOnlyOneExists();
			if (!singleAuthorizationNumber.IsEmpty)
			{
				AuthorizationNumber = singleAuthorizationNumber;
			}
		}

		[BusinessObjectTestExclude]
		[MaxLength(nameof(CSI_ReferenceNumber2MaxLength))]
		public ZString CSI_ReferenceNumber2
		{
			get => HasPreviousDocuments ? PreviousDocuments[0].CSI_ReferenceNumber2 : ZString.Empty;
			set
			{
				if (CSI_ReferenceNumber2 != value)
				{
					CheckMaximumLength(CSI_ReferenceNumber2Info, value);
					foreach (PreviousDocument previousDocument in PreviousDocuments)
					{
						previousDocument.CSI_ReferenceNumber2 = value;
					}
					CSI_ReferenceNumber2Info.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo CSI_ReferenceNumber2Info => GetZPropertyInfo(PreviousDocument.Schema.CSI_ReferenceNumber2);

		public int CSI_ReferenceNumber2MaxLength => HasPreviousDocuments ? PreviousDocuments[0].CSI_ReferenceNumber2MaxLength : PreviousDocument.Schema.CSI_ReferenceNumber2MaxLength;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(PreviousDocumentMasterLookups.CustomsOfficeList))]
		[MaxLength(PreviousDocument.Schema.CSI_CustomsOfficeMaxLength)]
		[ResourceStringData("a0d23f41-ce7f-477a-9092-5684cfad9004", Caption = "Monitoring Customs Office")]
		public ZString CSI_CustomsOffice
		{
			get => HasPreviousDocuments ? PreviousDocuments[0].CSI_CustomsOffice : ZString.Empty;
			set
			{
				if (CSI_CustomsOffice != value)
				{
					CheckMaximumLength(CSI_CustomsOfficeInfo, value);
					foreach (PreviousDocument previousDocument in PreviousDocuments)
					{
						previousDocument.CSI_CustomsOffice = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateCSI_CustomsOffice();
					}
					CSI_CustomsOfficeInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo CSI_CustomsOfficeInfo => GetZPropertyInfo(PreviousDocument.Schema.CSI_CustomsOffice);

		[BusinessObjectTestExclude]
		[ResourceStringData("7c47d0df-d932-4849-ae20-ae398991c9aa", Caption = "Simplified Grant Authorization?")]
		public ZBool SimplifiedGrantAuthorizationFlag
		{
			get => HasPreviousDocuments ? PreviousDocuments[0].SimplifiedGrantAuthorizationFlag : ZBool.False;
			set
			{
				if (SimplifiedGrantAuthorizationFlag != value)
				{
					foreach (PreviousDocument previousDocument in PreviousDocuments)
					{
						previousDocument.SimplifiedGrantAuthorizationFlag = value;
					}

					ClearAuthorizationNumberIfReadOnly();
					SimplifiedGrantAuthorizationFlagInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo SimplifiedGrantAuthorizationFlagInfo => GetZPropertyInfo(PreviousDocument.Schema.SimplifiedGrantAuthorizationFlag);

		#endregion

		public bool IsImport => Parent.JobDeclaration?.IsImport ?? false;
		public bool IsExport => Parent.JobDeclaration?.IsExport ?? false;

		public PreviousDocumentMasterValidation Validation => new PreviousDocumentMasterValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public PreviousDocumentMasterLookups Lookups => new PreviousDocumentMasterLookups(this);

		public bool HasPreviousDocuments => PreviousDocuments.Count > 0;

		public bool IsAvailable(string fieldName) //Checks whether the given property is shown as a common field above the grid, (i.e. whether it should be validated or not)
			=> new PreviousDocumentConfiguration().GetAvailableFieldsFromProcedureCode(CSI_Procedure).Contains(fieldName);
	}
}
