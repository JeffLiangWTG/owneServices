using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Modules;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.Business
{
	public class CopyTemplateTreeBizo : EntityCopyTemplateBizo
	{
		public CopyTemplateTreeBizo(CopyTemplateTree copyTemplateTree, UniversalCopyTemplate parent)
			: base(copyTemplateTree, copyTemplateTree)
		{
			Parent = parent;
		}

		public UniversalCopyTemplate Parent { get; private set; }

		public new CopyTemplateTree CopyTemplateNode => (CopyTemplateTree)base.CopyTemplateNode;

		#region Properties

		#region ConfigurationName

		public ZString ConfigurationName
		{
			get { return CopyTemplateNode.ConfigurationName; }
			set
			{
				if (CopyTemplateNode.ConfigurationName != value)
				{
					CopyTemplateNode.ConfigurationName = value;
					if (Parent != null)
					{
						Parent.SyncNameFromTemplateTree();
					}
					HasChanges = true;
					ConfigurationNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ConfigurationNameInfo => GetZPropertyInfo(nameof(ConfigurationName));

		#endregion

		#region FilterList

		[ResourceStringData("Enterprise.UniversalCopy.Business.CopyTemplateTreeBizo|FilterList", Caption = "Applicable Filter", ShortCaption = "Filter")]
		public ZString FilterList
		{
			get { return CopyTemplateNode.FilterList; }
			set
			{
				if (CopyTemplateNode.FilterList != value)
				{
					CopyTemplateNode.FilterList = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						Validation.ValidateFilterList();
					}
					FilterListInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FilterListInfo => GetZPropertyInfo(nameof(FilterList));

		#endregion

		#region ConfigurationSource

		[List("ConfigurationSources")]
		public ZString ConfigurationSource
		{
			get { return CopyTemplateNode.ConfigurationSource; }
			set
			{
				if (CopyTemplateNode.ConfigurationSource != value)
				{
					CopyTemplateNode.ConfigurationSource = value;

					HasChanges = true;
					ConfigurationSourceInfo.RefreshBinding();
					ConfigurationSourceDescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ConfigurationSourceInfo => GetZPropertyInfo(nameof(ConfigurationSource));

		public CodeDescriptionPairList ConfigurationSources => configurationSources ?? (configurationSources = GetConfigurationSourcesCore());
		CodeDescriptionPairList configurationSources;

		CodeDescriptionPairList GetConfigurationSourcesCore()
		{
			var copyMethods = new CodeDescriptionPairList();
			copyMethods.AddPair(ConfigurationSourceCodes.Selected, ResString.GetMultilingualString("25EA5AF9-CB02-4A0C-BEE5-8E50B561181D", "Selected"));
			copyMethods.AddPair(ConfigurationSourceCodes.FilteredRecord, ResString.GetMultilingualString("E35F6E36-E750-4DFD-B306-6C1B077A11B0", "Filtered record"));
			copyMethods.AddPair(ConfigurationSourceCodes.NominatedRecord, ResString.GetMultilingualString("165CE079-939F-4C29-9E34-6FD4BB614408", "Nominated record"));
			return copyMethods;
		}

#if DEBUG
		[BusinessObjectTestExclude]
#endif
		[List("ConfigurationSourceDescriptions")]
		public ZString ConfigurationSourceDescription
		{
			get { return ConfigurationSources.GetDescriptionFromCode(ConfigurationSource); }
			set
			{
				ConfigurationSource = ConfigurationSources.GetCodeFromDescription(value);
				ConfigurationSourceDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConfigurationSourceDescriptionInfo => GetZPropertyInfo(nameof(ConfigurationSourceDescription));

		public CodeDescriptionPairList ConfigurationSourceDescriptions
		{
			get
			{
				if (configurationSourceDescriptions == null)
				{
					configurationSourceDescriptions = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair item in ConfigurationSources)
					{
						configurationSourceDescriptions.AddPair(item.MultilingualDescription);
					}
				}
				return configurationSourceDescriptions;
			}
		}
		CodeDescriptionPairList configurationSourceDescriptions;

		#endregion

		#region NominatedRecordPk

		[List("NominatedObjectsCollection")]
		public ZGuid NominatedRecordPk
		{
			get
			{
				return CopyTemplateNode.NominatedRecordPk;
			}
			set
			{
				if (CopyTemplateNode.NominatedRecordPk != value)
				{
					CopyTemplateNode.NominatedRecordPk = (value.IsEmpty || !value.IsValid ? Guid.Empty : value.ToGuid());

					HasChanges = true;
					NominatedRecordPkInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo NominatedRecordPkInfo => GetZPropertyInfo(nameof(NominatedRecordPk));

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList NominatedObjectsCollection
		{
			get
			{
				if (nominatedObjectsCollection == null)
				{
					var module = NewModuleFromModuleID();
					if (module != null)
					{
						if (module is IZFilterGridModule filterGridModule)
						{
							filterGridModule.AllowLoadTemplateRecords = true;
						}

						nominatedObjectsCollection = module.GetNewBusinessObjectCollection();
					}
				}

				return nominatedObjectsCollection;
			}
		}
		IList nominatedObjectsCollection;

		protected IZFilterModule NewModuleFromModuleID()
		{
			IZFilterModule module = null;
			var moduleID = Parent.GetModuleIdentifier();

			if (moduleID != null && moduleID != ModuleIDs.NotAssigned)
			{
				var tempModule = GetZModule(moduleID) ?? throw new ZException("ZModuleFactory did not return a module for ID : " + moduleID.ToString());

				module = tempModule as IZFilterModule;
				tempModule.Dispose();
				if (module == null)
				{
					throw new ZException("Module with ID " + moduleID.ToString() + " of type " + tempModule.GetType().Name + " is not ZFilterModule.");
				}
			}

			return module;
		}

		[DefaultValue("")]
		public string CountryOverride { get; set; }

		IZModule GetZModule(ModuleIdentifier moduleID)
		{
			if (!string.IsNullOrEmpty(CountryOverride))
			{
				return ObjectFactory.Get<IModuleFactory>().Create(moduleID, CountryOverride);
			}
			else
			{
				return ObjectFactory.Get<IModuleFactory>().Create(moduleID);
			}
		}

		#endregion

		#region OrderBy

		public ZString OrderBy
		{
			get
			{
				if (orderBy.IsEmpty && !updated)
				{
					if (EntityFilter != null)
					{
						orderBy = EntityFilter.OrderBy;
					}
					updated = true;
				}
				return orderBy;
			}
			set
			{
				SetNonPersistentPropertyValue(OrderByInfo, ref orderBy, value, true);
			}
		}
		ZString orderBy;
		bool updated;

		public ZPropertyInfo OrderByInfo => GetZPropertyInfo(nameof(OrderBy));

		[List("OrderByFieldList")]
		public ZString OrderByField
		{
			get { return OrderBy.Split(' ').FirstOrDefault(); }
			set
			{
				if (value != OrderByField)
				{
					OrderBy = value + (OrderByDescending && !value.IsEmpty ? new ZString(" DESC") : ZString.Empty);
					OrderByFieldInfo.RefreshBinding();
					HasChanges = true;

					if (value.IsEmpty)
					{
						OrderByDescending = false;
					}
				}
			}
		}

		public ZPropertyInfo OrderByFieldInfo => GetZPropertyInfo(nameof(OrderByField));

		public ZBool OrderByDescending
		{
			get { return OrderBy.Split(' ').Skip(1).FirstOrDefault() == "DESC"; }
			set
			{
				if (value != OrderByDescending)
				{
					OrderBy = OrderByField + (value ? new ZString(" DESC") : ZString.Empty);
					OrderByDescendingInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo OrderByDescendingInfo => GetZPropertyInfo(nameof(OrderByDescending));

		public CodeDescriptionPairList OrderByFieldList
		{
			get
			{
				if (orderByFieldList == null)
				{
					orderByFieldList = new CodeDescriptionPairList();
					foreach (PropertyCopyTemplateBizo propertyBizo in PropertyNodes)
					{
						orderByFieldList.Add(new CodeDescriptionPair(propertyBizo.Name.ToString(), propertyBizo.Description.ToString()));
					}
				}
				return orderByFieldList;
			}
		}
		CodeDescriptionPairList orderByFieldList;

		#endregion

		#region IsActive

		public ZBool IsActive
		{
			get { return CopyTemplateNode.IsActive; }
			set
			{
				if (CopyTemplateNode.IsActive != value)
				{
					CopyTemplateNode.IsActive = value;
					IsActiveInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo IsActiveInfo => GetZPropertyInfo(nameof(IsActive));

		#endregion

		#endregion

		#region Overrides

		public override EntityFilter EntityFilter
		{
			get { return CopyTemplateNode.Filter; }
			set { CopyTemplateNode.Filter = value; }
		}

		protected override string EntityTableNameCore => CopyTemplateNode.TableName;

		protected override ZString KindCore => Res.GetString("928dae25-a6aa-46cf-802a-88cb76144fea", "Root Element");

		protected internal override bool NeedsChildrenForCopy()
		{
			return true;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("376d1540-1f4c-4d13-b248-bc701b815496", "Copy Template");

		#endregion

		#region Validation

		public new CopyTemplateTreeBizoValidation Validation => (CopyTemplateTreeBizoValidation)base.Validation;

		protected override CopyTemplateNodeBizoValidation GetNewValidation()
		{
			return new CopyTemplateTreeBizoValidation(this);
		}

		#endregion
	}

	#region Validation Class

	public class CopyTemplateTreeBizoValidation : CopyTemplateNodeBizoValidation
	{
		public CopyTemplateTreeBizoValidation(CopyTemplateTreeBizo parent) : base(parent) { }

		protected new CopyTemplateTreeBizo Parent
		{
			get { return (CopyTemplateTreeBizo)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateConfigurationName();
			ValidateFilterList();
			ValidateIsFiltered();
			ValidateOrderBy();
			ValidateOrderByField();
			ValidateHasSomethingToCopy();
			ValidateNominatedRecordPk();
		}

		#region Properties Validation

		public void ValidateConfigurationName()
		{
			ZValidationInternals.Validate(Parent.ConfigurationNameInfo, CheckConfigurationName);
		}

		void CheckConfigurationName()
		{
			MandatoryValidation.CheckEntered(Parent.ConfigurationNameInfo);
			if (Parent.Parent != null && !Parent.ConfigurationNameInfo.HasErrors())
			{
				Parent.Parent.Validation.ValidateS9_FilterName();
				if (Parent.Parent.S9_FilterNameInfo.HasNotifications())
				{
					Parent.ConfigurationNameInfo.AddAllNotificationsFrom(Parent.Parent.S9_FilterNameInfo);
				}
			}
		}

		public void ValidateFilterList()
		{
			ZValidationInternals.Validate(Parent.FilterListInfo, CheckFilterList);
		}

		void CheckFilterList()
		{
			if (!Parent.FilterList.IsEmpty)
			{
				var parent = Parent.Parent;
				if (parent != null && parent.IsPublishedGlobal && !parent.IsApplicable)
				{
					Parent.FilterListInfo.AddError(Res.GetString("16FF25DA-A68E-4AD6-BF10-188DA9D15B9A", "Filter should only be used to restrict the template to the same current login country/region.\r\nThis filter '{0}' is not valid for this country/region '{1}'; valid filter should be either 'CTY={1}' or 'BKRCTY={1}'.", Parent.FilterList, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				}
			}
		}

		public void ValidateHasSomethingToCopy()
		{
			Parent.ClearRowNotifications();
			var innerEntityNode = Parent.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			if (innerEntityNode != null && !innerEntityNode.Nodes.Any(node => node.HasData()))
			{
				Parent.AddRowError(Res.GetString("ff1c6d72-0597-4835-a45b-0239418b8ae1", "Unable to save this template: there is nothing selected on main element to copy."));
			}
		}

		public void ValidateIsFiltered() { }

		public void ValidateOrderBy()
		{
			ZValidationInternals.Validate(Parent.OrderByInfo, CheckOrderBy);
		}

		void CheckOrderBy()
		{
			if (Parent.ConfigurationSource == ConfigurationSourceCodes.FilteredRecord)
			{
				if (!Parent.OrderBy.IsEmpty)
				{
					var query = new ZQuery { FetchOnlyFromLocalCache = true, MaximumRows = 1, OrderBy = Parent.OrderBy };
					BusinessObjectFactory factory = Parent.FilterStripBizo != null ? Parent.FilterStripBizo.Factory : new BusinessObjectFactory { RefreshEnabled = false };

					try
					{
						((IBusinessObjectFactoryInternals)factory).RowFactory.Load(Parent.CopyTemplateNode.TableName, query);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (ex.IsCriticalException())
						{
							throw;
						}
						Parent.OrderByInfo.AddError(Res.GetString("6797791f-2551-4fee-9eb2-a6471f93799e", "Enter correct Order By expression.") + " " + ex.Message);
					}
				}
			}
		}

		public void ValidateOrderByField()
		{
			ZValidationInternals.Validate(Parent.OrderByFieldInfo, CheckOrderByField);
		}

		void CheckOrderByField()
		{
			if (Parent.ConfigurationSource == ConfigurationSourceCodes.FilteredRecord)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OrderByFieldInfo);
			}
		}

		public void ValidateNominatedRecordPk()
		{
			ZValidationInternals.Validate(Parent.NominatedRecordPkInfo, CheckNominatedRecordPk);
		}

		void CheckNominatedRecordPk()
		{
			if (Parent.ConfigurationSource == ConfigurationSourceCodes.NominatedRecord)
			{
				MandatoryValidation.CheckEntered(Parent.NominatedRecordPkInfo);
				ListValidation.ErrorIfInvalidPK(Parent.NominatedRecordPkInfo);
			}
		}

		#endregion
	}

	#endregion
}
