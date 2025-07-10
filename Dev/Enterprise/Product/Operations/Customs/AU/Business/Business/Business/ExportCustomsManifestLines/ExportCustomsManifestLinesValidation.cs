using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestLinesValidation : Customs.Business.ExportCustomsManifestLinesValidation
	{
		public ExportCustomsManifestLinesValidation(ExportCustomsManifestLines parent)
			: base(parent)
		{
		}

		#region Overridden Validation

		protected override void CheckEL_WeightUQ()
		{
			base.CheckEL_WeightUQ();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Line.EL_WeightUQInfo, Line.EL_Weight_List);
		}

		protected override void CheckEL_VolumeUQ()
		{
			base.CheckEL_VolumeUQ();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Line.EL_VolumeUQInfo, Line.EL_Volume_List);
		}

		protected override void CheckEL_TypeOfCAN()
		{
			base.CheckEL_TypeOfCAN();
			MandatoryValidation.WarnIfNotEntered(Line.EL_TypeOfCANInfo);
			ListValidation.MessageErrorIfInvalidCode(Line.EL_TypeOfCANInfo, Line.Lookups.TypeOfCANs);

			ValidateEL_RN_NKCountryOfDestination();
			ValidateEL_GoodsDescription();
			ValidateEL_GoodsOwner();
			ValidateEL_GoodsOwnerPartyID();
		}

		protected override void CheckEL_AirWayBill()
		{
			base.CheckEL_AirWayBill();
			if ((Line.Header.IsAir && (Line.Header.IsMainManifest || Line.Header.IsCTO)) || (Line.Header.IsAirCTOHeader && Line.Header.IsConsolidation))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Line.EL_AirWayBillInfo);
				if (!Line.Header.IsConsolidation && !Line.EL_AirWayBill.IsEmpty)
				{
					var validator = new AirWayBillValidator();
					validator.ValidateAndAddMessageError(Line.EL_AirWayBillInfo);
				}
			}
		}

		protected override void CheckEL_CAN()
		{
			base.CheckEL_CAN();
			if (Line.IsCANLine)
			{
				new Common.AU.CMR.CANValidation().ValidateCANField(Line.EL_CANInfo);
				if (IsCANIsDuplicated(Line.EL_CAN))
				{
					Line.EL_CANInfo.AddMessageError("This CAN appears elsewhere on the manifest");
				}
			}
			else if (Line.IsExemptLine)
			{
				if (Line.IsCTO)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Line.EL_CANInfo, "Exemption Number");
				}
				else if (!Line.EL_CAN.IsEmpty)
				{
					Line.EL_CANInfo.AddMessageError("A CAN is not required for exempt lines");
				}
			}
			else if (Line.IsCCANLine)
			{
				if (Line.EL_CAN.Length != 14)
				{
					Line.EL_CANInfo.AddMessageError("A C-CAN must be 14 characters long");
				}
			}
		}

		protected override void CheckEL_NumberOfPackages()
		{
			base.CheckEL_NumberOfPackages();
			if (Line.EL_NumberOfPackages < 0)
			{
				Line.EL_NumberOfPackagesInfo.AddMessageError("The number of pacakges can not be negative");
			}
			else if (Line.Header != null && Line.Header.IsAir && Line.EL_NumberOfPackages == 0 && !Line.IsCTO)
			{
				Line.EL_NumberOfPackagesInfo.AddMessageError("The number of packages must be greater than 0 for air manifests");
			}
			else if (Line.EL_NumberOfPackages == 0 && Line.Header.IsSea && Line.EL_NumberOfContainers == 0)
			{
				Line.EL_NumberOfPackagesInfo.AddMessageError("The number of packages must be greater than 0 when there are 0 containers");
			}
		}

		protected override void CheckEL_NumberOfContainers()
		{
			base.CheckEL_NumberOfContainers();
			if (Line.EL_NumberOfContainers < 0)
			{
				Line.EL_NumberOfContainersInfo.AddMessageError("The number of containers can not be negative");
			}
			else if (Line.Header != null && Line.Header.IsAir && Line.EL_NumberOfContainers != 0)
			{
				Line.EL_NumberOfContainersInfo.AddMessageError("The number of containers must be 0 for air manifests");
			}
			else if (Line.Header.IsSea && Line.EL_NumberOfPackages == 0 && Line.EL_NumberOfContainers == 0)
			{
				Line.EL_NumberOfContainersInfo.AddMessageError("The number of containers must be greater than 0 when there are 0 packages and the mode of transport is Sea.");
			}
		}

		protected override void CheckEL_GoodsDescription()
		{
			base.CheckEL_GoodsDescription();
			if (Line.IsExemptLine && Line.EL_GoodsDescription.IsEmpty)
			{
				Line.EL_GoodsDescriptionInfo.AddMessageError("A Goods Description is required when using an Exemption Code.");
			}
		}

		protected override void CheckEL_GoodsOwner()
		{
			base.CheckEL_GoodsOwner();
			if (IsGoodsOwnerRequired)
			{
				Line.EL_GoodsOwnerInfo.AddMessageError(missingOwnerInformationErrorMessage);
			}

			ValidateEL_GoodsOwnerPartyID();
		}

		protected override void CheckEL_GoodsOwnerPartyID()
		{
			base.CheckEL_GoodsOwnerPartyID();
			if (IsGoodsOwnerRequired)
			{
				Line.EL_GoodsOwnerPartyIDInfo.AddMessageError(missingOwnerInformationErrorMessage);
			}

			ValidateEL_GoodsOwner();
		}

		bool IsGoodsOwnerRequired
		{
			get
			{
				return Line.Header != null
					&& ((!Line.Header.IsMainManifest && Line.IsPersonalEffectsOrLowValue) || (Line.Header.IsAirCTOHeader && (Line.Header.IsCTO || Line.Header.IsMainManifest || Line.Header.IsConsolidation)))
					&& Line.Owner == null
					&& Line.EL_GoodsOwner.IsEmpty
					&& Line.EL_GoodsOwnerPartyID.IsEmpty;
			}
		}

		protected string missingOwnerInformationErrorMessage = "Either a Goods Owner Party ID or an Owner Name is required.";

		protected override void CheckEL_RN_NKCountryOfDestination()
		{
			base.CheckEL_RN_NKCountryOfDestination();
			if (Line.IsExemptLine)
			{
				if (Line.EL_RN_NKCountryOfDestination.IsEmpty)
				{
					Line.EL_RN_NKCountryOfDestinationInfo.AddMessageError("A Country/Region of Destination is required when using an Exemption Code.");
				}
			}
			if (!Line.EL_RN_NKCountryOfDestination.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Line.EL_RN_NKCountryOfDestinationInfo, Line.Lookups.CountryOfDestinations);
			}
		}

		#endregion

		protected bool IsCANIsDuplicated(ZString cAN)
		{
			int occurances = 0;
			foreach (ExportCustomsManifestLines manifestLine in Line.Header.Lines)
			{
				if (manifestLine.EL_CAN == cAN)
				{
					occurances++;
				}
			}
			return occurances > 1;
		}
		protected ExportCustomsManifestLines Line
		{
			get
			{
				return (ExportCustomsManifestLines)Parent;
			}
		}
	}
}
