using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	public abstract class ImportExportMapping<TMapping, TWizard> : NonPersistentBusinessObject, IObsoleteValidation
		where TMapping : ImportExportMapping<TMapping, TWizard>
		where TWizard : ImportExportWizard
	{
		public ImportExportMapping(IImportPropertyInfo property, ImportExportMappingCollection<TMapping, TWizard> parentCollection)
		{
			Property = property;
			this.parentCollection = parentCollection;
		}

		#region Properties

		#region Text

		public ZString Text
		{
			get { return Property != null ? Property.HeaderText : String.Empty; }
		}

		public ZPropertyInfo TextInfo
		{
			get { return GetZPropertyInfo(nameof(Text)); }
		}

		#endregion

		#region MappingName

		public string MappingName
		{
			get
			{
				return
#if DEBUG
 mappingNameForTest ??
#endif
 (Property != null ? Property.MappingName : String.Empty);
			}
#if DEBUG
			internal set { mappingNameForTest = value; }
#endif
		}

#if DEBUG
		string mappingNameForTest;
#endif

		#endregion

		#region PropertyType

		public Type PropertyType
		{
			get { return Property != null ? Property.PropertyType : typeof(ZString); }
		}

		public bool IsType(BusinessObject bizObj, Type parentType)
		{
			return Property.IsType(bizObj, parentType);
		}

		#endregion

		#region MapAs

		[List("MapAsLookup")]
		[MaxLength(50)]
		public ZString MapAs
		{
			get
			{
				if (!MapAsSet)
				{
					if (MapAsLookup.Count > 0)
					{
						mapAs = MapAsLookup[0].Code;
					}

					MapAsSet = true;
				}

				return mapAs;
			}
			set
			{
				if (mapAs != value)
				{
					SetNonPersistentPropertyValue(MapAsInfo, ref mapAs, value);
					if (!IsValidationSuspended)
					{
						ValidateMapAs();
						OnMapAsChanged();
					}
				}
			}
		}

		ZString mapAs;

		public bool MapAsSet { get; private set; }

		protected virtual void OnMapAsChanged()
		{ }

		public ZPropertyInfo MapAsInfo
		{
			get { return GetZPropertyInfo(nameof(MapAs)); }
		}

		protected bool MapAs_ReadOnly
		{
			get { return MapAsLookup.Count == 0; }
		}

		void ValidateMapAs()
		{
			MapAsInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(MapAsInfo, MapAsLookup);
		}

		#endregion

		#region Expression

		[MaxLength(300)]
		public ZString Expression
		{
			get { return expression; }
			set
			{
				if (expression != value)
				{
					SetNonPersistentPropertyValue(ExpressionInfo, ref expression, value);
					OnExpressionChanged();
				}
			}
		}
		ZString expression;

		public ZPropertyInfo ExpressionInfo
		{
			get { return GetZPropertyInfo(nameof(Expression)); }
		}

		protected virtual bool Expression_ReadOnly
		{
			get { return false; }
		}

		protected virtual void OnExpressionChanged()
		{ }

		#endregion

		#region CustomMapList

		[MaxLength(50)]
		public ZString CustomMapList
		{
			get { return customMapList; }
			set
			{
				if (customMapList != value)
				{
					SetNonPersistentPropertyValue(CustomMapListInfo, ref customMapList, value);
					if (!IsValidationSuspended)
					{
						ValidateCustomMapList();
					}
				}
			}
		}

		ZString customMapList;

		public ZPropertyInfo CustomMapListInfo
		{
			get { return GetZPropertyInfo(nameof(CustomMapList)); }
		}

		void ValidateCustomMapList()
		{
			CustomMapListInfo.ClearAllNotifications();
			if (!CustomMapList.IsEmpty)
			{
				if (CustomMapLists.Find(CustomMapList) == null)
				{
					CustomMapListInfo.AddError(Res.GetString("7b4529a5-9349-44f6-823a-cd2cd06ec5f6", "Enter a valid list."));
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region MapAsLookup

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Lookup Mapping")]
		public virtual CodeDescriptionPairList MapAsLookup
		{
			get
			{
				if (mapAsLookup == null)
				{
					mapAsLookup = new CodeDescriptionPairList();
					IFindBoxListProviderEx findBoxListProviderEx = GetFindBoxListProvider() as IFindBoxListProviderEx;
					if (findBoxListProviderEx != null)
					{
						mapAsLookup.AddPair("Code");
						foreach (AlternateKey key in findBoxListProviderEx.AlternateKeys)
						{
							mapAsLookup.AddPair(key.ColumnDescription, key.ColumnName);
						}
					}
				}

				return mapAsLookup;
			}
		}

		CodeDescriptionPairList mapAsLookup;

		#endregion

		#region CustomMapLists

		public CustomMapPairListWrapperCollection CustomMapLists
		{
			get { return ParentCollection.Parent.CustomMapLists; }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMapAs();
			ValidateCustomMapList();
		}

		#endregion

		public bool TryFromCustomMapList(ref string value)
		{
			CustomMapPairListWrapper w = CustomMapLists.Find(CustomMapList);
			if (w != null)
			{
				string result = w.List.GetDescriptionFromCode(value);
				if (result != null)
				{
					value = result;
					return true;
				}
			}

			return false;
		}

		protected abstract IFindBoxListProvider GetFindBoxListProvider();

		public IImportPropertyInfo Property { get; private set; }

		protected ImportExportMappingCollection<TMapping, TWizard> ParentCollection
		{
			get { return parentCollection; }
		}
		readonly ImportExportMappingCollection<TMapping, TWizard> parentCollection;
	}
}
