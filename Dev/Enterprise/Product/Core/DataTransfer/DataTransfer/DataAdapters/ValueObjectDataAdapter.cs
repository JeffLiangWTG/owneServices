using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Events = Enterprise.ZArchitecture.Business.Events;

namespace Enterprise.DataTransfer.DataAdapters
{
	public abstract class ValueObjectDataAdapter<TBusinessObject, TValueObject> : IValueObjectDataAdapter, IProgressSupporter
		where TBusinessObject : BusinessObject
		where TValueObject : IValueObject
	{
		protected virtual Type ValueObjectCollectionType
		{
			get { return null; }
		}

		#region Properties

		public abstract string RootCollectionElementName { get; }
		public abstract string RootElementName { get; }
		public abstract XmlSchema Schema { get; }
		public abstract XmlSchema CollectionSchema { get; }

		Func<bool> MeetAdditionalRequirementsForExportEvent;

		public ZString FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				fileName = value;
			}
		}

		ZString fileName = ZString.Empty;

		#endregion

		protected abstract void ImportFromValueObjectCore(TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context);
		protected abstract void ExportToValueObjectCore(TBusinessObject bizObj, TValueObject constructedValueObject, IValueObjectExportContext context);

		protected virtual TBusinessObject FindBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			return null;
		}

		#region Types

		public Type ValueObjectType
		{
			get { return typeof(TValueObject); }
		}

		public Type BusinessObjectType
		{
			get { return typeof(TBusinessObject); }
		}

		#endregion

		#region EDI Interchange

		protected void CreateEDIInterchange(IValueObjectImportContext context)
		{
			if (context.Interchange != null && ((XmlInterchange)context.Interchange).InterchangeInfo != null && context.Interchange.IsSpecified)
			{
				context.EDIInterchange = GetNewEDIInterchange(new BusinessObjectFactory());
			}
			if (context.EDIInterchange != null)
			{
				PopulateEDIInterchange(context.EDIInterchange, ((XmlInterchange)context.Interchange));
			}
		}

		protected virtual void PopulateEDIInterchange(EDIInterchange eDIInterchange, XmlInterchange xmlInterchange)
		{
			if (xmlInterchange != null)
			{
				var xml = ZString.Empty;
				if (xmlInterchange.Payload != null && xmlInterchange.Payload.IsSpecified && xmlInterchange.Payload.Data != null)
				{
					if (xmlInterchange.Payload.DataAdapter == null)
					{
						xmlInterchange.Payload.DataAdapter = this;
					}
					xml = xmlInterchange.Payload.GetOuterXml();
				}
				eDIInterchange.EI_BodyText = xml;

				eDIInterchange.EI_InterchangeNum = GetInterchangeNumber(xmlInterchange);
				eDIInterchange.EI_From = xmlInterchange.InterchangeInfo.Source.OriginServer;
			}
			eDIInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			eDIInterchange.EI_Status = EDIInterchange.Status.Received;
			eDIInterchange.EI_RetryCount = 0;
		}

		protected virtual ZString GetInterchangeNumber(XmlInterchange interchange)
		{
			var result = ZString.Empty;
			foreach (InterchangeInfoReferenceKey refKey in interchange.InterchangeInfo.ReferenceKeys)
			{
				if (refKey.ReferenceKeyName == ReferenceType.UniqueIdentifier)
				{
					result = refKey.Value;
					break;
				}
			}
			return result;
		}

		protected virtual EDIInterchange GetNewEDIInterchange(BusinessObjectFactory factory)
		{
			return null;
		}

		#endregion

		#region EDIMessages

		protected void AddEDIMessage(TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context)
		{
			if (context.EDIInterchange == null)
			{
				CreateEDIInterchange(context);
			}
			if (context.EDIInterchange != null)
			{
				var message = context.EDIInterchange.ContainedMessages.AddNew();
				PopulateEDIMessage(message, bizObj, value, context);
			}
		}

		protected virtual void PopulateEDIMessage(EDIMessage message, TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context)
		{
			if (context.CurrentObjectXMLUTF8 != null)
			{
				context.CurrentObjectXMLUTF8.Position = 0;
				message.EM_MessageText = new StreamReader(context.CurrentObjectXMLUTF8).ReadToEnd();
			}
			message.EM_LinkedObject = bizObj;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
		}

		#endregion

		#region ImportFromValueObject / ExportToValueObject

		public virtual bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible
		{
			get { return false; }
		}

		public virtual bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked
		{
			get { return false; }
		}

		public virtual void ImportFromValueObject(TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context)
		{
			if (bizObj == null)
			{
				throw new ArgumentNullException(nameof(bizObj));
			}

			if (bizObj.Factory != context.Factory && !AllowDifferentImportContextFactory)
			{
				ErrorReporter.ReportOnce("ImportFromVOFactoryForBizObjAndContextDifferent", "Factory for the BusinessObject has to be the same as the Factory in the Context");
			}

			if (value != null && value.IsSpecified)
			{
				var supportImporting = bizObj as ISupportDataImporting;
				var wasImportingData = false;
				if (supportImporting != null)
				{
					wasImportingData = supportImporting.IsImportingData;
					supportImporting.IsImportingData = true;
				}

				try
				{
					ImportFromValueObjectCore(bizObj, value, context);
					if (UserDefinedValuesAttribute.IsEnabled(bizObj))
					{
						ImportCustomValues(bizObj, value, context);
					}
					if (!context.NotificationsContainsNotifictionType(ErrorType.DataErrorPreventSave))
					{
						NotifyBizObjCreatedOrUpdated(context, bizObj);
					}
				}
				finally
				{
					if (supportImporting != null && !wasImportingData)
					{
						supportImporting.IsImportingData = false;
					}

					AfterImportFromValueObject(bizObj, value, context);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in XML interchange")]
		protected void ImportCustomValues(BusinessObject businessObject, IValueObject valueObject, IValueObjectImportContext context)
		{
			foreach (CustomValue customValue in GetCustomValues(valueObject))
			{
				var value = customValue.Value;
				IZType zValue;
				switch (customValue.Type.ToString())
				{
					case "String":
						zValue = value;
						break;
					case "Decimal":
						zValue = new ZDecimal(value);
						break;
					case "Integer":
						zValue = new ZInt(value);
						break;
					case "Short":
						zValue = new ZShort(value);
						break;
					case "Byte":
						zValue = new ZByte(value);
						break;
					case "Boolean":
						zValue = new ZBool(value);
						break;
					case "DateTime":
						zValue = !value.IsEmpty ? new ZDateTime(XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind)) : ZDateTime.Empty;
						break;
					default:
						return;
				}
				businessObject.SetUserDefinedValue(customValue.Name, zValue, context);
			}
		}

		static CustomValueCollection GetCustomValues(IValueObject valueObject)
		{
			var customValuesProperty = valueObject.GetType().GetProperty("CustomValues");
			if (customValuesProperty != null)
			{
				return (CustomValueCollection)customValuesProperty.GetValue(valueObject, null);
			}
			else
			{
				return new CustomValueCollection();
			}
		}

		protected virtual void AfterImportFromValueObject(TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context)
		{
		}

		protected virtual bool AllowDifferentImportContextFactory
		{
			get { return false; }
		}

		public TValueObject ExportToValueObject(TBusinessObject bizObj, IValueObjectExportContext context)
		{
			var result = default(TValueObject);
			if (bizObj != null)
			{
				result = (TValueObject)Activator.CreateInstance(ValueObjectType);
				ExportToValueObjectNotCreatingEmptyElements(bizObj, result, context);
			}
			return result;
		}

		public void ExportToValueObject(TBusinessObject bizObj, TValueObject constructedValueObject, IValueObjectExportContext context)
		{
			if (constructedValueObject != null && bizObj != null)
			{
				ExportToValueObjectNotCreatingEmptyElements(bizObj, constructedValueObject, context);
			}
		}

		void ExportToValueObjectNotCreatingEmptyElements(TBusinessObject bizObj, TValueObject constructedValueObject, IValueObjectExportContext context)
		{
			var originalFlagValue = constructedValueObject.ShouldCreateElementForEmptyValue;

			try
			{
				constructedValueObject.ShouldCreateElementForEmptyValue = false;
				ExportToValueObjectCore(bizObj, constructedValueObject, context);
				if (UserDefinedValuesAttribute.IsEnabled(bizObj))
				{
					ExportCustomValues(bizObj, constructedValueObject, context);
				}
			}
			finally
			{
				constructedValueObject.ShouldCreateElementForEmptyValue = originalFlagValue;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in XML interchange")]
		protected void ExportCustomValues(BusinessObject bizObj, IValueObject constructedValueObject, IValueObjectExportContext context)
		{
			var customValues = GetCustomValues(constructedValueObject);
			foreach (var dynamicPropertyValue in bizObj.GetUserDefinedValues())
			{
				if (ShouldExportCustomValue(dynamicPropertyValue, bizObj))
				{
					var customValue = customValues.AddNew();
					customValue.Name = dynamicPropertyValue.PropertyName;

					if (dynamicPropertyValue.Value is ZString)
					{
						customValue.Type = "String";
					}
					else if (dynamicPropertyValue.Value is ZDecimal)
					{
						customValue.Type = "Decimal";
					}
					else if (dynamicPropertyValue.Value is ZInt)
					{
						customValue.Type = "Integer";
					}
					else if (dynamicPropertyValue.Value is ZShort)
					{
						customValue.Type = "Short";
					}
					else if (dynamicPropertyValue.Value is ZByte)
					{
						customValue.Type = "Byte";
					}
					else if (dynamicPropertyValue.Value is ZBool)
					{
						customValue.Type = "Boolean";
					}
					else if (dynamicPropertyValue.Value is ZDateTime)
					{
						customValue.Type = "DateTime";
					}

					if (dynamicPropertyValue.Value is ZDateTime)
					{
						customValue.Value = ((ZDateTime)dynamicPropertyValue.Value).ToISO8601String();
					}
					else
					{
						customValue.Value = dynamicPropertyValue.Value.ToString();
					}
				}
			}
		}

		protected virtual bool ShouldExportCustomValue(IPropertyValue dynamicPropertyValue, BusinessObject bizObj)
		{
			return true;
		}

		protected void AddImportEvent(TBusinessObject bizObj)
		{
			if (bizObj != null && (!bizObj.IsInDatabase || bizObj.HasChanges) && !(bizObj is NonPersistentBusinessObject))
			{
				AddImportEvent(bizObj, ZString.Empty);
			}
		}

		protected virtual void AddImportEvent(TBusinessObject bizObj, ZString reference)
		{
			if (bizObj != null && (!bizObj.IsInDatabase || bizObj.HasChanges) && !(bizObj is NonPersistentBusinessObject))
			{
				bizObj.GetLogs().AddNew(Events.DataImport, reference);
			}
		}

		protected void AddExportEvent(TValueObject valueObject, TBusinessObject bizObj, IValueObjectExportContext context)
		{
			AddExportEvent(valueObject, bizObj, context, ZString.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is a hard coded constant")]
		protected virtual void AddExportEvent(TValueObject valueObject, TBusinessObject bizObj, IValueObjectExportContext context, ZString reference)
		{
			if (valueObject != null && !(bizObj is NonPersistentBusinessObject)
				&& (MeetAdditionalRequirementsForExportEvent == null || MeetAdditionalRequirementsForExportEvent()))
			{
				var log = bizObj.GetLogs().AddNew(Events.DataExport, reference);
				if (context != null && log.SL_Reference.IsEmpty && !string.IsNullOrEmpty(context.ExportPurpose))
				{
					string description = bizObj.Factory.LoadFromNaturalKey<IEDIMessagePurpose>(EDIMessagePurposeSchema.EMP_Code, context.ExportPurpose)?.EMP_Description;
					ZString suggestedReference = "Purpose: " + (string.IsNullOrEmpty(description) ? context.ExportPurpose : description);

					using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
					{
						log.SL_Reference = suggestedReference.Left(StmALog.Schema.SL_ReferenceMaxLength);
					}
				}
			}
		}

		#endregion

		protected virtual TBusinessObject NewBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			return context.Factory.New<TBusinessObject>();
		}

		#region CreateOrUpdateFromValueObject

		public virtual TBusinessObject CreateOrUpdateFromValueObject(TValueObject value, IValueObjectImportContext context)
		{
			var result = CreateOrUpdateFromValueObjectCore(value, context);
			AddEDIMessage(result, value, context);
			return result;
		}

		protected virtual TBusinessObject CreateOrUpdateFromValueObjectCore(TValueObject value, IValueObjectImportContext context)
		{
			TBusinessObject result = null;
			if (value != null)
			{
				if (ShouldCreateOrUpdateBusinessObject(value, context))
				{
					var shouldImportFromValueObject = true;

					result = FindBusinessObject(value, context);
					if (result == null)
					{
						if (!MinimumRequirementsMetForNewImport(value, context))
						{
							return null;
						}
						result = NewBusinessObject(value, context);
					}
					else
					{
						shouldImportFromValueObject = ShouldUpdateExistingObject(result, context);
					}

					if (shouldImportFromValueObject)
					{
						context.ImportingJob = result;
						ImportFromValueObject(result, value, context);
					}
					else
					{
						OnUserDeclinedImport(result, value, context);
					}
				}
			}
			return result;
		}

		protected virtual bool ShouldCreateOrUpdateBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			return true;
		}

		#endregion

		#region FromXmlInterchange / ToXmlInterchange

		public TBusinessObject[] FromXmlInterchange(IBusinessObjectCollection collectionForRelationshipSetup, IValueObjectImportContext context)
		{
			XmlInterchange interchange = (XmlInterchange)context.Interchange;

			if (interchange.Payload.Data == null)
			{
				context.Notify(new Notification(ErrorType.Error, Res.GetString("48f3d703-3fba-4c52-a2af-b9747ca65b8e", "Payload.Data was not initialized")));
			}

			var result = new List<TBusinessObject>();

			if (interchange.Payload.Data is TBusinessObject)
			{
				result.Add((TBusinessObject)interchange.Payload.Data);
			}
			else if (interchange.Payload.Data is TValueObject)
			{
				result.Add(ToBusinessObject((TValueObject)interchange.Payload.Data, context));
			}
			else if (interchange.Payload.Data is IEnumerable)
			{
				foreach (object element in (IEnumerable)interchange.Payload.Data)
				{
					TBusinessObject bizo = element as TBusinessObject;

					if (bizo == null && element is IValueObject)
					{
						bizo = ToBusinessObject((TValueObject)element, context);
					}

					if (bizo != null)
					{
						if (collectionForRelationshipSetup != null)
						{
							collectionForRelationshipSetup.Add(bizo);
						}
						result.Add(bizo);
					}
				}
			}

			return result.ToArray();
		}

		TBusinessObject ToBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			return IsValueObjectCancellation(value, context)
				? CancelBusinessObject(value, context)
				: CreateOrUpdateFromValueObject(value, context);
		}

		protected virtual bool IsValueObjectCancellation(IValueObject value, IValueObjectImportContext context)
		{
			return false;
		}

		protected virtual TBusinessObject CancelBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public virtual XmlInterchange ToXmlInterchange(IList bizObjs, IValueObjectExportContext context)
		{
			if (bizObjs.Count == 0)
			{
				throw new ArgumentException("You must pass at least 1 business object.");
			}

			BusinessObjectFactory factory;
			if (bizObjs is BusinessObjectReader)
			{
				factory = ((BusinessObjectReader)bizObjs).Factory;
			}
			else
			{
				factory = ((BusinessObject)bizObjs[0]).Factory;
			}

			var result = XmlInterchange.NewPopulatedInterchange(factory, context);
			result.Payload = new Payload { Data = bizObjs, Context = context, DataAdapter = this };
			return result;
		}

		#endregion

		#region ToValueObjectCollection

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		protected IList ToValueObjectCollections(TBusinessObject[] bizObjs, IValueObjectExportContext context)
		{
			if ((bizObjs == null) || bizObjs.Length == 0)
			{
				return null;
			}
			else
			{
				var result = (IList)Activator.CreateInstance(ValueObjectCollectionType, true);
				foreach (var bizObj in bizObjs)
				{
					result.Add(ExportToValueObject(bizObj, context));
				}
				return result;
			}
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		protected IList ToValueObjectCollection(IBusinessObjectCollection bizObjs, IValueObjectExportContext context)
		{
			var bizobjarray = bizObjs.ToArray<TBusinessObject>();
			return ToValueObjectCollections(bizobjarray, context);
		}

		#endregion

		#region Progress

		public void OnProgress()
		{
			if (Progress != null)
			{
				Progress(this, EventArgs.Empty);
			}
		}

		public event EventHandler Progress;

		#endregion

		#region Implementation

		protected virtual bool ShouldUpdateExistingObject(TBusinessObject bizObj, INotifications notifications)
		{
			return ConfirmUpdateOfExistingBusinessObject(bizObj, notifications);
		}

		protected virtual bool ConfirmUpdateOfExistingBusinessObject(TBusinessObject obj, INotifications notifications)
		{
			var queryArgs = new QueryUserYesNoYesAllNoAllEventArgs();
			queryArgs.Message = UpdateBusinessObjectMessage(obj);
			notifications.QueryUser(queryArgs);

			return queryArgs.Response;
		}

		protected virtual string UpdateBusinessObjectMessage(TBusinessObject obj)
		{
			return Res.GetString("4246c36f-eea2-4d99-acc9-8e89a3d6bab3", "Found {0}. Is it OK to update it?", obj.HumanReadableName);
		}

		protected virtual void OnUserDeclinedImport(TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context)
		{
		}

		protected virtual bool MinimumRequirementsMetForNewImport(TValueObject shipmentValue, IValueObjectImportContext importContext)
		{
			return true;
		}

		protected virtual void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			notifications.Notify(new BusinessObjectCreatedOrUpdatedNotification(bizObj));
		}

		protected virtual string GetXmlNamespace()
		{
			return XmlSchemaDefinitions.EdiXmlNamespace;
		}

		#endregion

		#region IValueObjectDataAdapter Members

		string IValueObjectDataAdapter.FileName
		{
			get { return FileName; }
			set { FileName = value; }
		}

		Type IValueObjectDataAdapter.ValueObjectType
		{
			get { return ValueObjectType; }
		}

		Type IValueObjectDataAdapter.BusinessObjectType
		{
			get { return BusinessObjectType; }
		}

		XmlSchema IValueObjectDataAdapter.Schema
		{
			get { return Schema; }
		}

		XmlSchema IValueObjectDataAdapter.CollectionSchema
		{
			get { return CollectionSchema; }
		}

		BusinessObject[] IValueObjectDataAdapter.FromXmlInterchange(IBusinessObjectCollection collectionForRelationshipSetup, IValueObjectImportContext context)
		{
			return FromXmlInterchange(collectionForRelationshipSetup, context);
		}

		IValueObject IValueObjectDataAdapter.ToXmlInterchange(IList bizObjs, IValueObjectExportContext context)
		{
			return ToXmlInterchange(bizObjs, context);
		}

		BusinessObject IValueObjectDataAdapter.NewBusinessObject(IValueObject value, IValueObjectImportContext context)
		{
			return NewBusinessObject((TValueObject)value, context);
		}

		BusinessObject IValueObjectDataAdapter.FindBusinessObject(IValueObject value, IValueObjectImportContext context)
		{
			return FindBusinessObject((TValueObject)value, context);
		}

		BusinessObject IValueObjectDataAdapter.CreateOrUpdateFromValueObject(IValueObject value, IValueObjectImportContext context)
		{
			return CreateOrUpdateFromValueObject((TValueObject)value, context);
		}

		void IValueObjectDataAdapter.ImportFromValueObject(BusinessObject bizObj, IValueObject value, IValueObjectImportContext context)
		{
			ImportFromValueObject((TBusinessObject)bizObj, (TValueObject)value, context);
		}

		IValueObject IValueObjectDataAdapter.ExportToValueObject(BusinessObject bizObj, IValueObjectExportContext context)
		{
			return ExportToValueObject((TBusinessObject)bizObj, context);
		}

		void IValueObjectDataAdapter.ExportToValueObject(BusinessObject bizObj, IValueObject constructedValueObject, IValueObjectExportContext context)
		{
			ExportToValueObject((TBusinessObject)bizObj, (TValueObject)constructedValueObject, context);
		}

		void IValueObjectDataAdapter.SetAdditionalRequirementsForDataExportEvent(Func<bool> conditions)
		{
			MeetAdditionalRequirementsForExportEvent = conditions;
		}

		#endregion

	}
}
