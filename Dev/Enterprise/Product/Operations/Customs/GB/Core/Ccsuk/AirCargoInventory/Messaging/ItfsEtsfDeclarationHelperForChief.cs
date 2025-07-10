using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Helpers;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class ItfsEtsfDeclarationHelperForChief
	{
		public ItfsEtsfDeclarationHelperForChief(Business.Declaration.JobDeclaration declaration)
		{
			this.declaration = declaration;
			Initialise();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's really not that complicated")]
		void Initialise()
		{
			ReasonForNotEligible = "This is not an inventory-control CCS-UK air import declaration";
			if (declaration.IsInventoryControlledAirImport && declaration.ZG_Gateway == GatewayList.Codes.CCSUKviaNTMsgGW)
			{
				ReasonForNotEligible = "The declarant is not an AEO";
				if (!AeoNumber.IsEmpty)
				{
					ReasonForNotEligible = "No linked CCS-UK air waybill was found";
					FindAwb();
					if (Awb != null)
					{
						ReasonForNotEligible = "No inbound P5 inter-shed removal advice message was found for " + Awb.HumanReadableName;
						var p5Report = Awb.Messages.OfType<EDIMessage>().FirstOrDefault(m => m.EM_MessageSubType == "P5");
						if (p5Report == null)
						{
							var hawb = Awb as CusHAWB;
							if (hawb != null)
							{
								p5Report = hawb.MAWB.Messages.OfType<EDIMessage>().FirstOrDefault(m => m.EM_MessageSubType == "P5");
								if (p5Report != null)
								{
									Awb = hawb.MAWB;
								}
							}
						}
						if (p5Report != null)
						{
							var parser = new FsaParser(p5Report);
							var result = parser.Parse();
							if (result.ChildConsignments.Any())
							{
								var oldShedSegment = result.ChildConsignments.FirstOrDefault(c => c.OldOrNewDataIndicator == "OLD");
								var newShedSegment = result.ChildConsignments.FirstOrDefault(c => c.OldOrNewDataIndicator == "NEW");
								if (oldShedSegment != null && oldShedSegment.InwardLeg != null && oldShedSegment.InwardLeg.Locations != null && oldShedSegment.InwardLeg.Locations.AirportOfArrival != null)
								{
									oldItsfAirportAndShed = oldShedSegment.InwardLeg.Locations.AirportOfArrival.LocationCode + oldShedSegment.InwardLeg.Locations.AirportOfArrival.ShedOperator;
								}
								if (newShedSegment != null && newShedSegment.InwardLeg != null && newShedSegment.InwardLeg.Locations != null && newShedSegment.InwardLeg.Locations.AirportOfArrival != null)
								{
									onwardEtsfAirportAndShed = newShedSegment.InwardLeg.Locations.AirportOfArrival.LocationCode + newShedSegment.InwardLeg.Locations.AirportOfArrival.ShedOperator;
								}
							}
							ReasonForNotEligible = "Shed codes could not be extracted from message #" + p5Report.EM_MessageNum + " on " + Awb.HumanReadableName;
							if (!onwardEtsfAirportAndShed.IsEmpty && !oldItsfAirportAndShed.IsEmpty)
							{
								ReasonForNotEligible = "";
								IsEligibleForItsfEntry = true;
							}
						}
					}
				}
			}
		}

		public string ReasonForNotEligible { get; private set; }

		public bool IsEligibleForItsfEntry
		{
			get;
			private set;
		}

		public string GetConfirmationText()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} is being inter-shed removed from {1} to {2}. Under the guidelines of CIP(14)21, you may send a CHIEF entry for this consignment before it has left the old shed. To do so you must update box 30, the Master UCR and create an Additional Information statement GEN51. Would you like {3} to do this for you?", Awb.HumanReadableName, oldItsfAirportAndShed, onwardEtsfAirportAndShed, Core.Constants.ProductName);
		}

		public string Update()
		{
			declaration.JE_LocationOfGoods = oldItsfAirportAndShed.Left(3);
			declaration.SubLocation = oldItsfAirportAndShed.Right(3);
			var aiStatement = FindOrMakeGen51AiStatement();
			aiStatement.CSI_Description = onwardEtsfAirportAndShed + " " + AeoNumber;
			declaration.JE_MasterUCR = new Business.Declaration.MasterUCRCalculator().Calculate(declaration);
			return "Declaration updated successfully.";
		}

		string aeoNumber;
		ZString AeoNumber
		{
			get { return aeoNumber ?? (aeoNumber = GetAeoNumber()); }
		}

		string GetAeoNumber()
		{
			ZString result = "";
			var orgHeader = declaration.Declarant != null ? declaration.Declarant.Header : null;
			if (orgHeader != null)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator);
				var orgCusCodes = (OrgCusCode[])orgHeader.CustomsCodes.Find(query);
				result = orgCusCodes.Any() ? orgCusCodes[0].OK_RN_NKCodeCountry + orgCusCodes[0].OK_CustomsRegNo : "";
			}
			return result;
		}

		AdditionalInfo FindOrMakeGen51AiStatement()
		{
			var gen51 = Business.Declaration.MasterUCRCalculator.FindGen51Statement(declaration);
			if (gen51 == null)
			{
				if (declaration.Invoices.Count == 0)
				{
					declaration.Invoices.AddNew();
				}
				if (declaration.Invoices[0].InvoiceLines.Count == 0)
				{
					declaration.Invoices[0].InvoiceLines.AddNew();
				}
				gen51 = declaration.InvoiceLines[0].AdditionalInfos.AddNew();
				gen51.CSI_Code = "GEN51";
			}
			return gen51;
		}

		internal void FindAwb()
		{
			var baseHawb = GbCcsukMUCREntryNumValidation.FindBaseAwbFromFK(declaration).Item1;
			if (baseHawb == null)
			{
				var matches = Business.Declaration.MasterUCRHelper.PreparePatternMatch(declaration.JE_MasterUCR);
				if (matches != null && matches.Count > 0)
				{
					var mawbOrBasic = Business.Declaration.MasterUCRHelper.FindMawbFromPatternMatch(matches, declaration.Factory) as CusMAWB;
					if (mawbOrBasic != null)
					{
						var optionalHawbFromMucr = Business.Declaration.MasterUCRHelper.GetMatchedCode(matches, "HAWB");
						if (!optionalHawbFromMucr.IsEmpty)
						{
							var hawb = Business.Declaration.MasterUCRHelper.FindHawbFromPattern(optionalHawbFromMucr, mawbOrBasic.PK, declaration.Factory) as CusHAWB;
							if (hawb != null)
							{
								Awb = hawb;
							}
						}
						else
						{
							Awb = mawbOrBasic;
						}
					}
				}
			}
			else
			{
				var workerOrReal = baseHawb as CusHAWB;
				Awb = workerOrReal != null && workerOrReal.CS_IsMasterHouse ? workerOrReal.MAWB : workerOrReal;
			}
		}

		public ICcsukCusAwb Awb { get; private set; }
		ZString onwardEtsfAirportAndShed;
		ZString oldItsfAirportAndShed;
		readonly Business.Declaration.JobDeclaration declaration;
	}
}
