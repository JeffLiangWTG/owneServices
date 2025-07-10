using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AddInfoQuarantineExDocHeaderValidation : AutoAddInfoQuarantineExDocHeaderValidation
	{
		public AddInfoQuarantineExDocHeaderValidation(AutoAddInfoQuarantineExDocHeader parent)
			: base(parent)
		{
			this.parent = parent.Parent as QuarantineExDocHeader;
		}

		protected override void CheckZH_PrintLocation()
		{
			base.CheckZH_PrintLocation();
			ListValidation.ErrorIfInvalidCode(parent.QH_PrintLocationInfo, parent.Lookups.LocationWithAqisPlace);
		}

		protected override void CheckZH_StorageLocation()
		{
			base.CheckZH_StorageLocation();
			ListValidation.ErrorIfInvalidCode(parent.QH_StorageLocationInfo, parent.Lookups.Location);
		}

		protected override void CheckZH_AuthorisationLocation()
		{
			base.CheckZH_AuthorisationLocation();
			ListValidation.ErrorIfInvalidCode(parent.QH_AuthorisationLocationInfo, parent.Lookups.LocationWithAqisPlace);
		}

		protected override void CheckZH_ForwardLocation()
		{
			base.CheckZH_ForwardLocation();
			ListValidation.ErrorIfInvalidCode(parent.QH_ForwardLocationInfo, parent.Lookups.Location);
		}

		protected override void CheckZH_TransferEDIUserLocation()
		{
			base.CheckZH_TransferEDIUserLocation();
			ListValidation.ErrorIfInvalidCode(parent.QH_TransferEDIUserLocationInfo, parent.Lookups.Location);
		}

		protected override void CheckZH_TransferExporterLocation()
		{
			base.CheckZH_TransferExporterLocation();
			ListValidation.ErrorIfInvalidCode(parent.QH_TransferExporterLocationInfo, parent.Lookups.Location);
		}

		protected override void CheckZH_ApprovedCertifier()
		{
			base.CheckZH_ApprovedCertifier();
			if (parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QH_ApprovedCertifierInfo, parent.Lookups.EXDOCApprovedCertifiers);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(parent.QH_ApprovedCertifierInfo, "Approved Certifier for products other than meat");
			}
		}

		protected override void CheckZH_AvAnimalAge()
		{
			base.CheckZH_AvAnimalAge();
			if (parent.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QH_AvAnimalAgeInfo, parent.Lookups.EXDOCAverageAgeOfAnimalsList);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(parent.QH_AvAnimalAgeInfo, "Average Age of Animals for products other than meat");
			}
		}

		protected override void CheckZH_TrueAndCompleteIndicator()
		{
			if (!parent.IsNEXDOCSActive)
			{
				var commoditiesWhichMustHaveTrueAndCompleteIndicators = parent.Factory.GetCachedValue("AUQuarantineExDocHeaderValidation.CommoditiesWhichMustHaveTrueAndCompleteIndicators", () => new List<ZString>()
				{
				EXDOCCommodityCodes.Codes.Dairy,
				EXDOCCommodityCodes.Codes.Fish,
				EXDOCCommodityCodes.Codes.Eggs,
				EXDOCCommodityCodes.Codes.Meat,
				});

				ListValidation.MessageErrorIfInvalidCode(parent.QH_TrueAndCompleteIndicatorInfo);
				if (parent.QH_TrueAndCompleteIndicator.IsEmpty && commoditiesWhichMustHaveTrueAndCompleteIndicators.Contains(parent.QH_ProduceType))
				{
					parent.QH_TrueAndCompleteIndicatorInfo.AddMessageError("The true and complete question needs to be answered when produce type is dairy, eggs, fish or meat.");
				}
			}
		}

		readonly QuarantineExDocHeader parent;
	}
}
