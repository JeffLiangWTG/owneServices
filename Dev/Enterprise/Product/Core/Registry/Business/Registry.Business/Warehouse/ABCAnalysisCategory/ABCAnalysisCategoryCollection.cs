using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ABCAnalysisCategoryCollection : RegistryBusinessObjectCollectionTemplate, ICodeDescriptionPairList
	{
		#region Add

		public new ABCAnalysisCategory AddNew()
		{
			return (ABCAnalysisCategory)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var abcAnalysisCategory = bizOAdded as ABCAnalysisCategory;
			if (abcAnalysisCategory != null)
			{
				abcAnalysisCategory.Operator = "<";

				if (Count > 1)
				{
					this[Count - 2].Operator = ">=";
				}
			}
		}

		#endregion

		#region Remove

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var abcAnalysisCategory = bizO as ABCAnalysisCategory;
			if (abcAnalysisCategory != null)
			{
				if (Count > 0)
				{
					this[Count - 1].Operator = "<";
				}
			}
		}

		#endregion

		#region Default

		public static ABCAnalysisCategoryCollection Default
		{
			get
			{
				var result = new ABCAnalysisCategoryCollection();

				SetupDefaultFields(result.AddNew(), "A", 80);
				SetupDefaultFields(result.AddNew(), "B", 15);
				SetupDefaultFields(result.AddNew(), "C", 5);
				SetupDefaultFields(result.AddNew(), "D", 5);

				return result;
			}
		}

		static void SetupDefaultFields(ABCAnalysisCategory abcAnalysisCategory, ZString categoryName, ZInt percentageOfTotal)
		{
			using (abcAnalysisCategory.GetValidationSuspender())
			{
				abcAnalysisCategory.CategoryName = categoryName;
				abcAnalysisCategory.PercentageOfTotal = percentageOfTotal;
			}
		}

		#endregion

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ABCAnalysisCategoryCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ABCAnalysisCategory();
		}

		public new ABCAnalysisCategory this[int i]
		{
			get { return (ABCAnalysisCategory)Elements[i]; }
		}

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return this.Any(category => ((ABCAnalysisCategory)category).CategoryName == code.ToString());
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			return FindBoxListProvider.DescriptionFromCode(code);
		}

		#endregion
	}
}
