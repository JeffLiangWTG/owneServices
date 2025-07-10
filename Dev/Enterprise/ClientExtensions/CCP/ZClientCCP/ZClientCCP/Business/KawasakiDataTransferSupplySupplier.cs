using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.ZClientCCP.GUI;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.ZClientCCP.Business
{
	public class KawasakiDataTransferSupplySupplier : NonPersistentBusinessObject, IObsoleteValidation
	{
		public KawasakiDataTransferSupplySupplier(BaseJobDeclaration jobDec, string dialogFilter, string formHeading)
		{
			JobDeclaration = jobDec;
			this.fDialogFilter = dialogFilter;
			this.fFormHeading = formHeading;
		}

		#region Schema

		public static class Schema
		{
			public const string SupplierForDeclaration = "SupplierForDeclaration";
			public const string FileName = "FileName";
		}

		#endregion

		#region Declaration

		protected BaseJobDeclaration fJobDeclaration;
		public BaseJobDeclaration JobDeclaration
		{
			get { return fJobDeclaration; }
			set
			{
				if (fJobDeclaration != value)
				{
					UnRegisterEditableChildObject(fJobDeclaration);
					fJobDeclaration = value;
					RegisterEditableChildObject(fJobDeclaration);
				}
			}
		}
		protected KawasakiDataTransferForm DataTransferForm;

		#endregion

		#region DataTransfer Properties

		public string DialogFilter
		{
			get { return fDialogFilter; }
		}
		protected string fDialogFilter;

		public string FormHeading
		{
			get { return fFormHeading; }
		}
		protected string fFormHeading;

		#endregion

		#region SupplierList

		public OrgHeaderCollection SupplierList
		{
			get
			{
				if (fSupplierList == null)
				{
					fSupplierList = new ConsignorCollection(new BusinessObjectFactory());
				}
				return fSupplierList;
			}
		}
		protected ConsignorCollection fSupplierList;

		#endregion

		#region SupplierForDeclaration

		public ZGuid SupplierForDeclaration
		{
			get { return JobDeclaration.JE_OH_Supplier; }
			set
			{
				JobDeclaration.JE_OH_Supplier = value;
				HasChanges = true;
				if (!IsValidationSuspended)
				{
					ValidateSupplierForDeclaration();
				}
				SupplierForDeclarationInfo.RefreshBinding();
			}
		}

		public void ValidateSupplierForDeclaration()
		{
			SupplierForDeclarationInfo.ClearAllNotifications();
			CheckSupplierForDeclarationIsEmpty();
			TypeValidation.CheckValidGuid(SupplierForDeclarationInfo);
		}

		public ZPropertyInfo SupplierForDeclarationInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.SupplierForDeclaration); }
		}

		protected void CheckSupplierForDeclarationIsEmpty()
		{
			if (SupplierForDeclaration.IsEmpty && !SupplierForDeclarationInfo.HasErrors())
			{
				SupplierForDeclarationInfo.AddError("You need to specify a supplier before importing.");
			}
		}

		#endregion

		#region FileName

		[MaxLength(255)]
		public ZString FileName
		{
			get { return fileName; }
			set
			{
				CheckMaximumLength(FileNameInfo, value);
				SetNonPersistentPropertyValue(FileNameInfo, ref fileName, value);
				HasChanges = true;
				if (!IsValidationSuspended)
				{
					ValidateFileName();
				}
			}
		}
		ZString fileName;

		public void ValidateFileName()
		{
			FileNameInfo.ClearAllNotifications();
			CheckFileNameIsEmpty();
		}

		public ZPropertyInfo FileNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.FileName); }
		}

		protected void CheckFileNameIsEmpty()
		{
			if (FileName.IsEmpty && !FileNameInfo.HasErrors())
			{
				FileNameInfo.AddError("You need to specify a Kawasaki Data File.");
			}
		}

		#endregion
	}
}

#region SetUp
#endregion
