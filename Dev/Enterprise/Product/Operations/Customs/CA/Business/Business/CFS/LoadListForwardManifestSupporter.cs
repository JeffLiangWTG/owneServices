using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LoadListForwardManifestSupport
	{
		public LoadListForwardManifestSupport(CFSLoadListConsol loadListConsol)
		{
			this.loadListConsol = loadListConsol;
			Argument.NotNull(loadListConsol, "loadListConsol");
			Argument.NotNull(loadListConsol.CanadaCCNNumber, "loadListConsol.CanadaCCNNumber");
		}

		readonly CFSLoadListConsol loadListConsol;

		BusinessObjectFactory Factory
		{
			get
			{
				return loadListConsol.Factory;
			}
		}

		#region MatchingForwardedManifests

		ACIHouseBillMessage[] forwardedManifests;
		public ACIHouseBillMessage[] MatchingForwardedManifests
		{
			get
			{
				return forwardedManifests ?? (forwardedManifests = getMatchingForwardedManifests());
			}
		}

		ACIHouseBillMessage[] getMatchingForwardedManifests()
		{
			var query = new ZQuery();

			query.AddToFilter(CreateACIHouseBillMessageQuery());
			query.AddToFilter(GetMatchingForwardedManifestQuery(RemoveSpaces(loadListConsol.CanadaCCNNumber)));

			return Factory.Load<ACIHouseBillMessage>(query) ?? Array.Empty<ACIHouseBillMessage>();
		}

		static string RemoveSpaces(string value)
		{
			return value.Replace(" ", "");
		}

		static ZQuery CreateACIHouseBillMessageQuery()
		{
			var query = new ZDBOnlyQuery(typeof(ACIHouseBillMessage));

			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAACI);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.ACIHouseBill);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageTypeList.Codes.ManifestForwardHouse);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);

			return query;
		}

		static ZQuery GetMatchingForwardedManifestQuery(ZString primaryCCN)
		{
			var query = new ZDBOnlyQuery(typeof(ACIHouseBillMessage));
			query.AddSubQuery(EDIMessageQueryHelper.GetGenAddOnTextQuery(SQLComparisonOperator.Equal, "WH", EDIMessage.Schema.SNPType), JoinCondition.And);
			query.AddSubQuery(EDIMessageQueryHelper.GetGenAddOnTextQuery(SQLComparisonOperator.Equal, primaryCCN, ACIHouseBillMessage.Schema.PrimaryCCN), JoinCondition.And);
			return query;
		}
		#endregion

		#region ClassifyForwardedManifests
		List<ACIHouseBillMessage> unlinkedAndNoShipmentMessages;
		List<ACIHouseBillMessage> unlinkedAndExistingShipmentMessages;
		List<ACIHouseBillMessage> linkedAndAttachedShipmentMessages;
		List<ACIHouseBillMessage> linkedInconsistentMesasges;

		void classifyForwardedManifests()
		{
			unlinkedAndNoShipmentMessages = new List<ACIHouseBillMessage>();
			unlinkedAndExistingShipmentMessages = new List<ACIHouseBillMessage>();
			linkedAndAttachedShipmentMessages = new List<ACIHouseBillMessage>();
			linkedInconsistentMesasges = new List<ACIHouseBillMessage>();

			foreach (ACIHouseBillMessage message in MatchingForwardedManifests)
			{
				if (message.EM_LinkUniqueID.IsEmpty)
				{
					if (loadListConsol.Shipments.Any(shipment => RemoveSpaces(((CFSShipment)shipment).CanadaHouseCCN) == message.CargoControlNumber))
					{
						unlinkedAndExistingShipmentMessages.Add(message);
					}
					else
					{
						unlinkedAndNoShipmentMessages.Add(message);
					}
				}
				else
				{
					var shipment = (CFSShipment)loadListConsol.Shipments.FindByPK(message.EM_LinkUniqueID);

					if (shipment != null && RemoveSpaces(shipment.CanadaHouseCCN) == message.CargoControlNumber)
					{
						linkedAndAttachedShipmentMessages.Add(message);
					}
					else
					{
						linkedInconsistentMesasges.Add(message);
					}
				}
			}
		}

		#endregion

		#region Classified Messsages

		public ACIHouseBillMessage[] UnlinkedAndNoShipmentMessages
		{
			get
			{
				if (unlinkedAndNoShipmentMessages == null)
				{
					classifyForwardedManifests();
				}

				return unlinkedAndNoShipmentMessages.ToArray();
			}
		}

		public ACIHouseBillMessage[] UnlinkedAndExistingShipmentMessages
		{
			get
			{
				if (unlinkedAndExistingShipmentMessages == null)
				{
					classifyForwardedManifests();
				}

				return unlinkedAndExistingShipmentMessages.ToArray();
			}
		}

		public ACIHouseBillMessage[] LinkedAndAttachedShipmentMessages
		{
			get
			{
				if (linkedAndAttachedShipmentMessages == null)
				{
					classifyForwardedManifests();
				}

				return linkedAndAttachedShipmentMessages.ToArray();
			}
		}

		public ACIHouseBillMessage[] LinkedInconsistentMesasges
		{
			get
			{
				if (linkedInconsistentMesasges == null)
				{
					classifyForwardedManifests();
				}

				return linkedInconsistentMesasges.ToArray();
			}
		}
		#endregion

		#region AttachOrCreateShipmentsFromForwardedManifests

		public static string AttachOrCreateShipmentsFromForwardedManifests(LoadListForwardManifestSupport support)
		{
			ZStringBuilder resultBuilder = new ZStringBuilder();

			List<CFSShipment> createdShipments = new List<CFSShipment>();
			List<CFSShipment> attachedShipments = new List<CFSShipment>();
			bool succeed = false;

			try
			{
				foreach (ACIHouseBillMessage message in support.UnlinkedAndNoShipmentMessages)
				{
					var shipment = support.createAndAttachShipmentFromForwardedManifest(message);
					if (shipment != null)
					{
						createdShipments.Add(shipment);
					}
				}

				foreach (ACIHouseBillMessage message in support.UnlinkedAndExistingShipmentMessages)
				{
					var shipment = support.attachShipmentToForwardedManifest(message);
					if (shipment != null)
					{
						attachedShipments.Add(shipment);
					}
				}

				support.Factory.Save();
				succeed = true;
			}
			catch (ZSaveException ex)
			{
				rollbackCreateAndAttachShipments(createdShipments, support);

				ZExceptionReporting.HandleSaveException(ex);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				rollbackCreateAndAttachShipments(createdShipments, support);

				throw;
			}

			if (succeed)
			{
				foreach (CFSShipment shipment in createdShipments)
				{
					support.reportcreatedAndAttachShipment(shipment, resultBuilder);
				}

				foreach (CFSShipment shipment in attachedShipments)
				{
					support.reportAttachedShipment(shipment, resultBuilder);
				}
			}

			foreach (ACIHouseBillMessage message in support.LinkedInconsistentMesasges)
			{
				support.reportInconsistency(message, resultBuilder);
			}

			return resultBuilder.ToStringWithNewLineBetweenAppends();
		}

		static void rollbackCreateAndAttachShipments(List<CFSShipment> createdShipments, LoadListForwardManifestSupport support)
		{
			foreach (CFSShipment shipment in createdShipments)
			{
				support.loadListConsol.Shipments.RemoveAndDelete(shipment);
			}

			foreach (ACIHouseBillMessage message in support.UnlinkedAndNoShipmentMessages)
			{
				message.EM_LinkedObject = null;
			}

			foreach (ACIHouseBillMessage message in support.UnlinkedAndExistingShipmentMessages)
			{
				message.EM_LinkedObject = null;
			}
		}

