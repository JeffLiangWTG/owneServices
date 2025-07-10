using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Res = Enterprise.DataTransfer.SystemMerge.Business.Res;

namespace Enterprise.DataTransfer.SystemMerge.Xml
{
	public class SysMergeClassificationObjectSerializer : XmlValueObjectSerializer
	{
		public SysMergeClassificationObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			BusinessObject topLevelBizObj;
			try
			{
				topLevelBizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
				SaveIfNotInTheDatabaseAndNotify(topLevelBizObj, context);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				topLevelBizObj = null;
				context.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
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
					context.Notify(SysMergeValueObjectHelper.GetImportedSuccessfullyNotification(GetClassificationDisplayInfo(topLevelBizObj)));
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
					else
					{
						string message = String.Format("{0}\r\n\r\n{1}", GetClassificationDisplayInfo(topLevelBizObj), ex.Message);
						throw new Exception(message, ex);
					}
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

		string GetClassificationDisplayInfo(BusinessObject topLevelBizObj)
		{
			var topLevelClassification = (BaseCusClassification)topLevelBizObj;
			string result = Res.GetString("9758A443-0F83-4e86-B021-9B3DEE1D2DEC", "Classification: [({0}) - {1} - {2}]",
				topLevelClassification.PK.ToString(), topLevelClassification.CC_LookupCode, topLevelClassification.CC_Description);
			return result;
		}
	}
}
