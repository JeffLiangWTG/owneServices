using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ContentInformationType))]
	class ContentInformationTypeTest : Customs.Business.Testing.CusCodeDataTest<ContentInformationType>
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			NUnit.Framework.Assert.That(contentInformationType.CY_Type, Is.EqualTo(CusCodeDataTypeList.Codes.ContentInformationType).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDegreePercentage_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(contentInformationType.DegreePercentageInfo).Caption, Is.EqualTo("Degree Percentage"));
		}

		[ExpectNoExceptions]
		public void TestDegreePercentage()
		{
			contentInformationType.CY_Code = "01";
			NUnit.Framework.Assert.That(contentInformationType.DegreePercentage, Is.EqualTo(0m).Using(CustomComparers.TypeComparison));

			contentInformationType.CY_Data = "a";
			NUnit.Framework.Assert.That(contentInformationType.DegreePercentage, Is.EqualTo(0m).Using(CustomComparers.TypeComparison));

			contentInformationType.CY_Data = "0.01";
			NUnit.Framework.Assert.That(contentInformationType.DegreePercentage, Is.EqualTo(0.01m).Using(CustomComparers.TypeComparison));

			var oldValue = ZDecimal.Zero;
			var newValue = ZDecimal.Zero;
			contentInformationType.DegreePercentageInfo.ValueChanged += (sender, e) =>
			{
				var va = (ValueChangedEventArgs)e;
				oldValue = new ZDecimal(va.OldValue);
				newValue = new ZDecimal(va.NewValue);
			};
			contentInformationType.DegreePercentage = 0.45m;
			NUnit.Framework.Assert.That(contentInformationType.CY_Data, Is.EqualTo("0.45").Using(CustomComparers.TypeComparison), "code.CY_Data");
			NUnit.Framework.Assert.That(oldValue, Is.EqualTo(0.01m).Using(CustomComparers.TypeComparison), "oldValue");
			NUnit.Framework.Assert.That(newValue, Is.EqualTo(0.45m).Using(CustomComparers.TypeComparison), "newValue");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.CreateContentInformationType();
		}

		protected override void SetUp()
		{
			base.SetUp();
			contentInformationType = Factory.CreateContentInformationType();
		}
		ContentInformationType contentInformationType;

		#endregion

	}
}
