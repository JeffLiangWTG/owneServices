using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public abstract class JXCRecord
	{
		public JXCRecord(ZString lineType, ZString lineContent)
		{
			this.LineType = lineType;
			this.LineContent = lineContent;
		}

		protected JASCsvLine Fields
		{
			get
			{
				if (fFields == null)
				{
					fFields = new JASCsvLine(LineContent, JXCConstants.Delimiter);
				}
				return fFields;
			}
		}

		protected ZString ConvertJASWeightUnit(ZString jASWeightUnit)
		{
			return (jASWeightUnit.ToUpper() == "L") ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;
		}

		protected JASOrgHeader FindOrCreateTempOrganisation(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber)
		{
			return FindOrCreateTempOrganisation(organisation, organisationType, sourceObject, notificationSubscriber, Enum.GetName(typeof(OrganisationTypes), organisationType));
		}

		protected JASOrgHeader FindOrCreateTempOrganisation(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber, ZString noteOrgCaption)
		{
			JASOrgHeader result = null;

			if (organisation.OrganisationDetails.Name.ContainsAnyLetters)
			{
				FindOrCreateTempOrganisationParamsListForTest.Add(new FindOrCreateTempOrganisationParams(organisation, organisationType, sourceObject, notificationSubscriber));
				BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider(sourceObject.Factory);
				OrganisationMatching matching = GetOrganisationMatching(factoryProvider, notificationSubscriber);
				var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationType = organisationType, OrganisationSubType = noteOrgCaption };
				result = (JASOrgHeader)matching.FindOrCreateTempOrganisation(organisation, sourceObject, organisationType, unmatchOrgRecordCriteria);
			}

			return result;
		}

		protected ZGuid FindOrCreateTempOrganisationPK(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber)
		{
			return FindOrCreateTempOrganisationPK(organisation, organisationType, sourceObject, notificationSubscriber, Enum.GetName(typeof(OrganisationTypes), organisationType));
		}

		protected ZGuid FindOrCreateTempOrganisationPK(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber, ZString noteOrgCaption)
		{
			OrgHeader orgHeader = FindOrCreateTempOrganisation(organisation, organisationType, sourceObject, notificationSubscriber, noteOrgCaption);
			return (orgHeader != null) ? orgHeader.PK : ZGuid.Empty;
		}

		protected JASOrgHeader FindOrganisation(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber)
		{
			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider(sourceObject.Factory);
			OrganisationMatching matching = GetOrganisationMatching(factoryProvider, notificationSubscriber);
			return (JASOrgHeader)matching.FindOrganisation(organisation, sourceObject, organisationType);
		}

		protected ZString GetUNLOCOFromOfficeCode(BusinessObjectFactory factory, ZString officeCode)
		{
			JASOrgHeader officeOrg = JASOrgHeader.FindOrgHeaderByOfficeCode(factory, officeCode);
			return (officeOrg != null) ? officeOrg.OH_RL_NKClosestPort : ZString.Empty;
		}

		protected bool IsDefaultUnmatchedOrg(JASOrgHeader orgHeader)
		{
			ZGuid defaultUnmatchedOrgPK = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			return (orgHeader.PK == defaultUnmatchedOrgPK);
		}

		protected void NotifyAttachingNewShipmentToConsol(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			if (notificationSubscriber != null)
			{
				InfoNotification notification = new InfoNotification("Attaching new shipment to " + consol.HumanReadableName);
				notificationSubscriber.Notify(notification);
			}
		}

		protected void NotifyAttachingExistingShipmentToConsol(JASForwardingConsol consol, JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			if (notificationSubscriber != null)
			{
				InfoNotification notification = new InfoNotification("Attaching " + shipment.HumanReadableName + " to " + consol.HumanReadableName);
				notificationSubscriber.Notify(notification);
			}
		}

		protected virtual OrganisationMatching GetOrganisationMatching(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			return new OrganisationMatching(factoryProvider, Xsd.XmlInterchange.Empty, notificationSubscriber);
		}

		public readonly ZString LineType;
		public readonly ZString LineContent;
		JASCsvLine fFields;

		#region For Test

		public class FindOrCreateTempOrganisationParams
		{
			public FindOrCreateTempOrganisationParams(Xsd.Organisation organisation, OrganisationTypes organisationType, BusinessObject sourceObject, INotifications notificationSubscriber)
			{
				this.Organisation = organisation;
				this.OrganisationType = organisationType;
				this.SourceObject = sourceObject;
				this.NotificationSubscriber = notificationSubscriber;
			}

			public Xsd.Organisation Organisation;
			public OrganisationTypes OrganisationType;
			public BusinessObject SourceObject;
			public INotifications NotificationSubscriber;
		}

		public class FindOrCreateTempOrganisationParamsList : ArrayList
		{
			public FindOrCreateTempOrganisationParams GetByOrganisationName(ZString orgName)
			{
				foreach (FindOrCreateTempOrganisationParams @params in this)
				{
					if (@params.Organisation.OrganisationDetails.Name == orgName)
					{
						return @params;
					}
				}

				return null;
			}
		}

		public FindOrCreateTempOrganisationParamsList FindOrCreateTempOrganisationParamsListForTest = new FindOrCreateTempOrganisationParamsList();

		#endregion
	}
}
