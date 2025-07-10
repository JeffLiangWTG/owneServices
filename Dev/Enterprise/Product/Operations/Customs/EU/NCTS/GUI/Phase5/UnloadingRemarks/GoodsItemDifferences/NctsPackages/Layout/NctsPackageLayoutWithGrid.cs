using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class NctsPackageLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public NctsPackageLayoutWithGrid()
		{
			Layout = CreateNctsPackageLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateNctsPackageLayout()
		{
			var builder = new NctsPackageLayoutBuilder<NctsPackage>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SequenceNumberCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.DeclaredValueLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.UnitTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.UnitCountCalcEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MarksAndNumbersTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.PlaceHolder1Label, ControlWidthClass.Auto);
			builder.Add(commonBag.UnloadedValueLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.DifUnitTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DifUnitCountCalcEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DifMarksAndNumbersTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(NctsPackagesGridUserControl);
	}
}
