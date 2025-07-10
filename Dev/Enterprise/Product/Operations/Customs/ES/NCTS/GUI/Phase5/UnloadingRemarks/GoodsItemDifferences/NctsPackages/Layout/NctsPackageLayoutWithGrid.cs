using System;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	sealed class NctsPackageLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public NctsPackageLayoutWithGrid()
		{
			Layout = CreateNctsPackageLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(EU.NCTS.GUI.NctsPackagesGridUserControl);

		PanelLayout CreateNctsPackageLayout()
		{
			var builder = new EU.NCTS.GUI.NctsPackageLayoutBuilder<NctsPackage>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SequenceNumberCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.DeclaredValueLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.UnitTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.UnitCountCalcEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PackageIDTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.BrandTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ModelTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.PlaceHolder1Label, ControlWidthClass.Auto);
			builder.Add(commonBag.UnloadedValueLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.DifUnitTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DifUnitCountCalcEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DifMarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DifPackageIDTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DifBrandTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DifModelTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
