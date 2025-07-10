using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ColumnConfigurationField : FilterFieldWithUTSupport, IColumnHeadingManagerListener, IJsonSerializable
	{
		public ColumnConfigurationField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal ColumnConfigurationField(ColumnConfigurationFieldJsonData data)
			: base(data)
		{
			var reportID = ZGuid.ParseSafe(data.ReportID);
			if (reportID.IsValid)
			{
				Manager = new ColumnConfigurationsManager(reportID, data.Scheduled);
				Manager.SaveToFilterField = data.ManagerSaveToFilterField;
				fName = data.Name;

				var uniqueDescription = data.UniqueDescription;
				var linkPK = new ZGuid(new Guid(data.LinkPK));
				if (linkPK.IsValid)
				{
					fValue = new CombinedConfigurationManager(Manager, linkPK, data.LinkCode, "", uniqueDescription);
				}
				else if (!string.IsNullOrEmpty(uniqueDescription))
				{
					fValue = new CombinedConfigurationManager(Manager, uniqueDescription);
				}
			}
		}

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is ColumnConfigurationField originalColumnConfigurationField)
			{
				((IColumnHeadingManagerListener)this).SetManager(originalColumnConfigurationField.Manager);
			}
		}

		#endregion

#if DEBUG
		public override void ClearValueForUnitTest()
		{
			Value = null;
		}
#endif

		public ColumnConfigurationManager Value
		{
			get
			{
				return fValue;
			}
			set
			{
				if (value != fValue)
				{
					if (value != null)
					{
						if (Manager == null)
						{
							value.Load();
						}
						else if (Manager.CurrentColumnConfigurationManager != value)
						{
							value.Load();
						}
					}

					fValue = value;

					if (ValueChanged != null)
					{
						ValueChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		ColumnConfigurationManager fValue;

		public event EventHandler ValueChanged;

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.ColumnConfigurationFieldUserControl; }
		}

		public override bool IsEmpty
		{
			get { return Value == null; }
		}

		public override object ValueAsObject
		{
			get { return Value; }
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		protected override string NonEmptyWhereClause()
		{
			return "";
		}

		public IReadOnlyCollection<ColumnConfigurationManager> SavedConfigurations
		{
			get
			{
				if (fSavedConfigurations == null)
				{
					fSavedConfigurations = Manager.ConfigurationManagersForAllSavedConfigurations.ToArray();
				}
				return fSavedConfigurations;
			}
		}
		ColumnConfigurationManager[] fSavedConfigurations;

		#region IColumnHeadingManagerListener Members

		public event EventHandler ConfigurationsSavedORDeleted;

		void IColumnHeadingManagerListener.SetManager(ColumnConfigurationsManager manager)
		{
			manager.CurrentConfigurationRefreshed += new EventHandler(Manager_ColumnConfigurationsManagerCurrentConfigurationChange);
			manager.ConfigurationDeleted += new ConfigurationSavedOrDeletedEventHandler(Manager_ConfigurationDeleted);
			manager.ConfigurationSaved += new ConfigurationSavedOrDeletedEventHandler(Manager_ConfigurationSaved);
			this.Manager = manager;
		}

		void Manager_ColumnConfigurationsManagerCurrentConfigurationChange(object sender, EventArgs e)
		{
			Value = Manager.CurrentColumnConfigurationManager;
		}

		void Manager_ConfigurationSaved(ColumnConfigurationManager saved)
		{
			NotifyConfigurationUpdated();
		}

		void Manager_ConfigurationDeleted(ColumnConfigurationManager deleted)
		{
			NotifyConfigurationUpdated();
			Value = SavedConfigurations.FirstOrDefault();
		}

		void NotifyConfigurationUpdated()
		{
			fSavedConfigurations = null;
			Value = Manager.CurrentColumnConfigurationManager;
			if (ConfigurationsSavedORDeleted != null)
			{
				ConfigurationsSavedORDeleted(this, EventArgs.Empty);
			}
		}

		internal ColumnConfigurationsManager Manager { get; private set; }

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new ColumnConfigurationFieldJsonData
			{
				LinkPK = fValue?.LinkPK.ToString() ?? string.Empty,
				ReportID = fValue?.ReportID.ToString() ?? string.Empty,
				Scheduled = fValue?.Scheduled ?? false,
				Description = fValue?.Description ?? string.Empty,
				LinkCode = fValue?.LinkCode ?? string.Empty,
				UniqueDescription = fValue?.UniqueDescription ?? string.Empty,
				ManagerSaveToFilterField = fValue?.HeadingManager?.SaveToFilterField,
				Name = fName
			};

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
		}

		public override void ClearValues()
		{
		}

		#endregion
	}
}
