using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class SalesTradeLaneChecklistField : FilterFieldWithUTSupport, IJsonSerializable
	{
		#region Constructors

		public SalesTradeLaneChecklistField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Properties

		public string ModeField
		{
			get { return modeField; }
			set { modeField = value; }
		}
		string modeField = "";

		public string TypeField
		{
			get { return typeField; }
			set { typeField = value; }
		}
		string typeField = "";

		public override bool IsEmpty
		{
			get { return !RootItemsCollection.HasIncludedItems; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.SalesTradeLaneChecklistUserControl; }
		}

		#endregion

		#region Items

		void InitializeItems()
		{
			var products = Factory.Load<IOrgSalesProduct>(new ZQuery());

			foreach (var product in products)
			{
				var productItem = new SalesTradeLanePartItem(FieldName, product.MP_Code, product.MP_Name);
				var modes =
					product.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse ?
						OrgSalesLookups.GetServiceTypes(product.MP_Code) :
						OrgTradeDetailLookups.GetTradeModes(product.MP_Code);

				foreach (ICodeDescription mode in modes)
				{
					var modeItem = new SalesTradeLanePartItem(ModeField, mode.Code, mode.Description);
					foreach (ICodeDescription type in OrgTradeDetailLookups.GetTradeTypes(product.MP_Code, mode.Code))
					{
						var typeItem = new SalesTradeLanePartItem(TypeField, type.Code, type.Description);
						modeItem.SubItemsCollection.Add(typeItem);
					}

					productItem.SubItemsCollection.Add(modeItem);
				}
				RootItemsCollection.Add(productItem);
			}

			var opportunityValueLookup = new OrgOpportunityValueLookups(null);
			foreach (ICodeDescription type in opportunityValueLookup.ActiveValueTypes)
			{
				var productItem = new SalesTradeLanePartItem(FieldName, type.Code, type.Description);
				RootItemsCollection.Add(productItem);
			}
		}

		public SalesTradeLanePartItemCollection RootItemsCollection
		{
			get
			{
				if (rootItemsCollection == null)
				{
					rootItemsCollection = new SalesTradeLanePartItemCollection();
					InitializeItems();
				}

				return rootItemsCollection;
			}
		}
		SalesTradeLanePartItemCollection rootItemsCollection;

		#endregion

		#region Where Clause

		protected override string NonEmptyWhereClause()
		{
			var newParameterList = new SqlParameterList();
			var result = RootItemsCollection.BuildFullWhereClause(newParameterList);
			ParameterList.Clear();
			ParameterList.AddRange(newParameterList);

			return result;
		}

		#endregion

		#region Value

		public override object ValueAsObject
		{
			get { return fName; }
		}

		public override void ClearValues()
		{
			RootItemsCollection.RecursivelySetIncluded(false);
		}

		public override void SafeCopyValuesFrom(IFilter source)
		{
			var sourceChecklistField = source as SalesTradeLaneChecklistField;
			if (sourceChecklistField != null)
			{
				RootItemsCollection.CopyIncludeItems(sourceChecklistField.RootItemsCollection);
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		#endregion

		#region Constructor For IJsonSerializable

		internal SalesTradeLaneChecklistField(SalesTradeLaneChecklistFieldJsonData data)
			: base(data)
		{
			ModeField = data.ModeField;
			TypeField = data.TypeField;

			RootItemsCollection.CopyIncludeItems(new SalesTradeLanePartItemCollection(data.RootItemsCollection));
		}

		#endregion

		#region Test
#if DEBUG

		public override void ClearValueForUnitTest()
		{
			ClearValues();
		}

#endif
		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new SalesTradeLaneChecklistFilter();
			SetBaseFilterData(filterData);

			CopySalesTradeLaneChecklist(filterData.RootItems, RootItemsCollection);

			reportFilterData.SalesTradeLaneChecklistFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.SalesTradeLaneChecklistFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				CopySalesTradeLaneChecklist(RootItemsCollection, selectedValue.RootItems);
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new SalesTradeLaneChecklistFieldJsonData()
			{
				ModeField = ModeField,
				TypeField = TypeField,
				RootItemsCollection = (SalesTradeLanePartItemCollectionJsonData)RootItemsCollection?.GetJsonData()
			};
			SetJsonData(result);
			return result;
		}

		#endregion

		internal static void CopySalesTradeLaneChecklist(List<CodeDescriptionTree> filterRoot, SalesTradeLanePartItemCollection fieldRoot)
		{
			foreach (SalesTradeLanePartItem item in fieldRoot)
			{
				var codeDescription = new CodeDescriptionTree { Code = item.Code, Description = item.Description, Include = item.Include, Column = item.Column };
				filterRoot.Add(codeDescription);
				CopySalesTradeLaneChecklist(codeDescription.SubItems, item.SubItemsCollection);
			}
		}

		static void CopySalesTradeLaneChecklist(SalesTradeLanePartItemCollection fieldRoot, List<CodeDescriptionTree> filterRoot)
		{
			fieldRoot.RemoveAll();
			foreach (var node in filterRoot)
			{
				var item = new SalesTradeLanePartItem(node.Column, node.Code, node.Description) { Include = node.Include };
				if (item.Include)
				{
					fieldRoot.Add(item);
				}
				CopySalesTradeLaneChecklist(item.SubItemsCollection, node.SubItems);
			}
		}
	}
}
