using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PerformanceReportingMetricCategory : RegistryBusinessObjectTemplate, ICodeDescription
	{
		#region Constants

		public static string TypeBestInClass
		{
			get { return (NoResString)"Best In Class"; }
		}
		public static string TypeAdvantage
		{
			get { return (NoResString)"Advantage"; }
		}
		public static string TypeTypical
		{
			get { return (NoResString)"Typical"; }
		}
		public static string TypeDisadvantage
		{
			get { return (NoResString)"Disadvantage"; }
		}
		public static string TypeMajorOpportunity
		{
			get { return (NoResString)"Major Opportunity"; }
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string CategoryName = "CategoryName";
			public const string EnglishCategoryName = "EnglishCategoryName";
			public const string Operator = "Operator";
			public const string Value = "Value";
			public const string IsTarget = "IsTarget";
		}

		#endregion

		#region Parent

		[BusinessObjectTestExclude]
		public PerformanceReportingMetric Parent
		{
			get { return parent; }
			set { parent = value; }
		}
		PerformanceReportingMetric parent;

		#endregion

		//#region ParentCollection

		//PerformanceReportingMetricCategoryCollection ParentCollection
		//{
		//    get { return Parent != null ? Parent.MetricCategories : new PerformanceReportingMetricCategoryCollection(null); }
		//}

		//#endregion

		#region Properties

		#region CategoryName

		[MaxLength(MaxCategoryNameLength)]
		public MultilingualString CategoryName
		{
			get { return categoryName ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(CategoryNameInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(CategoryNameInfo, ref categoryName, value, false);
				EnglishCategoryNameInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateCategoryName();
				}
			}
		}

		public ZPropertyInfo CategoryNameInfo
		{
			get { return GetZPropertyInfo(Schema.CategoryName); }
		}

		//

		[MaxLength(MaxCategoryNameLength)]
		public ZString EnglishCategoryName
		{
			get { return CategoryName.GetUnresolvedString(); }
			set { CategoryName = (NoResString)value; }
		}

		public ZPropertyInfo EnglishCategoryNameInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishCategoryName); }
		}

		internal const int MaxCategoryNameLength = 100;
		MultilingualString categoryName;

		#endregion

		#region Operator

		[ReadOnly(true)]
		[MaxLength(2)]
		public ZString Operator
		{
			get { return _operator; }
			set { SetNonPersistentPropertyValue<ZString>(OperatorInfo, ref _operator, value); }
		}

		public ZPropertyInfo OperatorInfo
		{
			get { return GetZPropertyInfo(Schema.Operator); }
		}

		ZString _operator;

		#endregion

		#region Value

		public ZDecimal Value
		{
			get { return value; }
			set
			{
				SetNonPersistentPropertyValue<ZDecimal>(ValueInfo, ref this.value, value);
				if (!IsValidationSuspended)
				{
					ValidateValue();
				}
			}
		}

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(Schema.Value); }
		}

		ZDecimal value;

		#endregion

		#region IsTarget

		public ZBool IsTarget
		{
			get { return isTarget; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(IsTargetInfo, ref isTarget, value);
				if (!IsValidationSuspended)
				{
					ValidateIsTarget();
				}
			}
		}

		public ZPropertyInfo IsTargetInfo
		{
			get { return GetZPropertyInfo(Schema.IsTarget); }
		}

		ZBool isTarget;

		#endregion

		#endregion

		#region Validation

		#region ValidateCategoryName

		public void ValidateCategoryName()
		{
			CategoryNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CategoryNameInfo);

			var parent = Parent;
			if (parent != null)
			{
				foreach (PerformanceReportingMetricCategory category in parent.MetricCategories)
				{
					if (category != this && category.CategoryName.Equals(CategoryName))
					{
						CategoryNameInfo.AddError(ResString.GetMultilingualString("DDF01ED4-5E4C-4F7B-B671-93240781515A", "Category Name must be unique."));
						break;
					}
				}
			}
		}

		#endregion

		#region ValidateValue

		public void ValidateValue()
		{
			ValueInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(ValueInfo);

			if (!ValueInfo.HasErrors())
			{
				var parent = Parent;
				if (parent != null)
				{
					var categories = parent.MetricCategories;
					for (var index = 1; index < categories.Count; index++)
					{
						var currentCategory = categories[index];
						currentCategory.ValueInfo.ClearAllNotifications();
						if (index < categories.Count - 1)
						{
							if (currentCategory.Value >= categories[index - 1].Value && currentCategory.Value != 0)
							{
								currentCategory.ValueInfo.AddError(ResString.GetMultilingualString("6BBD4333-8115-4CB8-A32B-CF90ED03CF69", "The value must be in descending order."));
							}
						}
						else if (currentCategory.Value != categories[index - 1].Value)
						{
							currentCategory.ValueInfo.AddError(ResString.GetMultilingualString("97BA1E58-5E54-4E5C-ADA5-FACE3E01E7A5", "The lowest category must have its value equal to the value immediately above it."));
						}
					}
				}
			}
		}

		#endregion

		#region ValidateIsTarget

		public void ValidateIsTarget()
		{
			IsTargetInfo.ClearAllNotifications();

			if (!IsTargetInfo.HasErrors())
			{
				var parent = Parent;
				if (parent != null)
				{
					var categories = parent.MetricCategories;
					var targets = categories.Cast<PerformanceReportingMetricCategory>().Where(c => c.IsTarget);
					var targetsCount = targets.Count();
					if (targetsCount > 1)
					{
						foreach (var target in targets)
						{
							target.IsTargetInfo.AddError(ResString.GetMultilingualString("EA5F7C2E-01B9-4852-8F00-A06ED16E1AE4", "Only one category can be selected as target."));
						}
					}
					else if (targetsCount == 1)
					{
						targets.ElementAt(0).IsTargetInfo.ClearAllNotifications();
					}
				}
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCategoryName();
			ValidateValue();
			ValidateIsTarget();
		}

		#endregion

		#region Clone

		protected virtual PerformanceReportingMetricCategory CreatePerformanceReportingCategory()
		{
			return new PerformanceReportingMetricCategory();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = CreatePerformanceReportingCategory();

			result.CategoryName = CategoryName;
			result.Operator = Operator;
			result.Value = Value;
			result.IsTarget = IsTarget;

			return result;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CategoryName, EnglishCategoryName);
			writer.WriteElementString(Schema.Operator, Operator);
			writer.WriteElementString(Schema.Value, Value.ToString());
			writer.WriteElementString(Schema.IsTarget, IsTarget.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnglishCategoryName = reader.ReadElementString(Schema.CategoryName);
			Operator = reader.ReadElementString(Schema.Operator);
			Value = reader.ReadElementStringAsZDecimal(Schema.Value);
			IsTarget =	reader.ReadElementStringAsZBool(Schema.IsTarget);
		}

		#endregion

		#region ICodeDescription

		string ICodeDescription.Code
		{
			get { return CategoryName; }
		}

		string ICodeDescription.Description
		{
			get { return string.Format("{0} {1}", Operator, Value); }
		}

		#endregion
	}
}
