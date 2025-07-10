using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class CusHAWBAutoQueueMovement : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string FreeTextSegmentName = "FreeTextSegmentName";
			public const string FreeTextSegmentValue = "FreeTextSegmentValue";
			public const string QueueName = "QueueName";
			public const string ReasonCode = "ReasonCode";
			public const string StatusCode = "StatusCode";
			public const string Priority = "Priority";
		}

		#endregion

		public CusHAWBAutoQueueMovement()
		{
		}

		#region Bound Properties

		#region FreeTextSegmentName

		[MaxLength(100)]
		public ZString FreeTextSegmentName
		{
			get { return fFreeTextSegmentName; }
			set
			{
				CheckMaximumLength(FreeTextSegmentNameInfo, value);
				SetNonPersistentPropertyValue(FreeTextSegmentNameInfo, ref fFreeTextSegmentName, value);

				if (!IsValidationSuspended)
				{
					ValidateFreeTextSegmentName();
				}
			}
		}

		public ZPropertyInfo FreeTextSegmentNameInfo
		{
			get { return GetZPropertyInfo(Schema.FreeTextSegmentName); }
		}

		ZString fFreeTextSegmentName;

		#endregion

		#region FreeTextSegmentValue

		[MaxLength(200)]
		public ZString FreeTextSegmentValue
		{
			get { return fFreeTextSegmentValue; }
			set
			{
				CheckMaximumLength(FreeTextSegmentValueInfo, value);
				SetNonPersistentPropertyValue(FreeTextSegmentValueInfo, ref fFreeTextSegmentValue, value);

				if (!IsValidationSuspended)
				{
					ValidateFreeTextSegmentValue();
				}
			}
		}

		public ZPropertyInfo FreeTextSegmentValueInfo
		{
			get { return GetZPropertyInfo(Schema.FreeTextSegmentValue); }
		}

		ZString fFreeTextSegmentValue;

		#endregion

		#region Priority

		public ZInt Priority
		{
			get { return fPriority; }
			set
			{
				fPriority = value;

				if (!IsValidationSuspended)
				{
					ValidatePriority();
				}
				PriorityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PriorityInfo
		{
			get { return GetZPropertyInfo(Schema.Priority, "Queue Movement Priority"); }
		}

		ZInt fPriority;

		#endregion

		public NonPersistentCargoReportQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = new NonPersistentCargoReportQueue(new BusinessObjectFactory());
					RegisterEditableChildObject(fQueue);
				}
				return fQueue;
			}
		}

		[Obsolete("Should not be used directly, only made public for Validation", true)]
		public ZInt FreeTextSegmentNameAndValueHashCode
		{
			get { return (FreeTextSegmentName.Trim() + FreeTextSegmentValue.Trim()).ToUpper().GetHashCode(); }
		}

		ZPropertyInfo FreeTextSegmentNameAndValueHashCodeInfo
		{
			get
			{
#pragma warning disable CA1507 // Use nameof to express symbol names - suppressed due to Obsolete property reference
				return GetZPropertyInfo("FreeTextSegmentNameAndValueHashCode", "Free Text Segment Name And Value Pair");
#pragma warning restore CA1507 // Use nameof to express symbol names
			}
		}

		NonPersistentCargoReportQueue fQueue;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateFreeTextSegmentName();
			ValidateFreeTextSegmentValue();
			ValidatePriority();
		}

		void ValidateFreeTextSegmentName()
		{
			FreeTextSegmentNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FreeTextSegmentNameInfo);
			ValidateFreeTextSegmentNameValueHashCode(FreeTextSegmentNameInfo);
		}

		void ValidateFreeTextSegmentValue()
		{
			FreeTextSegmentValueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FreeTextSegmentValueInfo);
			ValidateFreeTextSegmentNameValueHashCode(FreeTextSegmentValueInfo);
		}

		void ValidateFreeTextSegmentNameValueHashCode(ZPropertyInfo propertyInfoToValidate)
		{
			FreeTextSegmentNameAndValueHashCodeInfo.ClearAllNotifications();
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(FreeTextSegmentNameAndValueHashCodeInfo);
				propertyInfoToValidate.AddAllNotificationsFrom(FreeTextSegmentNameAndValueHashCodeInfo);
			}
		}

		void ValidatePriority()
		{
			PriorityInfo.ClearAllNotifications();
			if (Priority < 1)
			{
				PriorityInfo.AddError("Priority value has to be greater than 0");
			}

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PriorityInfo);
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.FreeTextSegmentName, FreeTextSegmentName);
			writer.WriteElementString(Schema.FreeTextSegmentValue, FreeTextSegmentValue);
			writer.WriteElementString(Schema.QueueName, Queue.QueueName);
			writer.WriteElementString(Schema.ReasonCode, Queue.Status);
			writer.WriteElementString(Schema.StatusCode, Queue.SubStatus);
			writer.WriteElementString(Schema.Priority, Priority.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FreeTextSegmentName = reader.ReadElementString(Schema.FreeTextSegmentName);
			FreeTextSegmentValue = reader.ReadElementString(Schema.FreeTextSegmentValue);
			Queue.QueueName = reader.ReadElementString(Schema.QueueName);
			Queue.Status = reader.ReadElementString(Schema.ReasonCode);
			Queue.SubStatus = reader.ReadElementString(Schema.StatusCode);
			Priority = ZInt.Parse(reader.ReadElementString(Schema.Priority));
			RefreshBinding();
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CusHAWBAutoQueueMovement result = new CusHAWBAutoQueueMovement();
			if (fQueue != null)
			{
				result.fQueue = fQueue;
				result.RegisterEditableChildObject(result.Queue);
			}
			return result;
		}

		#endregion
	}
}
