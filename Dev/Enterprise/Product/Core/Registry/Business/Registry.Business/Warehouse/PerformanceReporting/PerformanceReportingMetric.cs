using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business.Warehouse
{
	#region class PerformanceReportingMetricType

	public class PerformanceReportingMetricType : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string OutboundOnTimeReadyToShip = "OOTRTS";
			public const string OutboundTotalOrderCycleTimeEnteredToReleased = "OTOCTETR";
			public const string OutboundOperationalOrderCycleTimePickedToFinalized = "OOCTPTF";
			public const string OutboundFillRateOrder = "OFRO";
			public const string OutboundFillRateLine = "OFRL";
			public const string OutboundBackOrdersAsAPercentOfTotalOrders = "OBO";
			public const string OutboundOrdersPerHour = "OOPH";
			public const string OutboundOrderLinesPerHour = "OOLPH";
			public const string OutboundPicksPerHour = "OPPH";
			public const string OutboundPickLinesPerHour = "OPLPH";

			public const string InboundDockToStock = "IDTS";
			public const string InboundEarlyReceipts = "IER";
			public const string InboundOnTimeReceipts = "ITR";
			public const string InboundLateReceipts = "ILR";
			public const string InboundDamageFreeReceiptsJob = "IDFRJ";
			public const string InboundDamageFreeReceiptsLine = "IDFRL";
			public const string InboundReceiveJobsPerHour = "IRJPH";
			public const string InboundReceiveLinesPerHour = "IRLPH";
		}

		public static class Descriptions
		{
			public static string OutboundOnTimeReadyToShip { get { return (NoResString)"Outbound - On Time Ready To Ship"; } }
			public static string OutboundTotalOrderCycleTimeEnteredToReleased { get { return (NoResString)"Outbound - Total Order Cycle Time Entered To Released"; } }
			public static string OutboundOperationalOrderCycleTimePickedToFinalized { get { return (NoResString)"Outbound - Operational Order Cycle Time Picked To Finalized"; } }
			public static string OutboundFillRateOrder { get { return (NoResString)"Outbound - Fill Rate - Order"; } }
			public static string OutboundFillRateLine { get { return (NoResString)"Outbound - Fill Rate - Line"; } }
			public static string OutboundBackOrdersAsAPercentOfTotalOrders { get { return (NoResString)"Outbound - Back Orders As A Percent Of Total Orders"; } }
			public static string OutboundOrdersPerHour { get { return (NoResString)"Outbound - Orders Per Hour"; } }
			public static string OutboundOrderLinesPerHour { get { return (NoResString)"Outbound - Order Lines Per Hour"; } }
			public static string OutboundPicksPerHour { get { return (NoResString)"Outbound - Picks Per Hour"; } }
			public static string OutboundPickLinesPerHour { get { return (NoResString)"Outbound - Pick Lines Per Hour"; } }

			public static string InboundDockToStock { get { return (NoResString)"Inbound - Dock To Stock"; } }
			public static string InboundEarlyReceipts { get { return (NoResString)"Inbound - Early Receipts"; } }
			public static string InboundOnTimeReceipts { get { return (NoResString)"Inbound - On Time Receipts"; } }
			public static string InboundLateReceipts { get { return (NoResString)"Inbound - Late Receipts"; } }
			public static string InboundDamageFreeReceiptsJob { get { return (NoResString)"Inbound - Damage Free Receipts - Job"; } }
			public static string InboundDamageFreeReceiptsLine { get { return (NoResString)"Inbound - Damage Free Receipts - Line"; } }
			public static string InboundReceiveJobsPerHour { get { return (NoResString)"Inbound - Receive Jobs Per Hour"; } }
			public static string InboundReceiveLinesPerHour { get { return (NoResString)"Inbound - Receive Lines Per Hour"; } }
		}

		public PerformanceReportingMetricType()
		{
			AddPair(Codes.OutboundOnTimeReadyToShip, Descriptions.OutboundOnTimeReadyToShip);
			AddPair(Codes.OutboundTotalOrderCycleTimeEnteredToReleased, Descriptions.OutboundTotalOrderCycleTimeEnteredToReleased);
			AddPair(Codes.OutboundOperationalOrderCycleTimePickedToFinalized, Descriptions.OutboundOperationalOrderCycleTimePickedToFinalized);
			AddPair(Codes.OutboundFillRateOrder, Descriptions.OutboundFillRateOrder);
			AddPair(Codes.OutboundFillRateLine, Descriptions.OutboundFillRateLine);
			AddPair(Codes.OutboundBackOrdersAsAPercentOfTotalOrders, Descriptions.OutboundBackOrdersAsAPercentOfTotalOrders);
			AddPair(Codes.OutboundOrdersPerHour, Descriptions.OutboundOrdersPerHour);
			AddPair(Codes.OutboundOrderLinesPerHour, Descriptions.OutboundOrderLinesPerHour);
			AddPair(Codes.OutboundPicksPerHour, Descriptions.OutboundPicksPerHour);
			AddPair(Codes.OutboundPickLinesPerHour, Descriptions.OutboundPickLinesPerHour);
			AddPair(Codes.InboundDockToStock, Descriptions.InboundDockToStock);
			AddPair(Codes.InboundEarlyReceipts, Descriptions.InboundEarlyReceipts);
			AddPair(Codes.InboundOnTimeReceipts, Descriptions.InboundOnTimeReceipts);
			AddPair(Codes.InboundLateReceipts, Descriptions.InboundLateReceipts);
			AddPair(Codes.InboundDamageFreeReceiptsJob, Descriptions.InboundDamageFreeReceiptsJob);
			AddPair(Codes.InboundDamageFreeReceiptsLine, Descriptions.InboundDamageFreeReceiptsLine);
			AddPair(Codes.InboundReceiveJobsPerHour, Descriptions.InboundReceiveJobsPerHour);
			AddPair(Codes.InboundReceiveLinesPerHour, Descriptions.InboundReceiveLinesPerHour);
		}
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PerformanceReportingMetric : RegistryBusinessObjectTemplate, IDisposable
	{
		#region Schema

		public static class Schema
		{
			public const string MetricCode = "MetricCode";
			public const string MetricName = "MetricName";
			public const string MetricCategories = "MetricCategories";
		}

		#endregion

		#region MetricCode

		[CargoWise.ComponentModel.MaxLength(10)]
		public ZString MetricCode
		{
			get { return metricCode; }
			set
			{
				value = value.TrimEnd(' ');
				if (value != metricCode)
				{
					CheckMaximumLength(MetricCodeInfo, value);
					SetNonPersistentPropertyValue<ZString>(MetricCodeInfo, ref metricCode, value);
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateMetriCode();
					}
				}
			}
		}
		ZString metricCode;

		public ZPropertyInfo MetricCodeInfo
		{
			get { return GetZPropertyInfo(Schema.MetricCode); }
		}

		void ValidateMetriCode()
		{
			MetricCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MetricCodeInfo, (IMultilingualString)ResString.GetMultilingualString("{3A345A9A-FCE8-4BBF-8CA6-72BDBF015871}", "Metric Code"));
		}

		#endregion

		#region MetricName

		public ZString MetricName
		{
			get { return !MetricCode.IsEmpty ? MetricList.GetDescriptionFromCode(MetricCode) : string.Empty; }
		}

		PerformanceReportingMetricType MetricList
		{
			get { return metricList ?? (metricList = new PerformanceReportingMetricType()); }
		}

		PerformanceReportingMetricType metricList;

		#endregion

		#region MetricCategories

		[ChildEditable]
		public PerformanceReportingMetricCategoryCollection MetricCategories
		{
			get
			{
				if (metricCategories == null)
				{
					metricCategories = new PerformanceReportingMetricCategoryCollection(this);
					RegisterEditableChildObject(metricCategories);
				}
				return metricCategories;
			}
		}

		PerformanceReportingMetricCategoryCollection metricCategories;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (metricCategories != null)
			{
				UnRegisterEditableChildObject(metricCategories);
				metricCategories = null;
			}
		}

		#endregion

		#region Implementation

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new PerformanceReportingMetric();
			try
			{
				((IBusinessObjectInternals)result).IsCopying = true;
				result.MetricCode = MetricCode;
				result.MetricCategories.RemoveAll();
				result.MetricCategories.AddRange((BusinessObjectCollection)MetricCategories.Clone(fallbackLevel, factory));
			}
			finally
			{
				result.HasChanges = false;
				((IBusinessObjectInternals)result).IsCopying = false;
			}
			return result;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MetricCode, MetricCode);
			PerformanceReportingCategoryCollectionSerializer.Serialize(writer, MetricCategories);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			try
			{
				((IBusinessObjectInternals)this).IsCopying = true;
				MetricCode = reader.ReadElementString(Schema.MetricCode);
				MetricCategories.RemoveAndDeleteAll();
				MetricCategories.AddRange((PerformanceReportingMetricCategoryCollection)PerformanceReportingCategoryCollectionSerializer.Deserialize(reader));
			}
			finally
			{
				HasChanges = false;
				((IBusinessObjectInternals)this).IsCopying = false;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateMetriCode();
		}

		#endregion

		ZXmlSerializer PerformanceReportingCategoryCollectionSerializer
		{
			get { return performanceReportingCategoryCollectionSerializer ?? (performanceReportingCategoryCollectionSerializer = ZXmlSerializer.New(typeof(PerformanceReportingMetricCategoryCollection))); }
		}

		ZXmlSerializer performanceReportingCategoryCollectionSerializer;

		#endregion
	}
}
