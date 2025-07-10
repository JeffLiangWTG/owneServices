using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailValidation : Customs.Business.CusSeaManOBLDetailValidation
	{
		public CusSeaManOBLDetailValidation(CusSeaManOBLDetail parent)
			: base(parent)
		{
			this.detail = parent;
		}

		public const string MultipleOceanBillsInFCLContainerMessage = "Multiple Ocean Bills packed in FCL Container.\r\nCargo Line Type must change to FCX or LCL to allow the container to be shared";

		protected override void CheckBD_LineCargoType()
		{
			base.CheckBD_LineCargoType();

			MessageValidation.CheckEntered(detail.BD_LineCargoTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(detail.BD_LineCargoTypeInfo, detail.Lookups.CargoTypes);

			ValidateBD_ContainerNumber();
			ValidateBD_MarksAndNumbers();
			ValidateBD_NoOfPacks();
			ValidateBD_PackType();
			ValidateBD_RC_ContainerType();

			if (detail.Header != null && (detail.IsBulk || detail.IsBreakBulk))
			{
				bool found = false;
				foreach (CusSeaManOBLDetail otherDetail in detail.Header.Details)
				{
					if (otherDetail != detail && otherDetail.BD_LineCargoType == detail.BD_LineCargoType)
					{
						found = true;
					}
				}

				if (found)
				{
					detail.BD_LineCargoTypeInfo.AddMessageError("Only one " + detail.Lookups.CargoTypes.GetDescriptionFromCode(detail.BD_LineCargoType) + " container allowed.");
				}
			}
		}

		protected override void CheckBD_ContainerNumber()
		{
			base.CheckBD_ContainerNumber();

			if (!detail.IsBreakBulk && !detail.IsBulk)
			{
				if (detail.BD_ContainerNumber.IsEmpty)
				{
					detail.BD_ContainerNumberInfo.AddMessageError("Container Number is required if Cargo Type is not Break Bulk or Bulk.");
				}
				else
				{
					ContainerNumberValidation.WarnIfInvalid(detail.BD_ContainerNumberInfo);
				}
				if (detail.BD_LineCargoType == Enterprise.Core.Constants.ContainerModes.FCL && detail.Header != null && detail.Header.TransportHeader != null)
				{
					foreach (CusSeaManOBLHeader oBLHeader in detail.Header.TransportHeader.OceanBills)
					{
						foreach (CusSeaManOBLDetail otherDetails in oBLHeader.Details)
						{
							if (otherDetails.PK != detail.PK && otherDetails.BD_ContainerNumber == detail.BD_ContainerNumber)
							{
								detail.BD_ContainerNumberInfo.AddMessageError(MultipleOceanBillsInFCLContainerMessage);
							}
						}
					}
				}
			}

			if (detail.BD_ContainerNumber.Length > 17)
			{
				detail.BD_ContainerNumberInfo.AddMessageError("Container Number must be under 18 characters.");
			}
		}

		protected override void CheckBD_GoodsDescription()
		{
			base.CheckBD_GoodsDescription();

			if (detail.BD_GoodsDescription.Length < 2)
			{
				detail.BD_GoodsDescriptionInfo.AddMessageError("Goods Description must be at least two characters in length.");
			}
			else if (!detail.BD_GoodsDescription.ContainsAnyLetters)
			{
				detail.BD_GoodsDescriptionInfo.AddMessageError("Goods Description must contain letters.");
			}
		}

		protected override void CheckBD_MarksAndNumbers()
		{
			base.CheckBD_MarksAndNumbers();

			if (detail.BD_MarksAndNumbers.IsEmpty && (detail.IsLCL || detail.IsBreakBulk))
			{
				detail.BD_MarksAndNumbersInfo.AddMessageError("Marks and Numbers is required if Cargo Type is Less than Container Load or Break Bulk.");
			}
		}

		protected override void CheckBD_NoOfPacks()
		{
			base.CheckBD_NoOfPacks();

			if (!detail.IsBulk && detail.BD_NoOfPacks == 0)
			{
				detail.BD_NoOfPacksInfo.AddMessageError("Number of Packages is required unless Cargo Type is Bulk.");
			}
			else if (!detail.BD_NoOfPacks.IsInRange(0, 9999999))
			{
				detail.BD_NoOfPacksInfo.AddMessageError("Number of Packages must be a number between 1 and 9999999.");
			}
		}

		protected override void CheckBD_PackType()
		{
			base.CheckBD_PackType();

			if (!detail.IsBulk)
			{
				MessageValidation.CheckEntered(detail.BD_PackTypeInfo, "Package Type is required unless Cargo Type is Bulk");

				if (!detail.BD_PackType.IsEmpty)
				{
					ListValidation.WarnIfInvalidCode(detail.BD_PackTypeInfo, detail.Lookups.PackageTypes);
				}
			}
		}

		protected override void CheckBD_GrossWeight()
		{
			base.CheckBD_GrossWeight();

			if (detail.BD_GrossWeight <= 0)
			{
				detail.BD_GrossWeightInfo.AddMessageError("Gross Weight must be a number between 0.01 to 9999999999999.99");
			}
		}

		protected override void CheckBD_GrossWeightUM()
		{
			base.CheckBD_GrossWeightUM();

			ListValidation.MessageErrorIfInvalidCode(detail.BD_GrossWeightUMInfo, detail.Lookups.GrossWeightCodes);
		}

		protected override void CheckBD_CargoVolume()
		{
			base.CheckBD_CargoVolume();

			if (detail.BD_CargoVolume <= 0)
			{
				detail.BD_CargoVolumeInfo.AddMessageError("Cargo Volume must be a number between 0.01 to 9999999999999.99");
			}
		}

		protected override void CheckBD_CargoVolumeUM()
		{
			base.CheckBD_CargoVolumeUM();

			ListValidation.MessageErrorIfInvalidCode(detail.BD_CargoVolumeUMInfo, detail.Lookups.QuantityUnits);
		}

		protected override void CheckBD_SACIndicator()
		{
			base.CheckBD_SACIndicator();

			if (detail.BD_SACIndicator && detail.Header != null && detail.Header.BO_FreightForwarderIndicator)
			{
				detail.BD_SACIndicatorInfo.AddMessageError("Self-Assessed Clearence Indicator cannot be set if Freight Forwarder Indicator on Ocean Bill is set.");
			}

#if DEBUG
			SACHitCount++;
#endif
		}

		protected override void CheckBD_TypeOfContainer()
		{
			base.CheckBD_TypeOfContainer();
			if (!detail.IsBulk && !detail.IsBreakBulk)
			{
				MessageValidation.CheckEntered(detail.BD_TypeOfContainerInfo, "Container Type is required.");
				ListValidation.MessageErrorIfInvalidCode(detail.BD_TypeOfContainerInfo, detail.Lookups.TypesOfContainers);
			}
		}

		protected override void CheckBD_ContainerSizeOrISOCode()
		{
			base.CheckBD_ContainerSizeOrISOCode();
			if (!detail.IsBulk && !detail.IsBreakBulk)
			{
				MessageValidation.CheckEntered(detail.BD_ContainerSizeOrISOCodeInfo, "Container Size is required.");
				ListValidation.MessageErrorIfInvalidCode(detail.BD_ContainerSizeOrISOCodeInfo, detail.Lookups.ContainerSizes);
			}
		}

		#region Implementation

		MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(detail);
				}
				return fMessageValidation;
			}
		}
		MessageValidation fMessageValidation;

		readonly CusSeaManOBLDetail detail;

#if DEBUG
		public int SACHitCount;
#endif

		#endregion
	}
}
