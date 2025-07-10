using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ExportWizardMapping))]
	sealed class ExportWizardMappingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestToString()
		{
			DummyChildBusinessObject bizObj = Factory.New<DummyChildBusinessObject>();
			bizObj.Z0_Number = 10;
			bizObj.Z0_VarCharMax = "TTT";
			bizObj.Z0_Bool = ZBool.True;
			bizObj.Z0_Short = 1;
			bizObj.Z0_AnotherDecimal = 1.1;
			bizObj.Z0_Date = new ZDateTime(2007, 10, 10, 16, 45, 10);

			AssertEquals("10", Helper.Wizard.Mapping[0].ToString(bizObj));
			AssertEquals("TTT", Helper.Wizard.Mapping[1].ToString(bizObj));
			AssertEquals("Y", Helper.Wizard.Mapping[2].ToString(bizObj));
			AssertEquals("1", Helper.Wizard.Mapping[3].ToString(bizObj));
			AssertEquals("1.1", Helper.Wizard.Mapping[4].ToString(bizObj));

			Helper.Wizard.Mapping[5].MapAs = "Short Date";
			AssertEquals(bizObj.Z0_Date.ToString("d"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "Long Date";
			AssertEquals(bizObj.Z0_Date.ToString("D"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "Long Date and Short Time";
			AssertEquals(bizObj.Z0_Date.ToString("f"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "Long Date and Long Time";
			AssertEquals(bizObj.Z0_Date.ToString("F"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "Short Date and Short Time";
			AssertEquals(bizObj.Z0_Date.ToString("g"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "Short date and Long Time";
			AssertEquals(bizObj.Z0_Date.ToString("G"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "Short Time";
			AssertEquals(bizObj.Z0_Date.ToString("t"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "Long Time";
			AssertEquals(bizObj.Z0_Date.ToString("T"), Helper.Wizard.Mapping[5].ToString(bizObj));
			Helper.Wizard.Mapping[5].MapAs = "";
			AssertEquals(bizObj.Z0_Date.ToString(), Helper.Wizard.Mapping[5].ToString(bizObj));
		}

		public void TestToStringFixedString()
		{
			DummyChildBusinessObject bizObj = Factory.New<DummyChildBusinessObject>();
			Helper.Wizard.FixedWidth = true;
			Helper.Wizard.Mapping[1].Width = 10;
			bizObj.Z0_VarCharMax = "12345";
			AssertEquals("12345     ", Helper.Wizard.Mapping[1].ToString(bizObj));
			Helper.Wizard.Mapping[1].Alignment = ExportWizardMapping.FieldAlignment.Right;
			AssertEquals("     12345", Helper.Wizard.Mapping[1].ToString(bizObj));
			bizObj.Z0_VarCharMax = "1234567890";
			AssertEquals("1234567890", Helper.Wizard.Mapping[1].ToString(bizObj));
			bizObj.Z0_VarCharMax = "123456789012";
			AssertEquals("1234567890", Helper.Wizard.Mapping[1].ToString(bizObj));
		}

		public void TestToStringWithExpression()
		{
			var mapping = new ExportWizardMapping(Helper.CollectionInfo.RowTypes.ElementAt(0), new ExportWizardMappingCollection(Helper.Wizard));

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 2;
			dummy.Z0_AnotherDecimal = 15;
			AssertEquals("", mapping.ToString(dummy));

			mapping.Expression = "Z0_Number * Z0_AnotherDecimal";
			AssertEquals("30", mapping.ToString(dummy));

			mapping.Expression = "Z0_Number123 * Z0_AnotherDecimal";
			AssertEquals("ERROR: global name 'Z0_Number123' is not defined", mapping.ToString(dummy));
		}

		public void TestAlignment()
		{
			AssertEquals(ExportWizardMapping.FieldAlignment.Right, Helper.Wizard.Mapping[0].Alignment);
			AssertEquals(ExportWizardMapping.FieldAlignment.Left, Helper.Wizard.Mapping[1].Alignment);
			AssertEquals("Left", Helper.Wizard.Mapping[1].AlignmentString);
			Helper.Wizard.Mapping[1].Alignment = ExportWizardMapping.FieldAlignment.Right;
			AssertEquals(ExportWizardMapping.FieldAlignment.Right, Helper.Wizard.Mapping[1].Alignment);
			AssertEquals("Right", Helper.Wizard.Mapping[1].AlignmentString);

			Helper.Wizard.Mapping[1].AlignmentString = "AAA";
			AssertNoErrors(Helper.Wizard.Mapping[1].AlignmentStringInfo);

			Helper.Wizard.FixedWidth = true;
			Helper.Wizard.Mapping[1].RunPreSaveValidation();
			AssertHasError(Helper.Wizard.Mapping[1].AlignmentStringInfo, "Enter a valid selection.");

			Helper.Wizard.Mapping[1].AlignmentString = "";
			AssertHasError(Helper.Wizard.Mapping[1].AlignmentStringInfo, "Please enter a value.");

			Helper.Wizard.Mapping[1].AlignmentString = "Left";
			AssertNoErrors(Helper.Wizard.Mapping[1].AlignmentStringInfo);
			AssertEquals(ExportWizardMapping.FieldAlignment.Left, Helper.Wizard.Mapping[1].Alignment);

			Helper.Wizard.Mapping[1].AlignmentString = "RIGHT";
			AssertNoErrors(Helper.Wizard.Mapping[1].AlignmentStringInfo);
			AssertEquals(ExportWizardMapping.FieldAlignment.Right, Helper.Wizard.Mapping[1].Alignment);
		}

		public void TestWidth()
		{
			Helper.Wizard.Mapping[0].Width = 0;
			AssertNoErrors(Helper.Wizard.Mapping[0].WidthInfo);

			Helper.Wizard.FixedWidth = true;
			Helper.Wizard.Mapping[0].RunPreSaveValidation();
			AssertHasError(Helper.Wizard.Mapping[0].WidthInfo, "Please enter a value.");

			Helper.Wizard.Mapping[0].Width = 10;
			AssertNoErrors(Helper.Wizard.Mapping[0].WidthInfo);
		}

		public void TestWidthAlignmentReadOnly()
		{
			Assert(Helper.Wizard.Mapping[0].WidthInfo.ReadOnly);
			Assert(Helper.Wizard.Mapping[0].AlignmentStringInfo.ReadOnly);

			Helper.Wizard.FixedWidth = true;

			Assert(!Helper.Wizard.Mapping[0].WidthInfo.ReadOnly);
			Assert(!Helper.Wizard.Mapping[0].AlignmentStringInfo.ReadOnly);
		}

		public void TestWithExpression()
		{
			var mapping = new ExportWizardMapping(Helper.CollectionInfo.RowTypes.ElementAt(0), new ExportWizardMappingCollection(Helper.Wizard));
			AssertEquals("", mapping.Text);
			AssertEquals("", mapping.MappingName);
			AssertEquals(typeof(ZString), mapping.PropertyType);
			AssertEquals("", mapping.MapAs);
			AssertEquals(1, mapping.Order);

			AssertEquals("", mapping.Header);
			mapping.Header = "JKL";
			AssertEquals("JKL", mapping.Header);

			AssertEquals(0, mapping.Width);
			mapping.Width = 20;
			AssertEquals(20, mapping.Width);

			AssertEquals(ExportWizardMapping.FieldAlignment.Left, mapping.Alignment);
			mapping.AlignmentString = "Right";
			AssertEquals(ExportWizardMapping.FieldAlignment.Right, mapping.Alignment);

			AssertEquals("", mapping.Expression);
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 2;
			dummy.Z0_AnotherDecimal = 15;
			mapping.Expression = "Z0_Number * Z0_AnotherDecimal";
			AssertEquals("30", mapping.ExprDelegate(dummy));
			dummy.Z0_VarCharMax = "Description";
			mapping.Expression = "str(Z0_Number) + ' / ' + left(Z0_VarCharMax, 4)";
			AssertEquals("2 / Desc", mapping.ExprDelegate(dummy));
			dummy.Z0_VarCharMax = "Txt";
			AssertEquals("2 / Txt", mapping.ExprDelegate(dummy));

			using (Culture.SetTemporarily(Culture.Default))
			{
				mapping.Expression = "Z0_Date.ToString('d')";
				AssertEquals("1/01/0001", mapping.ExprDelegate(dummy));
				dummy.Z0_Date = new ZDateTime(2009, 12, 11);
				AssertEquals("11/12/2009", mapping.ExprDelegate(dummy));
			}
		}

		public void TestWithZGuidExpression()
		{
			var collection = new ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject>(Factory);
			var parent = collection.AddNew();
			parent.Z0_Code = "COD";
			parent.Z0_Description = "Description";
			var child1 = parent.Dependents.AddNew();
			var info = new ExportCollectionInfoImpl(Factory, new BusinessObject[] { child1 })
			{
				{
					(NoResString)"DummyDependantBusinessObject", typeof(DummyDependantBusinessObject), new ImportPropertyInfoCollection()
					{
						new ImportPropertyInfoImpl<DummyDependantBusinessObject>(DummyDependentBizoSchema.Constants.ZD1_Code),
						new ImportPropertyInfoImpl<DummyDependantBusinessObject>(DummyDependentBizoSchema.Constants.ZD1_Z0, bizObj => collection),
					}
				}
			};
			ExportWizard wizard = new ExportWizard(info, null, new FileMapperForTest());
			var mapping1 = new ExportWizardMapping(info.RowTypes.ElementAt(0).Properties.ElementAt(1), wizard.Mapping);
			AssertEquals(2, mapping1.MapAsLookup.Count);

			var mapping2 = new ExportWizardMapping(info.RowTypes.ElementAt(0), wizard.Mapping);
			mapping2.Expression = "ZD1_Z0[0]";
			AssertEquals("COD", mapping2.ExprDelegate(child1));
			mapping2.Expression = "ZD1_Z0[1]";
			AssertEquals("Description", mapping2.ExprDelegate(child1));
			mapping2.Expression = "ZD1_Z0";
			AssertEquals("('COD', 'Description')", mapping2.ExprDelegate(child1));
		}

		public void TestExpression_ReadOnly()
		{
			Assert(Helper.Wizard.Mapping[0].ExpressionInfo.ReadOnly);
			Assert(!new ExportWizardMapping(Helper.CollectionInfo.RowTypes.ElementAt(0), Helper.Wizard.Mapping).ExpressionInfo.ReadOnly);
		}

		public void TestCondition()
		{
			var mapping = new ExportWizardMapping(Helper.CollectionInfo.RowTypes.ElementAt(0), new ExportWizardMappingCollection(Helper.Wizard));
			AssertEquals("", mapping.ConditionExpr);

			var dummy = Factory.New<DummyBusinessObject>();
			mapping.ConditionExpr = "Z0_AnotherDecimal > 10";
			Assert(!mapping.ConditionDelegate(dummy));
			dummy.Z0_AnotherDecimal = 15;
			Assert(mapping.ConditionDelegate(dummy));

			mapping.ConditionExpr = "Z0_AnotherDecimal > 10 and Z0_VarCharMax.Length > 0";
			Assert(!mapping.ConditionDelegate(dummy));
			dummy.Z0_VarCharMax = "Description";
			Assert(mapping.ConditionDelegate(dummy));

			mapping.ConditionExpr = "Z0_Bool";
			Assert(!mapping.ConditionDelegate(dummy));
			dummy.Z0_Bool = true;
			Assert(mapping.ConditionDelegate(dummy));

			mapping.ConditionExpr = "not Z0_Bool";
			Assert(!mapping.ConditionDelegate(dummy));
			dummy.Z0_Bool = false;
			Assert(mapping.ConditionDelegate(dummy));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			foreach (IImportPropertyInfo property in Helper.CollectionInfo.RowTypes.ElementAt(0).Properties)
			{
				if (property.MappingName == DummyBizoSchema.Constants.Z0_VarCharMax)
				{
					return new ExportWizardMapping(property, new ExportWizardMappingCollection(Helper.Wizard));
				}
			}

			return null;
		}

		ExportWizardTest.ExportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ExportWizardTest.ExportWizardTestHelper(Factory);
				}

				return helper;
			}
		}

		ExportWizardTest.ExportWizardTestHelper helper;

		#endregion
	}
}
