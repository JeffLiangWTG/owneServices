using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDJobDeclarationValidation : ExportJobDeclarationValidation
	{
		public EXDJobDeclarationValidation(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			if (!JobDeclaration.ExportGoodsTypeIsSpares && !JobDeclaration.ExportGoodsTypeIsStores)
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_OH_ImporterInfo);

				if (JobDeclaration.Importer != null)
				{
					if (JobDeclaration.Importer.IsMiscellaneous)
					{
						if (JobDeclaration.ZA_ConsigneeNameHidden.IsEmpty)
						{
							JobDeclaration.JE_OH_ImporterInfo.AddMessageError("The Importer must have a valid name entered");
						}
					}
					if (JobDeclaration.ImporterCity.IsEmpty)
					{
						JobDeclaration.JE_OH_ImporterInfo.AddMessageError("The Importer must have a valid city or UNLOCO entered");
					}
				}
			}
		}

		protected override void CheckJE_TotalNoOfPacks()
		{
			base.CheckJE_TotalNoOfPacks();
			string messageError = "";
			if (JobDeclaration.JE_ContainerCount == 0 && JobDeclaration.JE_ContainerMode != Core.Constants.ContainerModes.Bulk && JobDeclaration.JE_ContainerMode != Core.Constants.ContainerModes.Liquid)
			{
				if (JobDeclaration.JE_TotalNoOfPacks == 0)
				{
					messageError = "Total Number of Packages must be greater than 0 when Total Number of Containers is 0 and the cargo type is not Bulk or Liquid";
				}
			}
			if (JobDeclaration.JE_TransportMode == Enterprise.Core.Constants.TransportModes.Air)
			{
				if (JobDeclaration.JE_TotalNoOfPacks == 0)
				{
					messageError = "Total Number of Packages must be greater than 0 when Mode of Transport is Air";
				}
			}
			else if (JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.NonContainerised || JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.Combination)
			{
				if (JobDeclaration.JE_TotalNoOfPacks == 0)
				{
					messageError = "Total Number of Packages must be greater than 0 when Container Mode is Non-containerised or Combination";
				}
			}
			if (!string.IsNullOrEmpty(messageError))
			{
				JobDeclaration.JE_TotalNoOfPacksInfo.AddMessageError(messageError);
			}

			if (JobDeclaration.JE_TotalNoOfPacks > 9999999)
			{
				JobDeclaration.JE_TotalNoOfPacksInfo.AddMessageError("Total Number of Packages must be less than 9,999,999");
			}
		}

		protected override void CheckJE_ContainerCount()
		{
			base.CheckJE_ContainerCount();

			if (JobDeclaration.IsSea)
			{
				string messageError = "";
				if (JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.Containerised || JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.Combination)
				{
					if (JobDeclaration.JE_ContainerCount == 0)
					{
						messageError = "Total Number of Containers must be greater than 0 when Container Mode is Containerised or Combination";
					}
				}
				else if (JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.NonContainerised || JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.Bulk || JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.Liquid)
				{
					if (JobDeclaration.JE_ContainerCount != 0)
					{
						messageError = "Total Number of Containers must be 0 when Container Mode is Non-containerised, Bulk or Liquid";
					}
				}

				if (JobDeclaration.JE_ExportGoodsType == "PO" || JobDeclaration.JE_ExportGoodsType == "OP" || JobDeclaration.JE_ExportGoodsType == "AB")
				{
					if (JobDeclaration.JE_ContainerCount != 0)
					{
						messageError = "Total Number of Containers must be 0 when Export Goods Type is Postal, Own Power or Accompanied";
					}
				}

				if (!string.IsNullOrEmpty(messageError))
				{
					JobDeclaration.JE_ContainerCountInfo.AddMessageError(messageError);
				}

				if (JobDeclaration.JE_ContainerCount > 9999999)
				{
					JobDeclaration.JE_ContainerCountInfo.AddMessageError("Total Number of Containers must be less than 9,999,999");
				}

				if (JobDeclaration.JE_ContainerCount != JobDeclaration.CusContainers.Count)
				{
					JobDeclaration.JE_ContainerCountInfo.AddWarning(string.Format("Total Number of Containers entered does not tally with the containers entered on Containers tab: ({0}).", JobDeclaration.CusContainers.Count));
				}
			}
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (JobDeclaration.IsSea && (JobDeclaration.ExportGoodsTypeIsSpares || JobDeclaration.ExportGoodsTypeIsStores))
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_VesselNameInfo);
			}
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			if (JobDeclaration.SupplierWrapper != null && !JobDeclaration.SupplierWrapper.IsCompanyOrg && JobDeclaration.SupplierWrapper.GoodsOwnerPartyID.IsEmpty)
			{
				JobDeclaration.JE_OH_SupplierInfo.AddMessageError("If the supplier is not the current company, either an ABN or Customs Client Code must be configured for them in order to send EXD messages");
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			MessageValidation.CheckEntered(JobDeclaration.JE_ExportDateInfo);
			if (JobDeclaration.JE_ExportDate.IsValid)
			{
				if (JobDeclaration.JE_ExportDate > ZDateTime.Today.AddMonths(6))
				{
					JobDeclaration.JE_ExportDateInfo.AddMessageError("Declaration may only be lodged within 180 days of intended export date");
				}
				else if (JobDeclaration.JE_ExportDate < ZDateTime.Today && JobDeclaration.IsNonConfirming)
				{
					JobDeclaration.JE_ExportDateInfo.AddMessageError("Declaration must be lodged before the intended date of export");
				}
				else if (JobDeclaration.JE_ExportDate.AddDays(14) < ZDateTime.Today && JobDeclaration.JE_MessageSubType == JobDeclaration.MessageSubType.Confirming)
				{
					JobDeclaration.JE_ExportDateInfo.AddMessageError("Confirming declaration can only be sent up to 14 days after the date of export");
				}
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			if (!JobDeclaration.ExportGoodsTypeIsSpares && !JobDeclaration.ExportGoodsTypeIsStores)
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_RL_NKFinalDestinationInfo);
			}
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			if (JobDeclaration.JE_ExportGoodsType == "OT" || JobDeclaration.JE_ExportGoodsType == "OP" || JobDeclaration.JE_ExportGoodsType == "AB")
			{
				if (JobDeclaration.JE_RL_NKPortOfArrival.IsEmpty)
				{
					JobDeclaration.JE_RL_NKPortOfArrivalInfo.AddMessageError("First Port of Discharge Must Be Provided Where Export Goods Type is OT (Other), OP (Own Power) or AB (Accompanied Baggage)");
				}
			}
		}

		protected override void CheckJE_ExportGoodsType()
		{
			base.CheckJE_ExportGoodsType();
			if (JobDeclaration.JE_ExportGoodsType != JobDeclaration.ExportGoodsType.GeneralConsignedCargo &&
				JobDeclaration.JE_ExportGoodsType != JobDeclaration.ExportGoodsType.Stores &&
				JobDeclaration.JE_ExportGoodsType != JobDeclaration.ExportGoodsType.SpareParts &&
				JobDeclaration.JE_ExportGoodsType != JobDeclaration.ExportGoodsType.OwnPower &&
				JobDeclaration.JE_ExportGoodsType != JobDeclaration.ExportGoodsType.AccompaniedBaggage &&
				JobDeclaration.JE_ExportGoodsType != JobDeclaration.ExportGoodsType.Postal)
			{
				JobDeclaration.JE_ExportGoodsTypeInfo.AddMessageError("Export Goods Type must be OT, ST, SP, OP, AB or PO");
			}
			else if (JobDeclaration.IsPost && JobDeclaration.JE_ExportGoodsType != JobDeclaration.ExportGoodsType.Postal)
			{
				JobDeclaration.JE_ExportGoodsTypeInfo.AddMessageError("Export goods type must be Postal");
			}

			ValidateJE_OH_Importer();
			ValidateJE_TransportMode();
			ValidateJE_RL_NKFinalDestination();
			ValidateJE_ContainerCount();

			MessageValidation.CheckEntered(JobDeclaration.JE_ExportGoodsTypeInfo);
		}

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();
			if (JobDeclaration.IsPost && JobDeclaration.JE_ContainerMode != Core.Constants.ContainerModes.NonContainerised)
			{
				JobDeclaration.JE_ContainerModeInfo.AddMessageError("The container mode must be non-containerised for post declarations.");
			}
		}

		protected override void CheckExportDeclarationNumber()
		{
			base.ValidateExportDeclarationNumber();
			if (JobDeclaration.DeclarationNumber.Length > 9)
			{
				JobDeclaration.DeclarationNumberInfo.AddMessageError("The export declaration number must be 9 characters or less.");
			}
		}

		protected override bool JE_MergeByRequired
		{
			get { return false; }
		}
	}
}
