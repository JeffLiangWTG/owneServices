using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportJobDeclarationValidation : JobDeclarationValidation
	{
		public ExportJobDeclarationValidation(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		#region Overridden Validation

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			MessageValidation.CheckEntered(JobDeclaration.JE_OH_SupplierInfo);
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			MessageValidation.CheckEntered(JobDeclaration.JE_RL_NKPortOfLoadingInfo);
			if (JobDeclaration.PortOfLoading != null && JobDeclaration.PortOfLoading.Country != null && JobDeclaration.PortOfLoading.RL_RN_NKCountryCode != "AU")
			{
				JobDeclaration.JE_RL_NKPortOfLoadingInfo.AddMessageError("Port of loading must be an Australian port.");
			}
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			MessageValidation.CheckEntered(JobDeclaration.JE_RL_NKPortOfArrivalInfo);
			if (JobDeclaration.PortOfArrival != null && JobDeclaration.PortOfArrival.RL_RN_NKCountryCode == "AU")
			{
				JobDeclaration.JE_RL_NKPortOfArrivalInfo.AddMessageError("Port of discharge must be an overseas port.");
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (!JobDeclaration.IsExWarehouse && !JobDeclaration.ExportGoodsTypeIsPostal)
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_TransportModeInfo);
			}
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			MessageValidation.CheckEntered(JobDeclaration.JE_RL_NKOriginInfo);
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if ((JobDeclaration.IsSea || JobDeclaration.IsAir) && (JobDeclaration.ExportGoodsTypeIsSpares || JobDeclaration.ExportGoodsTypeIsStores))
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_VoyageFlightNoInfo);
			}
			if (!JobDeclaration.JE_VoyageFlightNoInfo.HasNotifications() && JobDeclaration.IsAir && (JobDeclaration.ExportGoodsTypeIsSpares || JobDeclaration.ExportGoodsTypeIsStores))
			{
				ZString warningMessage = FlightNoValidation.ValidateFlightNo(JobDeclaration.JE_VoyageFlightNoInfo);
				if (!warningMessage.IsEmpty)
				{
					JobDeclaration.JE_VoyageFlightNoInfo.AddWarning(warningMessage);
				}
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (JobDeclaration.JE_ExportDate.IsEmpty || !JobDeclaration.JE_ExportDate.IsValid)
			{
				JobDeclaration.JE_ExportDateInfo.AddMessageError("Valid export date is required");
			}
		}

		#endregion
	}
}
