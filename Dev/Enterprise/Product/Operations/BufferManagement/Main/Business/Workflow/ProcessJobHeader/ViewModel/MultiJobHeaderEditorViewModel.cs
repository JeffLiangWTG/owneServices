using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class MultiJobHeaderEditorViewModel : NonPersistentBusinessObject<MultiJobHeaderEditorViewModelValidation>, IDateAcceptability
	{
		public MultiJobHeaderEditorViewModel(IEnumerable<BusinessObject> scheduledEntities, BusinessObjectFactory factory)
			: base(factory)
		{
			this.scheduledEntities = scheduledEntities.ToArray();
		}

		readonly BusinessObject[] scheduledEntities;

		#region BusinessObject Overrides

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			foreach (JobHeaderView view in JobHeaderViews)
			{
				if (view.HasChanges)
				{
					var earliestStartUTC = view.EarliestStartDateLocal.ToUniversalBranchTime(Factory);
					var agreedDeliveryUTC = view.AgreedDeliveryDateLocal.ToUniversalBranchTime(Factory);

					view.ProcessHeader.FH_DoNotStartBeforeDate = earliestStartUTC.WithinSmallDateTimeRange();
					view.ProcessHeader.FH_AgreedDeliveryDate = agreedDeliveryUTC.WithinSmallDateTimeRange();
					view.ProcessHeader.FH_DateAcceptability = view.DateAcceptability;
				}
			}
		}

		public override MultiJobHeaderEditorViewModelValidation GetNewValidation()
		{
			return new MultiJobHeaderEditorViewModelValidation(this);
		}

		#endregion

		#region Properties

		[ResourceStringData("MultiJobHeaderEditorViewModel.EarliestStartDateLocal", Caption = "Earliest Start Date", FullDescription = "The Earliest Start Date to set on all selected rows.")]
		public ZDateTime EarliestStartDateLocal
		{
			get { return earliestStartDateLocal; }
			set { SetRefreshingBinding(ref earliestStartDateLocal, EarliestStartDateLocalInfo, value); }
		}
		ZDateTime earliestStartDateLocal;

		public ZPropertyInfo EarliestStartDateLocalInfo => GetZPropertyInfo(nameof(EarliestStartDateLocal));

		[ResourceStringData("MultiJobHeaderEditorViewModel.AgreedDeliveryDateLocal", Caption = "Agreed Delivery Date", FullDescription = "The Agreed Delivery Date to set on all selected rows.")]
		public ZDateTime AgreedDeliveryDateLocal
		{
			get { return agreedDeliveryDateLocal; }
			set { SetRefreshingBinding(ref agreedDeliveryDateLocal, AgreedDeliveryDateLocalInfo, value); }
		}
		ZDateTime agreedDeliveryDateLocal;

		public ZPropertyInfo AgreedDeliveryDateLocalInfo => GetZPropertyInfo(nameof(AgreedDeliveryDateLocal));

		[ResourceStringData("MultiJobHeaderEditorViewModel.EarliestStartDateDays", Caption = "Earliest Start Date: Days", FullDescription = "The number of days (positive or negative) to add to the Earliest Start Date of all selected rows.")]
		public ZInt EarliestStartDateDays
		{
			get { return earliestStartDateDays; }
			set { SetRefreshingBinding(ref earliestStartDateDays, EarliestStartDateDaysInfo, value); }
		}
		ZInt earliestStartDateDays;

		public ZPropertyInfo EarliestStartDateDaysInfo => GetZPropertyInfo(nameof(EarliestStartDateDays));

		[ResourceStringData("MultiJobHeaderEditorViewModel.AgreedDeliveryDateDays", Caption = "Agreed Delivery Date: Days", FullDescription = "The number of days (positive or negative) to add to the Agreed Delivery Date of all selected rows.")]
		public ZInt AgreedDeliveryDateDays
		{
			get { return agreedDeliveryDateDays; }
			set { SetRefreshingBinding(ref agreedDeliveryDateDays, AgreedDeliveryDateDaysInfo, value); }
		}
		ZInt agreedDeliveryDateDays;

		public ZPropertyInfo AgreedDeliveryDateDaysInfo => GetZPropertyInfo(nameof(AgreedDeliveryDateDays));

		[ZDateTimeOffsetValueNegatable]
		[ResourceStringData("MultiJobHeaderEditorViewModel.EarliestStartDateOffset", Caption = "Hours", FullDescription = "The number of hours (positive or negative) to add to the Earliest Start Date of all selected rows.")]
		public ZDateTime EarliestStartDateOffset
		{
			get { return earliestStartDateOffset; }
			set { SetRefreshingBinding(ref earliestStartDateOffset, EarliestStartDateOffsetInfo, value.ConvertToDurationBasedDate(EarliestStartDateOffsetInfo)); }
		}
		ZDateTime earliestStartDateOffset;

		public ZPropertyInfo EarliestStartDateOffsetInfo => GetZPropertyInfo(nameof(EarliestStartDateOffset));

		[ZDateTimeOffsetValueNegatable]
		[ResourceStringData("MultiJobHeaderEditorViewModel.AgreedDeliveryDateOffset", Caption = "Hours", FullDescription = "The number of hours (positive or negative) to add to the Agreed Delivery Date of all selected rows.")]
		public ZDateTime AgreedDeliveryDateOffset
		{
			get { return agreedDeliveryDateOffset; }
			set { SetRefreshingBinding(ref agreedDeliveryDateOffset, AgreedDeliveryDateOffsetInfo, value.ConvertToDurationBasedDate(AgreedDeliveryDateOffsetInfo)); }
		}
		ZDateTime agreedDeliveryDateOffset;

		public ZPropertyInfo AgreedDeliveryDateOffsetInfo => GetZPropertyInfo(nameof(AgreedDeliveryDateOffset));

		static void SetRefreshingBinding<T>(ref T field, ZPropertyInfo info, T value)
		{
			field = value;
			info.RefreshBinding();
		}

		[ResourceStringData("MultiJobHeaderEditorViewModel.FH_DateAcceptability", Caption = "Date Acceptability", FullDescription = "The 'hardness' or 'softness' for a late or early delivery against the agreed delivery date")]
		[List("DateAcceptabilities")]
		public ZString FH_DateAcceptability
		{
			get { return fH_DateAcceptability; }
			set { SetRefreshingBinding(ref fH_DateAcceptability, FH_DateAcceptabilityInfo, value); }
		}
		ZString fH_DateAcceptability;

		public ZPropertyInfo FH_DateAcceptabilityInfo => GetZPropertyInfo(nameof(FH_DateAcceptability));

		public CodeDescriptionPairList DateAcceptabilities
		{
			get { return Factory.GetCachedValue<DateAcceptabilityList>(); }
		}

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public JobHeaderViewCollection JobHeaderViews
		{
			get
			{
				if (jobHeaderViews == null)
				{
					jobHeaderViews = new JobHeaderViewCollection(scheduledEntities, Factory);
					RegisterEditableChildObject(jobHeaderViews);
				}

				return jobHeaderViews;
			}
		}

		JobHeaderViewCollection jobHeaderViews;

		#endregion

		#region MassUpdate

		public void MassUpdate(IEnumerable<JobHeaderView> selectedBizos, SetProperty setProperty)
		{
			foreach (var bizo in selectedBizos)
			{
				switch (setProperty)
				{
					case SetProperty.AgreedDeliveryDate:
						bizo.AgreedDeliveryDateLocal = AgreedDeliveryDateLocal;
						break;

					case SetProperty.EarliestStartDate:
						bizo.EarliestStartDateLocal = EarliestStartDateLocal;
						break;

					case SetProperty.FH_DateAcceptability:
						bizo.DateAcceptability = FH_DateAcceptability;
						break;
				}
			}
		}

		public void MassUpdateIncrementalValues(IEnumerable<JobHeaderView> selectedBizos, SetProperty setProperty)
		{
			foreach (var bizo in selectedBizos)
			{
				switch (setProperty)
				{
					case SetProperty.AgreedDeliveryDate:
						UpdateIncrementalValues(bizo.AgreedDeliveryDateLocalInfo, AgreedDeliveryDateDays, AgreedDeliveryDateOffset);
						break;

					case SetProperty.EarliestStartDate:
						UpdateIncrementalValues(bizo.EarliestStartDateLocalInfo, EarliestStartDateDays, EarliestStartDateOffset);
						break;
				}
			}
		}

		static void UpdateIncrementalValues(ZPropertyInfo property, ZInt daysToAdd, ZDateTime offsetToAdd)
		{
			var propertyValue = (ZDateTime)property.Value;

			if (propertyValue.IsEmpty)
			{
				propertyValue = ZDateTime.Now;
			}

			propertyValue = propertyValue.AddDays(daysToAdd);

			if (offsetToAdd.IsValid)
			{
				var span = offsetToAdd - ZDateTime.DefaultNegatableDurationEpoch;

				propertyValue = propertyValue.Add(span);
			}

			property.Value = propertyValue;
		}

		#endregion

		public enum SetProperty
		{
			Unknown = 0,
			EarliestStartDate,
			AgreedDeliveryDate,
			FH_DateAcceptability
		}

		#region IDateAcceptability Members

		ZString IDateAcceptability.FH_DateAcceptability => FH_DateAcceptability;

		ZPropertyInfo IDateAcceptability.FH_DateAcceptabilityInfo => FH_DateAcceptabilityInfo;

		#endregion
	}
}
