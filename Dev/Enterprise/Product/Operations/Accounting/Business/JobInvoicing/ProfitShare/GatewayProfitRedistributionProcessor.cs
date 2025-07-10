using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class GatewayProfitRedistributionProcessor
	{
		public GatewayProfitRedistributionProcessor(ForwardingProfitShareRedistribution forwardingProfitShareRedistribution, IDisposableProfitShareRedistributionLogger logger)
		{
			Argument.NotNull(forwardingProfitShareRedistribution, nameof(forwardingProfitShareRedistribution));
			Argument.NotNull(logger, nameof(logger));

			this.profitShareRedistribution = forwardingProfitShareRedistribution;
			this.factory = forwardingProfitShareRedistribution.Factory;
			this.forwardingConsols = forwardingProfitShareRedistribution.Consols.Cast<ProfitShareForwardingConsolWrapper>().ToArray();
			this.orgProfitShareDetailsList = forwardingProfitShareRedistribution.ProfitShareRules.Cast<ProfitShareRedistributionRule>().Select(x => x.ProfitShareDetails).ToArray();
			this.logger = logger;
			this.consolWrappers = forwardingProfitShareRedistribution.Consols.Cast<ProfitShareForwardingConsolWrapper>().DistinctBy(x => x.Consol.PK).ToDictionary(x => x.Consol.PK, x => x);
			this.shipmentWrappers = forwardingProfitShareRedistribution.Shipments.Cast<ProfitShareForwardingShipmentWrapper>().DistinctBy(x => x.Shipment.PK).ToDictionary(x => x.Shipment.PK, x => x);
		}

		readonly ForwardingProfitShareRedistribution profitShareRedistribution;
		readonly BusinessObjectFactory factory;
		readonly IEnumerable<ProfitShareForwardingConsolWrapper> forwardingConsols;
		readonly IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList;
		readonly IDisposableProfitShareRedistributionLogger logger;
		readonly Dictionary<ZGuid, ProfitShareForwardingConsolWrapper> consolWrappers;
		readonly Dictionary<ZGuid, ProfitShareForwardingShipmentWrapper> shipmentWrappers;

		public bool Process()
		{
			var result = true;
			try
			{
				//no test written for this yet
				ResetAllPreviouslyCalculatedValuesToZero();

				var validator = new GatewayProfitRedistributionValidator(factory, forwardingConsols, orgProfitShareDetailsList, logger);
				if (validator.IsValid())
				{
					logger.Information("");//just blank line for better readability

					//Take any rules, GatewayProfitRedistributionValidator will ensure that all have same rules for
					//Apportion Method and Share Lossed and Apply To
					var criteria = new GatewayProfitRedistributionApportionmentCriteria(factory, profitShareRedistribution, orgProfitShareDetailsList.First());
					var shipments = forwardingConsols.SelectMany(x => x.Consol.Shipments.ToArray<ForwardingShipment>()).DistinctBy(x => x.PK).ToList();
					var apportionmentCalculator = new GatewayProfitRedistributionApportionmentCalculator(forwardingConsols, shipments, criteria, logger);
					apportionmentCalculator.CalculateProfitPerShipment();

					var matcher = new GatewayProfitRedistributionMatcher(factory, orgProfitShareDetailsList, logger);

					logger.Information("");//just blank line for better readability
					foreach (var shipment in shipments)
					{
						var sortedConsols = shipment.Consols.Cast<ForwardingConsol>().ToArray();
						MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
						var firstConsol = sortedConsols.FirstOrDefault(f => forwardingConsols.Any(x => f.PK == x.Consol.PK));

						if (firstConsol == null)
						{
							result = false;
							logger.Warning($"Unable to find first consol for {shipment.JS_UniqueConsignRef}");
							continue;
						}

						logger.Information($"{firstConsol.JK_UniqueConsignRef} is selected as the first consol of {shipment.JS_UniqueConsignRef}");

						var matchingOrgProfitShareDetails = matcher.GetBestOrgProfitShareDetails(shipment);

						if (matchingOrgProfitShareDetails == null)
						{
							result = false;
							logger.Warning($"Unable to find matching profit rules for {shipment.JS_UniqueConsignRef}");
							continue;
						}

						logger.Information($"The Best matching profit share rule for {shipment.JS_UniqueConsignRef} is:\r\n{ConvertProfitShareDetailsToString(matchingOrgProfitShareDetails)}");

						//todo:refactoring needed, will do later
						if (consolWrappers == null || !consolWrappers.TryGetValue(firstConsol.PK, out var consolWrapper))
						{
							result = false;
							logger.Warning($"Unable to find valid consol reference for {firstConsol.JK_UniqueConsignRef}");
							continue;
						}

						if (shipmentWrappers == null || !shipmentWrappers.TryGetValue(shipment.PK, out var shipmentWrapper))
						{
							result = false;
							logger.Warning($"Unable to find valid shipment reference for {shipment.JS_UniqueConsignRef}");
							continue;
						}

						var calculator = new GatewayProfitRedistributionCalculator(factory,
							consolWrapper,
							shipmentWrapper,
							matchingOrgProfitShareDetails,
							apportionmentCalculator.GetShipmentShare(shipment.PK),
							profitShareRedistribution);
						var profitShareDetails = calculator.CreateProfitShares();

						if (!profitShareDetails.Any())
						{
							result = false;
							logger.Warning($"Unable to share profit for {shipment.JS_UniqueConsignRef}");
							continue;
						}

						using (var chargeCreator = new GatewayProfitRedistributionChargeCreator(factory, firstConsol, profitShareDetails, shipment.JS_UniqueConsignRef))
						{
							if (!chargeCreator.CreateCharges())
							{
								result = false;
								logger.Warning($"Unable to create profit charges for {shipment.JS_UniqueConsignRef}");
							}
							else
							{
								logger.Information($"Profit Charges are successfully created for {shipment.JS_UniqueConsignRef}");
							}
						}

						logger.OnIndividualCompleted(new ProfitShareRedistributedEventArgs(shipment.PK));

						logger.Information("");//just blank line for better readability
					}

					LogActualTotalProfitAvailableForRedistributionAndOriginallyRedistributed();
					WriteRedistrubtionProcessLogIntoNote(profitShareRedistribution);
				}
				else
				{
					result = false;
				}
			}
			catch (Exception ex)
			{
				result = false;
				logger.Error((NoResString)"Exception:" + ex.Message);
			}
			finally
			{
				logger.OnAllCompleted(EventArgs.Empty);
			}

			return result;
		}

		void ResetAllPreviouslyCalculatedValuesToZero()
		{
			if (profitShareRedistribution != null)
			{
				var currencyCode = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;

				profitShareRedistribution.PSR_TotalProfitShare = 0;
				profitShareRedistribution.PSR_RedistributedProfitShare = 0;
				profitShareRedistribution.PSR_RX_NKCurrency = currencyCode;

				var consolidationProfitShares = profitShareRedistribution.ConsolProfitShares.Cast<ConsolidationProfitShare>();

				if (consolidationProfitShares != null)
				{
					foreach (var consolidationProfitShare in consolidationProfitShares)
					{
						consolidationProfitShare.CPS_TotalConsolProfitShare = 0;
						consolidationProfitShare.CPS_RedistributedConsolProfitShare = 0;
						consolidationProfitShare.CPS_RX_NKCurrency = currencyCode;

						var shipmentProfitShares = consolidationProfitShare.ShipmentProfitShares.Cast<ShipmentProfitShares>();

						if (shipmentProfitShares != null)
						{
							foreach (var shipmentProfitShare in shipmentProfitShares)
							{
								shipmentProfitShare.PSS_PickupAgentShare = 0;
								shipmentProfitShare.PSS_DeliveryAgentShare = 0;
								shipmentProfitShare.PSS_RedistributedAmount = 0;
							}
						}
					}
				}

				if (consolWrappers != null)
				{
					foreach (var consolWrapper in consolWrappers.Values)
					{
						consolWrapper.JK_Calc_TotalProfitAmount = 0;
						consolWrapper.JK_Calc_RedistributedProfitAmount = 0;
					}
				}

				if (shipmentWrappers != null)
				{
					foreach (var shipmentWrapper in shipmentWrappers.Values)
					{
						shipmentWrapper.JS_Calc_PickupAgentProfitShare = 0;
						shipmentWrapper.JS_Calc_DeliveryAgentProfitShare = 0;
					}
				}
			}
		}

		void LogActualTotalProfitAvailableForRedistributionAndOriginallyRedistributed()
		{
			if (profitShareRedistribution != null)
			{
				var currencyCode = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;

				var initialProfit = profitShareRedistribution.PSR_RedistributedProfitShare;
				var actualDistributed = new ZDecimal(0m);

				var consolidationProfitShares = profitShareRedistribution.ConsolProfitShares.Cast<ConsolidationProfitShare>();

				if (consolidationProfitShares != null)
				{
					foreach (var consolidationProfitShare in consolidationProfitShares)
					{
						var shipmentProfitShares = consolidationProfitShare.ShipmentProfitShares.Cast<ShipmentProfitShares>();

						if (shipmentProfitShares != null)
						{
							foreach (var shipmentProfitShare in shipmentProfitShares)
							{
								actualDistributed += shipmentProfitShare.PSS_RedistributedAmount;
							}
						}
					}
				}
				profitShareRedistribution.PSR_RedistributedProfitShare = actualDistributed;
				logger.Information($"Available Profit for redistribution was ({currencyCode}): {initialProfit.ToString()}");
				logger.Information($"Distributed Profit for redistribution is ({currencyCode}): {actualDistributed.ToString()}");
			}
		}

		static string ConvertProfitShareDetailsToString(OrgProfitShareDetails details)
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendLine($"Start Date: {details.O4_StartDate.ToShortDateString()}");
			stringBuilder.AppendLine($"End Date: {details.O4_EndDate.ToShortDateString()}");
			stringBuilder.AppendLine($"Sending Location: {details.O4_SendingPortOrCountry}");
			stringBuilder.AppendLine($"Receiving Location: {details.O4_ReceivingPortOrCountry}");
			stringBuilder.AppendLine($"Mode: {details.O4_FreightMode}");
			stringBuilder.AppendLine($"Agreement Type: {details.O4_AgreementType}");
			stringBuilder.AppendLine($"Share Loses: {details.O4_ShareLosses.ToString()}");
			stringBuilder.AppendLine($"Controlling Customer: {details.ControllingAgent?.OH_Code ?? (NoResString)"Empty"}");
			stringBuilder.AppendLine($"Job Type: {details.O4_JobType}");
			stringBuilder.AppendLine($"Apportionment Method: {details.O4_GatewayProfitApportionmentMethod}");

			foreach (OrgProfitShareParty party in details.PartyDetailsForGatewayProfitShareRedistribution)
			{
				stringBuilder.AppendLine($"Party Type: {party.PartyTypeDescription}");
				stringBuilder.AppendLine($"Party Profit Share: {party.PS_PartyProfitSharePercent.ToStringTrimZeros()}%");
			}

			return stringBuilder.ToString().Trim();
		}

		void WriteRedistrubtionProcessLogIntoNote(IStmNoteParent noteParent)
		{
			if (noteParent == null)
			{
				return;
			}

			var note = noteParent.Notes.AddNew();
			using (note.GetValidationSuspender())
			{
				note.ST_Description = PredefinedNoteTypes.Instance.ProfitShareRedistributionAuditLog.Description;
				note.ST_GC_RelatedCompany = Env.CurrentCompany.PK;
				note.ST_IsCustomDescription = false;
			}

			var noteBuilder = new ZStringBuilder();
			noteBuilder.AppendIfNotEmpty(note.ST_NoteText);
			if (noteBuilder.IsEmpty)
			{
				noteBuilder.Append((NoResString)"User:\t\t\t\t");
				noteBuilder.AppendLine(GlbStaff.CurrentUser.GS_FullName);
				noteBuilder.Append((NoResString)"Time:\t\t\t\t");
				noteBuilder.AppendLine(ZDateTime.Now.ToLongTimeString());
			}

			noteBuilder.AppendLine();
			noteBuilder.Append(this.logger.DumpLogs());
			note.ST_NoteText = noteBuilder.ToString().TrimToFit(note.NoteTextMaxLength);
			note.ReadOnly = true;
		}
	}
}
