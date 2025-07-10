using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	public class ZGuidFindBox : ZCodeFindBox
	{
		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZGuidFindBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Binding Mappings

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength", Enabled = false)] // disable MaxLength binding
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		#endregion

		#region Primary Key from Code Check
		[DefaultValue(false)]
		public bool IsPrimaryKeyFromCodeRequired { get; set; }
		#endregion

		#region MaxLength

		const int DefaultMaxLength = 130;

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			InvalidateList();
			if (!this.IsDesignMode())
			{
				UpdateMaxLength();
			}
		}

		public override System.Collections.IList List
		{
			get { return base.List; }
			set
			{
				base.List = value;
				if (Visible)
				{
					UpdateMaxLength();
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (!this.IsDesignMode())
			{
				UpdateMaxLength();
			}
		}

		void UpdateMaxLength()
		{
			var typeOfListElements = this.TypeOfListElements;
			if (typeOfListElements != lastTypeOfListElements)
			{
				SetMaxLength();
			}
			lastTypeOfListElements = typeOfListElements;
		}
		Type lastTypeOfListElements;

		Type TypeOfListElements
		{
			get { return ListProvider != null && ListProvider.List != null ? ListProvider.List.TypeOfElements : null; }
		}

		protected virtual void SetMaxLength()
		{
			var result = GetMaxLengthFromListProvider(ListProvider);
			CodeBox.MaxLength = (result != -1) ? result : DefaultMaxLength;
		}

		#endregion

		#region GetMaxLengthFromListProvider

		internal static int GetMaxLengthFromListProvider(IFindBoxListProvider listProvider)
		{
			var result = -1;
			if (listProvider != null && listProvider.List != null)
			{
				if (typeof(ICompositeCollection).IsAssignableFrom(listProvider.List.GetType()))
				{
					result = ((ICompositeCollection)(listProvider.List)).MaxLength;
				}
				else
				{
					var elementType = listProvider.List.TypeOfElements;

					if (typeof(NonPersistentBusinessObject).IsAssignableFrom(elementType))
					{
						return result;
					}

					string propertyName = null;
					SchemaStringColumn schemaColumn = null;

					try
					{
						var tableName = BusinessObjectFactory.GetTableNameFromType(elementType);
						propertyName = CodePropertyAttribute.CodePropertyNameFromType(elementType);

						schemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(propertyName, tableName) as SchemaStringColumn;
					}
					catch (NoCodePropertyException)
					{
						// This error should be a developer error, as we can use a default maxlength
					}

					if (propertyName != null)
					{
						if (schemaColumn != null)
						{
							result = schemaColumn.MaxLength;
						}
						else
						{
							BusinessObject bizObj = null;
							var isActiveBusinessObjectCollection = typeof(IActiveBusinessObjectCollection).IsAssignableFrom(listProvider.List.GetType());
							if (!isActiveBusinessObjectCollection && listProvider.List.Count > 0) // .Count for ActiveBusinessObjectCollection loads whole collection
							{
								bizObj = (BusinessObject)((System.Collections.IList)listProvider.List)[0];
							}
							else
							{
								bizObj = listProvider.GetBusinessObjectFromCode(listProvider.NearestMatch("", true, 0).Item1);
							}

							if (bizObj != null)
							{
								result = CargoWise.ComponentModel.MetaData.GetMaxLength(bizObj, TypeDescriptor.GetProperties(bizObj)[propertyName]);
							}
						}
					}
					else
					{
						ErrorReporter.ReportOnce(elementType.FullName, "Cannot find code property for " + elementType.FullName + " for maxlength of ZGuidFindBox.");
					}
				}
			}

			return result;
		}

		#endregion

		#region Guid

		Tuple<ZGuid, string> guidCodePairCache;
		public ZGuid Guid
		{
			get
			{
				var currentCode = Code;
				if (string.IsNullOrEmpty(currentCode))
				{
					return ZGuid.Empty;
				}
				else if (guidCodePairCache == null || !guidCodePairCache.Item2.Equals(currentCode, StringComparison.OrdinalIgnoreCase))
				{
					var guid = ListProvider.PrimaryKeyFromCode(currentCode);
					guidCodePairCache = Tuple.Create(guid, currentCode);
					return guid;
				}
				else
				{
					return guidCodePairCache.Item1;
				}
			}
		}

		internal void SetGuid(ZGuid guid, string code)
		{
			guidCodePairCache = Tuple.Create(guid, code);
		}

		internal void ClearCodePairCache()
		{
			guidCodePairCache = null;
		}

		#endregion

		#region Parse / Format
#if DEBUG
		internal void OnParseValueExposed(ConvertEventArgs e)
		{
			OnParseValue(e);
		}
#endif
		protected override void OnParseValue(ConvertEventArgs e)
		{
			var value = Guid;

			if (!value.IsValid)
			{
				FieldInvalidTextMemory.SetInvalidText(CurrentItem, DataPropertyName, Code);
			}

			e.Value = value;
		}
#if DEBUG
		internal void OnFormatValueExposed(ConvertEventArgs e)
		{
			OnFormatValue(e);
		}
#endif
		protected override void OnFormatValue(ConvertEventArgs e)
		{
			if (e.Value != null)
			{
				if (!(e.Value is ZGuid))
				{
					throw new Exception("Incorrect Type bound to ZGuidFindBox (" + Name + "): <" + e.Value.GetType().FullName + ">");
				}

				var value = (ZGuid)e.Value;
				string code = null;
				if (value.IsValid || value.IsEmpty)
				{
					code = GetCode(value);
				}
				else
				{
					code = FieldInvalidTextMemory.GetInvalidText(CurrentItem, DataPropertyName);
					if (IFindBox.Description == Enterprise.Core.Constants.FindBoxMessages.InvalidSelection)
					{
						e.Value = code;
						return;
					}
				}

				SetGuid(value, code);
				e.Value = code;
				if (string.IsNullOrEmpty(BindToForDescription))
				{
					IFindBox.Description = DescriptionFromPrimaryKey(value, code);
				}
			}
		}

		ZGuid cachedPk;
		protected string DescriptionFromPrimaryKey(ZGuid pk, string code)
		{
			if (pk != cachedPk || code != cachedCode)
			{
				cachedPk = pk;
				cachedCode = code;
				cachedDescription = IFindBox.ListProvider.DescriptionFromPrimaryKey(pk);

				if (cachedDescription == null)
				{
					cachedDescription = pk.IsEmpty && CanReferenceByDescription ? string.Empty : InvalidDescription(pk);
				}
			}

			return cachedDescription;
		}
#if DEBUG
		internal string GetDescriptionExposed()
		{
			return GetDescription();
		}
#endif
		protected override string GetDescription()
		{
			return DescriptionFromPrimaryKey(Guid, Code);
		}

		protected override void UpdateDescriptionFromCode()
		{
			base.UpdateDescriptionFromCode();
			cachedPk = Guid;
		}

		protected override void UpdateCodeFromDescription()
		{
			base.UpdateCodeFromDescription();
			cachedPk = Guid;
		}

		protected string InvalidDescription(ZGuid pk)
		{
			return pk.IsEmpty ? Constants.FindBoxMessages.NoneSelected : Constants.FindBoxMessages.InvalidSelection;
		}

		#endregion

		#region Popup

		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			var popup = base.CreateEmbeddedPopup(module);
			popup.Selected += Popup_Selected;
			return popup;
		}

		void Popup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length == 1)
			{
				var bizObj = e.SelectedBusinessObjects[0];

				if (bizObj is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord && templateRecordProvider.TemplateRecord is BusinessObject templateBizo)
				{
					bizObj = templateBizo;
				}

				var newCode = ((ICodeDescription)bizObj).Code;
				var guid = bizObj.PK;

				if (IsPrimaryKeyFromCodeRequired)
				{
					guid = ListProvider.PrimaryKeyFromCode(newCode);
				}

				SetGuid(guid, newCode);

				if (string.Equals(IFindBox.Code, newCode, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(newCode))
				{
					IFindBox.Code = "";
					IFindBox.Code = newCode; //Needed to refresh binding.
				}

				OnSingleBusinessObjectSelectedInPopup(bizObj);
			}
		}

		protected virtual void OnSingleBusinessObjectSelectedInPopup(BusinessObject selectedObject)
		{
		}

		#endregion

		#region IDataBoundControl Members

		public override Type DataSourceType
		{
			get { return typeof(ZGuid); }
		}

		#endregion

		#region Implementation

		string GetCode(ZGuid pk)
		{
			return IFindBox.ListProvider.CodeFromPrimaryKey(pk);
		}

		protected override ZCodeFindBoxFetchHintHandler GetFetchHintHandler(IBindToList control, object dataSource)
		{
			return new ZGuidFindBoxFetchHintHandler(control, dataSource);
		}

		protected
#if DEBUG
		internal
#endif
		override IEnumerable<BusinessObject> GetBizObjsToEditOrView()
		{
			var bizObjs = base.GetBizObjsToEditOrView();

			if (Guid.IsValid)
			{
				var bizObjWithMatchingPk = bizObjs.FirstOrDefault(x => x.PK == Guid);
				if (bizObjWithMatchingPk != null)
				{
					return new BusinessObject[] { bizObjWithMatchingPk };
				}
			}
			return bizObjs;
		}

		#endregion
	}
}
