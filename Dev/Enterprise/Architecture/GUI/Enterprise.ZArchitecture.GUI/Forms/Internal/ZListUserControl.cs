using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[DefaultDataSourceBindingMember(null)]
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	[CompositeFieldControl]
	public abstract class ZListUserControl : ZUserControl, IBindTo, IDataBoundControl, IBindToList, IBindingMemberForCompileTimeCheckProvider, IAdditionalInformation
	{
		#region CurrentItem / DataPropertyName / List / PullList

		/// <summary>
		/// Get the property name that the control is bound to (which is ColumnName when bound to a DataGrid column).
		/// The property sits on the object returned from CurrentItem.<br>
		/// <br>
		/// This is distinct from DataMember which expresses the full path to the property from the DataSource.
		/// </summary>
		protected internal string DataPropertyName
		{
			get { return dataPropertyName ?? new KBindingMemberInfo(DataMember).BindingField; }
			set { dataPropertyName = value; }
		}
		string dataPropertyName;

		/// <summary>
		/// Get the business object for which the control is bound to, or the business object on the current
		/// row of a grid.<br />
		/// <br />
		/// This is distinct from DataSource which is the top level binding object.
		/// For example, you may have this situation:
		/// - DataSource is ForwardingShipment  (this property is empty when bound to a DataGrid column)
		/// - DataMember = OrderLines.JI_Partno (this property is empty when bound to a DataGrid column)
		/// - CurrentItem is OrderLine
		/// - DataPropertyName = JI_Partno
		/// </summary>
		protected internal object CurrentItem
		{
			get
			{
				var result = currentItem;
				if (currentItem == null)
				{
					var bm = DataSource == null ? null : BindingContext[DataSource, new KBindingMemberInfo(DataMember).BindingPath];
					result = (bm == null || bm.Position == -1) ? null : bm.GetCurrent();
				}
				return result;
			}
			set { currentItem = value; }
		}
		object currentItem;

		PropertyDescriptor listPropertyDescriptor;
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PropertyDescriptor ListPropertyDescriptor
		{
			get
			{
				return listPropertyDescriptor;
			}
		}

		internal bool EnablePullListCostTimeRecording { get; set; }

		internal double PullListCost { get; set; }

		/// <summary>
		/// Get the list which is shown in a combo box drop down or find box find screen.
		/// </summary>
		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual IList List
		{
			get
			{
				if (list == null)
				{
					if (!inPullList)
					{
						inPullList = true;
						try
						{
							if (EnablePullListCostTimeRecording)
							{
								var stopWatch = new Stopwatch();
								stopWatch.Start();
								PullList();
								stopWatch.Stop();
								PullListCost = stopWatch.Elapsed.TotalSeconds;
							}
							else
							{
								PullList();
							}
						}
						finally
						{
							inPullList = false;
						}
					}
				}
				return list;
			}
			set
			{
				var isChanged = list != value;
				list = value;

				if (value is IBusinessObjectCollection collection)
				{
					collection.Factory?.ThreadSentry.EnsureCurrentThreadIsOwner(() => $"Trying to assign a collection owned by another thread to List. ZListUserControl type: {GetType()}, collection type: {collection.GetType()}"); // Developer diagnostic info
				}

				if (isChanged)
				{
					InvalidateListCore();
				}
			}
		}
		IList list;
		bool inPullList;

		protected IList ListNoPullIfNull
		{
			get { return list; }
		}

		internal void InvalidateList()
		{
			if (!DisableInvalidation)
			{
				list = null;
				InvalidateListCore();
			}
		}

		[DefaultValue(false)]
		public bool DisableInvalidation { get; set; }

		protected virtual void InvalidateListCore()
		{
		}

		internal void PullList()
		{
			IList newList = null;
			if (DataSource != null || CurrentItem != null)
			{
				var dataMember = DataSource != null ? DataMember : DataPropertyName;
				var dataSource = DataSource ?? CurrentItem;

				var listMember = GetListMember(dataSource, dataMember, true);

				if (string.IsNullOrEmpty(listMember))
				{
					var bindingInfo = new KBindingMemberInfo(dataMember);

					var context = BindingContext[dataSource, bindingInfo.BindingPath];
					if (context != null)
					{
						newList = (IList)ZMetaData.GetMetaData(
							CurrentItem,
							context.GetItemProperties()[bindingInfo.BindingField],
							MetaDataTypes.ListDataSource);
						AddAdditionalInformationIfNeeded(newList, bindingInfo.BindingField);
					}
				}

				if (newList == null)
				{
					newList = GetList(dataSource, listMember, dataMember);
				}
			}
			List = newList;
		}

		string GetListMember(object dataSource, string dataMember, bool throwOnPropertyNotFound)
		{
			var fullBindToList = string.IsNullOrEmpty(BindTo) || string.IsNullOrEmpty(bindToList) ? bindToList : BindingHelper.GetNestedControlDataMember(BindTo, dataMember, bindToList);
			var property = !dataMember.Contains(".") ? ZCustomTypeDescriptor.GetProperties(ListUtil.GetListElementType(dataSource.GetType()) ?? dataSource.GetType())[dataMember] : BindingHelper.GetDescriptor(BindingContext, dataSource, dataMember, throwOnPropertyNotFound);
			var listMember = property == null ? fullBindToList : MetadataAccessor.GetListMember(fullBindToList, property, dataMember);

			listMember = listMember == null ? "" : listMember.Replace(ZLookups.LookupsBindingMember + ".", ZLookups.LookupsBindingMember + "+");

			return listMember;
		}

		#endregion

		#region GetList

		protected virtual IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return GetList(dataSource, listMember, this, dataMemberForErrorReporting, ref preventErrorReportForMissingListAttribute);
		}

		internal static IList GetList(object dataSource, string listMember, Control source, string dataMemberForErrorReporting)
		{
			var placeHolderCounter = 0;
			return GetList(dataSource, listMember, source, dataMemberForErrorReporting, ref placeHolderCounter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		static IList GetList(object dataSource, string listMember, Control source, string dataMemberForErrorReporting, ref int preventErrorReportForMissingListAttribute)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException(nameof(dataSource));
			}

			if (source.IsDisposed)
			{
				throw new InvalidOperationException("ZListUserControl already been disposed when GetList is called.");
			}

			IList result = null;

			var parsedBindToList = new KBindingMemberInfo(listMember);

			var bizO = dataSource as BusinessObject;
			if (bizO != null && bizO.IsDeleted)
			{
				return null;
			}

			if (bizO != null && listMember.CountMatches('+') <= 1)
			{
				var lookupCollection = ReturnLookupCollectionSpecifiedInListDecoration(source, bizO, listMember);
				if (lookupCollection != null)
				{
					AddAdditionalInformationIfNeeded(lookupCollection, listMember);
					return lookupCollection;
				}
			}

			var parentBindingManager = source.BindingContext[dataSource, parsedBindToList.BindingPath];

			var listProperty = parentBindingManager.GetItemProperties()[parsedBindToList.BindingField];
			if (listProperty == null)
			{
				if (listMember == "." && dataSource is IList)
				{
					result = (IList)dataSource;
					AddAdditionalInformationIfNeeded(result, parsedBindToList.BindingField);
				}
				else if (preventErrorReportForMissingListAttribute <= 0 && source.Parent != null)
				{
					var message =
						string.IsNullOrEmpty(listMember) ?
						"{0}: You must specify [List] on property '{4}'." : "{0}: Could not find";
					message += " listMember='{1}' dataSource.GetType()='{2}' dataMember='{3}'.";

					ErrorReporter.ReportOnce(
						"ListPropertyDescriptorNotFoundForZListUserControlSubclass_" + source.Name + "_" + source.GetBindingMember() + "_" + listMember,
						string.Format(CultureInfo.InvariantCulture,
							message,
							ControlDescription.GetControlPath(source),
							listMember,
							dataSource.GetType().FullName,
							dataMemberForErrorReporting,
							new KBindingMemberInfo(dataMemberForErrorReporting).BindingField));
				}
			}
			else
			{
				if (!(parentBindingManager is CurrencyManager && (parentBindingManager.Position == -1 || ((CurrencyManager)parentBindingManager).List.Count == 0)) &&
					!(parentBindingManager is PropertyManager && parentBindingManager.GetCurrent() == null))
				{
					var businessObject = parentBindingManager.GetCurrent() as BusinessObject;
					if (businessObject == null || !businessObject.IsDeleted)
					{
						result = (IList)listProperty.GetValue(parentBindingManager.GetCurrent());
						AddAdditionalInformationIfNeeded(result, parsedBindToList.BindingField);
					}
				}

				SetListPropertyDescriptorForZListUserControl(source, listProperty);
			}
			return result;
		}

		static void AddAdditionalInformationIfNeeded(IList list, string listMember)
		{
			if (!string.IsNullOrEmpty(listMember) && list is IAdditionalInformationWithSetter additionalInformation)
			{
				var information = additionalInformation.AdditionalInformation ?? string.Empty;
				if (!information.Contains(listMember))
				{
					information = listMember + System.Environment.NewLine + information;
					additionalInformation.SetAdditionalInformation(information.Trim());
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		static IList ReturnLookupCollectionSpecifiedInListDecoration(Control source, BusinessObject dataSource, string listMember)
		{
			try
			{
				const string lookupsText = "Lookups";

				if (listMember.StartsWith(lookupsText, StringComparison.OrdinalIgnoreCase)
					&& listMember.Length > lookupsText.Length
					&& ".+".IndexOf(listMember[lookupsText.Length]) > -1)
				{
					var propertyInfo = PropertyCache.GetTopProperty(dataSource.GetType(), lookupsText);
					var lookup = propertyInfo.GetValue(dataSource, null);
					var lookupType = lookup.GetType();
					var listPropertyName = listMember.Substring(lookupsText.Length + 1);

					SetListPropertyDescriptorForZListUserControl(source, ZCustomTypeDescriptor.GetProperties(lookupType)[listPropertyName]);
					var lookupCollection = (IList)PropertyCache.GetTopProperty(lookupType, listPropertyName).GetValue(lookup, null);
					if (lookupCollection != null && source is ZListUserControl sourceListUserControl)
					{
						sourceListUserControl.UpdateLookups(lookupCollection);
					}
					return lookupCollection;
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			return null;
		}

		protected virtual void UpdateLookups(IList lookupCollection)
		{
		}

		static void SetListPropertyDescriptorForZListUserControl(Control source, PropertyDescriptor propertyDescriptor)
		{
			var listUserControl = source as ZListUserControl;
			if (listUserControl != null)
			{
				listUserControl.listPropertyDescriptor = propertyDescriptor;
			}
		}

		protected internal IDisposable PreventErrorReportForMissingListAttribute()
		{
			preventErrorReportForMissingListAttribute++;
			return new DisposableAction(() => preventErrorReportForMissingListAttribute = preventErrorReportForMissingListAttribute > 0 ? preventErrorReportForMissingListAttribute - 1 : 0);
		}
		internal int preventErrorReportForMissingListAttribute;

		protected internal virtual bool RequiresList
		{
			get { return true; }
		}

		#endregion

		#region Developer Warning Notification

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				if (value != null)
				{
					base.Site = value;

					var uiService = (IPropertyValueUIService)GetService(typeof(IPropertyValueUIService));
					if (uiService != null)
					{
						uiService.AddPropertyValueUIHandler(UIServicePropertyValueUIHandler);
					}
				}
				else
				{
					var uiService = (IPropertyValueUIService)GetService(typeof(IPropertyValueUIService));
					if (uiService != null)
					{
						uiService.RemovePropertyValueUIHandler(UIServicePropertyValueUIHandler);
					}

					base.Site = value;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Designer Developer Text, Dev info")]
		static void UIServicePropertyValueUIHandler(ITypeDescriptorContext context, PropertyDescriptor descriptor, ArrayList items)
		{
			if (descriptor.DisplayName.Equals("BindToList"))
			{
				var instance = (IBindToList)context.Instance;

				if (string.IsNullOrEmpty(instance.BindToList))
				{
					items.Clear();
				}
				else
				{
					var tooltip = "WARNING! Click to read important information about the BindToList property";
					var icon = Enterprise.ZArchitecture.GUI.Properties.Resources.Warning;

					if (!IsItemExists(items, tooltip))
					{
						items.Add(new PropertyValueUIItem(icon, Handler, tooltip));
					}
				}
			}

			void Handler(ITypeDescriptorContext descriptorContext, PropertyDescriptor propertyDescriptor, PropertyValueUIItem item) => Globals.Message.Show("[List] attribute usually used for automatic binding", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		static bool IsItemExists(IEnumerable items, string tooltip)
		{
			var exists = false;
			foreach (PropertyValueUIItem item in items)
			{
				if (item.ToolTip == tooltip)
				{
					exists = true;
					break;
				}
			}
			return exists;
		}

		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		#region IBindToList Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToList))]
		public string BindToList
		{
			get
			{
				var result = bindToList;
				if (!this.IsDesignMode())
				{
					var dataSource = DataSource ?? CurrentItem;
					var dataMember = DataSource != null ? DataMember : DataPropertyName;

					if (dataSource != null)
					{
						var property = BindingHelper.GetDescriptor(BindingContext, dataSource, dataMember, false);
						result = property == null ? "" : MetadataAccessor.GetListMember(bindToList, property, dataMember);
					}
				}
				return result;
			}
			set
			{
				if (value != ZString.Empty)
				{
					BindToListAndModuleIdWarner.ShowOnChangedBindToListWarning(this);
				}
				bindToList = value;
				UpdateListBinding(false);
			}
		}
		string bindToList = "";

		#endregion

		#region IDataBoundControl

		object IDataBoundControl.DataSource
		{
			get { return DataSource; }
		}

		string IDataBoundControl.DataMember
		{
			get { return DataMember; }
		}

		/// <summary>
		/// Get the top-level data source from which the control is bound.
		/// When bound to a DataGrid, this property is empty. See also CurrentItem/DataPropertyName.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override object DataSourceCore
		{
			get { return dataSource; }
		}
		object dataSource;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			dataSource = null;
		}

		/// <summary>
		/// Get the full binding path from which the control is bound.
		/// When bound to a DataGrid, this property is empty. See also CurrentItem/DataPropertyName.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override string DataMemberCore
		{
			get { return dataMember; }
		}
		string dataMember;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null && string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentNullException(nameof(dataMember));
			}

			this.dataSource = dataSource;
			this.dataMember = dataMember;
			DataBoundControl.SetDataBindingForMetadataProperties(this, dataSource, dataMember);

			UpdateListBinding(true);
		}

		protected internal bool IsBound
		{
			get { return DataSource != null; }
		}

		void UpdateListBinding(bool throwOnPropertyNotFound)
		{
			if (RequiresListDataBinding && !this.IsDesignMode())
			{
				DataBindings.RemoveBinding(nameof(List)); // Programmatic constant
				if (DataSource != null && RequiresListDataBinding)
				{
					var listMember = GetListMember(DataSource, DataMember, throwOnPropertyNotFound);
					if (!string.IsNullOrEmpty(listMember))
					{
						// this is required specifically for the ZGuidFindBox for when the code width changes
						DataBindings.Add(new KBinding(nameof(List), DataSource, listMember, true, DataSourceUpdateMode.Never));
					}
				}
			}
		}

		protected virtual bool RequiresListDataBinding
		{
			get { return false; }
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			return GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember);
		}

		protected virtual CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			if (!string.IsNullOrEmpty(BindToList))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(IList), BindToList));
			}
			return result;
		}

		#endregion

		#region IAdditionalInformation Members
		string IAdditionalInformation.AdditionalInformation
		{
			get
			{
				var result = (List as IAdditionalInformation)?.AdditionalInformation ?? string.Empty;
				if (BindToList is string bindToList && !result.Contains(bindToList))
				{
					result = bindToList + System.Environment.NewLine + result;
				}
				return result.Trim();
			}
		}
		#endregion;
	}
}
