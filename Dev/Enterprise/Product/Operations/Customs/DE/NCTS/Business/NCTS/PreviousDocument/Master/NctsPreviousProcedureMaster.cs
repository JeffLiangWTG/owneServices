using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsPreviousProcedureMaster : NonPersistentBusinessObject
	{
		public NctsPreviousProcedureMaster(BusinessObjectFactory factory, INctsPreviousProcedureParentProvider parent)
			: base(factory)
		{
			Parent = parent;
		}

		public readonly INctsPreviousProcedureParentProvider Parent;

		EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousProcedures => Parent.PreviousProcedures;

		EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => Parent.PreviousDocuments;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(NctsPreviousProcedureMasterLookups.AuthorizationNumberList))]
		[ReadOnlyMember(nameof(AuthorizationNumber_ReadOnly))]
		[MaxLength(NctsPreviousDocument.Schema.AuthorizationNumberMaxLength)]
		[ResourceStringData("7C773DAC-105D-4B14-8110-0B128A5CC12A", Caption = "Authorization Number")]
		public ZString AuthorizationNumber
		{
			get => HasPreviousProcedures ? PreviousProcedures[0].AuthorizationNumber : ZString.Empty;
			set
			{
				if (AuthorizationNumber != value)
				{
					CheckMaximumLength(AuthorizationNumberInfo, value);
					foreach (NctsPreviousDocument previousProcedure in PreviousProcedures)
					{
						previousProcedure.AuthorizationNumber = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateAuthorizationNumber();
					}
					AuthorizationNumberInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo AuthorizationNumberInfo => GetZPropertyInfo(NctsPreviousDocument.Schema.AuthorizationNumber);

		bool AuthorizationNumber_ReadOnly => CSI_Procedure == NctsPreviousProcedureList.Codes._9DEY && SimplifiedGrantAuthorizationFlag;

		void ClearAuthorizationNumberIfReadOnly()
		{
			if (AuthorizationNumber_ReadOnly)
			{
				AuthorizationNumber = ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsPreviousProcedureMasterLookups.ProcedureList))]
		[MaxLength(NctsPreviousDocument.Schema.CSI_ProcedureMaxLength)]
		[ResourceStringData("64420231-0D1F-44BB-9BA6-6E7461D9F218", Caption = "Previous Procedure")]
		public ZString CSI_Procedure
		{
			get => HasPreviousProcedures ? PreviousProcedures[0].CSI_Procedure : ZString.Empty;
			set
			{
				if (CSI_Procedure != value)
				{
					var args = new CancelEventArgs(false);
					Parent.NctsHeader?.PreviousProcedureMasterCSI_ProcedureAboutToChange(this, args);
					if (!args.Cancel)
					{
						CheckMaximumLength(CSI_ProcedureInfo, value);
						PreviousProcedures.RemoveAndDeleteAll();

						if (!value.IsEmpty)
						{
							var newPreviousProcedure = PreviousProcedures.AddNew();
							newPreviousProcedure.CSI_Procedure = value;

							DeleteOldAndAddNewPreviousDocuments(value);
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

		public ZPropertyInfo CSI_ProcedureInfo => GetZPropertyInfo(NctsPreviousDocument.Schema.CSI_Procedure);

		public EventHandler CSI_Procedure_ValueChanged;

		[BusinessObjectTestExclude]
		[MaxLength(NctsPreviousDocument.Schema.ReferenceNumber2MaxLength)]
		public ZString CSI_ReferenceNumber2
		{
			get => HasPreviousProcedures ? PreviousProcedures[0].CSI_ReferenceNumber2 : ZString.Empty;
			set
			{
				if (CSI_ReferenceNumber2 != value)
				{
					CheckMaximumLength(CSI_ReferenceNumber2Info, value);
					foreach (NctsPreviousDocument previousProcedure in PreviousProcedures)
					{
						previousProcedure.CSI_ReferenceNumber2 = value;
					}
					CSI_ReferenceNumber2Info.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CSI_ReferenceNumber2Info => GetZPropertyInfo(NctsPreviousDocument.Schema.CSI_ReferenceNumber2);

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(NctsPreviousProcedureMasterLookups.CustomsOfficeList))]
		[MaxLength(NctsPreviousDocument.Schema.CSI_CustomsOfficeMaxLength)]
		[ResourceStringData("B51E65B9-FA1B-4708-85D3-14ACF5C4C5F0", Caption = "Monitoring Customs Office")]
		public ZString CSI_CustomsOffice
		{
			get => HasPreviousProcedures ? PreviousProcedures[0].CSI_CustomsOffice : ZString.Empty;
			set
			{
				if (CSI_CustomsOffice != value)
				{
					CheckMaximumLength(CSI_CustomsOfficeInfo, value);
					foreach (NctsPreviousDocument previousProcedure in PreviousProcedures)
					{
						previousProcedure.CSI_CustomsOffice = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateCSI_CustomsOffice();
					}
					CSI_CustomsOfficeInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo CSI_CustomsOfficeInfo => GetZPropertyInfo(NctsPreviousDocument.Schema.CSI_CustomsOffice);

		[BusinessObjectTestExclude]
		[ResourceStringData("30695B62-E195-4FE8-9F2E-C4DB4F8112B9", Caption = "Simplified Grant Authorization?")]
		public ZBool SimplifiedGrantAuthorizationFlag
		{
			get => HasPreviousProcedures ? PreviousProcedures[0].SimplifiedGrantAuthorizationFlag : ZBool.False;
			set
			{
				if (SimplifiedGrantAuthorizationFlag != value)
				{
					foreach (NctsPreviousDocument previousProcedure in PreviousProcedures)
					{
						previousProcedure.SimplifiedGrantAuthorizationFlag = value;
					}

					ClearAuthorizationNumberIfReadOnly();
					SimplifiedGrantAuthorizationFlagInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo SimplifiedGrantAuthorizationFlagInfo => GetZPropertyInfo(NctsPreviousDocument.Schema.SimplifiedGrantAuthorizationFlag);

		public NctsPreviousProcedureMasterValidation Validation => new NctsPreviousProcedureMasterValidation(this);

		public NctsPreviousProcedureMasterLookups Lookups => new NctsPreviousProcedureMasterLookups(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		bool HasPreviousProcedures => PreviousProcedures.Count > 0;

		void DeleteOldAndAddNewPreviousDocuments(ZString procedure)
		{
			var procedureList = Lookups.ProcedureList;
			if (procedureList.ContainsCode(procedure))
			{
				var removingPreviousDocuments = new List<NctsPreviousDocument>();
				foreach (NctsPreviousDocument previousDocument in PreviousDocuments)
				{
					if (procedureList.ContainsCode(previousDocument.CSI_Code))
					{
						removingPreviousDocuments.Add(previousDocument);
					}
				}

				foreach (var previousDocument in removingPreviousDocuments)
				{
					PreviousDocuments.RemoveAndDelete(previousDocument);
				}

				var newPreviousDocument = PreviousDocuments.AddNew();
				newPreviousDocument.CSI_Code = procedure;
			}
		}
	}
}
