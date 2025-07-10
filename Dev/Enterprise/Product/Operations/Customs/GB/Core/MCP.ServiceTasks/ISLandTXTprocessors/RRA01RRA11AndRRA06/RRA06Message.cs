using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.HoldAdderRemoverClearer;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11
{
	public class RRA06Message : RRA01AndRRA11Message, IPortAuthorityHoldApplicationProvider
	{
		public RRA06Message(ZString messageTextAsReceivedFromCustoms) : base(messageTextAsReceivedFromCustoms)
		{
		}

		protected override List<ZString> ValidMessageIdentifiers => new List<ZString> { "RRA06" };

		protected override ZInt MessageLength => 22;

		public List<ZString> HoldsToAdd { get; private set; } = new List<ZString>();

		public override void SetUpInstanceOfMessageFromString()
		{
			ValidateMessage();

			MessageType = messageReceivedFromCustoms.Substring(1, 5);
			ZString[] elements = messageReceivedFromCustoms.Split(this.delimiter, this.delimiter);
			BerthCode = elements[1].TrimEnd();
			UCN = new UniqueConsignmentNumber(elements[2]); // this will take the full 14 chars and chop it up
			Container = elements[3].TrimEnd();
			ContainerNumber2 = elements[4].TrimEnd();
			MarksAndNumbers = elements[5].TrimEnd();
			BlNumber = elements[6].TrimEnd();
			Packages = ZInt.ParseEmptyAsZero(elements[7]);
			Weight = ZDecimal.Parse(elements[8]);
			SetHolds(elements);

			DateOfStatusEvent = this.ChopUpDateFromString(elements[11]);
			AgentsReference = elements[12];
			CHIEFEntryEPU = elements[13];
			CHIEFEntryNumber = elements[14];
			CHIEFEntryDate = elements[15];
			CHIEFEntryTime = elements[16];
			RemovalType = elements[17];
			RemovalDestination = elements[18];
			RemovalCode = elements[19].SubstringSafe(0, 3).TrimEnd();
			UserName = elements[21].SubstringSafe(0, 20).TrimEnd();
		}

		public override void ApplyHoldsOrClear(CusEntryHeader entryHeader)
		{
			AddClearedEventIfNeeded(entryHeader);
			Business.GbExtensionHelpers.UpdateEntryHeaderToCleared(entryHeader, Date);

			var cusContainer = entryHeader.Declaration.CusContainers.Find(Container);
			if (cusContainer != null)
			{
				ApplyHoldsOrClearForContainer(cusContainer, entryHeader);
			}
		}

		public string ChiefEntryDateInDDMMYY
		{
			get
			{
				if (ZDateTime.TryParseExact(CHIEFEntryDate, out ZDateTime entryDate, "yyMMdd"))
				{
					return entryDate.ToString("ddMMyy", CultureInfo.CurrentCulture);
				}

				return null;
			}
		}

		void ApplyHoldsOrClearForContainer(Customs.Business.BaseCusContainer cusContainer, CusEntryHeader entryHeader)
		{
			var holdProvider = this as IPortAuthorityHoldApplicationProvider;
			if (AreThereAnyHolds())
			{
				if (holdProvider != null)
				{
					var holdString = string.Format(CultureInfo.CurrentCulture, "Hold-{0}", holdProvider.HoldType);
					entryHeader.Logs.AddNew(Events.CustomsEntryStatus, string.Format(CultureInfo.CurrentCulture, "{0}-{1}", holdString, cusContainer.CO_ContainerNumber), holdProvider.Date.ToOffset());
					cusContainer.JobContainer?.Logs.AddNew(Events.CustomsEntryStatus, string.Format(CultureInfo.CurrentCulture, "{0}-{1}-{2}-{3}", holdString, CHIEFEntryEPU, CHIEFEntryNumber, ChiefEntryDateInDDMMYY), holdProvider.Date.ToOffset());
				}
				cusContainer.CO_MessageStatus = EU.Business.Declaration.ContainerStatusCodesList.Codes.HoldIsAdded;
			}
			else
			{
				if (holdProvider != null)
				{
					entryHeader.Logs.AddNew(Events.CustomsEntryStatus, string.Format(CultureInfo.CurrentCulture, "Released by RRA06-{0}", cusContainer.CO_ContainerNumber), holdProvider.Date.ToOffset());
					cusContainer.JobContainer?.Logs.AddNew(Events.CustomsEntryStatus, string.Format(CultureInfo.CurrentCulture, "{0}-{1}-{2}-{3}", "Released by RRA06", CHIEFEntryEPU, CHIEFEntryNumber, ChiefEntryDateInDDMMYY), holdProvider.Date.ToOffset());
				}
				cusContainer.CO_MessageStatus = EU.Business.Declaration.ContainerStatusCodesList.Codes.Released;
			}
		}

		public override void ProcessShipments(CusEntryHeader entryHeader)
		{
			base.ProcessShipments(entryHeader);

			if (IsShipmentDeclarationAndBuyersConsolOrAssemblyMaster(entryHeader.Declaration))
			{
				foreach (ForwardingShipment relatedShipment in GetRelatedShipments(entryHeader.Declaration.Shipment))
				{
					foreach (Customs.Business.BaseJobDeclaration declaration in relatedShipment.Declarations.Where(x => x.IsDeclarationMatchSpecificCountry(CountryCodes.UnitedKingdom)
																													&& !((Customs.Business.BaseJobDeclaration)x).CustomsEntryHeaders.Any()))
					{
						var cusContainer = declaration.CusContainers.Find(Container);
						if (cusContainer != null)
						{
							ApplyHoldsOrClearForContainer(cusContainer, entryHeader);
						}

						if (declaration.JE_EntryStatus != Common.EU.EntryStatusList.Codes.Clear)
						{
							declaration.JE_EntryStatus = Common.EU.EntryStatusList.Codes.Clear;
						}
					}
				}
			}
		}

		CoLoadForwardingShipmentCollection GetRelatedShipments(ForwardingShipment masterShipment)
		{
			return masterShipment.CoLoadShipments;
		}

		bool IsShipmentDeclarationAndBuyersConsolOrAssemblyMaster(JobDeclaration declaration)
		{
			return declaration.Shipment != null && (declaration.Shipment.IsBuyersConsolLead || declaration.Shipment.IsAssemblyMaster);
		}

		void SetHolds(ZString[] elements)
		{
			if (ConvertNYToBool(elements[9][0]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.SubjectToPortHealthDetain);
			}
			var holds = elements[10];
			if (ConvertNYToBool(holds[0]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.LocalCustomsHoldApplied);
			}

			if (ConvertNYToBool(holds[1]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.TradingStandardsHoldApplied);
			}

			if (ConvertNYToBool(holds[2]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.DefraHoldApplied);
			}

			if (ConvertNYToBool(holds[3]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.ScannerHoldApplied);
			}

			if (ConvertNYToBool(holds[4]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.PoliceSpecialBranchHoldApplied);
			}

			if (ConvertNYToBool(holds[5]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.RuralHoldApplied);
			}

			if (ConvertNYToBool(holds[6]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.EnvironmentAgencyHoldApplied);
			}

			if (ConvertNYToBool(holds[7]))
			{
				HoldsToAdd.Add(PortHoldAuthorities.Descriptions.ForestryCommissionHoldApplied);
			}
		}

		bool ConvertNYToBool(char element)
		{
			return char.Equals(element, 'Y');
		}

		public override bool AreThereAnyHolds()
		{
			return HoldsToAdd.Any();
		}

		public override ZString HoldsString
		{
			get
			{
				return ZString.Join(", ", HoldsToAdd.ToArray());
			}
		}

		string IPortAuthorityHoldApplicationProvider.HoldType
		{
			get
			{
				PortHoldAuthorities auths = new PortHoldAuthorities();
				return auths.GetCodeFromDescription(HoldAuthority);
			}
		}

		string HoldAuthority
		{
			get
			{
				List<string> knownAuthorities = new List<string>();
				foreach (var hold in HoldsToAdd)
				{
					if (!knownAuthorities.Contains(hold))
					{
						knownAuthorities.Add(hold);
					}
				}

				if (knownAuthorities.Count == 0)
				{
					return string.Empty; // no hold
				}
				else if (knownAuthorities.Count == 1)
				{
					return knownAuthorities[0];  // a specfic authority
				}
				else
				{
					return PortHoldAuthorities.Descriptions.MultipleAuthorities;
				}
			}
		}

		AddOrRemove IPortAuthorityHoldApplicationProvider.DirectionOfApplication
		{
			get { return AreThereAnyHolds() ? AddOrRemove.HoldAdd : AddOrRemove.Cleared; }
		}

		ZDateTime Date
		{
			get { return DateOfStatusEvent; }
		}
	}
}
