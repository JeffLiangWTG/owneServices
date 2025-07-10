using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.XmlDefinition;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.SystemMerge.Xml
{
	public class SysMergeOrganisationXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public SysMergeOrganisationXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override IValueObjectImportContext GetNewImportContext(BusinessObjectFactoryProvider factoryProvider, DataTransfer.Xml.XsdVersion1.XmlInterchange interchange, INotifications notifications)
		{
			return new ValueObjectImportContext(factoryProvider, interchange, new SysMergeOrganisationMatching(), notifications);
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			context.FactoryProvider.CreateNewWithoutSave();
			BusinessObject topLevelBizObj;
			try
			{
				topLevelBizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
				SaveIfNotInTheDatabaseAndNotify(topLevelBizObj, context);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				topLevelBizObj = null;
				if (valueObject != null)
				{
					var bizObj = ((SysMergeOrganisation)valueObject).OrganisationDetail.OrgHeader;
					context.Notify(new InfoNotification("\r\n"));
					context.Notify(new ErrorNotification(ErrorType.Error,
						Res.GetString("19268F48-1E3D-4B48-A6E2-D876C732B371",
							"Error when importing organization [({0}) - {1} - {2}]. Details: {3}", bizObj.PK, bizObj.Code, bizObj.FullName,
							ex.Message
							)));
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
				}
				context.FactoryProvider.RemoveCurrent();
			}

			return topLevelBizObj;
		}

		/// <summary>
		/// Only Save Factory and Notify if the Top Level Business Object is a new imported one
		/// (as opposed to an existing one, which has been loaded and skipped from import)
		/// </summary>
		void SaveIfNotInTheDatabaseAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			if (!topLevelBizObj.IsInDatabase)
			{
				try
				{
					SaveAndNotify(topLevelBizObj, context);
					context.Notify(SysMergeValueObjectHelper.GetImportedSuccessfullyNotification(GetOrganisationDisplayInfo(topLevelBizObj)));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					string message = String.Format("{0}\r\n\r\n{1}", GetOrganisationDisplayInfo(topLevelBizObj), ex.Message);
					throw new Exception(message, ex);
				}
			}
		}

		/// <summary>
		/// Saves and add a create business object notification in order to increment the count of imported top level objects.
		/// </summary>
		void SaveAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			BusinessObjectCreatedOrUpdatedNotification createdBizObjNotification = new BusinessObjectCreatedOrUpdatedNotification(topLevelBizObj);
			context.FactoryProvider.SaveCurrentAndUpdateRecordCounts();
			context.Notify(createdBizObjNotification);
		}

		string GetOrganisationDisplayInfo(BusinessObject topLevelBizObj)
		{
			OrgHeaderForDataTransfer topLevelOrg = (OrgHeaderForDataTransfer)topLevelBizObj;
			string orgDisplayInfo = Res.GetString("9a6208a9-4071-43dd-9667-388071ffb4b3", "Organization: [({0}) - {1} - {2}]",
				topLevelOrg.PK.ToString(), topLevelOrg.OH_Code, topLevelOrg.OH_FullName);
			return orgDisplayInfo;
		}
	}
}
