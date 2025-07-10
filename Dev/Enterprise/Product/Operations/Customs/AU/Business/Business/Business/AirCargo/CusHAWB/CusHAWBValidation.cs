using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusHAWBValidation : Customs.Business.CusHAWBValidation
	{
		protected CusHAWBValidation(CusHAWBBase parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			try
			{
				isValidatingAll = true;
				base.ValidateAll();
				ValidateCS_fPartShipConsignmentReference();
			}
			finally
			{
				isValidatingAll = false;
			}
		}

		protected bool isValidatingAll;

		public void ValidateCS_fPartShipConsignmentReference()
		{
			ValidateCalculatedProperty(HAWB.CS_fPartShipConsignmentReferenceInfo);
		}

		protected virtual void CheckCS_fPartShipConsignmentReference()
		{
			if (HAWB.MAWB != null && HAWB.MAWB.IsAltPartShipModelActive)
			{
				var query = new ZDBOnlyQuery(typeof(CusHAWBBase));
				query.AddToFilter(CusHAWBSchema.CS_HAWB, HAWB.CS_HAWB);
				query.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, HAWB.PK);
				foreach (CusHAWBBase hawb in CusHAWBBase.LoadAllFromQuery(query, HAWB.Factory, loadRecentOnly: true))
				{
					if (hawb.CS_fPartShipConsignmentReference != HAWB.CS_fPartShipConsignmentReference)
					{
						HAWB.CS_fPartShipConsignmentReferenceInfo.AddMessageError(string.Format("MAWB {0} also contains this HAWB but has a different Coinsignment Reference specified ({1})",
							hawb.MAWB != null ? hawb.MAWB.CM_MAWB.ToString() : "?", hawb.CS_fPartShipConsignmentReference));
					}
				}
			}
		}

		protected override void CheckCS_ConsigneeName()
		{
			base.CheckCS_ConsigneeName();
			MessageErrorIfNoAlphaChars(HAWB.CS_ConsigneeNameInfo);
		}

		protected override void CheckCS_ConsigneeStreet()
		{
			base.CheckCS_ConsigneeStreet();
			if (!HAWB.CS_ConsigneeStreet.IsEmpty && HAWB.AddressInfo.IsAddressAPOBox(HAWB.CS_ConsigneeStreet, ZString.Empty))
			{
				HAWB.CS_ConsigneeStreetInfo.AddWarning("It is suggested that Consignee street is not of PO Box type.");
			}
		}

		protected override void CheckCS_ResponsiblePartyID()
		{
			base.CheckCS_ResponsiblePartyID();
			if (HAWB.CS_ResponsiblePartyID.IsEmpty && HAWB.MAWB != null && HAWB.MAWB.CM_ResponsiblePartyID.IsEmpty)
			{
				HAWB.CS_ResponsiblePartyIDInfo.AddMessageError(ResponsiblePartEmpty);
			}
		}
		internal const string ResponsiblePartEmpty = "You have not entered a Responsible Party Id and the MAWB Responsible party is also blank.";

		protected override void CheckCS_ConsignorName()
		{
			base.CheckCS_ConsignorName();
			MessageErrorIfNoAlphaChars(HAWB.CS_ConsignorNameInfo);
		}

		protected override void CheckCS_RN_NKConsignorCountry()
		{
			base.CheckCS_RN_NKConsignorCountry();

			var consignor = HAWB.Consignor;
			if (consignor == null || consignor.PK != Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation)
			{
				if (!HAWB.CS_RN_NKConsignorCountry.IsEmpty && HAWB.ConsignorCountry == null)
				{
					HAWB.CS_RN_NKConsignorCountryInfo.AddError("Please enter a valid consignor country/region.");
				}
				else if (HAWB.CS_RN_NKConsignorCountry.IsEmpty)
				{
					MandatoryValidation.WarnIfNotEntered(HAWB.CS_RN_NKConsignorCountryInfo);
				}
				else if (HAWB.ConsignorCountry.Code == Core.Constants.CountryCodes.Australia)
				{
					HAWB.CS_RN_NKConsignorCountryInfo.AddMessageError("Consignor should be an overseas party.");
				}
			}
		}

		protected override void CheckCS_RN_NKConsigneeCountry()
		{
			base.CheckCS_RN_NKConsigneeCountry();
			if (!HAWB.CS_RN_NKConsigneeCountry.IsEmpty && HAWB.ConsigneeCountry == null)
			{
				HAWB.CS_RN_NKConsigneeCountryInfo.AddError("Please enter a valid consignee country/region.");
			}
			else if (HAWB.CS_RN_NKConsigneeCountry.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(HAWB.CS_RN_NKConsigneeCountryInfo);
			}
		}

		protected override void CheckCS_GoodsDescription()
		{
			base.CheckCS_GoodsDescription();
			MessageErrorIfNoAlphaChars(HAWB.CS_GoodsDescriptionInfo);
		}

		protected override void CheckCS_HAWB()
		{
			base.CheckCS_HAWB();
			if (Parent.CS_HAWB.IsEmpty)
			{
				Parent.CS_HAWBInfo.AddMessageError("Housebill number is required for air cargo messaging.");
			}
		}

		protected override void CheckCS_RL_NKDestination()
		{
			base.CheckCS_RL_NKDestination();
			if (Parent.Destination == null)
			{
				Parent.CS_RL_NKDestinationInfo.AddMessageError("Destination port is required. Please enter a valid port.");
			}
			else
			{
				ZString portWarning = MessageValidation.ValidatePortType(Parent.CS_RL_NKDestination, true, false);
				if (!portWarning.IsEmpty)
				{
					Parent.CS_RL_NKDestinationInfo.AddWarning(portWarning);
				}

				if (Parent.CS_RL_NKDestination.EndsWith("ZZZ"))
				{
					Parent.CS_RL_NKDestinationInfo.AddMessageError("Destination port is invalid. Please check the job detail and enter a correct port.");
				}
			}

			if (HAWB.MAWB != null && !HAWB.MAWB.CM_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.Australia) && HAWB.CS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.Australia))
			{
				Parent.CS_RL_NKDestinationInfo.AddMessageError("Destination Port must be overseas as the Discharge Port is overseas.");
			}
		}

		protected override void CheckCS_RL_NKOrigin()
		{
			base.CheckCS_RL_NKOrigin();
			if (Parent.Origin == null)
			{
				Parent.CS_RL_NKOriginInfo.AddMessageError("Origin port is required. Please enter a valid port.");
			}
			else
			{
				ZString portWarning = MessageValidation.ValidatePortType(Parent.CS_RL_NKOrigin, true, false);
				if (!portWarning.IsEmpty)
				{
					Parent.CS_RL_NKOriginInfo.AddWarning(portWarning);
				}

				if (Parent.CS_RL_NKOrigin.EndsWith("ZZZ"))
				{
					Parent.CS_RL_NKOriginInfo.AddMessageError("Origin port is invalid. Please check the job detail and enter a correct port.");
				}
				else if (Parent.Origin.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
				{
					Parent.CS_RL_NKOriginInfo.AddMessageError("Origin port should be an overseas one");
				}
			}
		}

		protected override void CheckCS_Weight()
		{
			base.CheckCS_Weight();
			if (Parent.CS_Weight == 0m)
			{
				Parent.CS_WeightInfo.AddMessageError("Weight is required for air cargo messaging.");
			}
			else if (Parent.CS_Weight < 0.001m)
			{
				Parent.CS_WeightInfo.AddMessageError("Weight must be >= 0.001 for air cargo messaging.");
			}
		}

		protected override void CheckCS_WeightUQ()
		{
			base.CheckCS_WeightUQ();
			if (Parent.CS_WeightUQ.IsEmpty)
			{
				Parent.CS_WeightUQInfo.AddMessageError("Unit of weight is required for air cargo messaging.");
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CS_WeightUQInfo, HAWB.Lookups.UnitOfWeightList);
		}

		protected override void CheckCS_FreightPrepaidCollect()
		{
			base.CheckCS_FreightPrepaidCollect();
			if (Parent.CS_FreightPrepaidCollect.IsEmpty && IsFreightPrepaidCollectRequired)
			{
				Parent.CS_FreightPrepaidCollectInfo.AddMessageError(FreightPrepaidOrCollectRequiredMessage);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CS_FreightPrepaidCollectInfo, HAWB.Lookups.PrepaidCollectListForValidation);
		}

		protected override void CheckCS_PiecesManifested()
		{
			base.CheckCS_PiecesManifested();
			if (Parent.CS_PiecesManifested <= 0)
			{
				Parent.CS_PiecesManifestedInfo.AddMessageError("A valid number of pieces manifested is required for air cargo messaging.");
			}
		}

		protected override void CheckCS_GoodsValue()
		{
			base.CheckCS_GoodsValue();
			if (Parent.CS_GoodsValue < 0m)
			{
				Parent.CS_GoodsValueInfo.AddError("Invalid Goods Value");
			}
			else if (Parent.CS_GoodsValue == 0m)
			{
				HAWB.CS_GoodsValueInfo.AddWarning("This will be declared as 'No Commercial Value Consignment'.");
			}
		}

		protected override void CheckCS_CommercialStatus()
		{
			base.CheckCS_CommercialStatus();
			ListValidation.ErrorIfInvalidCode(Parent.CS_CommercialStatusInfo, HAWB.Lookups.CommercialStatusList);
		}

		protected override void CheckCS_ConsigneeBusinessNumber()
		{
			base.CheckCS_ConsigneeBusinessNumber();
			CargoHelper.CheckBusinessNumberOrIdentifierShouldBeEntered(Parent.CS_ConsigneeBusinessNumberInfo, Parent.CS_ConsigneeBusinessNumber, Parent.CS_ConsigneeIdentifier);
		}

		protected override void CheckCS_ConsigneeIdentifier()
		{
			base.CheckCS_ConsigneeIdentifier();
			CargoHelper.CheckBusinessNumberOrIdentifierShouldBeEntered(Parent.CS_ConsigneeIdentifierInfo, Parent.CS_ConsigneeBusinessNumber, Parent.CS_ConsigneeIdentifier);
		}

		#region Implementation

		public virtual CusHAWBBase HAWB
		{
			get { return (CusHAWBBase)Parent; }
		}

		protected virtual bool IsFreightPrepaidCollectRequired
		{
			get { return false; }
		}

		protected virtual ZString FreightPrepaidOrCollectRequiredMessage
		{
			get { return "Prepaid or Collect is required for air cargo messaging."; }
		}

		protected MessageValidation fMessageValidation;
		protected MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(Parent);
				}
				return fMessageValidation;
			}
		}

		void MessageErrorIfNoAlphaChars(ZPropertyInfo info)
		{
			ZString value = (ZString)info.Value;
			if (value.IsEmpty)
			{
				info.AddMessageError(info.Description + " is required");
			}
			else if (!value.ContainsAnyLetters)
			{
				info.AddMessageError(info.Description + " must have at least 1 alpha character");
			}
		}

		#endregion
	}
}
