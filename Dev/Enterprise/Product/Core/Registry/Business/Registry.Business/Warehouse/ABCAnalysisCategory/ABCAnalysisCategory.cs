using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ABCAnalysisCategory : RegistryBusinessObjectTemplate, ICodeDescription
	{
		#region Schema

		public static class Schema
		{
			public const string CategoryName = "CategoryName";
			public const string Operator = "Operator";
			public const string PercentageOfTotal = "PercentageOfTotal";
		}

		#endregion

		#region Related Entities

		#region Parent

		[BusinessObjectTestExclude]
		public ABCAnalysisCategoryCollection Parent
		{
			get { return (ABCAnalysisCategoryCollection)GetParentCollection(this, typeof(ABCAnalysisCategoryCollection)); }
		}

		#endregion

		#endregion

		#region Properties

		#region CategoryName

		[MaxLength(3)]
		public ZString CategoryName
		{
			get { return categoryName; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(CategoryNameInfo, ref categoryName, value);
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

		ZString categoryName;

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

		#region PercentageOfTotal

		public ZInt PercentageOfTotal
		{
			get { return percentageOfTotal; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(PercentageOfTotalInfo, ref percentageOfTotal, value);
				if (!IsValidationSuspended)
				{
					ValidatePercentageOfTotal();
				}
			}
		}

		public ZPropertyInfo PercentageOfTotalInfo
		{
			get { return GetZPropertyInfo(Schema.PercentageOfTotal); }
		}

		ZInt percentageOfTotal;

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
				foreach (ABCAnalysisCategory abcAnalysisCategory in parent)
				{
					if (abcAnalysisCategory != this && abcAnalysisCategory.CategoryName == CategoryName)
					{
						CategoryNameInfo.AddError(Res.GetString("29259a95-098f-4468-a685-8fd8451e1ca7", "Category Name must be unique"));
						break;
					}
				}
			}
		}

		#endregion

		#region ValidatePercentageOfTotal

		public void ValidatePercentageOfTotal()
		{
			PercentageOfTotalInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PercentageOfTotalInfo);
			MandatoryValidation.CheckNotNegative(PercentageOfTotalInfo);

			var parent = Parent;
			if (PercentageOfTotal > 99)
			{
				PercentageOfTotalInfo.AddError(Res.GetString("f92e5043-4e37-4f65-9a08-b42aca5a801f", "Percentage of Total can only be up to two digits long."));
			}
			else if (!PercentageOfTotalInfo.HasErrors() && parent != null)
			{
				for (var index = 1; index < parent.Count; index++)
				{
					var currentCategory = parent[index];
					currentCategory.PercentageOfTotalInfo.ClearAllNotifications();
					if (index < parent.Count - 1)
					{
						if (currentCategory.PercentageOfTotal >= parent[index - 1].PercentageOfTotal)
						{
							currentCategory.PercentageOfTotalInfo.AddError(Res.GetString("0aad0e27-9327-4fb0-a67f-3d7ad7249c3b", "The Percentage Totals must be in descending order."));
						}
					}
					else if (currentCategory.PercentageOfTotal != parent[index - 1].PercentageOfTotal)
					{
						currentCategory.PercentageOfTotalInfo.AddError(Res.GetString("68908f48-9156-40ae-9988-ce64125dd50c", "The lowest category must have its Percentage of Total equal to the value immediately above it."));
					}
				}
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCategoryName();
			ValidatePercentageOfTotal();
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new ABCAnalysisCategory();

			result.CategoryName = CategoryName;
			result.Operator = Operator;
			result.PercentageOfTotal = PercentageOfTotal;

			return result;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CategoryName, CategoryName);
			writer.WriteElementString(Schema.Operator, Operator);
			writer.WriteElementString(Schema.PercentageOfTotal, PercentageOfTotal.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CategoryName = reader.ReadElementString(Schema.CategoryName);
			Operator = reader.ReadElementString(Schema.Operator);
			PercentageOfTotal = reader.ReadElementStringAsZInt(Schema.PercentageOfTotal);
		}

		#endregion

		#region ICodeDescription

		// WebTracker needs these properties to be public.

		public string Code
		{
			get { return CategoryName; }
		}

		public string Description
		{
			get
			{
				// for Web, stating just "> 15%" makes no sense so we need to prepend the code.
				var description = string.Format("{0} {1}%", Operator, PercentageOfTotal);
				return Globals.IsWeb ? string.Format("{0} - {1}", CategoryName, description) : description;
			}
		}

		#endregion
	}
}
