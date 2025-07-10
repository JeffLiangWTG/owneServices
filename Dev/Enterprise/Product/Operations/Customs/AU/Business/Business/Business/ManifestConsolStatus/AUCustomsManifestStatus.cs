using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AUCustomsManifestStatus : CustomsManifestStatus
	{
		public abstract new class Schema : CustomsManifestStatus.Schema
		{
			public const string ContingencyCAN = "ContingencyCAN";
		}

		public AUCustomsManifestStatus(ForwardingConsol consol)
			: base(consol)
		{
		}

		public ZString PremisesID
		{
			get
			{
				if (!NoEntryNumber)
				{
					var edimessage = ManifestProvider.Messages.Cast<EDIMessage>()
						.Where(x => x.EM_ApplicationCode == EDIInterchange.ApplicationCodes.CMR && x.EM_MessageType == "ESM" && x.EM_MessageSubType == CMRMessage.MessageSubTypes.Original && x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
						.OrderByDescending(x => x.EM_MessageDateTime)
						.FirstOrDefault();

					if (edimessage != null)
					{
						var cmrmessage = edimessage as CMRMessage;
						var autoEdifactMessage = cmrmessage != null ? cmrmessage.AutoEdifactMessage : null;
						var edifactMessage = (Edifact.D99B.Messages.CUSCAR.CUSCARMessage)autoEdifactMessage;
						var result = string.Empty;
						if (edifactMessage != null && edifactMessage.LOC.Count > 0)
						{
							var codeQualifier = edifactMessage.LOC[0].LocationFunctionCodeQualifier.ToString();
							var locationIdentification = edifactMessage.LOC[0].LocationIdentification;
							if (codeQualifier == Enterprise.Edifact.D99B.Elements.LocationFunctionCodeQualifierList.PlaceOfConsolidation && locationIdentification != null)
							{
								result = locationIdentification.LocationNameCode;
							}
						}
						return result;
					}
				}

				return GetPremisesID();
			}
		}

		string GetPremisesID()
		{
			var result = string.Empty;
			if (ManifestProvider.PackDepotAddress != null)
			{
				var headerPk = ManifestProvider.PackDepotAddress.OA_OH;
				if (GlbCompany.CurrentCompany.GC_OH_OrgProxy == headerPk || GlbCompany.CurrentCompany.Branches.Any(branch => branch.GB_OH_OrgProxy == headerPk))
				{
					result = ManifestProvider.PackDepotAddress.LocalControlledPremisesID;
				}
			}
			return result;
		}

		public new ForwardingConsol ManifestProvider
		{
			get { return (ForwardingConsol)base.ManifestProvider; }
		}

		#region Contingency CAN

		[MaxLength(14)]
		public ZString ContingencyCAN
		{
			get { return new FreightConsolWrapper(ManifestProvider).ContingencyCAN; }
			set
			{
				if (ContingencyCAN != value)
				{
					CheckMaximumLength(ContingencyCANInfo, value);
					new FreightConsolWrapper(ManifestProvider).ContingencyCAN = value;
					ContingencyCANInfo.RefreshBinding();
				}
			}
		}

		protected bool ContingencyCAN_ReadOnly
		{
			get { return IsContingencyCANInfoReadonly(); }
		}

		public ZPropertyInfo ContingencyCANInfo
		{
			get { return GetZPropertyInfo(Schema.ContingencyCAN); }
		}

		protected virtual bool IsContingencyCANInfoReadonly()
		{
			return false;
		}

		#endregion

		protected override bool NoEntryNumber
		{
			get { return base.NoEntryNumber || E2_CustomsEntryNumber == EmptyCustomsEntryNumberString; }
		}

		protected override bool ContinueWithAction(Customs.Business.ISendsMessagesToCustoms sender, Action action)
		{
			var result = true;
			if (Env.Registry.CMRTestMode)
			{
				result = sender.ContinueWithAction(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText, "Continue with send?");
			}

			var serviceWarning = CMRMessageManager.CheckMessageSendingServiceTaskIsRunning();
			if (!serviceWarning.IsEmpty)
			{
				result = sender.ContinueWithAction(serviceWarning, "Service Task Not Running");
			}
			return result;
		}

		protected override string ValidateEnvironmentForSendingManifests(Customs.Business.ISendsMessagesToCustoms sender)
		{
			var entryNum = new FreightConsolWrapper(ManifestProvider).GetPermit();
			if (entryNum != null && entryNum.CE_EntryNum.Length == 14)
			{
				return "Unable to send Exit2 message as Exit2 no longer exists.";
			}
			else
			{
				return base.ValidateEnvironmentForSendingManifests(sender);
			}
		}

		public const string EmptyCustomsEntryNumberString = "#-#-#-#-#";
	}
}
