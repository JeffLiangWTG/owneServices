using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class PermitTypeChecklistField : FilterFieldWithUTSupport, IJsonSerializable
	{
		#region Constructors

		public PermitTypeChecklistField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Properties

		public string SubTypeField
		{
			get { return subTypeField; }
			set { subTypeField = value; }
		}
		string subTypeField = "";

		public override bool IsEmpty
		{
			get { return !RootItemsCollection.HasIncludedItems; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.PermitTypeChecklistUserControl; }
		}

		#endregion

		#region Items

		void InitializeItems()
		{
			var countrySpecificInstruction = GetByCountryCode(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (countrySpecificInstruction != null)
			{
				var types = countrySpecificInstruction.GetTypeList();

				foreach (ICodeDescription type in types)
				{
					var typeItem = new PermitTypePartItem(FieldName, type.Code, type.Description);
					var subTypes = countrySpecificInstruction.GetSubTypeList(type.Code);

					foreach (ICodeDescription subType in subTypes)
					{
						var subTypeItem = new PermitTypePartItem(SubTypeField, subType.Code, subType.Description);

						typeItem.SubItemsCollection.Add(subTypeItem);
					}

					RootItemsCollection.Add(typeItem);
				}
			}
		}

		public static IPermitCountrySpecificInstruction GetByCountryCode(BusinessObjectFactory factory, string countryCode)
		{
			return factory.GetCachedValue<IPermitCountrySpecificInstruction>(string.Format(CultureInfo.InvariantCulture, "PermitCountrySpecificInstruction_{0}", countryCode), () =>
			{
				IPermitCountrySpecificInstruction result = null;
				var types = ObjectFactory.Get<Hashtable>("PermitCountrySpecificInstructions");
				if (!string.IsNullOrEmpty(countryCode))
				{
					var objectHandle = (ObjectHandle)types[countryCode];
					result = (IPermitCountrySpecificInstruction)objectHandle?.GetObject(factory);
				}

				if (result == null)
				{
					var objectHandle = (ObjectHandle)types["Shared"];
					result = (IPermitCountrySpecificInstruction)objectHandle?.GetObject(factory);
				}

				return result;
			});
		}

		public PermitTypePartItemCollection RootItemsCollection
		{
			get
			{
				if (rootItemsCollection == null)
				{
					rootItemsCollection = new PermitTypePartItemCollection();
					InitializeItems();
				}

				return rootItemsCollection;
			}
		}
		PermitTypePartItemCollection rootItemsCollection;

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
			get
			{
				var result = new ZStringBuilder();
				foreach (PermitTypePartItem item in RootItemsCollection)
				{
					if (item.Include)
					{
						var subTypeStr = new ZStringBuilder();
						if (item.SubItemsCollection.Count > 0)
						{
							foreach (PermitTypePartItem subItem in item.SubItemsCollection)
							{
								if (item.Include)
								{
									subTypeStr.Append(subItem.Code);
								}
							}
						}
						if (subTypeStr.IsEmpty)
						{
							result.Append(item.Code);
						}
						else
						{
							result.AppendFormat("{0}({1})", item.Code, subTypeStr.ToStringWithDelimiterBetweenAppends(","));
						}
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public override void ClearValues()
		{
			RootItemsCollection.RecursivelySetIncluded(false);
		}

		public override void SafeCopyValuesFrom(IFilter source)
		{
			var sourceChecklistField = source as PermitTypeChecklistField;
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

		internal PermitTypeChecklistField(PermitTypeChecklistFieldJsonData data)
			: base(data)
		{
			SubTypeField = data.SubTypeField;

			RootItemsCollection.CopyIncludeItems(new PermitTypePartItemCollection(data.RootItemsCollection));
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
			var filterData = new PermitTypeChecklistFilter();
			SetBaseFilterData(filterData);

			CopyPermitTypeChecklist(filterData.RootItems, RootItemsCollection);

			reportFilterData.PermitTypeChecklistFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.PermitTypeChecklistFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				CopyPermitTypeChecklist(RootItemsCollection, selectedValue.RootItems);
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new PermitTypeChecklistFieldJsonData()
			{
				SubTypeField = SubTypeField,
				RootItemsCollection = (PermitTypePartItemCollectionJsonData)RootItemsCollection?.GetJsonData()
			};
			SetJsonData(result);
			return result;
		}

		#endregion

		internal static void CopyPermitTypeChecklist(List<CodeDescriptionTree> filterRoot, PermitTypePartItemCollection fieldRoot)
		{
			foreach (PermitTypePartItem item in fieldRoot)
			{
				var codeDescription = new CodeDescriptionTree { Code = item.Code, Description = item.Description, Include = item.Include, Column = item.Column };
				filterRoot.Add(codeDescription);
				CopyPermitTypeChecklist(codeDescription.SubItems, item.SubItemsCollection);
			}
		}

		static void CopyPermitTypeChecklist(PermitTypePartItemCollection fieldRoot, List<CodeDescriptionTree> filterRoot)
		{
			fieldRoot.RemoveAll();
			foreach (var node in filterRoot)
			{
				var item = new PermitTypePartItem(node.Column, node.Code, node.Description) { Include = node.Include };
				if (item.Include)
				{
					fieldRoot.Add(item);
				}
				CopyPermitTypeChecklist(item.SubItemsCollection, node.SubItems);
			}
		}
	}
}