#if DEBUG
		protected virtual
#endif
		CFSShipment createAndAttachShipmentFromForwardedManifest(ACIHouseBillMessage message)
		{
			var shipment = findAttachedShipmentByCCN(message.CargoControlNumber);

			if (shipment == null)
			{
				shipment = new ForwardedManifestSupporter(message).AddNewShipmentToLoadList(loadListConsol);
				message.EM_LinkedObject = shipment;

				return shipment;
			}
			else
			{
				return null;
			}
		}

		void reportcreatedAndAttachShipment(CFSShipment shipment, ZStringBuilder resultBuilder)
		{
			resultBuilder.Append(Res.GetString("711377e0-7104-4f02-824b-440b0de78d06", "{0} was created and linked to Forwarded Manifest (CCN: {1}).",
				shipment.HumanReadableName, RemoveSpaces(shipment.CanadaHouseCCN)));
		}

		CFSShipment attachShipmentToForwardedManifest(ACIHouseBillMessage message)
		{
			var shipment = findAttachedShipmentByCCN(message.CargoControlNumber);

			if (shipment != null)
			{
				message.EM_LinkedObject = shipment;
			}

			return shipment;
		}

		CFSShipment findAttachedShipmentByCCN(string ccn)
		{
			return (CFSShipment)loadListConsol.Shipments.FirstOrDefault(shipment => RemoveSpaces(((CFSShipment)shipment).CanadaHouseCCN) == ccn);
		}

		void reportAttachedShipment(CFSShipment shipment, ZStringBuilder resultBuilder)
		{
			resultBuilder.Append(Res.GetString("baaed938-d5c4-42cc-9ba2-86f941f0baad", "{0} was linked to Forwarded Manifest (CCN: {1}).",
				shipment.HumanReadableName, RemoveSpaces(shipment.CanadaHouseCCN)));
		}

		void reportInconsistency(ACIHouseBillMessage message, ZStringBuilder resultBuilder)
		{
			if (message.EM_LinkUniqueID.IsValid)
			{
				BusinessObject linkedObject = loadListConsol.Shipments.FindByPK(message.EM_LinkUniqueID);

				if (linkedObject != null)
				{
					if (RemoveSpaces(((CFSShipment)linkedObject).CanadaHouseCCN) != message.CargoControlNumber)
					{
						resultBuilder.Append(Res.GetString("775536b0-c105-4ac3-8f10-2132a003b4e2", "{0} had already been linked to Forwarded Manifest (CCN: {1}), \r\n    but the shipment house CCN is {2}",
							linkedObject.HumanReadableName, message.CargoControlNumber, RemoveSpaces(((CFSShipment)linkedObject).CanadaHouseCCN)));
					}
				}
				else
				{
					linkedObject = Factory.Load<CFSShipment>(message.EM_LinkUniqueID);

					if (linkedObject == null)
					{
						linkedObject = message.EM_LinkedObject;
					}

					if (linkedObject != null)
					{
						resultBuilder.Append(Res.GetString("aa1ab05e-6be0-4bc6-8337-fb5e0a7568c2", "{0} had already been linked to Forwarded Manifest (CCN: {1}), \r\n    but it is not a shipment that is attached to this Load List.",
							linkedObject.HumanReadableName, message.CargoControlNumber));
					}
				}
			}
		}
		#endregion
	}
}
