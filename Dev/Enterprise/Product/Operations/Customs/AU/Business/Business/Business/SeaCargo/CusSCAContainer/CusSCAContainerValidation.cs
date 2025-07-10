using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerValidation : Customs.Business.CusSCAContainerValidation
	{
		public CusSCAContainerValidation(Customs.Business.BaseCusSCAContainer parent)
			: base(parent)
		{
		}

		public CusSCAContainer SCAContainer
		{
			get { return (CusSCAContainer)Parent; }
		}

		public MessageValidation MessageValidation
		{
			get { return new MessageValidation(Parent); }
		}

		public void RunPreUnderbondValidation()
		{
			preUnderbondValidationRunning = true;
			try
			{
				RunPreUnderbondValidationCore();
			}
			finally
			{
				preUnderbondValidationRunning = false;
			}
		}
		protected bool preUnderbondValidationRunning;

		protected virtual void RunPreUnderbondValidationCore()
		{
		}

		#region Overrides

		protected override void CheckCN_ContainerMode()
		{
			base.CheckCN_ContainerMode();
			MessageValidation.CheckEntered(SCAContainer.CN_ContainerModeInfo);
			ListValidation.MessageErrorIfInvalidCode(SCAContainer.CN_ContainerModeInfo, SCAContainer.CN_ContainerMode_List);
			if (!SCAContainer.IsBreakBulk && !SCAContainer.IsBulk)
			{
				ValidateCN_ContainerNumber();
			}

			if (SCAContainer.OceanBill != null && (SCAContainer.IsBulk || SCAContainer.IsBreakBulk))
			{
				bool found = false;
				foreach (CusSCAContainer otherContainer in SCAContainer.OceanBill.Containers)
				{
					if (otherContainer != SCAContainer && otherContainer.CN_ContainerMode == SCAContainer.CN_ContainerMode)
					{
						found = true;
					}
				}

				if (found)
				{
					SCAContainer.CN_ContainerModeInfo.AddError("Only one " + ImportCargoTypes.GetDescriptionFromCode(SCAContainer.CN_ContainerMode) + " container is allowed.");
				}
			}
		}

		protected override void CheckCN_SealNumber()
		{
			base.CheckCN_SealNumber();
			if (SCAContainer.CN_SealNumber.IsEmpty)
			{
				SCAContainer.CN_SealNumberInfo.AddWarning("It is suggested that a seal number is entered for the Container. Customs may impede this house bill if the container seal is blank.");
			}
		}

		protected override void CheckCN_ContainerNumber()
		{
			base.CheckCN_ContainerNumber();
			if (!CusSCAPivot.ContainerIsBulkOrBreakBulk(SCAContainer.CN_ContainerNumber))
			{
				ContainerNumberValidation.WarnIfInvalid(SCAContainer.CN_ContainerNumberInfo);
				MessageValidation.CheckEntered(SCAContainer.CN_ContainerNumberInfo);
			}

			MessageValidation.CheckEntered(SCAContainer.CN_ContainerNumberInfo);
			if (SCAContainer.CN_ContainerNumber.Length > 17)
			{
				SCAContainer.CN_ContainerNumberInfo.AddMessageError("Container Number length must be under 18 characters.");
			}

			if (SCAContainer.OceanBill != null)
			{
				bool found = false;
				foreach (CusSCAContainer otherContainer in SCAContainer.OceanBill.Containers)
				{
					if (otherContainer != SCAContainer && otherContainer.CN_ContainerNumber == SCAContainer.CN_ContainerNumber)
					{
						found = true;
					}
				}

				if (found)
				{
					SCAContainer.CN_ContainerNumberInfo.AddError("Duplicate container numbers are not allowed.");
				}
			}
		}

		#region Container Type

		protected override void CheckCN_TypeOfContainer()
		{
			base.CheckCN_TypeOfContainer();
			if (!SCAContainer.IsBreakBulk && !SCAContainer.IsBulk)
			{
				MessageValidation.CheckEntered(Parent.CN_TypeOfContainerInfo, "Container Type is required.");
				ListValidation.MessageErrorIfInvalidCode(SCAContainer.CN_TypeOfContainerInfo, SCAContainer.Lookups.TypesOfContainer);
			}

			ValidateCN_RC_NKContainerType();
		}

		protected bool IsContainerTypeRequired
		{
			get { return SCAContainer.CN_TypeOfContainer.IsEmpty || SCAContainer.CN_ContainerSizeOrISOCode.IsEmpty; }
		}

		#endregion

		protected override void CheckCN_RC_NKContainerType()
		{
			base.CheckCN_RC_NKContainerType();
			if (!CusSCAPivot.ContainerIsBulkOrBreakBulk(SCAContainer.CN_ContainerNumber))
			{
				if (SCAContainer.ContainerType != null && SCAContainer.ContainerType.RC_ISOType.IsEmpty)
				{
					SCAContainer.CN_RC_NKContainerTypeInfo.AddMessageError("Container type must have a valid ISO Number entered. Use F3/F4 to edit the container type and add an ISO Number.");
				}

				if (IsContainerTypeRequired)
				{
					MessageValidation.CheckEntered(SCAContainer.CN_RC_NKContainerTypeInfo);
				}

				ListValidation.MessageErrorIfInvalidCode(SCAContainer.CN_RC_NKContainerTypeInfo, SCAContainer.CN_ContainerType_List);
			}
		}

		#endregion

		#region Container Size / ISO Code

		protected override void CheckCN_ContainerSizeOrISOCode()
		{
			base.CheckCN_ContainerSizeOrISOCode();
			if (!SCAContainer.IsBreakBulk && !SCAContainer.IsBulk)
			{
				MessageValidation.CheckEntered(Parent.CN_ContainerSizeOrISOCodeInfo, "Container Size is required.");
				ListValidation.MessageErrorIfInvalidCode(Parent.CN_ContainerSizeOrISOCodeInfo, SCAContainer.Lookups.ContainerSizes);
			}

			ValidateCN_RC_NKContainerType();
		}

		#endregion

		protected override void CheckCFSOrgValidation()
		{
			if (!SCAContainer.CN_OA_UnderbondTo.IsEmpty)
			{
				var oceanBill = SCAContainer.OceanBill;
				if (oceanBill != null)
				{
					var consol = oceanBill.Consol;
					if (consol != null && consol.UnpackDepotAddress != null && !consol.UnpackDepotAddress.OA_IsActive)
					{
						SCAContainer.CN_ContainerNumberInfo.AddWarning(ArrivalCFSAddressIsInactive);
					}
				}
			}
		}

		internal const string ArrivalCFSAddressIsInactive = "The Arrival CFS address selected on the Consol is inactive. Please select another address.";

		protected override void CheckCTOOrgValidation()
		{
			if (!SCAContainer.CN_OA_UnderbondFrom.IsEmpty)
			{
				var oceanBill = SCAContainer.OceanBill;
				if (oceanBill != null)
				{
					var consol = oceanBill.Consol;
					if (consol != null && consol.ArrivalCTOAddress != null && !consol.ArrivalCTOAddress.OA_IsActive)
					{
						SCAContainer.CN_ContainerNumberInfo.AddWarning(ArrivalCTOAddressIsInactive);
					}
				}
			}
		}

		internal const string ArrivalCTOAddressIsInactive = "The Arrival CTO address selected on the Consol is inactive. Please select another address.";

		CMRImportCargoTypes ImportCargoTypes
		{
			get
			{
				if (fImportCargoTypes == null)
				{
					fImportCargoTypes = new CMRImportCargoTypes();
				}
				return fImportCargoTypes;
			}
		}
		CMRImportCargoTypes fImportCargoTypes;
	}
}
