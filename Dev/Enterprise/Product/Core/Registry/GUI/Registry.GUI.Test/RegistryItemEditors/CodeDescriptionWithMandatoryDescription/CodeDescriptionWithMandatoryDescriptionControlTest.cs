using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionWithMandatoryDescriptionControl))]
	sealed class CodeDescriptionWithMandatoryDescriptionControlTest : RegistryZUserControlTestCase
	{
		[RequiresSTA]
		public void TestDescriptionColumn()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var key = "91A087B0-08AD-4951-9A2C-EF2C0B791906";
				mockChs.Put(key, new ResourceStringData(key, "描述"));
				var codeDescriptionWithMandatoryDescriptionCollection = new CodeDescriptionWithMandatoryDescriptionCollection();
				var codeDescriptionWithMandatoryDescription = codeDescriptionWithMandatoryDescriptionCollection.AddNew();
				codeDescriptionWithMandatoryDescription.Description = ResString.GetMultilingualString(key, "description");
				codeDescriptionWithMandatoryDescription.EnglishDescription = "description";

				var control = new CodeDescriptionWithMandatoryDescriptionControl();
				AssertDescriptionColumn(control, codeDescriptionWithMandatoryDescriptionCollection, typeof(ZString), "description");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule", Justification = "Testing")]
		void AssertDescriptionColumn(CodeDescriptionWithMandatoryDescriptionControl control, object dataSource, Type columnDataType, string columnValue)
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(dataSource, null);
				control.CodeDescriptionWithMandatoryDescriptionGrid.Select(0);
				var selectedElement = ((BusinessObject)control.CodeDescriptionWithMandatoryDescriptionGrid.ListManager.Current)[control.zTextBoxColumnStyleInfo2.ColumnName];
				AssertEquals(columnDataType, selectedElement.GetType());

				AssertEquals(columnValue,
					columnDataType == typeof(ResourceString)
						? ((MultilingualString)selectedElement).ToString(Core.SharedConstants.Languages.ChineseSimplified)
						: selectedElement.ToString());
			}
		}

		public void TestGridCannotBeSorted()
		{
			using (var control = new CodeDescriptionWithMandatoryDescriptionControl())
			{
				AssertEquals("CodeDescriptionWithMandatoryDescriptionGrid.AllowSorting", false, control.CodeDescriptionWithMandatoryDescriptionGrid.AllowSorting);
			}
		}

		#region Implementation

		protected override RegistryZUserControl GetNewControl()
		{
			return new CodeDescriptionWithMandatoryDescriptionControl();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionWithMandatoryDescriptionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionWithMandatoryDescriptionControl)control).CodeDescriptionWithMandatoryDescriptionGrid.ReadOnly;
		}

		#endregion
	}
}
