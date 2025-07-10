using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceDateConfigurationValidation
	{
		public InvoiceDateConfigurationValidation(IInvoiceDateConfiguration parent)
		{
			this.Parent = parent;
		}

		readonly IInvoiceDateConfiguration Parent;

		#region JobType

		public void ValidateJobType()
		{
			MandatoryValidation.CheckEntered(Parent.JobTypeInfo, (IMultilingualString)ResString.GetMultilingualString("eb734f70-3a22-4c7f-8b5d-250462f7de50", "Job Type"));
			ListValidation.ErrorIfInvalidCode(Parent.JobTypeInfo, Parent.JobTypeList);
			if (!Parent.JobTypeInfo.HasErrors())
			{
				BusinessObjectCollection parentCollection = Parent.ParentCollection;
				if (parentCollection != null)
				{
					foreach (IInvoiceDateConfiguration revRecognition in parentCollection)
					{
						if (revRecognition != Parent)
						{
							if ((Parent.JobType == InvoiceDateConfigurationLookups.JobTypeAdditionalCodes.All || revRecognition.JobType == InvoiceDateConfigurationLookups.JobTypeAdditionalCodes.All || revRecognition.JobType == Parent.JobType)
								&&
								(revRecognition.DirectionCode == "" || revRecognition.DirectionCode == Constants.FreightShipmentDirection.Code.All || Parent.DirectionCode == Constants.FreightShipmentDirection.Code.All || revRecognition.DirectionCode == Parent.DirectionCode)
								&&
								(revRecognition.Mode == "" || revRecognition.Mode == InvoiceDateConfigurationLookups.ModeAdditionalCodes.All || Parent.Mode == InvoiceDateConfigurationLookups.ModeAdditionalCodes.All || revRecognition.Mode == Parent.Mode)
								&&
								(revRecognition.BrokerCode == "" || revRecognition.BrokerCode == InvoiceDateConfigurationLookups.BrokerCodes.All || Parent.BrokerCode == InvoiceDateConfigurationLookups.BrokerCodes.All || revRecognition.BrokerCode == Parent.BrokerCode))
							{
								Parent.JobTypeInfo.AddError(Res.GetString("1366335b-b233-4df7-abdb-e6b648c18648", "At least one more record already sets invoice date configuration behavior for the same Job parameters."));
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
				MandatoryValidation.CheckEntered(Parent.DirectionCodeInfo, Res.GetString("7f67decc-3d73-4c04-8b6b-270de24f22da", "Direction"));
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
				MandatoryValidation.CheckEntered(Parent.ModeInfo, Res.GetString("01b751da-ac0c-42f6-b42c-7034af952f3f", "Mode"));
				ListValidation.ErrorIfInvalidCode(Parent.ModeInfo, Parent.ModeList);

				if (!Parent.ModeInfo.HasErrors())
				{
					if ((Parent.JobType == "CSH" || Parent.JobType == "CLL") &&
						(Parent.Mode != Core.Constants.TransportModes.Air &&
						Parent.Mode != Core.Constants.TransportModes.Sea &&
						Parent.Mode != Core.Constants.TransportModes.Road &&
						Parent.Mode != Core.Constants.TransportModes.Rail &&
						Parent.Mode != InvoiceDateConfigurationLookups.ModeAdditionalCodes.All))
					{
						Parent.ModeInfo.AddError(Res.GetString("69f89146-36ea-44a0-a07c-01e42364fc4a", "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type."));
					}
					else if (Parent.JobType == "SHP" && Parent.Mode != "AIR" && Parent.SignificantDateCode == InvoiceDateConfigurationLookups.SignificantDateCodes.AWBIssueDate)
					{
						Parent.ModeInfo.AddError(Res.GetString("e8740c1e-0939-493a-98e0-c2b9c310af79", "You can only use the 'AWB Issue Date' Recognition Option for Shipments with a transport mode of 'AIR'."));
					}
				}
			}
		}

		#endregion

		#region SignificantDateCode

		public void ValidateSignificantDateCode()
		{
			MandatoryValidation.CheckEntered(Parent.SignificantDateCodeInfo, (IMultilingualString)ResString.GetMultilingualString("668de67b-a04f-4461-87fc-7024a005340c", "Significant Date"));
			ListValidation.ErrorIfInvalidCode(Parent.SignificantDateCodeInfo, Parent.SignificantDateList);
		}

		#endregion

		#region Broker

		public void ValidateBrokerCode()
		{
			if (!Parent.BrokerCodeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.BrokerCodeInfo, Res.GetString("a4100243-d479-467f-8909-edc89f2bb611", "Broker"));
				ListValidation.ErrorIfInvalidCode(Parent.BrokerCodeInfo, Parent.BrokerList);
			}
		}

		#endregion

		#region PriorClosedPeriod

		public void ValidatePriorClosedPeriod()
		{
			if (Parent.SignificantDateCode != InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate)
			{
				MandatoryValidation.CheckEntered(Parent.PriorClosedPeriodInfo, (IMultilingualString)ResString.GetMultilingualString("45b935bb-e1e5-43a9-abb4-6031afa4079c", "Prior Closed Period"));
				ListValidation.ErrorIfInvalidCode(Parent.PriorClosedPeriodInfo, Parent.PriorClosedPeriodList);
			}
		}

		#endregion

		#region PriorOpenPeriod

		public void ValidatePriorOpenPeriod()
		{
			if (Parent.SignificantDateCode != InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate)
			{
				MandatoryValidation.CheckEntered(Parent.PriorOpenPeriodInfo, (IMultilingualString)ResString.GetMultilingualString("cec408d6-4c38-4116-be27-e1bf6da1eb5d", "Prior Open Period"));
				ListValidation.ErrorIfInvalidCode(Parent.PriorOpenPeriodInfo, Parent.PriorOpenPeriodList);
			}
		}

		#endregion

		#region CurrentPeriod

		public void ValidateCurrentPeriod()
		{
			MandatoryValidation.CheckEntered(Parent.CurrentPeriodInfo, (IMultilingualString)ResString.GetMultilingualString("c5440590-3522-4d3f-8f73-043786fbf2cd", "Current Period"));
			ListValidation.ErrorIfInvalidCode(Parent.CurrentPeriodInfo, Parent.CurrentPeriodList);
		}

		#endregion

		#region FuturePeriod

		public void ValidateFuturePeriod()
		{
			if (Parent.SignificantDateCode != InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate)
			{
				MandatoryValidation.CheckEntered(Parent.FuturePeriodInfo, (IMultilingualString)ResString.GetMultilingualString("6c2b1a7f-cb3b-44fb-b9f5-54e475f6fc8c", "Future Period"));
				ListValidation.ErrorIfInvalidCode(Parent.FuturePeriodInfo, Parent.FuturePeriodList);
			}
		}

		#endregion

		#region Override

		public void ValidateOverride()
		{
			if (Parent.Override && !Parent.Today)
			{
				Parent.OverrideInfo.AddError(Res.GetString("79086e95-23ae-4cb6-9eaa-d766409c17ca", "When ticking Override, Today must also be ticked."));
			}
		}

		#endregion

		#region Today

		public void ValidateToday()
		{
			Parent.ValidateOverride();
		}

		#endregion

		#region ReversalRule

		public void ValidateReversalRule()
		{
			MandatoryValidation.CheckEntered(Parent.ReversalRuleInfo, (IMultilingualString)ResString.GetMultilingualString("4131b1ae-1e7b-4a9b-9db0-4df68c307cd8", "Reversal Rule"));
			ListValidation.ErrorIfInvalidCode(Parent.ReversalRuleInfo, Parent.ReversalRuleList);
		}

		#endregion
	}
}