using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class PostDateConfigurationValidation
	{
		public PostDateConfigurationValidation(PostDateConfiguration parent)
		{
			this.Parent = parent;
		}

		readonly PostDateConfiguration Parent;

		#region JobType

		public void ValidateJobType()
		{
			MandatoryValidation.CheckEntered(Parent.JobTypeInfo, (IMultilingualString)ResString.GetMultilingualString("ca8196b1-acb2-4680-a398-c645a717d90e", "Job Type"));
			ListValidation.ErrorIfInvalidCode(Parent.JobTypeInfo, Parent.JobTypeList);
			if (!Parent.JobTypeInfo.HasErrors())
			{
				BusinessObjectCollection parentCollection = Parent.ParentCollection;
				if (parentCollection != null)
				{
					foreach (PostDateConfiguration config in parentCollection)
					{
						if (config != Parent)
						{
							if ((Parent.JobType == PostDateConfigurationLookups.JobTypeAdditionalCodes.All || config.JobType == PostDateConfigurationLookups.JobTypeAdditionalCodes.All || config.JobType == Parent.JobType)
								&&
								(config.DirectionCode == "" || config.DirectionCode == Constants.FreightShipmentDirection.Code.All || Parent.DirectionCode == Constants.FreightShipmentDirection.Code.All || config.DirectionCode == Parent.DirectionCode)
								&&
								(config.Mode == "" || config.Mode == PostDateConfigurationLookups.ModeAdditionalCodes.All || Parent.Mode == PostDateConfigurationLookups.ModeAdditionalCodes.All || config.Mode == Parent.Mode)
								&&
								(config.BrokerCode == "" || config.BrokerCode == PostDateConfigurationLookups.BrokerCodes.All || Parent.BrokerCode == PostDateConfigurationLookups.BrokerCodes.All || config.BrokerCode == Parent.BrokerCode))
							{
								Parent.JobTypeInfo.AddError(Res.GetString("02664ed4-0e46-4f35-95bf-06f218a16be4", "At least one more record already sets invoice date configuration behavior for the same Job parameters."));
								break;
							}
						}
					}
				}
			}
			if (!Parent.JobTypeInfo.HasErrors())
			{
				Parent.ValidateDirectionCode();
				Parent.ValidateMode();
				Parent.ValidateBrokerCode();
				Parent.ValidateSignificantDateCode();
				Parent.ValidatePriorClosedPeriod();
				Parent.ValidatePriorOpenPeriod();
				Parent.ValidateCurrentPeriod();
				Parent.ValidateFuturePeriod();
			}
		}

		#endregion

		#region Direction

		public void ValidateDirectionCode()
		{
			if (!Parent.DirectionCodeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.DirectionCodeInfo, Res.GetString("ff5fe95f-8027-439a-8dc7-18db670c6f4b", "Direction"));
				ListValidation.ErrorIfInvalidCode(Parent.DirectionCodeInfo, Parent.DirectionList);

				if (!Parent.DirectionCodeInfo.HasErrors())
				{
					Parent.ValidateBrokerCode();
				}
			}
		}

		#endregion

		#region Mode

		public void ValidateMode()
		{
			if (!Parent.ModeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ModeInfo, Res.GetString("c86553bb-f147-4136-b350-a5c40f10045b", "Mode"));
				ListValidation.ErrorIfInvalidCode(Parent.ModeInfo, Parent.ModeList);

				if (!Parent.ModeInfo.HasErrors())
				{
					if ((Parent.JobType == "CSH" || Parent.JobType == "CLL") &&
						(Parent.Mode != Core.Constants.TransportModes.Air &&
						Parent.Mode != Core.Constants.TransportModes.Sea &&
						Parent.Mode != Core.Constants.TransportModes.Road &&
						Parent.Mode != Core.Constants.TransportModes.Rail &&
						Parent.Mode != PostDateConfigurationLookups.ModeAdditionalCodes.All))
					{
						Parent.ModeInfo.AddError(Res.GetString("198e6b79-c09f-402c-befe-6344965007af", "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type."));
					}
					else if (Parent.JobType == "SHP" && Parent.Mode != "AIR" && Parent.SignificantDateCode == PostDateConfigurationLookups.SignificantDateCodes.AWBIssueDate)
					{
						Parent.ModeInfo.AddError(Res.GetString("3fdb3205-cdbe-43b8-9a2a-6172c07bd66b", "You can only use the 'AWB Issue Date' Significant Date for Shipments with a transport mode of 'AIR'."));
					}
				}
			}
		}

		#endregion

		#region SignificantDateCode

		public void ValidateSignificantDateCode()
		{
			MandatoryValidation.CheckEntered(Parent.SignificantDateCodeInfo, (IMultilingualString)ResString.GetMultilingualString("69f51b45-594b-4763-ae70-3619cdb34122", "Significant Date"));
			ListValidation.ErrorIfInvalidCode(Parent.SignificantDateCodeInfo, Parent.SignificantDateList);
		}

		#endregion

		#region Broker

		public void ValidateBrokerCode()
		{
			if (!Parent.BrokerCodeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.BrokerCodeInfo, Res.GetString("16c41ac6-7d42-4a77-92bd-2b8e228f7e93", "Broker"));
				ListValidation.ErrorIfInvalidCode(Parent.BrokerCodeInfo, Parent.BrokerList);
			}
		}

		#endregion

		#region PriorClosedPeriod

		public void ValidatePriorClosedPeriod()
		{
			if (!Parent.PriorClosedPeriodInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.PriorClosedPeriodInfo, (IMultilingualString)ResString.GetMultilingualString("1df3beb6-194c-45f4-a359-a5bad406ca8f", "Prior Closed Period"));
				ListValidation.ErrorIfInvalidCode(Parent.PriorClosedPeriodInfo, Parent.PriorClosedPeriodList);
			}
		}

		#endregion

		#region PriorOpenPeriod

		public void ValidatePriorOpenPeriod()
		{
			if (!Parent.PriorOpenPeriodInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.PriorOpenPeriodInfo, (IMultilingualString)ResString.GetMultilingualString("a2f99128-d282-48c0-863d-50ac4626dbd8", "Prior Open Period"));
				ListValidation.ErrorIfInvalidCode(Parent.PriorOpenPeriodInfo, Parent.PriorOpenPeriodList);
			}
		}

		#endregion

		#region CurrentPeriod

		public void ValidateCurrentPeriod()
		{
			MandatoryValidation.CheckEntered(Parent.CurrentPeriodInfo, (IMultilingualString)ResString.GetMultilingualString("7b23b2c7-bbd1-42f7-a7e3-1c58773d9a95", "Current Period"));
			ListValidation.ErrorIfInvalidCode(Parent.CurrentPeriodInfo, Parent.CurrentPeriodList);
		}

		#endregion

		#region FuturePeriod

		public void ValidateFuturePeriod()
		{
			if (!Parent.FuturePeriodInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.FuturePeriodInfo, (IMultilingualString)ResString.GetMultilingualString("9a2ce02f-0757-47f4-8fe2-ef9b88cc7ec2", "Future Period"));
				ListValidation.ErrorIfInvalidCode(Parent.FuturePeriodInfo, Parent.FuturePeriodList);
			}
		}

		#endregion

		#region ReversalRule

		public void ValidateReversalRule()
		{
			MandatoryValidation.CheckEntered(Parent.ReversalRuleInfo, (IMultilingualString)ResString.GetMultilingualString("0c844f64-ac35-41d0-a339-d8fbd372cc7d", "Reversal Rule"));
			ListValidation.ErrorIfInvalidCode(Parent.ReversalRuleInfo, Parent.ReversalRuleList);
		}

		#endregion
	}
}