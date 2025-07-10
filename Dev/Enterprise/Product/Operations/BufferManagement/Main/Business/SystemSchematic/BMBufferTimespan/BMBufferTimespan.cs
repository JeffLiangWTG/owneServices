using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(nameof(BMT_Name)), DescriptionProperty(nameof(TimeDisplay)), PreventDelete(false)]
	public class BMBufferTimespan : AutoBMBufferTimespan, IBMBufferTimespan, IAuditParent
	{
		public BMBufferTimespan(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			BMT_BufferTimespanInMinutes = 42;
		}

#endif
		#endregion

		#region New Properties

		[BusinessObjectTestExclude]
		[ResourceStringData("BMBufferTimespan.BufferTimespan", Caption = "Buffer Timespan")]
		public ZDateTime BufferTimespan
		{
			get { return BMT_BufferTimespanInMinutes.GetDateTimeFromMinutes(); }
			set
			{
				BMT_BufferTimespanInMinutes = (ZInt)value.GetMinutesFromDateTimeSpan();
				BufferTimespanInfo.RefreshBinding();
				BMT_BufferTimespanInMinutesInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo BufferTimespanInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BufferTimespan), o => BMT_BufferTimespanInMinutesInfo); }
		}

		public double BufferTimeSpanHours
		{
			get { return BMT_BufferTimespanInMinutes / 60.0; }
		}

		public ZString TimeDisplay => $"{BMT_BufferTimespanInMinutes / 60:000}:{BMT_BufferTimespanInMinutes % 60:00}";

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
